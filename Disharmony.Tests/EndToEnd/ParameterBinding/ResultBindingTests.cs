namespace Disharmony.Tests.EndToEnd.ParameterBinding;

public static partial class ResultBindingPatches
{
    public static int valueObserved;
    public static string? referenceObserved;
    public static int innerObserved;
    public static BindingStruct structObserved;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntResult))]
    public static void Prefix_Result_Primitive_ReadByValue(int __result) => valueObserved = __result;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntResult))]
    public static void Postfix_Result_Primitive_ReadByValue(int __result) => valueObserved = __result;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StringResult))]
    public static void Prefix_Result_ReferenceType_ReadByValue(string? __result) => referenceObserved = __result;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StructResult))]
    public static void Prefix_Result_Struct_ReadByValue(BindingStruct __result) => structObserved = __result;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StringResult))]
    public static void Postfix_Result_ReferenceType_ReadByValue(string __result) => referenceObserved = __result;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StructResult))]
    public static void Postfix_Result_Struct_ReadByValue(BindingStruct __result) => structObserved = __result;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowingIntResult))]
    public static bool Prefix_Result_Primitive_WriteByReference(ref int __result)
    {
        __result = 42;
        return false;
    }

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntResult))]
    public static void Postfix_Result_Primitive_WriteByReference(ref int __result) => __result = 42;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowingStringResult))]
    public static bool Prefix_Result_ReferenceType_WriteByReference(ref string? __result)
    {
        __result = "patched";
        return false;
    }

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowingStructResult))]
    public static bool Prefix_Result_Struct_WriteByReference(ref BindingStruct __result)
    {
        __result = new BindingStruct { Value = 42 };
        return false;
    }

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StringResult))]
    public static void Postfix_Result_ReferenceType_WriteByReference(ref string __result) => __result = "patched";

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StructResult))]
    public static void Postfix_Result_Struct_WriteByReference(ref BindingStruct __result) =>
        __result = new BindingStruct { Value = 42 };

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.IntResult))]
    public static void InnerPrefix_Result_Primitive_ReadByValue(int __result) => innerObserved = __result;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StringResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.StringResult))]
    public static void InnerPrefix_Result_ReferenceType_ReadByValue(string? __result) => referenceObserved = __result;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StructResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.StructResult))]
    public static void InnerPrefix_Result_Struct_ReadByValue(BindingStruct __result) => structObserved = __result;

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.IntResult))]
    public static void InnerPostfix_Result_Primitive_ReadByValue(int __result) => innerObserved = __result;

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StringResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.StringResult))]
    public static void InnerPostfix_Result_ReferenceType_ReadByValue(string __result) => referenceObserved = __result;

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StructResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.StructResult))]
    public static void InnerPostfix_Result_Struct_ReadByValue(BindingStruct __result) => structObserved = __result;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.IntResult))]
    public static bool InnerPrefix_Result_Primitive_WriteByReference(ref int __result)
    {
        __result = 42;
        return false;
    }

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StringResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.StringResult))]
    public static bool InnerPrefix_Result_ReferenceType_WriteByReference(ref string? __result)
    {
        __result = "patched";
        return false;
    }

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StructResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.StructResult))]
    public static bool InnerPrefix_Result_Struct_WriteByReference(ref BindingStruct __result)
    {
        __result = new BindingStruct { Value = 42 };
        return false;
    }

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.IntResult))]
    public static void InnerPostfix_Result_Primitive_WriteByReference(ref int __result) => __result = 42;

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StringResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.StringResult))]
    public static void InnerPostfix_Result_ReferenceType_WriteByReference(ref string __result) => __result = "patched";

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StructResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.StructResult))]
    public static void InnerPostfix_Result_Struct_WriteByReference(ref BindingStruct __result) =>
        __result = new BindingStruct { Value = 42 };

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntResult))]
    public static void Postfix_ReturnValueAttribute_Primitive_WriteByReference([ReturnValue] ref int value) => value = 42;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntResult))]
    public static void Prefix_ReturnValueAttribute_Primitive_ReadByValue([ReturnValue] int value) => valueObserved = value;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StringResult))]
    public static void Prefix_ReturnValueAttribute_ReferenceType_ReadByValue([ReturnValue] string? value) =>
        referenceObserved = value;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowingStringResult))]
    public static bool Prefix_ReturnValueAttribute_ReferenceType_WriteByReference([ReturnValue] ref string? value)
    {
        value = "patched";
        return false;
    }

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StructResult))]
    public static void Prefix_ReturnValueAttribute_Struct_ReadByValue([ReturnValue] BindingStruct value) => structObserved = value;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowingStructResult))]
    public static bool Prefix_ReturnValueAttribute_Struct_WriteByReference([ReturnValue] ref BindingStruct value)
    {
        value = new BindingStruct { Value = 42 };
        return false;
    }

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntResult))]
    public static void Postfix_ReturnValueAttribute_Primitive_ReadByValue([ReturnValue] int value) => valueObserved = value;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StringResult))]
    public static void Postfix_ReturnValueAttribute_ReferenceType_ReadByValue([ReturnValue] string value) =>
        referenceObserved = value;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StringResult))]
    public static void Postfix_ReturnValueAttribute_ReferenceType_WriteByReference([ReturnValue] ref string value) =>
        value = "patched";

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StructResult))]
    public static void Postfix_ReturnValueAttribute_Struct_ReadByValue([ReturnValue] BindingStruct value) => structObserved = value;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StructResult))]
    public static void Postfix_ReturnValueAttribute_Struct_WriteByReference([ReturnValue] ref BindingStruct value) =>
        value = new BindingStruct { Value = 42 };
}

