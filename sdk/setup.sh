#!/bin/bash
set -e

echo "Intentum SDK Setup"
echo "=================="
echo ""

# Check for .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK not found. Install from https://dot.net/download"
    exit 1
fi

echo "Found dotnet: $(dotnet --version)"

# Check for Kiota
if command -v kiota &> /dev/null; then
    echo "Found kiota: $(kiota --version)"
else
    echo "Installing Microsoft.OpenApi.Kiota..."
    dotnet tool install -g Microsoft.OpenApi.Kiota
    echo "Kiota installed successfully."
fi

echo ""
echo "Setup complete. Run 'bash sdk/generate.sh' to generate SDKs."
