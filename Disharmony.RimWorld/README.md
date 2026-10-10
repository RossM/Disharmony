# Write a RimWorld mod with Disharmony

**Not yet published:** The NuGet package and official Workshop mod are not available yet. The setup below uses the planned first beta version.

This guide takes you from a C# project to a patch you can see in RimWorld 1.6: selecting a pawn will show an extra line in its information panel. It assumes you know basic C# and have worked through [your first Disharmony patch](../Disharmony/README.md#write-your-first-patch), or already understand patch registration and postfixes.

You will build your own mod against the Disharmony NuGet package. Players install the official Disharmony mod separately; it supplies Disharmony and Harmony when the game runs.

## Before you start

You need RimWorld 1.6, the official Disharmony mod installed, a C# editor, and the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0), version 10.0.400 or newer. The SDK compiles the project; the mod itself targets **.NET Framework 4.7.2** (`net472`), which matches RimWorld 1.6. It is a class library loaded by the game, rather than a console program you run with `dotnet run`.

The commands below use PowerShell. Keep the source project outside the game's installation directory and copy the finished mod into the game when you are ready to test.

## Create the project

In a terminal, open the directory where you keep your projects and run:

```powershell
mkdir DisharmonyDemo
cd DisharmonyDemo
dotnet new classlib --name DisharmonyDemo --output Source
mkdir About
```

Open `Source/DisharmonyDemo.csproj` in your editor and replace its contents with:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net472</TargetFramework>
    <LangVersion>14.0</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Krafs.Rimworld.Ref" Version="1.6.4871" PrivateAssets="all" />
    <PackageReference Include="Disharmony" Version="1.0.0-beta.1" ExcludeAssets="runtime" />
    <PackageReference Include="Lib.Harmony" Version="[2.4.1,3.0.0-0)" ExcludeAssets="runtime" />
  </ItemGroup>
</Project>
```

Delete the generated `Source/Class1.cs`.

[Krafs.Rimworld.Ref](https://github.com/krafs/RimRef) supplies reference assemblies: they expose RimWorld's types and method signatures to the compiler, but contain no executable game code. Your installed game supplies that code at runtime. To understand a game method's behavior, open the installed game's `RimWorldWin64_Data/Managed/Assembly-CSharp.dll` in [ILSpy](https://github.com/icsharpcode/ILSpy); the reference package cannot show you its implementation.

The two `ExcludeAssets="runtime"` settings keep Disharmony and Harmony available to the compiler without copying their DLLs into your mod. The official Disharmony mod supplies them instead. The Disharmony analyzer is still included and can report mistakes in patch declarations as you build.

## Depend on Disharmony

Create `About/About.xml` with:

```xml
<?xml version="1.0" encoding="utf-8"?>
<ModMetaData>
  <name>Disharmony Demo</name>
  <author>Your name</author>
  <packageId>YourName.DisharmonyDemo</packageId>
  <description>Adds a demonstration line to the pawn information panel.</description>
  <supportedVersions>
    <li>1.6</li>
  </supportedVersions>
  <modDependencies>
    <li>
      <packageId>Xylthixlm.Disharmony</packageId>
      <displayName>Disharmony</displayName>
    </li>
  </modDependencies>
  <loadAfter>
    <li>Xylthixlm.Disharmony</li>
  </loadAfter>
</ModMetaData>
```

`packageId` identifies your mod independently of its display name. Replace `YourName.DisharmonyDemo` with your own unique identifier when making a mod to distribute. Keep the dependency's ID, `Xylthixlm.Disharmony`, as written.

`modDependencies` tells RimWorld that Disharmony is required. `loadAfter` tells it that your mod belongs later in the load order, so Disharmony is available before your mod initializes. Your mod does not need a separate dependency on the Harmony mod.

## Add the patch

Create `Source/PawnInspectPatches.cs` with this **mod source**:

```csharp
using Disharmony;
using Verse;

namespace DisharmonyDemo;

[Patch(typeof(Pawn))]
public static class PawnInspectPatches
{
    [Postfix]
    [Target(nameof(Pawn.GetInspectString))]
    public static void AddDemoLine([ReturnValue] ref string result)
    {
        result += "\nDisharmony demo is active.";
    }
}
```

The game class `Verse.Pawn` represents a colonist, animal, or other pawn. Its `GetInspectString()` method returns text used in the information panel when a pawn is selected. This postfix appends a line after the method returns, preserving the game's existing information. `Pawn` comes from the game reference; do not create a copy of that class in your project.

## Register when RimWorld loads your mod

Create `Source/DisharmonyDemoMod.cs` with:

```csharp
using Disharmony;
using Verse;

