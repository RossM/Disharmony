namespace Disharmony.Tests.EndToEnd.Patching;

public static class ConstructorPatchingPatches
{
    public static int executionCount;
    public static int parameterObserved;
    public static ConstructorTargets? instanceObserved;
    public static ConstructorTargets? resultObserved;

    [Prefix]
    [Target(typeof(ConstructorTargets), memberType: MemberType.Constructor, parameterTypes: [])]
    public static void Prefix_Constructor_ReferenceType_Parameterless_Executes() => executionCount++;

    [Postfix]
    [Target(typeof(ConstructorTargets), memberType: MemberType.Constructor, parameterTypes: [])]
    public static void Postfix_Constructor_ReferenceType_Parameterless_Executes() => executionCount++;

    public static void Prefix_StaticConstructor_ReferenceType_Parameterless_Executes() => executionCount++;

    [Prefix] [Inner(
        typeof(ConstructorTargets),
        memberType: MemberType.Constructor,
        parameterTypes: [])]
    [Target(typeof(ConstructorTargets), nameof(ConstructorTargets.Create), parameterTypes: [])]
    public static void InnerPrefix_Constructor_ReferenceType_Parameterless_Executes() => executionCount++;

    [Postfix] [Inner(
        typeof(ConstructorTargets),
        memberType: MemberType.Constructor,
        parameterTypes: [])]
    [Target(typeof(ConstructorTargets), nameof(ConstructorTargets.Create), parameterTypes: [])]
    public static void InnerPostfix_Constructor_ReferenceType_Parameterless_Executes() => executionCount++;

    [Prefix]
    [Target(
        typeof(ConstructorTargets),
        memberType: MemberType.Constructor,
        parameterTypes: [typeof(int)])]
    public static void Prefix_Constructor_ReferenceType_Parameter_Primitive_ReadByValue(int value) => parameterObserved = value;

    [Prefix]
    [Target(
        typeof(ConstructorTargets),
        memberType: MemberType.Constructor,
        parameterTypes: [typeof(int)])]
    public static void Prefix_Constructor_ReferenceType_ParameterAttribute_Index0_Primitive_ReadByValue(
        [Argument(0)] int argument) => parameterObserved = argument;

    [Prefix]
    [Target(
        typeof(ConstructorTargets),
        memberType: MemberType.Constructor,
        parameterTypes: [typeof(int)])]
    public static void Prefix_Constructor_ReferenceType_ParameterAttribute_Index0_Primitive_WriteByReference(
        [Argument(0)] ref int argument) => argument = 42;

    [Postfix]
    [Target(
        typeof(ConstructorTargets),
        memberType: MemberType.Constructor,
        parameterTypes: [typeof(int)])]
    public static void Postfix_Constructor_ReferenceType_Parameter_Primitive_ReadByValue(int value) => parameterObserved = value;

    [Postfix]
    [Target(
        typeof(ConstructorTargets),
        memberType: MemberType.Constructor,
        parameterTypes: [typeof(int)])]
    public static void Postfix_Constructor_ReferenceType_Parameter_Primitive_ReadByReference_Rejected(ref int value) =>
        parameterObserved = value;

    [Prefix] [Inner(
        typeof(ConstructorTargets),
        memberType: MemberType.Constructor,
        parameterTypes: [typeof(int)])]
    [Target(typeof(ConstructorTargets), nameof(ConstructorTargets.Create), parameterTypes: [typeof(int)])]
    public static void InnerPrefix_Constructor_ReferenceType_Parameter_Primitive_ReadByValue(int value) => parameterObserved = value;

    [Postfix] [Inner(
        typeof(ConstructorTargets),
        memberType: MemberType.Constructor,
        parameterTypes: [typeof(int)])]
    [Target(typeof(ConstructorTargets), nameof(ConstructorTargets.Create), parameterTypes: [typeof(int)])]
    public static void InnerPostfix_Constructor_ReferenceType_Parameter_Primitive_ReadByValue(int value) => parameterObserved = value;

    [Postfix] [Inner(
        typeof(ConstructorTargets),
        memberType: MemberType.Constructor,
        parameterTypes: [typeof(int)])]
    [Target(typeof(ConstructorTargets), nameof(ConstructorTargets.Create), parameterTypes: [typeof(int)])]
    public static void InnerPostfix_Constructor_ReferenceType_Parameter_Primitive_ReadByReference_Rejected(ref int value) =>
        parameterObserved = value;

