namespace Disharmony.Tests.EndToEnd.ParameterBinding;

public static class FieldBindingPatches
{
    [Prefix]
    [Target(typeof(FieldLookupDerivedTargets), nameof(FieldLookupDerivedTargets.Target))]
    public static void Prefix_ExplicitTypeBaseField_Named_Primitive_ReadByValue(
        [Field(typeof(FieldLookupBaseTargets), nameof(FieldLookupBaseTargets.Value))] int field) =>
        observed = field;

    [Prefix]
    [Target(typeof(FieldLookupDerivedTargets), nameof(FieldLookupDerivedTargets.Target))]
    public static void Prefix_ExplicitTypeBaseField_NullName_Primitive_ReadByValue(
        [Field(typeof(FieldLookupBaseTargets), null)] int Value) =>
        observed = Value;

    [Prefix]
    [Target(typeof(FieldLookupDerivedTargets), nameof(FieldLookupDerivedTargets.Target))]
    public static void Prefix_ExplicitTypeBaseField_NullName_Primitive_ReadByReference(
        [Field(typeof(FieldLookupBaseTargets), null)] ref int Value) =>
        observed = Value;

    [Prefix]
    [Target(typeof(FieldLookupDerivedTargets), nameof(FieldLookupDerivedTargets.Target))]
    public static void Prefix_ExplicitTypeBaseField_NullName_Primitive_WriteByReference(
        [Field(typeof(FieldLookupBaseTargets), null)] ref int Value) =>
        Value = 42;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Prefix_ExplicitTypeStaticField_Named_Primitive_ReadByValue(
        [Field(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Field))] int field) =>
        observed = field;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Prefix_ExplicitTypeStaticField_NullName_Primitive_ReadByValue(
        [Field(typeof(InnerStaticMethodTargets), null)] int Field) =>
        observed = Field;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Prefix_ExplicitTypeStaticField_NullName_Primitive_ReadByReference(
        [Field(typeof(InnerStaticMethodTargets), null)] ref int Field) =>
        observed = Field;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Prefix_ExplicitTypeStaticField_NullName_Primitive_WriteByReference(
        [Field(typeof(InnerStaticMethodTargets), null)] ref int Field) =>
        Field = 42;

    [Prefix]
    [Target(typeof(FieldLookupDerivedTargets), nameof(FieldLookupDerivedTargets.Target))]
    public static void Prefix_QualifiedBaseField_Primitive_ReadByValue(
        [Field("FieldLookupBaseTargets.Value")] int field) =>
        observed = field;

    [Prefix]
    [Target(typeof(FieldLookupDerivedTargets), nameof(FieldLookupDerivedTargets.Target))]
    public static void Prefix_QualifiedBaseField_Primitive_ReadByReference(
        [Field("FieldLookupBaseTargets.Value")] ref int field) =>
        observed = field;

    [Prefix]
    [Target(typeof(FieldLookupDerivedTargets), nameof(FieldLookupDerivedTargets.Target))]
    public static void Prefix_QualifiedBaseField_Primitive_WriteByReference(
        [Field("FieldLookupBaseTargets.Value")] ref int field) =>
        field = 42;

    [Prefix]
    [Inner(typeof(FieldLookupDerivedTargets), nameof(FieldLookupDerivedTargets.Target))]
    [Target(typeof(FieldLookupDerivedTargets), nameof(FieldLookupDerivedTargets.CallInner))]
    public static void InnerPrefix_QualifiedInnerBaseField_Primitive_ReadByValue(
        [Field("FieldLookupBaseTargets.Value")] int field) =>
        observed = field;

    [Prefix]
    [Inner(typeof(FieldLookupDerivedTargets), nameof(FieldLookupDerivedTargets.Target))]
    [Target(typeof(FieldLookupDerivedTargets), nameof(FieldLookupDerivedTargets.CallInner))]
    public static void InnerPrefix_QualifiedInnerBaseField_Primitive_ReadByReference(
        [Field("FieldLookupBaseTargets.Value")] ref int field) =>
        observed = field;

    [Prefix]
    [Inner(typeof(FieldLookupDerivedTargets), nameof(FieldLookupDerivedTargets.Target))]
    [Target(typeof(FieldLookupDerivedTargets), nameof(FieldLookupDerivedTargets.CallInner))]
    public static void InnerPrefix_QualifiedInnerBaseField_Primitive_WriteByReference(
        [Field("FieldLookupBaseTargets.Value")] ref int field) =>
        field = 42;

    [Prefix]
    [Inner(typeof(FieldLookupDerivedTargets), nameof(FieldLookupDerivedTargets.Target))]
    [Target(typeof(FieldLookupDerivedTargets), nameof(FieldLookupDerivedTargets.CallInner))]
    public static void InnerPrefix_QualifiedOuterBaseField_Primitive_ReadByValue(
        [Field("FieldLookupBaseTargets.Value", Scope.Outer)] int field) =>
        observed = field;

    [Prefix]
    [Inner(typeof(FieldLookupDerivedTargets), nameof(FieldLookupDerivedTargets.Target))]
    [Target(typeof(FieldLookupDerivedTargets), nameof(FieldLookupDerivedTargets.CallInner))]
    public static void InnerPrefix_QualifiedOuterBaseField_Primitive_ReadByReference(
        [Field("FieldLookupBaseTargets.Value", Scope.Outer)] ref int field) =>
        observed = field;

    [Prefix]
    [Inner(typeof(FieldLookupDerivedTargets), nameof(FieldLookupDerivedTargets.Target))]
    [Target(typeof(FieldLookupDerivedTargets), nameof(FieldLookupDerivedTargets.CallInner))]
    public static void InnerPrefix_QualifiedOuterBaseField_Primitive_WriteByReference(
        [Field("FieldLookupBaseTargets.Value", Scope.Outer)] ref int field) =>
        field = 42;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Prefix_QualifiedStaticField_Primitive_ReadByValue(
        [Field("Disharmony.Tests.InnerStaticMethodTargets.Field")] int field) =>
        observed = field;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Prefix_QualifiedStaticField_Primitive_ReadByReference(
        [Field("Disharmony.Tests.InnerStaticMethodTargets.Field")] ref int field) =>
        observed = field;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Prefix_QualifiedStaticField_Primitive_WriteByReference(
        [Field("Disharmony.Tests.InnerStaticMethodTargets.Field")] ref int field) =>
        field = 42;

    [Prefix]
    [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.IntIdentity))]
    public static void InnerPrefix_QualifiedInnerStaticField_Primitive_ReadByValue(
        [Field("Disharmony.Tests.InnerStaticMethodTargets.Field")] int field) =>
        observed = field;

    [Prefix]
    [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.IntIdentity))]
    public static void InnerPrefix_QualifiedInnerStaticField_Primitive_ReadByReference(
        [Field("Disharmony.Tests.InnerStaticMethodTargets.Field")] ref int field) =>
        observed = field;

    [Prefix]
    [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntIdentity))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.IntIdentity))]
    public static void InnerPrefix_QualifiedInnerStaticField_Primitive_WriteByReference(
        [Field("Disharmony.Tests.InnerStaticMethodTargets.Field")] ref int field) =>
        field = 42;

    public static int observed;
    public static BindingReference? referenceObserved;
    public static BindingStruct structObserved;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_TripleUnderscoreField_Primitive_ReadByValue(int ___primitiveField) => observed = ___primitiveField;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_TripleUnderscoreField_Primitive_WriteByReference(ref int ___primitiveField) => ___primitiveField = 42;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_TripleUnderscoreField_Primitive_ReadByReference(ref int ___primitiveField) =>
        observed = ___primitiveField;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_TripleUnderscoreField_ReferenceType_ReadByValue(BindingReference ___referenceField) =>
        referenceObserved = ___referenceField;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_TripleUnderscoreField_ReferenceType_WriteByReference(ref BindingReference ___referenceField) =>
        ___referenceField = new BindingReference { Value = 42 };

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_TripleUnderscoreField_ReferenceType_ReadByReference(
        ref BindingReference ___referenceField) => referenceObserved = ___referenceField;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_TripleUnderscoreField_Struct_ReadByValue(BindingStruct ___structField) => structObserved = ___structField;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_TripleUnderscoreField_Struct_WriteByReference(ref BindingStruct ___structField) =>
        ___structField = new BindingStruct { Value = 42 };

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_TripleUnderscoreField_Struct_ReadByReference(ref BindingStruct ___structField) =>
        structObserved = ___structField;

    [Prefix] [Inner(typeof(InstanceMethodTargetsWithoutFields), nameof(InstanceMethodTargetsWithoutFields.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallInnerWithoutField))]
    public static void InnerPrefix_TripleUnderscoreField_OuterClassInstance_Primitive_ReadByValue(int ___foo) => observed = ___foo;

    [Postfix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallInnerWithField))]
    public static void InnerPostfix_TripleUnderscoreField_InnerClassInstance_Primitive_ReadByValue_WhenBothScopesMatch(int ___foo) =>
        observed = ___foo;

    [Prefix] [Inner(typeof(InstanceMethodTargetsWithoutFields), nameof(InstanceMethodTargetsWithoutFields.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallInnerWithoutField))]
    public static void InnerPrefix_TripleUnderscoreField_OuterClassInstance_Primitive_WriteByReference(ref int ___foo) => ___foo = 42;

    [Prefix] [Inner(typeof(InstanceMethodTargetsWithoutFields), nameof(InstanceMethodTargetsWithoutFields.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallInnerWithoutField))]
    public static void InnerPrefix_TripleUnderscoreField_OuterClassInstance_Primitive_ReadByReference(ref int ___foo) => observed = ___foo;

    [Postfix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallInnerWithField))]
    public static void InnerPostfix_TripleUnderscoreField_InnerClassInstance_Primitive_WriteByReference(ref int ___foo) => ___foo = 42;

    [Postfix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallInnerWithField))]
    public static void InnerPostfix_TripleUnderscoreField_InnerClassInstance_Primitive_ReadByReference(ref int ___foo) => observed = ___foo;

    [Prefix] [Inner(typeof(InstanceMethodTargetsWithoutFields), nameof(InstanceMethodTargetsWithoutFields.Void))]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.CallInnerWithoutField))]
    public static void InnerPrefix_TripleUnderscoreField_OuterStructInstance_Primitive_ReadByValue(int ___foo) => observed = ___foo;

    [Postfix] [Inner(typeof(InnerStructMethodTargets), nameof(InnerStructMethodTargets.Void))]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.CallInnerWithField))]
    public static void InnerPostfix_TripleUnderscoreField_InnerStructInstance_Primitive_ReadByValue_WhenBothScopesMatch(int ___foo) =>
        observed = ___foo;

    [Prefix] [Inner(typeof(InstanceMethodTargetsWithoutFields), nameof(InstanceMethodTargetsWithoutFields.Void))]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.CallInnerWithoutField))]
    public static void InnerPrefix_TripleUnderscoreField_OuterStructInstance_Primitive_WriteByReference(ref int ___foo) => ___foo = 42;

    [Prefix] [Inner(typeof(InstanceMethodTargetsWithoutFields), nameof(InstanceMethodTargetsWithoutFields.Void))]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.CallInnerWithoutField))]
    public static void InnerPrefix_TripleUnderscoreField_OuterStructInstance_Primitive_ReadByReference(ref int ___foo) => observed = ___foo;

    [Prefix] [Inner(typeof(InnerStructMethodTargets), nameof(InnerStructMethodTargets.Void))]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.CallInnerWithField))]
    public static void InnerPrefix_TripleUnderscoreField_InnerStructInstance_Primitive_WriteByReference(ref int ___foo) => ___foo = 42;

    [Prefix] [Inner(typeof(InnerStructMethodTargets), nameof(InnerStructMethodTargets.Void))]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.CallInnerWithField))]
    public static void InnerPrefix_TripleUnderscoreField_InnerStructInstance_Primitive_ReadByReference(ref int ___foo) => observed = ___foo;

    [Prefix] [Inner(typeof(InnerStructMethodTargets), nameof(InnerStructMethodTargets.Void))]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.CallInnerWithFieldByValue))]
    public static void InnerPrefix_TripleUnderscoreField_InnerStructPassedByValue_Primitive_WriteByReference(ref int ___foo) => ___foo = 42;

    [Prefix] [Inner(typeof(InnerStructMethodTargets), nameof(InnerStructMethodTargets.Void))]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.CallInnerWithFieldByValue))]
    public static void InnerPrefix_TripleUnderscoreField_InnerStructPassedByValue_Primitive_ReadByReference(ref int ___foo) =>
        observed = ___foo;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_FieldAttribute_Primitive_WriteByReference([Field("foo")] ref int field) => field = 42;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_FieldAttribute_Primitive_ReadByReference([Field("foo")] ref int field) => observed = field;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_FieldAttribute_Primitive_ReadByValue([Field("primitiveField")] int field) => observed = field;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_FieldAttribute_NullName_UsesParameterName([Field] int primitiveField) =>
        observed = primitiveField;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_FieldAttribute_ReferenceType_ReadByValue([Field("referenceField")] BindingReference field) =>
        referenceObserved = field;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_FieldAttribute_ReferenceType_WriteByReference([Field("referenceField")] ref BindingReference field) =>
        field = new BindingReference { Value = 42 };

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_FieldAttribute_ReferenceType_ReadByReference(
        [Field("referenceField")] ref BindingReference field) => referenceObserved = field;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_FieldAttribute_Struct_ReadByValue([Field("structField")] BindingStruct field) => structObserved = field;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_FieldAttribute_Struct_WriteByReference([Field("structField")] ref BindingStruct field) =>
        field = new BindingStruct { Value = 42 };

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_FieldAttribute_Struct_ReadByReference([Field("structField")] ref BindingStruct field) =>
        structObserved = field;

    [Prefix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallInnerWithField))]
    public static void InnerPrefix_FieldAttribute_OuterScope_Primitive_ReadByValue([Field("foo", Scope.Outer)] int field) =>
        observed = field;

    [Prefix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallInnerWithField))]
    public static void InnerPrefix_FieldAttribute_InnerScope_Primitive_ReadByValue([Field("foo", Scope.Inner)] int field) =>
        observed = field;

    [Prefix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallInnerWithField))]
    public static void InnerPrefix_FieldAttribute_OuterScope_AutoPropertyBackingField_ReadByValue(
        [Field(nameof(ClassMethodTargets.AutoProperty), Scope.Outer)] int field) => observed = field;

    [Prefix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallInnerWithField))]
    public static void InnerPrefix_FieldAttribute_OuterScope_AutoPropertyBackingField_ReadByReference(
        [Field(nameof(ClassMethodTargets.AutoProperty), Scope.Outer)] ref int field) => observed = field;

    [Prefix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallInnerWithField))]
    public static void InnerPrefix_FieldAttribute_OuterScope_AutoPropertyBackingField_WriteByReference(
        [Field(nameof(ClassMethodTargets.AutoProperty), Scope.Outer)] ref int field) => field = 42;

    [Prefix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallInnerWithField))]
    public static void InnerPrefix_FieldAttribute_InnerScope_AutoPropertyBackingField_ReadByValue(
        [Field(nameof(InnerInstanceMethodTargets.AutoProperty), Scope.Inner)] int field) => observed = field;

    [Prefix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallInnerWithField))]
    public static void InnerPrefix_FieldAttribute_InnerScope_AutoPropertyBackingField_ReadByReference(
        [Field(nameof(InnerInstanceMethodTargets.AutoProperty), Scope.Inner)] ref int field) => observed = field;

    [Prefix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallInnerWithField))]
    public static void InnerPrefix_FieldAttribute_InnerScope_AutoPropertyBackingField_WriteByReference(
        [Field(nameof(InnerInstanceMethodTargets.AutoProperty), Scope.Inner)] ref int field) => field = 42;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_FieldAttribute_Struct_AsObjectByWriteableReference_Rejected(
        [Field("structField")] ref object field) { }

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_FieldAttribute_Struct_AsObjectByReadonlyReference_Rejected(
        [Field("structField")] in object field) { }

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_FieldAttribute_ReferenceType_AsObjectByWriteableReference_Rejected(
        [Field("referenceField")] ref object field) { }

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_FieldAttribute_ReferenceType_AsObjectByReadonlyReference(
        [Field("referenceField")] in object field) => referenceObserved = (BindingReference)field;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_FieldAttribute_ReferenceType_AsStringByOutReference(
        [Field("objectField")] out string field) => field = "patched";
}

