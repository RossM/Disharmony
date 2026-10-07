namespace Disharmony.Tests.EndToEnd.ParameterBinding;

public static partial class StateBindingPatches
{
    public static int observed;
    public static int secondaryObserved;
    public static string? referenceObserved;
    public static BindingStruct structObserved;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_Primitive_ReadByValue_Prefix(out int __state) => __state = 42;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_Primitive_ReadByValue_Postfix(int __state) => observed = __state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_Primitive_WriteByReference_Prefix(out int __state) => __state = 42;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_Primitive_WriteByReference_FirstPostfix(ref int __state) => __state = 43;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_Primitive_WriteByReference_SecondPostfix(int __state) => observed = __state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_ReferenceType_ReadByValue_Prefix(out string __state) => __state = "original";

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_ReferenceType_ReadByValue_Postfix(string __state) => referenceObserved = __state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_ReferenceType_WriteByReference_Prefix(out string __state) =>
        __state = "original";

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_ReferenceType_WriteByReference_FirstPostfix(ref string __state) =>
        __state = "patched";

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_ReferenceType_WriteByReference_SecondPostfix(string __state) =>
        referenceObserved = __state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_Struct_ReadByValue_Prefix(out BindingStruct __state) =>
        __state = new BindingStruct { Value = 42 };

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_Struct_ReadByValue_Postfix(BindingStruct __state) => structObserved = __state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_Struct_WriteByReference_Prefix(out BindingStruct __state) =>
        __state = new BindingStruct { Value = 1 };

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_Struct_WriteByReference_FirstPostfix(ref BindingStruct __state) =>
        __state = new BindingStruct { Value = 42 };

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_Struct_WriteByReference_SecondPostfix(BindingStruct __state) =>
        structObserved = __state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Primitive_ReadByValue_Prefix([State] out int state) => state = 42;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Primitive_ReadByValue_Postfix([State] int state) => observed = state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Primitive_WriteByReference_Prefix([State] out int state) => state = 1;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Primitive_WriteByReference_FirstPostfix([State] ref int state) => state = 42;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Primitive_WriteByReference_SecondPostfix([State] int state) => observed = state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_ReferenceType_ReadByValue_Prefix([State] out string state) => state = "original";

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_ReferenceType_ReadByValue_Postfix([State] string state) => referenceObserved = state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_ReferenceType_WriteByReference_Prefix([State] out string state) =>
        state = "original";

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_ReferenceType_WriteByReference_FirstPostfix([State] ref string state) =>
        state = "patched";

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_ReferenceType_WriteByReference_SecondPostfix([State] string state) =>
        referenceObserved = state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Struct_ReadByValue_Prefix([State] out BindingStruct state) =>
        state = new BindingStruct { Value = 42 };

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Struct_ReadByValue_Postfix([State] BindingStruct state) => structObserved = state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Struct_WriteByReference_Prefix([State] out BindingStruct state) =>
        state = new BindingStruct { Value = 1 };

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Struct_WriteByReference_FirstPostfix([State] ref BindingStruct state) =>
        state = new BindingStruct { Value = 42 };

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Struct_WriteByReference_SecondPostfix([State] BindingStruct state) =>
        structObserved = state;
}

