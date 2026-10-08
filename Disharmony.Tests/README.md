# Development and testing

The library, test project, and test-target fixtures multi-target .NET Framework 4.7.2 (`net472`) and .NET 10
(`net10.0`). Use the SDK selected by the repository's `global.json` (10.0.400 or a later patch in the same feature band). Running `net472` tests through `dotnet test` requires Windows and
.NET Framework 4.7.2 or later; the NUnitLite runner also supports Mono.

## Build and test

Run these commands from the repository root. Omit `--framework` to build or test both targets, or select one:

```powershell
dotnet build .\Disharmony\Disharmony.csproj
dotnet test .\Disharmony.Tests\Disharmony.Tests.csproj
dotnet test .\Disharmony.Tests\Disharmony.Tests.csproj --framework net472
dotnet test .\Disharmony.Tests\Disharmony.Tests.csproj --framework net10.0
```

Build outputs are separated by configuration and framework under `bin`. The .NET Framework output directory is
`net472`. Project references select the matching target automatically.
Visual Studio Test Explorer and `dotnet test` run the tests for each target through the NUnit test adapter.
These commands use the referenced Harmony 2.4.2 package. Use the compatibility runner below to test the same
Disharmony binaries across runtimes, or to exercise the Harmony 3 preview adapter. Harmony 3 runs are development experiments; see [compatibility](../docs/compatibility.md) for the supported release scope.

## Build the RimWorld integration

To build the official Disharmony mod from this repository, run:

```powershell
dotnet build .\Disharmony.RimWorld\Disharmony.RimWorld.csproj --configuration Release
```

The project targets RimWorld 1.6 and `net472`; `Krafs.Rimworld.Ref` supplies the compile-time game references. The installable output is `Disharmony.RimWorld/bin/Release/Mod`:

```text
About/
  About.xml
Assemblies/
  0Harmony.dll
  Disharmony.dll
  Disharmony.RimWorld.dll
```

Copy that folder into RimWorld's `Mods` directory to test it. All source files under `Disharmony.RimWorld/About/` are included with their subdirectories preserved. Each build replaces the generated mod folder; edit the source files rather than the output. For a mod that uses Disharmony, follow [the author setup and deployment guide](../Disharmony.RimWorld/README.md#depend-on-disharmony) instead.

## Hang detection

NUnit's [`Timeout`](https://docs.nunit.org/articles/nunit/writing-tests/attributes/timeout.html) attributes are enabled only for `net472`; NUnit rejects them on modern .NET because it cannot
abort a running thread. `CancelAfter` requires cooperative cancellation, which cannot interrupt the synchronous
optimizer and patching calls these tests exercise. To bound a hung .NET 10 test, run it through [VSTest's process-level
hang detection](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test-vstest) (the timeout resets after each completed test):

```powershell
dotnet test .\Disharmony.Tests\Disharmony.Tests.csproj --framework net10.0 --blame-hang --blame-hang-timeout 30s --blame-hang-dump-type none
```

The .NET 10 NUnitLite runner has no hard per-test timeout. Use the command above when investigating possible hangs.

## NUnitLite runners

The script builds only the target required by the selected runtime, then runs each selected Harmony version in a
fresh process:

* `Disharmony.Tests\Run-DisharmonyTests.ps1 -Runtime Clr` runs `net472` with Harmony v2 on the Microsoft .NET Framework CLR.
* `Disharmony.Tests\Run-DisharmonyTests.ps1 -Runtime Mono` runs `net472` with Harmony v2 on Mono (the default).
* `Disharmony.Tests\Run-DisharmonyTests.ps1 -Runtime Dotnet` runs `net10.0` with both Harmony v2 and v3 on .NET 10.

Harmony v2 comes from the project's pinned NuGet package. The merged .NET 10 Harmony v3 beta DLL is versioned at
`TestArtifacts\Harmony\3.0.0.0-preview.1\net10.0\0Harmony.dll` and used by default. Use `-HarmonyV3Path` to test a
different build. The bundled beta identifies itself as `3.0.0.0-preview.1`; its `UpdatePatchInfo` takes serialized bytes and
`UpdateWrapper` publishes the patch information itself. A .NET 10 DLL cannot be used for the CLR/Mono runs; v3 is
currently tested only on .NET 10.