public static partial class ResultBindingPatches
{
    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntResult))]
    public static void Prefix_Result_Primitive_AsObjectByWriteableReference_Rejected(ref object __result) { }

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntResult))]
    public static void Prefix_Result_Primitive_AsObjectByReadonlyReference_Rejected(in object __result) { }

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StringResult))]
    public static void Postfix_Result_ReferenceType_AsObjectByWriteableReference_Rejected(ref object __result) { }

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StringResult))]
    public static void Postfix_Result_ReferenceType_AsObjectByReadonlyReference(in object __result) =>
        referenceObserved = (string)__result;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ObjectIdentity))]
    public static void Postfix_Result_ReferenceType_AsStringByOutReference(out string __result) =>
        __result = "patched";

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntResult))]
    public static void Prefix_Result_Primitive_ReadByReference(ref int __result) => valueObserved = __result;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StringResult))]
    public static void Prefix_Result_ReferenceType_ReadByReference(ref string? __result) => referenceObserved = __result;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StructResult))]
    public static void Prefix_Result_Struct_ReadByReference(ref BindingStruct __result) => structObserved = __result;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntResult))]
    public static void Postfix_Result_Primitive_ReadByReference(ref int __result) => valueObserved = __result;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StringResult))]
    public static void Postfix_Result_ReferenceType_ReadByReference(ref string __result) => referenceObserved = __result;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StructResult))]
    public static void Postfix_Result_Struct_ReadByReference(ref BindingStruct __result) => structObserved = __result;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.IntResult))]
    public static void InnerPrefix_Result_Primitive_ReadByReference(ref int __result) => innerObserved = __result;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StringResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.StringResult))]
    public static void InnerPrefix_Result_ReferenceType_ReadByReference(ref string? __result) =>
        referenceObserved = __result;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StructResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.StructResult))]
    public static void InnerPrefix_Result_Struct_ReadByReference(ref BindingStruct __result) => structObserved = __result;

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.IntResult))]
    public static void InnerPostfix_Result_Primitive_ReadByReference(ref int __result) => innerObserved = __result;

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StringResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.StringResult))]
    public static void InnerPostfix_Result_ReferenceType_ReadByReference(ref string __result) =>
        referenceObserved = __result;

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.StructResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.StructResult))]
    public static void InnerPostfix_Result_Struct_ReadByReference(ref BindingStruct __result) => structObserved = __result;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntResult))]
    public static void Postfix_ReturnValueAttribute_Primitive_ReadByReference([ReturnValue] ref int value) =>
        valueObserved = value;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StringResult))]
    public static void Prefix_ReturnValueAttribute_ReferenceType_ReadByReference(
        [ReturnValue] ref string? value) => referenceObserved = value;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StructResult))]
    public static void Prefix_ReturnValueAttribute_Struct_ReadByReference(
        [ReturnValue] ref BindingStruct value) => structObserved = value;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StringResult))]
    public static void Postfix_ReturnValueAttribute_ReferenceType_ReadByReference(
        [ReturnValue] ref string value) => referenceObserved = value;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StructResult))]
    public static void Postfix_ReturnValueAttribute_Struct_ReadByReference(
        [ReturnValue] ref BindingStruct value) => structObserved = value;
}

