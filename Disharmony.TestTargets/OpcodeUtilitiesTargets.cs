namespace Disharmony.Tests;

public struct OpcodeUtilitiesStruct
{
    public int Value;

    public OpcodeUtilitiesStruct(int value) => Value = value;
}

public sealed class OpcodeUtilitiesClass
{
    public byte ByteField;
    public int IntField;
    public OpcodeUtilitiesStruct StructField;
    public OpcodeUtilitiesClass ClassField = null!;

    public static byte StaticByteField;
    public static int StaticIntField;
    public static OpcodeUtilitiesStruct StaticStructField;
    public static OpcodeUtilitiesClass StaticClassField = null!;

    public OpcodeUtilitiesClass() { }

    public OpcodeUtilitiesClass(int value) => IntField = value;

    public int ReturnInstanceInt() => IntField;

    public byte ReturnInstanceByte() => ByteField;

    public void ReturnInstanceVoid() { }

    public static void ReturnVoid() { }

    public static int ReturnInt() => 0;

    public static byte ReturnByte() => 0;

    public static long ReturnLong() => 0;

    public static IntPtr ReturnIntPtr() => IntPtr.Zero;

    public static double ReturnDouble() => 0;

    public static OpcodeUtilitiesStruct ReturnStruct() => default;

    public static OpcodeUtilitiesClass ReturnClass() => null!;

    public static ref int ReturnIntByReference() => ref StaticIntField;

    public static ref OpcodeUtilitiesStruct ReturnStructByReference() => ref StaticStructField;

    public static ref OpcodeUtilitiesClass ReturnClassByReference() => ref StaticClassField;
}
