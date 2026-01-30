#!/usr/bin/env bash
# Run JetBrains Rider/ReSharper InspectCode and write XML so Cursor/AI can read results.
# Requires: dotnet tool install JetBrains.ReSharper.GlobalTools --global
# Then: jb inspectcode ...

set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
SOLUTION="$REPO_ROOT/Intentum.slnx"
OUTPUT_XML="$REPO_ROOT/inspection-results.xml"

if ! command -v jb &>/dev/null; then
  echo "jb not found. Install ReSharper command line tools:"
  echo "  dotnet tool install JetBrains.ReSharper.GlobalTools --global"
  exit 1
fi

cd "$REPO_ROOT"
echo "Running InspectCode on $SOLUTION -> $OUTPUT_XML"
jb inspectcode "$SOLUTION" -o="$OUTPUT_XML" -f=Xml 2>&1 || true
echo "Done. Read inspection-results.xml for issues (or use: cat inspection-results.xml)."
