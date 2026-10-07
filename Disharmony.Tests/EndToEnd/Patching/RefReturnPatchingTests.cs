namespace Disharmony.Tests.EndToEnd.Patching;

public static class RefReturnPatchingPatches
{
    public static BindingReference? ReferenceObserved;
    public static BindingReference? ReplacementReference;
    public static BindingStruct StructObserved;

    [Postfix]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.PrimitiveReference))]
    public static void Postfix_Result_Primitive_WriteByReference(ref int __result) =>
        __result = 42;

    [Postfix]
    [Inner(typeof(RefReturnTargets), nameof(RefReturnTargets.PrimitiveReference))]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.CallPrimitiveReference))]
    public static void InnerPostfix_Result_Primitive_ReadByValue(int __result) =>
        Observed = __result;

    [Postfix]
    [Inner(typeof(RefReturnTargets), nameof(RefReturnTargets.PrimitiveReference))]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.CallPrimitiveReference))]
    public static void InnerPostfix_Result_Primitive_ReadByReference(ref int __result) =>
        Observed = __result;

    [Postfix]
    [Inner(typeof(RefReturnTargets), nameof(RefReturnTargets.PrimitiveReference))]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.CallPrimitiveReference))]
    public static void InnerPostfix_Result_Primitive_WriteByReference(ref int __result) =>
        __result = 42;

    [Prefix]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.PrimitiveReference))]
    public static bool Prefix_Result_Primitive_SkipWithSuppliedReference(ref int __result)
    {
        __result = 42;
        return false;
    }

    [Prefix]
    [Inner(typeof(RefReturnTargets), nameof(RefReturnTargets.PrimitiveReference))]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.CallPrimitiveReference))]
    public static bool InnerPrefix_Result_Primitive_SkipWithSuppliedReference(ref int __result)
    {
        __result = 42;
        return false;
    }

    [Postfix]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.ObjectReference))]
    public static void Postfix_Result_ReferenceType_ReadByValue(BindingReference __result) =>
        ReferenceObserved = __result;

    [Postfix]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.ObjectReference))]
    public static void Postfix_Result_ReferenceType_ReadByReference(ref BindingReference __result) =>
        ReferenceObserved = __result;

    [Postfix]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.ObjectReference))]
    public static void Postfix_Result_ReferenceType_WriteByReference(ref BindingReference __result) =>
        __result = ReplacementReference!;

    [Postfix]
    [Inner(typeof(RefReturnTargets), nameof(RefReturnTargets.ObjectReference))]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.CallObjectReference))]
    public static void InnerPostfix_Result_ReferenceType_ReadByValue(BindingReference __result) =>
        ReferenceObserved = __result;

    [Postfix]
    [Inner(typeof(RefReturnTargets), nameof(RefReturnTargets.ObjectReference))]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.CallObjectReference))]
    public static void InnerPostfix_Result_ReferenceType_ReadByReference(ref BindingReference __result) =>
        ReferenceObserved = __result;

    [Postfix]
    [Inner(typeof(RefReturnTargets), nameof(RefReturnTargets.ObjectReference))]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.CallObjectReference))]
    public static void InnerPostfix_Result_ReferenceType_WriteByReference(ref BindingReference __result) =>
        __result = ReplacementReference!;

    [Prefix]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.ObjectReference))]
    public static bool Prefix_Result_ReferenceType_SkipWithSuppliedReference(ref BindingReference __result)
    {
        __result = ReplacementReference!;
        return false;
    }

    [Prefix]
    [Inner(typeof(RefReturnTargets), nameof(RefReturnTargets.ObjectReference))]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.CallObjectReference))]
    public static bool InnerPrefix_Result_ReferenceType_SkipWithSuppliedReference(ref BindingReference __result)
    {
        __result = ReplacementReference!;
        return false;
    }

    [Postfix]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.StructReference))]
    public static void Postfix_Result_Struct_ReadByValue(BindingStruct __result) =>
        StructObserved = __result;

    [Postfix]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.StructReference))]
    public static void Postfix_Result_Struct_ReadByReference(ref BindingStruct __result) =>
        StructObserved = __result;

    [Postfix]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.StructReference))]
    public static void Postfix_Result_Struct_WriteByReference(ref BindingStruct __result) =>
        __result = new BindingStruct { Value = 42 };

    [Postfix]
    [Inner(typeof(RefReturnTargets), nameof(RefReturnTargets.StructReference))]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.CallStructReference))]
    public static void InnerPostfix_Result_Struct_ReadByValue(BindingStruct __result) =>
        StructObserved = __result;

    [Postfix]
    [Inner(typeof(RefReturnTargets), nameof(RefReturnTargets.StructReference))]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.CallStructReference))]
    public static void InnerPostfix_Result_Struct_ReadByReference(ref BindingStruct __result) =>
        StructObserved = __result;

    [Postfix]
    [Inner(typeof(RefReturnTargets), nameof(RefReturnTargets.StructReference))]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.CallStructReference))]
    public static void InnerPostfix_Result_Struct_WriteByReference(ref BindingStruct __result) =>
        __result = new BindingStruct { Value = 42 };

    [Prefix]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.StructReference))]
    public static bool Prefix_Result_Struct_SkipWithSuppliedReference(ref BindingStruct __result)
    {
        __result = new BindingStruct { Value = 42 };
        return false;
    }

    [Prefix]
    [Inner(typeof(RefReturnTargets), nameof(RefReturnTargets.StructReference))]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.CallStructReference))]
    public static bool InnerPrefix_Result_Struct_SkipWithSuppliedReference(ref BindingStruct __result)
    {
        __result = new BindingStruct { Value = 42 };
        return false;
    }

    public static int ExecutionCount;
    public static int Observed;

    [Prefix]
    [Target(typeof(BoundaryTargets), nameof(BoundaryTargets.RefReturnMethod))]
    public static void Prefix_StaticField_PreservesAliasOnFirstAndSubsequentCalls() => ExecutionCount++;

    [Postfix]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.PrimitiveReference))]
    public static void Postfix_InstancePrimitive_PreservesAlias() => ExecutionCount++;

    [Prefix]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.ObjectReference))]
    public static void Prefix_InstanceReferenceType_PreservesAlias() => ExecutionCount++;

    [Postfix]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.StructReference))]
    public static void Postfix_InstanceStruct_PreservesAlias() => ExecutionCount++;

    [Prefix]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.ArgumentReference))]
    public static void Prefix_RefArgument_PreservesCallerStorage() => ExecutionCount++;

    [Postfix]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.ArrayElement))]
    public static void Postfix_ArrayElement_PreservesSelectedElement() => ExecutionCount++;

    [Postfix]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.ReadonlyReference))]
    public static void Postfix_ReadonlyReference_PreservesLiveAlias() => ExecutionCount++;

    [Prefix]
    [Inner(typeof(RefReturnTargets), nameof(RefReturnTargets.ArrayElement))]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.CallArrayElement))]
    public static void InnerPrefix_ArrayElement_PreservesAlias() => ExecutionCount++;

    [Postfix]
    [Inner(typeof(RefReturnTargets), nameof(RefReturnTargets.ArrayElement))]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.CallArrayElement))]
    public static void InnerPostfix_ArrayElement_PreservesAlias() => ExecutionCount++;

    [Postfix]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.PrimitiveReference))]
    public static void Postfix_Result_Primitive_ReadByValue(int __result) => Observed = __result;

    [Postfix]
    [Target(typeof(RefReturnTargets), nameof(RefReturnTargets.PrimitiveReference))]
    public static void Postfix_Result_Primitive_ReadByReference(ref int __result) => Observed = __result;

}

