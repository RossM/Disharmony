namespace Disharmony.Tests.EndToEnd.ParameterBinding;

public static partial class CapturedVariableBindingPatches
{
    public static int observed;
    public static BindingReference? referenceObserved;
    public static BindingStruct structObserved;

    [Prefix]
    [Target(typeof(LocalFunctionTargets), "CapturedVariableMethod.LocalMethod")]
    public static void Prefix_LocalFunctionCapturedVariable_Primitive_ReadByValue(int captured) => observed = captured;

    [Prefix]
    [Target(typeof(LocalFunctionTargets), "CapturedReferenceVariableMethod.LocalMethod")]
    public static void Prefix_LocalFunctionCapturedVariable_ReferenceType_ReadByValue(BindingReference captured) =>
        referenceObserved = captured;

    [Prefix]
    [Target(typeof(LocalFunctionTargets), "CapturedStructVariableMethod.LocalMethod")]
    public static void Prefix_LocalFunctionCapturedVariable_Struct_ReadByValue(BindingStruct captured) => structObserved = captured;

    [Postfix]
    [Target(typeof(LocalFunctionTargets), "CapturedVariableMethod.LocalMethod")]
    public static void Postfix_LocalFunctionCapturedVariable_Primitive_ReadByValue(int captured) => observed = captured;

    [Prefix] [Inner(typeof(LocalFunctionTargets), "CapturedVariableMethod.LocalMethod")]
    [Target(typeof(LocalFunctionTargets), nameof(LocalFunctionTargets.CapturedVariableMethod))]
    public static void InnerPrefix_LocalFunctionCallCapturedVariable_Primitive_ReadByValue(int captured) => observed = captured;

    [Prefix] [Inner(typeof(LocalFunctionTargets), "CapturedReferenceVariableMethod.LocalMethod")]
    [Target(typeof(LocalFunctionTargets), nameof(LocalFunctionTargets.CapturedReferenceVariableMethod))]
    public static void InnerPrefix_LocalFunctionCallCapturedVariable_ReferenceType_ReadByValue(BindingReference captured) =>
        referenceObserved = captured;

    [Prefix] [Inner(typeof(LocalFunctionTargets), "CapturedStructVariableMethod.LocalMethod")]
    [Target(typeof(LocalFunctionTargets), nameof(LocalFunctionTargets.CapturedStructVariableMethod))]
    public static void InnerPrefix_LocalFunctionCallCapturedVariable_Struct_ReadByValue(BindingStruct captured) =>
        structObserved = captured;

    [Postfix] [Inner(typeof(LocalFunctionTargets), "CapturedVariableMethod.LocalMethod")]
    [Target(typeof(LocalFunctionTargets), nameof(LocalFunctionTargets.CapturedVariableMethod))]
    public static void InnerPostfix_LocalFunctionCallCapturedVariable_Primitive_ReadByValue(int captured) => observed = captured;

    [Prefix]
    [Target(typeof(LocalFunctionTargets), "CapturedVariableMethod.LocalMethod")]
    public static void Prefix_LocalFunctionCapturedVariable_Primitive_WriteByReference(ref int captured) => captured = 42;

    [Prefix]
    [Target(typeof(LocalFunctionTargets), "CapturedReferenceVariableMethod.LocalMethod")]
    public static void Prefix_LocalFunctionCapturedVariable_ReferenceType_WriteByReference(ref BindingReference captured) =>
        captured = new BindingReference { Value = 42 };

    [Prefix]
    [Target(typeof(LocalFunctionTargets), "CapturedStructVariableMethod.LocalMethod")]
    public static void Prefix_LocalFunctionCapturedVariable_Struct_WriteByReference(ref BindingStruct captured) =>
        captured = new BindingStruct { Value = 42 };

    [Postfix]
    [Target(typeof(LocalFunctionTargets), "CapturedVariableMethod.LocalMethod")]
    public static void Postfix_LocalFunctionCapturedVariable_Primitive_WriteByReference(ref int captured) => captured = 42;