public static partial class StateBindingPatches
{
    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_Primitive_ReadByReference_Prefix(out int __state) => __state = 42;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_Primitive_ReadByReference_Postfix(ref int __state) => observed = __state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_ReferenceType_ReadByReference_Prefix(out string __state) => __state = "original";

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_ReferenceType_ReadByReference_Postfix(ref string __state) =>
        referenceObserved = __state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_Struct_ReadByReference_Prefix(out BindingStruct __state) =>
        __state = new BindingStruct { Value = 42 };

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateParameter_Struct_ReadByReference_Postfix(ref BindingStruct __state) => structObserved = __state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Primitive_ReadByReference_Prefix([State] out int state) => state = 42;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Primitive_ReadByReference_Postfix([State] ref int state) => observed = state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_ReferenceType_ReadByReference_Prefix([State] out string state) =>
        state = "original";

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_ReferenceType_ReadByReference_Postfix([State] ref string state) =>
        referenceObserved = state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Struct_ReadByReference_Prefix([State] out BindingStruct state) =>
        state = new BindingStruct { Value = 42 };

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Struct_ReadByReference_Postfix([State] ref BindingStruct state) =>
        structObserved = state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void StateSharing_SamePatchCall_SameType_ImplicitAndAttribute_Shares_Prefix(out int __state) =>
        __state = 42;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void StateSharing_SamePatchCall_SameType_ImplicitAndAttribute_Shares_Postfix([State("__state")] int state) =>
        observed = state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void StateSharing_SamePatchCall_DifferentClasses_SameType_SameExplicitKey_Shares_Prefix(
        [State("shared")] out int state) =>
        state = 42;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void StateSharing_SamePatchCall_SameType_DifferentExplicitKeys_Separate_Prefix(
        [State("first")] out int first,
        [State("second")] out int second)
    {
        first = 41;
        second = 42;
    }

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void StateSharing_SamePatchCall_SameType_DifferentExplicitKeys_Separate_Postfix(
        [State("first")] int first,
        [State("second")] int second)
    {
        observed = first;
        secondaryObserved = second;
    }

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void StateSharing_SamePatchCall_SameType_KeyedAndUnkeyed_Separate_Prefix(
        [State] out int unkeyed,
        [State("keyed")] out int keyed)
    {
        unkeyed = 41;
        keyed = 42;
    }

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void StateSharing_SamePatchCall_SameType_KeyedAndUnkeyed_Separate_Postfix(
        [State] int unkeyed,
        [State("keyed")] int keyed)
    {
        observed = unkeyed;
        secondaryObserved = keyed;
    }

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void StateSharing_SamePatchCall_DifferentTypes_Unkeyed_Separate_Prefix(
        [State] out int primitive,
        [State] out string reference)
    {
        primitive = 42;
        reference = "state";
    }

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void StateSharing_SamePatchCall_DifferentTypes_Unkeyed_Separate_Postfix(
        [State] int primitive,
        [State] string reference)
    {
        observed = primitive;
        referenceObserved = reference;
    }

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void StateSharing_DifferentPatchCalls_SameType_Unkeyed_Separate_Prefix(out int __state) =>
        __state = 42;
}

public static class OtherStateBindingPatches
{
    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void StateSharing_SamePatchCall_DifferentClasses_SameType_SameExplicitKey_Shares_Postfix(
        [State("shared")] int state) =>
        StateBindingPatches.observed = state;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void StateSharing_DifferentPatchCalls_SameType_Unkeyed_Separate_Postfix(int __state) =>
        StateBindingPatches.observed = __state;
}

[TestFixture]
public sealed partial class StateBindingTests
{
    [Test]
    public void Postfix_StateParameter_Primitive_ReadByReference()
    {
        StateBindingPatches.observed = 0;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_Primitive_ReadByReference_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_Primitive_ReadByReference_Postfix))!);
        StaticMethodTargets.Void();
        Assert.That(StateBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_StateParameter_ReferenceType_ReadByReference()
    {
        StateBindingPatches.referenceObserved = null;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_ReferenceType_ReadByReference_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_ReferenceType_ReadByReference_Postfix))!);
        StaticMethodTargets.Void();
        Assert.That(StateBindingPatches.referenceObserved, Is.EqualTo("original"));
    }

    [Test]
    public void Postfix_StateParameter_Struct_ReadByReference()
    {
        StateBindingPatches.structObserved = default;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_Struct_ReadByReference_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_Struct_ReadByReference_Postfix))!);
        StaticMethodTargets.Void();
        Assert.That(StateBindingPatches.structObserved.Value, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_StateAttribute_Primitive_ReadByReference()
    {
        StateBindingPatches.observed = 0;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_Primitive_ReadByReference_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_Primitive_ReadByReference_Postfix))!);
        StaticMethodTargets.Void();
        Assert.That(StateBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_StateAttribute_ReferenceType_ReadByReference()
    {
        StateBindingPatches.referenceObserved = null;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_ReferenceType_ReadByReference_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_ReferenceType_ReadByReference_Postfix))!);
        StaticMethodTargets.Void();
        Assert.That(StateBindingPatches.referenceObserved, Is.EqualTo("original"));
    }

    [Test]
    public void Postfix_StateAttribute_Struct_ReadByReference()
    {
        StateBindingPatches.structObserved = default;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_Struct_ReadByReference_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_Struct_ReadByReference_Postfix))!);
        StaticMethodTargets.Void();
        Assert.That(StateBindingPatches.structObserved.Value, Is.EqualTo(42));
    }
}

