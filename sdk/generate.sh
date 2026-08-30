#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SPEC_PATH="${1:-$SCRIPT_DIR/../docs/openapi/intentum.yaml}"

echo "Intentum SDK Generator"
echo "======================"
echo ""

if [ ! -f "$SPEC_PATH" ]; then
    echo "ERROR: OpenAPI spec not found at $SPEC_PATH" >&2
    exit 1
fi

echo "Using spec: $SPEC_PATH"

# Check for Kiota
if ! command -v kiota &> /dev/null; then
    echo "ERROR: Kiota not found. Run 'bash sdk/setup.sh' first." >&2
    exit 1
fi

# Clean previous generated code
echo "Cleaning previous SDK output..."
rm -rf "$SCRIPT_DIR/csharp/IntentumSdk"
rm -rf "$SCRIPT_DIR/python/intentum_sdk"
rm -rf "$SCRIPT_DIR/typescript/intentum-sdk"

# Generate C# SDK
echo "Generating C# SDK..."
kiota generate --openapi "$SPEC_PATH" --language csharp --output "$SCRIPT_DIR/csharp/IntentumSdk" --class-name IntentumClient

# Generate Python SDK
echo "Generating Python SDK..."
kiota generate --openapi "$SPEC_PATH" --language python --output "$SCRIPT_DIR/python/intentum_sdk" --class-name IntentumClient

# Generate TypeScript SDK
echo "Generating TypeScript SDK..."
kiota generate --openapi "$SPEC_PATH" --language typescript --output "$SCRIPT_DIR/typescript/intentum-sdk" --class-name IntentumClient

echo ""
echo "SDK generation complete!"
echo ""
echo "Generated:"
echo "  - C#:         sdk/csharp/IntentumSdk/"
echo "  - Python:     sdk/python/intentum_sdk/"
echo "  - TypeScript: sdk/typescript/intentum-sdk/"