[TestFixture]
#if NETFRAMEWORK
[Timeout(10000)]
#endif
public sealed class RefReturnPatchingTests : PatchTestBase
{
    [Test]
    public void Postfix_Result_Primitive_WriteByReference()
    {
        RefReturnPatchingPatches.Observed = default;
        var original = 11;
        var target = new RefReturnTargets { Primitive = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.Postfix_Result_Primitive_WriteByReference));

        ref int result = ref target.PrimitiveReference();

        var expected = 42;
        Assert.That(result, Is.EqualTo(expected));
        Assert.That(target.Primitive, Is.EqualTo(expected));

        var later = 73;
        result = later;
        Assert.That(target.Primitive, Is.EqualTo(later));
    }

    [Test]
    public void InnerPostfix_Result_Primitive_ReadByValue()
    {
        RefReturnPatchingPatches.Observed = default;
        var original = 11;
        var target = new RefReturnTargets { Primitive = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.InnerPostfix_Result_Primitive_ReadByValue));

        ref int result = ref RefReturnTargets.CallPrimitiveReference(target);

        Assert.That(result, Is.EqualTo(original));
        Assert.That(target.Primitive, Is.EqualTo(original));
        Assert.That(RefReturnPatchingPatches.Observed, Is.EqualTo(original));

        var later = 73;
        result = later;
        Assert.That(target.Primitive, Is.EqualTo(later));
    }

    [Test]
    public void InnerPostfix_Result_Primitive_ReadByReference()
    {
        RefReturnPatchingPatches.Observed = default;
        var original = 11;
        var target = new RefReturnTargets { Primitive = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.InnerPostfix_Result_Primitive_ReadByReference));

        ref int result = ref RefReturnTargets.CallPrimitiveReference(target);

        Assert.That(result, Is.EqualTo(original));
        Assert.That(target.Primitive, Is.EqualTo(original));
        Assert.That(RefReturnPatchingPatches.Observed, Is.EqualTo(original));

        var later = 73;
        result = later;
        Assert.That(target.Primitive, Is.EqualTo(later));
    }

    [Test]
    public void InnerPostfix_Result_Primitive_WriteByReference()
    {
        RefReturnPatchingPatches.Observed = default;
        var original = 11;
        var target = new RefReturnTargets { Primitive = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.InnerPostfix_Result_Primitive_WriteByReference));

        ref int result = ref RefReturnTargets.CallPrimitiveReference(target);

        var expected = 42;
        Assert.That(result, Is.EqualTo(expected));
        Assert.That(target.Primitive, Is.EqualTo(expected));

        var later = 73;
        result = later;
        Assert.That(target.Primitive, Is.EqualTo(later));
    }

    [Test]
    public void Prefix_Result_Primitive_SkipWithSuppliedReference()
    {
        var original = 11;
        var target = new RefReturnTargets { Primitive = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.Prefix_Result_Primitive_SkipWithSuppliedReference));

        ref int first = ref target.PrimitiveReference();
        ref int second = ref target.PrimitiveReference();
        var expected = 42;
        Assert.That(first, Is.EqualTo(expected));
        Assert.That(second, Is.EqualTo(expected));

        var later = 73;
        first = later;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.That(first, Is.EqualTo(later));
        Assert.That(second, Is.EqualTo(expected));
        Assert.That(target.Primitive, Is.EqualTo(original));
    }

    [Test]
    public void InnerPrefix_Result_Primitive_SkipWithSuppliedReference()
    {
        var original = 11;
        var target = new RefReturnTargets { Primitive = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.InnerPrefix_Result_Primitive_SkipWithSuppliedReference));

        ref int first = ref RefReturnTargets.CallPrimitiveReference(target);
        ref int second = ref RefReturnTargets.CallPrimitiveReference(target);
        var expected = 42;
        Assert.That(first, Is.EqualTo(expected));
        Assert.That(second, Is.EqualTo(expected));

        var later = 73;
        first = later;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.That(first, Is.EqualTo(later));
        Assert.That(second, Is.EqualTo(expected));
        Assert.That(target.Primitive, Is.EqualTo(original));
    }

    [Test]
    public void Postfix_Result_ReferenceType_ReadByValue()
    {
        RefReturnPatchingPatches.ReferenceObserved = default;
        RefReturnPatchingPatches.ReplacementReference = new BindingReference { Value = 42 };
        var original = new BindingReference { Value = 11 };
        var target = new RefReturnTargets { Reference = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.Postfix_Result_ReferenceType_ReadByValue));

        ref BindingReference result = ref target.ObjectReference();

        Assert.That(result, Is.SameAs(original));
        Assert.That(target.Reference, Is.SameAs(original));
        Assert.That(RefReturnPatchingPatches.ReferenceObserved, Is.SameAs(original));

        var later = new BindingReference { Value = 73 };
        result = later;
        Assert.That(target.Reference, Is.SameAs(later));
    }

    [Test]
    public void Postfix_Result_ReferenceType_ReadByReference()
    {
        RefReturnPatchingPatches.ReferenceObserved = default;
        RefReturnPatchingPatches.ReplacementReference = new BindingReference { Value = 42 };
        var original = new BindingReference { Value = 11 };
        var target = new RefReturnTargets { Reference = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.Postfix_Result_ReferenceType_ReadByReference));

        ref BindingReference result = ref target.ObjectReference();

        Assert.That(result, Is.SameAs(original));
        Assert.That(target.Reference, Is.SameAs(original));
        Assert.That(RefReturnPatchingPatches.ReferenceObserved, Is.SameAs(original));

        var later = new BindingReference { Value = 73 };
        result = later;
        Assert.That(target.Reference, Is.SameAs(later));
    }

    [Test]
    public void Postfix_Result_ReferenceType_WriteByReference()
    {
        RefReturnPatchingPatches.ReferenceObserved = default;
        RefReturnPatchingPatches.ReplacementReference = new BindingReference { Value = 42 };
        var original = new BindingReference { Value = 11 };
        var target = new RefReturnTargets { Reference = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.Postfix_Result_ReferenceType_WriteByReference));

        ref BindingReference result = ref target.ObjectReference();

        var expected = RefReturnPatchingPatches.ReplacementReference!;
        Assert.That(result, Is.SameAs(expected));
        Assert.That(target.Reference, Is.SameAs(expected));

        var later = new BindingReference { Value = 73 };
        result = later;
        Assert.That(target.Reference, Is.SameAs(later));
    }

    [Test]
    public void InnerPostfix_Result_ReferenceType_ReadByValue()
    {
        RefReturnPatchingPatches.ReferenceObserved = default;
        RefReturnPatchingPatches.ReplacementReference = new BindingReference { Value = 42 };
        var original = new BindingReference { Value = 11 };
        var target = new RefReturnTargets { Reference = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.InnerPostfix_Result_ReferenceType_ReadByValue));

        ref BindingReference result = ref RefReturnTargets.CallObjectReference(target);

        Assert.That(result, Is.SameAs(original));
        Assert.That(target.Reference, Is.SameAs(original));
        Assert.That(RefReturnPatchingPatches.ReferenceObserved, Is.SameAs(original));

        var later = new BindingReference { Value = 73 };
        result = later;
        Assert.That(target.Reference, Is.SameAs(later));
    }

    [Test]
    public void InnerPostfix_Result_ReferenceType_ReadByReference()
    {
        RefReturnPatchingPatches.ReferenceObserved = default;
        RefReturnPatchingPatches.ReplacementReference = new BindingReference { Value = 42 };
        var original = new BindingReference { Value = 11 };
        var target = new RefReturnTargets { Reference = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.InnerPostfix_Result_ReferenceType_ReadByReference));

        ref BindingReference result = ref RefReturnTargets.CallObjectReference(target);

        Assert.That(result, Is.SameAs(original));
        Assert.That(target.Reference, Is.SameAs(original));
        Assert.That(RefReturnPatchingPatches.ReferenceObserved, Is.SameAs(original));

        var later = new BindingReference { Value = 73 };
        result = later;
        Assert.That(target.Reference, Is.SameAs(later));
    }

    [Test]
    public void InnerPostfix_Result_ReferenceType_WriteByReference()
    {
        RefReturnPatchingPatches.ReferenceObserved = default;
        RefReturnPatchingPatches.ReplacementReference = new BindingReference { Value = 42 };
        var original = new BindingReference { Value = 11 };
        var target = new RefReturnTargets { Reference = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.InnerPostfix_Result_ReferenceType_WriteByReference));

        ref BindingReference result = ref RefReturnTargets.CallObjectReference(target);

        var expected = RefReturnPatchingPatches.ReplacementReference!;
        Assert.That(result, Is.SameAs(expected));
        Assert.That(target.Reference, Is.SameAs(expected));

        var later = new BindingReference { Value = 73 };
        result = later;
        Assert.That(target.Reference, Is.SameAs(later));
    }

    [Test]
    public void Prefix_Result_ReferenceType_SkipWithSuppliedReference()
    {
        RefReturnPatchingPatches.ReplacementReference = new BindingReference { Value = 42 };
        var original = new BindingReference { Value = 11 };
        var target = new RefReturnTargets { Reference = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.Prefix_Result_ReferenceType_SkipWithSuppliedReference));

        ref BindingReference first = ref target.ObjectReference();
        ref BindingReference second = ref target.ObjectReference();
        var expected = RefReturnPatchingPatches.ReplacementReference!;
        Assert.That(first, Is.SameAs(expected));
        Assert.That(second, Is.SameAs(expected));

        var later = new BindingReference { Value = 73 };
        first = later;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.That(first, Is.SameAs(later));
        Assert.That(second, Is.SameAs(expected));
        Assert.That(target.Reference, Is.SameAs(original));
    }

    [Test]
    public void InnerPrefix_Result_ReferenceType_SkipWithSuppliedReference()
    {
        RefReturnPatchingPatches.ReplacementReference = new BindingReference { Value = 42 };
        var original = new BindingReference { Value = 11 };
        var target = new RefReturnTargets { Reference = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.InnerPrefix_Result_ReferenceType_SkipWithSuppliedReference));

        ref BindingReference first = ref RefReturnTargets.CallObjectReference(target);
        ref BindingReference second = ref RefReturnTargets.CallObjectReference(target);
        var expected = RefReturnPatchingPatches.ReplacementReference!;
        Assert.That(first, Is.SameAs(expected));
        Assert.That(second, Is.SameAs(expected));

        var later = new BindingReference { Value = 73 };
        first = later;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.That(first, Is.SameAs(later));
        Assert.That(second, Is.SameAs(expected));
        Assert.That(target.Reference, Is.SameAs(original));
    }

    [Test]
    public void Postfix_Result_Struct_ReadByValue()
    {
        RefReturnPatchingPatches.StructObserved = default;
        var original = new BindingStruct { Value = 11 };
        var target = new RefReturnTargets { Structure = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.Postfix_Result_Struct_ReadByValue));

        ref BindingStruct result = ref target.StructReference();

        Assert.That(result, Is.EqualTo(original));
        Assert.That(target.Structure, Is.EqualTo(original));
        Assert.That(RefReturnPatchingPatches.StructObserved, Is.EqualTo(original));

        var later = new BindingStruct { Value = 73 };
        result = later;
        Assert.That(target.Structure, Is.EqualTo(later));
    }

    [Test]
    public void Postfix_Result_Struct_ReadByReference()
    {
        RefReturnPatchingPatches.StructObserved = default;
        var original = new BindingStruct { Value = 11 };
        var target = new RefReturnTargets { Structure = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.Postfix_Result_Struct_ReadByReference));

        ref BindingStruct result = ref target.StructReference();

        Assert.That(result, Is.EqualTo(original));
        Assert.That(target.Structure, Is.EqualTo(original));
        Assert.That(RefReturnPatchingPatches.StructObserved, Is.EqualTo(original));

        var later = new BindingStruct { Value = 73 };
        result = later;
        Assert.That(target.Structure, Is.EqualTo(later));
    }

    [Test]
    public void Postfix_Result_Struct_WriteByReference()
    {
        RefReturnPatchingPatches.StructObserved = default;
        var original = new BindingStruct { Value = 11 };
        var target = new RefReturnTargets { Structure = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.Postfix_Result_Struct_WriteByReference));

        ref BindingStruct result = ref target.StructReference();

        var expected = new BindingStruct { Value = 42 };
        Assert.That(result, Is.EqualTo(expected));
        Assert.That(target.Structure, Is.EqualTo(expected));

        var later = new BindingStruct { Value = 73 };
        result = later;
        Assert.That(target.Structure, Is.EqualTo(later));
    }

    [Test]
    public void InnerPostfix_Result_Struct_ReadByValue()
    {
        RefReturnPatchingPatches.StructObserved = default;
        var original = new BindingStruct { Value = 11 };
        var target = new RefReturnTargets { Structure = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.InnerPostfix_Result_Struct_ReadByValue));

        ref BindingStruct result = ref RefReturnTargets.CallStructReference(target);

        Assert.That(result, Is.EqualTo(original));
        Assert.That(target.Structure, Is.EqualTo(original));
        Assert.That(RefReturnPatchingPatches.StructObserved, Is.EqualTo(original));

        var later = new BindingStruct { Value = 73 };
        result = later;
        Assert.That(target.Structure, Is.EqualTo(later));
    }

    [Test]
    public void InnerPostfix_Result_Struct_ReadByReference()
    {
        RefReturnPatchingPatches.StructObserved = default;
        var original = new BindingStruct { Value = 11 };
        var target = new RefReturnTargets { Structure = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.InnerPostfix_Result_Struct_ReadByReference));

        ref BindingStruct result = ref RefReturnTargets.CallStructReference(target);

        Assert.That(result, Is.EqualTo(original));
        Assert.That(target.Structure, Is.EqualTo(original));
        Assert.That(RefReturnPatchingPatches.StructObserved, Is.EqualTo(original));

        var later = new BindingStruct { Value = 73 };
        result = later;
        Assert.That(target.Structure, Is.EqualTo(later));
    }

    [Test]
    public void InnerPostfix_Result_Struct_WriteByReference()
    {
        RefReturnPatchingPatches.StructObserved = default;
        var original = new BindingStruct { Value = 11 };
        var target = new RefReturnTargets { Structure = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.InnerPostfix_Result_Struct_WriteByReference));

        ref BindingStruct result = ref RefReturnTargets.CallStructReference(target);

        var expected = new BindingStruct { Value = 42 };
        Assert.That(result, Is.EqualTo(expected));
        Assert.That(target.Structure, Is.EqualTo(expected));

        var later = new BindingStruct { Value = 73 };
        result = later;
        Assert.That(target.Structure, Is.EqualTo(later));
    }

    [Test]
    public void Prefix_Result_Struct_SkipWithSuppliedReference()
    {
        var original = new BindingStruct { Value = 11 };
        var target = new RefReturnTargets { Structure = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.Prefix_Result_Struct_SkipWithSuppliedReference));

        ref BindingStruct first = ref target.StructReference();
        ref BindingStruct second = ref target.StructReference();
        var expected = new BindingStruct { Value = 42 };
        Assert.That(first, Is.EqualTo(expected));
        Assert.That(second, Is.EqualTo(expected));

        var later = new BindingStruct { Value = 73 };
        first = later;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.That(first, Is.EqualTo(later));
        Assert.That(second, Is.EqualTo(expected));
        Assert.That(target.Structure, Is.EqualTo(original));
    }

    [Test]
    public void InnerPrefix_Result_Struct_SkipWithSuppliedReference()
    {
        var original = new BindingStruct { Value = 11 };
        var target = new RefReturnTargets { Structure = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.InnerPrefix_Result_Struct_SkipWithSuppliedReference));

        ref BindingStruct first = ref RefReturnTargets.CallStructReference(target);
        ref BindingStruct second = ref RefReturnTargets.CallStructReference(target);
        var expected = new BindingStruct { Value = 42 };
        Assert.That(first, Is.EqualTo(expected));
        Assert.That(second, Is.EqualTo(expected));

        var later = new BindingStruct { Value = 73 };
        first = later;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.That(first, Is.EqualTo(later));
        Assert.That(second, Is.EqualTo(expected));
        Assert.That(target.Structure, Is.EqualTo(original));
    }

    [Test]
    public void Prefix_StaticField_PreservesAliasOnFirstAndSubsequentCalls()
    {
        RefReturnPatchingPatches.ExecutionCount = 0;
        BoundaryTargets.RefReturnStorage = 11;
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.Prefix_StaticField_PreservesAliasOnFirstAndSubsequentCalls));

        ref int first = ref BoundaryTargets.RefReturnMethod();
        Assert.That(first, Is.EqualTo(11));
        first = 23;
        Assert.That(BoundaryTargets.RefReturnStorage, Is.EqualTo(23));

        ref int second = ref BoundaryTargets.RefReturnMethod();
        second = 37;
        Assert.That(first, Is.EqualTo(37));
        Assert.That(BoundaryTargets.RefReturnStorage, Is.EqualTo(37));
        Assert.That(RefReturnPatchingPatches.ExecutionCount, Is.EqualTo(2));
    }

    [Test]
    public void Postfix_InstancePrimitive_PreservesAlias()
    {
        RefReturnPatchingPatches.ExecutionCount = 0;
        var target = new RefReturnTargets { Primitive = 11 };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.Postfix_InstancePrimitive_PreservesAlias));

        ref int result = ref target.PrimitiveReference();
        Assert.That(result, Is.EqualTo(11));
        result = 23;
        Assert.That(target.Primitive, Is.EqualTo(23));
        target.Primitive = 37;
        Assert.That(result, Is.EqualTo(37));
        Assert.That(RefReturnPatchingPatches.ExecutionCount, Is.EqualTo(1));
    }

    [Test]
    public void Prefix_InstanceReferenceType_PreservesAlias()
    {
        RefReturnPatchingPatches.ExecutionCount = 0;
        var original = new BindingReference { Value = 11 };
        var target = new RefReturnTargets { Reference = original };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.Prefix_InstanceReferenceType_PreservesAlias));

        ref BindingReference result = ref target.ObjectReference();
        Assert.That(result, Is.SameAs(original));
        var replacement = new BindingReference { Value = 23 };
        result = replacement;
        Assert.That(target.Reference, Is.SameAs(replacement));
        Assert.That(RefReturnPatchingPatches.ExecutionCount, Is.EqualTo(1));
    }

    [Test]
    public void Postfix_InstanceStruct_PreservesAlias()
    {
        RefReturnPatchingPatches.ExecutionCount = 0;
        var target = new RefReturnTargets { Structure = new BindingStruct { Value = 11 } };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.Postfix_InstanceStruct_PreservesAlias));

        ref BindingStruct result = ref target.StructReference();
        Assert.That(result.Value, Is.EqualTo(11));
        result.Value = 23;
        Assert.That(target.Structure.Value, Is.EqualTo(23));
        Assert.That(RefReturnPatchingPatches.ExecutionCount, Is.EqualTo(1));
    }

    [Test]
    public void Prefix_RefArgument_PreservesCallerStorage()
    {
        RefReturnPatchingPatches.ExecutionCount = 0;
        int value = 11;
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.Prefix_RefArgument_PreservesCallerStorage));

        ref int result = ref RefReturnTargets.ArgumentReference(ref value);
        Assert.That(result, Is.EqualTo(11));
        result = 23;
        Assert.That(value, Is.EqualTo(23));
        Assert.That(RefReturnPatchingPatches.ExecutionCount, Is.EqualTo(1));
    }

    [Test]
    public void Postfix_ArrayElement_PreservesSelectedElement()
    {
        RefReturnPatchingPatches.ExecutionCount = 0;
        int[] values = [11, 23, 37];
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.Postfix_ArrayElement_PreservesSelectedElement));

        ref int result = ref RefReturnTargets.ArrayElement(values, 1);
        Assert.That(result, Is.EqualTo(23));
        result = 42;
        Assert.That(values, Is.EqualTo(new[] { 11, 42, 37 }));
        Assert.That(RefReturnPatchingPatches.ExecutionCount, Is.EqualTo(1));
    }

    [Test]
    public void Postfix_ReadonlyReference_PreservesLiveAlias()
    {
        RefReturnPatchingPatches.ExecutionCount = 0;
        var target = new RefReturnTargets { Primitive = 11 };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.Postfix_ReadonlyReference_PreservesLiveAlias));

        ref readonly int result = ref target.ReadonlyReference();
        Assert.That(result, Is.EqualTo(11));
        target.Primitive = 23;
        Assert.That(result, Is.EqualTo(23));
        Assert.That(RefReturnPatchingPatches.ExecutionCount, Is.EqualTo(1));
    }

    [Test]
    public void InnerPrefix_ArrayElement_PreservesAlias()
    {
        RefReturnPatchingPatches.ExecutionCount = 0;
        int[] values = [11, 23, 37];
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.InnerPrefix_ArrayElement_PreservesAlias));

        ref int result = ref RefReturnTargets.CallArrayElement(values, 1);
        Assert.That(result, Is.EqualTo(23));
        result = 42;
        Assert.That(values, Is.EqualTo(new[] { 11, 42, 37 }));
        Assert.That(RefReturnPatchingPatches.ExecutionCount, Is.EqualTo(1));
    }

    [Test]
    public void InnerPostfix_ArrayElement_PreservesAlias()
    {
        RefReturnPatchingPatches.ExecutionCount = 0;
        int[] values = [11, 23, 37];
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.InnerPostfix_ArrayElement_PreservesAlias));

        ref int result = ref RefReturnTargets.CallArrayElement(values, 1);
        Assert.That(result, Is.EqualTo(23));
        result = 42;
        Assert.That(values, Is.EqualTo(new[] { 11, 42, 37 }));
        Assert.That(RefReturnPatchingPatches.ExecutionCount, Is.EqualTo(1));
    }

    [Test]
    public void Postfix_Result_Primitive_ReadByValue()
    {
        RefReturnPatchingPatches.Observed = 0;
        var target = new RefReturnTargets { Primitive = 42 };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.Postfix_Result_Primitive_ReadByValue));

        ref int result = ref target.PrimitiveReference();

        Assert.That(RefReturnPatchingPatches.Observed, Is.EqualTo(42));
        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_Result_Primitive_ReadByReference()
    {
        RefReturnPatchingPatches.Observed = 0;
        var target = new RefReturnTargets { Primitive = 42 };
        ApplyPatch(typeof(RefReturnPatchingPatches), nameof(RefReturnPatchingPatches.Postfix_Result_Primitive_ReadByReference));

        ref int result = ref target.PrimitiveReference();

        Assert.That(RefReturnPatchingPatches.Observed, Is.EqualTo(42));
        Assert.That(result, Is.EqualTo(42));
    }

}
