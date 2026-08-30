$ErrorActionPreference = 'Stop'

Write-Host "Intentum SDK Setup" -ForegroundColor Cyan
Write-Host "=================="
Write-Host ""

# Check for .NET SDK
$dotnet = Get-Command "dotnet" -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Host "ERROR: .NET SDK not found. Install from https://dot.net/download" -ForegroundColor Red
    exit 1
}

Write-Host "Found dotnet: $(dotnet --version)"

# Check for Kiota
$kiota = Get-Command "kiota" -ErrorAction SilentlyContinue
if ($kiota) {
    Write-Host "Found kiota: $(kiota --version)"
} else {
    Write-Host "Installing Microsoft.OpenApi.Kiota..." -ForegroundColor Yellow
    dotnet tool install -g Microsoft.OpenApi.Kiota
    Write-Host "Kiota installed successfully." -ForegroundColor Green
}

# Verify installation
$kiotaAfter = Get-Command "kiota" -ErrorAction SilentlyContinue
if (-not $kiotaAfter) {
    Write-Host "ERROR: Kiota installation failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Setup complete. Run '.\sdk\generate.ps1' to generate SDKs." -ForegroundColor Green
