// ReSharper disable ReturnValueOfPureMethodIsNotUsed
namespace Disharmony.Tests.EndToEnd.ParameterBinding;

public static partial class IteratorParameterBindingPatches
{
    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateIdentity))]
    public static void InnerPrefix_ParameterAttribute_InstanceIteratorOriginalParameter_Name_Primitive_ReadByValue(
        [Argument("outerValue", Scope.Outer)] int namedValue) => parameterObserved = namedValue;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.EnumerateIdentity))]
    public static void InnerPrefix_ParameterAttribute_StaticIteratorOriginalParameter_Name_Primitive_ReadByValue(
        [Argument("outerValue", Scope.Outer)] int namedValue) => parameterObserved = namedValue;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateIdentity))]
    public static void InnerPrefix_StateAttribute_Iterator_OuterScope_Rejected([State] int state) { }

    public static int parameterObserved;
    public static int fieldObserved;
    public static ClassMethodTargets? instanceObserved;
    public static BindingReference? referenceObserved;
    public static BindingStruct structObserved;
    public static ClassMethodTargets? replacementInstance;
    public static int structInstanceFieldObserved;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateIdentity))]
    public static void InnerPrefix_IteratorOriginalParameter_Primitive_ReadByValue(int outerValue) => parameterObserved = outerValue;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateIdentity))]
    public static void InnerPrefix_ParameterAttribute_InstanceIteratorOriginalParameter_Index0_Primitive_ReadByValue(
        [Argument(0, Scope.Outer)] int indexedValue) => parameterObserved = indexedValue;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.EnumerateIdentity))]
    public static void InnerPrefix_ParameterAttribute_StaticIteratorSingleParameter_Index0_Primitive_ReadByValue(
        [Argument(0, Scope.Outer)] int indexedValue) => parameterObserved = indexedValue;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.EnumeratePair))]
    public static void InnerPrefix_ParameterAttribute_StaticIteratorTwoParameters_Index0_Primitive_ReadByValue(
        [Argument(0, Scope.Outer)] int indexedValue) => parameterObserved = indexedValue;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateIdentity))]
    public static void InnerPrefix_IteratorOriginalParameter_Primitive_WriteByReference_Rejected(ref int outerValue) => outerValue = 42;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StringIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateReferenceIdentity))]
    public static void InnerPrefix_IteratorOriginalParameter_ReferenceType_ReadByValue(BindingReference outerValue) =>
        referenceObserved = outerValue;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StringIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateReferenceIdentity))]
    public static void InnerPrefix_IteratorOriginalParameter_ReferenceType_WriteByReference_Rejected(ref BindingReference outerValue) =>
        outerValue = new BindingReference { Value = 42 };

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StructIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateStructIdentity))]
    public static void InnerPrefix_IteratorOriginalParameter_Struct_ReadByValue(BindingStruct outerValue) => structObserved = outerValue;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StructIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateStructIdentity))]
    public static void InnerPrefix_IteratorOriginalParameter_Struct_WriteByReference_Rejected(ref BindingStruct outerValue) =>
        outerValue = new BindingStruct { Value = 42 };

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateIdentity))]
    public static void InnerPostfix_IteratorOriginalParameter_Primitive_ReadByValue(int outerValue) => parameterObserved = outerValue;

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateIdentity))]
    public static void InnerPostfix_IteratorOriginalParameter_Primitive_ReadByReference_Rejected(ref int outerValue) =>
        parameterObserved = outerValue;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateIdentity))]
    public static void InnerPrefix_IteratorDeclaringField_Primitive_ReadByValue([Field("foo", Scope.Outer)] int value) =>
        fieldObserved = value;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateIdentity))]
    public static void InnerPrefix_IteratorDeclaringField_Primitive_WriteByReference(
        [Field("primitiveField", Scope.Outer)] ref int value) => value = 42;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateIdentity))]
    public static void InnerPrefix_IteratorDeclaringField_Primitive_ReadByReference(
        [Field("primitiveField", Scope.Outer)] ref int value) => fieldObserved = value;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateIdentity))]
    public static void InnerPrefix_IteratorDeclaringField_ReferenceType_ReadByValue(
        [Field("referenceField", Scope.Outer)] BindingReference value) => referenceObserved = value;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateIdentity))]
    public static void InnerPrefix_IteratorDeclaringField_ReferenceType_WriteByReference(
        [Field("referenceField", Scope.Outer)] ref BindingReference value) =>
        value = new BindingReference { Value = 42 };

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateIdentity))]
    public static void InnerPrefix_IteratorDeclaringField_ReferenceType_ReadByReference(
        [Field("referenceField", Scope.Outer)] ref BindingReference value) => referenceObserved = value;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateIdentity))]
    public static void InnerPrefix_IteratorDeclaringField_Struct_ReadByValue(
        [Field("structField", Scope.Outer)] BindingStruct value) => structObserved = value;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateIdentity))]
    public static void InnerPrefix_IteratorDeclaringField_Struct_WriteByReference(
        [Field("structField", Scope.Outer)] ref BindingStruct value) =>
        value = new BindingStruct { Value = 42 };

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateIdentity))]
    public static void InnerPrefix_IteratorDeclaringField_Struct_ReadByReference(
        [Field("structField", Scope.Outer)] ref BindingStruct value) => structObserved = value;

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateIdentity))]
    public static void InnerPostfix_IteratorDeclaringInstance_ReferenceType_ReadByValue(
        [Instance(Scope.Outer)] ClassMethodTargets instance) =>
        instanceObserved = instance;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateDeclaringInstanceValue))]
    public static void InnerPrefix_IteratorDeclaringInstance_ReferenceType_ReadByValue(
        [Instance(Scope.Outer)] ClassMethodTargets instance) => instanceObserved = instance;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateDeclaringInstanceValue))]
    public static void InnerPrefix_IteratorDeclaringInstance_ReferenceType_WriteByReference(
        [Instance(Scope.Outer)] ref ClassMethodTargets instance) => instance = replacementInstance!;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.EnumerateDeclaringInstanceValue))]
    public static void InnerPrefix_IteratorDeclaringInstance_Struct_ReadByValue(
        [Instance(Scope.Outer)] StructMethodTargets instance) => structInstanceFieldObserved = instance.foo;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.EnumerateDeclaringInstanceValue))]
    public static void InnerPrefix_IteratorDeclaringInstance_Struct_WriteByReference(
        [Instance(Scope.Outer)] ref StructMethodTargets instance) => instance.foo = 42;
}