    [Prefix] [Inner(typeof(LocalFunctionTargets), "CapturedVariableMethod.LocalMethod")]
    [Target(typeof(LocalFunctionTargets), nameof(LocalFunctionTargets.CapturedVariableMethod))]
    public static void InnerPrefix_LocalFunctionCallCapturedVariable_Primitive_WriteByReference(ref int captured) => captured = 42;

    [Prefix] [Inner(typeof(LocalFunctionTargets), "CapturedReferenceVariableMethod.LocalMethod")]
    [Target(typeof(LocalFunctionTargets), nameof(LocalFunctionTargets.CapturedReferenceVariableMethod))]
    public static void InnerPrefix_LocalFunctionCallCapturedVariable_ReferenceType_WriteByReference(ref BindingReference captured) =>
        captured = new BindingReference { Value = 42 };

    [Prefix] [Inner(typeof(LocalFunctionTargets), "CapturedStructVariableMethod.LocalMethod")]
    [Target(typeof(LocalFunctionTargets), nameof(LocalFunctionTargets.CapturedStructVariableMethod))]
    public static void InnerPrefix_LocalFunctionCallCapturedVariable_Struct_WriteByReference(ref BindingStruct captured) =>
        captured = new BindingStruct { Value = 42 };

    [Postfix] [Inner(typeof(LocalFunctionTargets), "CapturedVariableMethod.LocalMethod")]
    [Target(typeof(LocalFunctionTargets), nameof(LocalFunctionTargets.CapturedVariableMethod))]
    public static void InnerPostfix_LocalFunctionCallCapturedVariable_Primitive_WriteByReference(ref int captured) => captured = 42;
}

