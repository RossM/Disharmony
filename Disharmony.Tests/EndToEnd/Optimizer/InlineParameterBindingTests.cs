namespace Disharmony.Tests.EndToEnd.Optimizer;

[TestFixture]
#if NETFRAMEWORK
[Timeout(5000)]
#endif
public sealed class InlineParameterBindingTests : PatchTestBase
{
    [SetUp]
    public void EnableOptimizer()
    {
        HarmonyInterface.Instance.optimizerEnabled = true;
        InlineParameterBindingTargets.TargetCalls = 0;
    }

    [TearDown]
    public void DisableOptimizer() =>
        HarmonyInterface.Instance.optimizerEnabled = false;

    private static void ApplyInlinePatch(string patchMethodName, PatchKind patchKind,
        string targetMethodName, string? innerMethodName = null)
    {
        MethodInfo patch = typeof(InlineParameterBindingPatches).GetMethod(patchMethodName)!;
        MethodInfo target = typeof(InlineParameterBindingTargets).GetMethod(targetMethodName)!;
        MethodInfo? innerTarget = innerMethodName == null
            ? null
            : typeof(InlineParameterBindingTargets).GetMethod(innerMethodName)!;
        PatchConfig patchConfig = patchKind switch
        {
            PatchKind.Prefix => Patch.Prefix,
            PatchKind.Postfix => Patch.Postfix,
            _ => throw new ArgumentOutOfRangeException(nameof(patchKind)),
        };
        if (innerTarget is not null)
            patchConfig = patchConfig.Inner(innerTarget);
        Patcher.Patch(patchConfig.With(patch)
            .Options(PatchOptions.Optimize | PatchOptions.Inline).Of(target));
    }

    private static void ApplyInlinePatch(string patchMethodName, PatchKind patchKind,
        MethodBase target)
    {
        MethodInfo patch = typeof(InlineParameterBindingPatches).GetMethod(patchMethodName)!;
        PatchConfig patchConfig = patchKind switch
        {
            PatchKind.Prefix => Patch.Prefix,
            PatchKind.Postfix => Patch.Postfix,
            _ => throw new ArgumentOutOfRangeException(nameof(patchKind)),
        };
        Patcher.Patch(patchConfig.With(patch)
            .Options(PatchOptions.Optimize | PatchOptions.Inline).Of(target));
    }

