namespace Disharmony.Tests.EndToEnd.ParameterBinding;

public static class ReadonlyReferenceBindingPatches
{
    public static int ValueObserved;
    public static ClassMethodTargets? CallerObserved;
    public static InnerInstanceMethodTargets? InnerInstanceObserved;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntIdentity))]
    public static void Postfix_Parameter_Primitive_ReadByReadonlyReference(in int value) =>
        ValueObserved = value;

    [Prefix]
    [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Void))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.OuterArgument))]
    public static void InnerPrefix_OuterParameter_Primitive_ReadByReadonlyReference(in int outerValue) =>
        ValueObserved = outerValue;

    [Postfix]
    [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntArgument))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.IntArgument))]
    public static void InnerPostfix_InnerParameter_Primitive_ReadByReadonlyReference(in int value) =>
        ValueObserved = value;

    [Postfix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Postfix_Instance_ReferenceType_ReadByReadonlyReference(in ClassMethodTargets __instance) =>
        CallerObserved = __instance;

    [Postfix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Postfix_Instance_ReferenceType_MutateReferentThroughReadonlyReference(
        in ClassMethodTargets __instance) =>
        __instance.foo = 42;

    [Prefix]
    [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallStaticVoid))]
    public static void InnerPrefix_Caller_ReferenceType_ReadByReadonlyReference(in ClassMethodTargets __caller) =>
        CallerObserved = __caller;

    [Postfix]
    [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallInnerWithField))]
    public static void InnerPostfix_InnerInstance_ReferenceType_ReadByReadonlyReference(
        [Instance(Scope.Inner)] in InnerInstanceMethodTargets instance) =>
        InnerInstanceObserved = instance;

    [Prefix]
    [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateIdentity))]
    public static void IteratorInnerPrefix_OuterInstance_ReferenceType_ReadByReadonlyReference(
        [Instance(Scope.Outer)] in ClassMethodTargets instance) =>
        CallerObserved = instance;
}

[TestFixture]
public sealed class ReadonlyReferenceBindingTests : PatchTestBase
{
    [Test]
    public void Postfix_Parameter_Primitive_ReadByReadonlyReference()
    {
        ReadonlyReferenceBindingPatches.ValueObserved = 0;
        ApplyPatch(
            typeof(ReadonlyReferenceBindingPatches),
            nameof(ReadonlyReferenceBindingPatches.Postfix_Parameter_Primitive_ReadByReadonlyReference));

        int result = StaticMethodTargets.IntIdentity(42);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(42));
            Assert.That(ReadonlyReferenceBindingPatches.ValueObserved, Is.EqualTo(42));
        });
    }

    [Test]
    public void InnerPrefix_OuterParameter_Primitive_ReadByReadonlyReference()
    {
        ReadonlyReferenceBindingPatches.ValueObserved = 0;
        ApplyPatch(
            typeof(ReadonlyReferenceBindingPatches),
            nameof(ReadonlyReferenceBindingPatches.InnerPrefix_OuterParameter_Primitive_ReadByReadonlyReference));

        OuterStaticMethodTargets.OuterArgument(42);

        Assert.That(ReadonlyReferenceBindingPatches.ValueObserved, Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfix_InnerParameter_Primitive_ReadByReadonlyReference()
    {
        ReadonlyReferenceBindingPatches.ValueObserved = 0;
        ApplyPatch(
            typeof(ReadonlyReferenceBindingPatches),
            nameof(ReadonlyReferenceBindingPatches.InnerPostfix_InnerParameter_Primitive_ReadByReadonlyReference));

        OuterStaticMethodTargets.IntArgument(42);

        Assert.That(ReadonlyReferenceBindingPatches.ValueObserved, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_Instance_ReferenceType_ReadByReadonlyReference()
    {
        ReadonlyReferenceBindingPatches.CallerObserved = null;
        ApplyPatch(
            typeof(ReadonlyReferenceBindingPatches),
            nameof(ReadonlyReferenceBindingPatches.Postfix_Instance_ReferenceType_ReadByReadonlyReference));
        ClassMethodTargets target = new();

        target.Void();

        Assert.That(ReadonlyReferenceBindingPatches.CallerObserved, Is.SameAs(target));
    }

    [Test]
    public void Postfix_Instance_ReferenceType_MutateReferentThroughReadonlyReference()
    {
        ApplyPatch(
            typeof(ReadonlyReferenceBindingPatches),
            nameof(ReadonlyReferenceBindingPatches.Postfix_Instance_ReferenceType_MutateReferentThroughReadonlyReference));
        ClassMethodTargets target = new() { foo = 1 };

        target.Void();

        Assert.That(target.foo, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_Caller_ReferenceType_ReadByReadonlyReference()
    {
        ReadonlyReferenceBindingPatches.CallerObserved = null;
        ApplyPatch(
            typeof(ReadonlyReferenceBindingPatches),
            nameof(ReadonlyReferenceBindingPatches.InnerPrefix_Caller_ReferenceType_ReadByReadonlyReference));
        ClassMethodTargets target = new();

        target.CallStaticVoid();

        Assert.That(ReadonlyReferenceBindingPatches.CallerObserved, Is.SameAs(target));
    }

    [Test]
    public void InnerPostfix_InnerInstance_ReferenceType_ReadByReadonlyReference()
    {
        ReadonlyReferenceBindingPatches.InnerInstanceObserved = null;
        ApplyPatch(
            typeof(ReadonlyReferenceBindingPatches),
            nameof(ReadonlyReferenceBindingPatches.InnerPostfix_InnerInstance_ReferenceType_ReadByReadonlyReference));
        ClassMethodTargets target = new();
        InnerInstanceMethodTargets inner = new() { foo = 42 };

        target.CallInnerWithField(inner);

        Assert.That(ReadonlyReferenceBindingPatches.InnerInstanceObserved, Is.SameAs(inner));
    }

    [Test]
    public void IteratorInnerPrefix_OuterInstance_ReferenceType_ReadByReadonlyReference()
    {
        ReadonlyReferenceBindingPatches.CallerObserved = null;
        ApplyPatch(
            typeof(ReadonlyReferenceBindingPatches),
            nameof(ReadonlyReferenceBindingPatches.IteratorInnerPrefix_OuterInstance_ReferenceType_ReadByReadonlyReference));
        ClassMethodTargets target = new() { foo = 1 };

        int result = target.EnumerateIdentity(42).Single();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(42));
            Assert.That(ReadonlyReferenceBindingPatches.CallerObserved, Is.SameAs(target));
        });
    }
}
