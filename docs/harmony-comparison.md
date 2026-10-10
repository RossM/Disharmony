# From Harmony to Disharmony

If you already write Harmony patches, this guide explains what Disharmony offers, where the APIs differ, and how to add it to an existing mod. You can keep your Harmony patches and adopt Disharmony for individual changes.

The comparison below uses **Harmony 2.4.x**, the series supported by Disharmony's initial beta. See [runtime and dependency compatibility](compatibility.md) for the supported library builds and required Harmony versions.

To add Disharmony to an existing mod, review [compatibility and conversion](#compatibility-and-conversion), then follow [the adoption example](#add-disharmony-to-an-existing-harmony-mod).

## Why use Disharmony?

### Change an operation without writing a transpiler

An inner patch changes a selected operation inside a method. For example, you can adjust a price-calculation call during checkout while leaving other callers unchanged.

Write the change as a C# prefix or postfix. Disharmony handles finding the operation and inserting your patch. The [worked comparison](#worked-comparison-change-a-call-inside-a-method) shows the equivalent Harmony transpiler.

### Get useful errors when applying patches

Disharmony does more error checking than Harmony. It rejects invalid bindings with an explanation of the faulty patch. For example:

- A `ref int` parameter cannot replace a `float` result.
- Two patches cannot share a state key if one uses it as a `float` and the other as a `string`.

It also reports when an inner operation cannot be found. If a game update removes the call you intended to patch, you get an error instead of silently leaving the behavior unchanged. You can explicitly allow a missing operation for an optional patch.

### Catch mistakes in your IDE

The bundled Roslyn analyzer reports mistakes as you edit and compile, before you launch the game. For example:

- A patch method is missing `static`.
- A postfix returns a value instead of `void`.

See [analyzer setup and diagnostics](obj/guides/analyzers.md) for compatible IDE/compiler versions and the full list of checks. Checks that depend on the actual game methods still happen when your mod runs.

### Spend less startup time generating patches

Disharmony patches take effect immediately, but generation of the patched method bodies can be deferred. This reduces the work that must finish during mod initialization.

The RimWorld integration completes generation in the background. If a method is called before its patched body is ready, that first call finishes the work and can take longer.

For other games, [Patcher](xref:Disharmony.Patcher) explains how to complete deferred work with `ForceApply`.

## API differences that affect your patches

Harmony already provides attributes, argument aliases, delegate injection, and fluent configuration. Disharmony offers a different way to express and combine those requests, as well as its inner-patching model.

| Task | Harmony 2 | Disharmony |
| --- | --- | --- |
| Declare what a patch does | `[HarmonyPatch]` supplies discovery and target metadata. Method names or attributes identify prefixes and postfixes. | `[Patch]` marks the container/default type, `[Target]` selects the member, and `[Prefix]` or `[Postfix]` selects when to run. Add `[Inner]` to select an operation within it. |
| Bind values to patch parameters | Special names such as `__instance`, `__result`, and `___field`; `[HarmonyArgument]` can map arguments to other names. | Explicit bindings such as `[Instance] Checkout checkout` or `[ReturnValue] ref float price` let you name each value by its meaning. Familiar Harmony names also work. |
| Use caller values when changing an inner call | A transpiler explicitly supplies the needed values to the inserted code. | `Scope.Outer` and `Scope.Inner` distinguish the caller's object/arguments from those of the selected inner operation. |
| Select several methods | `TargetMethods()` returns the targets to patch. | Repeat `[Target]` for individual members, or use `[Targets]` for all matches. Selectors also cover local functions and lambdas. |
| Select a by-reference overload | Supply argument types and a separate `ArgumentType[]` for reference/out variations. | Put `typeof(Ref<int>)`, `typeof(In<int>)`, or `typeof(Out<int>)` directly in the parameter-type list. The markers describe the target signature; patch parameters still use ordinary C# `ref`, `in`, or `out`. |
| Share temporary values between patches | `__state` connects a patch class's prefix/postfix; a composite object can carry several values. | Named `[State]` keys connect related whole-method and inner patches. See [sharing boundaries and examples](usage-examples.md#shared-state-and-constants). |
| Call an inaccessible method or base implementation | Annotated delegate types, `AccessTools` helpers, and reverse patches provide related facilities. | `[Method]` selects a callable delegate from the patch parameter's signature; `[BaseMethod]` supplies the nearest base implementation. See [private-method delegates](usage-examples.md#private-method-delegates) and [base-method delegates](usage-examples.md#base-method-delegates). |
| Configure and later remove patches | Configure `HarmonyMethod` values or a fluent `PatchProcessor`; remove patches by owner ID or patch method. | Build immutable `PatchConfig` values, apply them with `Patcher`, and retain a `PatchHandle` to remove precisely that registration group. |
| Handle an exception | A separate finalizer patch kind. | An `AlwaysRun` postfix can inspect or change the exception. See [exception recovery](usage-examples.md#alwaysrun-postfixes). |

State in both frameworks lasts for the current invocation; it is not storage between calls. Disharmony's named state also lets inner and whole-method patches exchange values, within the documented registration and patch-class boundaries.

## Worked comparison: change a call inside a method

Inner targeting and scoped bindings work together when a change should apply only to one caller. Consider a checkout that should discount members' item prices by 10% during a sale.

The service fee must stay unchanged. Estimates and direct calls to the price calculator must also keep their original behavior. Both implementations below make that same change.

[Try the comparison yourself](#try-the-comparison-yourself) to see the effect of each patch.

### The game code

Both implementations patch this **hypothetical game source**:

```csharp
using System.Runtime.CompilerServices;

public sealed class Customer
{
    public bool IsMember { get; set; }
}

public sealed class PriceCalculator
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public float GetPrice(Customer buyer, int quantity)
    {
        return 10f * quantity;
    }
}

public sealed class Checkout
{
    public bool SaleEnabled { get; set; }
    public float ServiceFee { get; set; } = 2f;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public float Total(PriceCalculator calculator, Customer buyer, int quantity)
    {
        return calculator.GetPrice(buyer, quantity) + ServiceFee;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public float Estimate(PriceCalculator calculator, Customer buyer, int quantity)
    {
        return calculator.GetPrice(buyer, quantity) + ServiceFee;
    }
}
```

The `NoInlining` attributes make this runnable comparison reliable: the runtime cannot copy these methods into callers before they are patched. In a real game you do not control the target source; consider inlining when choosing a patch location and register before the game uses it. See [planning a game patch](planning-a-game-patch.md).

### Disharmony: a postfix on the inner call

The Disharmony **mod source** selects the call and changes its result:

```csharp
using Disharmony;

[Patch(typeof(Checkout))]
public static class DisharmonySalePatches
{
    [Postfix]
    [Target(nameof(Checkout.Total),
        typeof(PriceCalculator), typeof(Customer), typeof(int))]
    [Inner(typeof(PriceCalculator), nameof(PriceCalculator.GetPrice),
        typeof(Customer), typeof(int))]
    public static void Discount(
        [Instance(Scope.Outer)] Checkout checkout,
        [Argument("buyer", Scope.Inner)] Customer buyer,
        [ReturnValue] ref float price)
    {
        if (checkout.SaleEnabled && buyer.IsMember)
            price *= 0.9f;
    }
}
```

`[Target]` selects the containing `Total` method. `[Inner]` selects its call to `GetPrice`. The postfix changes that call's result before `Total` adds the service fee.

`checkout` comes from the outer method; `buyer` comes from the inner call. `[ReturnValue]` binds the price returned by that call. There is no IL code to write, and the bindings state which values the patch needs.

Patching `GetPrice` directly would also discount estimates and other callers. Patching the final result of `Total` would discount the service fee as well. The inner patch changes exactly the operation intended here.

### Harmony 2: a transpiler

The Harmony 2 **mod source** inserts a helper call with a transpiler:

```csharp
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

[HarmonyPatch(typeof(Checkout), nameof(Checkout.Total),
    typeof(PriceCalculator), typeof(Customer), typeof(int))]
public static class HarmonySalePatches
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        var getPrice = AccessTools.Method(typeof(PriceCalculator),
            nameof(PriceCalculator.GetPrice),
            new[] { typeof(Customer), typeof(int) });
        var discount = AccessTools.Method(typeof(HarmonySalePatches),
            nameof(Discount));
        var matcher = new CodeMatcher(instructions);

        matcher.MatchStartForward(new CodeMatch(code => code.Calls(getPrice)))
            .ThrowIfInvalid("Checkout.Total no longer calls PriceCalculator.GetPrice")
            .Repeat(match => match.Advance(1).InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Call, discount)));

        return matcher.Instructions();
    }

    public static float Discount(float price, Checkout checkout, Customer buyer)
    {
        return checkout.SaleEnabled && buyer.IsMember ? price * 0.9f : price;
    }
}
```

This uses Harmony's [CodeMatcher](https://harmony.pardeike.net/v2/articles/patching-transpiler-matcher.html) to find each `GetPrice` call and insert a helper call immediately afterward. The original price remains on the evaluation stack. `Ldarg_0` pushes the checkout instance, and `Ldarg_2` pushes `buyer`: argument zero is `this`, followed by `calculator` and `buyer`. The helper consumes those three values and returns the adjusted price.

Both patches contain the same discount rule. The transpiler additionally expresses how to find the call and supply the helper's arguments. If `Total`'s parameters change order, its argument-loading instructions need review. Disharmony's scoped bindings remove that instruction bookkeeping. Both approaches still depend on finding the intended `GetPrice` call.

Both examples change every matching call within `Total`. If you need to distinguish two calls to the same member by surrounding instructions or control flow, retain a transpiler for that selection. Likewise, an arbitrary algorithm rewrite may require a transpiler rather than an inner patch.

### Compare the results

Each case buys two items at 10 each, with a service fee of 2:

| Observation | Original | Either patched version |
| --- | --- | --- |
| Member checkout during sale | 22 | 20 |
| Direct member price | 20 | 20 |
| Member estimate | 22 | 22 |
| Non-member checkout during sale | 22 | 22 |
| Member checkout outside sale | 22 | 22 |

The member's item price becomes 18, then the unchanged fee of 2 produces a total of 20. The other callers and conditions retain their original results.

## Compatibility and conversion

Disharmony's initial beta uses Harmony 2. Check [runtime and dependency compatibility](compatibility.md) for the supported builds and required versions before adding the package to your mod.

### Harmony 3 and inner patching

Disharmony implemented inner patching before Harmony 3 introduced its infix-patching support; see [its implementation history in RimworldMods](https://github.com/RossM/RimworldMods/commit/46b8564870c7956e7f0341cde2a660a6f8136916).

[Harmony 3 also offers operation patches](https://harmony.pardeike.net/v3/articles/patching-infix.html). **Harmony 3 support is incomplete and subject to change.**

Harmony 2.4.2 exposes inner-patch APIs, but [their insertion implementation is a stub](https://github.com/pardeike/Harmony/blob/v2.4.2.0/Harmony/Internal/Infix.cs).

### Before converting an existing patch

Mixing frameworks is supported, but conversion involves more than changing attribute names:

- **Registration:** Harmony registers Harmony methods; `Patcher` registers Disharmony methods. Sharing the class marker and category does not translate `[HarmonyPrefix]`, `[HarmonyPostfix]`, or `[HarmonyPatch]` method targets into Disharmony declarations.
- **Return values:** Disharmony postfixes return `void`. To replace a result, use a reference binding. Convert a Harmony pass-through postfix explicitly rather than retaining its non-void signature.
- **State:** review the [sharing boundaries](xref:Disharmony.StateAttribute) when splitting or regrouping patches. Separate registration calls do not share Disharmony state.
- **Ordering:** Disharmony uses its own `[Priority]`: higher values run earlier for prefixes and later for postfixes. Harmony's `[HarmonyPriority]`, `[HarmonyBefore]`, and `[HarmonyAfter]` do not set Disharmony ordering. There is no Disharmony equivalent of ordering by another mod's Harmony ID. Check interactions when both frameworks patch the same method.
- **Skipping and exceptions:** a Disharmony prefix returning `false` skips the operation and remaining ordinary prefixes; postfixes still run. Use `AlwaysRun` for required observation or cleanup, respecting its [execution rules](xref:Disharmony.PatchOptions). Do not assume Harmony's observation-only prefix or finalizer behavior transfers unchanged.
- **Target restrictions:** the beta rejects generic outer methods and members declared on generic types, as well as abstract and P/Invoke targets. Check [Patcher](xref:Disharmony.Patcher) and the [selector reference](xref:Disharmony.TargetAttribute) for contracts before replacing a working Harmony patch.

Disharmony and Harmony patches can still disagree about game behavior. Inner patches reduce how much code a patch changes; they do not guarantee compatibility with every other mod or every game update. Test the converted behavior, unaffected callers, and the mod combinations you intend to support.

## Add Disharmony to an existing Harmony mod

Start with one change that benefits from inner patching or Disharmony's bindings. Existing Harmony patches do not need to be converted.

### Add the package and arrange deployment

**Not yet published:** The Disharmony NuGet package and official Workshop mod are not available yet.

Add `Disharmony` version `1.0.0-beta.1` to the project that contains the new patch. If it already references `Lib.Harmony`, check that its version meets [Disharmony's requirements](compatibility.md#harmony-version). The package includes the analyzer; you do not add Roslyn as a runtime dependency.

For a RimWorld mod, follow [project references](obj/guides/rimworld.md#create-the-project) and [the Disharmony mod dependency](obj/guides/rimworld.md#depend-on-disharmony): the official Disharmony mod supplies the runtime assemblies. Your mod's metadata should require Disharmony and load after it; a separate Harmony mod dependency is unnecessary. For other games, ensure that Disharmony and the required Harmony version are available when your mod loads.

### Keep Harmony and Disharmony patches together

Continue the checkout example with a mod that already gives members a credit of 1 on estimates. Keep that Harmony postfix and add the sale discount with Disharmony. This **mod source** reuses the game types from [the worked comparison](#the-game-code):

```csharp
using Disharmony;
using HarmonyLib;

[HarmonyPatch(typeof(Checkout))]
[HarmonyPatchCategory("pricing")]
public static class MixedCheckoutPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Checkout.Estimate),
        typeof(PriceCalculator), typeof(Customer), typeof(int))]
    public static void ApplyEstimateCredit(Customer buyer, ref float __result)
    {
        if (buyer.IsMember)
            __result -= 1f;
    }

    [Postfix]
    [Target(nameof(Checkout.Total),
        typeof(PriceCalculator), typeof(Customer), typeof(int))]
    [Inner(typeof(PriceCalculator), nameof(PriceCalculator.GetPrice),
        typeof(Customer), typeof(int))]
    public static void ApplySaleDiscount(
        [Instance(Scope.Outer)] Checkout checkout,
        [Argument("buyer", Scope.Inner)] Customer buyer,
        [ReturnValue] ref float price)
    {
        if (checkout.SaleEnabled && buyer.IsMember)
            price *= 0.9f;
    }
}
```

Disharmony accepts `[HarmonyPatch]` as a substitute for `[Patch]`, and `[HarmonyPatchCategory]` as a substitute for `[Category]`. Here the shared class metadata supplies the target type and category to both frameworks. The method declarations still use their own framework's attributes.

Register both sets of patches from your mod's initialization code. Retain your existing Harmony instance and its ID; this example creates one for demonstration:

```csharp
var harmony = new Harmony("example.checkout.mixed");
var assembly = typeof(MixedCheckoutPatches).Assembly;
harmony.PatchCategory(assembly, "pricing");
PatchHandle salePatches = Patcher.PatchCategory(assembly, "pricing");
```

The member checkout during a sale is now 20, and the member estimate is 21: both the Disharmony discount and the Harmony credit are active. The shared `pricing` category selects the mixed patch class. See [Try the mixed patches](#try-the-mixed-patches) to see both changes together.

If the sale patch must later be removed, call:

```csharp
Patcher.Unpatch(salePatches);
```

That removes the Disharmony discount while leaving the Harmony estimate credit active. Keep the handle in your mod if you need that operation later. Do not register the old transpiler and its Disharmony replacement together: both can apply the same discount.

## Try the comparison yourself

### Create a console project

Use the .NET 10 SDK, version 10.0.400 or newer. Create a console project and add Disharmony:

```shell
dotnet new console --name HarmonyComparison --framework net10.0
cd HarmonyComparison
dotnet add package Disharmony --version 1.0.0-beta.1
```

Add [the game classes](#the-game-code) and both patch classes from [the comparison](#worked-comparison-change-a-call-inside-a-method) to your project, including their `using` directives.

### Apply the Disharmony patch

Replace the generated `Program.cs` with the following code. It first displays the original checkout total, then registers the discount and shows which prices changed.

```csharp
using System;
using Disharmony;
using HarmonyLib;

public static class Program
{
    public static void Main()
    {
        var buyer = new Customer { IsMember = true };
        var calculator = new PriceCalculator();
        var checkout = new Checkout { SaleEnabled = true };
        Console.WriteLine($"Before patch: {checkout.Total(calculator, buyer, 2)}");

        Patcher.PatchAll(typeof(DisharmonySalePatches));

        Console.WriteLine($"Member checkout during sale: {checkout.Total(calculator, buyer, 2)}");
        Console.WriteLine($"Direct member price: {calculator.GetPrice(buyer, 2)}");
        Console.WriteLine($"Member estimate: {checkout.Estimate(calculator, buyer, 2)}");
        buyer.IsMember = false;
        Console.WriteLine($"Non-member checkout during sale: {checkout.Total(calculator, buyer, 2)}");
        buyer.IsMember = true;
        checkout.SaleEnabled = false;
        Console.WriteLine($"Member checkout outside sale: {checkout.Total(calculator, buyer, 2)}");
    }
}
```

Run the project:

```shell
dotnet run --configuration Release
```

You should see:

```text
Before patch: 22
Member checkout during sale: 20
Direct member price: 20
Member estimate: 22
Non-member checkout during sale: 22
Member checkout outside sale: 22
```

The member checkout falls from 22 to 20 during a sale. Direct prices, estimates, and the other checkout cases stay unchanged, matching [the results table](#compare-the-results).

### Try the Harmony patch

Replace the `Patcher.PatchAll(typeof(DisharmonySalePatches));` registration statement in `Main` with:

```csharp
new Harmony("example.checkout.sale")
    .CreateClassProcessor(typeof(HarmonySalePatches)).Patch();
```

Run the project again. The output should be identical: the transpiler and inner patch implement the same discount.

### Try the mixed patches

Add the mixed patch class from [the adoption example](#keep-harmony-and-disharmony-patches-together). Replace the sale-patch registration in `Main` with:

```csharp
var harmony = new Harmony("example.checkout.mixed");
var assembly = typeof(MixedCheckoutPatches).Assembly;
harmony.PatchCategory(assembly, "pricing");
PatchHandle salePatches = Patcher.PatchCategory(assembly, "pricing");
```

Run the project again. The member checkout still prints 20, and the member estimate now prints 21. You have kept the Harmony estimate credit and added the Disharmony sale discount. The category registers the mixed patches without applying either standalone discount patch.

In a game mod, the game supplies the target classes and the initialization hook. Your mod contains the patch classes and registers them from that hook; see [game code and mod code](runtime-patching.md#game-code-and-mod-code).

For further examples, see [usage examples](usage-examples.md). For binding signatures and defaults, use the [API reference](xref:Disharmony).