[TestFixture]
public sealed partial class ResultBindingTests
{
    [Test]
    public void Prefix_Result_Primitive_AsObjectByWriteableReference_Rejected()
    {
        Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(ResultBindingPatches),
            nameof(ResultBindingPatches.Prefix_Result_Primitive_AsObjectByWriteableReference_Rejected)));
    }

    [Test]
    public void Prefix_Result_Primitive_AsObjectByReadonlyReference_Rejected()
    {
        Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(ResultBindingPatches),
            nameof(ResultBindingPatches.Prefix_Result_Primitive_AsObjectByReadonlyReference_Rejected)));
    }

    [Test]
    public void Postfix_Result_ReferenceType_AsObjectByWriteableReference_Rejected()
    {
        Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(ResultBindingPatches),
            nameof(ResultBindingPatches.Postfix_Result_ReferenceType_AsObjectByWriteableReference_Rejected)));
    }

    [Test]
    public void Postfix_Result_ReferenceType_AsObjectByReadonlyReference()
    {
        ResultBindingPatches.referenceObserved = null;
        ApplyPatch(
            typeof(ResultBindingPatches),
            nameof(ResultBindingPatches.Postfix_Result_ReferenceType_AsObjectByReadonlyReference));

        string result = StaticMethodTargets.StringResult();

        Assert.That(result, Is.EqualTo("original"));
        Assert.That(ResultBindingPatches.referenceObserved, Is.EqualTo("original"));
    }

    [Test]
    public void Postfix_Result_ReferenceType_AsStringByOutReference()
    {
        ApplyPatch(
            typeof(ResultBindingPatches),
            nameof(ResultBindingPatches.Postfix_Result_ReferenceType_AsStringByOutReference));

        object result = StaticMethodTargets.ObjectIdentity(new object());

        Assert.That(result, Is.EqualTo("patched"));
    }

    [Test]
    public void Prefix_Result_Primitive_ReadByReference()
    {
        ResultBindingPatches.valueObserved = -1;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Prefix_Result_Primitive_ReadByReference));
        StaticMethodTargets.IntResult();
        Assert.That(ResultBindingPatches.valueObserved, Is.Zero);
    }

    [Test]
    public void Prefix_Result_ReferenceType_ReadByReference()
    {
        ResultBindingPatches.referenceObserved = "sentinel";
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Prefix_Result_ReferenceType_ReadByReference));
        StaticMethodTargets.StringResult();
        Assert.That(ResultBindingPatches.referenceObserved, Is.Null);
    }

    [Test]
    public void Prefix_Result_Struct_ReadByReference()
    {
        ResultBindingPatches.structObserved = new BindingStruct { Value = -1 };
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Prefix_Result_Struct_ReadByReference));
        StaticMethodTargets.StructResult();
        Assert.That(ResultBindingPatches.structObserved.Value, Is.Zero);
    }

    [Test]
    public void Postfix_Result_Primitive_ReadByReference()
    {
        ResultBindingPatches.valueObserved = 0;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Postfix_Result_Primitive_ReadByReference));
        StaticMethodTargets.IntResult();
        Assert.That(ResultBindingPatches.valueObserved, Is.EqualTo(1));
    }

    [Test]
    public void Postfix_Result_ReferenceType_ReadByReference()
    {
        ResultBindingPatches.referenceObserved = null;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Postfix_Result_ReferenceType_ReadByReference));
        StaticMethodTargets.StringResult();
        Assert.That(ResultBindingPatches.referenceObserved, Is.EqualTo("original"));
    }

    [Test]
    public void Postfix_Result_Struct_ReadByReference()
    {
        ResultBindingPatches.structObserved = default;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Postfix_Result_Struct_ReadByReference));
        StaticMethodTargets.StructResult();
        Assert.That(ResultBindingPatches.structObserved.Value, Is.EqualTo(1));
    }

    [Test]
    public void InnerPrefix_Result_Primitive_ReadByReference()
    {
        ResultBindingPatches.innerObserved = -1;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.InnerPrefix_Result_Primitive_ReadByReference));
        OuterStaticMethodTargets.IntResult();
        Assert.That(ResultBindingPatches.innerObserved, Is.Zero);
    }

    [Test]
    public void InnerPrefix_Result_ReferenceType_ReadByReference()
    {
        ResultBindingPatches.referenceObserved = "sentinel";
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.InnerPrefix_Result_ReferenceType_ReadByReference));
        OuterStaticMethodTargets.StringResult();
        Assert.That(ResultBindingPatches.referenceObserved, Is.Null);
    }

    [Test]
    public void InnerPrefix_Result_Struct_ReadByReference()
    {
        ResultBindingPatches.structObserved = new BindingStruct { Value = -1 };
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.InnerPrefix_Result_Struct_ReadByReference));
        OuterStaticMethodTargets.StructResult();
        Assert.That(ResultBindingPatches.structObserved.Value, Is.Zero);
    }

    [Test]
    public void InnerPostfix_Result_Primitive_ReadByReference()
    {
        ResultBindingPatches.innerObserved = 0;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.InnerPostfix_Result_Primitive_ReadByReference));
        OuterStaticMethodTargets.IntResult();
        Assert.That(ResultBindingPatches.innerObserved, Is.EqualTo(1));
    }

    [Test]
    public void InnerPostfix_Result_ReferenceType_ReadByReference()
    {
        ResultBindingPatches.referenceObserved = null;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.InnerPostfix_Result_ReferenceType_ReadByReference));
        OuterStaticMethodTargets.StringResult();
        Assert.That(ResultBindingPatches.referenceObserved, Is.EqualTo("original"));
    }

    [Test]
    public void InnerPostfix_Result_Struct_ReadByReference()
    {
        ResultBindingPatches.structObserved = default;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.InnerPostfix_Result_Struct_ReadByReference));
        OuterStaticMethodTargets.StructResult();
        Assert.That(ResultBindingPatches.structObserved.Value, Is.EqualTo(1));
    }

    [Test]
    public void Postfix_ReturnValueAttribute_Primitive_ReadByReference()
    {
        ResultBindingPatches.valueObserved = 0;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Postfix_ReturnValueAttribute_Primitive_ReadByReference));
        StaticMethodTargets.IntResult();
        Assert.That(ResultBindingPatches.valueObserved, Is.EqualTo(1));
    }

    [Test]
    public void Prefix_ReturnValueAttribute_ReferenceType_ReadByReference()
    {
        ResultBindingPatches.referenceObserved = "sentinel";
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Prefix_ReturnValueAttribute_ReferenceType_ReadByReference));
        StaticMethodTargets.StringResult();
        Assert.That(ResultBindingPatches.referenceObserved, Is.Null);
    }

    [Test]
    public void Prefix_ReturnValueAttribute_Struct_ReadByReference()
    {
        ResultBindingPatches.structObserved = new BindingStruct { Value = -1 };
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Prefix_ReturnValueAttribute_Struct_ReadByReference));
        StaticMethodTargets.StructResult();
        Assert.That(ResultBindingPatches.structObserved.Value, Is.Zero);
    }

    [Test]
    public void Postfix_ReturnValueAttribute_ReferenceType_ReadByReference()
    {
        ResultBindingPatches.referenceObserved = null;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Postfix_ReturnValueAttribute_ReferenceType_ReadByReference));
        StaticMethodTargets.StringResult();
        Assert.That(ResultBindingPatches.referenceObserved, Is.EqualTo("original"));
    }

    [Test]
    public void Postfix_ReturnValueAttribute_Struct_ReadByReference()
    {
        ResultBindingPatches.structObserved = default;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Postfix_ReturnValueAttribute_Struct_ReadByReference));
        StaticMethodTargets.StructResult();
        Assert.That(ResultBindingPatches.structObserved.Value, Is.EqualTo(1));
    }
}

