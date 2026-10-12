# Getting started with Disharmony

Disharmony is a C# game-modding framework built on Harmony. It lets mod authors change game behavior at runtime by
writing small methods called patches. A patch can run before or after a game method, inspect its inputs, change its
result, or replace its behavior.

Disharmony extends this model to operations **inside** a method. You can patch a particular method call, field or
property access, or constant while keeping the surrounding game logic intact. You describe the operation you want
to change, and Disharmony handles the underlying IL (intermediate language) instructions.

Disharmony patches coexist and interact sensibly with Harmony patches on the same method, and Disharmony understands
selected Harmony attributes and conventions. You can use Disharmony alongside existing Harmony patches.
As with any patching framework, interactions depend on the changes each mod makes.

If you are new to runtime patching, start with [How runtime patching works](../docs/runtime-patching.md), which explains separate game and mod assemblies, targets, and registration.

## Why patch inside a method?

Suppose a game calculates prices in a method called `GetPrice`. A patch that changes its return value changes prices
wherever that method is called. That is useful for a general pricing adjustment, but a discount that belongs only at
checkout needs a narrower scope: the calls to `GetPrice` inside `Checkout.Total`.

With Harmony, a patch that runs before a method is called a **prefix**, and one that runs afterward is a **postfix**.
Changes within the method often require a **transpiler**, which searches and rewrites its compiled instructions.
Disharmony lets you put prefixes and postfixes around the `GetPrice` calls inside `Checkout.Total`. These **inner patches** can express many
changes that would otherwise require a hand-written transpiler.

For new mod authors, this means you can write such patches in ordinary C# without first learning to manipulate IL.
For experienced Harmony modders, it means less instruction-matching code to maintain and a clearer statement of
what a patch is intended to change.

The examples below introduce Disharmony's attribute API; [the fluent API](#configure-patches-in-code) provides the
same model for targets selected at runtime.

## Write your first patch

In this exercise, you will run a small console application, apply a 10% member discount, and compare the results before and after patching. Then you will limit the discount to checkout calls.

The application includes fictional game classes and your patch code in one project so you can run the example. In a real mod, the game supplies those classes through assembly references; your patch code and initialization belong in your mod project. See [How runtime patching works](../docs/runtime-patching.md#game-code-and-mod-code) for that separation.

### Create the example project

Use the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0), version 10.0.400 or newer, and a C# editor. Open a terminal in the directory where you want to create the example and run:

```shell
dotnet new console --name FirstDisharmonyPatch --framework net10.0
cd FirstDisharmonyPatch
dotnet add package Disharmony --version 1.0.0-beta.2
```

The last command adds Disharmony to the project and restores its Harmony dependency and bundled analyzer. This console example uses `net10.0`; for a game mod, choose the target framework that matches the game's runtime. See [compatibility](../docs/compatibility.md) for the supported builds and Harmony versions.

### Add the fictional game source

Create `Game.cs` in the project directory with this complete source:

```csharp
using System.Runtime.CompilerServices;

public sealed class Customer
{
    public bool IsMember { get; set; }
}

public class PriceCalculator
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public float GetPrice(Customer buyer, int quantity)
    {
        return 10f * quantity;
    }
}

public sealed class Checkout
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public float Total(PriceCalculator calculator, Customer buyer, int quantity)
    {
        return calculator.GetPrice(buyer, quantity);
    }
}
```

Each item costs 10. `GetPrice` currently charges members and non-members the same amount, and `Checkout.Total` calls it to calculate the total.

The runtime can copy a called method's code into its caller to make it run faster. This is called **inlining**. If it happens before a patch is registered, the copied code can keep running without the patch. The sample's `NoInlining` attributes prevent this so you can reliably compare behavior before and after registration.

### Add the discount patch

Create `PriceCalculatorPatches.cs` with this **mod source**:

