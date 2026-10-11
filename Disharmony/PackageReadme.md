# Disharmony

Disharmony is a C# modding framework built on Harmony. You can use prefixes and postfixes on whole methods, or put them around individual method calls inside a specific method. It works on field accesses and constants too.

This is a beta release; APIs may change.

## Install

Add Disharmony to the project containing your patches:

```shell
dotnet add package Disharmony --prerelease
```

The package includes `net472` and `net10.0` builds. Use `net472` for Mono or Microsoft .NET Framework, and `net10.0` for Microsoft .NET 10. NuGet selects the build for your project's target framework.

The `net472` build requires Harmony 2.4.1+; the `net10.0` build requires Harmony 2.4.2+. NuGet restores the appropriate dependency from the 2.x series. Harmony 3 support is incomplete and subject to change. See [Compatibility](https://rossm.github.io/Disharmony/compatibility.html) for supported runtimes and Harmony versions.

Start with [Write your first patch](https://rossm.github.io/Disharmony/guides/getting-started.html#write-your-first-patch), then explore [Usage examples](https://rossm.github.io/Disharmony/usage-examples.html) or look up the [API reference](https://rossm.github.io/Disharmony/api/Disharmony.html).

## Patch diagnostics

The included analyzer checks your patches and points out possible mistakes while you edit or build. There is nothing else to install or enable.

It requires Roslyn 5.9 or newer; the .NET 10.0.400 SDK supplies a compatible compiler. Modern SDKs with older compilers skip the analyzer. This does not change which game runtimes your mod can target.

Look up a `DISHARMONY` warning or error in the [Diagnostic reference](https://rossm.github.io/Disharmony/analyzer-diagnostics.html). See [Analyzer setup and configuration](https://rossm.github.io/Disharmony/guides/analyzers.html) for compiler and editor requirements, severity settings, and limitations.

## Deploy a RimWorld mod

Use this NuGet package to compile your mod. When distributing it, declare a dependency on the [official Disharmony mod](https://steamcommunity.com/sharedfiles/filedetails/?id=3818084845), which supplies the runtime assemblies for players. Do not include `Disharmony.dll` or `0Harmony.dll` in your mod's `Assemblies` directory.

See [RimWorld dependency setup](https://rossm.github.io/Disharmony/guides/rimworld.html#depend-on-disharmony) for the `About.xml` entries, then [Build and install](https://rossm.github.io/Disharmony/guides/rimworld.html#build-and-install) to deploy and test your mod.

## License

Disharmony and Harmony are both licensed under the MIT license.
