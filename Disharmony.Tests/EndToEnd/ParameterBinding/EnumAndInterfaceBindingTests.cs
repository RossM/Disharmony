namespace Disharmony.Tests.EndToEnd.ParameterBinding;

public static class EnumAndInterfaceBindingPatches
{
    public static BindingEnum EnumObserved;
    public static IBindingInterface? InterfaceObserved;
    public static IBindingInterface? InterfaceReplacement;
    public static System.Runtime.Serialization.ISerializable? ExceptionObserved;

    [Prefix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.EnumIdentity))]
    public static void Prefix_ParameterAttribute_Enum_ReadByValue([Argument("value")] BindingEnum value) =>
        EnumObserved = value;

    [Prefix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.EnumIdentity))]
    public static void Prefix_ParameterAttribute_Enum_ReadByReference([Argument("value")] ref BindingEnum value) =>
        EnumObserved = value;

    [Prefix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.EnumIdentity))]
    public static void Prefix_ParameterAttribute_Enum_WriteByReference([Argument("value")] ref BindingEnum value) =>
        value = BindingEnum.Replacement;

    [Prefix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.InterfaceIdentity))]
    public static void Prefix_ParameterAttribute_Interface_ReadByValue(
        [Argument("value")] IBindingInterface value) => InterfaceObserved = value;

    [Prefix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.InterfaceIdentity))]
    public static void Prefix_ParameterAttribute_Interface_ReadByReference(
        [Argument("value")] ref IBindingInterface value) => InterfaceObserved = value;

    [Prefix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.InterfaceIdentity))]
    public static void Prefix_ParameterAttribute_Interface_WriteByReference(
        [Argument("value")] ref IBindingInterface value) => value = InterfaceReplacement!;

    [Prefix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.ConcreteInterfaceIdentity))]
    public static void Prefix_ParameterAttribute_ConcreteClass_AsInterface_ReadByValue(
        [Argument("value")] IBindingInterface value) => InterfaceObserved = value;

    [Prefix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.ConcreteInterfaceIdentity))]
    public static void Prefix_ParameterAttribute_ConcreteClass_AsInterface_ReadByReadonlyReference(
        [Argument("value")] in IBindingInterface value) => InterfaceObserved = value;

    [Postfix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.EnumResult))]
    public static void Postfix_ReturnValueAttribute_Enum_ReadByValue([ReturnValue] BindingEnum value) =>
        EnumObserved = value;

    [Postfix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.EnumResult))]
    public static void Postfix_ReturnValueAttribute_Enum_ReadByReference([ReturnValue] ref BindingEnum value) =>
        EnumObserved = value;

    [Postfix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.EnumResult))]
    public static void Postfix_ReturnValueAttribute_Enum_WriteByReference([ReturnValue] ref BindingEnum value) =>
        value = BindingEnum.Replacement;

    [Postfix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.InterfaceResult))]
    public static void Postfix_ReturnValueAttribute_Interface_ReadByValue([ReturnValue] IBindingInterface value) =>
        InterfaceObserved = value;

    [Postfix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.InterfaceResult))]
    public static void Postfix_ReturnValueAttribute_Interface_ReadByReference(
        [ReturnValue] ref IBindingInterface value) => InterfaceObserved = value;

    [Postfix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.InterfaceResult))]
    public static void Postfix_ReturnValueAttribute_Interface_WriteByReference(
        [ReturnValue] ref IBindingInterface value) => value = InterfaceReplacement!;

    [Postfix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.ConcreteInterfaceResult))]
    public static void Postfix_ReturnValueAttribute_ConcreteClass_AsInterface_ReadByValue(
        [ReturnValue] IBindingInterface value) => InterfaceObserved = value;

    [Postfix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.ConcreteInterfaceResult))]
    public static void Postfix_ReturnValueAttribute_ConcreteClass_AsInterface_ReadByReadonlyReference(
        [ReturnValue] in IBindingInterface value) => InterfaceObserved = value;

    [Prefix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.Void))]
    public static void Prefix_FieldAttribute_Enum_ReadByValue([Field(nameof(EnumAndInterfaceBindingTargets.EnumField))]
        BindingEnum field) => EnumObserved = field;

    [Prefix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.Void))]
    public static void Prefix_FieldAttribute_Enum_ReadByReference([Field(nameof(EnumAndInterfaceBindingTargets.EnumField))]
        ref BindingEnum field) => EnumObserved = field;

    [Prefix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.Void))]
    public static void Prefix_FieldAttribute_Enum_WriteByReference([Field(nameof(EnumAndInterfaceBindingTargets.EnumField))]
        ref BindingEnum field) => field = BindingEnum.Replacement;

    [Prefix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.Void))]
    public static void Prefix_FieldAttribute_Interface_ReadByValue(
        [Field(nameof(EnumAndInterfaceBindingTargets.InterfaceField))] IBindingInterface field) =>
        InterfaceObserved = field;

    [Prefix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.Void))]
    public static void Prefix_FieldAttribute_Interface_ReadByReference(
        [Field(nameof(EnumAndInterfaceBindingTargets.InterfaceField))] ref IBindingInterface field) =>
        InterfaceObserved = field;

    [Prefix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.Void))]
    public static void Prefix_FieldAttribute_Interface_WriteByReference(
        [Field(nameof(EnumAndInterfaceBindingTargets.InterfaceField))] ref IBindingInterface field) =>
        field = InterfaceReplacement!;

    [Prefix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.Void))]
    public static void Prefix_FieldAttribute_ConcreteClass_AsInterface_ReadByValue(
        [Field(nameof(EnumAndInterfaceBindingTargets.ConcreteInterfaceField))] IBindingInterface field) =>
        InterfaceObserved = field;

    [Prefix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.Void))]
    public static void Prefix_FieldAttribute_ConcreteClass_AsInterface_ReadByReadonlyReference(
        [Field(nameof(EnumAndInterfaceBindingTargets.ConcreteInterfaceField))] in IBindingInterface field) =>
        InterfaceObserved = field;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Enum_ReadByValue_Prefix([State] out BindingEnum state) =>
        state = BindingEnum.Original;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Enum_ReadByValue_Postfix([State] BindingEnum state) =>
        EnumObserved = state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Enum_ReadByReference_Prefix([State] out BindingEnum state) =>
        state = BindingEnum.Original;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Enum_ReadByReference_Postfix([State] ref BindingEnum state) =>
        EnumObserved = state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Enum_WriteByReference_Prefix([State] out BindingEnum state) =>
        state = BindingEnum.Original;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Enum_WriteByReference_FirstPostfix([State] ref BindingEnum state) =>
        state = BindingEnum.Replacement;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Enum_WriteByReference_SecondPostfix([State] BindingEnum state) =>
        EnumObserved = state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Interface_ReadByValue_Prefix([State] out IBindingInterface state) =>
        state = new BindingInterfaceValue(1);

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Interface_ReadByValue_Postfix([State] IBindingInterface state) =>
        InterfaceObserved = state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Interface_ReadByReference_Prefix([State] out IBindingInterface state) =>
        state = new BindingInterfaceValue(1);

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Interface_ReadByReference_Postfix([State] ref IBindingInterface state) =>
        InterfaceObserved = state;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Interface_WriteByReference_Prefix([State] out IBindingInterface state) =>
        state = new BindingInterfaceValue(1);

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Interface_WriteByReference_FirstPostfix(
        [State] ref IBindingInterface state) => state = new BindingInterfaceValue(42);

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Postfix_StateAttribute_Interface_WriteByReference_SecondPostfix([State] IBindingInterface state) =>
        InterfaceObserved = state;

    [Prefix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.Void))]
    public static void Prefix_InstanceAttribute_Interface_ReadByValue([Instance] IBindingInterface instance) =>
        InterfaceObserved = instance;

    [Prefix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.Void))]
    public static void Prefix_InstanceAttribute_Interface_ReadByReadonlyReference(
        [Instance] in IBindingInterface instance) => InterfaceObserved = instance;

    [Prefix]
    [Target(typeof(EnumAndInterfaceCapturedVariableTargets), "EnumCapturedVariable.LocalMethod")]
    public static void Prefix_CapturedVariable_Enum_ReadByValue(BindingEnum captured) => EnumObserved = captured;

    [Prefix]
    [Target(typeof(EnumAndInterfaceCapturedVariableTargets), "EnumCapturedVariable.LocalMethod")]
    public static void Prefix_CapturedVariable_Enum_ReadByReference(ref BindingEnum captured) => EnumObserved = captured;

    [Prefix]
    [Target(typeof(EnumAndInterfaceCapturedVariableTargets), "EnumCapturedVariable.LocalMethod")]
    public static void Prefix_CapturedVariable_Enum_WriteByReference(ref BindingEnum captured) =>
        captured = BindingEnum.Replacement;

    [Prefix]
    [Target(typeof(EnumAndInterfaceCapturedVariableTargets), "InterfaceCapturedVariable.LocalMethod")]
    public static void Prefix_CapturedVariable_Interface_ReadByValue(IBindingInterface captured) =>
        InterfaceObserved = captured;

    [Prefix]
    [Target(typeof(EnumAndInterfaceCapturedVariableTargets), "InterfaceCapturedVariable.LocalMethod")]
    public static void Prefix_CapturedVariable_Interface_ReadByReference(ref IBindingInterface captured) =>
        InterfaceObserved = captured;

    [Prefix]
    [Target(typeof(EnumAndInterfaceCapturedVariableTargets), "InterfaceCapturedVariable.LocalMethod")]
    public static void Prefix_CapturedVariable_Interface_WriteByReference(ref IBindingInterface captured) =>
        captured = InterfaceReplacement!;

    [Prefix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.Void))]
    public static void Prefix_MethodAttribute_EnumSignature_InvokesDelegate(
        [Method(nameof(EnumAndInterfaceBindingTargets.BoundEnumMethod))] Func<BindingEnum, BindingEnum> method) =>
        EnumObserved = method(BindingEnum.Replacement);

    [Prefix]
    [Target(typeof(EnumAndInterfaceBindingTargets), nameof(EnumAndInterfaceBindingTargets.Void))]
    public static void Prefix_MethodAttribute_InterfaceSignature_InvokesDelegate(
        [Method(nameof(EnumAndInterfaceBindingTargets.BoundInterfaceMethod))]
        Func<IBindingInterface, IBindingInterface> method) =>
        InterfaceObserved = method(new BindingInterfaceValue(42));

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowInvalidOperationException))]
    public static void Postfix_ExceptionAttribute_Interface_ReadByValue(
        [Exception] System.Runtime.Serialization.ISerializable exception) => ExceptionObserved = exception;
}

