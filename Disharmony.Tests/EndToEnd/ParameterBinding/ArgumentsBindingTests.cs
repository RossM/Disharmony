namespace Disharmony.Tests.EndToEnd.ParameterBinding;

public static class ArgumentsBindingPatches
{
    public static object[]? Observed;
    public static object[]? OuterObserved;

    [Prefix]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.Primitive_ByValue))]
    public static void Prefix_Arguments_Primitive_ByValue_ReadsArgument([Arguments] object[] __args) => Observed = __args;

    [Prefix]
    [Inner(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.Primitive_ByValue))]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.CallPrimitive_ByValue))]
    public static void InnerPrefix_Arguments_Primitive_ByValue_ReadsArgument(object[] __args) => Observed = __args;

    [Prefix]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.Primitive_ByValue))]
    public static void Prefix_Arguments_Primitive_ByValue_ArrayWriteDoesNotChangeArgument([Arguments] object[] __args)
    {
        __args[0] = 7;
    }

    [Prefix]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.Primitive_ByReference))]
    public static void Prefix_Arguments_Primitive_ByReference_ReadsArgument([Arguments] object[] __args) => Observed = __args;

    [Prefix]
    [Inner(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.Primitive_ByReference))]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.CallPrimitive_ByReference))]
    public static void InnerPrefix_Arguments_Primitive_ByReference_ReadsArgument(object[] __args) => Observed = __args;

    [Prefix]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.Primitive_ByReference))]
    public static void Prefix_Arguments_Primitive_ByReference_ArrayWriteDoesNotChangeArgument([Arguments] object[] __args)
    {
        __args[0] = 7;
    }

    [Prefix]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.ReferenceType_ByValue))]
    public static void Prefix_Arguments_ReferenceType_ByValue_ReadsArgument([Arguments] object[] __args) => Observed = __args;

    [Prefix]
    [Inner(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.ReferenceType_ByValue))]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.CallReferenceType_ByValue))]
    public static void InnerPrefix_Arguments_ReferenceType_ByValue_ReadsArgument(object[] __args) => Observed = __args;

    [Prefix]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.ReferenceType_ByValue))]
    public static void Prefix_Arguments_ReferenceType_ByValue_ArrayWriteDoesNotChangeArgument([Arguments] object[] __args)
    {
        __args[0] = new BindingReference { Value = 7 };
    }

    [Prefix]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.ReferenceType_ByReference))]
    public static void Prefix_Arguments_ReferenceType_ByReference_ReadsArgument([Arguments] object[] __args) => Observed = __args;

    [Prefix]
    [Inner(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.ReferenceType_ByReference))]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.CallReferenceType_ByReference))]
    public static void InnerPrefix_Arguments_ReferenceType_ByReference_ReadsArgument(object[] __args) => Observed = __args;

    [Prefix]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.ReferenceType_ByReference))]
    public static void Prefix_Arguments_ReferenceType_ByReference_ArrayWriteDoesNotChangeArgument([Arguments] object[] __args)
    {
        __args[0] = new BindingReference { Value = 7 };
    }

    [Prefix]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.Struct_ByValue))]
    public static void Prefix_Arguments_Struct_ByValue_ReadsArgument([Arguments] object[] __args) => Observed = __args;

    [Prefix]
    [Inner(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.Struct_ByValue))]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.CallStruct_ByValue))]
    public static void InnerPrefix_Arguments_Struct_ByValue_ReadsArgument(object[] __args) => Observed = __args;

    [Prefix]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.Struct_ByValue))]
    public static void Prefix_Arguments_Struct_ByValue_ArrayWriteDoesNotChangeArgument([Arguments] object[] __args)
    {
        __args[0] = new BindingStruct { Value = 7 };
    }

    [Prefix]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.Struct_ByReference))]
    public static void Prefix_Arguments_Struct_ByReference_ReadsArgument([Arguments] object[] __args) => Observed = __args;

    [Prefix]
    [Inner(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.Struct_ByReference))]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.CallStruct_ByReference))]
    public static void InnerPrefix_Arguments_Struct_ByReference_ReadsArgument(object[] __args) => Observed = __args;

    [Prefix]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.Struct_ByReference))]
    public static void Prefix_Arguments_Struct_ByReference_ArrayWriteDoesNotChangeArgument([Arguments] object[] __args)
    {
        __args[0] = new BindingStruct { Value = 7 };
    }

    [Prefix]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.NumericByValue))]
    public static void Prefix_Arguments_NumericTypes_ByValue([Arguments] object[] arguments) => Observed = arguments;

    [Prefix]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.NumericByReference))]
    public static void Prefix_Arguments_NumericTypes_ByReference([Arguments] object[] arguments) => Observed = arguments;

    [Prefix]
    [Target(typeof(ArgumentsBindingInstanceTargets), nameof(ArgumentsBindingInstanceTargets.Values))]
    public static void Prefix_Arguments_InstanceMethod_ExcludesInstance([Arguments] object[] arguments) => Observed = arguments;

    [Prefix]
    [Target(typeof(ArgumentsBindingStructTargets), nameof(ArgumentsBindingStructTargets.Values))]
    public static void Prefix_Arguments_StructMethod_ExcludesInstance([Arguments] object[] arguments) => Observed = arguments;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Prefix_Arguments_StaticMethod_NoArguments([Arguments] object[] arguments) => Observed = arguments;

    [Postfix]
    [Target(typeof(ArgumentsBindingInstanceTargets), nameof(ArgumentsBindingInstanceTargets.Empty))]
    public static void Postfix_Arguments_InstanceMethod_NoArguments([Arguments] object[] arguments) => Observed = arguments;

    [Postfix]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.ReplaceReferences))]
    public static void Postfix_Arguments_RefParameters_ObservesUpdatedValues([Arguments] object[] arguments) => Observed = arguments;

    [Postfix]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.OutValues))]
    public static void Postfix_Arguments_OutParameters_ObservesAssignedValues([Arguments] object[] arguments) => Observed = arguments;

    [Prefix]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.InValues))]
    public static void Prefix_Arguments_InParameters_ReadsValues([Arguments] object[] arguments) => Observed = arguments;

    [Prefix]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.NullValues))]
    public static void Prefix_Arguments_NullReferenceAndNullable_ProducesNullElements([Arguments] object[] arguments) => Observed = arguments;

    [Prefix]
    [Inner(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.Primitive_ByValue))]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.CallWithOuterArguments))]
    public static void InnerPrefix_Arguments_Scopes_UseRespectiveArgumentLists(object[] __args, [Arguments(Scope.Outer)] object[] outer)
    {
        Observed = __args;
        OuterObserved = outer;
    }

    [Postfix]
    [Inner(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.Primitive_ByValue))]
    [Target(typeof(ArgumentsBindingTargets), nameof(ArgumentsBindingTargets.CallWithOuterArguments))]
    public static void InnerPostfix_Arguments_Scopes_UseRespectiveArgumentLists(object[] __args, [Arguments(Scope.Outer)] object[] outer)
    {
        Observed = __args;
        OuterObserved = outer;
    }

    [Prefix]
    [Target(typeof(ConstructorTargets), memberType: MemberType.Constructor, parameterTypes: [typeof(int)])]
    public static void Prefix_Arguments_Constructor_ExcludesInstance(object[] __args) => Observed = __args;

    [Prefix]
    [Inner(typeof(ConstructorTargets), memberType: MemberType.Constructor, parameterTypes: [typeof(int)])]
    [Target(typeof(ConstructorTargets), nameof(ConstructorTargets.Create), parameterTypes: [typeof(int)])]
    public static void InnerPrefix_Arguments_Constructor_ExcludesInstance(object[] __args) => Observed = __args;

}

