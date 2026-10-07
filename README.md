# Disharmony

Disharmony is a C# game-modding framework built on Harmony. It supports patches around methods and around individual calls, member accesses, and constants inside methods.

- [Introduction and usage](Disharmony/README.md)
- [Optional analyzers](Disharmony.Analyzers/README.md)
- [Tests and runtime compatibility](Disharmony.Tests/README.md)
- [Deferred test-coverage proposals](Disharmony.Tests/Test-Plan.md)

## Build

Use the .NET 10 SDK. The library and runtime tests target .NET Framework 4.7.2 and .NET 10; the analyzers target .NET Standard 2.0.

```powershell
dotnet build .\Disharmony.sln
dotnet test .\Disharmony.Tests\Disharmony.Tests.csproj
dotnet test .\Disharmony.Analyzers.Tests\Disharmony.Analyzers.Tests.csproj
```

The solution builds test-target fixtures in Release configuration for predictable generated IL, including during Debug builds. See the test guide for Mono and Harmony 3 preview compatibility runs.

## License

[MIT](LICENSE). The pinned Harmony test dependency includes its own license under `TestArtifacts/Harmony`.