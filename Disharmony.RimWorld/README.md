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

Compile your mod against the Disharmony library and let this dependency provide `Disharmony.dll` at runtime.