[TestFixture]
public sealed class EnumAndInterfaceBindingTests : PatchTestBase
{
    [Test]
    public void Prefix_ParameterAttribute_Enum_ReadByValue()
    {
        EnumAndInterfaceBindingPatches.EnumObserved = default;
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_ParameterAttribute_Enum_ReadByValue));

        BindingEnum result = EnumAndInterfaceBindingTargets.EnumIdentity(BindingEnum.Original);

        Assert.That(result, Is.EqualTo(BindingEnum.Original));
        Assert.That(EnumAndInterfaceBindingPatches.EnumObserved, Is.EqualTo(BindingEnum.Original));
    }

    [Test]
    public void Prefix_ParameterAttribute_Enum_ReadByReference()
    {
        EnumAndInterfaceBindingPatches.EnumObserved = default;
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_ParameterAttribute_Enum_ReadByReference));

        BindingEnum result = EnumAndInterfaceBindingTargets.EnumIdentity(BindingEnum.Original);

        Assert.That(result, Is.EqualTo(BindingEnum.Original));
        Assert.That(EnumAndInterfaceBindingPatches.EnumObserved, Is.EqualTo(BindingEnum.Original));
    }

    [Test]
    public void Prefix_ParameterAttribute_Enum_WriteByReference()
    {
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_ParameterAttribute_Enum_WriteByReference));

        BindingEnum result = EnumAndInterfaceBindingTargets.EnumIdentity(BindingEnum.Original);

        Assert.That(result, Is.EqualTo(BindingEnum.Replacement));
    }

    [Test]
    public void Prefix_ParameterAttribute_Interface_ReadByValue()
    {
        EnumAndInterfaceBindingPatches.InterfaceObserved = null;
        var value = new BindingInterfaceValue(1);
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_ParameterAttribute_Interface_ReadByValue));

        IBindingInterface result = EnumAndInterfaceBindingTargets.InterfaceIdentity(value);

        Assert.That(result, Is.SameAs(value));
        Assert.That(EnumAndInterfaceBindingPatches.InterfaceObserved, Is.SameAs(value));
    }

    [Test]
    public void Prefix_ParameterAttribute_Interface_ReadByReference()
    {
        EnumAndInterfaceBindingPatches.InterfaceObserved = null;
        var value = new BindingInterfaceValue(1);
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_ParameterAttribute_Interface_ReadByReference));

        IBindingInterface result = EnumAndInterfaceBindingTargets.InterfaceIdentity(value);

        Assert.That(result, Is.SameAs(value));
        Assert.That(EnumAndInterfaceBindingPatches.InterfaceObserved, Is.SameAs(value));
    }

    [Test]
    public void Prefix_ParameterAttribute_Interface_WriteByReference()
    {
        var original = new BindingInterfaceValue(1);
        var replacement = new BindingInterfaceValue(42);
        EnumAndInterfaceBindingPatches.InterfaceReplacement = replacement;
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_ParameterAttribute_Interface_WriteByReference));

        IBindingInterface result = EnumAndInterfaceBindingTargets.InterfaceIdentity(original);

        Assert.That(result, Is.SameAs(replacement));
    }

    [Test]
    public void Prefix_ParameterAttribute_ConcreteClass_AsInterface_ReadByValue()
    {
        EnumAndInterfaceBindingPatches.InterfaceObserved = null;
        var value = new BindingInterfaceValue(1);
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_ParameterAttribute_ConcreteClass_AsInterface_ReadByValue));

        BindingInterfaceValue result = EnumAndInterfaceBindingTargets.ConcreteInterfaceIdentity(value);

        Assert.That(result, Is.SameAs(value));
        Assert.That(EnumAndInterfaceBindingPatches.InterfaceObserved, Is.SameAs(value));
    }

    [Test]
    public void Prefix_ParameterAttribute_ConcreteClass_AsInterface_ReadByReadonlyReference()
    {
        EnumAndInterfaceBindingPatches.InterfaceObserved = null;
        var value = new BindingInterfaceValue(1);
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches), nameof(EnumAndInterfaceBindingPatches
            .Prefix_ParameterAttribute_ConcreteClass_AsInterface_ReadByReadonlyReference));

        BindingInterfaceValue result = EnumAndInterfaceBindingTargets.ConcreteInterfaceIdentity(value);

        Assert.That(result, Is.SameAs(value));
        Assert.That(EnumAndInterfaceBindingPatches.InterfaceObserved, Is.SameAs(value));
    }

    [Test]
    public void Postfix_ReturnValueAttribute_Enum_ReadByValue()
    {
        EnumAndInterfaceBindingPatches.EnumObserved = default;
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Postfix_ReturnValueAttribute_Enum_ReadByValue));

        BindingEnum result = EnumAndInterfaceBindingTargets.EnumResult();

        Assert.That(result, Is.EqualTo(BindingEnum.Original));
        Assert.That(EnumAndInterfaceBindingPatches.EnumObserved, Is.EqualTo(BindingEnum.Original));
    }

    [Test]
    public void Postfix_ReturnValueAttribute_Enum_ReadByReference()
    {
        EnumAndInterfaceBindingPatches.EnumObserved = default;
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Postfix_ReturnValueAttribute_Enum_ReadByReference));

        BindingEnum result = EnumAndInterfaceBindingTargets.EnumResult();

        Assert.That(result, Is.EqualTo(BindingEnum.Original));
        Assert.That(EnumAndInterfaceBindingPatches.EnumObserved, Is.EqualTo(BindingEnum.Original));
    }

    [Test]
    public void Postfix_ReturnValueAttribute_Enum_WriteByReference()
    {
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Postfix_ReturnValueAttribute_Enum_WriteByReference));

        BindingEnum result = EnumAndInterfaceBindingTargets.EnumResult();

        Assert.That(result, Is.EqualTo(BindingEnum.Replacement));
    }

    [Test]
    public void Postfix_ReturnValueAttribute_Interface_ReadByValue()
    {
        EnumAndInterfaceBindingPatches.InterfaceObserved = null;
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Postfix_ReturnValueAttribute_Interface_ReadByValue));

        IBindingInterface result = EnumAndInterfaceBindingTargets.InterfaceResult();

        Assert.That(EnumAndInterfaceBindingPatches.InterfaceObserved, Is.SameAs(result));
    }

    [Test]
    public void Postfix_ReturnValueAttribute_Interface_ReadByReference()
    {
        EnumAndInterfaceBindingPatches.InterfaceObserved = null;
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Postfix_ReturnValueAttribute_Interface_ReadByReference));

        IBindingInterface result = EnumAndInterfaceBindingTargets.InterfaceResult();

        Assert.That(EnumAndInterfaceBindingPatches.InterfaceObserved, Is.SameAs(result));
    }

    [Test]
    public void Postfix_ReturnValueAttribute_Interface_WriteByReference()
    {
        var replacement = new BindingInterfaceValue(42);
        EnumAndInterfaceBindingPatches.InterfaceReplacement = replacement;
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Postfix_ReturnValueAttribute_Interface_WriteByReference));

        IBindingInterface result = EnumAndInterfaceBindingTargets.InterfaceResult();

        Assert.That(result, Is.SameAs(replacement));
    }

    [Test]
    public void Postfix_ReturnValueAttribute_ConcreteClass_AsInterface_ReadByValue()
    {
        EnumAndInterfaceBindingPatches.InterfaceObserved = null;
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Postfix_ReturnValueAttribute_ConcreteClass_AsInterface_ReadByValue));

        BindingInterfaceValue result = EnumAndInterfaceBindingTargets.ConcreteInterfaceResult();

        Assert.That(EnumAndInterfaceBindingPatches.InterfaceObserved, Is.SameAs(result));
    }

    [Test]
    public void Postfix_ReturnValueAttribute_ConcreteClass_AsInterface_ReadByReadonlyReference()
    {
        EnumAndInterfaceBindingPatches.InterfaceObserved = null;
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches), nameof(EnumAndInterfaceBindingPatches
            .Postfix_ReturnValueAttribute_ConcreteClass_AsInterface_ReadByReadonlyReference));

        BindingInterfaceValue result = EnumAndInterfaceBindingTargets.ConcreteInterfaceResult();

        Assert.That(EnumAndInterfaceBindingPatches.InterfaceObserved, Is.SameAs(result));
    }

    [Test]
    public void Prefix_FieldAttribute_Enum_ReadByValue()
    {
        EnumAndInterfaceBindingPatches.EnumObserved = default;
        var target = new EnumAndInterfaceBindingTargets();
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_FieldAttribute_Enum_ReadByValue));

        target.Void();

        Assert.That(EnumAndInterfaceBindingPatches.EnumObserved, Is.EqualTo(BindingEnum.Original));
    }

    [Test]
    public void Prefix_FieldAttribute_Enum_ReadByReference()
    {
        EnumAndInterfaceBindingPatches.EnumObserved = default;
        var target = new EnumAndInterfaceBindingTargets();
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_FieldAttribute_Enum_ReadByReference));

        target.Void();

        Assert.That(EnumAndInterfaceBindingPatches.EnumObserved, Is.EqualTo(BindingEnum.Original));
    }

    [Test]
    public void Prefix_FieldAttribute_Enum_WriteByReference()
    {
        var target = new EnumAndInterfaceBindingTargets();
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_FieldAttribute_Enum_WriteByReference));

        target.Void();

        Assert.That(target.EnumField, Is.EqualTo(BindingEnum.Replacement));
    }

    [Test]
    public void Prefix_FieldAttribute_Interface_ReadByValue()
    {
        EnumAndInterfaceBindingPatches.InterfaceObserved = null;
        var target = new EnumAndInterfaceBindingTargets();
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_FieldAttribute_Interface_ReadByValue));

        target.Void();

        Assert.That(EnumAndInterfaceBindingPatches.InterfaceObserved, Is.SameAs(target.InterfaceField));
    }

    [Test]
    public void Prefix_FieldAttribute_Interface_ReadByReference()
    {
        EnumAndInterfaceBindingPatches.InterfaceObserved = null;
        var target = new EnumAndInterfaceBindingTargets();
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_FieldAttribute_Interface_ReadByReference));

        target.Void();

        Assert.That(EnumAndInterfaceBindingPatches.InterfaceObserved, Is.SameAs(target.InterfaceField));
    }

    [Test]
    public void Prefix_FieldAttribute_Interface_WriteByReference()
    {
        var replacement = new BindingInterfaceValue(42);
        var target = new EnumAndInterfaceBindingTargets();
        EnumAndInterfaceBindingPatches.InterfaceReplacement = replacement;
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_FieldAttribute_Interface_WriteByReference));

        target.Void();

        Assert.That(target.InterfaceField, Is.SameAs(replacement));
    }

    [Test]
    public void Prefix_FieldAttribute_ConcreteClass_AsInterface_ReadByValue()
    {
        EnumAndInterfaceBindingPatches.InterfaceObserved = null;
        var target = new EnumAndInterfaceBindingTargets();
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_FieldAttribute_ConcreteClass_AsInterface_ReadByValue));

        target.Void();

        Assert.That(EnumAndInterfaceBindingPatches.InterfaceObserved, Is.SameAs(target.ConcreteInterfaceField));
    }

    [Test]
    public void Prefix_FieldAttribute_ConcreteClass_AsInterface_ReadByReadonlyReference()
    {
        EnumAndInterfaceBindingPatches.InterfaceObserved = null;
        var target = new EnumAndInterfaceBindingTargets();
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches), nameof(EnumAndInterfaceBindingPatches
            .Prefix_FieldAttribute_ConcreteClass_AsInterface_ReadByReadonlyReference));

        target.Void();

        Assert.That(EnumAndInterfaceBindingPatches.InterfaceObserved, Is.SameAs(target.ConcreteInterfaceField));
    }

    [Test]
    public void Postfix_StateAttribute_Enum_ReadByValue()
    {
        EnumAndInterfaceBindingPatches.EnumObserved = default;
        Patcher.Patch(
            typeof(EnumAndInterfaceBindingPatches).GetMethod(
                nameof(EnumAndInterfaceBindingPatches.Postfix_StateAttribute_Enum_ReadByValue_Prefix))!,
            typeof(EnumAndInterfaceBindingPatches).GetMethod(
                nameof(EnumAndInterfaceBindingPatches.Postfix_StateAttribute_Enum_ReadByValue_Postfix))!);

        StaticMethodTargets.Void();

        Assert.That(EnumAndInterfaceBindingPatches.EnumObserved, Is.EqualTo(BindingEnum.Original));
    }

    [Test]
    public void Postfix_StateAttribute_Enum_ReadByReference()
    {
        EnumAndInterfaceBindingPatches.EnumObserved = default;
        Patcher.Patch(
            typeof(EnumAndInterfaceBindingPatches).GetMethod(
                nameof(EnumAndInterfaceBindingPatches.Postfix_StateAttribute_Enum_ReadByReference_Prefix))!,
            typeof(EnumAndInterfaceBindingPatches).GetMethod(
                nameof(EnumAndInterfaceBindingPatches.Postfix_StateAttribute_Enum_ReadByReference_Postfix))!);

        StaticMethodTargets.Void();

        Assert.That(EnumAndInterfaceBindingPatches.EnumObserved, Is.EqualTo(BindingEnum.Original));
    }

    [Test]
    public void Postfix_StateAttribute_Enum_WriteByReference()
    {
        EnumAndInterfaceBindingPatches.EnumObserved = default;
        Patcher.Patch(
            typeof(EnumAndInterfaceBindingPatches).GetMethod(
                nameof(EnumAndInterfaceBindingPatches.Postfix_StateAttribute_Enum_WriteByReference_Prefix))!,
            typeof(EnumAndInterfaceBindingPatches).GetMethod(
                nameof(EnumAndInterfaceBindingPatches.Postfix_StateAttribute_Enum_WriteByReference_FirstPostfix))!,
            typeof(EnumAndInterfaceBindingPatches).GetMethod(
                nameof(EnumAndInterfaceBindingPatches.Postfix_StateAttribute_Enum_WriteByReference_SecondPostfix))!);

        StaticMethodTargets.Void();

        Assert.That(EnumAndInterfaceBindingPatches.EnumObserved, Is.EqualTo(BindingEnum.Replacement));
    }

    [Test]
    public void Postfix_StateAttribute_Interface_ReadByValue()
    {
        EnumAndInterfaceBindingPatches.InterfaceObserved = null;
        Patcher.Patch(
            typeof(EnumAndInterfaceBindingPatches).GetMethod(
                nameof(EnumAndInterfaceBindingPatches.Postfix_StateAttribute_Interface_ReadByValue_Prefix))!,
            typeof(EnumAndInterfaceBindingPatches).GetMethod(
                nameof(EnumAndInterfaceBindingPatches.Postfix_StateAttribute_Interface_ReadByValue_Postfix))!);

        StaticMethodTargets.Void();

        Assert.That(EnumAndInterfaceBindingPatches.InterfaceObserved, Is.TypeOf<BindingInterfaceValue>()
            .With.Property(nameof(BindingInterfaceValue.Value)).EqualTo(1));
    }

    [Test]
    public void Postfix_StateAttribute_Interface_ReadByReference()
    {
        EnumAndInterfaceBindingPatches.InterfaceObserved = null;
        Patcher.Patch(
            typeof(EnumAndInterfaceBindingPatches).GetMethod(
                nameof(EnumAndInterfaceBindingPatches.Postfix_StateAttribute_Interface_ReadByReference_Prefix))!,
            typeof(EnumAndInterfaceBindingPatches).GetMethod(
                nameof(EnumAndInterfaceBindingPatches.Postfix_StateAttribute_Interface_ReadByReference_Postfix))!);

        StaticMethodTargets.Void();

        Assert.That(EnumAndInterfaceBindingPatches.InterfaceObserved, Is.TypeOf<BindingInterfaceValue>()
            .With.Property(nameof(BindingInterfaceValue.Value)).EqualTo(1));
    }

    [Test]
    public void Postfix_StateAttribute_Interface_WriteByReference()
    {
        EnumAndInterfaceBindingPatches.InterfaceObserved = null;
        Patcher.Patch(
            typeof(EnumAndInterfaceBindingPatches).GetMethod(
                nameof(EnumAndInterfaceBindingPatches.Postfix_StateAttribute_Interface_WriteByReference_Prefix))!,
            typeof(EnumAndInterfaceBindingPatches).GetMethod(
                nameof(EnumAndInterfaceBindingPatches.Postfix_StateAttribute_Interface_WriteByReference_FirstPostfix))!,
            typeof(EnumAndInterfaceBindingPatches).GetMethod(
                nameof(EnumAndInterfaceBindingPatches.Postfix_StateAttribute_Interface_WriteByReference_SecondPostfix))!);

        StaticMethodTargets.Void();

        Assert.That(EnumAndInterfaceBindingPatches.InterfaceObserved, Is.TypeOf<BindingInterfaceValue>()
            .With.Property(nameof(BindingInterfaceValue.Value)).EqualTo(42));
    }

    [Test]
    public void Prefix_InstanceAttribute_Interface_ReadByValue()
    {
        EnumAndInterfaceBindingPatches.InterfaceObserved = null;
        var target = new EnumAndInterfaceBindingTargets { Value = 42 };
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_InstanceAttribute_Interface_ReadByValue));

        target.Void();

        Assert.That(EnumAndInterfaceBindingPatches.InterfaceObserved, Is.SameAs(target));
    }

    [Test]
    public void Prefix_InstanceAttribute_Interface_ReadByReadonlyReference()
    {
        EnumAndInterfaceBindingPatches.InterfaceObserved = null;
        var target = new EnumAndInterfaceBindingTargets { Value = 42 };
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_InstanceAttribute_Interface_ReadByReadonlyReference));

        target.Void();

        Assert.That(EnumAndInterfaceBindingPatches.InterfaceObserved, Is.SameAs(target));
    }

    [Test]
    public void Prefix_CapturedVariable_Enum_ReadByValue()
    {
        EnumAndInterfaceBindingPatches.EnumObserved = default;
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_CapturedVariable_Enum_ReadByValue));

        BindingEnum result = EnumAndInterfaceCapturedVariableTargets.EnumCapturedVariable(BindingEnum.Original);

        Assert.That(result, Is.EqualTo(BindingEnum.Original));
        Assert.That(EnumAndInterfaceBindingPatches.EnumObserved, Is.EqualTo(BindingEnum.Original));
    }

    [Test]
    public void Prefix_CapturedVariable_Enum_ReadByReference()
    {
        EnumAndInterfaceBindingPatches.EnumObserved = default;
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_CapturedVariable_Enum_ReadByReference));

        BindingEnum result = EnumAndInterfaceCapturedVariableTargets.EnumCapturedVariable(BindingEnum.Original);

        Assert.That(result, Is.EqualTo(BindingEnum.Original));
        Assert.That(EnumAndInterfaceBindingPatches.EnumObserved, Is.EqualTo(BindingEnum.Original));
    }

    [Test]
    public void Prefix_CapturedVariable_Enum_WriteByReference()
    {
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_CapturedVariable_Enum_WriteByReference));

        BindingEnum result = EnumAndInterfaceCapturedVariableTargets.EnumCapturedVariable(BindingEnum.Original);

        Assert.That(result, Is.EqualTo(BindingEnum.Replacement));
    }

    [Test]
    public void Prefix_CapturedVariable_Interface_ReadByValue()
    {
        EnumAndInterfaceBindingPatches.InterfaceObserved = null;
        var value = new BindingInterfaceValue(1);
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_CapturedVariable_Interface_ReadByValue));

        IBindingInterface result = EnumAndInterfaceCapturedVariableTargets.InterfaceCapturedVariable(value);

        Assert.That(result, Is.SameAs(value));
        Assert.That(EnumAndInterfaceBindingPatches.InterfaceObserved, Is.SameAs(value));
    }

    [Test]
    public void Prefix_CapturedVariable_Interface_ReadByReference()
    {
        EnumAndInterfaceBindingPatches.InterfaceObserved = null;
        var value = new BindingInterfaceValue(1);
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_CapturedVariable_Interface_ReadByReference));

        IBindingInterface result = EnumAndInterfaceCapturedVariableTargets.InterfaceCapturedVariable(value);

        Assert.That(result, Is.SameAs(value));
        Assert.That(EnumAndInterfaceBindingPatches.InterfaceObserved, Is.SameAs(value));
    }

    [Test]
    public void Prefix_CapturedVariable_Interface_WriteByReference()
    {
        var original = new BindingInterfaceValue(1);
        var replacement = new BindingInterfaceValue(42);
        EnumAndInterfaceBindingPatches.InterfaceReplacement = replacement;
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_CapturedVariable_Interface_WriteByReference));

        IBindingInterface result = EnumAndInterfaceCapturedVariableTargets.InterfaceCapturedVariable(original);

        Assert.That(result, Is.SameAs(replacement));
    }

    [Test]
    public void Prefix_MethodAttribute_EnumSignature_InvokesDelegate()
    {
        EnumAndInterfaceBindingPatches.EnumObserved = default;
        var target = new EnumAndInterfaceBindingTargets();
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_MethodAttribute_EnumSignature_InvokesDelegate));

        target.Void();

        Assert.That(EnumAndInterfaceBindingPatches.EnumObserved, Is.EqualTo(BindingEnum.Replacement));
    }

    [Test]
    public void Prefix_MethodAttribute_InterfaceSignature_InvokesDelegate()
    {
        EnumAndInterfaceBindingPatches.InterfaceObserved = null;
        var target = new EnumAndInterfaceBindingTargets();
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Prefix_MethodAttribute_InterfaceSignature_InvokesDelegate));

        target.Void();

        Assert.That(EnumAndInterfaceBindingPatches.InterfaceObserved, Is.TypeOf<BindingInterfaceValue>()
            .With.Property(nameof(BindingInterfaceValue.Value)).EqualTo(42));
    }

    [Test]
    public void Postfix_ExceptionAttribute_Interface_ReadByValue()
    {
        EnumAndInterfaceBindingPatches.ExceptionObserved = null;
        ApplyPatch(typeof(EnumAndInterfaceBindingPatches),
            nameof(EnumAndInterfaceBindingPatches.Postfix_ExceptionAttribute_Interface_ReadByValue));

        var exception = Assert.Throws<InvalidOperationException>(StaticMethodTargets.ThrowInvalidOperationException);

        Assert.That(EnumAndInterfaceBindingPatches.ExceptionObserved, Is.SameAs(exception));
    }
}
