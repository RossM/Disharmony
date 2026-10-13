# Release validation

The [release checklist](../release-checklist.md) uses these checked-in fixtures to test the actual NuGet packages. No consumer code needs to be written during a release.

`NuGetConsumer` references the selected Disharmony package, without a project reference to the library or inherited repository build settings. It checks whole-method and inner patches, unaffected callers, lazy application, and unpatching. A wrong result or loaded version fails the process.

Run from the repository root in PowerShell 7.4 or later, with the repository's .NET SDK and Mono installed:

```powershell
./scripts/Test-NuGetPackage.ps1 -Version 1.0.0-beta.2 -PackageDirectory ./artifacts/nuget -OutputDirectory ./artifacts/package-validation
```

The runner uses a new output directory and package cache. It restores, builds, and runs four consumers: CLR/net472 with Harmony 2, Mono/net472 with Harmony 2, and CLR/net10.0 with each Harmony major. The Harmony 3 version comes from `Directory.Build.props`. Specify `-MonoExecutable PATH` if Mono is not on PATH or installed under `Program Files/Mono`.

For a local package, NuGet source mapping selects Disharmony exclusively from that package directory; dependencies come from NuGet.org. Add `-PublicFeed` after publication to restore Disharmony exclusively from NuGet.org, check its signature, and compare its contents with the publication artifact. The comparison ignores NuGet's added signature. Logs and consumer builds remain in the output directory.

This is a runtime/package smoke test, not a replacement for the library and analyzer test suites. The valid consumer patches also compile with warnings treated as errors. It does not intentionally generate analyzer diagnostics or exercise older compiler hosts on every release.

Recheck compiler-host selection, analyzer diagnostics in a consumer, SourceLink/PDB packaging, and the package's static file/dependency layout when changes affect those features. Such changes need their own validation before entering the release procedure; they are not recurring release chores. The [analyzer guide](../Disharmony.Analyzers/README.md) describes supported hosts.

`Test-ReleaseFiles.ps1` compares directory trees by relative file path and SHA-256 hash. The checklist uses it for the Workshop upload/subscribed installation and downloaded GitHub assets; an extra, missing, or changed file fails the check.

`Test-ReleaseCandidate.ps1` runs the library's CLR, Mono, and .NET 10 suites with their supported Harmony majors, plus the analyzer tests, in Debug and Release. It copies each runtime result into a configuration-specific directory before another run can overwrite it; analyzer results and a transcript are retained there too. It requires a new output directory and accepts `-MonoExecutable PATH`.

`Build-RimWorldRelease.ps1` runs from the tagged source worktree. It checks the requested version, builds the complete mod, checks its layout and bundled Harmony majors, and verifies that extracting the archive preserves every file. Its new output directory contains the mod ZIP, `mod-upload/`, `workshop-description.txt`, and a build transcript. It does not change the game's upload directory.
