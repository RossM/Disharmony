# Install Disharmony for RimWorld

**Not yet published:** The Steam Workshop mod is not available yet.

Disharmony lets other mods change how RimWorld works. Install it when a mod lists Disharmony as a requirement. It does not change gameplay on its own.

This is a beta release. [Project on GitHub](https://github.com/RossM/Disharmony).

**Supported version:** RimWorld 1.6.

## Installation

1. Subscribe to Disharmony and the mods you want to use that require it.
2. Wait for Steam to finish downloading them, then launch RimWorld and open **Mods**.
3. Enable Disharmony and the mods that require it, and arrange them in the [load order below](#load-order).
4. Apply the changes and restart RimWorld when prompted.

## Load order

Place Disharmony after **Core** and any enabled official expansions, and before the mods that require it.

Disharmony includes Harmony, so you do not need a separate Harmony subscription just to use Disharmony. If another mod requires the Harmony mod, keep it enabled and load it before Disharmony.

## If RimWorld says Disharmony is missing

Subscribing downloads the mod; it does not enable it. Check that Disharmony is in your active mod list and comes before the mod that requires it. If Disharmony is absent from the list, check that Steam has finished downloading it.

## Getting help

If a problem starts after adding or updating another mod, ask that mod's author for help first. To [report a problem with Disharmony](https://github.com/RossM/Disharmony/issues), include your full game log, RimWorld version, and mod list, and describe what went wrong and when it happened.

## For mod authors

Disharmony lets you change individual calls, member accesses, and constants inside game methods with C# prefixes and postfixes. Its included analyzer catches patch mistakes in your editor and build. [Compare it with Harmony](../docs/harmony-comparison.md) to see how an inner patch can replace a transpiler.

[Write a RimWorld mod with Disharmony](../Disharmony.RimWorld/README.md) covers project setup, patch registration, and the Disharmony dependency. If you are new to making mods, the [author documentation](../docs/index.md) will help you choose where to start.
