namespace Disharmony.Tests.EndToEnd.RuleBuilders;

public static class InnerMemberAccessPatches
{
    public static InnerInstanceMethodTargets? instanceObserved;
    public static InnerInstanceMethodTargets? replacementInstance;
    public static InnerStructMethodTargets structInstanceObserved;
    public static int valueObserved;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Field), MemberType.Getter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.FieldResult))]
    public static bool InnerPrefixCanReplaceStaticFieldRead(ref int __result)
    {
        __result = 42;
        return false;
    }

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Field), MemberType.Getter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.FieldResult))]
    public static void InnerPostfixCanReplaceStaticFieldRead(ref int __result) => __result = 42;

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Property), MemberType.Getter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.PropertyResult))]
    public static void InnerPostfixCanReplacePropertyGetterResult(ref int __result) => __result = 42;

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.EnumerateIntResult))]
    public static void InnerPostfixCanPatchCallInsideIteratorStateMachine(ref int __result) => __result = 42;

    [Postfix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.foo), MemberType.Getter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.ReadInstanceField))]
    public static void InnerPostfixCanReplaceInstanceFieldRead(ref int __result) => __result = 42;

    [Prefix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.foo), MemberType.Getter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.ReadInstanceField))]
    public static void InnerPrefix_InstanceField_ReferenceType_Instance_ReadByReference(
        [Instance(Scope.Inner)] ref InnerInstanceMethodTargets instance) => instanceObserved = instance;

    [Prefix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.foo), MemberType.Getter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.ReadInstanceField))]
    public static void InnerPrefix_InstanceField_ReferenceType_Instance_WriteByReference(
        [Instance(Scope.Inner)] ref InnerInstanceMethodTargets instance) => instance = replacementInstance!;

    [Postfix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.foo), MemberType.Getter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.ReadInstanceField))]
    public static void InnerPostfix_InstanceField_ReferenceType_Instance_ReadByReference(
        [Instance(Scope.Inner)] ref InnerInstanceMethodTargets instance) => instanceObserved = instance;

    [Postfix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.foo), MemberType.Getter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.ReadInstanceField))]
    public static void InnerPostfix_InstanceField_ReferenceType_Instance_WriteByReference(
        [Instance(Scope.Inner)] ref InnerInstanceMethodTargets instance)
    {
        instance = replacementInstance!;
        instanceObserved = instance;
    }

    [Postfix] [Inner(typeof(InnerStructMethodTargets), nameof(InnerStructMethodTargets.foo), MemberType.Getter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.ReadStructField))]
    public static void InnerPostfixCanReplaceStructFieldRead(ref int __result) => __result = 42;

    [Prefix] [Inner(typeof(InnerStructMethodTargets), nameof(InnerStructMethodTargets.foo), MemberType.Getter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.ReadStructField))]
    public static void InnerPrefix_InstanceField_Struct_Instance_ReadByReference(
        [Instance(Scope.Inner)] ref InnerStructMethodTargets instance) => structInstanceObserved = instance;

    [Prefix] [Inner(typeof(InnerStructMethodTargets), nameof(InnerStructMethodTargets.foo), MemberType.Getter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.ReadStructField))]
    public static void InnerPrefix_InstanceField_Struct_Instance_WriteByReference(
        [Instance(Scope.Inner)] ref InnerStructMethodTargets instance) => instance.foo = 42;

    [Postfix] [Inner(typeof(InnerStructMethodTargets), nameof(InnerStructMethodTargets.foo), MemberType.Getter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.ReadStructField))]
    public static void InnerPostfix_InstanceField_Struct_Instance_ReadByReference(
        [Instance(Scope.Inner)] ref InnerStructMethodTargets instance) => structInstanceObserved = instance;

    [Postfix] [Inner(typeof(InnerStructMethodTargets), nameof(InnerStructMethodTargets.foo), MemberType.Getter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.ReadStructField))]
    public static void InnerPostfix_InstanceField_Struct_Instance_WriteByReference(
        [Instance(Scope.Inner)] ref InnerStructMethodTargets instance)
    {
        instance.foo = 42;
        structInstanceObserved = instance;
    }

    [Postfix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.Property), MemberType.Getter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.ReadInstanceProperty))]
    public static void InnerPostfix_InstanceProperty_ReferenceType_Instance_ReadByReference_Rejected(
        [Instance(Scope.Inner)] ref InnerInstanceMethodTargets instance) => instanceObserved = instance;

    [Postfix] [Inner(typeof(InnerStructMethodTargets), nameof(InnerStructMethodTargets.Property), MemberType.Getter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.ReadStructProperty))]
    public static void InnerPostfix_InstanceProperty_Struct_Instance_ReadByReference(
        [Instance(Scope.Inner)] ref InnerStructMethodTargets instance) => structInstanceObserved = instance;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Field), MemberType.Setter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.SetStaticField))]
    public static void InnerPrefix_StaticFieldSetter_Primitive_Value_WriteByReference(ref int value) => value = 42;

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Field), MemberType.Setter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.SetStaticField))]
    public static void InnerPostfix_StaticFieldSetter_Primitive_Value_ReadByValue(int value) => valueObserved = value;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Field), MemberType.Setter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.SetStaticField))]
    public static bool InnerPrefix_StaticFieldSetter_Primitive_SkipWrite() => false;

    [Prefix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.foo), MemberType.Setter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.SetInstanceField))]
    public static void InnerPrefix_InstanceFieldSetter_ReferenceType_Instance_ReadByValue_Value_WriteByReference(
        [Instance(Scope.Inner)] InnerInstanceMethodTargets instance,
        ref int value)
    {
        instanceObserved = instance;
        value = 42;
    }

    [Postfix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.foo), MemberType.Setter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.SetInstanceField))]
    public static void InnerPostfix_InstanceFieldSetter_ReferenceType_Instance_ReadByValue_Value_ReadByValue(
        [Instance(Scope.Inner)] InnerInstanceMethodTargets instance,
        int value)
    {
        instanceObserved = instance;
        valueObserved = value;
    }

    [Prefix] [Inner(typeof(InnerStructMethodTargets), nameof(InnerStructMethodTargets.foo), MemberType.Setter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.SetStructField))]
    public static void InnerPrefix_InstanceFieldSetter_Struct_Instance_ReadByReference_Value_WriteByReference(
        [Instance(Scope.Inner)] ref InnerStructMethodTargets instance,
        ref int value)
    {
        structInstanceObserved = instance;
        value = 42;
    }

    [Postfix] [Inner(typeof(InnerStructMethodTargets), nameof(InnerStructMethodTargets.foo), MemberType.Setter)]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.SetStructField))]
    public static void InnerPostfix_InstanceFieldSetter_Struct_Instance_ReadByReference_Value_ReadByValue(
        [Instance(Scope.Inner)] ref InnerStructMethodTargets instance,
        int value)
    {
        structInstanceObserved = instance;
        valueObserved = value;
    }
}

