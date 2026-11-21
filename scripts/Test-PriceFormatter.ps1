# Compiles the actual formatter independently of ASP.NET and SQL Server.
$ErrorActionPreference = "Stop"
$projectRoot = Split-Path $PSScriptRoot -Parent
$testDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ("RealHomePriceChecks-" + [Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $testDirectory | Out-Null
try {
    @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup>
</Project>
'@ | Set-Content -Encoding UTF8 (Join-Path $testDirectory "PriceChecks.csproj")
    Copy-Item (Join-Path $projectRoot "Helpers/PriceFormatter.cs") (Join-Path $testDirectory "PriceFormatter.cs")
    @'
using RealEstateWeb.Helpers;
var cases = new (decimal Amount, string Expected)[]
{
    (10_200_000_000m, "10 tỷ 2"),
    (6_200_000_000m, "6 tỷ 2"),
    (12_800_000_000m, "12 tỷ 8"),
    (10_000_000_000m, "10 tỷ"),
    (1_000_000_000m, "1 tỷ"),
    (1_250_000_000m, "1 tỷ 25"),
    (10_020_000_000m, "10 tỷ 02"),
    (10_002_000_000m, "10 tỷ 002"),
    (999_000_000m, "999 triệu"),
    (850_500_000m, "850,5 triệu"),
    (1_000_000m, "1 triệu"),
    (999_999m, "999.999 đ"),
    (0m, "0 đ"),
    (10_200_500_000m, "10 tỷ 200,5 triệu"),
    (1_000_000_000.01m, "1 tỷ 0,00000001 triệu"),
    (-1_200_000_000m, "-1 tỷ 2")
};
foreach (var test in cases)
{
    var actual = PriceFormatter.Short(test.Amount);
    if (actual != test.Expected)
        throw new Exception($"{test.Amount}: expected '{test.Expected}', actual '{actual}'");
}
if (PriceFormatter.Full(10_200_000_000m) != "10.200.000.000 đ")
    throw new Exception("Full amount tooltip failed.");
if (PriceFormatter.Full(12.50m) != "12,50 đ")
    throw new Exception("Fractional VND display failed.");
Console.WriteLine($"PASS: {cases.Length} short-price cases and 2 full-price cases.");
'@ | Set-Content -Encoding UTF8 (Join-Path $testDirectory "Program.cs")
    dotnet run --project (Join-Path $testDirectory "PriceChecks.csproj")
    if ($LASTEXITCODE -ne 0) { throw "Price formatter checks failed." }
} finally {
    Remove-Item -Recurse -Force -LiteralPath $testDirectory
}
