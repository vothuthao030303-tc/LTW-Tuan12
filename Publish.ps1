param([string]$OutputDirectory = "./publish")
$ErrorActionPreference = "Stop"
Push-Location $PSScriptRoot
try {
    dotnet restore
    if ($LASTEXITCODE -ne 0) { throw "Restore failed." }
    dotnet publish ./RealEstateWeb.csproj -c Release --no-restore -o $OutputDirectory
    if ($LASTEXITCODE -ne 0) { throw "Publish failed." }
    Write-Host "Publish completed. Read HUONG_DAN_HOSTING.md before uploading."
} finally { Pop-Location }
