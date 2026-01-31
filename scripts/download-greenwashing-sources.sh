#!/usr/bin/env bash
# Download public greenwashing source pages (ClientEarth profiles, Sustainable Agency article)
# into docs/case-studies/downloaded/ for local analysis. No auth required.
set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
OUT_DIR="$REPO_ROOT/docs/case-studies/downloaded"
mkdir -p "$OUT_DIR"
cd "$REPO_ROOT"

echo "Downloading greenwashing sources to $OUT_DIR ..."

# ClientEarth Greenwashing Files — company profiles
for name in aramco chevron drax equinor exxonmobil ineos rwe shell total; do
  url="https://www.clientearth.org/projects/the-greenwashing-files/${name}/"
  out="$OUT_DIR/clientearth-${name}.html"
  echo "  $url -> $out"
  curl -sL --proto '=https' -o "$out" "$url" || echo "  (failed: $name)"
done

# ClientEarth index
curl -sL --proto '=https' -o "$OUT_DIR/clientearth-index.html" "https://www.clientearth.org/projects/the-greenwashing-files/" || true

# The Sustainable Agency — 20+ greenwashing examples article
curl -sL --proto '=https' -o "$OUT_DIR/sustainable-agency-greenwashing-examples.html" "https://thesustainableagency.com/blog/greenwashing-examples" || true

echo "Done. Files in $OUT_DIR"
echo "Mendeley dataset: if already downloaded, see docs/case-studies/downloaded/DataGreenwash/"
