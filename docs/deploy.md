# HexLabels — Deploy Contabo VPC

## Arhitectură

```
Internet
    │
    ▼
┌─────────────────────────────────────────┐
│  Nginx  (host, port 80/443)             │
│                                         │
│  /*     → /opt/hexlabels/frontend/      │  React static files
│  /api/* → localhost:8080                │
│  /scalar/*→ localhost:8080/scalar/      │
└──────────────────┬──────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────┐
│  MiniStack — Docker nginx LB (:8080)    │  least_conn load balancing
│                                         │
│  upstream hexlabels_backend {           │
│    server 127.0.0.1:5100;               │
│    server 127.0.0.1:5101;               │
│  }                                      │
└──────────┬──────────────────┬───────────┘
           │                  │
           ▼                  ▼
  ┌─────────────┐    ┌─────────────┐
  │ .NET API #1 │    │ .NET API #2 │   systemd hexlabels-api@5100
  │   :5100     │    │   :5101     │   systemd hexlabels-api@5101
  └─────────────┘    └─────────────┘
```

---

## Configurare inițială

### 1. Editează variabilele în `deploy.sh`

```bash
SERVER_IP="YOUR_CONTABO_IP"   # IP-ul VPC-ului Contabo
SSH_USER="root"                # user SSH (de obicei root la Contabo)
SSH_KEY="$HOME/.ssh/id_rsa"   # calea spre cheia privată SSH
DOMAIN="yourdomain.com"        # domeniu sau același IP
```

### 2. Prima rulare — setup server

```bash
./deploy.sh --setup
```

Instalează automat pe server:
- `nginx` — reverse proxy principal
- `.NET 10 ASP.NET Core Runtime`
- `Docker` + `Docker Compose`
- Firewall UFW (porturi 22, 80, 443)
- Systemd service template (`hexlabels-api@.service`)
- MiniStack Docker Compose cu nginx load balancer
- Configurație Nginx completă

---

## Deploy

### Deploy complet (backend + frontend)

```bash
./deploy.sh
```

### Deploy doar backend

```bash
./deploy.sh --backend
```

### Deploy doar frontend

```bash
./deploy.sh --frontend
```

---

## Ce face scriptul la fiecare deploy

```
1. Verifică prerequisite locale (dotnet, npm, rsync, ssh, cheia SSH)
2. Testează conexiunea SSH la server
3. Build backend   → dotnet publish -c Release → backend/publish/
4. Sync backend    → rsync backend/publish/ → server:/opt/hexlabels/backend/
5. Restart API     → systemctl restart hexlabels-api@5100 hexlabels-api@5101
6. Build frontend  → npm run build → frontend/dist/
7. Sync frontend   → rsync frontend/dist/ → server:/opt/hexlabels/frontend/
8. Reload Nginx    → nginx -t && systemctl reload nginx
9. Health check    → verifică MiniStack LB, Nginx, ambele instanțe API
```

---

## Structura pe server

```
/opt/hexlabels/
├── backend/           ← fișiere publicate .NET API
├── frontend/          ← fișiere statice React (dist/)
└── ministack/
    ├── docker-compose.yml    ← MiniStack LB
    └── nginx-lb.conf         ← config load balancer

/etc/systemd/system/
└── hexlabels-api@.service    ← template service (instanțe multiple)

/etc/nginx/sites-available/
└── hexlabels                 ← config Nginx principal

/var/log/hexlabels/
├── api-5100.log
├── api-5100-error.log
├── api-5101.log
└── api-5101-error.log
```

---

## Comenzi utile pe server

```bash
# Status servicii backend
systemctl status hexlabels-api@5100
systemctl status hexlabels-api@5101

# Loguri backend în timp real
journalctl -u hexlabels-api@5100 -f
journalctl -u hexlabels-api@5101 -f

# Status MiniStack load balancer
docker ps
docker logs hexlabels-lb -f

# Reload Nginx
nginx -t && systemctl reload nginx

# Restart complet
systemctl restart hexlabels-api@5100 hexlabels-api@5101
docker restart hexlabels-lb
```

---

## URL-uri după deploy

| Serviciu | URL |
|----------|-----|
| Frontend (React) | `http://DOMAIN/` |
| API | `http://DOMAIN/api/` |
| Scalar UI | `http://DOMAIN/scalar/v1` |
| OpenAPI JSON | `http://DOMAIN/openapi/v1.json` |
| LB Health | `http://DOMAIN:8080/health` |

---

## HTTPS cu Let's Encrypt (opțional)

```bash
# Pe server, după primul deploy
apt install certbot python3-certbot-nginx
certbot --nginx -d yourdomain.com
# Certbot modifică automat configurația Nginx
```
