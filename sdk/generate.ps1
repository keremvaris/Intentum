# sdk/generate.ps1
param(
    [string]$SpecPath = "$PSScriptRoot\..\docs\openapi\intentum.yaml"
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

# Clean previous generated code
Write-Host "Cleaning previous SDK output..."
if (Test-Path "$PSScriptRoot\csharp\IntentumSdk") { Remove-Item -Recurse -Force "$PSScriptRoot\csharp\IntentumSdk" }
if (Test-Path "$PSScriptRoot\python\intentum_sdk") { Remove-Item -Recurse -Force "$PSScriptRoot\python\intentum_sdk" }
if (Test-Path "$PSScriptRoot\typescript\intentum-sdk") { Remove-Item -Recurse -Force "$PSScriptRoot\typescript\intentum-sdk" }

# C# SDK
Write-Host "Generating C# SDK..."
kiota generate --openapi $SpecPath --language csharp --output "$PSScriptRoot\csharp\IntentumSdk" --class-name IntentumClient

if (-not (Test-Path "csharp/IntentumSdk/IntentumSdk.csproj")) {
    Write-Host "ERROR: C# SDK generation failed" -ForegroundColor Red
    exit 1
}

# Python SDK
Write-Host "Generating Python SDK..."
kiota generate --openapi $SpecPath --language python --output "$PSScriptRoot\python\intentum_sdk" --class-name IntentumClient

if (-not (Test-Path "python/intentum_sdk")) {
    Write-Host "ERROR: Python SDK generation failed" -ForegroundColor Red
    exit 1
}

# TypeScript SDK
Write-Host "Generating TypeScript SDK..."
kiota generate --openapi $SpecPath --language typescript --output "$PSScriptRoot\typescript\intentum-sdk" --class-name IntentumClient

if (-not (Test-Path "typescript/intentum-sdk")) {
    Write-Host "ERROR: TypeScript SDK generation failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "SDK generation complete!" -ForegroundColor Green
