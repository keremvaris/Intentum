#!/usr/bin/env bash
# Run unit tests with coverage locally. Report: TestResults/coverage.opencover.xml (Rider can open it).
set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$REPO_ROOT"

echo "Building..."
dotnet build Intentum.slnx --verbosity quiet

mkdir -p TestResults
echo "Running tests with coverage (same filter as CI)..."
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj \
  --no-build \
  -verbosity minimal \
  --filter 'FullyQualifiedName!~GreenwashingCaseStudyTests&FullyQualifiedName!~IntegrationTests' \
  --results-directory TestResults \
  /p:CollectCoverage=true \
  /p:CoverletOutput="$REPO_ROOT/TestResults/coverage.opencover.xml" \
  /p:CoverletOutputFormat=opencover

echo ""
echo "Coverage report: $REPO_ROOT/TestResults/coverage.opencover.xml"
echo "Open in Rider: Run → Show Coverage (or open the XML)."
