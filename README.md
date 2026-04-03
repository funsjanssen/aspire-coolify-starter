# Aspire + Coolify starter

A minimal [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/) solution with a Blazor web front end, a sample API, and a GitHub Actions workflow that builds container images and triggers a [Coolify](https://coolify.io/) deployment. Use it as a template for shipping Aspire-style apps to your own server.

## What’s included

| Piece | Role |
|--------|------|
| **AspireCoolifyStarter.AppHost** | Orchestrates local runs and emits Docker Compose output for container-based deployments. |
| **AspireCoolifyStarter.ApiService** | ASP.NET Core minimal API with OpenAPI and a sample `/weatherforecast` endpoint. Image name: `starter-api`. |
| **AspireCoolifyStarter.Web** | Blazor interactive server app that calls the API via Aspire service discovery. Image name: `starter-web`. |
| **AspireCoolifyStarter.ServiceDefaults** | Shared OpenTelemetry, health checks, and service defaults. |
| **AspireCoolifyStarter.Tests** | xUnit tests using `Aspire.Hosting.Testing`. |

The AppHost configures a Docker Compose environment (no custom bridge network) so generated `docker-compose` aligns with typical Coolify usage. Health checks are exposed at `/health` on both projects.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://docs.docker.com/get-docker/) (for running the AppHost and published containers locally)
- Optional: [Aspire CLI](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling) (`dotnet tool install -g aspire.cli`) for Aspire-specific workflows

## Local development

From the repository root:

```bash
aspire run
```

The dashboard and service URLs are printed in the console. Run tests with:

```bash
dotnet test
```

## Build container images locally

Images are produced with the built-in container publish target (names come from `ContainerImageName` in each `.csproj`):

```bash
dotnet publish src/AspireCoolifyStarter.ApiService/AspireCoolifyStarter.ApiService.csproj -c Release -t:PublishContainer
dotnet publish src/AspireCoolifyStarter.Web/AspireCoolifyStarter.Web.csproj -c Release -t:PublishContainer
```

## Deploy with GitHub Actions and Coolify

The workflow [`.github/workflows/deploy-coolify.yml`](.github/workflows/deploy-coolify.yml) runs on pushes to `main` and on manual dispatch. It:

1. Installs the Aspire CLI (prerelease).
2. Publishes both projects as containers and pushes them to **GitHub Container Registry** as:
   - `ghcr.io/<your-github-username-or-org>/starter-api:latest` (and per-commit tags)
   - `ghcr.io/<your-github-username-or-org>/starter-web:latest` (and per-commit tags)
3. Calls your Coolify deploy webhook with a bearer token.

### Repository secrets

Configure these in **GitHub → Settings → Secrets and variables → Actions**:

| Secret | Purpose |
|--------|---------|
| `COOLIFY_WEBHOOK` | Deploy webhook URL from Coolify (**Project → Webhooks**). |
| `COOLIFY_TOKEN` | API token with deploy permission (**Keys & Tokens → API tokens**). |

The workflow uses `GITHUB_TOKEN` to authenticate to GHCR; ensure **Packages** write permission is allowed for the workflow (the file sets `packages: write`).

### Coolify environment

Point your Coolify project at the same images the workflow pushes. When using Compose output from Aspire (see `src/AspireCoolifyStarter.AppHost/aspire-output/docker-compose.yaml` after a local run), the stack expects image and port variables such as `APISERVICE_IMAGE`, `WEBFRONTEND_IMAGE`, `APISERVICE_PORT`, and `WEBFRONTEND_PORT`. Set those to your GHCR image references and the ports Coolify should expose (commonly `8080` for each service if the app listens on that port).

For a step-by-step narrative that matches this pattern, see [Deploy a .NET Aspire app to Coolify using GitHub Actions](https://www.fjan.nl/en/posts/how-to-deploy-a-dotnet-aspire-app-to-coolify-using-github-actions).

## Project layout

```
src/
  AspireCoolifyStarter.AppHost/     # Aspire orchestration
  AspireCoolifyStarter.ApiService/  # Backend API
  AspireCoolifyStarter.Web/         # Blazor front end
  AspireCoolifyStarter.ServiceDefaults/
test/
  AspireCoolifyStarter.Tests/
```

## Contributing

Issues and pull requests are welcome. Fork the repo, keep changes focused, and run `dotnet test` before submitting.