[TestFixture]
public sealed partial class CapturedVariableBindingTests : PatchTestBase
{
    [Test]
    public void Prefix_LocalFunctionCapturedVariable_Primitive_ReadByValue()
    {
        CapturedVariableBindingPatches.observed = 0;
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.Prefix_LocalFunctionCapturedVariable_Primitive_ReadByValue));

        int result = LocalFunctionTargets.CapturedVariableMethod(42);

        Assert.That(result, Is.EqualTo(42));
        Assert.That(CapturedVariableBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_LocalFunctionCapturedVariable_ReferenceType_ReadByValue()
    {
        CapturedVariableBindingPatches.referenceObserved = null;
        var value = new BindingReference { Value = 42 };
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.Prefix_LocalFunctionCapturedVariable_ReferenceType_ReadByValue));

        LocalFunctionTargets.CapturedReferenceVariableMethod(value);

        Assert.That(CapturedVariableBindingPatches.referenceObserved, Is.SameAs(value));
    }

    [Test]
    public void Prefix_LocalFunctionCapturedVariable_Struct_ReadByValue()
    {
        CapturedVariableBindingPatches.structObserved = default;
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.Prefix_LocalFunctionCapturedVariable_Struct_ReadByValue));

        LocalFunctionTargets.CapturedStructVariableMethod(new BindingStruct { Value = 42 });

        Assert.That(CapturedVariableBindingPatches.structObserved.Value, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_LocalFunctionCapturedVariable_Primitive_ReadByValue()
    {
        CapturedVariableBindingPatches.observed = 0;
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.Postfix_LocalFunctionCapturedVariable_Primitive_ReadByValue));

        int result = LocalFunctionTargets.CapturedVariableMethod(42);

        Assert.That(result, Is.EqualTo(42));
        Assert.That(CapturedVariableBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_LocalFunctionCallCapturedVariable_Primitive_ReadByValue()
    {
        CapturedVariableBindingPatches.observed = 0;
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.InnerPrefix_LocalFunctionCallCapturedVariable_Primitive_ReadByValue));

        int result = LocalFunctionTargets.CapturedVariableMethod(42);

        Assert.That(result, Is.EqualTo(42));
        Assert.That(CapturedVariableBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_LocalFunctionCallCapturedVariable_ReferenceType_ReadByValue()
    {
        CapturedVariableBindingPatches.referenceObserved = null;
        var value = new BindingReference { Value = 42 };
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.InnerPrefix_LocalFunctionCallCapturedVariable_ReferenceType_ReadByValue));

        LocalFunctionTargets.CapturedReferenceVariableMethod(value);

        Assert.That(CapturedVariableBindingPatches.referenceObserved, Is.SameAs(value));
    }

    [Test]
    public void InnerPrefix_LocalFunctionCallCapturedVariable_Struct_ReadByValue()
    {
        CapturedVariableBindingPatches.structObserved = default;
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.InnerPrefix_LocalFunctionCallCapturedVariable_Struct_ReadByValue));

        LocalFunctionTargets.CapturedStructVariableMethod(new BindingStruct { Value = 42 });

        Assert.That(CapturedVariableBindingPatches.structObserved.Value, Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfix_LocalFunctionCallCapturedVariable_Primitive_ReadByValue()
    {
        CapturedVariableBindingPatches.observed = 0;
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.InnerPostfix_LocalFunctionCallCapturedVariable_Primitive_ReadByValue));

        int result = LocalFunctionTargets.CapturedVariableMethod(42);

        Assert.That(result, Is.EqualTo(42));
        Assert.That(CapturedVariableBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_LocalFunctionCapturedVariable_Primitive_WriteByReference()
    {
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.Prefix_LocalFunctionCapturedVariable_Primitive_WriteByReference));

        int result = LocalFunctionTargets.CapturedVariableMethod(1);

        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_LocalFunctionCapturedVariable_ReferenceType_WriteByReference()
    {
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.Prefix_LocalFunctionCapturedVariable_ReferenceType_WriteByReference));

        BindingReference result = LocalFunctionTargets.CapturedReferenceVariableMethod(new BindingReference { Value = 1 });

        Assert.That(result.Value, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_LocalFunctionCapturedVariable_Struct_WriteByReference()
    {
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.Prefix_LocalFunctionCapturedVariable_Struct_WriteByReference));

        BindingStruct result = LocalFunctionTargets.CapturedStructVariableMethod(new BindingStruct { Value = 1 });

        Assert.That(result.Value, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_LocalFunctionCapturedVariable_Primitive_WriteByReference()
    {
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.Postfix_LocalFunctionCapturedVariable_Primitive_WriteByReference));

        int result = LocalFunctionTargets.CapturedVariableMethod(1);

        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_LocalFunctionCallCapturedVariable_Primitive_WriteByReference()
    {
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.InnerPrefix_LocalFunctionCallCapturedVariable_Primitive_WriteByReference));

        int result = LocalFunctionTargets.CapturedVariableMethod(1);

        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_LocalFunctionCallCapturedVariable_ReferenceType_WriteByReference()
    {
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.InnerPrefix_LocalFunctionCallCapturedVariable_ReferenceType_WriteByReference));

        BindingReference result = LocalFunctionTargets.CapturedReferenceVariableMethod(new BindingReference { Value = 1 });

        Assert.That(result.Value, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_LocalFunctionCallCapturedVariable_Struct_WriteByReference()
    {
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.InnerPrefix_LocalFunctionCallCapturedVariable_Struct_WriteByReference));

        BindingStruct result = LocalFunctionTargets.CapturedStructVariableMethod(new BindingStruct { Value = 1 });

        Assert.That(result.Value, Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfix_LocalFunctionCallCapturedVariable_Primitive_WriteByReference()
    {
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.InnerPostfix_LocalFunctionCallCapturedVariable_Primitive_WriteByReference));

        int result = LocalFunctionTargets.CapturedVariableMethod(1);

        Assert.That(result, Is.EqualTo(42));
    }
}

public static partial class CapturedVariableBindingPatches
{
    [Prefix]
    [Target(typeof(LocalFunctionTargets), "CapturedReferenceVariableMethod.LocalMethod")]
    public static void Prefix_LocalFunctionCapturedVariable_ReferenceType_AsObjectByWriteableReference_Rejected(
        ref object captured) { }

