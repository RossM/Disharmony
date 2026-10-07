namespace Disharmony.Tests.EndToEnd.ParameterBinding;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class UnsupportedParameterBindingAttribute() : ParameterBindingAttribute(Scope.Any);

public static class ParameterBindingValidationPatches
{
    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntArgument))]
    public static void Prefix_UnsupportedParameterBindingAttribute_RejectedByPatch(
        [UnsupportedParameterBinding] int value) { }

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntArgument))]
    public static void Prefix_InvalidScope_RejectedByPatch(
        [Argument((Scope)int.MaxValue)] int value) { }

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntArgument))]
    public static void Prefix_MultipleParameterBindingAttributes_RejectedByPatch(
        [Argument] [Field("primitiveField")] int value) { }

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntArgument))]
    public static void Prefix_ParameterAttribute_IndexTooLarge_RejectedByPatch(
        [Argument(1)] int value) { }

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntArgument))]
    public static void Prefix_ParameterAttribute_NegativeIndex_RejectedByPatch(
        [Argument(-1)] int value) { }

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntArgument))]
    public static void Prefix_ParameterAttribute_MissingName_RejectedByPatch(
        [Argument("missing")] int value) { }

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_FieldAttribute_MissingName_RejectedByPatch(
        [Field("missing")] int value) { }

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Prefix_ReturnValueAttribute_VoidTarget_RejectedByPatch(
        [ReturnValue] int result) { }

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Prefix_InstanceAttribute_StaticTarget_RejectedByPatch(
        [Instance] object instance) { }

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_CallerParameter_OuterPatch_RejectedByPatch(ClassMethodTargets __caller) { }

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.EnumerateIntResult))]
    public static void IteratorInnerPrefix_InstanceAttribute_StaticOuterTarget_RejectedByPatch(
        [Instance(Scope.Outer)] object instance) { }

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.EnumerateIntResult))]
    public static void IteratorInnerPrefix_ParameterAttribute_MissingOuterParameter_RejectedByPatch(
        [Argument("missing", Scope.Outer)] int value) { }
}

[TestFixture]
public sealed class ParameterBindingValidationTests : PatchTestBase
{
    [Test]
    public void Prefix_UnsupportedParameterBindingAttribute_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(ParameterBindingValidationPatches),
            nameof(ParameterBindingValidationPatches.Prefix_UnsupportedParameterBindingAttribute_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<NotSupportedException>());
    }

    [Test]
    public void Prefix_InvalidScope_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(ParameterBindingValidationPatches),
            nameof(ParameterBindingValidationPatches.Prefix_InvalidScope_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Prefix_MultipleParameterBindingAttributes_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(ParameterBindingValidationPatches),
            nameof(ParameterBindingValidationPatches.Prefix_MultipleParameterBindingAttributes_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void Prefix_ParameterAttribute_IndexTooLarge_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(ParameterBindingValidationPatches),
            nameof(ParameterBindingValidationPatches.Prefix_ParameterAttribute_IndexTooLarge_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void Prefix_ParameterAttribute_NegativeIndex_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(ParameterBindingValidationPatches),
            nameof(ParameterBindingValidationPatches.Prefix_ParameterAttribute_NegativeIndex_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void Prefix_ParameterAttribute_MissingName_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(ParameterBindingValidationPatches),
            nameof(ParameterBindingValidationPatches.Prefix_ParameterAttribute_MissingName_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("value: Parameter not found"));
    }

    [Test]
    public void Prefix_FieldAttribute_MissingName_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(ParameterBindingValidationPatches),
            nameof(ParameterBindingValidationPatches.Prefix_FieldAttribute_MissingName_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("value: Field not found"));
    }

    [Test]
    public void Prefix_ReturnValueAttribute_VoidTarget_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(ParameterBindingValidationPatches),
            nameof(ParameterBindingValidationPatches.Prefix_ReturnValueAttribute_VoidTarget_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("result: Method returns void"));
    }

    [Test]
    public void Prefix_InstanceAttribute_StaticTarget_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(ParameterBindingValidationPatches),
            nameof(ParameterBindingValidationPatches.Prefix_InstanceAttribute_StaticTarget_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("instance: Method is static"));
    }

    [Test]
    public void Prefix_CallerParameter_OuterPatch_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(ParameterBindingValidationPatches),
            nameof(ParameterBindingValidationPatches.Prefix_CallerParameter_OuterPatch_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("__caller: Can only be used with inner patches"));
    }

    [Test]
    public void IteratorInnerPrefix_InstanceAttribute_StaticOuterTarget_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(ParameterBindingValidationPatches),
            nameof(ParameterBindingValidationPatches.IteratorInnerPrefix_InstanceAttribute_StaticOuterTarget_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("instance: Method is static"));
    }

    [Test]
    public void IteratorInnerPrefix_ParameterAttribute_MissingOuterParameter_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(ParameterBindingValidationPatches),
            nameof(ParameterBindingValidationPatches.IteratorInnerPrefix_ParameterAttribute_MissingOuterParameter_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("value: Parameter not found"));
    }
}
