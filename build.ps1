param(
    [string]$GameManaged = $env:VALHEIM_MANAGED,
    [string]$BepInExCore = $env:BEPINEX_CORE
)
$ErrorActionPreference = 'Stop'
foreach ($file in @('assembly_valheim.dll', 'Unity.TextMeshPro.dll')) {
    if (-not $GameManaged -or -not (Test-Path -LiteralPath (Join-Path $GameManaged $file))) { throw "Missing game assembly: $file. Set -GameManaged." }
}
foreach ($file in @('BepInEx.dll', '0Harmony.dll')) {
    if (-not $BepInExCore -or -not (Test-Path -LiteralPath (Join-Path $BepInExCore $file))) { throw "Missing BepInEx assembly: $file. Set -BepInExCore." }
}
dotnet run --project "$PSScriptRoot/tests/WorkstationSearch.Tests.csproj" -c Release
if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
dotnet build "$PSScriptRoot/src/WorkstationSearch.csproj" -c Release --nologo "-p:GameManaged=$GameManaged" "-p:BepInExCore=$BepInExCore"
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