```csharp
using Disharmony;

[Patch(typeof(PriceCalculator))]
public static class PriceCalculatorPatches
{
    [Postfix]
    [Target(nameof(PriceCalculator.GetPrice), typeof(Customer), typeof(int))]
    public static void ApplyMemberDiscount(Customer buyer, [ReturnValue] ref float result)
    {
        if (buyer.IsMember)
            result *= 0.9f;
    }
}
```

`[Patch(typeof(PriceCalculator))]` supplies the game type to target. `[Target]` selects `GetPrice` with the parameter types `Customer` and `int`. `[Postfix]` makes `ApplyMemberDiscount` run after that method returns.

Disharmony supplies the patch's arguments. `buyer` receives the game method's argument with the same name, and `[ReturnValue]` supplies its result. The `ref` modifier lets the patch change the result returned to the caller. For a member buying two items, the game calculates 20 and the patch changes it to 18.

### Register the patch and run the example

Replace the generated `Program.cs` with:

```csharp
using System;
using Disharmony;

public static class Program
{
    public static void Main()
    {
        var buyer = new Customer { IsMember = true };
        var calculator = new PriceCalculator();
        var checkout = new Checkout();

        Console.WriteLine($"Before patch: {calculator.GetPrice(buyer, 2)}");

        PatchHandle pricePatches = Patcher.PatchAll(typeof(PriceCalculatorPatches));

        Console.WriteLine($"Member price: {calculator.GetPrice(buyer, 2)}");
        Console.WriteLine($"Member checkout: {checkout.Total(calculator, buyer, 2)}");

        buyer.IsMember = false;
        Console.WriteLine($"Non-member price: {calculator.GetPrice(buyer, 2)}");
        Console.WriteLine($"Non-member checkout: {checkout.Total(calculator, buyer, 2)}");
    }
}
```