[TestFixture]
public sealed partial class ResultBindingTests : PatchTestBase
{
    [Test]
    public void Prefix_Result_Primitive_ReadByValue()
    {
        ResultBindingPatches.valueObserved = -1;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Prefix_Result_Primitive_ReadByValue));
        StaticMethodTargets.IntResult();
        Assert.That(ResultBindingPatches.valueObserved, Is.Zero);
    }

    [Test]
    public void Postfix_Result_Primitive_ReadByValue()
    {
        ResultBindingPatches.valueObserved = 0;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Postfix_Result_Primitive_ReadByValue));
        StaticMethodTargets.IntResult();
        Assert.That(ResultBindingPatches.valueObserved, Is.EqualTo(1));
    }

    [Test]
    public void Prefix_Result_ReferenceType_ReadByValue()
    {
        ResultBindingPatches.referenceObserved = "sentinel";
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Prefix_Result_ReferenceType_ReadByValue));
        StaticMethodTargets.StringResult();
        Assert.That(ResultBindingPatches.referenceObserved, Is.Null);
    }

    [Test]
    public void Prefix_Result_Struct_ReadByValue()
    {
        ResultBindingPatches.structObserved = new BindingStruct { Value = -1 };
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Prefix_Result_Struct_ReadByValue));

        StaticMethodTargets.StructResult();

        Assert.That(ResultBindingPatches.structObserved.Value, Is.Zero);
    }

    [Test]
    public void Postfix_Result_ReferenceType_ReadByValue()
    {
        ResultBindingPatches.referenceObserved = null;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Postfix_Result_ReferenceType_ReadByValue));
        StaticMethodTargets.StringResult();
        Assert.That(ResultBindingPatches.referenceObserved, Is.EqualTo("original"));
    }

    [Test]
    public void Postfix_Result_Struct_ReadByValue()
    {
        ResultBindingPatches.structObserved = default;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Postfix_Result_Struct_ReadByValue));

        StaticMethodTargets.StructResult();

        Assert.That(ResultBindingPatches.structObserved.Value, Is.EqualTo(1));
    }

    [Test]
    public void Prefix_Result_Primitive_WriteByReference()
    {
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Prefix_Result_Primitive_WriteByReference));
        Assert.That(StaticMethodTargets.ThrowingIntResult(), Is.EqualTo(42));
    }

    [Test]
    public void Postfix_Result_Primitive_WriteByReference()
    {
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Postfix_Result_Primitive_WriteByReference));
        Assert.That(StaticMethodTargets.IntResult(), Is.EqualTo(42));
    }

    [Test]
    public void Prefix_Result_ReferenceType_WriteByReference()
    {
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Prefix_Result_ReferenceType_WriteByReference));
        Assert.That(StaticMethodTargets.ThrowingStringResult(), Is.EqualTo("patched"));
    }

    [Test]
    public void Prefix_Result_Struct_WriteByReference()
    {
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Prefix_Result_Struct_WriteByReference));

        BindingStruct result = StaticMethodTargets.ThrowingStructResult();

        Assert.That(result.Value, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_Result_ReferenceType_WriteByReference()
    {
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Postfix_Result_ReferenceType_WriteByReference));
        Assert.That(StaticMethodTargets.StringResult(), Is.EqualTo("patched"));
    }

    [Test]
    public void Postfix_Result_Struct_WriteByReference()
    {
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Postfix_Result_Struct_WriteByReference));

        BindingStruct result = StaticMethodTargets.StructResult();

        Assert.That(result.Value, Is.EqualTo(42));
    }
}