[TestFixture]
public sealed partial class IteratorParameterBindingTests : PatchTestBase
{
    [Test]
    public void InnerPrefix_ParameterAttribute_InstanceIteratorOriginalParameter_Name_Primitive_ReadByValue()
    {
        IteratorParameterBindingPatches.parameterObserved = 0;
        var target = new ClassMethodTargets { foo = 7 };
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_ParameterAttribute_InstanceIteratorOriginalParameter_Name_Primitive_ReadByValue));

        int result = target.EnumerateIdentity(42).Single();

        Assert.That(result, Is.EqualTo(42));
        Assert.That(IteratorParameterBindingPatches.parameterObserved, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_ParameterAttribute_StaticIteratorOriginalParameter_Name_Primitive_ReadByValue()
    {
        IteratorParameterBindingPatches.parameterObserved = 0;
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_ParameterAttribute_StaticIteratorOriginalParameter_Name_Primitive_ReadByValue));

        int result = StaticMethodTargets.EnumerateIdentity(42).Single();

        Assert.That(result, Is.EqualTo(42));
        Assert.That(IteratorParameterBindingPatches.parameterObserved, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_StateAttribute_Iterator_OuterScope_Rejected()
    {
        // State always has Scope.Outer, but cannot be shared across iterator/MoveNext invocations.
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_StateAttribute_Iterator_OuterScope_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<NotSupportedException>()
            .With.Message.EqualTo("Not supported for state machine methods"));
    }

    [Test]
    public void InnerPrefix_IteratorOriginalParameter_Primitive_ReadByValue()
    {
        IteratorParameterBindingPatches.parameterObserved = 0;
        var target = new ClassMethodTargets();
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorOriginalParameter_Primitive_ReadByValue));

        int result = target.EnumerateIdentity(42).Single();

        Assert.That(result, Is.EqualTo(42));
        Assert.That(IteratorParameterBindingPatches.parameterObserved, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_ParameterAttribute_InstanceIteratorOriginalParameter_Index0_Primitive_ReadByValue()
    {
        IteratorParameterBindingPatches.parameterObserved = 0;
        var target = new ClassMethodTargets();
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_ParameterAttribute_InstanceIteratorOriginalParameter_Index0_Primitive_ReadByValue));

        int result = target.EnumerateIdentity(42).Single();

        Assert.That(result, Is.EqualTo(42));
        Assert.That(IteratorParameterBindingPatches.parameterObserved, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_ParameterAttribute_StaticIteratorSingleParameter_Index0_Primitive_ReadByValue()
    {
        IteratorParameterBindingPatches.parameterObserved = 0;
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_ParameterAttribute_StaticIteratorSingleParameter_Index0_Primitive_ReadByValue));

        int result = StaticMethodTargets.EnumerateIdentity(42).Single();

        Assert.That(result, Is.EqualTo(42));
        Assert.That(IteratorParameterBindingPatches.parameterObserved, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_ParameterAttribute_StaticIteratorTwoParameters_Index0_Primitive_ReadByValue()
    {
        IteratorParameterBindingPatches.parameterObserved = 0;
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_ParameterAttribute_StaticIteratorTwoParameters_Index0_Primitive_ReadByValue));

        int[] result = StaticMethodTargets.EnumeratePair(17, 42).ToArray();

        Assert.That(result, Is.EqualTo(new[] { 17, 42 }));
        Assert.That(IteratorParameterBindingPatches.parameterObserved, Is.EqualTo(17));
    }

    [Test]
    public void InnerPrefix_IteratorOriginalParameter_Primitive_WriteByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(IteratorParameterBindingPatches),
                nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorOriginalParameter_Primitive_WriteByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void InnerPrefix_IteratorOriginalParameter_ReferenceType_ReadByValue()
    {
        IteratorParameterBindingPatches.referenceObserved = null;
        var value = new BindingReference { Value = 42 };
        var target = new ClassMethodTargets();
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorOriginalParameter_ReferenceType_ReadByValue));

        target.EnumerateReferenceIdentity(value).Single();

        Assert.That(IteratorParameterBindingPatches.referenceObserved, Is.SameAs(value));
    }

    [Test]
    public void InnerPrefix_IteratorOriginalParameter_ReferenceType_WriteByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(IteratorParameterBindingPatches),
                nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorOriginalParameter_ReferenceType_WriteByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void InnerPrefix_IteratorOriginalParameter_Struct_ReadByValue()
    {
        IteratorParameterBindingPatches.structObserved = default;
        var target = new ClassMethodTargets();
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorOriginalParameter_Struct_ReadByValue));

        target.EnumerateStructIdentity(new BindingStruct { Value = 42 }).Single();

        Assert.That(IteratorParameterBindingPatches.structObserved.Value, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_IteratorOriginalParameter_Struct_WriteByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(IteratorParameterBindingPatches),
                nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorOriginalParameter_Struct_WriteByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void InnerPostfix_IteratorOriginalParameter_Primitive_ReadByValue()
    {
        IteratorParameterBindingPatches.parameterObserved = 0;
        var target = new ClassMethodTargets();
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPostfix_IteratorOriginalParameter_Primitive_ReadByValue));

        int result = target.EnumerateIdentity(42).Single();

        Assert.That(result, Is.EqualTo(42));
        Assert.That(IteratorParameterBindingPatches.parameterObserved, Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfix_IteratorOriginalParameter_Primitive_ReadByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(IteratorParameterBindingPatches),
                nameof(IteratorParameterBindingPatches.InnerPostfix_IteratorOriginalParameter_Primitive_ReadByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void InnerPrefix_IteratorDeclaringField_Primitive_ReadByValue()
    {
        IteratorParameterBindingPatches.fieldObserved = 0;
        var target = new ClassMethodTargets { foo = 42 };
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorDeclaringField_Primitive_ReadByValue));

        int result = target.EnumerateIdentity(1).Single();

        Assert.That(result, Is.EqualTo(1));
        Assert.That(IteratorParameterBindingPatches.fieldObserved, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_IteratorDeclaringField_Primitive_WriteByReference()
    {
        var target = new ClassMethodTargets { primitiveField = 1 };
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorDeclaringField_Primitive_WriteByReference));

        target.EnumerateIdentity(1).Single();

        Assert.That(target.primitiveField, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_IteratorDeclaringField_Primitive_ReadByReference()
    {
        IteratorParameterBindingPatches.fieldObserved = 0;
        var target = new ClassMethodTargets { primitiveField = 42 };
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorDeclaringField_Primitive_ReadByReference));

        target.EnumerateIdentity(1).Single();

        Assert.That(IteratorParameterBindingPatches.fieldObserved, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_IteratorDeclaringField_ReferenceType_ReadByValue()
    {
        IteratorParameterBindingPatches.referenceObserved = null;
        var field = new BindingReference { Value = 42 };
        var target = new ClassMethodTargets { referenceField = field };
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorDeclaringField_ReferenceType_ReadByValue));

        target.EnumerateIdentity(1).Single();

        Assert.That(IteratorParameterBindingPatches.referenceObserved, Is.SameAs(field));
    }

    [Test]
    public void InnerPrefix_IteratorDeclaringField_ReferenceType_WriteByReference()
    {
        var target = new ClassMethodTargets { referenceField = new BindingReference { Value = 1 } };
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorDeclaringField_ReferenceType_WriteByReference));

        target.EnumerateIdentity(1).Single();

        Assert.That(target.referenceField.Value, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_IteratorDeclaringField_ReferenceType_ReadByReference()
    {
        IteratorParameterBindingPatches.referenceObserved = null;
        var field = new BindingReference { Value = 42 };
        var target = new ClassMethodTargets { referenceField = field };
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorDeclaringField_ReferenceType_ReadByReference));

        target.EnumerateIdentity(1).Single();

        Assert.That(IteratorParameterBindingPatches.referenceObserved, Is.SameAs(field));
    }

    [Test]
    public void InnerPrefix_IteratorDeclaringField_Struct_ReadByValue()
    {
        IteratorParameterBindingPatches.structObserved = default;
        var target = new ClassMethodTargets { structField = new BindingStruct { Value = 42 } };
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorDeclaringField_Struct_ReadByValue));

        target.EnumerateIdentity(1).Single();

        Assert.That(IteratorParameterBindingPatches.structObserved.Value, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_IteratorDeclaringField_Struct_WriteByReference()
    {
        var target = new ClassMethodTargets { structField = new BindingStruct { Value = 1 } };
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorDeclaringField_Struct_WriteByReference));

        target.EnumerateIdentity(1).Single();

        Assert.That(target.structField.Value, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_IteratorDeclaringField_Struct_ReadByReference()
    {
        IteratorParameterBindingPatches.structObserved = default;
        var target = new ClassMethodTargets { structField = new BindingStruct { Value = 42 } };
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorDeclaringField_Struct_ReadByReference));

        target.EnumerateIdentity(1).Single();

        Assert.That(IteratorParameterBindingPatches.structObserved.Value, Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfix_IteratorDeclaringInstance_ReferenceType_ReadByValue()
    {
        IteratorParameterBindingPatches.instanceObserved = null;
        var target = new ClassMethodTargets();
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPostfix_IteratorDeclaringInstance_ReferenceType_ReadByValue));

        int result = target.EnumerateIdentity(42).Single();

        Assert.That(result, Is.EqualTo(42));
        Assert.That(IteratorParameterBindingPatches.instanceObserved, Is.SameAs(target));
    }

    [Test]
    public void InnerPrefix_IteratorDeclaringInstance_ReferenceType_ReadByValue()
    {
        IteratorParameterBindingPatches.instanceObserved = null;
        var target = new ClassMethodTargets { foo = 42 };
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorDeclaringInstance_ReferenceType_ReadByValue));

        target.EnumerateDeclaringInstanceValue().Single();

        Assert.That(IteratorParameterBindingPatches.instanceObserved, Is.SameAs(target));
    }

    [Test]
    public void InnerPrefix_IteratorDeclaringInstance_ReferenceType_WriteByReference()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(IteratorParameterBindingPatches),
                nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorDeclaringInstance_ReferenceType_WriteByReference)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
        Assert.That(
            exception.InnerException!.Message,
            Is.EqualTo("instance: Accessing 'this' by reference is not supported for iterator state machine methods"));
    }

    [Test]
    public void InnerPrefix_IteratorDeclaringInstance_Struct_ReadByValue()
    {
        IteratorParameterBindingPatches.structInstanceFieldObserved = 0;
        var target = new StructMethodTargets { foo = 42 };
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorDeclaringInstance_Struct_ReadByValue));

        target.EnumerateDeclaringInstanceValue().Single();

        Assert.That(IteratorParameterBindingPatches.structInstanceFieldObserved, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_IteratorDeclaringInstance_Struct_WriteByReference()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(IteratorParameterBindingPatches),
                nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorDeclaringInstance_Struct_WriteByReference)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
        Assert.That(
            exception.InnerException!.Message,
            Is.EqualTo("instance: Accessing 'this' by reference is not supported for iterator state machine methods"));
    }
}