[TestFixture]
public sealed class ArgumentsBindingTests : PatchTestBase
{
    [Test]
    public void Prefix_Arguments_Primitive_ByValue_ReadsArgument()
    {
        ArgumentsBindingPatches.Observed = null;
        int value = 42;
        var expected = value;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Prefix_Arguments_Primitive_ByValue_ReadsArgument));

        var result = ArgumentsBindingTargets.Primitive_ByValue(value);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(value, Is.EqualTo(expected));
            Assert.That(ArgumentsBindingPatches.Observed, Has.Length.EqualTo(1));
            Assert.That(ArgumentsBindingPatches.Observed![0], Is.EqualTo(expected));
            Assert.That(ArgumentsBindingPatches.Observed[0], Is.TypeOf<int>());
        });
    }

    [Test]
    public void InnerPrefix_Arguments_Primitive_ByValue_ReadsArgument()
    {
        ArgumentsBindingPatches.Observed = null;
        int value = 42;
        var expected = value;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.InnerPrefix_Arguments_Primitive_ByValue_ReadsArgument));

        var result = ArgumentsBindingTargets.CallPrimitive_ByValue(value);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(value, Is.EqualTo(expected));
            Assert.That(ArgumentsBindingPatches.Observed, Has.Length.EqualTo(1));
            Assert.That(ArgumentsBindingPatches.Observed![0], Is.EqualTo(expected));
            Assert.That(ArgumentsBindingPatches.Observed[0], Is.TypeOf<int>());
        });
    }

    [Test]
    public void Prefix_Arguments_Primitive_ByValue_ArrayWriteDoesNotChangeArgument()
    {
        ArgumentsBindingPatches.Observed = null;
        int value = 42;
        var expected = value;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Prefix_Arguments_Primitive_ByValue_ArrayWriteDoesNotChangeArgument));

        var result = ArgumentsBindingTargets.Primitive_ByValue(value);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(value, Is.EqualTo(expected));
        });
    }

    [Test]
    public void Prefix_Arguments_Primitive_ByReference_ReadsArgument()
    {
        ArgumentsBindingPatches.Observed = null;
        int value = 42;
        var expected = value;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Prefix_Arguments_Primitive_ByReference_ReadsArgument));

        var result = ArgumentsBindingTargets.Primitive_ByReference(ref value);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(value, Is.EqualTo(expected));
            Assert.That(ArgumentsBindingPatches.Observed, Has.Length.EqualTo(1));
            Assert.That(ArgumentsBindingPatches.Observed![0], Is.EqualTo(expected));
            Assert.That(ArgumentsBindingPatches.Observed[0], Is.TypeOf<int>());
        });
    }

    [Test]
    public void InnerPrefix_Arguments_Primitive_ByReference_ReadsArgument()
    {
        ArgumentsBindingPatches.Observed = null;
        int value = 42;
        var expected = value;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.InnerPrefix_Arguments_Primitive_ByReference_ReadsArgument));

        var result = ArgumentsBindingTargets.CallPrimitive_ByReference(ref value);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(value, Is.EqualTo(expected));
            Assert.That(ArgumentsBindingPatches.Observed, Has.Length.EqualTo(1));
            Assert.That(ArgumentsBindingPatches.Observed![0], Is.EqualTo(expected));
            Assert.That(ArgumentsBindingPatches.Observed[0], Is.TypeOf<int>());
        });
    }

    [Test]
    public void Prefix_Arguments_Primitive_ByReference_ArrayWriteDoesNotChangeArgument()
    {
        ArgumentsBindingPatches.Observed = null;
        int value = 42;
        var expected = value;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Prefix_Arguments_Primitive_ByReference_ArrayWriteDoesNotChangeArgument));

        var result = ArgumentsBindingTargets.Primitive_ByReference(ref value);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(value, Is.EqualTo(expected));
        });
    }

    [Test]
    public void Prefix_Arguments_ReferenceType_ByValue_ReadsArgument()
    {
        ArgumentsBindingPatches.Observed = null;
        object value = new BindingReference { Value = 42 };
        var expected = value;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Prefix_Arguments_ReferenceType_ByValue_ReadsArgument));

        var result = ArgumentsBindingTargets.ReferenceType_ByValue(value);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(expected));
            Assert.That(value, Is.SameAs(expected));
            Assert.That(ArgumentsBindingPatches.Observed, Has.Length.EqualTo(1));
            Assert.That(ArgumentsBindingPatches.Observed![0], Is.SameAs(expected));
            Assert.That(ArgumentsBindingPatches.Observed[0], Is.TypeOf<BindingReference>());
        });
    }

    [Test]
    public void InnerPrefix_Arguments_ReferenceType_ByValue_ReadsArgument()
    {
        ArgumentsBindingPatches.Observed = null;
        object value = new BindingReference { Value = 42 };
        var expected = value;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.InnerPrefix_Arguments_ReferenceType_ByValue_ReadsArgument));

        var result = ArgumentsBindingTargets.CallReferenceType_ByValue(value);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(expected));
            Assert.That(value, Is.SameAs(expected));
            Assert.That(ArgumentsBindingPatches.Observed, Has.Length.EqualTo(1));
            Assert.That(ArgumentsBindingPatches.Observed![0], Is.SameAs(expected));
            Assert.That(ArgumentsBindingPatches.Observed[0], Is.TypeOf<BindingReference>());
        });
    }

    [Test]
    public void Prefix_Arguments_ReferenceType_ByValue_ArrayWriteDoesNotChangeArgument()
    {
        ArgumentsBindingPatches.Observed = null;
        object value = new BindingReference { Value = 42 };
        var expected = value;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Prefix_Arguments_ReferenceType_ByValue_ArrayWriteDoesNotChangeArgument));

        var result = ArgumentsBindingTargets.ReferenceType_ByValue(value);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(expected));
            Assert.That(value, Is.SameAs(expected));
        });
    }

    [Test]
    public void Prefix_Arguments_ReferenceType_ByReference_ReadsArgument()
    {
        ArgumentsBindingPatches.Observed = null;
        object value = new BindingReference { Value = 42 };
        var expected = value;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Prefix_Arguments_ReferenceType_ByReference_ReadsArgument));

        var result = ArgumentsBindingTargets.ReferenceType_ByReference(ref value);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(expected));
            Assert.That(value, Is.SameAs(expected));
            Assert.That(ArgumentsBindingPatches.Observed, Has.Length.EqualTo(1));
            Assert.That(ArgumentsBindingPatches.Observed![0], Is.SameAs(expected));
            Assert.That(ArgumentsBindingPatches.Observed[0], Is.TypeOf<BindingReference>());
        });
    }

    [Test]
    public void InnerPrefix_Arguments_ReferenceType_ByReference_ReadsArgument()
    {
        ArgumentsBindingPatches.Observed = null;
        object value = new BindingReference { Value = 42 };
        var expected = value;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.InnerPrefix_Arguments_ReferenceType_ByReference_ReadsArgument));

        var result = ArgumentsBindingTargets.CallReferenceType_ByReference(ref value);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(expected));
            Assert.That(value, Is.SameAs(expected));
            Assert.That(ArgumentsBindingPatches.Observed, Has.Length.EqualTo(1));
            Assert.That(ArgumentsBindingPatches.Observed![0], Is.SameAs(expected));
            Assert.That(ArgumentsBindingPatches.Observed[0], Is.TypeOf<BindingReference>());
        });
    }

    [Test]
    public void Prefix_Arguments_ReferenceType_ByReference_ArrayWriteDoesNotChangeArgument()
    {
        ArgumentsBindingPatches.Observed = null;
        object value = new BindingReference { Value = 42 };
        var expected = value;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Prefix_Arguments_ReferenceType_ByReference_ArrayWriteDoesNotChangeArgument));

        var result = ArgumentsBindingTargets.ReferenceType_ByReference(ref value);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(expected));
            Assert.That(value, Is.SameAs(expected));
        });
    }

    [Test]
    public void Prefix_Arguments_Struct_ByValue_ReadsArgument()
    {
        ArgumentsBindingPatches.Observed = null;
        BindingStruct value = new BindingStruct { Value = 42 };
        var expected = value;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Prefix_Arguments_Struct_ByValue_ReadsArgument));

        var result = ArgumentsBindingTargets.Struct_ByValue(value);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(value, Is.EqualTo(expected));
            Assert.That(ArgumentsBindingPatches.Observed, Has.Length.EqualTo(1));
            Assert.That(ArgumentsBindingPatches.Observed![0], Is.EqualTo(expected));
            Assert.That(ArgumentsBindingPatches.Observed[0], Is.TypeOf<BindingStruct>());
        });
    }

    [Test]
    public void InnerPrefix_Arguments_Struct_ByValue_ReadsArgument()
    {
        ArgumentsBindingPatches.Observed = null;
        BindingStruct value = new BindingStruct { Value = 42 };
        var expected = value;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.InnerPrefix_Arguments_Struct_ByValue_ReadsArgument));

        var result = ArgumentsBindingTargets.CallStruct_ByValue(value);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(value, Is.EqualTo(expected));
            Assert.That(ArgumentsBindingPatches.Observed, Has.Length.EqualTo(1));
            Assert.That(ArgumentsBindingPatches.Observed![0], Is.EqualTo(expected));
            Assert.That(ArgumentsBindingPatches.Observed[0], Is.TypeOf<BindingStruct>());
        });
    }

    [Test]
    public void Prefix_Arguments_Struct_ByValue_ArrayWriteDoesNotChangeArgument()
    {
        ArgumentsBindingPatches.Observed = null;
        BindingStruct value = new BindingStruct { Value = 42 };
        var expected = value;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Prefix_Arguments_Struct_ByValue_ArrayWriteDoesNotChangeArgument));

        var result = ArgumentsBindingTargets.Struct_ByValue(value);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(value, Is.EqualTo(expected));
        });
    }

    [Test]
    public void Prefix_Arguments_Struct_ByReference_ReadsArgument()
    {
        ArgumentsBindingPatches.Observed = null;
        BindingStruct value = new BindingStruct { Value = 42 };
        var expected = value;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Prefix_Arguments_Struct_ByReference_ReadsArgument));

        var result = ArgumentsBindingTargets.Struct_ByReference(ref value);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(value, Is.EqualTo(expected));
            Assert.That(ArgumentsBindingPatches.Observed, Has.Length.EqualTo(1));
            Assert.That(ArgumentsBindingPatches.Observed![0], Is.EqualTo(expected));
            Assert.That(ArgumentsBindingPatches.Observed[0], Is.TypeOf<BindingStruct>());
        });
    }

    [Test]
    public void InnerPrefix_Arguments_Struct_ByReference_ReadsArgument()
    {
        ArgumentsBindingPatches.Observed = null;
        BindingStruct value = new BindingStruct { Value = 42 };
        var expected = value;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.InnerPrefix_Arguments_Struct_ByReference_ReadsArgument));

        var result = ArgumentsBindingTargets.CallStruct_ByReference(ref value);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(value, Is.EqualTo(expected));
            Assert.That(ArgumentsBindingPatches.Observed, Has.Length.EqualTo(1));
            Assert.That(ArgumentsBindingPatches.Observed![0], Is.EqualTo(expected));
            Assert.That(ArgumentsBindingPatches.Observed[0], Is.TypeOf<BindingStruct>());
        });
    }

    [Test]
    public void Prefix_Arguments_Struct_ByReference_ArrayWriteDoesNotChangeArgument()
    {
        ArgumentsBindingPatches.Observed = null;
        BindingStruct value = new BindingStruct { Value = 42 };
        var expected = value;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Prefix_Arguments_Struct_ByReference_ArrayWriteDoesNotChangeArgument));

        var result = ArgumentsBindingTargets.Struct_ByReference(ref value);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(value, Is.EqualTo(expected));
        });
    }

    [Test]
    public void Prefix_Arguments_NumericTypes_ByValue()
    {
        ArgumentsBindingPatches.Observed = null;
        bool boolean = true;
        byte unsignedByte = 201;
        sbyte signedByte = -101;
        short small = -1234;
        ushort unsignedSmall = 60000;
        char character = 'Q';
        uint unsigned = 4000000000U;
        long wide = -9000000000L;
        ulong unsignedWide = 18000000000000000000UL;
        float single = 1.25f;
        double real = -2.5;
        IntPtr native = new IntPtr(123);
        UIntPtr unsignedNative = new UIntPtr(456);
        int? nullable = 17;
        DayOfWeek enumeration = DayOfWeek.Friday;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Prefix_Arguments_NumericTypes_ByValue));

        ArgumentsBindingTargets.NumericByValue(boolean, unsignedByte, signedByte, small, unsignedSmall, character, unsigned, wide, unsignedWide, single, real, native, unsignedNative, nullable, enumeration);

        Assert.That(ArgumentsBindingPatches.Observed, Is.EqualTo(new object?[]
        {
            boolean, unsignedByte, signedByte, small, unsignedSmall, character, unsigned, wide, unsignedWide, single, real, native, unsignedNative, nullable, enumeration
        }));
        Assert.That(ArgumentsBindingPatches.Observed!.Select(value => value.GetType()), Is.EqualTo(new[]
        {
            typeof(bool), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(char), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(IntPtr), typeof(UIntPtr), typeof(int), typeof(DayOfWeek)
        }));
    }

    [Test]
    public void Prefix_Arguments_NumericTypes_ByReference()
    {
        ArgumentsBindingPatches.Observed = null;
        bool boolean = true;
        byte unsignedByte = 201;
        sbyte signedByte = -101;
        short small = -1234;
        ushort unsignedSmall = 60000;
        char character = 'Q';
        uint unsigned = 4000000000U;
        long wide = -9000000000L;
        ulong unsignedWide = 18000000000000000000UL;
        float single = 1.25f;
        double real = -2.5;
        IntPtr native = new IntPtr(123);
        UIntPtr unsignedNative = new UIntPtr(456);
        int? nullable = 17;
        DayOfWeek enumeration = DayOfWeek.Friday;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Prefix_Arguments_NumericTypes_ByReference));

        ArgumentsBindingTargets.NumericByReference(ref boolean, ref unsignedByte, ref signedByte, ref small, ref unsignedSmall, ref character, ref unsigned, ref wide, ref unsignedWide, ref single, ref real, ref native, ref unsignedNative, ref nullable, ref enumeration);

        Assert.That(ArgumentsBindingPatches.Observed, Is.EqualTo(new object?[]
        {
            boolean, unsignedByte, signedByte, small, unsignedSmall, character, unsigned, wide, unsignedWide, single, real, native, unsignedNative, nullable, enumeration
        }));
        Assert.That(ArgumentsBindingPatches.Observed!.Select(value => value.GetType()), Is.EqualTo(new[]
        {
            typeof(bool), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(char), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(IntPtr), typeof(UIntPtr), typeof(int), typeof(DayOfWeek)
        }));
    }

    [Test]
    public void Prefix_Arguments_InstanceMethod_ExcludesInstance()
    {
        ArgumentsBindingPatches.Observed = null;
        var target = new ArgumentsBindingInstanceTargets();
        object item = new BindingReference { Value = 11 };
        var structure = new BindingStruct { Value = 23 };
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Prefix_Arguments_InstanceMethod_ExcludesInstance));

        target.Values(item, ref structure, 31);

        Assert.That(ArgumentsBindingPatches.Observed, Is.EqualTo(new object?[] { item, structure, 31 }));
    }

    [Test]
    public void Prefix_Arguments_StructMethod_ExcludesInstance()
    {
        ArgumentsBindingPatches.Observed = null;
        var target = new ArgumentsBindingStructTargets();
        object item = new BindingReference { Value = 11 };
        var structure = new BindingStruct { Value = 23 };
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Prefix_Arguments_StructMethod_ExcludesInstance));

        target.Values(item, ref structure, 31);

        Assert.That(ArgumentsBindingPatches.Observed, Is.EqualTo(new object?[] { item, structure, 31 }));
    }

    [Test]
    public void Prefix_Arguments_StaticMethod_NoArguments()
    {
        ArgumentsBindingPatches.Observed = null;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Prefix_Arguments_StaticMethod_NoArguments));

        StaticMethodTargets.Void();

        Assert.That(ArgumentsBindingPatches.Observed, Is.EqualTo(new object?[] {  }));
    }

    [Test]
    public void Postfix_Arguments_InstanceMethod_NoArguments()
    {
        ArgumentsBindingPatches.Observed = null;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Postfix_Arguments_InstanceMethod_NoArguments));

        new ArgumentsBindingInstanceTargets().Empty();

        Assert.That(ArgumentsBindingPatches.Observed, Is.EqualTo(new object?[] {  }));
    }

    [Test]
    public void Postfix_Arguments_RefParameters_ObservesUpdatedValues()
    {
        ArgumentsBindingPatches.Observed = null;
        object item = new object();
        var structure = new BindingStruct { Value = 5 };
        int number = 7;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Postfix_Arguments_RefParameters_ObservesUpdatedValues));

        ArgumentsBindingTargets.ReplaceReferences(ref item, ref structure, ref number);

        Assert.That(ArgumentsBindingPatches.Observed, Is.EqualTo(new object?[] { "replacement", new BindingStruct { Value = 23 }, 31 }));
    }

    [Test]
    public void Postfix_Arguments_OutParameters_ObservesAssignedValues()
    {
        ArgumentsBindingPatches.Observed = null;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Postfix_Arguments_OutParameters_ObservesAssignedValues));

        ArgumentsBindingTargets.OutValues(out object item, out BindingStruct structure, out int number);

        Assert.That(ArgumentsBindingPatches.Observed, Is.EqualTo(new object?[] { "output", new BindingStruct { Value = 23 }, 31 }));
    }

    [Test]
    public void Prefix_Arguments_InParameters_ReadsValues()
    {
        ArgumentsBindingPatches.Observed = null;
        object item = new object();
        var structure = new BindingStruct { Value = 5 };
        int number = 7;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Prefix_Arguments_InParameters_ReadsValues));

        ArgumentsBindingTargets.InValues(in item, in structure, in number);

        Assert.That(ArgumentsBindingPatches.Observed, Is.EqualTo(new object?[] { item, structure, number }));
    }

    [Test]
    public void Prefix_Arguments_NullReferenceAndNullable_ProducesNullElements()
    {
        ArgumentsBindingPatches.Observed = null;
        object? item = null;
        int? number = null;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Prefix_Arguments_NullReferenceAndNullable_ProducesNullElements));

        ArgumentsBindingTargets.NullValues(ref item, ref number);

        Assert.That(ArgumentsBindingPatches.Observed, Is.EqualTo(new object?[] { null, null }));
    }

    [Test]
    public void InnerPrefix_Arguments_Scopes_UseRespectiveArgumentLists()
    {
        ArgumentsBindingPatches.Observed = null;
        ArgumentsBindingPatches.OuterObserved = null;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.InnerPrefix_Arguments_Scopes_UseRespectiveArgumentLists));

        Assert.That(ArgumentsBindingTargets.CallWithOuterArguments(41, "outer"), Is.EqualTo(42));

        Assert.That(ArgumentsBindingPatches.Observed, Is.EqualTo(new object?[] { 42 }));
        Assert.That(ArgumentsBindingPatches.OuterObserved, Is.EqualTo(new object?[] { 41, "outer" }));
    }

    [Test]
    public void InnerPostfix_Arguments_Scopes_UseRespectiveArgumentLists()
    {
        ArgumentsBindingPatches.Observed = null;
        ArgumentsBindingPatches.OuterObserved = null;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.InnerPostfix_Arguments_Scopes_UseRespectiveArgumentLists));

        Assert.That(ArgumentsBindingTargets.CallWithOuterArguments(41, "outer"), Is.EqualTo(42));

        Assert.That(ArgumentsBindingPatches.Observed, Is.EqualTo(new object?[] { 42 }));
        Assert.That(ArgumentsBindingPatches.OuterObserved, Is.EqualTo(new object?[] { 41, "outer" }));
    }

    [Test]
    public void Prefix_Arguments_Constructor_ExcludesInstance()
    {
        ArgumentsBindingPatches.Observed = null;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.Prefix_Arguments_Constructor_ExcludesInstance));

        var target = new ConstructorTargets(42);

        Assert.That(target.Value, Is.EqualTo(42));
        Assert.That(target.ConstructorExecuted, Is.True);
        Assert.That(ArgumentsBindingPatches.Observed, Is.EqualTo(new object?[] { 42 }));
    }

    [Test]
    public void InnerPrefix_Arguments_Constructor_ExcludesInstance()
    {
        ArgumentsBindingPatches.Observed = null;
        ApplyPatch(typeof(ArgumentsBindingPatches), nameof(ArgumentsBindingPatches.InnerPrefix_Arguments_Constructor_ExcludesInstance));

        var target = ConstructorTargets.Create(42);

        Assert.That(target.Value, Is.EqualTo(42));
        Assert.That(target.ConstructorExecuted, Is.True);
        Assert.That(ArgumentsBindingPatches.Observed, Is.EqualTo(new object?[] { 42 }));
    }

}
