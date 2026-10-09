# Disharmony

Disharmony is a C# framework for changing game behavior with mods. Built on Harmony, it lets you patch whole methods or individual calls, member accesses, and constants inside them.

## Why use Disharmony?

- **Change an operation inside a method.** Write a C# prefix or postfix for a selected call, member access, or constant without writing a transpiler.
- **Catch mistakes before launching the game.** The bundled analyzer reports problems with your patches as you edit and compile.
- **Get useful patching errors.** Disharmony checks targets and parameter bindings, and explains why a patch is invalid.
- **Reduce startup work.** Disharmony can defer patch generation to reduce the work done during mod startup.

You can keep existing Harmony patches and add Disharmony where it helps. [From Harmony to Disharmony](docs/harmony-comparison.md) compares the features and APIs and shows how to adopt it in an existing mod.

## Beta and compatibility

The initial beta supports **Mono/net472**, **Microsoft CLR/net472**, and **Microsoft CLR/net10.0** with Harmony 2. APIs may change during the beta. See [compatibility](docs/compatibility.md) to choose the library build and Harmony version for your game.

## Start writing mods

- **New to programming?** [Learn C# first](docs/learning-csharp.md).
- **Know C# but new to patching?** [Learn how runtime patching works](docs/runtime-patching.md).
- **Ready to try Disharmony?** [Write your first patch](Disharmony/README.md#write-your-first-patch) with complete examples you can build and run.
- **Already use Harmony?** [Add Disharmony to an existing mod](docs/harmony-comparison.md#add-disharmony-to-an-existing-harmony-mod).
- **Writing a RimWorld mod?** [Set up, install, and test your mod](Disharmony.RimWorld/README.md).

## Examples and reference

[Usage examples](docs/usage-examples.md) show how to make specific changes, including patching one call inside a method and sharing information between patches.

For `DISHARMONY` warnings and errors, use the [diagnostic reference](docs/analyzer-diagnostics.md). [Analyzer setup and configuration](Disharmony.Analyzers/README.md) explains how the checks work in your editor and build.

API comments in the source describe [attributes and parameter bindings](Disharmony/Attributes.cs), [patch registration](Disharmony/Patcher.cs), and [fluent configuration](Disharmony/Patch.cs).

## Playing RimWorld?

If a mod requires Disharmony, install and enable the official Disharmony mod. Follow the [installation and load-order instructions](Disharmony.RimWorld/About/README.md). Disharmony does not change gameplay on its own.

## Contributing and support

To work on Disharmony itself, start with [Development and testing](Disharmony.Tests/README.md). To improve the documentation, see [Editing and previewing the docs](docs/README.md).

[Report a problem or suggest an improvement](https://github.com/RossM/Disharmony/issues).

## License

[MIT](LICENSE).