    [Prefix]
    [Target(typeof(ConstructorTargets), memberType: MemberType.Constructor, parameterTypes: [])]
    public static void Prefix_Constructor_ReferenceType_Parameterless_Instance_ReadByValue(ConstructorTargets __instance) =>
        instanceObserved = __instance;

    [Postfix]
    [Target(typeof(ConstructorTargets), memberType: MemberType.Constructor, parameterTypes: [])]
    public static void Postfix_Constructor_ReferenceType_Parameterless_Instance_ReadByValue(ConstructorTargets __instance) =>
        instanceObserved = __instance;

    [Postfix]
    [Target(typeof(ConstructorTargets), memberType: MemberType.Constructor, parameterTypes: [])]
    public static void Postfix_Constructor_ReferenceType_Parameterless_Instance_ReadByReference_Rejected(
        ref ConstructorTargets __instance) => instanceObserved = __instance;

    [Postfix]
    [Target(typeof(ConstructorTargets), memberType: MemberType.Constructor, parameterTypes: [])]
    public static void Postfix_Constructor_ReferenceType_Parameterless_Instance_WriteByReference_Rejected(
        ref ConstructorTargets __instance) => __instance = null!;

    [Prefix] [Inner(
        typeof(ConstructorTargets),
        memberType: MemberType.Constructor,
        parameterTypes: [])]
    [Target(typeof(ConstructorTargets), nameof(ConstructorTargets.Create), parameterTypes: [])]
    public static void InnerPrefix_Constructor_ReferenceType_Parameterless_Result_ReadByValue(ConstructorTargets? __result) =>
        resultObserved = __result;

    [Postfix] [Inner(
        typeof(ConstructorTargets),
        memberType: MemberType.Constructor,
        parameterTypes: [])]
    [Target(typeof(ConstructorTargets), nameof(ConstructorTargets.Create), parameterTypes: [])]
    public static void InnerPostfix_Constructor_ReferenceType_Parameterless_Result_ReadByValue(ConstructorTargets __result) =>
        resultObserved = __result;
}

[TestFixture]
public sealed class ConstructorPatchingTests : PatchTestBase
{
    [Test]
    public void Prefix_Constructor_ReferenceType_Parameterless_Executes()
    {
        ConstructorPatchingPatches.executionCount = 0;
        ApplyPatch(
            typeof(ConstructorPatchingPatches),
            nameof(ConstructorPatchingPatches.Prefix_Constructor_ReferenceType_Parameterless_Executes));

        var result = new ConstructorTargets();

        Assert.That(ConstructorPatchingPatches.executionCount, Is.EqualTo(1));
        Assert.That(result.ConstructorExecuted, Is.True);
    }

    [Test]
    public void Postfix_Constructor_ReferenceType_Parameterless_Executes()
    {
        ConstructorPatchingPatches.executionCount = 0;
        ApplyPatch(
            typeof(ConstructorPatchingPatches),
            nameof(ConstructorPatchingPatches.Postfix_Constructor_ReferenceType_Parameterless_Executes));

        var result = new ConstructorTargets();

        Assert.That(ConstructorPatchingPatches.executionCount, Is.EqualTo(1));
        Assert.That(result.ConstructorExecuted, Is.True);
    }

    [Test]
    [Ignore("Harmony/MonoMod initializes the type while compiling its static constructor for patching")]
    public void Prefix_StaticConstructor_ReferenceType_Parameterless_Executes()
    {
        ConstructorPatchingPatches.executionCount = 0;
        StaticConstructorObservation.ConstructorExecuted = false;
        ConstructorInfo target = typeof(StaticConstructorTargets).TypeInitializer!;
        MethodInfo patch = typeof(ConstructorPatchingPatches)
            .GetMethod(nameof(ConstructorPatchingPatches.Prefix_StaticConstructor_ReferenceType_Parameterless_Executes))!;
        bool constructorExecutedDuringReflection = StaticConstructorObservation.ConstructorExecuted;
        PatchConfig config = Patch.Of(target).Prefix.With(patch);
        bool constructorExecutedDuringConfiguration = StaticConstructorObservation.ConstructorExecuted;

        Patcher.Patch(config);
        bool constructorExecutedDuringPatching = StaticConstructorObservation.ConstructorExecuted;
        int result = StaticConstructorTargets.Value;

        Assert.Multiple(() =>
        {
            Assert.That(constructorExecutedDuringReflection, Is.False);
            Assert.That(constructorExecutedDuringConfiguration, Is.False);
            Assert.That(constructorExecutedDuringPatching, Is.False);
            Assert.That(ConstructorPatchingPatches.executionCount, Is.EqualTo(1));
            Assert.That(StaticConstructorObservation.ConstructorExecuted, Is.True);
            Assert.That(result, Is.EqualTo(42));
        });
    }

