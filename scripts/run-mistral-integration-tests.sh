#!/usr/bin/env bash
# Loads .env (repo root) and runs Mistral integration tests.
# .env: MISTRAL_API_KEY (and optionally MISTRAL_BASE_URL, MISTRAL_EMBEDDING_MODEL).
# Create from .env.example; .env is gitignored.
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
echo "Running Mistral integration tests..."
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --filter "FullyQualifiedName~MistralIntegrationTests" "$@"
