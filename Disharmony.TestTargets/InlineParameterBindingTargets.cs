namespace Disharmony.Tests;

public static class InlineParameterBindingTargets
{
    public static int TargetCalls;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int OuterPrefix_Argument_WriteByReference(int value)
    {
        TargetCalls++;
        return value;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int InnerPrefix_Argument_WriteByReference(int value) =>
        InnerPrefix_Argument_WriteByReference_Inner(value);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int InnerPrefix_Argument_WriteByReference_Inner(int value)
    {
        TargetCalls++;
        return value;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int OuterPostfix_Result_WriteByReference()
    {
        TargetCalls++;
        return 10;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int InnerPostfix_Result_WriteByReference() =>
        InnerPostfix_Result_WriteByReference_Inner();

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int InnerPostfix_Result_WriteByReference_Inner()
    {
        TargetCalls++;
        return 10;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int PrimitiveIdentity(int value) => value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static OptimizerDataObject ReferenceIdentity(OptimizerDataObject value) => value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static OptimizerDataStruct StructIdentity(OptimizerDataStruct value) => value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int PrimitiveResult() => 7;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static OptimizerDataObject ReferenceResult() =>
        new() { Number = 7, Text = "original" };

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static OptimizerDataStruct StructResult() =>
        new() { Number = 7, Text = "original" };

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int RefPrimitiveIdentity(ref int value)
    {
        value++;
        return value;
    }
}

public static class InlineParameterBindingPatches
{
    public static int PrimitiveObserved;
    public static OptimizerDataObject? ReferenceObserved;
    public static OptimizerDataStruct StructObserved;
    public static int StateObserved;
    public static int ResultObserved;

    public static void OuterPrefix_Argument_WriteByReference(ref int value) => value = 20;
    public static void InnerPrefix_Argument_WriteByReference(ref int value) => value = 20;
    public static void OuterPostfix_Result_WriteByReference(ref int __result) => __result = 20;
    public static void InnerPostfix_Result_WriteByReference(ref int __result) => __result = 20;

    public static void Prefix_Argument_Primitive_ReadByReference(ref int value) =>
        PrimitiveObserved = value;

    public static void Prefix_Argument_Primitive_WriteByReference(ref int value) =>
        value = 42;

    public static void Prefix_Argument_ReferenceType_ReadByReference(ref OptimizerDataObject value) =>
        ReferenceObserved = value;

    public static void Prefix_Argument_ReferenceType_WriteByReference(ref OptimizerDataObject value) =>
        value = new OptimizerDataObject { Number = 42, Text = "patched" };

    public static void Prefix_Argument_Struct_ReadByReference(ref OptimizerDataStruct value) =>
        StructObserved = value;

    public static void Prefix_Argument_Struct_WriteByReference(ref OptimizerDataStruct value) =>
        value = new OptimizerDataStruct { Number = 42, Text = "patched" };

    public static void Postfix_Result_Primitive_ReadByReference(ref int __result) =>
        PrimitiveObserved = __result;

    public static void Postfix_Result_Primitive_WriteByReference(ref int __result) =>
        __result = 42;

    public static void Postfix_Result_ReferenceType_ReadByReference(ref OptimizerDataObject __result) =>
        ReferenceObserved = __result;

    public static void Postfix_Result_ReferenceType_WriteByReference(ref OptimizerDataObject __result) =>
        __result = new OptimizerDataObject { Number = 42, Text = "patched" };

    public static void Postfix_Result_Struct_ReadByReference(ref OptimizerDataStruct __result) =>
        StructObserved = __result;

    public static void Postfix_Result_Struct_WriteByReference(ref OptimizerDataStruct __result) =>
        __result = new OptimizerDataStruct { Number = 42, Text = "patched" };

    public static void Prefix_TargetRefArgument_Primitive_ReadByReference(ref int value) =>
        PrimitiveObserved = value;

    public static void Prefix_TargetRefArgument_Primitive_WriteByReference(ref int value) =>
        value = 42;

    public static void PrefixPostfix_StateAndResult_PreservesValues_Prefix(int value, out int __state) =>
        __state = value;

    public static void PrefixPostfix_StateAndResult_PreservesValues_Postfix(int __state, ref int __result)
    {
        StateObserved = __state;
        ResultObserved = __result;
        __result = 42;
    }
}