    [Prefix]
    [Target(typeof(LocalFunctionTargets), "CapturedReferenceVariableMethod.LocalMethod")]
    public static void Prefix_LocalFunctionCapturedVariable_ReferenceType_AsObjectByReadonlyReference(
        in object captured) => referenceObserved = (BindingReference)captured;

    [Prefix]
    [Target(typeof(LocalFunctionTargets), "CapturedObjectVariableMethod.LocalMethod")]
    public static void Prefix_LocalFunctionCapturedVariable_ReferenceType_AsStringByOutReference(
        out string captured) => captured = "patched";

    [Prefix]
    [Target(typeof(LocalFunctionTargets), "CapturedVariableMethod.LocalMethod")]
    public static void Prefix_LocalFunctionCapturedVariable_Primitive_ReadByReference(ref int captured) => observed = captured;

    [Prefix]
    [Target(typeof(LocalFunctionTargets), "CapturedReferenceVariableMethod.LocalMethod")]
    public static void Prefix_LocalFunctionCapturedVariable_ReferenceType_ReadByReference(
        ref BindingReference captured) => referenceObserved = captured;

    [Prefix]
    [Target(typeof(LocalFunctionTargets), "CapturedStructVariableMethod.LocalMethod")]
    public static void Prefix_LocalFunctionCapturedVariable_Struct_ReadByReference(ref BindingStruct captured) =>
        structObserved = captured;

    [Postfix]
    [Target(typeof(LocalFunctionTargets), "CapturedVariableMethod.LocalMethod")]
    public static void Postfix_LocalFunctionCapturedVariable_Primitive_ReadByReference(ref int captured) => observed = captured;

    [Prefix] [Inner(typeof(LocalFunctionTargets), "CapturedVariableMethod.LocalMethod")]
    [Target(typeof(LocalFunctionTargets), nameof(LocalFunctionTargets.CapturedVariableMethod))]
    public static void InnerPrefix_LocalFunctionCallCapturedVariable_Primitive_ReadByReference(ref int captured) =>
        observed = captured;

    [Prefix] [Inner(typeof(LocalFunctionTargets), "CapturedReferenceVariableMethod.LocalMethod")]
    [Target(typeof(LocalFunctionTargets), nameof(LocalFunctionTargets.CapturedReferenceVariableMethod))]
    public static void InnerPrefix_LocalFunctionCallCapturedVariable_ReferenceType_ReadByReference(
        ref BindingReference captured) => referenceObserved = captured;

    [Prefix] [Inner(typeof(LocalFunctionTargets), "CapturedStructVariableMethod.LocalMethod")]
    [Target(typeof(LocalFunctionTargets), nameof(LocalFunctionTargets.CapturedStructVariableMethod))]
    public static void InnerPrefix_LocalFunctionCallCapturedVariable_Struct_ReadByReference(ref BindingStruct captured) =>
        structObserved = captured;

    [Postfix] [Inner(typeof(LocalFunctionTargets), "CapturedVariableMethod.LocalMethod")]
    [Target(typeof(LocalFunctionTargets), nameof(LocalFunctionTargets.CapturedVariableMethod))]
    public static void InnerPostfix_LocalFunctionCallCapturedVariable_Primitive_ReadByReference(ref int captured) =>
        observed = captured;
}

