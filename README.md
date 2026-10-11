# Disharmony

**Not yet published:** The NuGet package and Steam Workshop mod are not available yet.

**Simpler patches for game mod authors.**

Disharmony lets you replace many Harmony transpilers with ordinary C# prefixes and postfixes. Patch a call inside one game method while leaving calls from other methods unchanged. Built on Harmony, it also supports whole-method patches and inner patches on member accesses and constants.

Its bundled analyzer catches patch mistakes as you edit and compile, and its runtime checks explain problems when patches are applied. Disharmony can also defer patch generation to reduce startup work. Keep your existing Harmony patches and try Disharmony on one change.

[See how a Disharmony patch replaces a transpiler](https://rossm.github.io/Disharmony/harmony-comparison.html#worked-comparison-change-a-call-inside-a-method).

**Installing a RimWorld mod that requires Disharmony?** Follow the [installation and load-order instructions](https://rossm.github.io/Disharmony/guides/rimworld-players.html). Disharmony does not change gameplay on its own.

## Beta and compatibility

The initial beta supports **Mono/net472**, **Microsoft CLR/net472**, and **Microsoft CLR/net10.0** with Harmony 2. Harmony 3 support is present but experimental. APIs may change during the beta. See [compatibility](https://rossm.github.io/Disharmony/compatibility.html) to choose the library build and Harmony version for your game.

## Start writing mods

- **New to programming?** [Learn C# first](https://rossm.github.io/Disharmony/learning-csharp.html).
- **Know C# but new to patching?** [Learn how runtime patching works](https://rossm.github.io/Disharmony/runtime-patching.html).
- **Ready to try Disharmony?** [Write your first patch](https://rossm.github.io/Disharmony/guides/getting-started.html#write-your-first-patch) with complete examples you can build and run.
- **Already use Harmony?** [Add Disharmony to an existing mod](https://rossm.github.io/Disharmony/harmony-comparison.html#add-disharmony-to-an-existing-harmony-mod).
- **Writing a RimWorld mod?** [Set up, install, and test your mod](https://rossm.github.io/Disharmony/guides/rimworld.html).

## Examples and reference

[Usage examples](https://rossm.github.io/Disharmony/usage-examples.html) show how to make specific changes, including patching one call inside a method and sharing information between patches.

For `DISHARMONY` warnings and errors, use the [diagnostic reference](https://rossm.github.io/Disharmony/analyzer-diagnostics.html). [Analyzer setup and configuration](https://rossm.github.io/Disharmony/guides/analyzers.html) explains how the checks work in your editor and build.

Use the searchable [HTML documentation](https://rossm.github.io/Disharmony/) and [API reference](https://rossm.github.io/Disharmony/api/Disharmony.html) to look up attributes, parameter bindings, patch registration, and fluent configuration.

## Contributing and support

To work on Disharmony itself, start with [Development and testing](Disharmony.Tests/README.md). To improve the documentation, see [Editing and previewing the docs](docs/README.md).

[Report a problem or suggest an improvement](https://github.com/RossM/Disharmony/issues).

## License

[MIT](LICENSE).

## FAQ

### Q: Why did you make this?

A: It started out as a small utility to make it easier to write transpilers for my own mod, and grew from there.

### Q: Isn't this just a copy of Harmony 3?

A: I implemented infix first, back in [this commit](https://github.com/RossM/RimworldMods/commit/46b8564870c7956e7f0341cde2a660a6f8136916). I shared my progress on Discord and Brrainz was, let's say, "inspired" to add many of my features to Harmony.

### Q: Why should I use this over Harmony 3?

A: Disharmony has a nicer API, does more error checking, has better tooling, and is faster. In my opinion, of course.

### Q: Did you use generative AI?

A: Almost all of the tests and most of the documentation was written by generative AI. Very little of the Disharmony library itself is written by AI, mostly because every time I tried to have it write something I hated it and ended up tearing it out and rewriting it.

### Q: Have you even tested this?

A: There are over 2000 tests, with the goal of having full code coverage of non-error paths independently with both unit tests and end-to-end integration tests.

### Q: Has this been used in a real mod?

A: I've got a fairly substantial [RimWorld mod](https://github.com/RossM/RimworldMods/tree/main/XylXenos) in progress that has been entirely switched from Harmony to Disharmony. It should be released Soon(tm).
