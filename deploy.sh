#!/usr/bin/env bash
# =============================================================================
#  HexLabels — Deploy Script → Contabo VPC
#
#  Arhitectură:
#    Internet → Nginx (host :80/:443)
#                 ├── /*     → React static files
#                 └── /api/* → MiniStack LB (:8080) → .NET API x2 (:5100,:5101)
#
#  Utilizare:
#    chmod +x deploy.sh
#    ./deploy.sh               # deploy complet
#    ./deploy.sh --backend     # doar backend
#    ./deploy.sh --frontend    # doar frontend
#    ./deploy.sh --setup       # configurare inițială server (prima rulare)
# =============================================================================
set -euo pipefail

# ─── CONFIGURARE DIN VARIABILE DE MEDIU ───────────────────────────────────────
SERVER_IP="${DEV_SERVER:?Variabila DEV_SERVER nu este setată}"
SSH_USER="${DEV_SSH_USER:?Variabila DEV_SSH_USER nu este setată}"
SSH_KEY="${DEV_SSH_KEY:?Variabila DEV_SSH_KEY nu este setată}"
DOMAIN="${DEV_DOMAIN:?Variabila DEV_DOMAIN nu este setată}"
APP_DIR="/opt/hexlabels"
BACKEND_PORT_1=5100
BACKEND_PORT_2=5101
LB_PORT=8080
BACKEND_INSTANCES=2
# ──────────────────────────────────────────────────────────────────────────────

RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'
BLUE='\033[0;34m'; CYAN='\033[0;36m'; BOLD='\033[1m'; NC='\033[0m'

step()  { echo -e "\n${BLUE}${BOLD}▶ $1${NC}"; }
ok()    { echo -e "  ${GREEN}✓ $1${NC}"; }
warn()  { echo -e "  ${YELLOW}⚠ $1${NC}"; }
die()   { echo -e "\n${RED}✗ EROARE: $1${NC}\n"; exit 1; }
info()  { echo -e "  ${CYAN}→ $1${NC}"; }

SSH_CMD="ssh -i $SSH_KEY -o StrictHostKeyChecking=no -o ConnectTimeout=10 $SSH_USER@$SERVER_IP"
RSYNC_CMD="rsync -avz --delete -e \"ssh -i $SSH_KEY -o StrictHostKeyChecking=no\""

remote() { $SSH_CMD "$@"; }

# ─── ARGUMENTE ────────────────────────────────────────────────────────────────
DEPLOY_BACKEND=true
DEPLOY_FRONTEND=true
RUN_SETUP=false

for arg in "$@"; do
  case $arg in
    --backend)  DEPLOY_FRONTEND=false ;;
    --frontend) DEPLOY_BACKEND=false  ;;
    --setup)    RUN_SETUP=true; DEPLOY_BACKEND=false; DEPLOY_FRONTEND=false ;;
    --help|-h)
      echo -e "${BOLD}Utilizare:${NC} ./deploy.sh [--backend|--frontend|--setup]"
      exit 0 ;;
  esac
done

# ─── VALIDĂRI ─────────────────────────────────────────────────────────────────
check_prerequisites() {
  step "Verificare prerequisite locale"
  command -v dotnet &>/dev/null || die "dotnet nu este instalat local"
  command -v npm    &>/dev/null || die "npm nu este instalat local"
  command -v rsync  &>/dev/null || die "rsync nu este instalat local"
  command -v ssh    &>/dev/null || die "ssh nu este disponibil"
  [[ -f "$SSH_KEY" ]] || die "Cheia SSH nu există: $SSH_KEY"
  [[ "$SERVER_IP" == "YOUR_CONTABO_IP" ]] && die "Setează SERVER_IP în deploy.sh"
  ok "Toate prerequisitele sunt prezente"
}

check_connection() {
  step "Testare conexiune SSH → $SERVER_IP"
  $SSH_CMD "echo ok" &>/dev/null || die "Nu se poate conecta la $SERVER_IP cu cheia $SSH_KEY"
  ok "Conexiune SSH reușită"
}

# ─── SETUP INIȚIAL SERVER ─────────────────────────────────────────────────────
setup_server() {
  step "Setup inițial server Contabo"

  info "Instalare pachete sistem"
  remote "apt-get update -qq && apt-get install -y -qq nginx curl wget rsync ufw"

  info "Instalare .NET 10 runtime"
  remote "
    if ! command -v dotnet &>/dev/null; then
      wget -q https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb
      dpkg -i /tmp/packages-microsoft-prod.deb
      apt-get update -qq
      apt-get install -y -qq aspnetcore-runtime-10.0
    else
      echo '.NET deja instalat: '$(dotnet --version)
    fi
  "

  info "Instalare Docker"
  remote "
    if ! command -v docker &>/dev/null; then
      curl -fsSL https://get.docker.com | sh
      systemctl enable docker
      systemctl start docker
    else
      echo 'Docker deja instalat: '$(docker --version)
    fi
  "

  info "Creare structură directoare"
  remote "
    mkdir -p $APP_DIR/{backend,frontend,ministack}
    mkdir -p /var/log/hexlabels
  "

  info "Configurare firewall UFW"
  remote "
    ufw --force enable
    ufw allow 22/tcp
    ufw allow 80/tcp
    ufw allow 443/tcp
    ufw reload
  " || warn "UFW nu a putut fi configurat (poate fi deja activ)"

  setup_systemd_service
  setup_ministack
  setup_nginx

  ok "Setup server finalizat"
  warn "Rulează './deploy.sh' pentru a face deploy aplicațiilor"
}

