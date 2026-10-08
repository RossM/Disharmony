# How runtime patching works

This guide is for readers who know C# but are new to changing a game's behavior with patches. You should be comfortable reading methods and classes; if you are new to programming, [learn C# first](learning-csharp.md).

## Game code and mod code

A C# game's code is compiled into **assemblies**, often DLL files. Your mod is a separate C# project that compiles into its own assembly. Referencing the game's assemblies lets your mod use the game's classes and methods.

Disharmony lets you write a **patch**: a method in your mod that changes how a selected game method runs. It uses Harmony to connect your patch to the game method while the game is running. The game's DLL files on disk stay unchanged.

The mod's patch code and the game's original method work together in the same process. A patch can change the arguments before the game method runs, change its result afterward, or replace its behavior. This page starts with changing a result.

## Start with a target method

Suppose a game charges 10 per item. This is the **hypothetical game source**, included so you can see the original behavior. In a real mod, this class would already belong to the game; your mod would reference it.

```csharp
public class PriceCalculator
{
    public float GetPrice(int quantity)
    {
        return 10f * quantity;
    }
}
```

Calling `GetPrice(2)` on a `PriceCalculator` returns `20`. We want to give a 10% discount, so that the same call returns `18`.

`GetPrice` is the **target method**: the game method whose behavior we want to change. The patch below will affect calls to that method on any `PriceCalculator` instance, wherever the game uses it.

## Change the result with a postfix

A **postfix** is a patch that runs after the target method returns. It can read the result and, if needed, change the value that the caller receives.

This code belongs in your **mod project**:

```csharp
using Disharmony;

[Patch(typeof(PriceCalculator))]
public static class PricePatches
{
    [Postfix]
    [Target(nameof(PriceCalculator.GetPrice), typeof(int))]
    public static void ApplyDiscount([ReturnValue] ref float result)
    {
        result *= 0.9f;
    }
}
```

The attributes in square brackets describe how Disharmony should use the patch:

- `[Patch(typeof(PriceCalculator))]` identifies the game class that this patch class will target.
- `[Target(nameof(PriceCalculator.GetPrice), typeof(int))]` selects `GetPrice` with one `int` parameter. `nameof` supplies the method's name; the parameter type distinguishes it from other overloads.
- `[Postfix]` makes `ApplyDiscount` run after the selected method returns.
- `[ReturnValue]` tells Disharmony to supply the target method's result as `result`. The C# `ref` modifier lets the patch change that value for the caller.

Patch methods must be static. Disharmony calls `ApplyDiscount` and supplies its arguments when the target runs; you write what the patch should do with those values.

The game method still calculates the price. For a quantity of 2 it produces `20`, then `ApplyDiscount` changes that value to `18` before it reaches the caller. The game's callers continue calling `GetPrice` as usual.

## Register the patch when your mod loads

To activate these patches, put this call in your mod's initialization code:

```csharp
Patcher.PatchAll(typeof(PricePatches));
```

This is **registration**: Disharmony reads the patch definitions in `PricePatches` and applies them to their targets. The attributes alone describe the patches; this call makes them active.

Register once when your mod loads, before the game uses the methods you want to change. The exact initialization hook depends on the game's mod system. Registration applies to the current game process, so your mod registers its patches each time the game starts.

After registration, calls to `GetPrice(2)` return `18` in this example. Other mods can patch the same method too, so their changes can also affect the final result.

## Change an input with a prefix

A **prefix** is a patch that runs before the target method. For example, we can change negative quantities to zero before the game calculates a price.

Add this method inside the mod's `PricePatches` class, alongside `ApplyDiscount`:

```csharp
[Prefix]
[Target(nameof(PriceCalculator.GetPrice), typeof(int))]
public static void ClampQuantity(ref int quantity)
{
    if (quantity < 0)
        quantity = 0;
}
```

Disharmony matches the parameter named `quantity` to the target's argument with that name. Declaring it with `ref` lets the prefix change the value passed to the target, even though `GetPrice` itself takes an ordinary `int` parameter.

Rebuild your mod after adding the prefix. When the game next loads it, the same registration call applies both patches. A call then follows these steps:

1. `ClampQuantity` runs and changes a negative quantity to zero.
2. The game's `GetPrice` method calculates a price using that quantity.
3. `ApplyDiscount` reduces the price by 10%, and the caller receives the result.

| Call | Original game result | Result with both patches |
| --- | --- | --- |
| `GetPrice(2)` | `20` | `18` |
| `GetPrice(-1)` | `-10` | `0` |

These patches change the behavior of the whole method. Disharmony can also patch individual calls, field or property accesses, and constants inside a method; [Getting started](obj/guides/getting-started.md#patch-an-operation-inside-a-method) introduces those inner patches.

## Continue with a first patch

[Getting started with Disharmony](obj/guides/getting-started.md) covers adding Disharmony to your project and using its patching APIs. Check [compatibility](compatibility.md) for the build and Harmony version your game needs.

Once you have followed the examples, [Plan a patch for your game](planning-a-game-patch.md) explains how to investigate the game's code, learn from existing mods, and decide where your change belongs.
