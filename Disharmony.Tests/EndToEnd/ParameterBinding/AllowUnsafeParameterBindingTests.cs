namespace Disharmony.Tests.EndToEnd.ParameterBinding;

public static class AllowUnsafeParameterBindingPatches
{
    [Prefix]
    [PatchOptions(PatchOptions.AllowUnsafe)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StringArgument))]
    public static void Prefix_Argument_ReferenceType_UnrelatedType_ByValue_AllowUnsafe_Accepted(
        BindingReference value) { }

    [Prefix]
    [PatchOptions(PatchOptions.AllowUnsafe)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StringArgument))]
    public static void Prefix_Argument_ReferenceType_UnrelatedType_ByReadonlyReference_AllowUnsafe_Accepted(
        in BindingReference value) { }

    [Prefix]
    [PatchOptions(PatchOptions.AllowUnsafe)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StringArgument))]
    public static void Prefix_Argument_ReferenceType_UnrelatedType_ByOutReference_AllowUnsafe_Accepted(
        out BindingReference value) => value = null!;

    [Prefix]
    [PatchOptions(PatchOptions.AllowUnsafe)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StringArgument))]
    public static void Prefix_Argument_ReferenceType_UnrelatedType_ByWritableReference_AllowUnsafe_Accepted(
        ref BindingReference value) { }

    [Postfix]
    [PatchOptions(PatchOptions.AllowUnsafe)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StringResult))]
    public static void Postfix_ReturnValue_ReferenceType_UnrelatedType_ByWritableReference_AllowUnsafe_Accepted(
        ref BindingReference __result) { }

    [Prefix]
    [PatchOptions(PatchOptions.AllowUnsafe)]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_Instance_ReferenceType_UnrelatedType_ByValue_AllowUnsafe_Accepted(
        [Instance] BindingReference instance) { }

    [Prefix]
    [PatchOptions(PatchOptions.AllowUnsafe)]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_Field_ReferenceType_UnrelatedType_ByWritableReference_AllowUnsafe_Accepted(
        [Field(nameof(ClassMethodTargets.referenceField))] ref string value) { }

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun | PatchOptions.AllowUnsafe)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_Exception_ReferenceType_UnrelatedType_ByValue_AllowUnsafe_Accepted(
        [Exception] BindingReference exception) { }

    [Prefix]
    [PatchOptions(PatchOptions.AllowUnsafe)]
    [Target(typeof(LocalFunctionTargets), "CapturedReferenceVariableMethod.LocalMethod")]
    public static void Prefix_CapturedVariable_ReferenceType_UnrelatedType_ByWritableReference_AllowUnsafe_Accepted(
        ref string captured) { }

    [Postfix]
    [PatchOptions(PatchOptions.AllowUnsafe)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntArgument))]
    public static void Postfix_OuterArgument_Primitive_ByWritableReference_AllowUnsafe_Accepted(ref int value) { }

    [Prefix]
    [PatchOptions(PatchOptions.AllowUnsafe)]
    [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Void))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.OuterArgument))]
    public static void InnerPrefix_OuterArgument_Primitive_ByWritableReference_AllowUnsafe_Accepted(
        ref int outerValue) { }

    [Postfix]
    [PatchOptions(PatchOptions.AllowUnsafe)]
    [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Void))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.OuterArgument))]
    public static void InnerPostfix_OuterArgument_Primitive_ByWritableReference_AllowUnsafe_Accepted(
        ref int outerValue) { }

    [Postfix]
    [PatchOptions(PatchOptions.AllowUnsafe)]
    [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntArgument))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.IntArgument))]
    public static void InnerPostfix_InnerArgument_Primitive_ByWritableReference_AllowUnsafe_Accepted(ref int value) { }

    [Postfix]
    [PatchOptions(PatchOptions.AllowUnsafe)]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Postfix_Instance_ReferenceType_ByWritableReference_AllowUnsafe_Accepted(
        ref ClassMethodTargets __instance) { }

    [Prefix]
    [PatchOptions(PatchOptions.AllowUnsafe)]
    [Target(typeof(MethodBindingStructTargets), nameof(MethodBindingStructTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_MutableStructInstanceMethod_AllowUnsafe_Accepted(
        [Method(nameof(MethodBindingStructTargets.BoundInstanceMethod))] Func<int, int> method) { }
}

[TestFixture]
public sealed class AllowUnsafeParameterBindingTests : PatchTestBase
{
    [Test]
    public void Prefix_Argument_ReferenceType_UnrelatedType_ByValue_AllowUnsafe_Accepted()
    {
        Assert.DoesNotThrow(() => ApplyPatch(
            typeof(AllowUnsafeParameterBindingPatches),
            nameof(AllowUnsafeParameterBindingPatches
                .Prefix_Argument_ReferenceType_UnrelatedType_ByValue_AllowUnsafe_Accepted)));
    }

    [Test]
    public void Prefix_Argument_ReferenceType_UnrelatedType_ByReadonlyReference_AllowUnsafe_Accepted()
    {
        Assert.DoesNotThrow(() => ApplyPatch(
            typeof(AllowUnsafeParameterBindingPatches),
            nameof(AllowUnsafeParameterBindingPatches
                .Prefix_Argument_ReferenceType_UnrelatedType_ByReadonlyReference_AllowUnsafe_Accepted)));
    }

    [Test]
    public void Prefix_Argument_ReferenceType_UnrelatedType_ByOutReference_AllowUnsafe_Accepted()
    {
        Assert.DoesNotThrow(() => ApplyPatch(
            typeof(AllowUnsafeParameterBindingPatches),
            nameof(AllowUnsafeParameterBindingPatches
                .Prefix_Argument_ReferenceType_UnrelatedType_ByOutReference_AllowUnsafe_Accepted)));
    }

    [Test]
    public void Prefix_Argument_ReferenceType_UnrelatedType_ByWritableReference_AllowUnsafe_Accepted()
    {
        Assert.DoesNotThrow(() => ApplyPatch(
            typeof(AllowUnsafeParameterBindingPatches),
            nameof(AllowUnsafeParameterBindingPatches
                .Prefix_Argument_ReferenceType_UnrelatedType_ByWritableReference_AllowUnsafe_Accepted)));
    }

    [Test]
    public void Postfix_ReturnValue_ReferenceType_UnrelatedType_ByWritableReference_AllowUnsafe_Accepted()
    {
        Assert.DoesNotThrow(() => ApplyPatch(
            typeof(AllowUnsafeParameterBindingPatches),
            nameof(AllowUnsafeParameterBindingPatches
                .Postfix_ReturnValue_ReferenceType_UnrelatedType_ByWritableReference_AllowUnsafe_Accepted)));
    }

    [Test]
    public void Prefix_Instance_ReferenceType_UnrelatedType_ByValue_AllowUnsafe_Accepted()
    {
        Assert.DoesNotThrow(() => ApplyPatch(
            typeof(AllowUnsafeParameterBindingPatches),
            nameof(AllowUnsafeParameterBindingPatches
                .Prefix_Instance_ReferenceType_UnrelatedType_ByValue_AllowUnsafe_Accepted)));
    }

    [Test]
    public void Prefix_Field_ReferenceType_UnrelatedType_ByWritableReference_AllowUnsafe_Accepted()
    {
        Assert.DoesNotThrow(() => ApplyPatch(
            typeof(AllowUnsafeParameterBindingPatches),
            nameof(AllowUnsafeParameterBindingPatches
                .Prefix_Field_ReferenceType_UnrelatedType_ByWritableReference_AllowUnsafe_Accepted)));
    }

    [Test]
    public void Postfix_Exception_ReferenceType_UnrelatedType_ByValue_AllowUnsafe_Accepted()
    {
        Assert.DoesNotThrow(() => ApplyPatch(
            typeof(AllowUnsafeParameterBindingPatches),
            nameof(AllowUnsafeParameterBindingPatches
                .Postfix_Exception_ReferenceType_UnrelatedType_ByValue_AllowUnsafe_Accepted)));
    }

    [Test]
    public void Prefix_CapturedVariable_ReferenceType_UnrelatedType_ByWritableReference_AllowUnsafe_Accepted()
    {
        Assert.DoesNotThrow(() => ApplyPatch(
            typeof(AllowUnsafeParameterBindingPatches),
            nameof(AllowUnsafeParameterBindingPatches
                .Prefix_CapturedVariable_ReferenceType_UnrelatedType_ByWritableReference_AllowUnsafe_Accepted)));
    }

    [Test]
    public void Postfix_OuterArgument_Primitive_ByWritableReference_AllowUnsafe_Accepted()
    {
        Assert.DoesNotThrow(() => ApplyPatch(
            typeof(AllowUnsafeParameterBindingPatches),
            nameof(AllowUnsafeParameterBindingPatches
                .Postfix_OuterArgument_Primitive_ByWritableReference_AllowUnsafe_Accepted)));
    }

    [Test]
    public void InnerPrefix_OuterArgument_Primitive_ByWritableReference_AllowUnsafe_Accepted()
    {
        Assert.DoesNotThrow(() => ApplyPatch(
            typeof(AllowUnsafeParameterBindingPatches),
            nameof(AllowUnsafeParameterBindingPatches
                .InnerPrefix_OuterArgument_Primitive_ByWritableReference_AllowUnsafe_Accepted)));
    }

    [Test]
    public void InnerPostfix_OuterArgument_Primitive_ByWritableReference_AllowUnsafe_Accepted()
    {
        Assert.DoesNotThrow(() => ApplyPatch(
            typeof(AllowUnsafeParameterBindingPatches),
            nameof(AllowUnsafeParameterBindingPatches
                .InnerPostfix_OuterArgument_Primitive_ByWritableReference_AllowUnsafe_Accepted)));
    }

    [Test]
    public void InnerPostfix_InnerArgument_Primitive_ByWritableReference_AllowUnsafe_Accepted()
    {
        Assert.DoesNotThrow(() => ApplyPatch(
            typeof(AllowUnsafeParameterBindingPatches),
            nameof(AllowUnsafeParameterBindingPatches
                .InnerPostfix_InnerArgument_Primitive_ByWritableReference_AllowUnsafe_Accepted)));
    }

    [Test]
    public void Postfix_Instance_ReferenceType_ByWritableReference_AllowUnsafe_Accepted()
    {
        Assert.DoesNotThrow(() => ApplyPatch(
            typeof(AllowUnsafeParameterBindingPatches),
            nameof(AllowUnsafeParameterBindingPatches
                .Postfix_Instance_ReferenceType_ByWritableReference_AllowUnsafe_Accepted)));
    }

    [Test]
    public void Prefix_MethodAttribute_MutableStructInstanceMethod_AllowUnsafe_Accepted()
    {
        Assert.DoesNotThrow(() => ApplyPatch(
            typeof(AllowUnsafeParameterBindingPatches),
            nameof(AllowUnsafeParameterBindingPatches
                .Prefix_MethodAttribute_MutableStructInstanceMethod_AllowUnsafe_Accepted)));
    }
}
