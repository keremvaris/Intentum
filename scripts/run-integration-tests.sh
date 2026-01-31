#!/usr/bin/env bash
# Loads local secrets from .env (repo root) and runs OpenAI integration tests.
# .env is gitignored; create it from .env.example and set OPENAI_API_KEY.
set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
ENV_FILE="$REPO_ROOT/.env"
if [[ -f "$ENV_FILE" ]]; then
  set -a
  # shellcheck source=/dev/null
  . "$ENV_FILE"
  set +a
fi
cd "$REPO_ROOT"
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --filter "FullyQualifiedName~OpenAIIntegrationTests" "$@"