    [Test]
    public void InnerPrefix_Constructor_ReferenceType_Parameterless_Executes()
    {
        ConstructorPatchingPatches.executionCount = 0;
        ApplyPatch(
            typeof(ConstructorPatchingPatches),
            nameof(ConstructorPatchingPatches.InnerPrefix_Constructor_ReferenceType_Parameterless_Executes));

        ConstructorTargets result = ConstructorTargets.Create();

        Assert.That(ConstructorPatchingPatches.executionCount, Is.EqualTo(1));
        Assert.That(result.ConstructorExecuted, Is.True);
    }

    [Test]
    public void InnerPostfix_Constructor_ReferenceType_Parameterless_Executes()
    {
        ConstructorPatchingPatches.executionCount = 0;
        ApplyPatch(
            typeof(ConstructorPatchingPatches),
            nameof(ConstructorPatchingPatches.InnerPostfix_Constructor_ReferenceType_Parameterless_Executes));

        ConstructorTargets result = ConstructorTargets.Create();

        Assert.That(ConstructorPatchingPatches.executionCount, Is.EqualTo(1));
        Assert.That(result.ConstructorExecuted, Is.True);
    }

    [Test]
    public void Prefix_Constructor_ReferenceType_Parameter_Primitive_ReadByValue()
    {
        ConstructorPatchingPatches.parameterObserved = 0;
        ApplyPatch(
            typeof(ConstructorPatchingPatches),
            nameof(ConstructorPatchingPatches.Prefix_Constructor_ReferenceType_Parameter_Primitive_ReadByValue));

        var result = new ConstructorTargets(42);

        Assert.That(result.Value, Is.EqualTo(42));
        Assert.That(ConstructorPatchingPatches.parameterObserved, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_Constructor_ReferenceType_ParameterAttribute_Index0_Primitive_ReadByValue()
    {
        ConstructorPatchingPatches.parameterObserved = 0;
        ApplyPatch(
            typeof(ConstructorPatchingPatches),
            nameof(ConstructorPatchingPatches
                .Prefix_Constructor_ReferenceType_ParameterAttribute_Index0_Primitive_ReadByValue));

        var result = new ConstructorTargets(42);

        Assert.That(result.Value, Is.EqualTo(42));
        Assert.That(ConstructorPatchingPatches.parameterObserved, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_Constructor_ReferenceType_ParameterAttribute_Index0_Primitive_WriteByReference()
    {
        ApplyPatch(
            typeof(ConstructorPatchingPatches),
            nameof(ConstructorPatchingPatches
                .Prefix_Constructor_ReferenceType_ParameterAttribute_Index0_Primitive_WriteByReference));

        var result = new ConstructorTargets(1);

        Assert.That(result.Value, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_Constructor_ReferenceType_Parameter_Primitive_ReadByValue()
    {
        ConstructorPatchingPatches.parameterObserved = 0;
        ApplyPatch(
            typeof(ConstructorPatchingPatches),
            nameof(ConstructorPatchingPatches.Postfix_Constructor_ReferenceType_Parameter_Primitive_ReadByValue));

        var result = new ConstructorTargets(42);

        Assert.That(result.Value, Is.EqualTo(42));
        Assert.That(ConstructorPatchingPatches.parameterObserved, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_Constructor_ReferenceType_Parameter_Primitive_ReadByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(ConstructorPatchingPatches),
                nameof(ConstructorPatchingPatches.Postfix_Constructor_ReferenceType_Parameter_Primitive_ReadByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void InnerPrefix_Constructor_ReferenceType_Parameter_Primitive_ReadByValue()
    {
        ConstructorPatchingPatches.parameterObserved = 0;
        ApplyPatch(
            typeof(ConstructorPatchingPatches),
            nameof(ConstructorPatchingPatches.InnerPrefix_Constructor_ReferenceType_Parameter_Primitive_ReadByValue));

        ConstructorTargets result = ConstructorTargets.Create(42);

        Assert.That(result.Value, Is.EqualTo(42));
        Assert.That(ConstructorPatchingPatches.parameterObserved, Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfix_Constructor_ReferenceType_Parameter_Primitive_ReadByValue()
    {
        ConstructorPatchingPatches.parameterObserved = 0;
        ApplyPatch(
            typeof(ConstructorPatchingPatches),
            nameof(ConstructorPatchingPatches.InnerPostfix_Constructor_ReferenceType_Parameter_Primitive_ReadByValue));

        ConstructorTargets result = ConstructorTargets.Create(42);

        Assert.That(result.Value, Is.EqualTo(42));
        Assert.That(ConstructorPatchingPatches.parameterObserved, Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfix_Constructor_ReferenceType_Parameter_Primitive_ReadByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(ConstructorPatchingPatches),
                nameof(ConstructorPatchingPatches.InnerPostfix_Constructor_ReferenceType_Parameter_Primitive_ReadByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void Prefix_Constructor_ReferenceType_Parameterless_Instance_ReadByValue()
    {
        ConstructorPatchingPatches.instanceObserved = null;
        ApplyPatch(
            typeof(ConstructorPatchingPatches),
            nameof(ConstructorPatchingPatches.Prefix_Constructor_ReferenceType_Parameterless_Instance_ReadByValue));

        var result = new ConstructorTargets();

        Assert.That(ConstructorPatchingPatches.instanceObserved, Is.SameAs(result));
    }

    [Test]
    public void Postfix_Constructor_ReferenceType_Parameterless_Instance_ReadByValue()
    {
        ConstructorPatchingPatches.instanceObserved = null;
        ApplyPatch(
            typeof(ConstructorPatchingPatches),
            nameof(ConstructorPatchingPatches.Postfix_Constructor_ReferenceType_Parameterless_Instance_ReadByValue));

        var result = new ConstructorTargets();

        Assert.That(ConstructorPatchingPatches.instanceObserved, Is.SameAs(result));
    }

    [Test]
    public void Postfix_Constructor_ReferenceType_Parameterless_Instance_ReadByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(ConstructorPatchingPatches),
                nameof(ConstructorPatchingPatches.Postfix_Constructor_ReferenceType_Parameterless_Instance_ReadByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void Postfix_Constructor_ReferenceType_Parameterless_Instance_WriteByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(ConstructorPatchingPatches),
                nameof(ConstructorPatchingPatches.Postfix_Constructor_ReferenceType_Parameterless_Instance_WriteByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void InnerPrefix_Constructor_ReferenceType_Parameterless_Result_ReadByValue()
    {
        ConstructorPatchingPatches.resultObserved = new ConstructorTargets();
        ApplyPatch(
            typeof(ConstructorPatchingPatches),
            nameof(ConstructorPatchingPatches.InnerPrefix_Constructor_ReferenceType_Parameterless_Result_ReadByValue));

        ConstructorTargets result = ConstructorTargets.Create();

        Assert.That(result.ConstructorExecuted, Is.True);
        Assert.That(ConstructorPatchingPatches.resultObserved, Is.Null);
    }

    [Test]
    public void InnerPostfix_Constructor_ReferenceType_Parameterless_Result_ReadByValue()
    {
        ConstructorPatchingPatches.resultObserved = null;
        ApplyPatch(
            typeof(ConstructorPatchingPatches),
            nameof(ConstructorPatchingPatches.InnerPostfix_Constructor_ReferenceType_Parameterless_Result_ReadByValue));

        ConstructorTargets result = ConstructorTargets.Create();

        Assert.That(result.ConstructorExecuted, Is.True);
        Assert.That(ConstructorPatchingPatches.resultObserved, Is.SameAs(result));
    }
}
