namespace Disharmony.Tests.EndToEnd.Patching;

public static class InvocationPatchingPatches
{
    public static int ParameterObserved;
    public static object? InstanceObserved;
    public static object? ResultObserved;

    public static void PatchConfig_OuterConstructor_ParameterAndInstanceBinding(
        int value,
        ConstructorTargets __instance)
    {
        ParameterObserved = value;
        InstanceObserved = __instance;
    }

    public static void PatchConfig_InnerConstructor_ParameterAndResultBinding(
        int value,
        ConstructorTargets? __result)
    {
        ParameterObserved = value;
        ResultObserved = __result;
    }

    public static void PatchConfig_InnerStaticFieldGetter_ResultBinding(ref int __result) => __result = 42;

    public static void PatchConfig_InnerInstanceFieldGetter_InstanceAndResultBinding(
        InnerInstanceMethodTargets __instance,
        int __result)
    {
        InstanceObserved = __instance;
        ResultObserved = __result;
    }

    public static void PatchConfig_InnerInstanceFieldSetter_InstanceAndValueBinding(
        InnerInstanceMethodTargets __instance,
        ref int value)
    {
        InstanceObserved = __instance;
        value = 42;
    }

    public static void PatchConfig_InnerStaticPropertyGetter_ResultBinding(ref int __result) => __result = 42;

    public static void PatchConfig_InnerStaticPropertySetter_ValueBinding(ref int value) => value = 42;

    public static void PatchConfig_InnerConstant_Int_ResultBinding(ref int __result) =>
        __result = ConstantTargets.IntReplacement;

    public static void PatchConfig_InnerConstant_Long_ResultBinding(ref long __result) =>
        __result = ConstantTargets.LongReplacement;

    public static void PatchConfig_InnerConstant_Float_ResultBinding(ref float __result) =>
        __result = ConstantTargets.FloatReplacement;

    public static void PatchConfig_InnerConstant_Double_ResultBinding(ref double __result) =>
        __result = ConstantTargets.DoubleReplacement;

    public static void PatchConfig_InnerConstant_String_ResultBinding(ref string __result) =>
        __result = ConstantTargets.StringReplacement;
}