Pass `-HarmonyVersion 2` or `-HarmonyVersion 3` to select one version. Both runs use the same test and Disharmony
assemblies, compiled against v2, with only `0Harmony.dll` replaced in the v3 run. Staged outputs are under
`Disharmony.Tests\bin\HarmonyCompatibility\<configuration>\<framework>\v<version>`. The test setup verifies the
loaded Harmony major version and prints its identity and location, so accidentally running v2 twice cannot pass
as a compatibility check. A failing run makes the script fail even if the other version succeeds.

Default NUnitLite result files are saved separately for each runtime and Harmony version under
`TestResults\Disharmony`, for example `test-result-dotnet-harmony3.xml`. A custom `--result` path requires selecting
one Harmony version to avoid overwriting another run's results.

Run the script from the repository root. For Mono, it checks `-MonoExecutable`, then `MONO_EXE`, then `mono` on
`PATH`, and finally the standard Windows installation directories. NUnitLite arguments can be supplied after the
script arguments, or explicitly through `-NUnitArguments`.

```powershell
.\Disharmony.Tests\Run-DisharmonyTests.ps1 -Runtime Clr
.\Disharmony.Tests\Run-DisharmonyTests.ps1 -Runtime Dotnet
.\Disharmony.Tests\Run-DisharmonyTests.ps1 -Runtime Dotnet -HarmonyVersion 2
.\Disharmony.Tests\Run-DisharmonyTests.ps1 -Runtime Dotnet -HarmonyVersion 3 -HarmonyV3Path C:\path\to\net10.0\0Harmony.dll
.\Disharmony.Tests\Run-DisharmonyTests.ps1 -Runtime Mono
.\Disharmony.Tests\Run-DisharmonyTests.ps1 -Runtime Mono --where 'test =~ Optimizer'
```

Pass `-Profile` to request a sampling profile under `TestResults\Disharmony`. `-MonoProfile` accepts another Mono
profiler specification and adds an output path under that directory when the specification does not provide one. The
script verifies that the requested file was created, because some Mono distributions do not include every profiler
module. In particular, the official Mono 6.12 x64 Windows package runs the tests but does not include the `log`
profiler used by these examples.

```powershell
.\Disharmony.Tests\Run-DisharmonyTests.ps1 -Runtime Mono -Configuration Release -Profile
.\Disharmony.Tests\Run-DisharmonyTests.ps1 -Runtime Mono -MonoProfile 'log:alloc,nocalls'
```

## Why the runtime matters

The Microsoft CLR and Mono do not reject all malformed generated IL in the same circumstances. A previous regression
left an unreachable `stloc` with an empty stack after an `AlwaysRun` postfix suppressed an exception and supplied a
result. The Microsoft CLR accepted the method, while Mono correctly rejected it as invalid IL. The defect is fixed, but
it demonstrates why a successful CLR run is not a substitute for running the end-to-end suite on Mono.

Use fresh processes for each runtime. Trampolines, resolved methods, and static state are process-wide and can otherwise
hide first-use behavior.

## Test environment and dependency caveats

Reflection-sensitive targets and all inline patch methods belong in `Disharmony.TestTargets`, which is always
compiled in Release mode so their generated IL is predictable.

At the start of each test run, the assembly-wide setup redirects Harmony's `FileLog` output to
`disharmony-tests.log` beside the built test executable and truncates the previous file. This captures both Harmony
logging and Disharmony logging written through `FileLog`. The exact path is printed to the test runner's progress
output. The writer permits concurrent readers, so the log can be inspected while a test run is still active.

Harmony coexistence tests use explicit, valid `Harmony.Patch` calls and a fixture-specific Harmony ID. Do not use an
invalid Harmony patch or a throwing Harmony transpiler merely to provoke an error: Harmony can emit invalid IL or retain
broken global patch state instead of rolling back, contaminating later tests.

Several boundary expectations reflect limitations below Disharmony rather than intended CLR restrictions. Constructed
generic methods are rejected because MonoMod 1.2.3 reaches an unimplemented `MethodTable.GetMethodDescForSlot` path on
the .NET Framework runtime. Varargs methods are rejected because Harmony generates invalid IL while resolving their
trampolines. The static-constructor test remains ignored because Harmony/MonoMod prepares the target in a way that runs
the type initializer before its patch can be installed.

Tests that inject failures through `HarmonyInterface.ApplyPatchHookForTesting` are compiled only in Debug builds,
matching the lifetime of that test hook. The normal Debug `dotnet test` run includes them; a Release test build does not.
