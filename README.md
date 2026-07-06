# vibe-code-ntier

A simple **n-tier .NET demo**: Razor Pages frontend, ASP.NET Core Web API, and PostgreSQL.

```
Browser  →  NameDemo.Web  →  NameDemo.Api  →  PostgreSQL
 (UI)         (Tier 1)          (Tier 2)         (Tier 3)
```

## Projects

| Project | Role |
|---------|------|
| `src/NameDemo.Web` | Frontend — "Enter your name" form |
| `src/NameDemo.Api` | Web API — `GET/POST /api/names` |
| `src/NameDemo.Data` | Data layer — EF Core models and `DbContext` |

## Run locally

### Prerequisites

- [.NET SDK 8](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for PostgreSQL)

### 1. Start the database

```bash
docker compose -f docker-compose.dev.yml up -d
```

### 2. Start the API (terminal 1)

```bash
export PATH="/usr/local/share/dotnet:$PATH"
dotnet run --project src/NameDemo.Api
```

API: http://localhost:5297  
Health: http://localhost:5297/health

### 3. Start the Web app (terminal 2)

```bash
export PATH="/usr/local/share/dotnet:$PATH"
dotnet run --project src/NameDemo.Web
```

Web: http://localhost:5265

The API applies EF Core migrations automatically on startup.

## Deploy on Elest.io

Use **three services** so each tier is visible in the demo.

### 1. PostgreSQL

Create a managed **PostgreSQL** service in Elest.io and copy its connection string.

### 2. API pipeline

1. CI/CD → GitHub → import `maperry2009/vibe-code-ntier`
2. Runtime: **.NET 8**
3. Use settings from [`elestio.yml`](elestio.yml), or set manually:
   - **Install:** `dotnet restore src/NameDemo.Api/NameDemo.Api.csproj`
   - **Build:** `dotnet publish src/NameDemo.Api/NameDemo.Api.csproj -c Release -o ./publish-api`
   - **Run:** `dotnet ./publish-api/NameDemo.Api.dll`
   - **Container port:** `8080`
   - **Reverse proxy target port:** `8080`
4. Environment variables:

   | Key | Value |
   |-----|-------|
   | `ASPNETCORE_URLS` | `http://0.0.0.0:8080` |
   | `ConnectionStrings__DefaultConnection` | Your Elest.io PostgreSQL connection string |
   | `AllowedOrigins__0` | Your Web pipeline public URL (e.g. `https://your-web-u123.vm.elestio.app`) |

### 3. Web pipeline

Create a **second CI/CD pipeline** from the same repo using [`elestio-web.yml`](elestio-web.yml):

- **Install:** `dotnet restore src/NameDemo.Web/NameDemo.Web.csproj`
- **Build:** `dotnet publish src/NameDemo.Web/NameDemo.Web.csproj -c Release -o ./publish-web`
- **Run:** `cd ./publish-web && dotnet NameDemo.Web.dll`
- **Container port:** `8080`

Environment variables:

| Key | Value |
|-----|-------|
| `ASPNETCORE_URLS` | `http://0.0.0.0:8080` |
| `ApiBaseUrl` | Your API pipeline public URL (e.g. `https://your-api-u123.vm.elestio.app`) |

Deploy the **API first**, then the **Web** pipeline with the API URL.

## API endpoints

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/names` | List saved names |
| `POST` | `/api/names` | Save a name (`{ "name": "Alice" }`) |
| `GET` | `/health` | Health check |

## Push to GitHub

```bash
git add .
git commit -m "Add n-tier name demo"
git push origin main
```