[TestFixture]
public sealed class InvocationPatchingTests : PatchTestBase
{
    [Test]
    public void PatchConfig_OuterConstructor_ParameterAndInstanceBinding()
    {
        ConstructorInfo target = typeof(ConstructorTargets).GetConstructor([typeof(int)])!;
        MethodInfo patch = typeof(InvocationPatchingPatches)
            .GetMethod(nameof(InvocationPatchingPatches.PatchConfig_OuterConstructor_ParameterAndInstanceBinding))!;
        InvocationPatchingPatches.ParameterObserved = 0;
        InvocationPatchingPatches.InstanceObserved = null;

        Patcher.Patch(Patch.Of(target).Prefix.With(patch));
        var result = new ConstructorTargets(42);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.EqualTo(42));
            Assert.That(InvocationPatchingPatches.ParameterObserved, Is.EqualTo(42));
            Assert.That(InvocationPatchingPatches.InstanceObserved, Is.SameAs(result));
        });
    }

    [Test]
    public void PatchConfig_InnerConstructor_ParameterAndResultBinding()
    {
        ConstructorInfo innerTarget = typeof(ConstructorTargets).GetConstructor([typeof(int)])!;
        MethodInfo outerTarget = typeof(ConstructorTargets).GetMethod(nameof(ConstructorTargets.Create), [typeof(int)])!;
        MethodInfo patch = typeof(InvocationPatchingPatches)
            .GetMethod(nameof(InvocationPatchingPatches.PatchConfig_InnerConstructor_ParameterAndResultBinding))!;
        InvocationPatchingPatches.ParameterObserved = 0;
        InvocationPatchingPatches.ResultObserved = new ConstructorTargets();

        Patcher.Patch(Patch.Inner(innerTarget).Prefix.With(patch).Of(outerTarget));
        ConstructorTargets result = ConstructorTargets.Create(42);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.EqualTo(42));
            Assert.That(InvocationPatchingPatches.ParameterObserved, Is.EqualTo(42));
            Assert.That(InvocationPatchingPatches.ResultObserved, Is.Null);
        });
    }

    [Test]
    public void PatchConfig_InnerStaticFieldGetter_ResultBinding()
    {
        FieldInfo innerTarget = typeof(InnerStaticMethodTargets).GetField(nameof(InnerStaticMethodTargets.Field))!;
        MethodInfo outerTarget = typeof(OuterStaticMethodTargets).GetMethod(nameof(OuterStaticMethodTargets.FieldResult))!;
        MethodInfo patch = typeof(InvocationPatchingPatches)
            .GetMethod(nameof(InvocationPatchingPatches.PatchConfig_InnerStaticFieldGetter_ResultBinding))!;
        InnerStaticMethodTargets.Field = 1;

        Patcher.Patch(Patch.InnerGet(innerTarget).Postfix.With(patch).Of(outerTarget));
        int result = OuterStaticMethodTargets.FieldResult();

        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void PatchConfig_InnerInstanceFieldGetter_InstanceAndResultBinding()
    {
        FieldInfo innerTarget = typeof(InnerInstanceMethodTargets).GetField(nameof(InnerInstanceMethodTargets.foo))!;
        MethodInfo outerTarget = typeof(OuterStaticMethodTargets)
            .GetMethod(nameof(OuterStaticMethodTargets.ReadInstanceField))!;
        MethodInfo patch = typeof(InvocationPatchingPatches)
            .GetMethod(nameof(InvocationPatchingPatches.PatchConfig_InnerInstanceFieldGetter_InstanceAndResultBinding))!;
        var inner = new InnerInstanceMethodTargets { foo = 42 };
        InvocationPatchingPatches.InstanceObserved = null;
        InvocationPatchingPatches.ResultObserved = null;

        Patcher.Patch(Patch.InnerGet(innerTarget).Postfix.With(patch).Of(outerTarget));
        int result = OuterStaticMethodTargets.ReadInstanceField(inner);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(42));
            Assert.That(InvocationPatchingPatches.InstanceObserved, Is.SameAs(inner));
            Assert.That(InvocationPatchingPatches.ResultObserved, Is.EqualTo(42));
        });
    }

    [Test]
    public void PatchConfig_InnerInstanceFieldSetter_InstanceAndValueBinding()
    {
        FieldInfo innerTarget = typeof(InnerInstanceMethodTargets).GetField(nameof(InnerInstanceMethodTargets.foo))!;
        MethodInfo outerTarget = typeof(OuterStaticMethodTargets)
            .GetMethod(nameof(OuterStaticMethodTargets.SetInstanceField))!;
        MethodInfo patch = typeof(InvocationPatchingPatches)
            .GetMethod(nameof(InvocationPatchingPatches.PatchConfig_InnerInstanceFieldSetter_InstanceAndValueBinding))!;
        var inner = new InnerInstanceMethodTargets { foo = 1 };
        InvocationPatchingPatches.InstanceObserved = null;

        Patcher.Patch(Patch.InnerSet(innerTarget).Prefix.With(patch).Of(outerTarget));
        OuterStaticMethodTargets.SetInstanceField(inner, 2);

        Assert.That(inner.foo, Is.EqualTo(42));
        Assert.That(InvocationPatchingPatches.InstanceObserved, Is.SameAs(inner));
    }

    [Test]
    public void PatchConfig_InnerStaticPropertyGetter_ResultBinding()
    {
        PropertyInfo innerTarget = typeof(InnerStaticMethodTargets)
            .GetProperty(nameof(InnerStaticMethodTargets.MutableProperty))!;
        MethodInfo outerTarget = typeof(OuterStaticMethodTargets)
            .GetMethod(nameof(OuterStaticMethodTargets.MutablePropertyResult))!;
        MethodInfo patch = typeof(InvocationPatchingPatches)
            .GetMethod(nameof(InvocationPatchingPatches.PatchConfig_InnerStaticPropertyGetter_ResultBinding))!;
        InnerStaticMethodTargets.MutableProperty = 1;

        Patcher.Patch(Patch.InnerGet(innerTarget).Postfix.With(patch).Of(outerTarget));
        int result = OuterStaticMethodTargets.MutablePropertyResult();

        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void PatchConfig_InnerStaticPropertySetter_ValueBinding()
    {
        PropertyInfo innerTarget = typeof(InnerStaticMethodTargets)
            .GetProperty(nameof(InnerStaticMethodTargets.MutableProperty))!;
        MethodInfo outerTarget = typeof(OuterStaticMethodTargets)
            .GetMethod(nameof(OuterStaticMethodTargets.SetStaticProperty))!;
        MethodInfo patch = typeof(InvocationPatchingPatches)
            .GetMethod(nameof(InvocationPatchingPatches.PatchConfig_InnerStaticPropertySetter_ValueBinding))!;
        InnerStaticMethodTargets.MutableProperty = 1;

        Patcher.Patch(Patch.InnerSet(innerTarget).Prefix.With(patch).Of(outerTarget));
        OuterStaticMethodTargets.SetStaticProperty(2);

        Assert.That(InnerStaticMethodTargets.MutableProperty, Is.EqualTo(42));
    }

    [Test]
    public void PatchConfig_InnerConstant_Int_ResultBinding()
    {
        MethodInfo target = typeof(ConstantTargets).GetMethod(nameof(ConstantTargets.IntResult))!;
        MethodInfo patch = typeof(InvocationPatchingPatches)
            .GetMethod(nameof(InvocationPatchingPatches.PatchConfig_InnerConstant_Int_ResultBinding))!;

        Patcher.Patch(Patch.InnerConstant(ConstantTargets.IntValue).Postfix.With(patch).Of(target));
        int result = ConstantTargets.IntResult();

        Assert.That(result, Is.EqualTo(ConstantTargets.IntReplacement));
    }

    [Test]
    public void PatchConfig_InnerConstant_Long_ResultBinding()
    {
        MethodInfo target = typeof(ConstantTargets).GetMethod(nameof(ConstantTargets.LongResult))!;
        MethodInfo patch = typeof(InvocationPatchingPatches)
            .GetMethod(nameof(InvocationPatchingPatches.PatchConfig_InnerConstant_Long_ResultBinding))!;

        Patcher.Patch(Patch.InnerConstant(ConstantTargets.LongValue).Postfix.With(patch).Of(target));
        long result = ConstantTargets.LongResult();

        Assert.That(result, Is.EqualTo(ConstantTargets.LongReplacement));
    }

    [Test]
    public void PatchConfig_InnerConstant_Float_ResultBinding()
    {
        MethodInfo target = typeof(ConstantTargets).GetMethod(nameof(ConstantTargets.FloatResult))!;
        MethodInfo patch = typeof(InvocationPatchingPatches)
            .GetMethod(nameof(InvocationPatchingPatches.PatchConfig_InnerConstant_Float_ResultBinding))!;

        Patcher.Patch(Patch.InnerConstant(ConstantTargets.FloatValue).Postfix.With(patch).Of(target));
        float result = ConstantTargets.FloatResult();

        Assert.That(result, Is.EqualTo(ConstantTargets.FloatReplacement));
    }

    [Test]
    public void PatchConfig_InnerConstant_Double_ResultBinding()
    {
        MethodInfo target = typeof(ConstantTargets).GetMethod(nameof(ConstantTargets.DoubleResult))!;
        MethodInfo patch = typeof(InvocationPatchingPatches)
            .GetMethod(nameof(InvocationPatchingPatches.PatchConfig_InnerConstant_Double_ResultBinding))!;

        Patcher.Patch(Patch.InnerConstant(ConstantTargets.DoubleValue).Postfix.With(patch).Of(target));
        double result = ConstantTargets.DoubleResult();

        Assert.That(result, Is.EqualTo(ConstantTargets.DoubleReplacement));
    }

    [Test]
    public void PatchConfig_InnerConstant_String_ResultBinding()
    {
        MethodInfo target = typeof(ConstantTargets).GetMethod(nameof(ConstantTargets.StringResult))!;
        MethodInfo patch = typeof(InvocationPatchingPatches)
            .GetMethod(nameof(InvocationPatchingPatches.PatchConfig_InnerConstant_String_ResultBinding))!;

        Patcher.Patch(Patch.InnerConstant(ConstantTargets.StringValue).Postfix.With(patch).Of(target));
        string result = ConstantTargets.StringResult();

        Assert.That(result, Is.EqualTo(ConstantTargets.StringReplacement));
    }
}
