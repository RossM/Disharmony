# Disharmony

**Simpler patches for game mod authors.**

Disharmony lets you replace many Harmony transpilers with ordinary C# prefixes and postfixes. Patch a call inside one game method while leaving calls from other methods unchanged. Built on Harmony, it also supports whole-method patches and inner patches on member accesses and constants.

Its bundled analyzer catches patch mistakes as you edit and compile, and its runtime checks explain problems when patches are applied. Keep your existing Harmony patches and try Disharmony on one change. [See a Disharmony patch alongside the equivalent transpiler](harmony-comparison.md#worked-comparison-change-a-call-inside-a-method).

The initial release is a beta. See [compatibility](compatibility.md) for supported runtimes and Harmony versions.

**Quick lookup:** [API reference](xref:Disharmony) · [Usage examples](usage-examples.md) · [Diagnostic reference](analyzer-diagnostics.md).

## Playing RimWorld?

If a mod requires Disharmony, install it from [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3818084845), then follow the [installation instructions](obj/guides/rimworld-players.md) to subscribe, enable it, and set the load order. Disharmony does not change gameplay on its own.

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
- [Diagnostic reference](analyzer-diagnostics.md): look up a `DISHARMONY` warning or error to understand the problem and how to fix it. [Analyzer setup and configuration](obj/guides/analyzers.md) covers compiler requirements and diagnostic settings.

## Contribute to Disharmony

[Development and testing](obj/guides/testing.md) explains how to build Disharmony, work on the analyzer or RimWorld integration, and run tests for your changes.

For documentation changes, see [Editing and previewing the docs](https://github.com/RossM/Disharmony/blob/main/docs/README.md).

[View the source on GitHub](https://github.com/RossM/Disharmony) or [report a problem or suggest an improvement](https://github.com/RossM/Disharmony/issues).
