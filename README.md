# Disharmony

Disharmony is a C# game-modding framework built on Harmony. It supports patches around methods and around individual calls, member accesses, and constants inside methods.

The initial beta supports Mono/net472, Microsoft CLR/net472, and Microsoft CLR/net10.0 with Harmony 2. See [compatibility](docs/compatibility.md) to choose the appropriate build and check Harmony version support.

- [Introduction and usage](Disharmony/README.md)
- [RimWorld library mod](Disharmony.RimWorld/README.md)
- [Optional analyzers](Disharmony.Analyzers/README.md)
- [Tests and runtime compatibility](Disharmony.Tests/README.md)
- [Deferred test-coverage proposals](Disharmony.Tests/Test-Plan.md)

## Build

Use the .NET 10.0.400 SDK or a later patch in the 10.0.4xx feature band. The repository selects that feature band through `global.json` and uses C# 14. The library and runtime tests target .NET Framework 4.7.2 and .NET 10; the analyzers target .NET Standard 2.0.

```powershell
dotnet build .\Disharmony.sln
dotnet test .\Disharmony.Tests\Disharmony.Tests.csproj
dotnet test .\Disharmony.Analyzers.Tests\Disharmony.Analyzers.Tests.csproj
```

The solution builds test-target fixtures in Release configuration for predictable generated IL, including during Debug builds. See the test guide for Mono and Harmony 3 preview compatibility runs.

## HTML documentation

Run `./docs/Build-Docs.ps1 -Serve` to build the searchable HTML guides and API reference and preview them at `http://localhost:8080`. See [documentation build instructions](docs/README.md).

## License

[MIT](LICENSE). The pinned Harmony test dependency includes its own license under `TestArtifacts/Harmony`.
