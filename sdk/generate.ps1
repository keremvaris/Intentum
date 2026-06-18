# sdk/generate.ps1
param(
    [string]$SpecPath = "../docs/openapi/intentum.yaml"
)

Write-Host "Intentum SDK Generator"
Write-Host "====================="
Write-Host ""
Write-Host "This script generates SDKs from the OpenAPI specification."
Write-Host ""
Write-Host "Prerequisites:"
Write-Host "  - .NET 10.0 SDK (dotnet)"
Write-Host "  - Microsoft Kiota (dotnet tool install -g Microsoft.OpenApi.Kiota)"
Write-Host ""

if (-not (Test-Path $SpecPath)) {
    Write-Host "ERROR: OpenAPI spec not found at $SpecPath" -ForegroundColor Red
    Write-Host "Make sure to run from the sdk/ directory or provide a valid path."
    exit 1
}

Write-Host "Generating SDKs from: $SpecPath"

# Check for Kiota
$kiotaAvailable = Get-Command "kiota" -ErrorAction SilentlyContinue
if (-not $kiotaAvailable) {
    Write-Host "WARNING: Kiota not found. Install with: dotnet tool install -g Microsoft.OpenApi.Kiota" -ForegroundColor Yellow
    Write-Host "Creating placeholder READMEs only."
    exit 0
}

# C# SDK
Write-Host "Generating C# SDK..."
kiota generate --openapi $SpecPath --language csharp --output csharp/IntentumSdk

# Python SDK
Write-Host "Generating Python SDK..."
kiota generate --openapi $SpecPath --language python --output python/intentum_sdk

# TypeScript SDK
Write-Host "Generating TypeScript SDK..."
kiota generate --openapi $SpecPath --language typescript --output typescript/intentum-sdk

Write-Host ""
Write-Host "SDK generation complete!" -ForegroundColor Green