[TestFixture]
public sealed class FieldBindingTests : PatchTestBase
{
    [Test]
    public void Prefix_ExplicitTypeBaseField_Named_Primitive_ReadByValue()
    {
        FieldBindingPatches.observed = 0;
        var target = new FieldLookupDerivedTargets { Value = 21 };
        ((FieldLookupBaseTargets)target).Value = 11;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_ExplicitTypeBaseField_Named_Primitive_ReadByValue));

        target.Target();

        Assert.Multiple(() =>
        {
            Assert.That(FieldBindingPatches.observed, Is.EqualTo(11));
            Assert.That(((FieldLookupBaseTargets)target).Value, Is.EqualTo(11));
            Assert.That(target.Value, Is.EqualTo(21));
        });
    }

    [Test]
    public void Prefix_ExplicitTypeBaseField_NullName_Primitive_ReadByValue()
    {
        FieldBindingPatches.observed = 0;
        var target = new FieldLookupDerivedTargets { Value = 21 };
        ((FieldLookupBaseTargets)target).Value = 11;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_ExplicitTypeBaseField_NullName_Primitive_ReadByValue));

        target.Target();

        Assert.Multiple(() =>
        {
            Assert.That(FieldBindingPatches.observed, Is.EqualTo(11));
            Assert.That(((FieldLookupBaseTargets)target).Value, Is.EqualTo(11));
            Assert.That(target.Value, Is.EqualTo(21));
        });
    }

    [Test]
    public void Prefix_ExplicitTypeBaseField_NullName_Primitive_ReadByReference()
    {
        FieldBindingPatches.observed = 0;
        var target = new FieldLookupDerivedTargets { Value = 21 };
        ((FieldLookupBaseTargets)target).Value = 11;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_ExplicitTypeBaseField_NullName_Primitive_ReadByReference));

        target.Target();

        Assert.Multiple(() =>
        {
            Assert.That(FieldBindingPatches.observed, Is.EqualTo(11));
            Assert.That(((FieldLookupBaseTargets)target).Value, Is.EqualTo(11));
            Assert.That(target.Value, Is.EqualTo(21));
        });
    }

    [Test]
    public void Prefix_ExplicitTypeBaseField_NullName_Primitive_WriteByReference()
    {
        FieldBindingPatches.observed = 0;
        var target = new FieldLookupDerivedTargets { Value = 21 };
        ((FieldLookupBaseTargets)target).Value = 11;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_ExplicitTypeBaseField_NullName_Primitive_WriteByReference));

        target.Target();

        Assert.Multiple(() =>
        {
            Assert.That(((FieldLookupBaseTargets)target).Value, Is.EqualTo(42));
            Assert.That(target.Value, Is.EqualTo(21));
        });
    }

    [Test]
    public void Prefix_ExplicitTypeStaticField_Named_Primitive_ReadByValue()
    {
        FieldBindingPatches.observed = 0;
        InnerStaticMethodTargets.Field = 11;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_ExplicitTypeStaticField_Named_Primitive_ReadByValue));

        StaticMethodTargets.Void();

        Assert.Multiple(() =>
        {
            Assert.That(FieldBindingPatches.observed, Is.EqualTo(11));
            Assert.That(InnerStaticMethodTargets.Field, Is.EqualTo(11));
        });
    }

    [Test]
    public void Prefix_ExplicitTypeStaticField_NullName_Primitive_ReadByValue()
    {
        FieldBindingPatches.observed = 0;
        InnerStaticMethodTargets.Field = 11;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_ExplicitTypeStaticField_NullName_Primitive_ReadByValue));

        StaticMethodTargets.Void();

        Assert.Multiple(() =>
        {
            Assert.That(FieldBindingPatches.observed, Is.EqualTo(11));
            Assert.That(InnerStaticMethodTargets.Field, Is.EqualTo(11));
        });
    }

    [Test]
    public void Prefix_ExplicitTypeStaticField_NullName_Primitive_ReadByReference()
    {
        FieldBindingPatches.observed = 0;
        InnerStaticMethodTargets.Field = 11;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_ExplicitTypeStaticField_NullName_Primitive_ReadByReference));

        StaticMethodTargets.Void();

        Assert.Multiple(() =>
        {
            Assert.That(FieldBindingPatches.observed, Is.EqualTo(11));
            Assert.That(InnerStaticMethodTargets.Field, Is.EqualTo(11));
        });
    }

    [Test]
    public void Prefix_ExplicitTypeStaticField_NullName_Primitive_WriteByReference()
    {
        FieldBindingPatches.observed = 0;
        InnerStaticMethodTargets.Field = 11;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_ExplicitTypeStaticField_NullName_Primitive_WriteByReference));

        StaticMethodTargets.Void();

        Assert.Multiple(() =>
        {
            Assert.That(InnerStaticMethodTargets.Field, Is.EqualTo(42));
        });
    }

    [Test]
    public void Prefix_QualifiedBaseField_Primitive_ReadByValue()
    {
        FieldBindingPatches.observed = 0;
        var target = new FieldLookupDerivedTargets { Value = 21 };
        ((FieldLookupBaseTargets)target).Value = 11;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_QualifiedBaseField_Primitive_ReadByValue));

        target.Target();

        Assert.Multiple(() =>
        {
            Assert.That(FieldBindingPatches.observed, Is.EqualTo(11));
            Assert.That(((FieldLookupBaseTargets)target).Value, Is.EqualTo(11));
            Assert.That(target.Value, Is.EqualTo(21));
        });
    }

    [Test]
    public void Prefix_QualifiedBaseField_Primitive_ReadByReference()
    {
        FieldBindingPatches.observed = 0;
        var target = new FieldLookupDerivedTargets { Value = 21 };
        ((FieldLookupBaseTargets)target).Value = 11;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_QualifiedBaseField_Primitive_ReadByReference));

        target.Target();

        Assert.Multiple(() =>
        {
            Assert.That(FieldBindingPatches.observed, Is.EqualTo(11));
            Assert.That(((FieldLookupBaseTargets)target).Value, Is.EqualTo(11));
            Assert.That(target.Value, Is.EqualTo(21));
        });
    }

    [Test]
    public void Prefix_QualifiedBaseField_Primitive_WriteByReference()
    {
        FieldBindingPatches.observed = 0;
        var target = new FieldLookupDerivedTargets { Value = 21 };
        ((FieldLookupBaseTargets)target).Value = 11;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_QualifiedBaseField_Primitive_WriteByReference));

        target.Target();

        Assert.Multiple(() =>
        {
            Assert.That(((FieldLookupBaseTargets)target).Value, Is.EqualTo(42));
            Assert.That(target.Value, Is.EqualTo(21));
        });
    }

    [Test]
    public void InnerPrefix_QualifiedInnerBaseField_Primitive_ReadByValue()
    {
        FieldBindingPatches.observed = 0;
        var target = new FieldLookupDerivedTargets { Value = 21 };
        ((FieldLookupBaseTargets)target).Value = 11;
        var inner = new FieldLookupDerivedTargets { Value = 23 };
        ((FieldLookupBaseTargets)inner).Value = 13;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.InnerPrefix_QualifiedInnerBaseField_Primitive_ReadByValue));

        target.CallInner(inner);

        Assert.Multiple(() =>
        {
            Assert.That(FieldBindingPatches.observed, Is.EqualTo(13));
            Assert.That(((FieldLookupBaseTargets)target).Value, Is.EqualTo(11));
            Assert.That(target.Value, Is.EqualTo(21));
            Assert.That(((FieldLookupBaseTargets)inner).Value, Is.EqualTo(13));
            Assert.That(inner.Value, Is.EqualTo(23));
        });
    }

    [Test]
    public void InnerPrefix_QualifiedInnerBaseField_Primitive_ReadByReference()
    {
        FieldBindingPatches.observed = 0;
        var target = new FieldLookupDerivedTargets { Value = 21 };
        ((FieldLookupBaseTargets)target).Value = 11;
        var inner = new FieldLookupDerivedTargets { Value = 23 };
        ((FieldLookupBaseTargets)inner).Value = 13;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.InnerPrefix_QualifiedInnerBaseField_Primitive_ReadByReference));

        target.CallInner(inner);

        Assert.Multiple(() =>
        {
            Assert.That(FieldBindingPatches.observed, Is.EqualTo(13));
            Assert.That(((FieldLookupBaseTargets)target).Value, Is.EqualTo(11));
            Assert.That(target.Value, Is.EqualTo(21));
            Assert.That(((FieldLookupBaseTargets)inner).Value, Is.EqualTo(13));
            Assert.That(inner.Value, Is.EqualTo(23));
        });
    }

    [Test]
    public void InnerPrefix_QualifiedInnerBaseField_Primitive_WriteByReference()
    {
        FieldBindingPatches.observed = 0;
        var target = new FieldLookupDerivedTargets { Value = 21 };
        ((FieldLookupBaseTargets)target).Value = 11;
        var inner = new FieldLookupDerivedTargets { Value = 23 };
        ((FieldLookupBaseTargets)inner).Value = 13;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.InnerPrefix_QualifiedInnerBaseField_Primitive_WriteByReference));

        target.CallInner(inner);

        Assert.Multiple(() =>
        {
            Assert.That(((FieldLookupBaseTargets)target).Value, Is.EqualTo(11));
            Assert.That(target.Value, Is.EqualTo(21));
            Assert.That(((FieldLookupBaseTargets)inner).Value, Is.EqualTo(42));
            Assert.That(inner.Value, Is.EqualTo(23));
        });
    }

    [Test]
    public void InnerPrefix_QualifiedOuterBaseField_Primitive_ReadByValue()
    {
        FieldBindingPatches.observed = 0;
        var target = new FieldLookupDerivedTargets { Value = 21 };
        ((FieldLookupBaseTargets)target).Value = 11;
        var inner = new FieldLookupDerivedTargets { Value = 23 };
        ((FieldLookupBaseTargets)inner).Value = 13;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.InnerPrefix_QualifiedOuterBaseField_Primitive_ReadByValue));

        target.CallInner(inner);

        Assert.Multiple(() =>
        {
            Assert.That(FieldBindingPatches.observed, Is.EqualTo(11));
            Assert.That(((FieldLookupBaseTargets)target).Value, Is.EqualTo(11));
            Assert.That(target.Value, Is.EqualTo(21));
            Assert.That(((FieldLookupBaseTargets)inner).Value, Is.EqualTo(13));
            Assert.That(inner.Value, Is.EqualTo(23));
        });
    }

    [Test]
    public void InnerPrefix_QualifiedOuterBaseField_Primitive_ReadByReference()
    {
        FieldBindingPatches.observed = 0;
        var target = new FieldLookupDerivedTargets { Value = 21 };
        ((FieldLookupBaseTargets)target).Value = 11;
        var inner = new FieldLookupDerivedTargets { Value = 23 };
        ((FieldLookupBaseTargets)inner).Value = 13;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.InnerPrefix_QualifiedOuterBaseField_Primitive_ReadByReference));

        target.CallInner(inner);

        Assert.Multiple(() =>
        {
            Assert.That(FieldBindingPatches.observed, Is.EqualTo(11));
            Assert.That(((FieldLookupBaseTargets)target).Value, Is.EqualTo(11));
            Assert.That(target.Value, Is.EqualTo(21));
            Assert.That(((FieldLookupBaseTargets)inner).Value, Is.EqualTo(13));
            Assert.That(inner.Value, Is.EqualTo(23));
        });
    }

    [Test]
    public void InnerPrefix_QualifiedOuterBaseField_Primitive_WriteByReference()
    {
        FieldBindingPatches.observed = 0;
        var target = new FieldLookupDerivedTargets { Value = 21 };
        ((FieldLookupBaseTargets)target).Value = 11;
        var inner = new FieldLookupDerivedTargets { Value = 23 };
        ((FieldLookupBaseTargets)inner).Value = 13;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.InnerPrefix_QualifiedOuterBaseField_Primitive_WriteByReference));

        target.CallInner(inner);

        Assert.Multiple(() =>
        {
            Assert.That(((FieldLookupBaseTargets)target).Value, Is.EqualTo(42));
            Assert.That(target.Value, Is.EqualTo(21));
            Assert.That(((FieldLookupBaseTargets)inner).Value, Is.EqualTo(13));
            Assert.That(inner.Value, Is.EqualTo(23));
        });
    }

    [Test]
    public void Prefix_QualifiedStaticField_Primitive_ReadByValue()
    {
        FieldBindingPatches.observed = 0;
        InnerStaticMethodTargets.Field = 11;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_QualifiedStaticField_Primitive_ReadByValue));

        StaticMethodTargets.Void();

        Assert.Multiple(() =>
        {
            Assert.That(FieldBindingPatches.observed, Is.EqualTo(11));
            Assert.That(InnerStaticMethodTargets.Field, Is.EqualTo(11));
        });
    }

    [Test]
    public void Prefix_QualifiedStaticField_Primitive_ReadByReference()
    {
        FieldBindingPatches.observed = 0;
        InnerStaticMethodTargets.Field = 11;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_QualifiedStaticField_Primitive_ReadByReference));

        StaticMethodTargets.Void();

        Assert.Multiple(() =>
        {
            Assert.That(FieldBindingPatches.observed, Is.EqualTo(11));
            Assert.That(InnerStaticMethodTargets.Field, Is.EqualTo(11));
        });
    }

    [Test]
    public void Prefix_QualifiedStaticField_Primitive_WriteByReference()
    {
        FieldBindingPatches.observed = 0;
        InnerStaticMethodTargets.Field = 11;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_QualifiedStaticField_Primitive_WriteByReference));

        StaticMethodTargets.Void();

        Assert.Multiple(() =>
        {
            Assert.That(InnerStaticMethodTargets.Field, Is.EqualTo(42));
        });
    }

    [Test]
    public void InnerPrefix_QualifiedInnerStaticField_Primitive_ReadByValue()
    {
        FieldBindingPatches.observed = 0;
        InnerStaticMethodTargets.Field = 11;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.InnerPrefix_QualifiedInnerStaticField_Primitive_ReadByValue));

        OuterStaticMethodTargets.IntIdentity(7);

        Assert.Multiple(() =>
        {
            Assert.That(FieldBindingPatches.observed, Is.EqualTo(11));
            Assert.That(InnerStaticMethodTargets.Field, Is.EqualTo(11));
        });
    }

    [Test]
    public void InnerPrefix_QualifiedInnerStaticField_Primitive_ReadByReference()
    {
        FieldBindingPatches.observed = 0;
        InnerStaticMethodTargets.Field = 11;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.InnerPrefix_QualifiedInnerStaticField_Primitive_ReadByReference));

        OuterStaticMethodTargets.IntIdentity(7);

        Assert.Multiple(() =>
        {
            Assert.That(FieldBindingPatches.observed, Is.EqualTo(11));
            Assert.That(InnerStaticMethodTargets.Field, Is.EqualTo(11));
        });
    }

    [Test]
    public void InnerPrefix_QualifiedInnerStaticField_Primitive_WriteByReference()
    {
        FieldBindingPatches.observed = 0;
        InnerStaticMethodTargets.Field = 11;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.InnerPrefix_QualifiedInnerStaticField_Primitive_WriteByReference));

        OuterStaticMethodTargets.IntIdentity(7);

        Assert.Multiple(() =>
        {
            Assert.That(InnerStaticMethodTargets.Field, Is.EqualTo(42));
        });
    }

    [Test]
    public void Prefix_TripleUnderscoreField_Primitive_ReadByValue()
    {
        FieldBindingPatches.observed = 0;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_TripleUnderscoreField_Primitive_ReadByValue));
        var target = new ClassMethodTargets { primitiveField = 42 };

        target.Void();

        Assert.That(FieldBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_TripleUnderscoreField_Primitive_WriteByReference()
    {
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_TripleUnderscoreField_Primitive_WriteByReference));
        var target = new ClassMethodTargets { primitiveField = 1 };

        target.Void();

        Assert.That(target.primitiveField, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_TripleUnderscoreField_ReferenceType_ReadByValue()
    {
        FieldBindingPatches.referenceObserved = null;
        var field = new BindingReference { Value = 42 };
        var target = new ClassMethodTargets { referenceField = field };
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_TripleUnderscoreField_ReferenceType_ReadByValue));

        target.Void();

        Assert.That(FieldBindingPatches.referenceObserved, Is.SameAs(field));
    }

    [Test]
    public void Prefix_TripleUnderscoreField_ReferenceType_WriteByReference()
    {
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_TripleUnderscoreField_ReferenceType_WriteByReference));
        var target = new ClassMethodTargets { referenceField = new BindingReference { Value = 1 } };

        target.Void();

        Assert.That(target.referenceField.Value, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_TripleUnderscoreField_Struct_ReadByValue()
    {
        FieldBindingPatches.structObserved = default;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_TripleUnderscoreField_Struct_ReadByValue));
        var target = new ClassMethodTargets { structField = new BindingStruct { Value = 42 } };

        target.Void();

        Assert.That(FieldBindingPatches.structObserved.Value, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_TripleUnderscoreField_Struct_WriteByReference()
    {
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_TripleUnderscoreField_Struct_WriteByReference));
        var target = new ClassMethodTargets { structField = new BindingStruct { Value = 1 } };

        target.Void();

        Assert.That(target.structField.Value, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_TripleUnderscoreField_OuterClassInstance_Primitive_ReadByValue()
    {
        FieldBindingPatches.observed = 0;
        ApplyPatch(typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.InnerPrefix_TripleUnderscoreField_OuterClassInstance_Primitive_ReadByValue));
        var outer = new ClassMethodTargets { foo = 42 };

        outer.CallInnerWithoutField(new InstanceMethodTargetsWithoutFields());

        Assert.That(FieldBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfix_TripleUnderscoreField_InnerClassInstance_Primitive_ReadByValue_WhenBothScopesMatch()
    {
        FieldBindingPatches.observed = 0;
        ApplyPatch(typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.InnerPostfix_TripleUnderscoreField_InnerClassInstance_Primitive_ReadByValue_WhenBothScopesMatch));
        var outer = new ClassMethodTargets { foo = 1 };
        var inner = new InnerInstanceMethodTargets { foo = 42 };

        outer.CallInnerWithField(inner);

        Assert.That(FieldBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_TripleUnderscoreField_OuterClassInstance_Primitive_WriteByReference()
    {
        ApplyPatch(typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.InnerPrefix_TripleUnderscoreField_OuterClassInstance_Primitive_WriteByReference));
        var outer = new ClassMethodTargets { foo = 1 };

        outer.CallInnerWithoutField(new InstanceMethodTargetsWithoutFields());

        Assert.That(outer.foo, Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfix_TripleUnderscoreField_InnerClassInstance_Primitive_WriteByReference()
    {
        ApplyPatch(typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.InnerPostfix_TripleUnderscoreField_InnerClassInstance_Primitive_WriteByReference));
        var outer = new ClassMethodTargets { foo = 1 };
        var inner = new InnerInstanceMethodTargets { foo = 1 };

        outer.CallInnerWithField(inner);

        Assert.That(inner.foo, Is.EqualTo(42));
        Assert.That(outer.foo, Is.EqualTo(1));
    }

    [Test]
    public void InnerPrefix_TripleUnderscoreField_OuterStructInstance_Primitive_ReadByValue()
    {
        FieldBindingPatches.observed = 0;
        ApplyPatch(typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.InnerPrefix_TripleUnderscoreField_OuterStructInstance_Primitive_ReadByValue));
        var outer = new StructMethodTargets { foo = 42 };

        outer.CallInnerWithoutField(new InstanceMethodTargetsWithoutFields());

        Assert.That(FieldBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfix_TripleUnderscoreField_InnerStructInstance_Primitive_ReadByValue_WhenBothScopesMatch()
    {
        FieldBindingPatches.observed = 0;
        ApplyPatch(typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.InnerPostfix_TripleUnderscoreField_InnerStructInstance_Primitive_ReadByValue_WhenBothScopesMatch));
        var outer = new StructMethodTargets { foo = 1 };
        var inner = new InnerStructMethodTargets { foo = 42 };

        outer.CallInnerWithField(ref inner);

        Assert.That(FieldBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_TripleUnderscoreField_OuterStructInstance_Primitive_WriteByReference()
    {
        ApplyPatch(typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.InnerPrefix_TripleUnderscoreField_OuterStructInstance_Primitive_WriteByReference));
        var outer = new StructMethodTargets { foo = 1 };

        outer.CallInnerWithoutField(new InstanceMethodTargetsWithoutFields());

        Assert.That(outer.foo, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_TripleUnderscoreField_InnerStructInstance_Primitive_WriteByReference()
    {
        InnerStructMethodTargets.FieldObserved = 0;
        ApplyPatch(typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.InnerPrefix_TripleUnderscoreField_InnerStructInstance_Primitive_WriteByReference));
        var outer = new StructMethodTargets { foo = 1 };
        var inner = new InnerStructMethodTargets { foo = 1 };

        outer.CallInnerWithField(ref inner);

        Assert.That(InnerStructMethodTargets.FieldObserved, Is.EqualTo(42));
        Assert.That(inner.foo, Is.EqualTo(42));
        Assert.That(outer.foo, Is.EqualTo(1));
    }

    [Test]
    public void InnerPrefix_TripleUnderscoreField_InnerStructPassedByValue_Primitive_WriteByReference()
    {
        InnerStructMethodTargets.FieldObserved = 0;
        ApplyPatch(typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.InnerPrefix_TripleUnderscoreField_InnerStructPassedByValue_Primitive_WriteByReference));
        var outer = new StructMethodTargets { foo = 1 };
        var inner = new InnerStructMethodTargets { foo = 1 };

        outer.CallInnerWithFieldByValue(inner);

        Assert.That(InnerStructMethodTargets.FieldObserved, Is.EqualTo(42));
        Assert.That(inner.foo, Is.EqualTo(1));
        Assert.That(outer.foo, Is.EqualTo(1));
    }

    [Test]
    public void Prefix_FieldAttribute_Primitive_WriteByReference()
    {
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_FieldAttribute_Primitive_WriteByReference));
        var target = new ClassMethodTargets { foo = 1 };

        target.Void();

        Assert.That(target.foo, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_FieldAttribute_Primitive_ReadByValue()
    {
        FieldBindingPatches.observed = 0;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_FieldAttribute_Primitive_ReadByValue));
        var target = new ClassMethodTargets { primitiveField = 42 };

        target.Void();

        Assert.That(FieldBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_FieldAttribute_ReferenceType_ReadByValue()
    {
        FieldBindingPatches.referenceObserved = null;
        var field = new BindingReference { Value = 42 };
        var target = new ClassMethodTargets { referenceField = field };
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_FieldAttribute_ReferenceType_ReadByValue));

        target.Void();

        Assert.That(FieldBindingPatches.referenceObserved, Is.SameAs(field));
    }

    [Test]
    public void Prefix_FieldAttribute_ReferenceType_WriteByReference()
    {
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_FieldAttribute_ReferenceType_WriteByReference));
        var target = new ClassMethodTargets { referenceField = new BindingReference { Value = 1 } };

        target.Void();

        Assert.That(target.referenceField.Value, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_FieldAttribute_Struct_ReadByValue()
    {
        FieldBindingPatches.structObserved = default;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_FieldAttribute_Struct_ReadByValue));
        var target = new ClassMethodTargets { structField = new BindingStruct { Value = 42 } };

        target.Void();

        Assert.That(FieldBindingPatches.structObserved.Value, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_FieldAttribute_Struct_WriteByReference()
    {
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_FieldAttribute_Struct_WriteByReference));
        var target = new ClassMethodTargets { structField = new BindingStruct { Value = 1 } };

        target.Void();

        Assert.That(target.structField.Value, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_FieldAttribute_OuterScope_Primitive_ReadByValue()
    {
        FieldBindingPatches.observed = 0;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.InnerPrefix_FieldAttribute_OuterScope_Primitive_ReadByValue));
        var outer = new ClassMethodTargets { foo = 1 };
        var inner = new InnerInstanceMethodTargets { foo = 42 };

        outer.CallInnerWithField(inner);

        Assert.That(FieldBindingPatches.observed, Is.EqualTo(1));
    }

    [Test]
    public void InnerPrefix_FieldAttribute_InnerScope_Primitive_ReadByValue()
    {
        FieldBindingPatches.observed = 0;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.InnerPrefix_FieldAttribute_InnerScope_Primitive_ReadByValue));
        var outer = new ClassMethodTargets { foo = 1 };
        var inner = new InnerInstanceMethodTargets { foo = 42 };

        outer.CallInnerWithField(inner);

        Assert.That(FieldBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_FieldAttribute_OuterScope_AutoPropertyBackingField_ReadByValue()
    {
        FieldBindingPatches.observed = 0;
        var outer = new ClassMethodTargets { AutoProperty = 42 };
        var inner = new InnerInstanceMethodTargets { AutoProperty = 1 };
        ApplyPatch(typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.InnerPrefix_FieldAttribute_OuterScope_AutoPropertyBackingField_ReadByValue));

        outer.CallInnerWithField(inner);

        Assert.That(FieldBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_FieldAttribute_OuterScope_AutoPropertyBackingField_ReadByReference()
    {
        FieldBindingPatches.observed = 0;
        var outer = new ClassMethodTargets { AutoProperty = 42 };
        var inner = new InnerInstanceMethodTargets { AutoProperty = 1 };
        ApplyPatch(typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.InnerPrefix_FieldAttribute_OuterScope_AutoPropertyBackingField_ReadByReference));

        outer.CallInnerWithField(inner);

        Assert.That(FieldBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_FieldAttribute_OuterScope_AutoPropertyBackingField_WriteByReference()
    {
        var outer = new ClassMethodTargets { AutoProperty = 1 };
        var inner = new InnerInstanceMethodTargets { AutoProperty = 2 };
        ApplyPatch(typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.InnerPrefix_FieldAttribute_OuterScope_AutoPropertyBackingField_WriteByReference));

        outer.CallInnerWithField(inner);

        Assert.That(outer.AutoProperty, Is.EqualTo(42));
        Assert.That(inner.AutoProperty, Is.EqualTo(2));
    }

    [Test]
    public void InnerPrefix_FieldAttribute_InnerScope_AutoPropertyBackingField_ReadByValue()
    {
        FieldBindingPatches.observed = 0;
        var outer = new ClassMethodTargets { AutoProperty = 1 };
        var inner = new InnerInstanceMethodTargets { AutoProperty = 42 };
        ApplyPatch(typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.InnerPrefix_FieldAttribute_InnerScope_AutoPropertyBackingField_ReadByValue));

        outer.CallInnerWithField(inner);

        Assert.That(FieldBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_FieldAttribute_InnerScope_AutoPropertyBackingField_ReadByReference()
    {
        FieldBindingPatches.observed = 0;
        var outer = new ClassMethodTargets { AutoProperty = 1 };
        var inner = new InnerInstanceMethodTargets { AutoProperty = 42 };
        ApplyPatch(typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.InnerPrefix_FieldAttribute_InnerScope_AutoPropertyBackingField_ReadByReference));

        outer.CallInnerWithField(inner);

        Assert.That(FieldBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_FieldAttribute_InnerScope_AutoPropertyBackingField_WriteByReference()
    {
        var outer = new ClassMethodTargets { AutoProperty = 1 };
        var inner = new InnerInstanceMethodTargets { AutoProperty = 2 };
        ApplyPatch(typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.InnerPrefix_FieldAttribute_InnerScope_AutoPropertyBackingField_WriteByReference));

        outer.CallInnerWithField(inner);

        Assert.That(inner.AutoProperty, Is.EqualTo(42));
        Assert.That(outer.AutoProperty, Is.EqualTo(1));
    }

    [Test]
    public void Prefix_TripleUnderscoreField_Primitive_ReadByReference()
    {
        FieldBindingPatches.observed = 0;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_TripleUnderscoreField_Primitive_ReadByReference));
        var target = new ClassMethodTargets { primitiveField = 42 };

        target.Void();

        Assert.That(FieldBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_TripleUnderscoreField_ReferenceType_ReadByReference()
    {
        FieldBindingPatches.referenceObserved = null;
        var field = new BindingReference { Value = 42 };
        var target = new ClassMethodTargets { referenceField = field };
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_TripleUnderscoreField_ReferenceType_ReadByReference));

        target.Void();

        Assert.That(FieldBindingPatches.referenceObserved, Is.SameAs(field));
    }

    [Test]
    public void Prefix_TripleUnderscoreField_Struct_ReadByReference()
    {
        FieldBindingPatches.structObserved = default;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_TripleUnderscoreField_Struct_ReadByReference));
        var target = new ClassMethodTargets { structField = new BindingStruct { Value = 42 } };

        target.Void();

        Assert.That(FieldBindingPatches.structObserved.Value, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_TripleUnderscoreField_OuterClassInstance_Primitive_ReadByReference()
    {
        FieldBindingPatches.observed = 0;
        ApplyPatch(typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.InnerPrefix_TripleUnderscoreField_OuterClassInstance_Primitive_ReadByReference));
        var outer = new ClassMethodTargets { foo = 42 };

        outer.CallInnerWithoutField(new InstanceMethodTargetsWithoutFields());

        Assert.That(FieldBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfix_TripleUnderscoreField_InnerClassInstance_Primitive_ReadByReference()
    {
        FieldBindingPatches.observed = 0;
        ApplyPatch(typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.InnerPostfix_TripleUnderscoreField_InnerClassInstance_Primitive_ReadByReference));
        var outer = new ClassMethodTargets { foo = 1 };
        var inner = new InnerInstanceMethodTargets { foo = 42 };

        outer.CallInnerWithField(inner);

        Assert.That(FieldBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_TripleUnderscoreField_OuterStructInstance_Primitive_ReadByReference()
    {
        FieldBindingPatches.observed = 0;
        ApplyPatch(typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.InnerPrefix_TripleUnderscoreField_OuterStructInstance_Primitive_ReadByReference));
        var outer = new StructMethodTargets { foo = 42 };

        outer.CallInnerWithoutField(new InstanceMethodTargetsWithoutFields());

        Assert.That(FieldBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_TripleUnderscoreField_InnerStructInstance_Primitive_ReadByReference()
    {
        FieldBindingPatches.observed = 0;
        ApplyPatch(typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.InnerPrefix_TripleUnderscoreField_InnerStructInstance_Primitive_ReadByReference));
        var outer = new StructMethodTargets { foo = 1 };
        var inner = new InnerStructMethodTargets { foo = 42 };

        outer.CallInnerWithField(ref inner);

        Assert.That(FieldBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_TripleUnderscoreField_InnerStructPassedByValue_Primitive_ReadByReference()
    {
        FieldBindingPatches.observed = 0;
        ApplyPatch(typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.InnerPrefix_TripleUnderscoreField_InnerStructPassedByValue_Primitive_ReadByReference));
        var outer = new StructMethodTargets { foo = 1 };
        var inner = new InnerStructMethodTargets { foo = 42 };

        outer.CallInnerWithFieldByValue(inner);

        Assert.That(FieldBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_FieldAttribute_Primitive_ReadByReference()
    {
        FieldBindingPatches.observed = 0;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_FieldAttribute_Primitive_ReadByReference));
        var target = new ClassMethodTargets { foo = 42 };

        target.Void();

        Assert.That(FieldBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_FieldAttribute_NullName_UsesParameterName()
    {
        FieldBindingPatches.observed = 0;
        ApplyPatch(
            typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.Prefix_FieldAttribute_NullName_UsesParameterName));
        var target = new ClassMethodTargets { primitiveField = 42 };

        target.Void();

        Assert.That(FieldBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_FieldAttribute_ReferenceType_ReadByReference()
    {
        FieldBindingPatches.referenceObserved = null;
        var field = new BindingReference { Value = 42 };
        var target = new ClassMethodTargets { referenceField = field };
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_FieldAttribute_ReferenceType_ReadByReference));

        target.Void();

        Assert.That(FieldBindingPatches.referenceObserved, Is.SameAs(field));
    }

    [Test]
    public void Prefix_FieldAttribute_Struct_ReadByReference()
    {
        FieldBindingPatches.structObserved = default;
        ApplyPatch(typeof(FieldBindingPatches), nameof(FieldBindingPatches.Prefix_FieldAttribute_Struct_ReadByReference));
        var target = new ClassMethodTargets { structField = new BindingStruct { Value = 42 } };

        target.Void();

        Assert.That(FieldBindingPatches.structObserved.Value, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_FieldAttribute_Struct_AsObjectByWriteableReference_Rejected()
    {
        Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.Prefix_FieldAttribute_Struct_AsObjectByWriteableReference_Rejected)));
    }

    [Test]
    public void Prefix_FieldAttribute_Struct_AsObjectByReadonlyReference_Rejected()
    {
        Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.Prefix_FieldAttribute_Struct_AsObjectByReadonlyReference_Rejected)));
    }

    [Test]
    public void Prefix_FieldAttribute_ReferenceType_AsObjectByWriteableReference_Rejected()
    {
        Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.Prefix_FieldAttribute_ReferenceType_AsObjectByWriteableReference_Rejected)));
    }

    [Test]
    public void Prefix_FieldAttribute_ReferenceType_AsObjectByReadonlyReference()
    {
        FieldBindingPatches.referenceObserved = null;
        var field = new BindingReference { Value = 42 };
        var target = new ClassMethodTargets { referenceField = field };
        ApplyPatch(
            typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.Prefix_FieldAttribute_ReferenceType_AsObjectByReadonlyReference));

        target.Void();

        Assert.That(FieldBindingPatches.referenceObserved, Is.SameAs(field));
    }

    [Test]
    public void Prefix_FieldAttribute_ReferenceType_AsStringByOutReference()
    {
        ApplyPatch(
            typeof(FieldBindingPatches),
            nameof(FieldBindingPatches.Prefix_FieldAttribute_ReferenceType_AsStringByOutReference));
        var target = new ClassMethodTargets { objectField = new object() };

        target.Void();

        Assert.That(target.objectField, Is.EqualTo("patched"));
    }
}
