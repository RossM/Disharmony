# Disharmony for RimWorld

This project builds Disharmony as a library mod for RimWorld 1.6. It targets .NET Framework 4.7.2 and uses `Krafs.Rimworld.Ref` to compile without a local game installation.

## Build the mod

From the repository root:

```powershell
dotnet build .\Disharmony.RimWorld\Disharmony.RimWorld.csproj --configuration Release
```

The installable mod folder is `Disharmony.RimWorld/bin/Release/Mod`:

```text
About/
  About.xml
Assemblies/
  0Harmony.dll
  Disharmony.dll
  Disharmony.RimWorld.dll
```

Copy that folder into RimWorld's `Mods` directory. All files added under the project's `About/` directory are included, with their subdirectories preserved. Each build replaces the generated mod folder; edit the project's source files rather than the output.

## Depend on Disharmony

Add this dependency to your mod's `About/About.xml`, combining it with any existing dependencies and load-order entries:

```xml
<modDependencies>
  <li>
    <packageId>Xylthixlm.Disharmony</packageId>
    <displayName>Disharmony</displayName>
  </li>
</modDependencies>
<loadAfter>
  <li>Xylthixlm.Disharmony</li>
</loadAfter>
```

Compile your mod against the Disharmony NuGet package and let this dependency supply the runtime assemblies.
The NuGet package includes the library and patch analyzer; the RimWorld integration is distributed as this mod.
Do not ship `Disharmony.dll`, `Disharmony.RimWorld.dll`, or `0Harmony.dll` in your mod's `Assemblies` directory.

NuGet normally copies runtime dependencies into your build output. To keep Disharmony and Harmony available for
compilation without copying their DLLs, place this after your existing Disharmony package reference:

```xml
<ItemGroup>
  <PackageReference Update="Disharmony" ExcludeAssets="runtime" />
  <PackageReference Include="Lib.Harmony" Version="[2.4.1,3.0.0-0)" ExcludeAssets="runtime" />
</ItemGroup>
```

If your project already references `Lib.Harmony`, add `ExcludeAssets="runtime"` to that reference instead of adding
a second one. The explicit Harmony reference applies the same exclusion to Disharmony's dependency. The bundled
Disharmony analyzer remains available.