public static partial class IteratorParameterBindingPatches
{
    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateIdentity))]
    public static void InnerPrefix_IteratorOriginalParameter_Primitive_ReadByReference_Rejected(ref int outerValue) =>
        parameterObserved = outerValue;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StringIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateReferenceIdentity))]
    public static void InnerPrefix_IteratorOriginalParameter_ReferenceType_ReadByReference_Rejected(
        ref BindingReference outerValue) => referenceObserved = outerValue;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StructIdentity))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateStructIdentity))]
    public static void InnerPrefix_IteratorOriginalParameter_Struct_ReadByReference_Rejected(ref BindingStruct outerValue) =>
        structObserved = outerValue;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.EnumerateDeclaringInstanceValue))]
    public static void InnerPrefix_IteratorDeclaringInstance_ReferenceType_ReadByReference(
        [Instance(Scope.Outer)] ref ClassMethodTargets instance) => instanceObserved = instance;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.EnumerateDeclaringInstanceValue))]
    public static void InnerPrefix_IteratorDeclaringInstance_Struct_ReadByReference(
        [Instance(Scope.Outer)] ref StructMethodTargets instance) => structInstanceFieldObserved = instance.foo;
}

[TestFixture]
public sealed partial class IteratorParameterBindingTests
{
    [Test]
    public void InnerPrefix_IteratorOriginalParameter_Primitive_ReadByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(IteratorParameterBindingPatches),
                nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorOriginalParameter_Primitive_ReadByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void InnerPrefix_IteratorOriginalParameter_ReferenceType_ReadByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(IteratorParameterBindingPatches),
                nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorOriginalParameter_ReferenceType_ReadByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void InnerPrefix_IteratorOriginalParameter_Struct_ReadByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(IteratorParameterBindingPatches),
                nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorOriginalParameter_Struct_ReadByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void InnerPrefix_IteratorDeclaringInstance_ReferenceType_ReadByReference()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(IteratorParameterBindingPatches),
                nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorDeclaringInstance_ReferenceType_ReadByReference)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
        Assert.That(
            exception.InnerException!.Message,
            Is.EqualTo("instance: Accessing 'this' by reference is not supported for iterator state machine methods"));
    }

    [Test]
    public void InnerPrefix_IteratorDeclaringInstance_Struct_ReadByReference()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(IteratorParameterBindingPatches),
                nameof(IteratorParameterBindingPatches.InnerPrefix_IteratorDeclaringInstance_Struct_ReadByReference)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
        Assert.That(
            exception.InnerException!.Message,
            Is.EqualTo("instance: Accessing 'this' by reference is not supported for iterator state machine methods"));
    }
}

