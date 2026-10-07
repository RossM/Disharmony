# Disharmony

Disharmony is a C# game-modding framework built on Harmony. It lets mod authors change game behavior at runtime by
writing small methods called patches. A patch can run before or after a game method, inspect its inputs, change its
result, or replace its behavior.

Disharmony extends this model to operations **inside** a method. You can patch a particular method call, field or
property access, or constant while keeping the surrounding game logic intact. You describe the operation you want
to change, and Disharmony handles the underlying IL (intermediate language) instructions.

Disharmony patches coexist and interact sensibly with Harmony patches on the same method, and Disharmony understands
selected Harmony attributes and conventions. You can adopt Disharmony as you need it without rewriting existing
Harmony patches or compromising compatibility with other mods.

## Why patch inside a method?

Suppose a game calculates prices in a method called `GetPrice`. A patch that changes its return value changes prices
wherever that method is called. That is useful for a general pricing adjustment, but a discount that belongs only at
checkout needs a narrower scope: the calls to `GetPrice` inside `Checkout.Total`.

With Harmony, a patch that runs before a method is called a **prefix**, and one that runs afterward is a **postfix**.
Changes within the method often require a **transpiler**, which searches and rewrites its compiled instructions.
Disharmony lets you use prefixes and postfixes at that finer level too. These **inner patches** can express many
changes that would otherwise require a hand-written transpiler.

For new mod authors, this means you can write such patches in ordinary C# without first learning to manipulate IL.
For experienced Harmony modders, it means less instruction-matching code to maintain and a clearer statement of
what a patch is intended to change.

The examples below introduce Disharmony's attribute API; [the fluent API](#configure-patches-in-code) provides the
same model for targets selected at runtime.

## Write your first patch

Disharmony targets .NET Framework 4.7.2. Reference `Disharmony.dll` and the relevant game assemblies in your mod
project, and ensure the game loads Disharmony and a compatible `0Harmony.dll`.

The following examples use fictional game types to illustrate the API. Suppose `PriceCalculator` has a method
`float GetPrice(Character buyer, int quantity)`, and you want colony members to receive a 10% discount:

```csharp
using Disharmony;

[Patch(typeof(PriceCalculator))]
public static class PriceCalculatorPatches
{
    [Postfix]
    [Target(nameof(PriceCalculator.GetPrice), typeof(Character), typeof(int))]
    public static void ApplyMemberDiscount(Character buyer, [ReturnValue] ref float result)
    {
        if (buyer.IsColonyMember)
            result *= 0.9f;
    }
}
```

`[Patch(typeof(PriceCalculator))]` groups the patches in this class and supplies the type whose members they target.
`[Target]` selects `GetPrice`; the parameter types identify the intended overload. `[Postfix]` tells Disharmony to
run `ApplyMemberDiscount` after that method returns.

Disharmony supplies the patch's arguments. The `buyer` parameter receives the game method's argument with the same
name, and `[ReturnValue]` binds `result` to its return value. Passing `result` by `ref` lets the patch change the price
returned to the caller. Patch methods must be static and only need to declare the values they use.

To activate the patch, call this once from your mod's initialization code:

```csharp
PatchHandle pricePatches = Patcher.PatchAll(typeof(PriceCalculatorPatches));
```

The patch is then active for calls to `GetPrice` throughout the game. The returned handle identifies this group of
patches and can be kept if you need to remove it later.

## Patch an operation inside a method

Now suppose the same member discount should apply only at checkout. Instead of patching `GetPrice` itself, you can
patch its calls within `Checkout.Total`:

```csharp
using Disharmony;

[Patch(typeof(Checkout))]
public static class CheckoutPatches
{
    [Postfix]
    [Target(nameof(Checkout.Total))]
    [Inner(typeof(PriceCalculator), nameof(PriceCalculator.GetPrice),
        typeof(Character), typeof(int))]
    public static void ApplyMemberDiscount(Character buyer, [ReturnValue] ref float result)
    {
        if (buyer.IsColonyMember)
            result *= 0.9f;
    }
}
```

