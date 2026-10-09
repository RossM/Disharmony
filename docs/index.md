# Disharmony

**Not yet published:** The NuGet package and Steam Workshop mod are not available yet.

Disharmony is a C# framework for changing game behavior with mods. Built on Harmony, it lets you patch whole methods or individual calls, member accesses, and constants inside them.

The initial release is a beta. See [compatibility](compatibility.md) for supported runtimes and Harmony versions.

**Quick lookup:** [API reference](xref:Disharmony) · [Usage examples](usage-examples.md) · [Diagnostic reference](analyzer-diagnostics.md).

## Playing RimWorld?

If a mod requires Disharmony, follow the [installation instructions](obj/guides/rimworld-players.md) to subscribe, enable it, and set the load order. Disharmony does not change gameplay on its own.

## Learn to write mods

- **New to programming?** [Learn C# first](learning-csharp.md), with a recommended tutorial and the skills you need before starting the patching guides.
- **Know C# but new to patching?** [How runtime patching works](runtime-patching.md) explains how a separate mod changes game behavior without replacing the game's assemblies.
- **Ready to write a patch?** [Getting started](obj/guides/getting-started.md) takes you through complete examples you can build and run. For RimWorld, continue with [Write a mod with Disharmony](obj/guides/rimworld.md) to apply a patch in-game.

After the introductory examples, [Plan a patch for your game](planning-a-game-patch.md) explains how to investigate game code, learn from existing mods, and choose where to make your change.

## Already use Harmony?

[From Harmony to Disharmony](harmony-comparison.md) compares the features and APIs, shows an alternative to a transpiler, and explains how to add Disharmony while keeping your existing Harmony patches.

## Find examples and reference

- [Usage examples](usage-examples.md): choose an example for the change you want to make, such as modifying one call inside a method or sharing information between patches.
- [API reference](xref:Disharmony): look up attributes, types, method signatures, and parameter meanings. Go directly to [patch registration](xref:Disharmony.Patcher) or [fluent configuration](xref:Disharmony.Patch).
- [Diagnostic reference](analyzer-diagnostics.md): look up a `DISHARMONY` warning or error to understand the problem and how to fix it. [Analyzer setup and configuration](obj/guides/analyzers.md) explains how to enable or configure these checks.

If an extension property appears as a `get_...` method in the API reference, see [Reading extension members](extension-members.md).

## Contribute to Disharmony

[Development and testing](obj/guides/testing.md) explains how to build Disharmony, work on the analyzer or RimWorld integration, and run tests for your changes.
