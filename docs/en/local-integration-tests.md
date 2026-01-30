# Local integration tests (OpenAI)

The **OpenAI integration tests** (`OpenAIIntegrationTests`) call the real OpenAI API when `OPENAI_API_KEY` is set. They are **not** run in CI by default; use them only on your machine.

## Rules (güvenlik)

- **Never** commit your API key. Do not put it in any file under version control.
- **Never** push a file that contains the key. `.env` is in `.gitignore` and will never be committed.
- Use the key **only** in this repo, on your machine, for running tests.

## One-time setup: secret file

1. Copy the template to `.env` (in the repo root):
   ```bash
   cp .env.example .env
   ```
2. Edit `.env` and set your key:
   ```
   OPENAI_API_KEY=sk-your-key-here
   ```
   Optionally set `OPENAI_BASE_URL` and `OPENAI_EMBEDDING_MODEL`. Save and close. `.env` is gitignored.

## Run integration tests (every time)

From the repo root:

```bash
./scripts/run-integration-tests.sh
```

Or with bash: `bash scripts/run-integration-tests.sh`

The script loads `.env` and runs the OpenAI integration tests. You do not need to `export` anything each time.

## Alternative: environment variable in the terminal

If you prefer not to use a file, set the variable in the terminal and run the tests:

```bash
export OPENAI_API_KEY='sk-your-key-here'
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --filter "FullyQualifiedName~OpenAIIntegrationTests"
```

Optional variables in `.env`: `OPENAI_BASE_URL` (default `https://api.openai.com/v1/`), `OPENAI_EMBEDDING_MODEL` (default `text-embedding-3-small`).

## Summary

| Step | Action |
|------|--------|
| One-time | `cp .env.example .env` → edit `.env`, set `OPENAI_API_KEY=sk-...` |
| Every time | `./scripts/run-integration-tests.sh` (script loads `.env` and runs tests) |
| Never commit | `.env` is in `.gitignore`; do not add or push it. |
