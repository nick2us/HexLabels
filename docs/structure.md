# HexLabels — Structura Proiectului

> Ultima actualizare: 2026-05-23

---

## Privire de ansamblu

HexLabels este o aplicație full-stack compusă dintr-un **backend .NET 10 Web API** și un **frontend React + TypeScript** (bundled cu Vite). Cele două proiecte sunt independente și comunică prin HTTP/REST.

```
HexLabels/
├── .gitignore                  ← Reguli gitignore globale (root)
├── package.json                ← Scripturi npm root (dev/build/deploy)
├── .vscode/
│   ├── launch.json             ← Configurații de lansare VS Code
│   └── tasks.json              ← Task-uri build și publish
├── backend/                    ← .NET 10 Web API
├── frontend/                   ← React 19 + TypeScript 6 (Vite 8)
└── docs/                       ← Documentație proiect (acest folder)
```

---

## Backend — `backend/`

### Tehnologii

| Pachet | Versiune | Rol |
|--------|----------|-----|
| .NET SDK | 10.0.107 | Runtime și SDK |
| ASP.NET Core | 10.0 | Framework Web API |
| `Microsoft.AspNetCore.OpenApi` | 10.0.7 | Generare spec OpenAPI (pachet nativ) |
| `Scalar.AspNetCore` | 2.14.14 | UI interactiv pentru documentația API |

### Structura fișierelor

```
backend/
├── HexLabels.Api.csproj        ← Definiția proiectului + dependențe NuGet
├── Program.cs                  ← Entry point: DI, middleware, endpoints
├── appsettings.json            ← Configurație generală (producție)
├── appsettings.Development.json← Configurație suprascriere (Development) — exclus din git
├── HexLabels.Api.http          ← Fișier HTTP pentru testare rapidă endpoints (VS Code REST Client)
├── Properties/
│   └── launchSettings.json     ← Profile de rulare locală (http / https)
├── bin/                        ← Output build — exclus din git
└── obj/                        ← Cache restore/build — exclus din git
```

### URL-uri locale

| Profil | URL |
|--------|-----|
| HTTP | `http://localhost:5241` |
| HTTPS | `https://localhost:7190` |

### Endpoint-uri disponibile

| Metodă | Rută | Descriere |
|--------|------|-----------|
| `GET` | `/weatherforecast` | Exemplu endpoint generat de template |
| `GET` | `/openapi/v1.json` | Spec OpenAPI în format JSON |
| `GET` | `/scalar/v1` | UI Scalar — documentație interactivă API |

### Configurare `Program.cs`

```
builder.Services.AddOpenApi()    →  Înregistrează generatorul OpenAPI nativ
app.MapOpenApi()                 →  Expune /openapi/v1.json
app.MapScalarApiReference()      →  Expune /scalar/v1 (temă: DeepSpace, client: C# HttpClient)
```

Scalar și OpenAPI sunt disponibile **doar în mediul Development**.

---

## Frontend — `frontend/`

### Tehnologii

| Pachet | Versiune | Rol |
|--------|----------|-----|
| React | ^19.2.6 | UI library |
| ReactDOM | ^19.2.6 | Rendering în browser |
| TypeScript | ~6.0.2 | Type safety |
| Vite | ^8.0.12 | Build tool și dev server |
| ESLint | ^10.3.1 | Linting cod |

### Structura fișierelor

```
frontend/
├── index.html                  ← Punct de intrare HTML
├── package.json                ← Dependențe npm și scripturi
├── package-lock.json           ← Lock file npm
├── vite.config.ts              ← Configurație Vite
├── tsconfig.json               ← Config TypeScript root
├── tsconfig.app.json           ← Config TypeScript pentru sursele aplicației
├── tsconfig.node.json          ← Config TypeScript pentru Vite/Node
├── eslint.config.js            ← Reguli ESLint
├── public/
│   ├── favicon.svg             ← Favicon aplicație
│   └── icons.svg               ← Iconuri statice
├── src/
│   ├── main.tsx                ← Entry point React (render root)
│   ├── App.tsx                 ← Componenta principală
│   ├── App.css                 ← Stiluri componentă App
│   ├── index.css               ← Stiluri globale
│   └── assets/                 ← Resurse statice importate în cod
│       ├── react.svg
│       ├── vite.svg
│       └── hero.png
└── node_modules/               ← Dependențe npm — exclus din git
```

### Scripturi npm

| Comandă | Acțiune |
|---------|---------|
| `npm run dev` | Pornește dev server Vite (hot reload) |
| `npm run build` | Compilează TypeScript + bundle producție în `dist/` |
| `npm run preview` | Previzualizare locală a bundle-ului de producție |
| `npm run lint` | Verificare cod cu ESLint |

### URL dev server

| URL | Descriere |
|-----|-----------|
| `http://localhost:5173` | Dev server Vite (port implicit) |

---

## VS Code — `.vscode/`

### `launch.json` — Configurații de lansare

| Nume | Tip | Acțiune |
|------|-----|---------|
| **Start Backend** | `coreclr` | Build + lansare .NET API în mod Debug |
| **Start Frontend** | `node` | Pornire `npm run dev`, deschide browser automat |
| **Deploy Both** | `node` | Execută task `deploy-all` (publish backend + build frontend) |
| **Start Backend + Frontend** | compound | Pornește simultan Backend și Frontend |

### `tasks.json` — Task-uri

| Label | Acțiune |
|-------|---------|
| `build-backend` | `dotnet build` în configurație Debug |
| `publish-backend` | `dotnet publish -c Release` → output în `backend/publish/` |
| `build-frontend` | `npm run build` → output în `frontend/dist/` |
| `deploy-all` | Rulează secvențial: `publish-backend` → `build-frontend` |

---

## `.gitignore`

Există trei fișiere `.gitignore`:

| Fișier | Acoperă |
|--------|---------|
| `.gitignore` (root) | Reguli globale pentru ambele proiecte |
| `backend/.gitignore` | `bin/`, `obj/`, `publish/`, `.vs/`, secrets |
| `frontend/.gitignore` | `node_modules/`, `dist/`, cache, fișiere `.local` |

---

## Scripturi root — `package.json`

```json
"dev:backend"   → cd backend && dotnet run
"dev:frontend"  → cd frontend && npm run dev
"build:backend" → dotnet publish -c Release
"build:frontend"→ npm run build
"deploy"        → build:backend + build:frontend (secvențial)
```

---

## Flux de lucru recomandat

```
1. Pornire dezvoltare
   VS Code → Run & Debug → "Start Backend + Frontend"
   sau terminal: npm run dev:backend  /  npm run dev:frontend

2. Testare API
   https://localhost:7190/scalar/v1

3. Build pentru producție
   VS Code → Run & Debug → "Deploy Both"
   sau terminal: npm run deploy
```