public static partial class IteratorParameterBindingPatches
{
    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(LocalFunctionTargets), "PrimitiveLocalIterator.LocalIterator")]
    public static void InnerPrefix_LocalIteratorEnclosingParameter_Primitive_ReadByValue(int enclosingValue) =>
        parameterObserved = enclosingValue;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(LocalFunctionTargets), "PrimitiveLocalIterator.LocalIterator")]
    public static void InnerPrefix_LocalIteratorEnclosingParameter_Primitive_ReadByReference_Rejected(ref int enclosingValue) =>
        parameterObserved = enclosingValue;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(LocalFunctionTargets), "PrimitiveLocalIterator.LocalIterator")]
    public static void InnerPrefix_LocalIteratorEnclosingParameter_Primitive_WriteByReference_Rejected(ref int enclosingValue) =>
        enclosingValue = 42;

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(LocalFunctionTargets), "PrimitiveLocalIterator.LocalIterator")]
    public static void InnerPostfix_LocalIteratorEnclosingParameter_Primitive_ReadByReference_Rejected(
        ref int enclosingValue) => parameterObserved = enclosingValue;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StringIdentity))]
    [Target(typeof(LocalFunctionTargets), "ReferenceTypeLocalIterator.LocalIterator")]
    public static void InnerPrefix_LocalIteratorEnclosingParameter_ReferenceType_ReadByValue(
        BindingReference enclosingValue) => referenceObserved = enclosingValue;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StringIdentity))]
    [Target(typeof(LocalFunctionTargets), "ReferenceTypeLocalIterator.LocalIterator")]
    public static void InnerPrefix_LocalIteratorEnclosingParameter_ReferenceType_ReadByReference_Rejected(
        ref BindingReference enclosingValue) => referenceObserved = enclosingValue;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StringIdentity))]
    [Target(typeof(LocalFunctionTargets), "ReferenceTypeLocalIterator.LocalIterator")]
    public static void InnerPrefix_LocalIteratorEnclosingParameter_ReferenceType_WriteByReference_Rejected(
        ref BindingReference enclosingValue) => enclosingValue = new BindingReference { Value = 42 };

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StructIdentity))]
    [Target(typeof(LocalFunctionTargets), "StructLocalIterator.LocalIterator")]
    public static void InnerPrefix_LocalIteratorEnclosingParameter_Struct_ReadByValue(BindingStruct enclosingValue) =>
        structObserved = enclosingValue;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StructIdentity))]
    [Target(typeof(LocalFunctionTargets), "StructLocalIterator.LocalIterator")]
    public static void InnerPrefix_LocalIteratorEnclosingParameter_Struct_ReadByReference_Rejected(ref BindingStruct enclosingValue) =>
        structObserved = enclosingValue;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StructIdentity))]
    [Target(typeof(LocalFunctionTargets), "StructLocalIterator.LocalIterator")]
    public static void InnerPrefix_LocalIteratorEnclosingParameter_Struct_WriteByReference_Rejected(ref BindingStruct enclosingValue) =>
        enclosingValue = new BindingStruct { Value = 42 };
}

