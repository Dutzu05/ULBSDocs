# ULBS Doc Auth

🚀 **ULBS Doc Auth** is a small end-to-end app that:
- authenticates users via **Google OAuth**
- lets them upload documents for **conversion** (DOC → DOCX and DOCX → PDF)
- exposes a clean **ASP.NET Core Web API** + a minimal static **frontend**

## Architecture (at a glance) 🧱

**User flow**
- Browser loads the static UI from the API (served from `frontend/`)
- User signs in with Google (Google Identity Services)
- Frontend calls the API endpoints for conversion and downloads the resulting file

**Runtime components**
- **Frontend (static HTML/CSS/JS)**: in `frontend/` (login + conversion UI)
- **API (ASP.NET Core, .NET 8)**: `UlbsDocAuth.Api/`
  - serves the frontend as static files
  - exposes REST endpoints for auth, certificates and conversions
  - contains the conversion controllers (upload-based conversion included)
- **DOCX → PDF converter helper**: `docx-to-pdf/DocxToPdfConverter/` (a separate converter project used for PDF conversion)

**Testing strategy** ✅
- `UlbsDocAuth.Api.Tests/`: fast unit + in-memory integration tests
- `UlbsDocAuth.E2E/`: smoke tests that hit a running API over real HTTP
- Pre-commit + CI enforce **tests passing** + **≥80% line coverage**

This repo contains:
- **UlbsDocAuth.Api**: ASP.NET Core Web API (runs on http://localhost:3000 in dev)
- **UlbsDocAuth.Api.Tests**: unit + in-memory integration tests (fast, no external deps)
- **UlbsDocAuth.E2E**: "real HTTP" smoke tests against a running API (local or staging)

## Run the API (local)
From the repo root:

```bash
dotnet run --project UlbsDocAuth.Api
```

Then open:
- http://localhost:3000/swagger

## Run unit/integration tests (local)

```bash
dotnet test UlbsDocAuth.Api.Tests/UlbsDocAuth.Api.Tests.csproj -c Release \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=cobertura \
  /p:Threshold=80 \
  /p:ThresholdType=line \
  /p:ThresholdStat=total
```

## Run E2E tests (local)
E2E tests **do not stop the API**. They are just another process that sends HTTP requests to your running app.

1) Start the API in one terminal
2) In another terminal:

```bash
dotnet test UlbsDocAuth.E2E/UlbsDocAuth.E2E.csproj -c Release --filter "Category=E2E"
```

By default, E2E hits `http://localhost:3000`.

To point E2E at another URL:

```bash
E2E_BASE_URL="https://your-host" dotnet test UlbsDocAuth.E2E/UlbsDocAuth.E2E.csproj -c Release --filter "Category=E2E"
```

## Git hooks (pre-commit)
A **pre-commit hook** runs **before** `git commit` finishes. In this repo it runs unit/integration tests + the 80% coverage gate.

Hooks are **local to your clone**. They do not affect running the application on another computer.

Enable the repo’s committable hooks:

```bash
./scripts/setup-hooks.sh
```

## GitHub Actions workflows
Workflows live in `.github/workflows/`.

- **CI**: `.github/workflows/ci.yml`
  - runs on push + PR
  - executes the unit/integration test suite + coverage gate

- **Scheduled E2E (staging)**: `.github/workflows/e2e-staging.yml`
  - runs daily (and can be manually triggered)
  - runs E2E smoke tests against a **running staging URL**
  - does **not** deploy anything by itself

To make scheduled E2E actually run, set a repo secret:
- `STAGING_BASE_URL` (example: `https://staging.example.com`)

Then you can watch results in GitHub → **Actions** tab.