[TestFixture]
public sealed partial class StateBindingTests : PatchTestBase
{
    [Test]
    public void Postfix_StateParameter_Primitive_ReadByValue()
    {
        StateBindingPatches.observed = 0;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_Primitive_ReadByValue_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_Primitive_ReadByValue_Postfix))!);

        StaticMethodTargets.Void();

        Assert.That(StateBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_StateParameter_Primitive_WriteByReference()
    {
        StateBindingPatches.observed = 0;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_Primitive_WriteByReference_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_Primitive_WriteByReference_FirstPostfix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_Primitive_WriteByReference_SecondPostfix))!);

        StaticMethodTargets.Void();

        Assert.That(StateBindingPatches.observed, Is.EqualTo(43));
    }

    [Test]
    public void Postfix_StateParameter_ReferenceType_ReadByValue()
    {
        StateBindingPatches.referenceObserved = null;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_ReferenceType_ReadByValue_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_ReferenceType_ReadByValue_Postfix))!);

        StaticMethodTargets.Void();

        Assert.That(StateBindingPatches.referenceObserved, Is.EqualTo("original"));
    }

    [Test]
    public void Postfix_StateParameter_ReferenceType_WriteByReference()
    {
        StateBindingPatches.referenceObserved = null;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_ReferenceType_WriteByReference_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_ReferenceType_WriteByReference_FirstPostfix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_ReferenceType_WriteByReference_SecondPostfix))!);

        StaticMethodTargets.Void();

        Assert.That(StateBindingPatches.referenceObserved, Is.EqualTo("patched"));
    }

    [Test]
    public void Postfix_StateParameter_Struct_ReadByValue()
    {
        StateBindingPatches.structObserved = default;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_Struct_ReadByValue_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_Struct_ReadByValue_Postfix))!);

        StaticMethodTargets.Void();

        Assert.That(StateBindingPatches.structObserved.Value, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_StateParameter_Struct_WriteByReference()
    {
        StateBindingPatches.structObserved = default;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_Struct_WriteByReference_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_Struct_WriteByReference_FirstPostfix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateParameter_Struct_WriteByReference_SecondPostfix))!);

        StaticMethodTargets.Void();

        Assert.That(StateBindingPatches.structObserved.Value, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_StateAttribute_Primitive_ReadByValue()
    {
        StateBindingPatches.observed = 0;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_Primitive_ReadByValue_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_Primitive_ReadByValue_Postfix))!);

        StaticMethodTargets.Void();

        Assert.That(StateBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_StateAttribute_Primitive_WriteByReference()
    {
        StateBindingPatches.observed = 0;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_Primitive_WriteByReference_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_Primitive_WriteByReference_FirstPostfix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_Primitive_WriteByReference_SecondPostfix))!);

        StaticMethodTargets.Void();

        Assert.That(StateBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_StateAttribute_ReferenceType_ReadByValue()
    {
        StateBindingPatches.referenceObserved = null;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_ReferenceType_ReadByValue_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_ReferenceType_ReadByValue_Postfix))!);

        StaticMethodTargets.Void();

        Assert.That(StateBindingPatches.referenceObserved, Is.EqualTo("original"));
    }

    [Test]
    public void Postfix_StateAttribute_ReferenceType_WriteByReference()
    {
        StateBindingPatches.referenceObserved = null;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_ReferenceType_WriteByReference_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_ReferenceType_WriteByReference_FirstPostfix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_ReferenceType_WriteByReference_SecondPostfix))!);

        StaticMethodTargets.Void();

        Assert.That(StateBindingPatches.referenceObserved, Is.EqualTo("patched"));
    }

    [Test]
    public void Postfix_StateAttribute_Struct_ReadByValue()
    {
        StateBindingPatches.structObserved = default;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_Struct_ReadByValue_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_Struct_ReadByValue_Postfix))!);

        StaticMethodTargets.Void();

        Assert.That(StateBindingPatches.structObserved.Value, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_StateAttribute_Struct_WriteByReference()
    {
        StateBindingPatches.structObserved = default;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_Struct_WriteByReference_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_Struct_WriteByReference_FirstPostfix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.Postfix_StateAttribute_Struct_WriteByReference_SecondPostfix))!);

        StaticMethodTargets.Void();

        Assert.That(StateBindingPatches.structObserved.Value, Is.EqualTo(42));
    }

    [Test]
    public void StateSharing_SamePatchCall_SameType_ImplicitAndAttribute_Shares()
    {
        StateBindingPatches.observed = 0;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.StateSharing_SamePatchCall_SameType_ImplicitAndAttribute_Shares_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.StateSharing_SamePatchCall_SameType_ImplicitAndAttribute_Shares_Postfix))!);

        StaticMethodTargets.Void();

        Assert.That(StateBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void StateSharing_SamePatchCall_DifferentClasses_SameType_SameExplicitKey_Shares()
    {
        StateBindingPatches.observed = 0;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.StateSharing_SamePatchCall_DifferentClasses_SameType_SameExplicitKey_Shares_Prefix))!,
            typeof(OtherStateBindingPatches).GetMethod(
                nameof(OtherStateBindingPatches.StateSharing_SamePatchCall_DifferentClasses_SameType_SameExplicitKey_Shares_Postfix))!);

        StaticMethodTargets.Void();

        Assert.That(StateBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void StateSharing_SamePatchCall_SameType_DifferentExplicitKeys_Separate()
    {
        StateBindingPatches.observed = 0;
        StateBindingPatches.secondaryObserved = 0;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.StateSharing_SamePatchCall_SameType_DifferentExplicitKeys_Separate_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.StateSharing_SamePatchCall_SameType_DifferentExplicitKeys_Separate_Postfix))!);

        StaticMethodTargets.Void();

        Assert.That(StateBindingPatches.observed, Is.EqualTo(41));
        Assert.That(StateBindingPatches.secondaryObserved, Is.EqualTo(42));
    }

    [Test]
    public void StateSharing_SamePatchCall_SameType_KeyedAndUnkeyed_Separate()
    {
        StateBindingPatches.observed = 0;
        StateBindingPatches.secondaryObserved = 0;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.StateSharing_SamePatchCall_SameType_KeyedAndUnkeyed_Separate_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.StateSharing_SamePatchCall_SameType_KeyedAndUnkeyed_Separate_Postfix))!);

        StaticMethodTargets.Void();

        Assert.That(StateBindingPatches.observed, Is.EqualTo(41));
        Assert.That(StateBindingPatches.secondaryObserved, Is.EqualTo(42));
    }

    [Test]
    public void StateSharing_SamePatchCall_DifferentTypes_Unkeyed_Separate()
    {
        StateBindingPatches.observed = 0;
        StateBindingPatches.referenceObserved = null;
        Patcher.Patch(
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.StateSharing_SamePatchCall_DifferentTypes_Unkeyed_Separate_Prefix))!,
            typeof(StateBindingPatches).GetMethod(
                nameof(StateBindingPatches.StateSharing_SamePatchCall_DifferentTypes_Unkeyed_Separate_Postfix))!);

        StaticMethodTargets.Void();

        Assert.That(StateBindingPatches.observed, Is.EqualTo(42));
        Assert.That(StateBindingPatches.referenceObserved, Is.EqualTo("state"));
    }

    [Test]
    public void StateSharing_DifferentPatchCalls_SameType_Unkeyed_Separate()
    {
        StateBindingPatches.observed = -1;
        // Each public patching call creates a distinct state-sharing group.
        ApplyPatch(typeof(StateBindingPatches),
            nameof(StateBindingPatches.StateSharing_DifferentPatchCalls_SameType_Unkeyed_Separate_Prefix));
        ApplyPatch(typeof(OtherStateBindingPatches),
            nameof(OtherStateBindingPatches.StateSharing_DifferentPatchCalls_SameType_Unkeyed_Separate_Postfix));

        StaticMethodTargets.Void();

        Assert.That(StateBindingPatches.observed, Is.Zero);
    }
}
