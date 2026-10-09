# Usage examples

Choose an example for the change you want to make. The examples share the [fictional shop source](#target-source) below; each applies its patches separately.

These examples build on [Getting started](obj/guides/getting-started.md) and assume you have completed its tutorial. To find a suitable target in an actual game, see [Plan a patch for your game](planning-a-game-patch.md).

| What you want to do | Features used |
| --- | --- |
| [Change a method call's result using values from its caller](#inner-calls-and-scope) | Inner method calls; outer and inner scope |
| [Change a field read without changing the stored value](#field-reads-and-multiple-targets) | Field reads; multiple `[Target]` attributes |
| [Change the value passed to a property setter](#property-setters) | Inner property setters; argument binding |
| [Pass a value from one patch to another](#shared-state-and-constants) | Constructor results; named state; constants |
| [Call a private method from a patch](#private-method-delegates) | Method delegates |
| [Call a base implementation instead of an override](#base-method-delegates) | Base-method delegates; skipping the original method |
| [Handle an exception from a call inside a method](#alwaysrun-postfixes) | `AlwaysRun` postfixes; exception binding |
| [Patch a lambda](#lambda-targets-and-inlining) | Lambda selectors; inline patches |
| [Select patch targets in registration code](#fluent-registration) | Fluent configuration; patch priority |

## Before trying an example

Use the console project from the tutorial:

1. Replace `Game.cs` with the complete [target source](#target-source) below.
2. Replace the tutorial's patch class with the class from your chosen example. Add imports needed by that class, such as `System.Text` for `StringBuilder`.
3. Replace the body of `Main` with these declarations, followed by the example's registration and demonstration statements:

```csharp
var buyer = new Customer { IsMember = true };
var calculator = new PriceCalculator();
var checkout = new Checkout { SaleEnabled = true };
```

Try one example per run, using its registration call as written. Registering the whole assembly would combine the examples and change their results. When switching examples, replace the patch class and the statements after these declarations; each run should start with fresh objects.

## Target source

The examples patch the classes below. Each item costs 10, and checkout adds a service fee of 2.

```csharp
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

public sealed class Customer
{
    public bool IsMember { get; set; }
    public bool CanBuy { get; set; } = true;
}

public sealed class Order
{
    public int Quantity { get; set; }
}

public sealed class PriceUnavailableException : Exception
{
    public PriceUnavailableException() : base("Pricing is offline.") { }
}

public class PriceCalculator
{
    public bool IsOnline { get; set; } = true;

    public virtual float GetPrice(Customer buyer, int quantity)
    {
        if (!IsOnline)
            throw new PriceUnavailableException();
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));
        return 10f * quantity;
    }
}

public sealed class Checkout
{
    public bool SaleEnabled { get; set; }
    public float ServiceFee = 2f;
    public float SpendingLimit { get; set; } = 30f;

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

    [MethodImpl(MethodImplOptions.NoInlining)]
    public int ChangeQuantity(Order order, int requested)
    {
        order.Quantity = requested;
        return order.Quantity;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public string GetReceipt(PriceCalculator calculator, Customer buyer, int quantity)
    {
        var receipt = new StringBuilder();
        receipt.Append("Items: ").Append(quantity).AppendLine();
        receipt.Append("Total: ");
        receipt.Append(Total(calculator, buyer, quantity)
            .ToString("0.00", CultureInfo.InvariantCulture));
        return receipt.ToString();
    }

    private bool IsEligible(Customer buyer)
    {
        return buyer.CanBuy;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool Approve(Customer buyer, float amount)
    {
        return IsEligible(buyer) && amount <= SpendingLimit;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public string?[] VisibleNames(string?[] names)
    {
        return Array.FindAll(names, name => name!.Length > 0);
    }
}
```

The runtime can optimize a method call by copying the called method's code into its caller. This is called **inlining**. If it happens before the patch is registered, that copied code will keep running without the patch. The sample targets use `NoInlining` to prevent this when you compile and run the examples.

<a id="inner-calls-and-scope"></a>

## Change a method call's result using values from its caller

`Checkout.Total` in the [target source](#target-source) calls `GetPrice` for the item price, then adds a service fee of 2. Two items cost 22, whether the buyer is a member or not. `Estimate` uses the same calculation.

**Goal:** give members a 10% discount during a checkout sale, without changing prices returned by direct calls to `PriceCalculator.GetPrice`.

```csharp
[Patch(typeof(Checkout))]
public static class SalePricePatches
{
    [Postfix]
    [Target(nameof(Checkout.Total))]
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

```csharp
PatchHandle handle = Patcher.PatchAll(typeof(SalePricePatches));
Console.WriteLine(checkout.Total(calculator, buyer, 2)); // 20
Console.WriteLine(calculator.GetPrice(buyer, 2));       // 20, unchanged
Console.WriteLine(checkout.Estimate(calculator, buyer, 2)); // 22, unchanged
```

`[Target]` selects `Checkout.Total`, the **outer target**. `[Inner]` selects the call to `GetPrice(Customer, int)` inside it, the **inner target**. The postfix runs just after that call returns. It reduces the item price from 20 to 18; `Total` then adds the fee of 2.

`Total` runs on the `checkout` object, while `GetPrice` runs on the `calculator` object. The patch needs the checkout to read `SaleEnabled`, and the buyer to read `IsMember`:

| Binding | Value supplied to this patch |
| --- | --- |
| `[Instance(Scope.Outer)]` | The `Checkout` object on which `Total` was called |
| `[Argument("buyer", Scope.Inner)]` | The buyer passed to the selected `GetPrice` call |
| `[ReturnValue]` | The price returned by that call; `ref` lets the patch change it |

Without `Scope.Outer`, `[Instance]` would supply the `PriceCalculator` for this inner patch. Disabling the sale, or using a non-member, leaves the total at 22.

Related: [InnerAttribute](xref:Disharmony.InnerAttribute), [Scope](xref:Disharmony.Scope), [InstanceAttribute](xref:Disharmony.InstanceAttribute), [ArgumentAttribute](xref:Disharmony.ArgumentAttribute).

<a id="field-reads-and-multiple-targets"></a>

## Change a field read without changing the stored value

`Total` and `Estimate` in the [target source](#target-source) both read `Checkout.ServiceFee` and add it to the item price. With the stored fee of 2, two items cost 22.

**Goal:** waive the service fee for members in both `Total` and `Estimate`, while keeping the field's stored value unchanged.

```csharp
[Patch(typeof(Checkout))]
public static class ServiceFeePatches
{
    [Postfix]
    [Target(nameof(Checkout.Total))]
    [Target(nameof(Checkout.Estimate))]
    [Inner(typeof(Checkout), nameof(Checkout.ServiceFee), MemberType.Getter)]
    public static void WaiveFee(
        [Argument("buyer", Scope.Outer)] Customer buyer,
        [ReturnValue] ref float fee)
    {
        if (buyer.IsMember)
            fee = 0f;
    }
}
```

```csharp
PatchHandle handle = Patcher.PatchAll(typeof(ServiceFeePatches));
Console.WriteLine(checkout.Total(calculator, buyer, 2));    // 20
Console.WriteLine(checkout.Estimate(calculator, buyer, 2)); // 20
Console.WriteLine(checkout.ServiceFee);                    // 2
```

For a field, `MemberType.Getter` selects a **read** of its value. When either method reads `ServiceFee`, the postfix receives 2 in `fee`. For a member, it changes that value to 0, so the method adds no fee. The field itself still contains 2.

The buyer is an argument of `Total` or `Estimate`. `[Argument("buyer", Scope.Outer)]` gets it from that method, since the field read has no arguments.

Each `[Target]` selects one method. Applying it twice selects both. To select all methods matching a name or parameter list, use `[Targets]`; the [lambda example](#lambda-targets-and-inlining) shows this.

Related: [MemberType](xref:Disharmony.MemberType), [TargetAttribute](xref:Disharmony.TargetAttribute), [TargetsAttribute](xref:Disharmony.TargetsAttribute). To access or modify the stored field itself, use [FieldAttribute](xref:Disharmony.FieldAttribute).

<a id="property-setters"></a>

## Change the value passed to a property setter

`Checkout.ChangeQuantity` in the [target source](#target-source) assigns its `requested` argument to `Order.Quantity` and returns the assigned value. It accepts negative values, so requesting -3 stores and returns -3.

**Goal:** prevent `ChangeQuantity` from assigning a negative quantity, without changing assignments made elsewhere.

```csharp
[Patch(typeof(Checkout))]
public static class QuantityPatches
{
    [Prefix]
    [Target(nameof(Checkout.ChangeQuantity))]
    [Inner(typeof(Order), nameof(Order.Quantity), MemberType.Setter)]
    public static void Clamp([Argument("value", Scope.Inner)] ref int value)
    {
        value = Math.Max(0, value);
    }
}
```

```csharp
PatchHandle handle = Patcher.PatchAll(typeof(QuantityPatches));
var order = new Order();
Console.WriteLine(checkout.ChangeQuantity(order, -3)); // 0
order.Quantity = -3;
Console.WriteLine(order.Quantity);                   // -3, direct assignment is unchanged
```

Assigning `order.Quantity` calls the property's setter. Its input parameter is named `value`, so `[Argument("value", Scope.Inner)]` gives `Clamp` the number about to be assigned.

Passing that number by `ref` lets `Clamp` change it to 0 before the setter runs. The prefix returns `void`, so it does not skip the setter. The assignment still happens, using the changed value. `ChangeQuantity`'s argument `requested` remains -3; only the value passed to the setter changes.

Related: [InnerAttribute](xref:Disharmony.InnerAttribute), [ArgumentAttribute](xref:Disharmony.ArgumentAttribute), [PrefixAttribute](xref:Disharmony.PrefixAttribute).

<a id="shared-state-and-constants"></a>

## Pass a value from one patch to another

**Goal:** add a member-account line to a receipt and change its total label.

`GetReceipt` in the [target source](#target-source) creates a `StringBuilder`, writes the items line, then writes the total label and price. For two items, it returns `Items: 2` followed by `Total: 22.00` on the next line.

To insert a line into that receipt, a patch needs access to the builder. But `receipt` is a local variable inside `GetReceipt`, so a patch cannot receive it through an argument binding.

Use two patches: one to capture the builder just after it is created, and another to use it when the target reaches the total label. `[State]` provides temporary storage for passing a value between patches during one call to the target method. The `saved` parameters in `Capture` and `Annotate` both use the key `"receipt"`, so they refer to the same stored value. You choose this key; it does not need to match a variable name in the target.

```csharp
[Patch(typeof(Checkout))]
public static class ReceiptPatches
{
    [Postfix]
    [Target(nameof(Checkout.GetReceipt))]
    [Inner(typeof(StringBuilder), memberType: MemberType.Constructor,
        parameterTypes: [])]
    public static void Capture(
        [ReturnValue] StringBuilder builder,
        [State("receipt")] out StringBuilder saved)
    {
        saved = builder;
    }

    [Postfix]
    [Target(nameof(Checkout.GetReceipt))]
    [InnerConstant("Total: ")]
    public static void Annotate(
        [Argument("buyer", Scope.Outer)] Customer buyer,
        [State("receipt")] StringBuilder saved,
        [ReturnValue] ref string label)
    {
        if (buyer.IsMember)
            saved.AppendLine("Member account");
        label = "Amount due: ";
    }
}
```

```csharp
PatchHandle handle = Patcher.PatchAll(typeof(ReceiptPatches));
Console.WriteLine(checkout.GetReceipt(calculator, buyer, 2));
```

The resulting text is:

```text
Items: 2
Member account
Amount due: 22.00
```

Here is what happens during a call to `GetReceipt`:

1. **Create the builder.** After `new StringBuilder()` creates the object, Disharmony calls `Capture`. `[ReturnValue]` supplies that new object. The patch assigns it to `saved`, and `[State("receipt")] out` saves it for `Annotate`.
2. **Write the items line.** The original method continues and appends `Items: 2` and a newline to that builder.
3. **Reach the total label.** Before `receipt.Append("Total: ")` appends its text, Disharmony calls `Annotate`. `[InnerConstant("Total: ")]` selects this use of the string literal. The postfix receives the string in `label` and the saved builder in `saved`.
4. **Add the member line and change the label.** For a member, `Annotate` calls `saved.AppendLine("Member account")`. It then sets `label` to `"Amount due: "`. Because `label` is passed by `ref`, this changes the text that `Append` will receive.
5. **Finish the receipt.** The original `Append` writes `Amount due: `, and the method appends the price and returns the completed text.

The constructor selector's `parameterTypes: []` selects `new StringBuilder()` with no arguments. The constant postfix runs after the string value is produced, before `Append` uses it.

The saved builder belongs to this call to `GetReceipt`. A later call starts with fresh state, so receipts do not share builders. Register both patches together with the single `PatchAll` call shown above; separate registration calls would not share this state.

When adapting this pattern, check the target's execution order: the patch that saves the value must run before the patch that reads it. Also, `InnerConstant` selects every occurrence of the specified value in the target. If `"Total: "` appeared twice, `Annotate` would run twice.

Related: [StateAttribute](xref:Disharmony.StateAttribute), [InnerConstantAttribute](xref:Disharmony.InnerConstantAttribute), [InnerAttribute](xref:Disharmony.InnerAttribute).

<a id="private-method-delegates"></a>

## Call a private method from a patch

`Checkout.Approve` in the [target source](#target-source) requires both a successful private `IsEligible` check and an amount no greater than `SpendingLimit`. The limit is 30, so an otherwise eligible buyer cannot spend 40.

**Goal:** allow a member to spend up to 20 above the checkout limit, while preserving the application's private eligibility check.

```csharp
[Patch(typeof(Checkout))]
public static class ApprovalPatches
{
    [Postfix]
    [Target(nameof(Checkout.Approve))]
    public static void ExtendLimit(
        Customer buyer,
        float amount,
        [Instance] Checkout checkout,
        [Method("IsEligible")] Func<Customer, bool> isEligible,
        [ReturnValue] ref bool approved)
    {
        if (!approved && buyer.IsMember && isEligible(buyer))
            approved = amount <= checkout.SpendingLimit + 20f;
    }
}
```

```csharp
PatchHandle handle = Patcher.PatchAll(typeof(ApprovalPatches));
Console.WriteLine(checkout.Approve(buyer, 40f)); // true; the unpatched limit is 30
buyer.CanBuy = false;
Console.WriteLine(checkout.Approve(buyer, 40f)); // false; eligibility still applies
```

`[Method("IsEligible")]` lets the patch call the private method through `isEligible`. This parameter is a delegate: calling `isEligible(buyer)` calls `IsEligible(buyer)` on the same checkout whose `Approve` method is running.

`Func<Customer, bool>` means the delegate takes one `Customer` and returns a `bool`, matching `IsEligible`. You pass only the buyer when calling it; Disharmony has already supplied the checkout object.

The higher limit applies only if `IsEligible` allows the buyer to make a purchase. If the application later changes that check, the patch will use the updated check too.

Related: [MethodAttribute](xref:Disharmony.MethodAttribute), [InstanceAttribute](xref:Disharmony.InstanceAttribute).

<a id="base-method-delegates"></a>

## Call a base implementation instead of an override

**Goal:** waive a premium calculator's surcharge for members by using the base implementation, while retaining the override for other buyers.

For this example, add the following subclass to the [fictional game source](#target-source). `PremiumPriceCalculator` adds 5 to the price calculated by `PriceCalculator`, so two items cost 25 for any buyer:

```csharp
public sealed class PremiumPriceCalculator : PriceCalculator
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public override float GetPrice(Customer buyer, int quantity)
    {
        return base.GetPrice(buyer, quantity) + 5f;
    }
}
```

The patch goes in your mod code:

```csharp
[Patch(typeof(PremiumPriceCalculator))]
public static class SurchargePatches
{
    [Prefix]
    [Target(nameof(PremiumPriceCalculator.GetPrice))]
    public static bool WaiveSurcharge(
        Customer buyer,
        int quantity,
        [BaseMethod] Func<Customer, int, float> basePrice,
        [ReturnValue] out float price)
    {
        price = 0f;
        if (!buyer.IsMember)
            return true;
        price = basePrice(buyer, quantity);
        return false;
    }
}
```

```csharp
PatchHandle handle = Patcher.PatchAll(typeof(SurchargePatches));
var premium = new PremiumPriceCalculator();
Console.WriteLine(premium.GetPrice(buyer, 2)); // 20 rather than 25
Console.WriteLine(premium.GetPrice(new Customer(), 2)); // 25
```

`[BaseMethod]` supplies `basePrice`, a delegate that calls `PriceCalculator.GetPrice` on this premium calculator. That method calculates the item price without adding the surcharge. Calling `premium.GetPrice` from the prefix would enter the patched override again and call the prefix repeatedly.

For a member, the prefix puts the base price into `price` and returns `false`. Disharmony skips the override and returns that price to the caller. For a non-member, the prefix returns `true`; the override runs and adds its surcharge. C# requires the prefix to assign its `out` parameter even when it returns `true`. The initial `price = 0f` satisfies that requirement; the override replaces the value when it runs.

Use `[BaseMethod]` for an instance method that has a base-class implementation. Static methods have no such implementation to call.

Related: [BaseMethodAttribute](xref:Disharmony.BaseMethodAttribute), [ReturnValueAttribute](xref:Disharmony.ReturnValueAttribute), [PrefixAttribute](xref:Disharmony.PrefixAttribute).

<a id="alwaysrun-postfixes"></a>

## Handle an exception from a call inside a method

`PriceCalculator.GetPrice` in the [target source](#target-source) throws `PriceUnavailableException` when `IsOnline` is false. That exception also prevents `Checkout.Total` from returning a total.

**Goal:** return just the service fee from `Total` when pricing is offline. Leave other calls to `GetPrice` unchanged.

```csharp
[Patch(typeof(Checkout))]
public static class OfflinePricePatches
{
    [Postfix]
    [Target(nameof(Checkout.Total))]
    [Inner(typeof(PriceCalculator), nameof(PriceCalculator.GetPrice),
        typeof(Customer), typeof(int))]
    [PatchOptions(PatchOptions.AlwaysRun)]
    public static void UseFallback(
        [Exception] ref Exception? error,
        [ReturnValue] ref float price)
    {
        if (error is PriceUnavailableException)
        {
            price = 0f;
            error = null;
        }
    }
}
```

```csharp
PatchHandle handle = Patcher.PatchAll(typeof(OfflinePricePatches));
calculator.IsOnline = false;
Console.WriteLine(checkout.Total(calculator, buyer, 2)); // 2: zero item price plus the fee
```

A normal postfix runs only if the patched operation finishes without throwing. `AlwaysRun` makes `UseFallback` run even when the call to `GetPrice` throws. `[Exception]` supplies the exception in `error`.

For `PriceUnavailableException`, the patch sets `price` to 0 and clears `error`. Clearing the exception lets `Total` continue as if `GetPrice` had returned 0. `Total` then adds its fee of 2. When `GetPrice` succeeds, `error` is `null`, so the patch leaves its result unchanged.

The patch suppresses only `PriceUnavailableException`. If pricing is online and the quantity is negative, the target's `ArgumentOutOfRangeException` still propagates. A direct call to an offline calculator still throws as well, because the patch surrounds only calls inside `Total`.

Only suppress exceptions when you know how the method should continue after the failure. Avoid throwing an exception from an `AlwaysRun` patch itself: doing so skips the remaining `AlwaysRun` patches.

Related: [PatchOptions.AlwaysRun](xref:Disharmony.PatchOptions.AlwaysRun), [ExceptionAttribute](xref:Disharmony.ExceptionAttribute), [ReturnValueAttribute](xref:Disharmony.ReturnValueAttribute).

<a id="lambda-targets-and-inlining"></a>

## Patch a lambda

`Checkout.VisibleNames` in the [target source](#target-source) calls `Array.FindAll(names, name => name!.Length > 0)`. `Array.FindAll` passes each name to the lambda and keeps it if the lambda returns `true`. An empty string returns `false`, but a null entry throws when the lambda reads `name.Length`. The `!` silences the compiler's nullability warning; it does not check for null.

**Goal:** fix `VisibleNames` failing on null entries, while preserving its existing rule that empty names are hidden.

```csharp
[Patch(typeof(Checkout))]
public static class NamePatches
{
    [Prefix]
    [Targets("VisibleNames.*", typeof(string))]
    [PatchOptions(PatchOptions.Inline)]
    public static bool IgnoreNull(
        [Argument(0)] string? name,
        [ReturnValue] out bool visible)
    {
        visible = false;
        return name is not null;
    }
}
```

```csharp
PatchHandle handle = Patcher.PatchAll(typeof(NamePatches));
string?[] names = checkout.VisibleNames(new string?[] { null, "", "Tea" });
Console.WriteLine(string.Join(", ", names)); // Tea
```

The compiler puts the lambda's code in a separate method. `"VisibleNames.*"` selects the lambda methods belonging to `VisibleNames`, and `typeof(string)` limits the selection to methods that take a string. `[Argument(0)]` gets the first argument without depending on its generated parameter name.

For a null entry, `IgnoreNull` sets `visible` to `false` and returns `false` to skip the lambda. These two values have different jobs: `visible` tells `Array.FindAll` to discard the entry, while the prefix's return value tells Disharmony to skip the original code. For any other entry, the prefix returns `true`, so the original lambda tests its length. Empty strings are still discarded.

`PatchOptions.Inline` tells Disharmony to copy `IgnoreNull`'s body into the patched lambda method. The null check then runs there, without a separate call to `IgnoreNull`. The prefix still controls whether the original lambda runs. This option controls how Disharmony inserts the patch; the `NoInlining` attributes on the sample targets control the runtime optimization described above.

There is one matching lambda in this example. If you add another string-taking lambda to `VisibleNames`, `[Targets]` will patch that one too. Check which methods match before using the same selector in a larger target.

Related: [TargetsAttribute](xref:Disharmony.TargetsAttribute), [TargetAttribute](xref:Disharmony.TargetAttribute), [ArgumentAttribute](xref:Disharmony.ArgumentAttribute), [PatchOptions.Inline](xref:Disharmony.PatchOptions.Inline).

<a id="fluent-registration"></a>

## Select patch targets in registration code

`Checkout.Estimate` in the [target source](#target-source) returns the item price plus a fee of 2, just like `Total`. Both return 22 for two items.

**Goal:** choose the targets in your registration code and apply the member discount to `Estimate`, leaving `Total` unchanged.

This example uses `SalePricePatches` from [Change a method call's result using values from its caller](#inner-calls-and-scope). Include that patch class, but do not run its `PatchAll` registration. The registration below chooses where to apply its `Discount` method.

```csharp
var outer = typeof(Checkout).GetMethod(nameof(Checkout.Estimate))!;
var inner = typeof(PriceCalculator).GetMethod(nameof(PriceCalculator.GetPrice))!;
var patchMethod = typeof(SalePricePatches).GetMethod(nameof(SalePricePatches.Discount))!;

PatchHandle handle = Patcher.Patch(
    Patch.Postfix.Of(outer).Inner(inner).With(patchMethod)
        .Priority(PatchPriority.Low));

Console.WriteLine(checkout.Estimate(calculator, buyer, 2)); // 20
Console.WriteLine(checkout.Total(calculator, buyer, 2));    // 22, unchanged
```

`GetMethod` looks up each method by name and returns a `MethodInfo` describing it. The registration then uses those objects to say where and how to apply the patch:

| Part of the registration | What it selects |
| --- | --- |
| `Patch.Postfix.Of(outer)` | A postfix within `Checkout.Estimate` |
| `.Inner(inner)` | Calls to `PriceCalculator.GetPrice` inside `Estimate` |
| `.With(patchMethod)` | `SalePricePatches.Discount` as the patch method to run |

The attributes on `Discount` still name `Total`, but this registration chooses `Estimate`. `Patcher.Patch` uses the configuration above instead of the method's `[Target]`, `[Inner]`, and `[Postfix]` attributes. Its parameter attributes still work: the patch receives the checkout, the buyer, and the price in the same way as the first example.

Priority controls the order of patches on the same operation. `.Priority(PatchPriority.Low)` makes this postfix run before postfixes with a higher priority. It has no effect on the result when this is the only patch. Omit the priority call to use the default.

Related: [Patch](xref:Disharmony.Patch), [PatchConfig](xref:Disharmony.PatchConfig), [Patcher](xref:Disharmony.Patcher), [PatchPriority](xref:Disharmony.PatchPriority). See [reading extension members](extension-members.md) for how this fluent syntax corresponds to generated API signatures.
