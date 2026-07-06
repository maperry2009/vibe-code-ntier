# vibe-code-ntier

A simple **n-tier .NET 8 demo** built to show how a change flows through all layers — Web UI, REST API, PostgreSQL — and deploys to [Elest.io](https://elest.io) via CI/CD.

```
Browser  →  NameDemo.Web  →  NameDemo.Api  →  PostgreSQL
 (UI)         (Tier 1)          (Tier 2)         (Tier 3)
                              ↓
                         n8n webhook (after save)
```

## Live demo (Elest.io)

| Tier | URL |
|------|-----|
| **Web** | https://namedemo-web-u75553.vm.elestio.app |
| **API** | https://vibe-code-mt-u75553.vm.elestio.app |
| **API health** | https://vibe-code-mt-u75553.vm.elestio.app/health |
| **API data** | https://vibe-code-mt-u75553.vm.elestio.app/api/names |

PostgreSQL runs as a separate managed Elest.io service. The Web and API tiers each run as **CI/CD pipeline containers** on a shared VM (Docker + Kestrel behind Elest.io’s reverse proxy).

## Projects

| Project | Role |
|---------|------|
| `src/NameDemo.Web` | Razor Pages frontend — name entry form and saved list |
| `src/NameDemo.Api` | ASP.NET Core Web API — `GET/POST /api/names`, n8n webhook |
| `src/NameDemo.Data` | EF Core models, `DbContext`, migrations |

## Features

- Enter a name on the Web app; it is saved via the API into PostgreSQL.
- EF Core migrations run automatically on API startup.
- After a successful save, the API calls an **n8n webhook** (GET with query params). Webhook failures are logged but **do not** roll back the database write.
- Styled dark UI (CSS embedded in the layout for reliable loading on Elest.io).

## Run locally

### Prerequisites

- [.NET SDK 8](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for local PostgreSQL only)

### 1. Start the database

```bash
docker compose -f docker-compose.dev.yml up -d
```

> `docker-compose.dev.yml` is for **local dev only**. Do not configure Elest.io pre-deploy scripts to run Docker Compose — production uses managed PostgreSQL.

### 2. Start the API (terminal 1)

```bash
export PATH="/usr/local/share/dotnet:$PATH"
dotnet run --project src/NameDemo.Api
```

- API: http://localhost:5297
- Health: http://localhost:5297/health

### 3. Start the Web app (terminal 2)

```bash
export PATH="/usr/local/share/dotnet:$PATH"
dotnet run --project src/NameDemo.Web
```

- Web: http://localhost:5265

The Web app reads `ApiBaseUrl` from `src/NameDemo.Web/appsettings.json` (defaults to the local API URL).

## Deploy on Elest.io

Use **three services** so each tier is visible in the demo:

1. **PostgreSQL** — managed database service
2. **API CI/CD pipeline** — hosts the Web API (`elestio.yml`)
3. **Web CI/CD pipeline** — hosts the Razor Pages app (`elestio-web.yml`)

Both pipelines deploy from this GitHub repo: `maperry2009/vibe-code-ntier`.

### Pipeline settings

For each pipeline:

- **Type:** Full Stack
- **Runtime:** .NET **8**
- **Lifecycle / pre-deploy scripts:** leave **empty** (no `docker compose`)

#### API pipeline

| Setting | Value |
|---------|-------|
| **Install** | `dotnet restore src/NameDemo.Api/NameDemo.Api.csproj` |
| **Build** | `dotnet publish src/NameDemo.Api/NameDemo.Api.csproj -c Release -o ./publish-api` |
| **Run** | `dotnet ./publish-api/NameDemo.Api.dll` |
| **Container port** | `8080` |

Environment variables (`KEY=value`, one per line):

```env
ASPNETCORE_URLS=http://0.0.0.0:8080
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=...;Port=...;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true
Webhook__Url=https://your-n8n-host/webhook/your-webhook-id
```

Optional CORS lockdown:

```env
AllowedOrigins__0=https://your-web-pipeline-url.vm.elestio.app
```

#### Web pipeline

Use a **different project name** than the API pipeline (e.g. `namedemo-web`).

| Setting | Value |
|---------|-------|
| **Install** | `dotnet restore src/NameDemo.Web/NameDemo.Web.csproj` |
| **Build** | `dotnet publish src/NameDemo.Web/NameDemo.Web.csproj -c Release -o ./publish-web` |
| **Run** | `cd ./publish-web && dotnet NameDemo.Web.dll` |
| **Container port** | `8080` |

Environment variables:

```env
ASPNETCORE_URLS=http://0.0.0.0:8080
ASPNETCORE_ENVIRONMENT=Production
ApiBaseUrl=https://your-api-pipeline-url.vm.elestio.app
```

Deploy the **API first**, then the **Web** pipeline with the API’s public URL (no trailing slash).

### Port mapping (shared VM)

When both pipelines run on the **same VM**, each gets its own **host port**. The app always listens on **8080 inside the container**.

```
Reverse proxy target port  =  Host port  (e.g. 3002 for API, 3001 for Web)
Container port             =  8080
```

If you see **Bad Gateway**, check that exposed ports match the reverse proxy target.

## n8n webhook integration

After a name is saved to PostgreSQL, the API calls the configured n8n webhook:

```
GET {Webhook__Url}?name=Michael&id=5&createdAt=2026-07-06T...
```

- Configured via `Webhook:Url` in settings, or `Webhook__Url` in Elest.io env vars.
- Called **after** `SaveChangesAsync` completes.
- Failures are logged as warnings; the HTTP response to the Web app still succeeds.

In n8n, access fields from the query string (e.g. `$json.query.name` depending on your Webhook node version).

## API endpoints

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/health` | Health check |
| `GET` | `/api/names` | List saved names |
| `POST` | `/api/names` | Save a name — body: `{ "name": "Alice" }` |

## Cursor demo workflow

This repo is set up to demo **cross-tier changes** with Cursor:

1. Tag a baseline: `git tag demo-baseline`
2. Prompt Cursor to add a feature across Data, API, and Web (include an EF migration).
3. Push to `main` → Elest.io rebuilds both pipelines.
4. Demo the live change on Web, API JSON, and PostgreSQL.
5. Prompt Cursor to remove the feature (with a migration to drop columns if needed).
6. Push again → back to baseline for the next audience.

Example features that work well: add a second field (last name), generate a slug in the API tier, or extend the n8n payload.

## Push to GitHub

```bash
git add .
git commit -m "Describe your change"
git push origin main
```

Elest.io CI/CD pipelines rebuild automatically on push to `main`.

## Repository layout

```
src/
  NameDemo.Web/       # Tier 1 — Razor Pages UI
  NameDemo.Api/       # Tier 2 — REST API + webhook
  NameDemo.Data/      # Tier 3 — EF Core + migrations
docker-compose.dev.yml   # Local PostgreSQL only
elestio.yml              # API pipeline reference config
elestio-web.yml          # Web pipeline reference config
```
