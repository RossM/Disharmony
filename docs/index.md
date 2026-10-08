# Disharmony

Disharmony is a C# game-modding framework built on Harmony. Write prefixes and postfixes around whole methods, or around individual calls, member accesses, and constants inside them.

## Start here

Check [compatibility](compatibility.md) to choose the library build for your game or application. Then follow [Getting started](obj/guides/getting-started.md) to write and register your first patch.

Explore [usage examples](usage-examples.md) for inner patches, state, private-method delegates, exception recovery, and compiler-generated targets.

The bundled [analyzers](obj/guides/analyzers.md) help you find mistakes in patch definitions and parameter bindings while compiling your project.

## API reference

Use the reference when you need the available overloads, parameter meanings, or constraints for a particular API:

- [Patch registration](xref:Disharmony.Patcher): apply and remove patches, or generate them before their first use.
- [Fluent configuration](xref:Disharmony.Patch): select targets and construct patches in code.
- [Patch configuration](xref:Disharmony.PatchConfig): inspect the configuration produced by the fluent API.
- [Attributes and types](xref:Disharmony): look up target selectors, parameter bindings, execution options, and exceptions.
- [IL rewrite rules](xref:Disharmony.RulesEngine): work directly with the lower-level rules engine.

Some extension properties appear as accessor methods in the reference. [Reading extension members](extension-members.md) shows how to use those entries in C#.

## Contributing

[Development and testing](obj/guides/testing.md) explains how to build Disharmony and run its tests.
