# From Harmony to Disharmony

If you already write Harmony patches, this guide explains what Disharmony offers, where the APIs differ, and how to add it to an existing mod. You can keep your Harmony patches and adopt Disharmony for individual changes.

The comparison below uses **Harmony 2.4.x**, the series supported by Disharmony's initial beta. See [runtime and dependency compatibility](compatibility.md) for the supported library builds and required Harmony versions.

To add Disharmony to an existing mod, review [compatibility and conversion](#compatibility-and-conversion), then follow [the adoption example](#add-disharmony-to-an-existing-harmony-mod).

## Why use Disharmony?

### Change an operation without writing a transpiler

Disharmony's **inner patches** let you put prefixes and postfixes around individual method calls inside a specific method. For example, you can adjust a price-calculation call during checkout while leaving other callers unchanged.

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

[Harmony 3 also offers operation patches](https://harmony.pardeike.net/v3/articles/patching-infix.html). **Harmony 3 support is incomplete and subject to change.**

Disharmony implemented inner patching before Harmony 3 introduced its infix-patching support; see [its implementation history in RimworldMods](https://github.com/RossM/RimworldMods/commit/46b8564870c7956e7f0341cde2a660a6f8136916).

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

## Harmony feature reference

Use this table when converting a Harmony 2 patch or looking for a familiar feature. The Harmony links lead to its documentation; the Disharmony links lead to the relevant API or example. **No equivalent** means Disharmony does not provide that feature. You can keep the patch or helper that needs it in Harmony, alongside your Disharmony patches.

| Harmony concept or feature | Disharmony equivalent or migration approach |
| --- | --- |
| [Patch class: `[HarmonyPatch]`](https://harmony.pardeike.net/v2/articles/annotations.html#patch-classes) | [`[Patch]`](xref:Disharmony.PatchAttribute). `[HarmonyPatch]` also works as the class marker and default target type when mixing frameworks. |
| [Target declarations: `[HarmonyPatch(...)]`](https://harmony.pardeike.net/v2/articles/annotations.html#basic-annotations) | [`[Target]`](xref:Disharmony.TargetAttribute) selects the member. A Harmony class marker can supply its type, but Harmony member-selection attributes are not converted into Disharmony targets. |
| [Overload selection, including `ArgumentType.Ref` and `ArgumentType.Out`](https://harmony.pardeike.net/v2/articles/annotations.html#basic-annotations) | Supply the parameter types to `[Target]`; use [`Ref<T>`](xref:Disharmony.Ref`1), [`In<T>`](xref:Disharmony.In`1), and [`Out<T>`](xref:Disharmony.Out`1) for by-reference parameters. |
| [Constructors and property accessors](https://harmony.pardeike.net/v2/articles/annotations.html#constructors) | `[Target]` with [`MemberType.Constructor`, `Getter`, or `Setter`](xref:Disharmony.MemberType). Constructor selectors use a `null` member name. |
| [Selecting targets in code: `TargetMethod()` and `TargetMethods()`](https://harmony.pardeike.net/v2/articles/patching-auxiliary.html#targetmethod) | Pass `MethodBase` targets to [`Patcher.Patch`](xref:Disharmony.Patcher). There are no automatically called target-selection methods; see [selecting targets in registration code](usage-examples.md#select-patch-targets-in-registration-code). |
| [Applying one patch to several methods](https://harmony.pardeike.net/v2/articles/annotations.html#patching-multiple-methods) | Repeat `[Target]` for individual members, use [`[Targets]`](xref:Disharmony.TargetsAttribute) for every match, or supply multiple targets to `Patcher.Patch`. |
| [Iterator body: `MethodType.Enumerator`](https://harmony.pardeike.net/v2/api/HarmonyLib.MethodType.html) | Inner patches automatically search the generated `MoveNext` body when their outer target is an iterator or async method. For a whole-method patch on that body, select `MoveNext` explicitly with [`GetStateMachineImplementation()`](xref:Disharmony.ReflectionExtensions). |
| [Generic methods and methods on generic types](https://harmony.pardeike.net/v2/articles/patching-edgecases.html#generics) | **No equivalent** for whole-method targets in the beta: Disharmony rejects both generic methods and members declared on generic types. This restriction does not prevent selecting an inner call to a constructed generic method with [`[Inner]`](xref:Disharmony.InnerAttribute). |
| [Prefix / `[HarmonyPrefix]`](https://harmony.pardeike.net/v2/articles/patching-prefix.html) | [`[Prefix]`](xref:Disharmony.PrefixAttribute). Return `void` to continue, or `bool` to decide whether to run the operation. |
| [Skipping the original with `false`](https://harmony.pardeike.net/v2/articles/patching-prefix.html#changing-the-result-and-skipping-the-original) | Return `false` from a prefix. This skips the operation and remaining ordinary Disharmony prefixes; postfixes still run. For an inner patch, only the selected operation is skipped. |
| [Prefixes that are not skipped because they have no side effects](https://harmony.pardeike.net/v2/articles/patching-prefix.html) | Opt in with [`PatchOptions.AlwaysRun`](xref:Disharmony.PatchOptions). These prefixes run **before** ordinary prefixes, must return `void`, and cannot bind the result. Disharmony does not infer this behavior from the signature. |
| [Postfix / `[HarmonyPostfix]`](https://harmony.pardeike.net/v2/articles/patching-postfix.html) | [`[Postfix]`](xref:Disharmony.PostfixAttribute). It must return `void`; bind the result by reference to change it. |
| [Pass-through postfix](https://harmony.pardeike.net/v2/articles/patching-postfix.html#pass-through-postfixes) | A `void` postfix with `[ReturnValue] ref T result`. For an iterator result, put the `yield` code in a separate helper and assign the helper's sequence. Non-void postfixes are not supported. |
| [Finalizer / `[HarmonyFinalizer]`](https://harmony.pardeike.net/v2/articles/patching-finalizer.html) | An `AlwaysRun` postfix with [`[Exception]`](xref:Disharmony.ExceptionAttribute) or `__exception`. Use `ref Exception?` to replace or suppress the exception. See [exception handling](usage-examples.md#alwaysrun-postfixes). It does not wrap separately registered Harmony patches. |
| [Original arguments by name](https://harmony.pardeike.net/v2/articles/patching-injections.html#method-arguments) | Matching parameter names also work. Use [`[Argument("name")]`](xref:Disharmony.ArgumentAttribute) to make the binding explicit. |
| [Argument aliases: `[HarmonyArgument]`](https://harmony.pardeike.net/v2/articles/patching-injections.html#method-arguments) | `[Argument("name")]` or `[Argument(index)]` on the patch parameter. |
| [Argument by index: `__0`, `__1`, etc.](https://harmony.pardeike.net/v2/articles/patching-injections.html#method-arguments) | `[Argument(0)]`, `[Argument(1)]`, etc. Disharmony does not recognize the `__0` naming convention. Indices exclude `this`. |
| [Instance: `__instance`](https://harmony.pardeike.net/v2/articles/patching-injections.html#__instance) | [`[Instance]`](xref:Disharmony.InstanceAttribute) or `__instance`. In an inner patch, this is the inner operation's instance; use `Scope.Outer` for the caller. |
| [Result: `__result`](https://harmony.pardeike.net/v2/articles/patching-injections.html#__result) | [`[ReturnValue]`](xref:Disharmony.ReturnValueAttribute) or `__result`. Pass by reference to change it. In an inner patch, it is the selected operation's result. |
| [Replacing a ref-return reference: `__resultRef` / `RefResult<T>`](https://harmony.pardeike.net/v2/articles/patching-injections.html#__resultref) | **No equivalent** for redirecting the returned reference to another storage location. A Disharmony `ref T` result binding can read or change the value at the returned location. |
| [Per-call state: `__state`](https://harmony.pardeike.net/v2/articles/patching-injections.html#__state) | [`[State]`](xref:Disharmony.StateAttribute) or `__state`. Named keys can share several values between whole-method and inner patches; see [state sharing boundaries and examples](usage-examples.md#shared-state-and-constants). Separate registration calls do not share state. |
| [Private fields: `___fieldName`](https://harmony.pardeike.net/v2/articles/patching-injections.html#___fields) | [`[Field("fieldName")]`](xref:Disharmony.FieldAttribute) or `___fieldName`. Pass by reference to change the field. |
| [All arguments: `__args`](https://harmony.pardeike.net/v2/articles/patching-injections.html#__args) | [`[Arguments]`](xref:Disharmony.ArgumentsAttribute) or `__args`. **Assigning array elements does not update the target's arguments.** Use individual argument bindings to change them. |
| [Target metadata: `__originalMethod`](https://harmony.pardeike.net/v2/articles/patching-injections.html#__originalmethod) | [`[MemberInfo]`](xref:Disharmony.MemberInfoAttribute) on a `MethodBase` parameter. The `__originalMethod` name is not recognized. For an inner member's metadata, specify `Scope.Inner`. |
| [Whether the original runs: `__runOriginal`](https://harmony.pardeike.net/v2/articles/patching-injections.html#__runoriginal) | **No equivalent** binding. |
| [Delegate injection: `[HarmonyDelegate]`](https://harmony.pardeike.net/v2/api/HarmonyLib.HarmonyDelegate.html) | [`[Method]`](xref:Disharmony.MethodAttribute) on a delegate parameter; no annotation on the delegate type is needed. See [private-method delegates](usage-examples.md#private-method-delegates). |
| [Calling a base implementation from a patch](https://harmony.pardeike.net/v2/articles/patching-edgecases.html#calling-base-methods) | [`[BaseMethod]`](xref:Disharmony.BaseMethodAttribute) or `__base` supplies a delegate. See [base-method delegates](usage-examples.md#base-method-delegates), including explicit nonvirtual calls with `[Method]`. |
| [Transpiler / `[HarmonyTranspiler]`](https://harmony.pardeike.net/v2/articles/patching-transpiler.html) | **No equivalent** patch kind. [`[Inner]`](xref:Disharmony.InnerAttribute) and [`[InnerConstant]`](xref:Disharmony.InnerConstantAttribute) replace many call, field-access, and constant edits. Keep Harmony transpilers for arbitrary IL changes or selection by surrounding instructions. |
| [Transpiler helpers: `CodeInstruction`, `CodeMatch`, and `CodeMatcher`](https://harmony.pardeike.net/v2/articles/patching-transpiler-matcher.html) | **No equivalent** helper API for Harmony instruction lists. Continue using these helpers in the Harmony transpilers you retain. |
| [Reverse patches: original, snapshot, or extracted method body](https://harmony.pardeike.net/v2/articles/reverse-patching.html) | **No equivalent**. `[Method]` can call an inaccessible method, but does not copy its body or preserve an unpatched version. |
| [Discovering patches: `PatchAll()`](https://harmony.pardeike.net/v2/articles/basics.html#patching-using-annotations) | [`Patcher.PatchAll(assembly)` or `Patcher.PatchAll(type)`](xref:Disharmony.Patcher). Discovery requires Disharmony patch-kind attributes; naming a method `Prefix` or `Postfix` is not enough. |
| [Categories: `[HarmonyPatchCategory]` / `PatchCategory()`](https://harmony.pardeike.net/v2/articles/annotations.html#basic-annotations) | [`[Category]`](xref:Disharmony.CategoryAttribute) and `Patcher.PatchCategory(assembly, category)`. `[HarmonyPatchCategory]` is also accepted. |
| [Patches without a category: `PatchAllUncategorized()`](https://harmony.pardeike.net/v2/articles/annotations.html) | `Patcher.PatchCategory(assembly, null)`. |
| [Manual configuration: `HarmonyMethod` / `PatchProcessor`](https://harmony.pardeike.net/v2/articles/basics.html#manual-patching) | Immutable [`PatchConfig`](xref:Disharmony.PatchConfig) values configured through [`Patch`](xref:Disharmony.Patch), then registered with `Patcher.Patch`. See [fluent registration](usage-examples.md#fluent-registration). |
| [Patch factories returning `MethodInfo` or `DynamicMethod`](https://harmony.pardeike.net/v2/articles/patching.html#patch-methods) | **No equivalent** automatic factory callback. Select your patch methods in registration code, then supply them to `Patch.With`. |
| [Before-registration hook: `Prepare()` / `[HarmonyPrepare]`](https://harmony.pardeike.net/v2/articles/patching-auxiliary.html#prepare) | **No equivalent** automatic hook. Initialize state and check whether to register before calling `Patcher`. |
| [After-registration hook: `Cleanup()` / `[HarmonyCleanup]`](https://harmony.pardeike.net/v2/articles/patching-auxiliary.html#cleanup) | **No equivalent** automatic hook. Put registration cleanup in your own `try`/`finally`. Deferred-generation failures are reported separately through [`Patcher.RuntimeExceptionHandler`](xref:Disharmony.Patcher); the RimWorld integration handles that reporting. |
| [Numeric priority: `[HarmonyPriority]`](https://harmony.pardeike.net/v2/articles/priorities.html) | [`[Priority]`](xref:Disharmony.PriorityAttribute) or `Patch.Priority`, using [`PatchPriority`](xref:Disharmony.PatchPriority) values. Higher values run earlier for prefixes and later for postfixes. Harmony priority attributes do not set Disharmony priority. |
| [Ordering by mod ID: `[HarmonyBefore]` / `[HarmonyAfter]`](https://harmony.pardeike.net/v2/articles/priorities.html) | **No equivalent**. Disharmony priority does not order its patches relative to separately registered Harmony patches. |
| [Harmony instance and owner ID](https://harmony.pardeike.net/v2/articles/basics.html#creating-a-harmony-instance) | Static `Patcher` registration returns a [`PatchHandle`](xref:Disharmony.PatchHandle); you do not create a Disharmony instance or supply an owner ID. |
| [Removing patches by owner, method, or patch kind](https://harmony.pardeike.net/v2/articles/basics.html#unpatching) | `Patcher.Unpatch(handle)` removes the registration group identified by that handle. There is **no equivalent** removal by Harmony ID, patch method, or kind. |
| [Inspecting registered patches: `GetPatchInfo`, `GetPatchedMethods`, `HasAnyPatches`, and `VersionInfo`](https://harmony.pardeike.net/v2/articles/basics.html#checking-for-existing-patches) | **No equivalent** public inventory of individual Disharmony patches. Harmony's inspection APIs do not list Disharmony prefixes and postfixes individually. |
| [Reflection lookup: `AccessTools` / `SymbolExtensions`](https://harmony.pardeike.net/v2/articles/utilities.html#accesstools) | **No equivalent** general-purpose lookup API. Continue using Harmony's helpers or standard reflection to obtain members for fluent registration. Attribute selectors perform their own lookup. |
| [Chained reflection access: `Traverse`](https://harmony.pardeike.net/v2/articles/utilities.html#traverse) | **No equivalent** general-purpose helper. Inside a patch, field bindings and method delegates can replace many reflection accesses. |
| [Debug IL logging: `Harmony.DEBUG` / `[HarmonyDebug]`](https://harmony.pardeike.net/v2/articles/basics.html#debug-log) | [`PatchOptions.Debug`](xref:Disharmony.PatchOptions) logs Disharmony's modified IL to the Harmony debug log. Harmony's own debug controls still apply to its output. |
| [General logging: `FileLog`](https://harmony.pardeike.net/v2/articles/utilities.html#filelog) | **No equivalent** logger. Use the game's logging system or Harmony's `FileLog`. |