[TestFixture]
public sealed class InnerMemberAccessTests : PatchTestBase
{
    [Test]
    public void InnerPrefixCanReplaceStaticFieldRead()
    {
        InnerStaticMethodTargets.Field = 1;
        ApplyPatch(typeof(InnerMemberAccessPatches), nameof(InnerMemberAccessPatches.InnerPrefixCanReplaceStaticFieldRead));

        Assert.That(OuterStaticMethodTargets.FieldResult(), Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfixCanReplaceStaticFieldRead()
    {
        InnerStaticMethodTargets.Field = 1;
        ApplyPatch(typeof(InnerMemberAccessPatches), nameof(InnerMemberAccessPatches.InnerPostfixCanReplaceStaticFieldRead));

        Assert.That(OuterStaticMethodTargets.FieldResult(), Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfixCanReplacePropertyGetterResult()
    {
        ApplyPatch(typeof(InnerMemberAccessPatches), nameof(InnerMemberAccessPatches.InnerPostfixCanReplacePropertyGetterResult));

        Assert.That(OuterStaticMethodTargets.PropertyResult(), Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfixCanPatchCallInsideIteratorStateMachine()
    {
        ApplyPatch(typeof(InnerMemberAccessPatches), nameof(InnerMemberAccessPatches.InnerPostfixCanPatchCallInsideIteratorStateMachine));

        int result = OuterStaticMethodTargets.EnumerateIntResult().Single();

        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfixCanReplaceInstanceFieldRead()
    {
        ApplyPatch(typeof(InnerMemberAccessPatches), nameof(InnerMemberAccessPatches.InnerPostfixCanReplaceInstanceFieldRead));
        var inner = new InnerInstanceMethodTargets { foo = 1 };

        int result = OuterStaticMethodTargets.ReadInstanceField(inner);

        Assert.That(result, Is.EqualTo(42));
        Assert.That(inner.foo, Is.EqualTo(1));
    }

    [Test]
    public void InnerPrefix_InstanceField_ReferenceType_Instance_ReadByReference()
    {
        InnerMemberAccessPatches.instanceObserved = null;
        var inner = new InnerInstanceMethodTargets { foo = 42 };
        ApplyPatch(
            typeof(InnerMemberAccessPatches),
            nameof(InnerMemberAccessPatches.InnerPrefix_InstanceField_ReferenceType_Instance_ReadByReference));

        int result = OuterStaticMethodTargets.ReadInstanceField(inner);

        Assert.That(result, Is.EqualTo(42));
        Assert.That(InnerMemberAccessPatches.instanceObserved, Is.SameAs(inner));
    }

    [Test]
    public void InnerPrefix_InstanceField_ReferenceType_Instance_WriteByReference()
    {
        var inner = new InnerInstanceMethodTargets { foo = 1 };
        var replacement = new InnerInstanceMethodTargets { foo = 42 };
        InnerMemberAccessPatches.replacementInstance = replacement;
        ApplyPatch(
            typeof(InnerMemberAccessPatches),
            nameof(InnerMemberAccessPatches.InnerPrefix_InstanceField_ReferenceType_Instance_WriteByReference));

        int result = OuterStaticMethodTargets.ReadInstanceField(inner);

        Assert.That(result, Is.EqualTo(42));
        Assert.That(inner.foo, Is.EqualTo(1));
    }

    [Test]
    public void InnerPostfix_InstanceField_ReferenceType_Instance_ReadByReference()
    {
        InnerMemberAccessPatches.instanceObserved = null;
        var inner = new InnerInstanceMethodTargets { foo = 42 };
        ApplyPatch(
            typeof(InnerMemberAccessPatches),
            nameof(InnerMemberAccessPatches.InnerPostfix_InstanceField_ReferenceType_Instance_ReadByReference));

        int result = OuterStaticMethodTargets.ReadInstanceField(inner);

        Assert.That(result, Is.EqualTo(42));
        Assert.That(InnerMemberAccessPatches.instanceObserved, Is.SameAs(inner));
    }

    [Test]
    public void InnerPostfix_InstanceField_ReferenceType_Instance_WriteByReference()
    {
        InnerMemberAccessPatches.instanceObserved = null;
        var inner = new InnerInstanceMethodTargets { foo = 1 };
        var replacement = new InnerInstanceMethodTargets { foo = 42 };
        InnerMemberAccessPatches.replacementInstance = replacement;
        ApplyPatch(
            typeof(InnerMemberAccessPatches),
            nameof(InnerMemberAccessPatches.InnerPostfix_InstanceField_ReferenceType_Instance_WriteByReference));

        int result = OuterStaticMethodTargets.ReadInstanceField(inner);

        Assert.That(result, Is.EqualTo(1));
        Assert.That(inner.foo, Is.EqualTo(1));
        Assert.That(InnerMemberAccessPatches.instanceObserved, Is.SameAs(replacement));
    }

    [Test]
    public void InnerPostfixCanReplaceStructFieldRead()
    {
        ApplyPatch(typeof(InnerMemberAccessPatches), nameof(InnerMemberAccessPatches.InnerPostfixCanReplaceStructFieldRead));
        var inner = new InnerStructMethodTargets { foo = 1 };

        int result = OuterStaticMethodTargets.ReadStructField(inner);

        Assert.That(result, Is.EqualTo(42));
        Assert.That(inner.foo, Is.EqualTo(1));
    }

    [Test]
    public void InnerPrefix_InstanceField_Struct_Instance_ReadByReference()
    {
        InnerMemberAccessPatches.structInstanceObserved = default;
        var inner = new InnerStructMethodTargets { foo = 42 };
        ApplyPatch(
            typeof(InnerMemberAccessPatches),
            nameof(InnerMemberAccessPatches.InnerPrefix_InstanceField_Struct_Instance_ReadByReference));

        int result = OuterStaticMethodTargets.ReadStructField(inner);

        Assert.That(result, Is.EqualTo(42));
        Assert.That(InnerMemberAccessPatches.structInstanceObserved.foo, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_InstanceField_Struct_Instance_WriteByReference()
    {
        var inner = new InnerStructMethodTargets { foo = 1 };
        ApplyPatch(
            typeof(InnerMemberAccessPatches),
            nameof(InnerMemberAccessPatches.InnerPrefix_InstanceField_Struct_Instance_WriteByReference));

        int result = OuterStaticMethodTargets.ReadStructField(inner);

        Assert.That(result, Is.EqualTo(42));
        Assert.That(inner.foo, Is.EqualTo(1));
    }

    [Test]
    public void InnerPostfix_InstanceField_Struct_Instance_ReadByReference()
    {
        InnerMemberAccessPatches.structInstanceObserved = default;
        var inner = new InnerStructMethodTargets { foo = 42 };
        ApplyPatch(
            typeof(InnerMemberAccessPatches),
            nameof(InnerMemberAccessPatches.InnerPostfix_InstanceField_Struct_Instance_ReadByReference));

        int result = OuterStaticMethodTargets.ReadStructField(inner);

        Assert.That(result, Is.EqualTo(42));
        Assert.That(InnerMemberAccessPatches.structInstanceObserved.foo, Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfix_InstanceField_Struct_Instance_WriteByReference()
    {
        InnerMemberAccessPatches.structInstanceObserved = default;
        var inner = new InnerStructMethodTargets { foo = 1 };
        ApplyPatch(
            typeof(InnerMemberAccessPatches),
            nameof(InnerMemberAccessPatches.InnerPostfix_InstanceField_Struct_Instance_WriteByReference));

        int result = OuterStaticMethodTargets.ReadStructField(inner);

        Assert.That(result, Is.EqualTo(1));
        Assert.That(inner.foo, Is.EqualTo(1));
        Assert.That(InnerMemberAccessPatches.structInstanceObserved.foo, Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfix_InstanceProperty_ReferenceType_Instance_ReadByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(InnerMemberAccessPatches),
                nameof(InnerMemberAccessPatches.InnerPostfix_InstanceProperty_ReferenceType_Instance_ReadByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void InnerPostfix_InstanceProperty_Struct_Instance_ReadByReference()
    {
        InnerMemberAccessPatches.structInstanceObserved = default;
        var inner = new InnerStructMethodTargets { foo = 42 };
        ApplyPatch(
            typeof(InnerMemberAccessPatches),
            nameof(InnerMemberAccessPatches.InnerPostfix_InstanceProperty_Struct_Instance_ReadByReference));

        int result = OuterStaticMethodTargets.ReadStructProperty(inner);

        Assert.That(result, Is.EqualTo(42));
        Assert.That(InnerMemberAccessPatches.structInstanceObserved.foo, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_StaticFieldSetter_Primitive_Value_WriteByReference()
    {
        InnerStaticMethodTargets.Field = 1;
        ApplyPatch(
            typeof(InnerMemberAccessPatches),
            nameof(InnerMemberAccessPatches.InnerPrefix_StaticFieldSetter_Primitive_Value_WriteByReference));

        OuterStaticMethodTargets.SetStaticField(2);

        Assert.That(InnerStaticMethodTargets.Field, Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfix_StaticFieldSetter_Primitive_Value_ReadByValue()
    {
        InnerMemberAccessPatches.valueObserved = 0;
        ApplyPatch(
            typeof(InnerMemberAccessPatches),
            nameof(InnerMemberAccessPatches.InnerPostfix_StaticFieldSetter_Primitive_Value_ReadByValue));

        OuterStaticMethodTargets.SetStaticField(42);

        Assert.That(InnerStaticMethodTargets.Field, Is.EqualTo(42));
        Assert.That(InnerMemberAccessPatches.valueObserved, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_StaticFieldSetter_Primitive_SkipWrite()
    {
        InnerStaticMethodTargets.Field = 1;
        ApplyPatch(
            typeof(InnerMemberAccessPatches),
            nameof(InnerMemberAccessPatches.InnerPrefix_StaticFieldSetter_Primitive_SkipWrite));

        OuterStaticMethodTargets.SetStaticField(42);

        Assert.That(InnerStaticMethodTargets.Field, Is.EqualTo(1));
    }

    [Test]
    public void InnerPrefix_InstanceFieldSetter_ReferenceType_Instance_ReadByValue_Value_WriteByReference()
    {
        InnerMemberAccessPatches.instanceObserved = null;
        var inner = new InnerInstanceMethodTargets { foo = 1 };
        ApplyPatch(
            typeof(InnerMemberAccessPatches),
            nameof(InnerMemberAccessPatches.InnerPrefix_InstanceFieldSetter_ReferenceType_Instance_ReadByValue_Value_WriteByReference));

        OuterStaticMethodTargets.SetInstanceField(inner, 2);

        Assert.That(inner.foo, Is.EqualTo(42));
        Assert.That(InnerMemberAccessPatches.instanceObserved, Is.SameAs(inner));
    }

    [Test]
    public void InnerPostfix_InstanceFieldSetter_ReferenceType_Instance_ReadByValue_Value_ReadByValue()
    {
        InnerMemberAccessPatches.instanceObserved = null;
        InnerMemberAccessPatches.valueObserved = 0;
        var inner = new InnerInstanceMethodTargets { foo = 1 };
        ApplyPatch(
            typeof(InnerMemberAccessPatches),
            nameof(InnerMemberAccessPatches.InnerPostfix_InstanceFieldSetter_ReferenceType_Instance_ReadByValue_Value_ReadByValue));

        OuterStaticMethodTargets.SetInstanceField(inner, 42);

        Assert.That(inner.foo, Is.EqualTo(42));
        Assert.That(InnerMemberAccessPatches.instanceObserved, Is.SameAs(inner));
        Assert.That(InnerMemberAccessPatches.valueObserved, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_InstanceFieldSetter_Struct_Instance_ReadByReference_Value_WriteByReference()
    {
        InnerMemberAccessPatches.structInstanceObserved = default;
        var inner = new InnerStructMethodTargets { foo = 1 };
        ApplyPatch(
            typeof(InnerMemberAccessPatches),
            nameof(InnerMemberAccessPatches.InnerPrefix_InstanceFieldSetter_Struct_Instance_ReadByReference_Value_WriteByReference));

        OuterStaticMethodTargets.SetStructField(ref inner, 2);

        Assert.That(inner.foo, Is.EqualTo(42));
        Assert.That(InnerMemberAccessPatches.structInstanceObserved.foo, Is.EqualTo(1));
    }

    [Test]
    public void InnerPostfix_InstanceFieldSetter_Struct_Instance_ReadByReference_Value_ReadByValue()
    {
        InnerMemberAccessPatches.structInstanceObserved = default;
        InnerMemberAccessPatches.valueObserved = 0;
        var inner = new InnerStructMethodTargets { foo = 1 };
        ApplyPatch(
            typeof(InnerMemberAccessPatches),
            nameof(InnerMemberAccessPatches.InnerPostfix_InstanceFieldSetter_Struct_Instance_ReadByReference_Value_ReadByValue));

        OuterStaticMethodTargets.SetStructField(ref inner, 42);

        Assert.That(inner.foo, Is.EqualTo(42));
        Assert.That(InnerMemberAccessPatches.structInstanceObserved.foo, Is.EqualTo(42));
        Assert.That(InnerMemberAccessPatches.valueObserved, Is.EqualTo(42));
    }
}