namespace DisharmonyDemo;

public sealed class DisharmonyDemoMod : Mod
{
    public DisharmonyDemoMod(ModContentPack content) : base(content)
    {
        Patcher.PatchAll(typeof(DisharmonyDemoMod).Assembly);
        Log.Message("Disharmony Demo: patches registered.");
    }
}
```

RimWorld constructs your `Mod` subclass when it loads the enabled mod. That constructor is where this example registers its patches, once per game launch. The assembly overload of `PatchAll` finds classes marked `[Patch]` in your mod's DLL, including `PawnInspectPatches`.

If you are adding Disharmony to an existing mod, put the registration call in its existing `Mod` constructor rather than adding another entry-point class. Do not register from a method that runs repeatedly, such as an update or drawing method. Patches that need loaded definitions or other game state may need a later initialization hook; this example only needs the `Pawn` method to exist.

## Build and install

From the `DisharmonyDemo` directory, run:

```powershell
dotnet build .\Source\DisharmonyDemo.csproj --configuration Release --output .\Assemblies
```

The first build restores the NuGet packages. Fix any build errors before installing the mod. The DLL is `Assemblies/DisharmonyDemo.dll`.

Create a `DisharmonyDemo` folder inside RimWorld's `Mods` directory. Copy your `About` directory and `Assemblies/DisharmonyDemo.dll` into it, giving you:

```text
Mods/
  DisharmonyDemo/
    About/
      About.xml
    Assemblies/
      DisharmonyDemo.dll
```

A debug-symbol file (`DisharmonyDemo.pdb`) may also be copied if you want it for debugging. Do not copy the source project, the game reference assemblies, or `Disharmony.dll`, `Disharmony.RimWorld.dll`, or `0Harmony.dll`. The official Disharmony mod supplies those last three assemblies. Check for leftover DLLs if you are reusing an existing mod folder.

## Verify in-game

1. Launch RimWorld 1.6 and open **Mods**. Enable Disharmony and Disharmony Demo. Keep Core and any enabled official expansions before them, and Disharmony before Disharmony Demo.
2. Apply the mod-list changes and restart the game when prompted.
3. Load a test colony or start a new one, then select a colonist. The pawn information panel should include **Disharmony demo is active.** after its normal text.
4. Enable **Development mode** in the game's options and open the debug log. Look for `Disharmony Demo: patches registered.` and check for errors from your mod or Disharmony.

The registration message confirms that RimWorld loaded your mod and ran its constructor. The extra line confirms that the patch actually changed the game method's result. Check both; successful compilation or registration alone does not prove the patch works.

After changing the code, close RimWorld, rebuild, replace the installed DLL, and relaunch. A running game continues to use the DLL it already loaded.

### If the line does not appear

| What you observe | What to check |
| --- | --- |
| Disharmony Demo is absent from the Mods list | `About/About.xml` must be directly inside `Mods/DisharmonyDemo`, with no extra enclosing folder. |
| A missing-dependency warning | Install and enable the official Disharmony mod; place it before Disharmony Demo. |
| No registration message | Check that the demo is enabled, that `Assemblies/DisharmonyDemo.dll` is installed, and that the startup log has no assembly-loading errors. |
| Registration message but no added line | Select a colonist, confirm you copied the latest build and restarted, and look for patching errors in the log. Test with only Core, Disharmony, and the demo to rule out another mod changing the same panel. |

## Use this setup for your own mod

Keep the project references, dependency metadata, and registration location; replace the demonstration patch with the behavior you want to change. If you already have Disharmony or Harmony package references, update those entries with the runtime exclusions rather than adding duplicates.

For your next patch, follow [Plan a patch for your game](../docs/planning-a-game-patch.md): investigate the relevant game code and existing mods, choose a target, and test the resulting behavior. [Usage examples](../docs/usage-examples.md) show more ways to select and change operations; [compatibility](../docs/compatibility.md) lists the supported library builds and Harmony versions.

To change or build the official Disharmony mod itself, see the [contributor build instructions](../Disharmony.Tests/README.md#build-the-rimworld-integration).
