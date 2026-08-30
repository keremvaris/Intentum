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

# Verify C# SDK was generated
if [ ! -d "$SCRIPT_DIR/csharp/IntentumSdk" ]; then
    echo "ERROR: C# SDK generation failed" >&2
    exit 1
fi

# Create .csproj file if not present (Kiota 1.34+ no longer generates it)
if [ ! -f "$SCRIPT_DIR/csharp/IntentumSdk/IntentumSdk.csproj" ]; then
    echo "Creating IntentumSdk.csproj..."
    cat > "$SCRIPT_DIR/csharp/IntentumSdk/IntentumSdk.csproj" << 'CSPROJ'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>IntentumSdk</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Kiota.Bundle" Version="2.0.0" />
  </ItemGroup>
</Project>
CSPROJ
fi

# Generate Python SDK
echo "Generating Python SDK..."
kiota generate --openapi "$SPEC_PATH" --language python --output "$SCRIPT_DIR/python/intentum_sdk" --class-name IntentumClient

# Verify Python SDK was generated
if [ ! -d "$SCRIPT_DIR/python/intentum_sdk" ]; then
    echo "ERROR: Python SDK generation failed" >&2
    exit 1
fi

# Generate TypeScript SDK
echo "Generating TypeScript SDK..."
kiota generate --openapi "$SPEC_PATH" --language typescript --output "$SCRIPT_DIR/typescript/intentum-sdk" --class-name IntentumClient

# Verify TypeScript SDK was generated
if [ ! -d "$SCRIPT_DIR/typescript/intentum-sdk" ]; then
    echo "ERROR: TypeScript SDK generation failed" >&2
    exit 1
fi

echo ""
echo "SDK generation complete!"
echo ""
echo "Generated:"
echo "  - C#:         sdk/csharp/IntentumSdk/"
echo "  - Python:     sdk/python/intentum_sdk/"
echo "  - TypeScript: sdk/typescript/intentum-sdk/"
