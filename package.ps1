$ErrorActionPreference = 'Stop'
$dll = Join-Path $PSScriptRoot 'src/bin/Release/net48/WorkstationSearch.dll'
if (-not (Test-Path -LiteralPath $dll)) { throw 'Run build.ps1 first.' }
[xml]$project = Get-Content -LiteralPath "$PSScriptRoot/src/WorkstationSearch.csproj"
$version = [string]$project.Project.PropertyGroup.Version
$actual = [Diagnostics.FileVersionInfo]::GetVersionInfo($dll).ProductVersion
if ($actual -ne $version -and -not $actual.StartsWith($version + '+')) { throw "Stale DLL: $actual, expected $version." }
$artifacts = Join-Path $PSScriptRoot 'artifacts'
$stage = Join-Path $artifacts ('stage-' + [guid]::NewGuid().ToString('N'))
$plugins = Join-Path $stage 'BepInEx/plugins/WorkstationSearch'
New-Item -ItemType Directory -Path $plugins -Force | Out-Null
Copy-Item -LiteralPath $dll -Destination $plugins
foreach ($name in @('README.md', 'CHANGELOG.md', 'LICENSE.txt')) { Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination $stage }
Copy-Item -LiteralPath "$PSScriptRoot/docs" -Destination $stage -Recurse
$zip = Join-Path $artifacts "WorkstationSearch-$version.zip"
Compress-Archive -Path "$stage/*" -DestinationPath $zip -Force
Get-FileHash -LiteralPath $zip -Algorithm SHA256