`Patcher.PatchAll(typeof(PriceCalculatorPatches))` registers the patches in that class. The returned `PatchHandle` identifies this group of patches if you want to [remove them later](#select-and-manage-patches).

Run the application from the project directory:

```shell
dotnet run --configuration Release
```

It prints:

```text
Before patch: 20
Member price: 18
Member checkout: 18
Non-member price: 20
Non-member checkout: 20
```

The discount applies to `GetPrice` wherever it is called, including the call inside `Checkout.Total`. The condition in the patch leaves non-member prices unchanged.

## Patch an operation inside a method

Now make the discount apply only at checkout. A direct call to `GetPrice` should return the original price, even for a member.

Create `CheckoutPatches.cs` in the same project:

```csharp
using Disharmony;

[Patch(typeof(Checkout))]
public static class CheckoutPatches
{
    [Postfix]
    [Target(nameof(Checkout.Total))]
    [Inner(typeof(PriceCalculator), nameof(PriceCalculator.GetPrice),
        typeof(Customer), typeof(int))]
    public static void ApplyMemberDiscount(Customer buyer, [ReturnValue] ref float result)
    {
        if (buyer.IsMember)
            result *= 0.9f;
    }
}
```

The patch body is the same, but the selectors give it a different scope. `[Target]` identifies `Checkout.Total` as the **outer target**: the method containing the operation to change. `[Inner]` selects the `GetPrice` call within it. The postfix adjusts that call's result before `Total` uses it.

In `Program.cs`, replace the registration line with:

```csharp
PatchHandle pricePatches = Patcher.PatchAll(typeof(CheckoutPatches));
```

This registers `CheckoutPatches` instead of `PriceCalculatorPatches`. Run `dotnet run --configuration Release` again. The new process applies only the newly selected patch class, and prints:

```text
Before patch: 20
Member price: 20
Member checkout: 18
Non-member price: 20
Non-member checkout: 20
```

The direct member price stays at 20; the discount applies only to the call inside `Total`. Inner patches can also target field and property accesses with `[Inner]`, or constants with `[InnerConstant]`. The [usage examples](../docs/usage-examples.md) show these selectors and how to choose values from the caller or inner operation.

### Use the patch in a game mod

In your mod project, reference the game's assemblies and add the Disharmony package. Choose the actual game method you want to change, and register your patch class once from your mod's initialization code, before the game uses that method. The console application's `Main` method supplies that initialization step for this exercise; your game's mod system supplies the corresponding hook.

[Plan a patch for your game](../docs/planning-a-game-patch.md) explains how to find and understand a suitable target. For RimWorld, follow [Write a RimWorld mod with Disharmony](../Disharmony.RimWorld/README.md) for project setup, registration, and in-game verification using the official Disharmony mod dependency. For other games, you can redistribute `Disharmony.dll` with your mod; ensure Harmony is also available when the game loads it.

The sections below cover additional bindings and registration options. You can return to them as needed while working on your own patches.

## Control inputs and execution

Both examples use postfixes to change a result. Prefixes use the same targeting and binding rules, but run before
the selected operation. For example, this method could be added to `PriceCalculatorPatches` to prevent a negative
quantity from reaching `GetPrice`:

```csharp
[Prefix]
[Target(nameof(PriceCalculator.GetPrice), typeof(Customer), typeof(int))]
public static void ClampQuantity(ref int quantity)
{
    if (quantity < 0)
        quantity = 0;
}
```

When trying this prefix, use the first example's registration of `PriceCalculatorPatches` instead of `CheckoutPatches`. A call to `GetPrice(buyer, -1)` then receives a quantity of zero and returns zero.

A prefix can also return `bool`. Returning `false` skips the selected operation, while its postfixes still run. A
prefix that supplies a replacement result can set it through `[ReturnValue] out ...` before returning `false`. Use `out` because the operation has not yet produced a result.
For an inner prefix, skipping affects only the matched operation; the surrounding method continues.
Prefixes otherwise return `void`, and postfixes always return `void`.

### Access other values

Alongside arguments and results, patches can access instances, fields, and shared state. Disharmony supports both
explicit binding attributes and several familiar Harmony parameter names:

| Value | Attribute | Parameter-name convention |
| --- | --- | --- |
| Argument | [Argument](Attributes.cs): `[Argument("name")]` or `[Argument(index)]` | *`name`* |
| Argument values as an array | [Arguments](Attributes.cs) (`[Arguments]`) | `__args` |
| Return value | [ReturnValue](Attributes.cs) (`[ReturnValue]`) | `__result` |
| Target instance | [Instance](Attributes.cs) (`[Instance]`) | `__instance`, `__caller` |
| Per-invocation shared state | [State](Attributes.cs) (`[State("name")]`) | `__state` |
| Instance or static field | [Field](Attributes.cs) (`[Field("name")]`) | *`___name`* |
| Delegate to an instance or static method | [Method](Attributes.cs) (`[Method("name")]`) | None |
| Delegate to the nearest base-class implementation | [BaseMethod](Attributes.cs) (`[BaseMethod]`) | `__base` |
| Exception, in an `AlwaysRun` postfix | [Exception](Attributes.cs) (`[Exception]`) | `__exception` |
| Target member metadata | [MemberInfo](Attributes.cs) (`[MemberInfo]`) | None |

Passing a bound value by value lets the patch read it; passing it by `ref` lets the patch replace it where supported.
Bindings such as `[Argument]` and `[Instance]` accept [Scope](Enums.cs) values `Scope.Inner` or `Scope.Outer` to choose the source in an inner patch.
The [attribute reference](Attributes.cs) describes each binding's behavior and constraints.

## Configure patches in code

For a complete example with target lookup, registration, and expected results, see [Select patch targets in registration code](../docs/usage-examples.md#fluent-registration).

When targets are selected at runtime, the fluent API lets you configure patches using reflection objects. With
`getPrice` and `applyMemberDiscount` holding the `MethodInfo` objects for `GetPrice` and `ApplyMemberDiscount`, the
same discount patch can be registered as:

```csharp
PatchHandle pricePatches = Patcher.Patch(
    Patch.Postfix.Of(getPrice).With(applyMemberDiscount));
```

The builder creates a `PatchConfig`, which `Patcher.Patch` applies. Add `.Inner(innerMethod)` to target calls inside
the outer method; the fluent API also supports fields, properties, and constants. Parameter-binding attributes such
as `[ReturnValue]` still apply, while the configuration supplies the patch type and targets.

## Select and manage patches

For larger patch sets, `Patcher.PatchAll(assembly)` discovers patch classes marked with `[Patch]` or `[HarmonyPatch]`.
Group classes with `[Category("name")]` and use `Patcher.PatchCategory(assembly, category)` to apply selected groups.
If you need to make the same change in multiple places, apply multiple [Target](Attributes.cs) attributes to one patch method, or use [Targets](Attributes.cs) to select every member matching a set of criteria.

Each registration call returns a `PatchHandle`. To remove the patches associated with a handle, call:

```csharp
Patcher.Unpatch(pricePatches);
```

Patches registered by other calls remain active.

### Deferred compilation and errors

The official RimWorld mod completes deferred compilation in the background and reports patching errors to the game log. RimWorld mods using that dependency ordinarily do not need to call `ForceApply` or install an error handler. The guidance below is for integrating Disharmony with other games or applications.

Disharmony may generate a patched method's body when the method is first called. Call `Patcher.ForceApply()` during
loading if you want to do that work earlier. It keeps processing pending methods until none remain, including methods
added while it runs.

Subscribe to `Patcher.RuntimeExceptionHandler` to receive errors encountered while generating or applying patches.
Without a subscriber, Disharmony writes them to Harmony's file log. These are patching errors, rather than exceptions
thrown by your target or patch methods during normal execution.

Use a handler to record or display the error. It must not add or remove patches, or wait for another thread to do so.
Calling Disharmony's patching or unpatching methods from a handler throws `InvalidOperationException`. Make any patch
changes after the handler returns.

If `ForceApply()` cannot compile a method, it reports the error and continues compiling the other methods. A later
`ForceApply()` call does not retry that failed method. This error-handling behavior is provisional and may change in a
future release. An exception thrown by your handler, or by Harmony while recovering from a failure, still propagates
to the caller.

### Use alongside Harmony

Register Harmony patches through Harmony's API and Disharmony patches through `Patcher`, even when they target
the same method. Both sets of patches remain active and work together. See [Add Disharmony to an existing Harmony mod](../docs/harmony-comparison.md#add-disharmony-to-an-existing-harmony-mod) for shared class metadata, registration, and conversion advice.

## Explore further

To apply these ideas to your own game, [Plan a patch for your game](../docs/planning-a-game-patch.md) explains how to investigate game code, learn from existing mods, and choose where to patch.

The [usage examples](../docs/usage-examples.md) show advanced features with complete hypothetical target source. Beyond the examples above, Disharmony supports:

* **Build-time checks.** The bundled [Disharmony analyzers](../Disharmony.Analyzers/README.md) catch mistakes in
  patch definitions and parameter bindings while you write your mod.
* **Compiler-generated targets.** [Target selectors](Attributes.cs) can select local functions, lambdas, and iterator methods.
* **Patch ordering and execution options.** Use [Priority](Attributes.cs) to control patch order and [PatchOptions](Enums.cs) to inline patch code or run postfixes even after an exception. Use `AlwaysRun` postfixes to
  inspect and handle exceptions.
* **Diagnostics.** Use [PatchOptions](Enums.cs) to log generated code and [Patcher](Patcher.cs) to handle errors encountered while applying patches.

For API details, see the [attribute reference](Attributes.cs), [fluent API](Patch.cs), and
[patch registration API](Patcher.cs).
