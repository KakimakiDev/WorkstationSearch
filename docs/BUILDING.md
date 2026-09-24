# Building and testing

Use Windows with the .NET 8 SDK and .NET Framework 4.8 targeting pack. Game and BepInEx assemblies must come from your own installation; they are not included in this repository.

```powershell
./build.ps1 -GameManaged 'D:/SteamLibrary/steamapps/common/Valheim/valheim_Data/Managed' -BepInExCore 'D:/ValheimProfile/BepInEx/core'
```

The script runs the search tests and builds the plugin. To run only the tests, including on a machine without Valheim:

```powershell
dotnet run --project tests/WorkstationSearch.Tests.csproj -c Release
```

Package a built plugin with `./package.ps1`. The output is a manual-install ZIP under `artifacts`.

See [release validation](VALIDATION.md) for in-game checks.
