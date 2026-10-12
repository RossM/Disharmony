# Analyzer diagnostic reference

Find the ID reported by your editor or build in the [index](#diagnostic-index), then follow its link for a correction. If you are checking what the analyzer can help with, browse the sections on [patch declarations](#patch-declarations), [targets and discovery](#targets-and-discovery), [parameter bindings](#parameter-bindings), [shared state](#shared-state), and [patch bodies](#patch-bodies).

An error identifies a patch declaration Disharmony cannot use as written. A warning identifies a likely mistake or a choice worth checking; some warnings can be intentional. Each entry explains the next step. For installation, compiler requirements, or changing a rule's severity, see [analyzer setup and configuration](obj/guides/analyzers.md).

## Diagnostic index

All listed rules are enabled by default. Project configuration can change their severity.

| ID | Default severity | Problem |
| --- | --- | --- |
| DISHARMONY0001 | Error | [Patch method has generic parameters](#disharmony0001) |
| DISHARMONY0002 | Error | [Patch method is not static](#disharmony0002) |
| DISHARMONY0003 | Error | [Prefix returns neither bool nor void](#disharmony0003) |
| DISHARMONY0004 | Error | [Postfix does not return void](#disharmony0004) |
| DISHARMONY0005 | Error | [AlwaysRun prefix does not return void](#disharmony0005) |
| DISHARMONY0006 | Warning | [Patch class is not marked for discovery](#disharmony0006) |
| DISHARMONY0007 | Warning | [Patch has no target](#disharmony0007) |
| DISHARMONY0008 | Warning | [Method has patch attributes but no patch kind](#disharmony0008) |
| DISHARMONY0009 | Error | [Method has multiple patch kinds](#disharmony0009) |
| DISHARMONY0010 | Error | [Patch has multiple inner selectors](#disharmony0010) |
| DISHARMONY0011 | Error | [Selector has no declaring type](#disharmony0011) |
| DISHARMONY0012 | Warning | [Inner constant is null](#disharmony0012) |
| DISHARMONY0014 | Warning | [Duplicate discovery attributes](#disharmony0014) |
| DISHARMONY0015 | Error | [Selector has no member name](#disharmony0015) |
| DISHARMONY0016 | Error | [Parameter has conflicting binding attributes](#disharmony0016) |
| DISHARMONY0017 | Error | [Parameter requires an inner patch](#disharmony0017) |
| DISHARMONY0018 | Error | [AlwaysRun prefix requests a result](#disharmony0018) |
| DISHARMONY0019 | Error | [Exception binding requires an AlwaysRun postfix](#disharmony0019) |
| DISHARMONY0020 | Error | [Method binding requires a delegate passed by value](#disharmony0020) |
| DISHARMONY0021 | Error | [Binding type is incompatible](#disharmony0021) |
| DISHARMONY0022 | Error | [Shared state has inconsistent types](#disharmony0022) |
| DISHARMONY0024 | Error | [Inner constant has no instance, arguments, or instance fields](#disharmony0024) |
| DISHARMONY0025 | Warning | [Prefix supplies a result without skipping the target](#disharmony0025) |
| DISHARMONY0027 | Error | [Special parameter name is not recognized](#disharmony0027) |
| DISHARMONY0028 | Warning | [Multiple parameters bind the same value](#disharmony0028) |
| DISHARMONY0029 | Warning | [Shared state has no writer](#disharmony0029) |
| DISHARMONY0030 | Warning | [Shared state has no reader](#disharmony0030) |
| DISHARMONY0031 | Warning | [Assignment changes only the patch parameter](#disharmony0031) |
| DISHARMONY0032 | Warning | [AlwaysRun patch explicitly throws](#disharmony0032) |
| DISHARMONY0034 | Warning | [Prefix result uses ref rather than out](#disharmony0034) |
| DISHARMONY0035 | Warning | [Postfix argument uses out](#disharmony0035) |
| DISHARMONY0036 | Error | [Binding must be passed by value](#disharmony0036) |
| DISHARMONY0037 | Error | [Inner constant has no member metadata or base method](#disharmony0037) |
| DISHARMONY0038 | Error | [Parameter cannot accept member metadata](#disharmony0038) |

## Reading the examples

The small examples below use this hypothetical game method:

```csharp
public class PriceCalculator
{
    public int GetPrice(int quantity) => quantity * 10;
}
```

Patch snippets use `using Disharmony;` and `using System;`. Unless a snippet includes a class, put its methods inside a class marked `[Patch(typeof(PriceCalculator))]`. Compare the failing and corrected versions rather than including both. These snippets focus on declarations; see [Getting started](obj/guides/getting-started.md#write-your-first-patch) for registration and a complete runnable patch.

## Patch declarations

### DISHARMONY0001

**The patch method or one of its enclosing types has generic parameters.** Disharmony needs a concrete patch method to call.

Move the prefix or postfix into a non-generic class and remove its method type parameters. Choose concrete parameter types that match your target.

Related: [Prefix](xref:Disharmony.PrefixAttribute), [Postfix](xref:Disharmony.PostfixAttribute).

### DISHARMONY0002

**The patch method is not static.** Disharmony calls your patch without creating an instance of its class.

This declaration fails:

```csharp
[Patch(typeof(PriceCalculator))]
public class PricePatches
{
    [Prefix]
    [Target(nameof(PriceCalculator.GetPrice))]
    public void BeforePrice() { }
}
```

Add `static` to `BeforePrice`:

```csharp
[Patch(typeof(PriceCalculator))]
public class PricePatches
{
    [Prefix]
    [Target(nameof(PriceCalculator.GetPrice))]
    public static void BeforePrice() { }
}
```

If the patch needs the game's object, request it with `[Instance]` or `__instance`; do not make the patch itself an instance method. See [Instance](xref:Disharmony.InstanceAttribute).

### DISHARMONY0003

**A prefix must return `void` or `bool`.** Its own return value tells Disharmony whether to run the target; it does not become the target's return value.

Returning a price directly is invalid:

```csharp
[Prefix]
[Target(nameof(PriceCalculator.GetPrice))]
public static int FixedPrice() => 20;
```

To replace the price, bind the target's result and return `false` to skip its calculation:

```csharp
[Prefix]
[Target(nameof(PriceCalculator.GetPrice))]
public static bool FixedPrice([ReturnValue] out int price)
{
    price = 20;
    return false;
}
```

Use `void` when the prefix should let the target run. Use `bool` when it needs to decide: `true` runs the target, `false` skips it. A `ref bool` return is not supported.

Related: [Prefix](xref:Disharmony.PrefixAttribute), [ReturnValue](xref:Disharmony.ReturnValueAttribute), [first patch](obj/guides/getting-started.md#write-your-first-patch).

### DISHARMONY0004

**A postfix must return `void`.** Change its return type and remove any returned value. To change the target's result, use a `ref` result parameter instead:

```csharp
[Postfix]
[Target(nameof(PriceCalculator.GetPrice))]
public static void Discount([ReturnValue] ref int price)
{
    price /= 2;
}
```

This differs from Harmony's pass-through postfixes. See [API differences for Harmony users](harmony-comparison.md#api-differences-that-affect-your-patches) and [ReturnValue](xref:Disharmony.ReturnValueAttribute).

### DISHARMONY0005

**An `AlwaysRun` prefix must return `void`.** These prefixes run before ordinary prefixes and cannot skip the target. A later ordinary prefix can still skip the target, after the `AlwaysRun` prefixes have run.

If the patch must decide whether the target runs, remove `AlwaysRun` and use a normal `bool` prefix. If it must run before any ordinary prefix can skip the target, keep `AlwaysRun` and change the return type to `void`.

Related: [AlwaysRun](xref:Disharmony.PatchOptions.AlwaysRun).

### DISHARMONY0009

**A method has more than one prefix/postfix marker.** Keep exactly one `[Prefix]` or `[Postfix]`. If work is needed before and after the target, split it into two patch methods. Also check for an inherited patch marker.

Related: [Prefix](xref:Disharmony.PrefixAttribute), [Postfix](xref:Disharmony.PostfixAttribute).

## Targets and discovery

### DISHARMONY0006

**The patch's containing class has no `[Patch]` or `[HarmonyPatch]`.** `PatchAll(Assembly)` and `PatchCategory` will not discover it as a patch class.

Add `[Patch]` to the class that actually declares the patch. `[HarmonyPatch]` is also accepted as the discovery marker. Marking an outer class does not mark a nested class.

If you register the attributed method directly with `Patcher.Patch(methodInfo)`, the class-discovery marker is not required. Keep its patch and targeting attributes; you can suppress this warning locally. The same applies when registering the class directly with `Patcher.PatchAll(typeof(PatchClass))`.

With fluent registration, remove the unused `[Prefix]` or `[Postfix]` instead; see [DISHARMONY0007](#disharmony0007).

Related: [PatchAttribute](xref:Disharmony.PatchAttribute), [registration example](usage-examples.md#select-patch-targets-in-registration-code).

### DISHARMONY0007

**No `[Target]` or `[Targets]` chooses the operation to patch.** A `[Patch]` class marker alone does not select a method.

This method has no target:

```csharp
[Prefix]
public static void BeforePrice() { }
```

Name the target on the method, or on its containing patch class:

```csharp
[Prefix]
[Target(nameof(PriceCalculator.GetPrice))]
public static void BeforePrice() { }
```

With fluent registration, the configuration selects both the target and the patch kind. `[Prefix]` and `[Postfix]` are ignored, so remove the superfluous attribute from the method.

Related: [Target](xref:Disharmony.TargetAttribute), [Targets](xref:Disharmony.TargetsAttribute), [registration example](usage-examples.md#select-patch-targets-in-registration-code).

### DISHARMONY0008

**A method has Disharmony patch attributes but no `[Prefix]` or `[Postfix]`.** A target or priority attribute does not tell Disharmony when to call the method.

Add `[Prefix]` or `[Postfix]` according to when the method should run. If the method is only a helper, remove the unused patch attributes from it.

Related: [Prefix](xref:Disharmony.PrefixAttribute), [Postfix](xref:Disharmony.PostfixAttribute).

### DISHARMONY0010

**A patch has more than one inner selector.** Keep one `[Inner]` or `[InnerConstant]` on each patch method. Split patches that select different operations into separate methods.

One inner selector can match several occurrences of the same operation inside the target. Multiple outer `[Target]` attributes are also allowed; this restriction concerns inner selectors only.

Related: [Inner](xref:Disharmony.InnerAttribute), [InnerConstant](xref:Disharmony.InnerConstantAttribute), [patching a call](usage-examples.md#change-a-method-calls-result-using-values-from-its-caller).

### DISHARMONY0011

**A selector names a member without identifying its declaring type.** Supply a type, for example `[Target(typeof(PriceCalculator), nameof(PriceCalculator.GetPrice))]`, or provide the default type with `[Patch(typeof(PriceCalculator))]` for outer targets.

An `[Inner]` selector needs its own declaring type; it does not inherit the outer target's type. A qualified member name such as `Example.PriceCalculator.GetPrice` or `Example.PriceCalculator:GetPrice` can also specify the type.

Related: [Target](xref:Disharmony.TargetAttribute), [Inner](xref:Disharmony.InnerAttribute).

### DISHARMONY0012

**`[InnerConstant(null)]` cannot target a null literal.** Disharmony does not support replacing that constant operation.

Select a nearby method call, field access, or the whole method instead, and change its result or argument where appropriate.

Related: [InnerConstant](xref:Disharmony.InnerConstantAttribute), [usage examples](usage-examples.md).

### DISHARMONY0014

**A class has more than one patch-discovery marker.** Keep one of `[Patch]` and `[HarmonyPatch]`.

If the class contains both Harmony and Disharmony patches, use `[HarmonyPatch]`; both frameworks recognize it.

Check inherited attributes too: repeating a marker already provided by a base class can cause this warning, even when the values agree.

Related: [PatchAttribute](xref:Disharmony.PatchAttribute), [Harmony compatibility](harmony-comparison.md#api-differences-that-affect-your-patches).

### DISHARMONY0015

**A target or inner selector has no member name.** Supply the method, field, or property name. When selecting a constructor, specify `memberType: MemberType.Constructor` instead of a name.

Related: [Target](xref:Disharmony.TargetAttribute), [MemberType](xref:Disharmony.MemberType).

## Parameter bindings

A binding determines which game value Disharmony passes to a patch parameter: for example, an argument, the result, or the object being patched. Bindings can use attributes or conventional names such as `__result`. See the [parameter-binding introduction](obj/guides/getting-started.md#access-other-values) if these are unfamiliar.

### DISHARMONY0016

**A parameter has more than one binding attribute.** Each parameter can have only one binding attribute. For example, combining `[Argument]` and `[ReturnValue]` on a parameter is invalid.

These bindings conflict:

```csharp
[Postfix]
[Target(nameof(PriceCalculator.GetPrice))]
public static void ReadPrice([Argument("quantity"), ReturnValue] int value)
{
    Console.WriteLine(value);
}
```

Use separate parameters if you need both values:

```csharp
[Postfix]
[Target(nameof(PriceCalculator.GetPrice))]
public static void ReadPrice([Argument("quantity")] int quantity, [ReturnValue] int price)
{
    Console.WriteLine($"{quantity} items cost {price}");
}
```

An explicit binding attribute takes precedence over a conventional parameter name. It is multiple binding attributes, rather than a name plus an attribute, that cause this error.

Related: [Argument](xref:Disharmony.ArgumentAttribute), [ReturnValue](xref:Disharmony.ReturnValueAttribute).

### DISHARMONY0017

**The parameter requests an inner-patch context, but the patch has no inner selector.** This includes `__caller` and bindings with an explicit `Scope.Inner`.

For an ordinary whole-method patch, bind its object with `[Instance]` or `__instance`, and use the ordinary/default scope. If the patch is meant to surround an operation inside a method, add the appropriate `[Inner]` or `[InnerConstant]` selector first.

Related: [Scope](xref:Disharmony.Scope), [Instance](xref:Disharmony.InstanceAttribute), [caller-value example](usage-examples.md#change-a-method-calls-result-using-values-from-its-caller).

### DISHARMONY0018

**An `AlwaysRun` prefix requests the target's return value.** That prefix cannot replace the result or skip the target.

For a replacement result, use a normal `bool` prefix, assign an `out` result parameter, and return `false` as in [DISHARMONY0003](#disharmony0003). To inspect or change a completed result, use a postfix. If the prefix only needs to observe the call, keep `AlwaysRun` and remove its result parameter.

Related: [AlwaysRun](xref:Disharmony.PatchOptions.AlwaysRun), [ReturnValue](xref:Disharmony.ReturnValueAttribute).

### DISHARMONY0019

**An exception binding is only available in an `AlwaysRun` postfix.** A normal postfix does not run when the target throws; a prefix runs before there is an exception to inspect.

This normal postfix cannot observe a failure:

```csharp
[Postfix]
[Target(nameof(PriceCalculator.GetPrice))]
public static void ReportFailure([Exception] Exception? error)
{
    if (error != null)
        Console.Error.WriteLine(error.Message);
}
```

Add `AlwaysRun` so the postfix is called after either success or failure:

```csharp
[Postfix]
[Target(nameof(PriceCalculator.GetPrice))]
[PatchOptions(PatchOptions.AlwaysRun)]
public static void ReportFailure([Exception] Exception? error)
{
    if (error != null)
        Console.Error.WriteLine(error.Message);
}
```

On success, `error` is null. This patch only reports an exception; it does not suppress it. To recover from a specific failure, see [handling an exception](usage-examples.md#handle-an-exception-from-a-call-inside-a-method).

Related: [Exception](xref:Disharmony.ExceptionAttribute), [AlwaysRun](xref:Disharmony.PatchOptions.AlwaysRun).

### DISHARMONY0020

**`[Method]` or `[BaseMethod]` needs a concrete delegate passed by value.** Use a delegate type such as `Func<int, int>`, `Action`, or your own declared delegate, without `ref`, `out`, or `in`. `Delegate` itself is not a concrete delegate type.

Choose a delegate signature that matches the requested method.

Related: [Method](xref:Disharmony.MethodAttribute), [BaseMethod](xref:Disharmony.BaseMethodAttribute), [private-method example](usage-examples.md#call-a-private-method-from-a-patch), [base-method example](usage-examples.md#call-a-base-implementation-instead-of-an-override).

### DISHARMONY0021

**The parameter's type cannot represent the value requested by its binding.** This check covers exceptions, argument arrays, and inner constant results.

Use `Exception?` for an exception binding, a type that accepts `object[]` for `[Arguments]`, and a compatible type for an inner constant. For example, an integer constant cannot bind directly to a `string` result parameter. With `ref`, use the exact bound type, since the patch can both read and replace it. Value types also require an exact match when passed by reference.

Related: [Exception](xref:Disharmony.ExceptionAttribute), [Arguments](xref:Disharmony.ArgumentsAttribute), [InnerConstant](xref:Disharmony.InnerConstantAttribute).

### DISHARMONY0024

**An inner constant has no instance, argument, or instance field to supply.** A literal value is not a call on an object.

To access the enclosing method's instance, arguments, or fields, select `Scope.Outer` on the binding.

Related: [InnerConstant](xref:Disharmony.InnerConstantAttribute), [Scope](xref:Disharmony.Scope), [Field](xref:Disharmony.FieldAttribute).

### DISHARMONY0025

**A prefix binds the target's return value but returns `void`.** A `void` prefix cannot skip the target. When the target runs, its return value will overwrite any value the prefix assigned to the result parameter.

This prefix tries to fix the price at 20, but `GetPrice(3)` will still return 30:

```csharp
[Prefix]
[Target(nameof(PriceCalculator.GetPrice))]
public static void FixedPrice([ReturnValue] out int price)
{
    price = 20;
}
```

Choose the correction that matches what you want the patch to do:

- **Replace the target's calculation:** change the prefix's return type to `bool`, assign the replacement result, and return `false`. Setting the result alone does not skip the target; returning `true` still lets it run and overwrite that value. See the corrected [replacement prefix](#disharmony0003).
- **Inspect or change the calculated result:** use a `[Postfix]`, because the result is not available before the target runs. Pass the result by `ref` if you want to change it, as in the [discount postfix](#disharmony0004).
- **Run work before the target without replacing its result:** remove the result parameter and keep the prefix's `void` return type.

Related: [Prefix](xref:Disharmony.PrefixAttribute), [ReturnValue](xref:Disharmony.ReturnValueAttribute).

### DISHARMONY0027

**A parameter starts with `__` but does not name a supported special binding.** Check its spelling. Supported names are `__instance`, `__caller`, `__result`, `__state`, `__base`, `__exception`, and `__args`. Three leading underscores, as in `___balance`, select a field.

For an ordinary argument whose name starts with `__`, or when you prefer a descriptive parameter name, use an explicit binding attribute such as `[Argument("quantity")] int count`. Explicit bindings override conventional names.

Related: [Argument](xref:Disharmony.ArgumentAttribute), [Field](xref:Disharmony.FieldAttribute), [parameter bindings](obj/guides/getting-started.md#access-other-values).

### DISHARMONY0028

**Two parameters in the same patch bind the same value.** Usually one is redundant. Keep a single parameter and reuse it in the patch body. Separate `ref` or `out` aliases can overwrite each other's changes.

If two parameters deliberately refer to different values, make their scopes, argument selectors, state keys, or declaring types explicit.

A duplicate read-only binding can be intentional; suppress this warning locally if retaining both makes the patch clearer and you have checked the bindings.

Related: [Argument](xref:Disharmony.ArgumentAttribute), [Scope](xref:Disharmony.Scope), [State](xref:Disharmony.StateAttribute).

### DISHARMONY0034

**A prefix binds the target's result with `ref`, but the target has not calculated that result yet.** Using `ref` can suggest that the prefix should read or modify an existing result.

Choose the correction that matches your intent:

- **Supply a replacement result:** use `out` and return `false` to skip the target, as in the [replacement prefix](#disharmony0003). Assign the `out` parameter on every return path, including any path that returns `true` to let the target run. When the target runs, its return value will overwrite the value you assigned.
- **Inspect or adjust the target's calculated result:** use a `[Postfix]`. Pass the result by value to read it, or by `ref` to change it, as in the [discount postfix](#disharmony0004).
- **Run work before the target without replacing its result:** remove the result parameter.

Related: [ReturnValue](xref:Disharmony.ReturnValueAttribute), [Prefix](xref:Disharmony.PrefixAttribute), [Postfix](xref:Disharmony.PostfixAttribute).

### DISHARMONY0035

**A postfix binds an argument with `out`.** It must assign a value without first reading the argument. The target has already run, so this assignment cannot change the input it used.

Choose the correction that matches your intent:

- **Overwrite a value returned through an argument:** if the target declares the parameter as `ref` or `out`, a postfix can change the value its caller receives. Declare the patch parameter `ref` to overwrite or adjust that value. If you deliberately use `out` to replace it without reading it, suppress this warning locally.
- **Read an argument after the target runs:** remove `out`, or use `in` for read-only reference access. This also applies when the target declares that argument as `out`: the postfix can read the value the target produced, and does not need to copy its modifier.
- **Change an input before the target uses it:** move the work into a `[Prefix]`. Use `ref` to adjust the incoming value, or `out` to replace it without reading it. For a target argument passed by value, changing `out` to `ref` while keeping the postfix is rejected too. See the [argument-changing prefix](#disharmony0031).
- **Change the target's return value:** bind `[ReturnValue] ref ...` in the postfix, as in the [discount example](#disharmony0004). Assigning an argument after the calculation does not change its result.

Related: [Argument](xref:Disharmony.ArgumentAttribute), [ReturnValue](xref:Disharmony.ReturnValueAttribute), [Prefix](xref:Disharmony.PrefixAttribute), [Postfix](xref:Disharmony.PostfixAttribute).

### DISHARMONY0036

**`[Arguments]` and `[MemberInfo]` parameters must be passed by value.** Remove `ref`, `out`, or `in` from the parameter. You can inspect the argument array or member metadata without passing the parameter by reference.

Related: [Arguments](xref:Disharmony.ArgumentsAttribute), [MemberInfo](xref:Disharmony.MemberInfoAttribute).

### DISHARMONY0037

**An inner constant has no member metadata or base method.** `[MemberInfo(Scope.Inner)]` and an inner-scoped `[BaseMethod]` cannot refer to a literal.

Use `[MemberInfo(Scope.Outer)]` to inspect the enclosing target; this is also `[MemberInfo]`'s default. If you need a base-method delegate for the enclosing target, request `Scope.Outer` and check that the target is an instance method with a suitable base implementation.

Related: [MemberInfo](xref:Disharmony.MemberInfoAttribute), [BaseMethod](xref:Disharmony.BaseMethodAttribute), [InnerConstant](xref:Disharmony.InnerConstantAttribute).

### DISHARMONY0038

**The `[MemberInfo]` parameter cannot accept any supported member metadata type.** Disharmony supplies `MethodInfo`, `ConstructorInfo`, or `FieldInfo`, depending on the selected operation.

Use `System.Reflection.MemberInfo` when a patch handles several kinds of member. For a known method target, `System.Reflection.MethodInfo` is appropriate.

Related: [MemberInfo](xref:Disharmony.MemberInfoAttribute).

## Shared state

State passes a value from one patch to another during a single invocation of the outer target. See [passing a value between patches](usage-examples.md#pass-a-value-from-one-patch-to-another) for a complete example and the [State API](xref:Disharmony.StateAttribute) for sharing rules.

### DISHARMONY0022

**Parameters with the same state key use different types.** One shared value cannot be both, for example, an `int` in a prefix and a `string` in its postfix.

Use the same type for every binding of that key, or give unrelated values distinct `[State("key")]` keys. Keep each state key's type consistent across the patch class, even if its methods target different members.

Related: [State](xref:Disharmony.StateAttribute), [state example](usage-examples.md#pass-a-value-from-one-patch-to-another).

### DISHARMONY0029

**No patch in this class writes the requested state through `ref` or `out`.** A reader gets the type's default value unless another patch writes it.

This postfix has no source for `__state`:

```csharp
[Postfix]
[Target(nameof(PriceCalculator.GetPrice))]
public static void ReportQuantity(int __state)
{
    Console.WriteLine(__state);
}
```

Add a writer in the same patch class, targeting the same method:

```csharp
[Prefix]
[Target(nameof(PriceCalculator.GetPrice))]
public static void SaveQuantity(int quantity, out int __state)
{
    __state = quantity;
}

[Postfix]
[Target(nameof(PriceCalculator.GetPrice))]
public static void ReportQuantity(int __state)
{
    Console.WriteLine(__state);
}
```

Here the prefix saves `quantity`, and the postfix reads that saved value. `__state` uses its parameter name as the key. Explicit `[State]` bindings must use matching keys as well as matching types.

If reading the type's default value is intentional, you can suppress this warning. If manual registration supplies a writer from another class, check the [sharing rules](xref:Disharmony.StateAttribute) before suppressing it.

### DISHARMONY0030

**All parameters bound to this state key use `out`.** Remove unused state, or add a reader passed by value, `in`, or `ref` where the value is needed.

A prefix writer and postfix reader usually belong in the same patch class and must share a target and state key, as in [DISHARMONY0029](#disharmony0029). If you only need the value within one patch, use a local variable instead of shared state. If manual registration supplies a reader from another class, verify the [sharing rules](xref:Disharmony.StateAttribute) before suppressing it.

## Patch bodies

### DISHARMONY0031

**Assigning to a parameter passed by value changes the patch's local copy.** It does not replace the bound argument, result, field, or state.

This assignment leaves the target's quantity unchanged:

```csharp
[Prefix]
[Target(nameof(PriceCalculator.GetPrice))]
public static void DoubleQuantity(int quantity)
{
    quantity *= 2;
}
```

Use `ref` to change the argument before the target reads it:

```csharp
[Prefix]
[Target(nameof(PriceCalculator.GetPrice))]
public static void DoubleQuantity(ref int quantity)
{
    quantity *= 2;
}
```

For other bindings, check whether their API allows passing by reference; `[Arguments]` and `[MemberInfo]` do not. If the calculation is only for the patch's own use, assign to a local variable instead of reassigning the bound parameter.

Related: [Argument](xref:Disharmony.ArgumentAttribute), [ReturnValue](xref:Disharmony.ReturnValueAttribute), [Field](xref:Disharmony.FieldAttribute).

### DISHARMONY0032

**An `AlwaysRun` patch contains an explicit `throw` or rethrow.** If it escapes the patch, later `AlwaysRun` patches will not run.

Avoid throwing from the patch itself. Report a failure without throwing, or handle it locally if the patch can continue safely. For an `AlwaysRun` postfix, leave the bound exception unchanged to let the target's failure propagate; do not throw it again. See [the exception-handling example](usage-examples.md#handle-an-exception-from-a-call-inside-a-method).

The warning also flags throws caught inside the patch. Local suppression can be appropriate when you have checked that they cannot escape.

Related: [AlwaysRun](xref:Disharmony.PatchOptions.AlwaysRun), [Exception](xref:Disharmony.ExceptionAttribute).
