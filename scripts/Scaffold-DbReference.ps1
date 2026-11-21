# Read the existing SQL Server schema into a separate folder for comparison.
# This script does not change the database and does not overwrite the running models.
$ErrorActionPreference = "Stop"
$projectRoot = Split-Path $PSScriptRoot -Parent
$referenceDirectory = Join-Path (Split-Path $projectRoot -Parent) "RealEstateWeb_DbFirstReference"
if (Test-Path $referenceDirectory) {
    throw "Reference folder already exists. Rename it before creating another reference."
}
Push-Location $projectRoot
try {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw "Tool restore failed." }
    $scaffoldArguments = @(
        "ef", "dbcontext", "scaffold",
        "Name=ConnectionStrings:DefaultConnection",
        "Microsoft.EntityFrameworkCore.SqlServer",
        "--project", "./RealEstateWeb.csproj",
        "--startup-project", "./RealEstateWeb.csproj",
        "--output-dir", (Join-Path $referenceDirectory "Models"),
        "--context-dir", (Join-Path $referenceDirectory "Data"),
        "--context", "RealEstateReferenceContext",
        "--namespace", "RealEstateWeb.Reference.Models",
        "--context-namespace", "RealEstateWeb.Reference.Data",
        "--table", "dbo.Users",
        "--table", "dbo.PropertyTypes",
        "--table", "dbo.Locations",
        "--table", "dbo.Properties",
        "--table", "dbo.PropertyImages",
        "--no-onconfiguring", "--data-annotations"
    )
    & dotnet @scaffoldArguments
    if ($LASTEXITCODE -ne 0) { throw "Scaffold failed. Check SQL Server connection and permissions." }
    Write-Host "Reference created outside the application: $referenceDirectory"
} finally {
    Pop-Location
}
