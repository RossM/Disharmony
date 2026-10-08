# Disharmony

Disharmony is a C# modding framework built on Harmony. Use prefixes and postfixes to patch methods, or to change individual calls, member accesses, and constants inside a method.

## Install

Add Disharmony to the project containing your patches:

```shell
dotnet add package Disharmony --prerelease
```

The package includes `net472` and `net10.0` builds. Use `net472` for Mono or Microsoft .NET Framework, and `net10.0` for Microsoft .NET 10. NuGet selects the build for your project's target framework.

The `net472` build requires Harmony 2.4.1+; the `net10.0` build requires Harmony 2.4.2+. NuGet restores the appropriate dependency from the 2.x series. Harmony 3 support is incomplete and subject to change.

Start with [your first patch](https://github.com/RossM/Disharmony/blob/main/Disharmony/README.md#write-your-first-patch), then explore [usage examples](https://github.com/RossM/Disharmony/blob/main/docs/usage-examples.md).

## Patch diagnostics

The included analyzer checks patch declarations as you edit and build. It requires Roslyn 5.9 or newer; the .NET 10.0.400 SDK supplies a compatible compiler. Modern SDKs with older compilers skip the analyzer. This does not change which game runtimes your mod can target.

See the [analyzer guide](https://github.com/RossM/Disharmony/blob/main/Disharmony.Analyzers/README.md) for available diagnostics and limitations.

## Deploy a RimWorld mod

Use this NuGet package to compile your mod. For players, add a dependency on the official Disharmony mod, which supplies the runtime assemblies. Do not include `Disharmony.dll` or `0Harmony.dll` in your mod's `Assemblies` directory.

See [RimWorld dependency setup](https://github.com/RossM/Disharmony/blob/main/Disharmony.RimWorld/README.md#depend-on-disharmony) for the `About.xml` entries.

## License

Disharmony is licensed under the MIT license. Harmony has its own license.
