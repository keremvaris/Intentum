# Local integration tests and VerifyAI

This page describes how to run **real API integration tests** (OpenAI, Mistral, Gemini, Azure OpenAI) and the **VerifyAI** app locally. These require API keys and are **not** run in CI by default (CI excludes `Category=Integration`); use them on your machine only.

---

## VerifyAI (single app: all providers, embedding + full pipeline)

**VerifyAI** is the single entry point to verify that Intentum works with any provider you have a key for. It runs **embedding** and **full pipeline** (BehaviorSpace → Infer → Policy) for each provider configured in `.env`.

### One-time setup

1. Copy the template to `.env` (repo root):
   ```bash
   cp .env.example .env
   ```
2. Edit `.env` and set at least one provider’s key (see `.env.example` for variable names):
   - **OpenAI:** `OPENAI_API_KEY`, optionally `OPENAI_BASE_URL`, `OPENAI_EMBEDDING_MODEL`
   - **Mistral:** `MISTRAL_API_KEY`, optionally `MISTRAL_BASE_URL`, `MISTRAL_EMBEDDING_MODEL`
   - **Gemini:** `GEMINI_API_KEY`, optionally `GEMINI_BASE_URL`, `GEMINI_EMBEDDING_MODEL`
   - **Azure OpenAI:** `AZURE_OPENAI_ENDPOINT`, `AZURE_OPENAI_API_KEY`, optionally `AZURE_OPENAI_EMBEDDING_DEPLOYMENT`, `AZURE_OPENAI_API_VERSION`

### Run VerifyAI

From the repo root:

```bash
dotnet run --project samples/Intentum.VerifyAI
```

The app loads `.env`, then for each provider with a key: (1) calls the embedding API, (2) runs the full intent pipeline (Infer + Policy), and prints `[OK] ProviderName` or `[FAIL]` / `[SKIP]` with a message.

### Run only specific providers

To avoid unnecessary API calls, run only the providers you care about. Set `VERIFY_AI_PROVIDERS` to a comma-separated list: `OpenAI`, `Azure`, `Gemini`, `Mistral`.

Example — Mistral only:

```bash
VERIFY_AI_PROVIDERS=Mistral dotnet run --project samples/Intentum.VerifyAI
```

Example — Mistral and OpenAI:

```bash
VERIFY_AI_PROVIDERS=Mistral,OpenAI dotnet run --project samples/Intentum.VerifyAI
```

If unset, **all** providers with a key in `.env` are run.

### Verbose: see request/response bodies

To log HTTP request and response bodies (e.g. for debugging), use `VERIFY_AI_VERBOSE=1`. To see only one provider’s request/response data, combine with `VERIFY_AI_PROVIDERS`:

```bash
VERIFY_AI_PROVIDERS=Mistral VERIFY_AI_VERBOSE=1 dotnet run --project samples/Intentum.VerifyAI
```

Verbose for all providers:

```bash
VERIFY_AI_VERBOSE=1 dotnet run --project samples/Intentum.VerifyAI
```

Or add `VERIFY_AI_VERBOSE=1` to your `.env`.

---

## Integration tests (xUnit, per provider)

Integration test classes call the real API when the corresponding env vars are set. They **fail with a clear message** when the key is missing (no silent skip). CI excludes them with `--filter "Category!=Integration"`.

| Provider        | Env vars (required) | Script |
|----------------|---------------------|--------|
| OpenAI         | `OPENAI_API_KEY`    | `./scripts/run-integration-tests.sh` |
| Mistral        | `MISTRAL_API_KEY`   | `./scripts/run-mistral-integration-tests.sh` |
| Gemini         | `GEMINI_API_KEY`    | `./scripts/run-gemini-integration-tests.sh` |
| Azure OpenAI   | `AZURE_OPENAI_ENDPOINT`, `AZURE_OPENAI_API_KEY` | `./scripts/run-azure-integration-tests.sh` |

Each script loads `.env` from the repo root (if present) and runs only that provider’s integration tests.

### Run integration tests (every time)

From the repo root, after setting the right keys in `.env`:

```bash
# OpenAI
./scripts/run-integration-tests.sh

# Mistral
./scripts/run-mistral-integration-tests.sh

# Gemini
./scripts/run-gemini-integration-tests.sh

# Azure OpenAI
./scripts/run-azure-integration-tests.sh
```

Or with `dotnet test` and a filter (without loading `.env` yourself, set the vars in the shell or use the script):

```bash
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --filter "FullyQualifiedName~OpenAIIntegrationTests"
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --filter "FullyQualifiedName~MistralIntegrationTests"
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --filter "FullyQualifiedName~GeminiIntegrationTests"
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --filter "FullyQualifiedName~AzureOpenAIIntegrationTests"
```

To **exclude all integration tests** (e.g. when you have no keys):

```bash
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --filter "Category!=Integration"
```

---

## Rules (security)

- **Never** commit your API keys. Do not put them in any file under version control.
- **Never** push a file that contains a key. `.env` is in `.gitignore` and will never be committed.
- Use keys **only** in this repo, on your machine, for running tests or VerifyAI.

---

## Summary

| Step | Action |
|------|--------|
| One-time | `cp .env.example .env` → edit `.env`, set at least one provider’s key (see `.env.example`) |
| Verify all (recommended) | `dotnet run --project samples/Intentum.VerifyAI` (embedding + full pipeline per provider) |
| Verbose | `VERIFY_AI_VERBOSE=1 dotnet run --project samples/Intentum.VerifyAI` |
| Per-provider tests | `./scripts/run-integration-tests.sh` (OpenAI), `run-mistral-integration-tests.sh`, `run-gemini-integration-tests.sh`, `run-azure-integration-tests.sh` |
| Exclude integration tests | `dotnet test ... --filter "Category!=Integration"` |
| Never commit | `.env` is in `.gitignore`; do not add or push it. |