The patch body is the same, but the selectors give it a different scope. `[Target]` identifies `Checkout.Total` as
the **outer target**: the method whose code Disharmony modifies. `[Inner]` selects the `GetPrice` calls within it.
The postfix receives each selected call's buyer and result, and adjusts the price before `Total` uses it.

For this version, register `CheckoutPatches` in place of `PriceCalculatorPatches`:

```csharp
PatchHandle pricePatches = Patcher.PatchAll(typeof(CheckoutPatches));
```

The discount now applies only to calls within `Total`; calls to `GetPrice` elsewhere keep their original behavior.
Inner patches can also target field and property accesses with `[Inner]`, or constants with `[InnerConstant]`.

## Control inputs and execution

Both examples use postfixes to change a result. Prefixes use the same targeting and binding rules, but run before
the selected operation. For example, this method could be added to `PriceCalculatorPatches` to prevent a negative
quantity from reaching `GetPrice`:

```csharp
[Prefix]
[Target(nameof(PriceCalculator.GetPrice), typeof(Character), typeof(int))]
public static void ClampQuantity(ref int quantity)
{
    if (quantity < 0)
        quantity = 0;
}
```

A prefix can also return `bool`. Returning `false` skips the selected operation, while its postfixes still run. A
prefix that supplies a replacement result can set it through `[ReturnValue] ref ...` before returning `false`.
For an inner prefix, skipping affects only the matched operation; the surrounding method continues.
Prefixes otherwise return `void`, and postfixes always return `void`.

### Access other values

Alongside arguments and results, patches can access instances, fields, and shared state. Disharmony supports both
explicit binding attributes and several familiar Harmony parameter names:

| Value | Attribute | Parameter-name convention |
| --- | --- | --- |
| Argument | `[Argument("name")]` or `[Argument(index)]` | *`name`* |
| Argument values as an array | `[Arguments]` | `__args` |
| Return value | `[ReturnValue]` | `__result` |
| Target instance | `[Instance]` | `__instance`, `__caller` |
| Per-invocation shared state | `[State("name")]` | `__state` |
| Instance or static field | `[Field("name")]` | *`___name`* |
| Delegate to an instance or static method | `[Method("name")]` | None |
| Delegate to the nearest base-class implementation | `[BaseMethod]` | `__base` |
| Exception, in an `AlwaysRun` postfix | `[Exception]` | `__exception` |
| Target member metadata | `[MemberInfo]` | None |

Passing a bound value by value lets the patch read it; passing it by `ref` lets the patch replace it where supported.
Bindings such as `[Argument]` and `[Instance]` accept `Scope.Inner` or `Scope.Outer` to choose the source in an inner patch.
The [attribute reference](Attributes.cs) describes each binding's behavior and constraints.

## Configure patches in code

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
If you need to make the same change in multiple places, single patch can target multiple methods.

Each registration call returns a `PatchHandle`. To remove the patches associated with a handle, call:

```csharp
Patcher.Unpatch(pricePatches);
```

Patches registered by other calls remain active.

### Use alongside Harmony

Register Harmony patches through Harmony's API and Disharmony patches through `Patcher`, even when they target
the same method. Both sets of patches remain active and work together.

## Explore further

Beyond the examples above, Disharmony supports:

* **Build-time checks.** The optional [Disharmony analyzers](../Disharmony.Analyzers/README.md) catch mistakes in
  patch definitions and parameter bindings while you write your mod.
* **Compiler-generated targets.** Patch local functions, lambdas, and iterator methods, and access captured variables.
* **Patch ordering and execution options.** Control patch order, inline patch code, or use `AlwaysRun` postfixes to
  inspect and handle exceptions.
* **Diagnostics.** Inspect generated code and report errors encountered while applying patches.

For API details, see the [attribute reference](Attributes.cs), [fluent API](Patch.cs), and
[patch registration API](Patcher.cs). The [test project](../Disharmony.Tests/README.md) provides examples of more
specialized patches and instructions for running the suite.
