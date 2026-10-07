namespace Disharmony.Tests;

public sealed class ArgumentsBindingInstanceTargets
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Values(object item, ref BindingStruct structure, int number) { }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Empty() { }
}

public struct ArgumentsBindingStructTargets
{
    public int InstanceValue;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Values(object item, ref BindingStruct structure, int number) { }
}

public static class ArgumentsBindingTargets
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int CallWithOuterArguments(int number, string label) => Primitive_ByValue(number + 1);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ReplaceReferences(ref object item, ref BindingStruct structure, ref int number)
    {
        item = "replacement";
        structure = new BindingStruct { Value = 23 };
        number = 31;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void NullValues(ref object? item, ref int? number) { }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void OutValues(out object item, out BindingStruct structure, out int number)
    {
        item = "output";
        structure = new BindingStruct { Value = 23 };
        number = 31;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void InValues(in object item, in BindingStruct structure, in int number) { }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int Primitive_ByValue(int value) => value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int CallPrimitive_ByValue(int value) => Primitive_ByValue(value);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int Primitive_ByReference(ref int value) => value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int CallPrimitive_ByReference(ref int value) => Primitive_ByReference(ref value);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static object ReferenceType_ByValue(object value) => value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static object CallReferenceType_ByValue(object value) => ReferenceType_ByValue(value);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static object ReferenceType_ByReference(ref object value) => value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static object CallReferenceType_ByReference(ref object value) => ReferenceType_ByReference(ref value);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static BindingStruct Struct_ByValue(BindingStruct value) => value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static BindingStruct CallStruct_ByValue(BindingStruct value) => Struct_ByValue(value);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static BindingStruct Struct_ByReference(ref BindingStruct value) => value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static BindingStruct CallStruct_ByReference(ref BindingStruct value) => Struct_ByReference(ref value);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void NumericByValue(bool boolean, byte unsignedByte, sbyte signedByte, short small, ushort unsignedSmall, char character, uint unsigned, long wide, ulong unsignedWide, float single, double real, IntPtr native, UIntPtr unsignedNative, int? nullable, DayOfWeek enumeration) { }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void NumericByReference(ref bool boolean, ref byte unsignedByte, ref sbyte signedByte, ref short small, ref ushort unsignedSmall, ref char character, ref uint unsigned, ref long wide, ref ulong unsignedWide, ref float single, ref double real, ref IntPtr native, ref UIntPtr unsignedNative, ref int? nullable, ref DayOfWeek enumeration) { }

}