# ─── SYSTEMD — Template service pentru backend ────────────────────────────────
setup_systemd_service() {
  info "Configurare systemd service template"

  local service_content="[Unit]
Description=HexLabels API instance %i
After=network.target

[Service]
Type=notify
User=www-data
WorkingDirectory=$APP_DIR/backend
ExecStart=/usr/bin/dotnet $APP_DIR/backend/HexLabels.Api.dll \
  --urls=http://localhost:%i
Restart=always
RestartSec=5
KillSignal=SIGINT
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
StandardOutput=append:/var/log/hexlabels/api-%i.log
StandardError=append:/var/log/hexlabels/api-%i-error.log
SyslogIdentifier=hexlabels-api-%i

[Install]
WantedBy=multi-user.target"

  remote "echo '$service_content' > /etc/systemd/system/hexlabels-api@.service"
  remote "systemctl daemon-reload"
  ok "Systemd service template creat"
}

# ─── MINISTACK — Nginx load balancer în Docker ────────────────────────────────
setup_ministack() {
  info "Configurare MiniStack (Docker nginx load balancer)"

  remote "cat > $APP_DIR/ministack/nginx-lb.conf << 'NGINX_LB'
upstream hexlabels_backend {
    least_conn;
    server 127.0.0.1:$BACKEND_PORT_1 weight=1 max_fails=3 fail_timeout=30s;
    server 127.0.0.1:$BACKEND_PORT_2 weight=1 max_fails=3 fail_timeout=30s;
    keepalive 32;
}

server {
    listen 80;

    location / {
        proxy_pass         http://hexlabels_backend;
        proxy_http_version 1.1;
        proxy_set_header   Connection        \"\";
        proxy_set_header   Host              \$host;
        proxy_set_header   X-Real-IP         \$remote_addr;
        proxy_set_header   X-Forwarded-For   \$proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto \$scheme;
        proxy_read_timeout 60s;
        proxy_connect_timeout 5s;
    }

    location /health {
        access_log off;
        return 200 'ok\n';
        add_header Content-Type text/plain;
    }
}
NGINX_LB"

  remote "cat > $APP_DIR/ministack/docker-compose.yml << 'COMPOSE'
services:
  lb:
    image: nginx:1.27-alpine
    container_name: hexlabels-lb
    restart: unless-stopped
    ports:
      - \"$LB_PORT:80\"
    volumes:
      - ./nginx-lb.conf:/etc/nginx/conf.d/default.conf:ro
    network_mode: host
    healthcheck:
      test: [\"CMD\", \"wget\", \"-qO-\", \"http://localhost/health\"]
      interval: 15s
      timeout: 5s
      retries: 3
COMPOSE"

  remote "cd $APP_DIR/ministack && docker compose up -d"
  ok "MiniStack pornit pe portul $LB_PORT"
}

# ─── NGINX HOST — Reverse proxy principal ─────────────────────────────────────
setup_nginx() {
  info "Configurare Nginx principal"

  remote "cat > /etc/nginx/sites-available/hexlabels << 'NGINX'
server {
    listen 80;
    server_name $DOMAIN;

    # Redirecționare www → non-www
    if (\$host = www.$DOMAIN) {
        return 301 http://$DOMAIN\$request_uri;
    }

    # ── Frontend (React static) ──────────────────────────────────────────────
    root $APP_DIR/frontend;
    index index.html;

    location / {
        try_files \$uri \$uri/ /index.html;
        expires 1h;
        add_header Cache-Control \"public, no-transform\";
    }

    # Fișiere statice cu cache lung
    location ~* \.(js|css|png|jpg|svg|ico|woff2?)$ {
        expires 30d;
        add_header Cache-Control \"public, immutable\";
        access_log off;
    }

    # ── Backend API → MiniStack Load Balancer ───────────────────────────────
    location /api/ {
        proxy_pass         http://localhost:$LB_PORT/;
        proxy_http_version 1.1;
        proxy_set_header   Host              \$host;
        proxy_set_header   X-Real-IP         \$remote_addr;
        proxy_set_header   X-Forwarded-For   \$proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto \$scheme;
        proxy_read_timeout 120s;
    }

    # ── Scalar UI ────────────────────────────────────────────────────────────
    location /scalar/ {
        proxy_pass http://localhost:$LB_PORT/scalar/;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
    }

    # Securitate headers
    add_header X-Frame-Options \"SAMEORIGIN\" always;
    add_header X-Content-Type-Options \"nosniff\" always;
    add_header Referrer-Policy \"strict-origin-when-cross-origin\" always;
}
NGINX"

  remote "
    ln -sf /etc/nginx/sites-available/hexlabels /etc/nginx/sites-enabled/hexlabels
    rm -f /etc/nginx/sites-enabled/default
    nginx -t && systemctl reload nginx
  "
  ok "Nginx configurat și reîncărcat"
}

# ─── BUILD LOCAL ──────────────────────────────────────────────────────────────
build_backend() {
  step "Build backend (.NET 10 → Release)"
  dotnet publish backend/HexLabels.Api.csproj \
    --configuration Release \
    --output backend/publish \
    --self-contained false \
    --runtime linux-x64 \
    -p:PublishSingleFile=false \
    --nologo -v quiet
  ok "Backend publicat în backend/publish/"
}

build_frontend() {
  step "Build frontend (React → Vite)"
  (cd frontend && npm run build --silent)
  ok "Frontend construit în frontend/dist/"
}

# ─── SYNC → SERVER ────────────────────────────────────────────────────────────
sync_backend() {
  step "Sincronizare backend → server"
  eval "$RSYNC_CMD backend/publish/ $SSH_USER@$SERVER_IP:$APP_DIR/backend/"
  ok "Fișiere backend sincronizate"
}

sync_frontend() {
  step "Sincronizare frontend → server"
  eval "$RSYNC_CMD frontend/dist/ $SSH_USER@$SERVER_IP:$APP_DIR/frontend/"
  ok "Fișiere frontend sincronizate"
}

# ─── RESTART SERVICII ─────────────────────────────────────────────────────────
restart_backend() {
  step "Restart instanțe backend (zero-downtime)"

  for port in $BACKEND_PORT_1 $BACKEND_PORT_2; do
    info "Restart hexlabels-api@$port"
    remote "
      systemctl enable hexlabels-api@$port 2>/dev/null || true
      systemctl restart hexlabels-api@$port
      sleep 2
      systemctl is-active --quiet hexlabels-api@$port \
        && echo '  instanță $port → OK' \
        || echo '  instanță $port → FAILED (verifică: journalctl -u hexlabels-api@$port -n 50)'
    "
  done
  ok "Backend restarts complete"
}

reload_nginx() {
  remote "nginx -t && systemctl reload nginx"
  ok "Nginx reîncărcat"
}

# ─── HEALTH CHECK ─────────────────────────────────────────────────────────────
health_check() {
  step "Health check"
  sleep 3

  local lb_health
  lb_health=$(remote "curl -sf http://localhost:$LB_PORT/health 2>/dev/null || echo 'FAIL'")
  if [[ "$lb_health" == *"ok"* ]]; then
    ok "MiniStack LB → sănătos"
  else
    warn "MiniStack LB → nu răspunde (verifică: docker logs hexlabels-lb)"
  fi

  local nginx_status
  nginx_status=$(remote "systemctl is-active nginx")
  [[ "$nginx_status" == "active" ]] && ok "Nginx → activ" || warn "Nginx → $nginx_status"

  for port in $BACKEND_PORT_1 $BACKEND_PORT_2; do
    local api_status
    api_status=$(remote "systemctl is-active hexlabels-api@$port 2>/dev/null || echo 'inactive'")
    [[ "$api_status" == "active" ]] \
      && ok "API instanță :$port → activă" \
      || warn "API instanță :$port → $api_status"
  done

  echo -e "\n  ${CYAN}URL aplicație:${NC} http://$DOMAIN"
  echo -e "  ${CYAN}API:${NC}          http://$DOMAIN/api/"
  echo -e "  ${CYAN}Scalar UI:${NC}    http://$DOMAIN/scalar/v1\n"
}

# ─── MAIN ─────────────────────────────────────────────────────────────────────
echo -e "\n${BOLD}${BLUE}═══════════════════════════════════════════════${NC}"
echo -e "${BOLD}${BLUE}   HexLabels Deploy → Contabo VPC ($SERVER_IP)${NC}"
echo -e "${BOLD}${BLUE}═══════════════════════════════════════════════${NC}"

check_prerequisites
check_connection

if $RUN_SETUP; then
  setup_server
  exit 0
fi

if $DEPLOY_BACKEND; then
  build_backend
  sync_backend
  restart_backend
fi

if $DEPLOY_FRONTEND; then
  build_frontend
  sync_frontend
  reload_nginx
fi

health_check

echo -e "${GREEN}${BOLD}Deploy finalizat cu succes!${NC}\n"
