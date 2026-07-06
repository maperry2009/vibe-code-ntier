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

## The demo

### Purpose

Show that **Cursor can change a real n-tier .NET application across all layers** — database, API, and UI — then deploy it live from GitHub to Elest.io without manual server setup.

The audience should leave understanding:

1. **Separation of tiers** — the browser never talks directly to the database.
2. **Modern hosting** — CI/CD pipelines run your apps in containers; you don’t manage IIS or a dedicated web server.
3. **AI-assisted development** — one prompt coordinates changes in multiple projects, migrations, and config.

### What you’re showing (baseline)

Before any live coding, walk through the running app (~2 minutes):

| Step | Show | Say |
|------|------|-----|
| 1 | **Web** — enter a name, submit | “This is the presentation tier — a Razor Pages app in its own container.” |
| 2 | **API** — open `/api/names` | “The Web app called this REST API. Here’s the JSON — no HTML, just data.” |
| 3 | **Elest.io dashboard** | “Three services: Web pipeline, API pipeline, and managed PostgreSQL. Same repo, two deployables.” |
| 4 | *(Optional)* **n8n** | “After save, the API fires a webhook — automation without blocking the database write.” |

The tier badges on the Web page (`Web → API → PostgreSQL`) match what you describe.

### Live Cursor demo (~10–15 minutes)

**Act 1 — Baseline (already running)**

- Confirm the app works: submit a name, show it in the list and in `/api/names`.

**Act 2 — Add a feature with Cursor**

Example prompt:

> Add a Last Name field across all 3 tiers (Data, API, Web). Include an EF migration. Keep existing first-name behavior working.

While Cursor works, explain that it will touch:

- `NameDemo.Data` — entity + migration
- `NameDemo.Api` — request/response contract
- `NameDemo.Web` — form and list display

Then:

```bash
git add .
git commit -m "Demo: add last name across all tiers"
git push origin main
```

Wait for Elest.io to rebuild **both** pipelines (~5–10 min). Mention: push triggers CI/CD; API runs the migration on startup.

**Act 3 — Prove all three tiers changed**

| Tier | Proof |
|------|-------|
| Web | Two input fields; full name in the list |
| API | `/api/names` includes `"lastName"` |
| Database | “Migration added a column when the API redeployed” |

**Act 4 — Revert with Cursor**

> Remove the last name feature completely from all 3 tiers. Add an EF migration to drop the column.

Push again → app returns to single-field baseline. Repeatable for the next audience.

### Suggested Cursor prompts

| Goal | Prompt |
|------|--------|
| Add a field | “Add [field] across Data, API, and Web with EF migration.” |
| Add API logic | “Generate a URL slug from the name in the API tier and store it in the database.” |
| Extend integration | “Include [field] in the n8n webhook query parameters.” |
| Revert | “Remove [feature] from all 3 tiers and add a migration to drop the column.” |

### n8n beat (optional)

If n8n is part of the story:

1. Open the n8n workflow before submitting a name.
2. Submit a name on the Web app.
3. Show the new execution in n8n with `name`, `id`, and `createdAt` from the query string.

Emphasize: the save succeeds even if n8n is down — the webhook is best-effort.

### Talking points (architecture)

- **“Where’s the web server?”** — Kestrel inside each .NET container; Elest.io’s reverse proxy handles HTTPS.
- **“Is this old n-tier?”** — Same logical layers enterprises use; hosting is containers + managed DB instead of separate IIS boxes.
- **“Why separate API and Web?”** — Clear boundaries, independent deploy, API could also serve mobile or SPA clients.

### Before you present

- [ ] Web and API URLs load (`/health` returns OK on API).
- [ ] Submitting a name works end-to-end.
- [ ] Git push access works (Personal Access Token if needed).
- [ ] Elest.io pipelines set to deploy from `main`.
- [ ] *(Optional)* n8n workflow active; `Webhook__Url` set on API pipeline.
- [ ] Tag baseline: `git tag demo-baseline`

### Demo length

| Segment | Time |
|---------|------|
| Baseline walkthrough | ~2 min |
| Cursor add feature + push | ~3 min coding + ~5–10 min deploy |
| Prove tiers changed | ~2 min |
| Cursor revert + push | ~3 min + deploy (can narrate while waiting) |
| **Total** | **~15–25 min** |

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

## Cursor + git workflow (reference)

Technical steps for the live demo:

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