    [Test]
    public void OuterPrefix_Argument_WriteByReference()
    {
        ApplyInlinePatch(
            nameof(InlineParameterBindingPatches.OuterPrefix_Argument_WriteByReference),
            PatchKind.Prefix,
            nameof(InlineParameterBindingTargets.OuterPrefix_Argument_WriteByReference));

        int result = InlineParameterBindingTargets.OuterPrefix_Argument_WriteByReference(10);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(20));
            Assert.That(InlineParameterBindingTargets.TargetCalls, Is.EqualTo(1));
        });
    }

    [Test]
    public void InnerPrefix_Argument_WriteByReference()
    {
        ApplyInlinePatch(
            nameof(InlineParameterBindingPatches.InnerPrefix_Argument_WriteByReference),
            PatchKind.Prefix,
            nameof(InlineParameterBindingTargets.InnerPrefix_Argument_WriteByReference),
            nameof(InlineParameterBindingTargets.InnerPrefix_Argument_WriteByReference_Inner));

        int result = InlineParameterBindingTargets.InnerPrefix_Argument_WriteByReference(10);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(20));
            Assert.That(InlineParameterBindingTargets.TargetCalls, Is.EqualTo(1));
        });
    }

    [Test]
    public void OuterPostfix_Result_WriteByReference()
    {
        ApplyInlinePatch(
            nameof(InlineParameterBindingPatches.OuterPostfix_Result_WriteByReference),
            PatchKind.Postfix,
            nameof(InlineParameterBindingTargets.OuterPostfix_Result_WriteByReference));

        int result = InlineParameterBindingTargets.OuterPostfix_Result_WriteByReference();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(20));
            Assert.That(InlineParameterBindingTargets.TargetCalls, Is.EqualTo(1));
        });
    }

    [Test]
    public void InnerPostfix_Result_WriteByReference()
    {
        ApplyInlinePatch(
            nameof(InlineParameterBindingPatches.InnerPostfix_Result_WriteByReference),
            PatchKind.Postfix,
            nameof(InlineParameterBindingTargets.InnerPostfix_Result_WriteByReference),
            nameof(InlineParameterBindingTargets.InnerPostfix_Result_WriteByReference_Inner));

        int result = InlineParameterBindingTargets.InnerPostfix_Result_WriteByReference();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(20));
            Assert.That(InlineParameterBindingTargets.TargetCalls, Is.EqualTo(1));
        });
    }

    [Test]
    public void Prefix_Argument_Primitive_ReadByReference()
    {
        InlineParameterBindingPatches.PrimitiveObserved = 0;
        ApplyInlinePatch(
            nameof(InlineParameterBindingPatches.Prefix_Argument_Primitive_ReadByReference),
            PatchKind.Prefix,
            typeof(InlineParameterBindingTargets).GetMethod(nameof(InlineParameterBindingTargets.PrimitiveIdentity))!);

        int result = InlineParameterBindingTargets.PrimitiveIdentity(7);

        Assert.That(InlineParameterBindingPatches.PrimitiveObserved, Is.EqualTo(7));
        Assert.That(result, Is.EqualTo(7));
    }

    [Test]
    public void Prefix_Argument_Primitive_WriteByReference()
    {
        ApplyInlinePatch(
            nameof(InlineParameterBindingPatches.Prefix_Argument_Primitive_WriteByReference),
            PatchKind.Prefix,
            typeof(InlineParameterBindingTargets).GetMethod(nameof(InlineParameterBindingTargets.PrimitiveIdentity))!);

        int result = InlineParameterBindingTargets.PrimitiveIdentity(7);

        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_Argument_ReferenceType_ReadByReference()
    {
        InlineParameterBindingPatches.ReferenceObserved = null;
        var original = new OptimizerDataObject { Number = 7, Text = "original" };
        ApplyInlinePatch(
            nameof(InlineParameterBindingPatches.Prefix_Argument_ReferenceType_ReadByReference),
            PatchKind.Prefix,
            typeof(InlineParameterBindingTargets).GetMethod(nameof(InlineParameterBindingTargets.ReferenceIdentity))!);

        OptimizerDataObject result = InlineParameterBindingTargets.ReferenceIdentity(original);

        Assert.That(InlineParameterBindingPatches.ReferenceObserved, Is.SameAs(original));
        Assert.That(result, Is.SameAs(original));
    }

    [Test]
    public void Prefix_Argument_ReferenceType_WriteByReference()
    {
        var original = new OptimizerDataObject { Number = 7, Text = "original" };
        ApplyInlinePatch(
            nameof(InlineParameterBindingPatches.Prefix_Argument_ReferenceType_WriteByReference),
            PatchKind.Prefix,
            typeof(InlineParameterBindingTargets).GetMethod(nameof(InlineParameterBindingTargets.ReferenceIdentity))!);

        OptimizerDataObject result = InlineParameterBindingTargets.ReferenceIdentity(original);

        Assert.That(original.Number, Is.EqualTo(7));
        Assert.That(original.Text, Is.EqualTo("original"));
        Assert.That(result, Is.Not.SameAs(original));
        Assert.That(result.Number, Is.EqualTo(42));
        Assert.That(result.Text, Is.EqualTo("patched"));
    }

    [Test]
    public void Prefix_Argument_Struct_ReadByReference()
    {
        InlineParameterBindingPatches.StructObserved = default;
        var original = new OptimizerDataStruct { Number = 7, Text = "original" };
        ApplyInlinePatch(
            nameof(InlineParameterBindingPatches.Prefix_Argument_Struct_ReadByReference),
            PatchKind.Prefix,
            typeof(InlineParameterBindingTargets).GetMethod(nameof(InlineParameterBindingTargets.StructIdentity))!);

        OptimizerDataStruct result = InlineParameterBindingTargets.StructIdentity(original);

        Assert.That(InlineParameterBindingPatches.StructObserved.Number, Is.EqualTo(7));
        Assert.That(InlineParameterBindingPatches.StructObserved.Text, Is.EqualTo("original"));
        Assert.That(result.Number, Is.EqualTo(7));
        Assert.That(result.Text, Is.EqualTo("original"));
    }

    [Test]
    public void Prefix_Argument_Struct_WriteByReference()
    {
        var original = new OptimizerDataStruct { Number = 7, Text = "original" };
        ApplyInlinePatch(
            nameof(InlineParameterBindingPatches.Prefix_Argument_Struct_WriteByReference),
            PatchKind.Prefix,
            typeof(InlineParameterBindingTargets).GetMethod(nameof(InlineParameterBindingTargets.StructIdentity))!);

        OptimizerDataStruct result = InlineParameterBindingTargets.StructIdentity(original);

        Assert.That(original.Number, Is.EqualTo(7));
        Assert.That(original.Text, Is.EqualTo("original"));
        Assert.That(result.Number, Is.EqualTo(42));
        Assert.That(result.Text, Is.EqualTo("patched"));
    }

    [Test]
    public void Postfix_Result_Primitive_ReadByReference()
    {
        InlineParameterBindingPatches.PrimitiveObserved = 0;
        ApplyInlinePatch(
            nameof(InlineParameterBindingPatches.Postfix_Result_Primitive_ReadByReference),
            PatchKind.Postfix,
            typeof(InlineParameterBindingTargets).GetMethod(nameof(InlineParameterBindingTargets.PrimitiveResult))!);

        int result = InlineParameterBindingTargets.PrimitiveResult();

        Assert.That(InlineParameterBindingPatches.PrimitiveObserved, Is.EqualTo(7));
        Assert.That(result, Is.EqualTo(7));
    }

    [Test]
    public void Postfix_Result_Primitive_WriteByReference()
    {
        ApplyInlinePatch(
            nameof(InlineParameterBindingPatches.Postfix_Result_Primitive_WriteByReference),
            PatchKind.Postfix,
            typeof(InlineParameterBindingTargets).GetMethod(nameof(InlineParameterBindingTargets.PrimitiveResult))!);

        int result = InlineParameterBindingTargets.PrimitiveResult();

        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_Result_ReferenceType_ReadByReference()
    {
        InlineParameterBindingPatches.ReferenceObserved = null;
        ApplyInlinePatch(
            nameof(InlineParameterBindingPatches.Postfix_Result_ReferenceType_ReadByReference),
            PatchKind.Postfix,
            typeof(InlineParameterBindingTargets).GetMethod(nameof(InlineParameterBindingTargets.ReferenceResult))!);

        OptimizerDataObject result = InlineParameterBindingTargets.ReferenceResult();

        Assert.That(InlineParameterBindingPatches.ReferenceObserved, Is.SameAs(result));
        Assert.That(result.Number, Is.EqualTo(7));
        Assert.That(result.Text, Is.EqualTo("original"));
    }

    [Test]
    public void Postfix_Result_ReferenceType_WriteByReference()
    {
        ApplyInlinePatch(
            nameof(InlineParameterBindingPatches.Postfix_Result_ReferenceType_WriteByReference),
            PatchKind.Postfix,
            typeof(InlineParameterBindingTargets).GetMethod(nameof(InlineParameterBindingTargets.ReferenceResult))!);

        OptimizerDataObject result = InlineParameterBindingTargets.ReferenceResult();

        Assert.That(result.Number, Is.EqualTo(42));
        Assert.That(result.Text, Is.EqualTo("patched"));
    }

    [Test]
    public void Postfix_Result_Struct_ReadByReference()
    {
        InlineParameterBindingPatches.StructObserved = default;
        ApplyInlinePatch(
            nameof(InlineParameterBindingPatches.Postfix_Result_Struct_ReadByReference),
            PatchKind.Postfix,
            typeof(InlineParameterBindingTargets).GetMethod(nameof(InlineParameterBindingTargets.StructResult))!);

        OptimizerDataStruct result = InlineParameterBindingTargets.StructResult();

        Assert.That(InlineParameterBindingPatches.StructObserved.Number, Is.EqualTo(7));
        Assert.That(InlineParameterBindingPatches.StructObserved.Text, Is.EqualTo("original"));
        Assert.That(result.Number, Is.EqualTo(7));
        Assert.That(result.Text, Is.EqualTo("original"));
    }

    [Test]
    public void Postfix_Result_Struct_WriteByReference()
    {
        ApplyInlinePatch(
            nameof(InlineParameterBindingPatches.Postfix_Result_Struct_WriteByReference),
            PatchKind.Postfix,
            typeof(InlineParameterBindingTargets).GetMethod(nameof(InlineParameterBindingTargets.StructResult))!);

        OptimizerDataStruct result = InlineParameterBindingTargets.StructResult();

        Assert.That(result.Number, Is.EqualTo(42));
        Assert.That(result.Text, Is.EqualTo("patched"));
    }

    [Test]
    public void Prefix_TargetRefArgument_Primitive_ReadByReference()
    {
        InlineParameterBindingPatches.PrimitiveObserved = 0;
        int value = 7;
        ApplyInlinePatch(
            nameof(InlineParameterBindingPatches.Prefix_TargetRefArgument_Primitive_ReadByReference),
            PatchKind.Prefix,
            typeof(InlineParameterBindingTargets).GetMethod(nameof(InlineParameterBindingTargets.RefPrimitiveIdentity))!);

        int result = InlineParameterBindingTargets.RefPrimitiveIdentity(ref value);

        Assert.That(InlineParameterBindingPatches.PrimitiveObserved, Is.EqualTo(7));
        Assert.That(value, Is.EqualTo(8));
        Assert.That(result, Is.EqualTo(8));
    }

    [Test]
    public void Prefix_TargetRefArgument_Primitive_WriteByReference()
    {
        int value = 7;
        ApplyInlinePatch(
            nameof(InlineParameterBindingPatches.Prefix_TargetRefArgument_Primitive_WriteByReference),
            PatchKind.Prefix,
            typeof(InlineParameterBindingTargets).GetMethod(nameof(InlineParameterBindingTargets.RefPrimitiveIdentity))!);

        int result = InlineParameterBindingTargets.RefPrimitiveIdentity(ref value);

        Assert.That(value, Is.EqualTo(43));
        Assert.That(result, Is.EqualTo(43));
    }

    [Test]
    public void PrefixPostfix_StateAndResult_PreservesValues()
    {
        InlineParameterBindingPatches.StateObserved = 0;
        InlineParameterBindingPatches.ResultObserved = 0;
        MethodInfo prefix = typeof(InlineParameterBindingPatches)
            .GetMethod(nameof(InlineParameterBindingPatches.PrefixPostfix_StateAndResult_PreservesValues_Prefix))!;
        MethodInfo postfix = typeof(InlineParameterBindingPatches)
            .GetMethod(nameof(InlineParameterBindingPatches.PrefixPostfix_StateAndResult_PreservesValues_Postfix))!;
        MethodInfo target = typeof(InlineParameterBindingTargets)
            .GetMethod(nameof(InlineParameterBindingTargets.PrimitiveIdentity))!;
        PatchOptions options = PatchOptions.Optimize | PatchOptions.Inline;

        Patcher.Patch(
            target,
            Patch.Prefix.With(prefix).Options(options),
            Patch.Postfix.With(postfix).Options(options));

        int result = InlineParameterBindingTargets.PrimitiveIdentity(7);

        Assert.That(InlineParameterBindingPatches.StateObserved, Is.EqualTo(7));
        Assert.That(InlineParameterBindingPatches.ResultObserved, Is.EqualTo(7));
        Assert.That(result, Is.EqualTo(42));
    }

}
