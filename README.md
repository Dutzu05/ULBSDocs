# ULBS Doc Auth

## Progress so far (context / recap)
This section is a quick, presentation-friendly recap of what has been implemented and documented in this repository so far, so it’s easy to understand the current state and continue development.

### What this repository contains
- **UlbsDocAuth.Api** — ASP.NET Core Web API (dev URL: http://localhost:3000)
- **UlbsDocAuth.Api.Tests** — unit + in-memory integration tests (fast, no external dependencies)
- **UlbsDocAuth.E2E** — end-to-end “real HTTP” smoke tests against a running API (local or staging)

### What we set up / covered
- **Local run workflow**
  - Running the API from the repo root.
  - Using **Swagger** as the main local interface to explore and validate endpoints.
- **Automated testing approach**
  - A clear split between:
    - **Unit + integration tests** (in-memory, fast feedback)
    - **E2E tests** (separate process that sends HTTP requests to a running instance)
  - A **coverage gate (80%)** enforced when running the unit/integration test suite.
- **Developer experience safeguards (Git hooks)**
  - A committable **pre-commit hook** that runs unit/integration tests + the coverage threshold before allowing a commit to finish.
  - Documented how to enable hooks locally via `./scripts/setup-hooks.sh`.
- **CI / automation in GitHub Actions**
  - **CI workflow** (`.github/workflows/ci.yml`) that runs on push and PR and enforces the same test + coverage gate.
  - **Scheduled E2E (staging)** workflow (`.github/workflows/e2e-staging.yml`) that runs daily (and can be triggered manually) to validate a deployed staging instance.
  - Documented the required secret to enable staged E2E runs: `STAGING_BASE_URL`.

### How to continue from here (typical next steps)
- Implement and iterate on API features while keeping:
  - unit/integration tests green and above the coverage threshold
  - E2E smoke coverage for critical user flows/endpoints
- Configure a real staging environment and set `STAGING_BASE_URL` so scheduled E2E runs provide ongoing confidence.
- Expand documentation as endpoints and authentication flows evolve (and keep the run/test instructions up to date).

---

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