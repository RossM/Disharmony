# Harmony v3 test dependency

`net10.0/0Harmony.dll` is the user-supplied merged Harmony beta used by the Disharmony compatibility tests.
Keep this binary in version control so a checkout contains the exact dependency needed to reproduce the v3 run.

- Assembly version: `3.0.0.0`
- Informational version: `3.0.0.0-preview.1`
- Target framework: `.NETCoreApp,Version=v10.0`
- SHA-256: `9603ADC2D383B4D1A3DC28777CF942EE0CA9A6875E05F21057697B341AF29463`
- Upstream project: [pardeike/Harmony](https://github.com/pardeike/Harmony)

The source commit for this supplied build is not recorded. The checksum identifies the tested binary.
The adjacent `LICENSE` contains Harmony's MIT license.

From the repository root, run both Harmony versions with:

```powershell
.\Disharmony.Tests\Run-DisharmonyTests.ps1 -Runtime Dotnet
```

This DLL is used only for the .NET 10 compatibility run. The Mono and CLR runs use the pinned Harmony v2 NuGet package.