[TestFixture]
public sealed partial class CapturedVariableBindingTests
{
    [Test]
    public void Prefix_LocalFunctionCapturedVariable_ReferenceType_AsObjectByWriteableReference_Rejected()
    {
        Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches
                .Prefix_LocalFunctionCapturedVariable_ReferenceType_AsObjectByWriteableReference_Rejected)));
    }

    [Test]
    public void Prefix_LocalFunctionCapturedVariable_ReferenceType_AsObjectByReadonlyReference()
    {
        CapturedVariableBindingPatches.referenceObserved = null;
        var value = new BindingReference { Value = 42 };
        ApplyPatch(
            typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches
                .Prefix_LocalFunctionCapturedVariable_ReferenceType_AsObjectByReadonlyReference));

        LocalFunctionTargets.CapturedReferenceVariableMethod(value);

        Assert.That(CapturedVariableBindingPatches.referenceObserved, Is.SameAs(value));
    }

    [Test]
    public void Prefix_LocalFunctionCapturedVariable_ReferenceType_AsStringByOutReference()
    {
        ApplyPatch(
            typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches
                .Prefix_LocalFunctionCapturedVariable_ReferenceType_AsStringByOutReference));

        object result = LocalFunctionTargets.CapturedObjectVariableMethod(new object());

        Assert.That(result, Is.EqualTo("patched"));
    }

    [Test]
    public void Prefix_LocalFunctionCapturedVariable_Primitive_ReadByReference()
    {
        CapturedVariableBindingPatches.observed = 0;
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.Prefix_LocalFunctionCapturedVariable_Primitive_ReadByReference));
        LocalFunctionTargets.CapturedVariableMethod(42);
        Assert.That(CapturedVariableBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_LocalFunctionCapturedVariable_ReferenceType_ReadByReference()
    {
        CapturedVariableBindingPatches.referenceObserved = null;
        var value = new BindingReference { Value = 42 };
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.Prefix_LocalFunctionCapturedVariable_ReferenceType_ReadByReference));
        LocalFunctionTargets.CapturedReferenceVariableMethod(value);
        Assert.That(CapturedVariableBindingPatches.referenceObserved, Is.SameAs(value));
    }

    [Test]
    public void Prefix_LocalFunctionCapturedVariable_Struct_ReadByReference()
    {
        CapturedVariableBindingPatches.structObserved = default;
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.Prefix_LocalFunctionCapturedVariable_Struct_ReadByReference));
        LocalFunctionTargets.CapturedStructVariableMethod(new BindingStruct { Value = 42 });
        Assert.That(CapturedVariableBindingPatches.structObserved.Value, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_LocalFunctionCapturedVariable_Primitive_ReadByReference()
    {
        CapturedVariableBindingPatches.observed = 0;
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.Postfix_LocalFunctionCapturedVariable_Primitive_ReadByReference));
        LocalFunctionTargets.CapturedVariableMethod(42);
        Assert.That(CapturedVariableBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_LocalFunctionCallCapturedVariable_Primitive_ReadByReference()
    {
        CapturedVariableBindingPatches.observed = 0;
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.InnerPrefix_LocalFunctionCallCapturedVariable_Primitive_ReadByReference));
        LocalFunctionTargets.CapturedVariableMethod(42);
        Assert.That(CapturedVariableBindingPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_LocalFunctionCallCapturedVariable_ReferenceType_ReadByReference()
    {
        CapturedVariableBindingPatches.referenceObserved = null;
        var value = new BindingReference { Value = 42 };
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.InnerPrefix_LocalFunctionCallCapturedVariable_ReferenceType_ReadByReference));
        LocalFunctionTargets.CapturedReferenceVariableMethod(value);
        Assert.That(CapturedVariableBindingPatches.referenceObserved, Is.SameAs(value));
    }

    [Test]
    public void InnerPrefix_LocalFunctionCallCapturedVariable_Struct_ReadByReference()
    {
        CapturedVariableBindingPatches.structObserved = default;
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.InnerPrefix_LocalFunctionCallCapturedVariable_Struct_ReadByReference));
        LocalFunctionTargets.CapturedStructVariableMethod(new BindingStruct { Value = 42 });
        Assert.That(CapturedVariableBindingPatches.structObserved.Value, Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfix_LocalFunctionCallCapturedVariable_Primitive_ReadByReference()
    {
        CapturedVariableBindingPatches.observed = 0;
        ApplyPatch(typeof(CapturedVariableBindingPatches),
            nameof(CapturedVariableBindingPatches.InnerPostfix_LocalFunctionCallCapturedVariable_Primitive_ReadByReference));
        LocalFunctionTargets.CapturedVariableMethod(42);
        Assert.That(CapturedVariableBindingPatches.observed, Is.EqualTo(42));
    }
}