[TestFixture]
public sealed partial class IteratorParameterBindingTests
{
    [Test]
    public void InnerPrefix_LocalIteratorEnclosingParameter_Primitive_ReadByValue()
    {
        IteratorParameterBindingPatches.parameterObserved = 0;
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_LocalIteratorEnclosingParameter_Primitive_ReadByValue));

        int result = LocalFunctionTargets.PrimitiveLocalIterator(42).Single();

        Assert.That(result, Is.EqualTo(42));
        Assert.That(IteratorParameterBindingPatches.parameterObserved, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_LocalIteratorEnclosingParameter_Primitive_ReadByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(IteratorParameterBindingPatches),
                nameof(IteratorParameterBindingPatches.InnerPrefix_LocalIteratorEnclosingParameter_Primitive_ReadByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void InnerPrefix_LocalIteratorEnclosingParameter_Primitive_WriteByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(IteratorParameterBindingPatches),
                nameof(IteratorParameterBindingPatches.InnerPrefix_LocalIteratorEnclosingParameter_Primitive_WriteByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void InnerPostfix_LocalIteratorEnclosingParameter_Primitive_ReadByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(IteratorParameterBindingPatches),
                nameof(IteratorParameterBindingPatches.InnerPostfix_LocalIteratorEnclosingParameter_Primitive_ReadByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void InnerPrefix_LocalIteratorEnclosingParameter_ReferenceType_ReadByValue()
    {
        IteratorParameterBindingPatches.referenceObserved = null;
        var value = new BindingReference { Value = 42 };
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_LocalIteratorEnclosingParameter_ReferenceType_ReadByValue));

        BindingReference result = LocalFunctionTargets.ReferenceTypeLocalIterator(value).Single();

        Assert.That(result, Is.SameAs(value));
        Assert.That(IteratorParameterBindingPatches.referenceObserved, Is.SameAs(value));
    }

    [Test]
    public void InnerPrefix_LocalIteratorEnclosingParameter_ReferenceType_ReadByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(IteratorParameterBindingPatches),
                nameof(IteratorParameterBindingPatches
                    .InnerPrefix_LocalIteratorEnclosingParameter_ReferenceType_ReadByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void InnerPrefix_LocalIteratorEnclosingParameter_ReferenceType_WriteByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(IteratorParameterBindingPatches),
                nameof(IteratorParameterBindingPatches
                    .InnerPrefix_LocalIteratorEnclosingParameter_ReferenceType_WriteByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void InnerPrefix_LocalIteratorEnclosingParameter_Struct_ReadByValue()
    {
        IteratorParameterBindingPatches.structObserved = default;
        ApplyPatch(
            typeof(IteratorParameterBindingPatches),
            nameof(IteratorParameterBindingPatches.InnerPrefix_LocalIteratorEnclosingParameter_Struct_ReadByValue));

        BindingStruct result = LocalFunctionTargets.StructLocalIterator(new BindingStruct { Value = 42 }).Single();

        Assert.That(result.Value, Is.EqualTo(42));
        Assert.That(IteratorParameterBindingPatches.structObserved.Value, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_LocalIteratorEnclosingParameter_Struct_ReadByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(IteratorParameterBindingPatches),
                nameof(IteratorParameterBindingPatches.InnerPrefix_LocalIteratorEnclosingParameter_Struct_ReadByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void InnerPrefix_LocalIteratorEnclosingParameter_Struct_WriteByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(IteratorParameterBindingPatches),
                nameof(IteratorParameterBindingPatches.InnerPrefix_LocalIteratorEnclosingParameter_Struct_WriteByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }
}