[TestFixture]
public sealed partial class ResultBindingTests
{
    [Test]
    public void InnerPrefix_Result_Primitive_ReadByValue()
    {
        ResultBindingPatches.innerObserved = -1;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.InnerPrefix_Result_Primitive_ReadByValue));
        OuterStaticMethodTargets.IntResult();
        Assert.That(ResultBindingPatches.innerObserved, Is.Zero);
    }

    [Test]
    public void InnerPrefix_Result_ReferenceType_ReadByValue()
    {
        ResultBindingPatches.referenceObserved = "sentinel";
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.InnerPrefix_Result_ReferenceType_ReadByValue));

        OuterStaticMethodTargets.StringResult();

        Assert.That(ResultBindingPatches.referenceObserved, Is.Null);
    }

    [Test]
    public void InnerPrefix_Result_Struct_ReadByValue()
    {
        ResultBindingPatches.structObserved = new BindingStruct { Value = -1 };
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.InnerPrefix_Result_Struct_ReadByValue));

        OuterStaticMethodTargets.StructResult();

        Assert.That(ResultBindingPatches.structObserved.Value, Is.Zero);
    }

    [Test]
    public void InnerPostfix_Result_Primitive_ReadByValue()
    {
        ResultBindingPatches.innerObserved = 0;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.InnerPostfix_Result_Primitive_ReadByValue));
        OuterStaticMethodTargets.IntResult();
        Assert.That(ResultBindingPatches.innerObserved, Is.EqualTo(1));
    }

    [Test]
    public void InnerPostfix_Result_ReferenceType_ReadByValue()
    {
        ResultBindingPatches.referenceObserved = null;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.InnerPostfix_Result_ReferenceType_ReadByValue));

        OuterStaticMethodTargets.StringResult();

        Assert.That(ResultBindingPatches.referenceObserved, Is.EqualTo("original"));
    }

    [Test]
    public void InnerPostfix_Result_Struct_ReadByValue()
    {
        ResultBindingPatches.structObserved = default;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.InnerPostfix_Result_Struct_ReadByValue));

        OuterStaticMethodTargets.StructResult();

        Assert.That(ResultBindingPatches.structObserved.Value, Is.EqualTo(1));
    }

    [Test]
    public void InnerPrefix_Result_Primitive_WriteByReference()
    {
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.InnerPrefix_Result_Primitive_WriteByReference));
        Assert.That(OuterStaticMethodTargets.IntResult(), Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_Result_ReferenceType_WriteByReference()
    {
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.InnerPrefix_Result_ReferenceType_WriteByReference));

        Assert.That(OuterStaticMethodTargets.StringResult(), Is.EqualTo("patched"));
    }

    [Test]
    public void InnerPrefix_Result_Struct_WriteByReference()
    {
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.InnerPrefix_Result_Struct_WriteByReference));

        BindingStruct result = OuterStaticMethodTargets.StructResult();

        Assert.That(result.Value, Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfix_Result_Primitive_WriteByReference()
    {
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.InnerPostfix_Result_Primitive_WriteByReference));
        Assert.That(OuterStaticMethodTargets.IntResult(), Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfix_Result_ReferenceType_WriteByReference()
    {
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.InnerPostfix_Result_ReferenceType_WriteByReference));

        Assert.That(OuterStaticMethodTargets.StringResult(), Is.EqualTo("patched"));
    }

    [Test]
    public void InnerPostfix_Result_Struct_WriteByReference()
    {
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.InnerPostfix_Result_Struct_WriteByReference));

        BindingStruct result = OuterStaticMethodTargets.StructResult();

        Assert.That(result.Value, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_ReturnValueAttribute_Primitive_WriteByReference()
    {
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Postfix_ReturnValueAttribute_Primitive_WriteByReference));

        Assert.That(StaticMethodTargets.IntResult(), Is.EqualTo(42));
    }

    [Test]
    public void Prefix_ReturnValueAttribute_Primitive_ReadByValue()
    {
        ResultBindingPatches.valueObserved = -1;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Prefix_ReturnValueAttribute_Primitive_ReadByValue));

        StaticMethodTargets.IntResult();

        Assert.That(ResultBindingPatches.valueObserved, Is.Zero);
    }

    [Test]
    public void Prefix_ReturnValueAttribute_ReferenceType_ReadByValue()
    {
        ResultBindingPatches.referenceObserved = "sentinel";
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Prefix_ReturnValueAttribute_ReferenceType_ReadByValue));

        StaticMethodTargets.StringResult();

        Assert.That(ResultBindingPatches.referenceObserved, Is.Null);
    }

    [Test]
    public void Prefix_ReturnValueAttribute_ReferenceType_WriteByReference()
    {
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Prefix_ReturnValueAttribute_ReferenceType_WriteByReference));

        Assert.That(StaticMethodTargets.ThrowingStringResult(), Is.EqualTo("patched"));
    }

    [Test]
    public void Prefix_ReturnValueAttribute_Struct_ReadByValue()
    {
        ResultBindingPatches.structObserved = new BindingStruct { Value = -1 };
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Prefix_ReturnValueAttribute_Struct_ReadByValue));

        StaticMethodTargets.StructResult();

        Assert.That(ResultBindingPatches.structObserved.Value, Is.Zero);
    }

    [Test]
    public void Prefix_ReturnValueAttribute_Struct_WriteByReference()
    {
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Prefix_ReturnValueAttribute_Struct_WriteByReference));

        BindingStruct result = StaticMethodTargets.ThrowingStructResult();

        Assert.That(result.Value, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_ReturnValueAttribute_Primitive_ReadByValue()
    {
        ResultBindingPatches.valueObserved = 0;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Postfix_ReturnValueAttribute_Primitive_ReadByValue));

        StaticMethodTargets.IntResult();

        Assert.That(ResultBindingPatches.valueObserved, Is.EqualTo(1));
    }

    [Test]
    public void Postfix_ReturnValueAttribute_ReferenceType_ReadByValue()
    {
        ResultBindingPatches.referenceObserved = null;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Postfix_ReturnValueAttribute_ReferenceType_ReadByValue));

        StaticMethodTargets.StringResult();

        Assert.That(ResultBindingPatches.referenceObserved, Is.EqualTo("original"));
    }

    [Test]
    public void Postfix_ReturnValueAttribute_ReferenceType_WriteByReference()
    {
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Postfix_ReturnValueAttribute_ReferenceType_WriteByReference));

        Assert.That(StaticMethodTargets.StringResult(), Is.EqualTo("patched"));
    }

    [Test]
    public void Postfix_ReturnValueAttribute_Struct_ReadByValue()
    {
        ResultBindingPatches.structObserved = default;
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Postfix_ReturnValueAttribute_Struct_ReadByValue));

        StaticMethodTargets.StructResult();

        Assert.That(ResultBindingPatches.structObserved.Value, Is.EqualTo(1));
    }

    [Test]
    public void Postfix_ReturnValueAttribute_Struct_WriteByReference()
    {
        ApplyPatch(typeof(ResultBindingPatches), nameof(ResultBindingPatches.Postfix_ReturnValueAttribute_Struct_WriteByReference));

        BindingStruct result = StaticMethodTargets.StructResult();

        Assert.That(result.Value, Is.EqualTo(42));
    }
}
