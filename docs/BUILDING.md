# Building and testing

Use Windows with the .NET 8 SDK and .NET Framework 4.8 targeting pack. Game and BepInEx assemblies must come from your own installation; they are not included in this repository.

```powershell
./build.ps1 -GameManaged 'D:/SteamLibrary/steamapps/common/Valheim/valheim_Data/Managed' -BepInExCore 'D:/ValheimProfile/BepInEx/core'
```

The script runs the search tests and builds the plugin. To run only the tests, including on a machine without Valheim:

```powershell
dotnet run --project tests/WorkstationSearch.Tests.csproj -c Release
```

Package a built plugin with `./package.ps1`. The output is a Thunderstore/Hexium-compatible ZIP under `artifacts`, also suitable for manual installation. Keep `manifest.json` and the plugin version in sync. The script validates the manifest version and 256x256 icon before packaging.

See [release validation](VALIDATION.md) for in-game checks.
