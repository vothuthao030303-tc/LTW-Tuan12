$ErrorActionPreference = "Stop"
$projectRoot = Split-Path $PSScriptRoot -Parent
Push-Location $projectRoot
try {
    dotnet --list-sdks
    if ($LASTEXITCODE -ne 0) { throw "Install .NET 8 SDK first." }
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw "Local EF tools restore failed." }
    dotnet restore ./RealEstateWeb.csproj
    if ($LASTEXITCODE -ne 0) { throw "NuGet restore failed." }
    dotnet build ./RealEstateWeb.csproj --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Build failed. Resolve errors before scaffolding." }
    dotnet ef --version
    if ($LASTEXITCODE -ne 0) { throw "EF tool validation failed." }
    dotnet aspnet-codegenerator --help
    if ($LASTEXITCODE -ne 0) { throw "Scaffolding tool validation failed." }
    Write-Host "Tools and project are ready. See HUONG_DAN_EF_TUAN11.md."
} finally {
    Pop-Location
}
