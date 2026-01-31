#!/usr/bin/env bash
# Run Intentum.Benchmarks in Release and copy the Markdown report to docs/case-studies/benchmark-results.md.
set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$REPO_ROOT"

echo "Running benchmarks (Release)..."
dotnet run --project benchmarks/Intentum.Benchmarks/Intentum.Benchmarks.csproj -c Release

ARTIFACTS_DIR="$REPO_ROOT/BenchmarkDotNet.Artifacts/results"
REPORT=$(find "$ARTIFACTS_DIR" -name "*-report-github.md" -print -quit 2>/dev/null || true)
if [[ -z "$REPORT" ]]; then
  echo "No *-report-github.md found under $ARTIFACTS_DIR"
  exit 1
fi

DEST="$REPO_ROOT/docs/case-studies/benchmark-results.md"
echo "Copying $REPORT -> $DEST"
{
  echo "# Benchmark results (Intentum.Benchmarks)"
  echo ""
  echo "This file is **generated** by running \`./scripts/run-benchmarks.sh\` from the repository root."
  echo "Commit this file after running the script to keep published results in sync."
  echo ""
  echo "---"
  echo ""
  cat "$REPORT"
  echo ""
  echo "---"
  echo ""
  echo "See [Benchmarks](../en/benchmarks.md) for how to run and filter benchmarks. For improvement ideas and suggested solutions, see [Benchmarks — Improvement opportunities](../en/benchmarks.md#improvement-opportunities-and-suggested-solutions)."
} > "$DEST"
echo "Done: $DEST"
