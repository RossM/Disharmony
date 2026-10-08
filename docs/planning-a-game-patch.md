# Plan a patch for your game

The Disharmony examples give you a target method and explain the change to make. In a real game, you first need to find that method and work out whether changing it will have the effect you want. This guide describes how to approach that investigation and turn it into a mod.

You should be comfortable reading C# and understand how a patch selects a method and changes its behavior. If patching is new to you, read [Getting started with Disharmony](obj/guides/getting-started.md) first.

## Choose a small, testable change

Describe what currently happens, what you want to happen instead, and a situation in which you can test the difference. Start with one behavior rather than a large feature with several interacting parts.

Keep that test situation in mind while reading the code. A method with a promising name may be used for something else, or may run only in circumstances different from the ones you want to change.

## Find and understand the game code

Use a decompiler such as [ILSpy](https://github.com/icsharpcode/ILSpy) to open the game's assemblies: DLL files containing compiled code. The decompiler reconstructs C# you can read. You are using this code to understand the game; your patch will live in a separate mod.

Search for likely classes and methods, then follow their calls to find where the behavior comes from. For each promising method, ask:

- What inputs does it receive, and where do those values come from?
- Which conditions decide what happens?
- What does it return, and what data does it change?
- Which other methods call it, and what do they do with its result?

Follow the methods it calls when you cannot explain a step. You will encounter C# features and game-specific types that were not covered by your first tutorial; look them up as you need them. Understanding the relevant code can take longer than writing the patch itself.

## Learn from existing mods

Look for a source-code link on a mod's Workshop page, or search GitHub for its name. Choose a small mod that changes something similar. Its source can show both how an author approached the game code and how they set up a working mod project.

Find its patch methods and compare them with the game methods they change. Work out why the author chose those methods, which values the patches use, and how the changes produce the intended behavior. Also follow how the mod applies its patches when it loads.

Check which game version the mod was written for. Compare its assumptions with the code in your version before borrowing the approach.

## Choose where to patch

A method may be used by several features. Changing it everywhere may have effects beyond the situation you intended to change. Look at its callers before deciding to patch the whole method.

For example, a pricing method might be called both during checkout and when displaying a price. A postfix on that method affects both uses. If the change belongs only at checkout, an inner patch on the call inside the checkout method may be a better fit. [Getting started's inner-patch example](obj/guides/getting-started.md#patch-an-operation-inside-a-method) shows this distinction.

Choose the smallest change that achieves your goal while preserving the surrounding behavior. The [usage examples](usage-examples.md) show several ways to express a change once you know which code to target.

## Build, load, and test the mod

Follow your game's mod-project setup instructions, add the required references, and arrange for your patches to be applied when the mod loads. Verify that the game loads your mod and its dependencies. For RimWorld, the [Disharmony integration guide](obj/guides/rimworld.md) explains the Disharmony dependency and deployment setup.

Reproduce your test situation and check the intended result. Also try other situations that use the same code and check that any effects there are intended. Read the game's logs when something fails.

If the patch has no effect or changes something unexpected, return to the game code and check your assumptions about when it runs and what it does. Expect to move back and forth between reading code, changing your patch, and testing as you learn more about the game.
