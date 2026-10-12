# Compatibility

## Choose a library build

The initial beta supports these runtime and target-framework combinations:

| Runtime hosting your game or application | Disharmony target framework |
| --- | --- |
| Mono | `net472` |
| Microsoft .NET Framework CLR | `net472` |
| Microsoft .NET 10 CLR | `net10.0` |

Choose the build for the runtime that will load your code. For example, a mod loaded by a Mono game uses the `net472` build, even if you compile the mod with the .NET 10 SDK.

## Harmony version

The supported Harmony 2 versions are **2.4.1 or later** for `net472` and **2.4.2 or later** for `net10.0`. Harmony 2.4.1 does not support the .NET 10 runtime.

Harmony 3 support is incomplete and subject to change.

## RimWorld

The official Disharmony mod supplies the runtime libraries for RimWorld mods that depend on it. Players should follow [installation and load-order instructions](obj/guides/rimworld-players.md); mod authors should follow [project setup and deployment](obj/guides/rimworld.md).

## Existing Harmony mods

You can keep Harmony patches and add Disharmony patches in the same mod. [From Harmony to Disharmony](harmony-comparison.md#add-disharmony-to-an-existing-harmony-mod) shows shared metadata, separate registration, deployment, and the behavior differences to check before converting a patch.

## Experimental options

The IL optimizer is experimental and has no public enablement API in the initial beta. Review [PatchOptions](xref:Disharmony.PatchOptions) for the behavior and constraints of optional patch flags.
