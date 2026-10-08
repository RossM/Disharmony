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

Use Harmony 2. Disharmony is currently built and tested against **Harmony 2.4.2**; other Harmony 2 versions have not all been validated.

Harmony 3 is not supported by the initial beta because it is still in beta and its interfaces may change.

## Experimental options

The IL optimizer is experimental and has no public enablement API in the initial beta. Review [PatchOptions](xref:Disharmony.PatchOptions) for the behavior and constraints of optional patch flags.
