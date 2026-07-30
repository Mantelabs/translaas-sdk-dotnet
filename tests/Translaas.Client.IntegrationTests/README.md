# Translaas Client Integration Tests

This project contains integration tests for the Translaas Client SDK. These tests are designed to run against a real development API instance.

## Prerequisites

- A running Translaas API instance (development environment)
- Valid API key for the development environment

## Configuration

Integration tests are configured via environment variables:

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `TRANSLAAS_API_KEY` | **Yes** to run | — | Raw `X-Api-Key` value |
| `TRANSLAAS_BASE_URL` | No | `https://api.translaas.local` | API origin only (no `/api` or `/sdk` suffix) |
| `TRANSLAAS_DEFAULT_PROJECT` | No | `translaas-sdk-samples` | Project id for scoped reads |

**Note:** Do NOT include `/api` in the BaseUrl — the client adds `/sdk/v1/translations/...` and `/api/v1/...` paths automatically.

## Running Integration Tests

### Windows (PowerShell)

```powershell
$env:TRANSLAAS_API_KEY = "your-api-key-here"
$env:TRANSLAAS_BASE_URL = "https://api.translaas.local"  # Optional
$env:TRANSLAAS_DEFAULT_PROJECT = "translaas-sdk-samples"  # Optional
dotnet test tests/Translaas.Client.IntegrationTests
```

### Linux/macOS (Bash)

```bash
export TRANSLAAS_API_KEY="your-api-key-here"
export TRANSLAAS_BASE_URL="https://api.translaas.local"  # Optional
export TRANSLAAS_DEFAULT_PROJECT="translaas-sdk-samples"  # Optional
dotnet test tests/Translaas.Client.IntegrationTests
```

### Running Specific Tests

```bash
# Run only GetEntryAsync tests
dotnet test tests/Translaas.Client.IntegrationTests --filter "FullyQualifiedName~GetEntryAsync"

# Run only error scenario tests
dotnet test tests/Translaas.Client.IntegrationTests --filter "FullyQualifiedName~ErrorScenarios"
```

## Test Behavior

- **If TRANSLAAS_API_KEY is not set**: Tests will be skipped automatically (no failures)
- **If TRANSLAAS_API_KEY is set**: Tests will run against the configured API

## Local Docker (`platform/translaas`)

Local Compose exposes one API origin for Admin (`/api/v1/...`) and SDK (`/sdk/v1/...`) routes. The default base URL is **`https://api.translaas.local`** (same as `TRANSLAAS_BASE_URL` in platform `.env.example`).

```powershell
# After: docker compose --profile core up -d
$env:TRANSLAAS_API_KEY = "<your-sdk-api-key>"
dotnet test tests/Translaas.Client.IntegrationTests
```

## Fixture Data

Canonical strings live in [translaas-sdk-examples `translaas_sdk_samples_strings.csv`](https://github.com/Mantelabs/translaas-sdk-examples/blob/main/dotnet/docs/translaas_sdk_samples_strings.csv). Live tests default to:

| Field | Value |
|-------|-------|
| Project | `translaas-sdk-samples` |
| Group (simple entry) | `common` |
| Entry (simple) | `welcome.message` |
| Group (plural) | `messages` |
| Entry (plural) | `item` |
| Language | `en` (optional: `fr`, `es`, `de`) |

Example SDK URL (matches Postman):

`GET /sdk/v1/translations/text?project=translaas-sdk-samples&group=common&lang=en&entry=welcome.message`

Constants are centralized in `IntegrationTestFixtures.cs`. Override the project with `TRANSLAAS_DEFAULT_PROJECT` when your API uses a different project id.

Tests that require populated payloads **soft-skip** when the API returns empty containers (204).

### API Behavior Notes

- The API returns **204 No Content** for non-existent resources (not 404 errors)
- The client handles 204 responses by returning:
  - **GetEntryAsync**: Returns the entry key as fallback (common i18n pattern)
  - **GetGroupAsync**: Returns empty `TranslationGroup`
  - **GetProjectAsync**: Returns empty `TranslationProject`
  - **GetProjectLocalesAsync**: Returns empty `ProjectLocales`
- Tests that expect data will fail if the test data doesn't exist in your API
- Tests for "not found" scenarios expect empty data, not exceptions

## CI/CD Integration

These tests are **optional** and should **not** run automatically in CI/CD pipelines unless:

1. A development API instance is available
2. A valid API key is configured as a CI/CD secret
3. The tests are explicitly enabled via environment variables

To exclude integration tests from CI/CD, ensure `TRANSLAAS_API_KEY` is not set in your CI/CD environment.
