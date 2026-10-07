using Disharmony.Optimizer;

namespace Disharmony;

[Flags]
internal enum OpCodeFlags
{
    Default = 0x0001,

    /// <summary>
    ///     Indicates that this operation can throw an exception.
    /// </summary>
    /// <remarks>
    ///     Exceptions that indicate bad CIL (e.g. <see cref="TypeLoadException" />) are not included.
    /// </remarks>
    CanThrow = 0x0002,

    /// <summary>
    ///     Indicates that the operation is an arithmetic operation, with output type determined by the types of its inputs.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This does not distinguish between integer and general numeric operations, or between operations with special
    ///         behavior for pointers and those without. The different operations only differ in which combinations of
    ///         inputs are valid CIL; for combinations that are valid, the results are always the same type, so this
    ///         flag represents the superset of all combinations that are valid for some combination.
    ///     </para>
    ///     <para>
    ///         If all operands have the same type, the result is that type.
    ///         If one operand is an <see langword="int" /> and the other is an <see cref="IntPtr" />, the result is
    ///         <see cref="IntPtr" />.
    ///         If one operand is a reference type and the other is an integer type, the result is the reference type.
    ///         If both operands are reference types, the result is <see cref="IntPtr" />.
    ///     </para>
    ///     <para>
    ///         See ECMA 335 part III Table 2: Binary Numeric Operations,
    ///         ECMA 335 part III Table 5: Integer Operations, and
    ///         ECMA 335 part III Table 7: Overflow Numeric Operations.
    ///     </para>
    /// </remarks>
    Arithmetic = 0x0004,

    /// <summary>
    ///     Indicates that the first argument is an int, long, or IntPtr and the second argument is an int or IntPtr,
    ///     and the result type is the same as the type of the first argument.
    /// </summary>
    /// <remarks>
    ///     See ECMA 335 part III Table 6: Shift Operations.
    /// </remarks>
    Shift = 0x0008,

    /// <summary>
    ///     Indicates that this instruction may have side effects other than control-flow changes.
    /// </summary>
    HasSideEffects = 0x0010,

    /// <summary>
    ///     Indicates that this instruction's output is the same as its input.
    /// </summary>
    PushesInput = 0x0020,

    /// <summary>
    ///     Indicates that the instruction is ldloc, ldarg, or ldind.
    /// </summary>
    /// <remarks>
    ///     The result type is determined by the type of the local variable, argument, or reference operand.
    /// </remarks>
    Load = 0x0040,

    /// <summary>
    ///     Indicates that the instruction is stloc, starg, or stind.
    /// </summary>
    /// <remarks>
    ///     The result type is determined by the type of the local variable, argument, or reference operand.
    /// </remarks>
    Store = 0x0080,

    /// <summary>
    ///     Indicates that the instruction is ldloca or ldarga.
    /// </summary>
    /// <remarks>
    ///     The result type is determined by the type of the local variable or argument.
    /// </remarks>
    LoadAddress = 0x0100,

    /// <summary>
    ///     Indicates that the instruction is ldloc, stloc, or ldloca.
    /// </summary>
    Local = 0x0200,

    /// <summary>
    ///     Indicates that the instruction is ldarg, starg, or ldarga.
    /// </summary>
    Argument = 0x0400,

    /// <summary>
    ///     Indicates that the instruction is ldind or stind.
    /// </summary>
    Indirect = 0x0800,

    /// <summary>
    ///     Indicates that the instruction is a macro whose operand is given by <see cref="OpCodeData.operand" />.
    /// </summary>
    FixedOperand = 0x1000,

    /// <summary>
    ///     Indicates that the instruction pushes a constant onto the stack.
    /// </summary>
    Constant = 0x2000,

    /// <summary>
    ///     Indicates that the result type of the instruction is given by a <see cref="Type" /> operand. Integer
    ///     and floating-point types are converted to their CIL stack type which is one of
    ///     <see langword="int" />, <see langword="long" />, <see cref="IntPtr" />, or <see langword="double" />.
    /// </summary>
    TypeFromOperand = 0x4000,

    /// <summary>
    ///     Indicates that the result type of the instruction is given by a <see cref="Type" /> operand's ByRef
    ///     version.
    /// </summary>
    TypeFromOperandRef = 0x8000,

    /// <summary>
    ///     Indicates that the instruction can throw <see cref="NullReferenceException" />.
    /// </summary>
    CanThrow_NullReference = 0x10000 | CanThrow,

    /// <summary>
    ///     Indicates that the instruction can throw <see cref="OverflowException" />.
    /// </summary>
    CanThrow_Overflow = 0x20000 | CanThrow,

    /// <summary>
    ///     Indicates that the instruction can throw <see cref="ArithmeticException" /> and/or
    ///     <see cref="DivideByZeroException" />.
    /// </summary>
    CanThrow_Arithmetic = 0x40000 | CanThrow,

    /// <summary>
    ///     Indicates that the instruction can throw <see cref="OutOfMemoryException" /> and/or
    ///     <see cref="StackOverflowException" />.
    /// </summary>
    CanThrow_OutOfMemory = 0x80000 | CanThrow,

    /// <summary>
    ///     Indicates that the instruction can throw <see cref="InvalidCastException" />.
    /// </summary>
    CanThrow_InvalidCast = 0x100000 | CanThrow,

    /// <summary>
    ///     Indicates that the instruction can throw <see cref="IndexOutOfRangeException" /> and/or
    ///     <see cref="NullReferenceException" />.
    /// </summary>
    CanThrow_IndexOutOfRange = 0x200000 | CanThrow_NullReference,

    ResultTypeMask = Arithmetic | Shift | Load | Store | LoadAddress | TypeFromOperand,

    SideEffectMask = CanThrow | HasSideEffects,

    VariableOperationMask = Load | Store | LoadAddress | Local | Argument | Indirect,
}

internal struct OpCodeData
{
    public OpCodeFlags flags;
    public Type? resultType;
    public int operand;
    public ushort canonical;

    private static readonly OpCodeData[] data = new OpCodeData[0x200];

    public static OpCodeData Get(ushort value)
    {
        var opCodeData = data[GetIndex(value)];
        return opCodeData.flags != 0 ? opCodeData : new OpCodeData { canonical = value };
    }

    public static OpCodeData Get(OpCode opCode) => Get(unchecked((ushort)opCode.Value));

    public static ushort GetCanonicalOpcode(ushort value) => Get(value).canonical;
    public static ushort GetCanonicalOpcode(OpCode opCode) => Get(opCode).canonical;
    public static ushort GetCanonicalOpcode(CodeInstruction inst) => Get(inst.opcode).canonical;
    public static ushort GetCanonicalOpcode(ILInstruction inst) => Get(inst.OpCode).canonical;

    public static int GetIntOperand(CodeInstruction inst) => GetIntOperand(inst.opcode, inst.operand);
    public static int GetIntOperand(ILInstruction inst) => GetIntOperand(inst.OpCode, inst.Operand);

    private static int GetIntOperand(OpCode opcode, object? operand)
    {
        var opcodeData = Get(opcode);
        return (opcodeData.flags & OpCodeFlags.FixedOperand) != 0 ? opcodeData.operand : Convert.ToInt32(operand);
    }

    private static int GetIndex(ushort value) => value >= 0xFE00 ? value - (0xFE00 - 0x100) : value;

    static OpCodeData()
    {
        foreach (var initValue in initValues)
        {
            var value = initValue.Data;
            value.canonical = value.canonical != 0 ? value.canonical : initValue.Value;
            var index = GetIndex(initValue.Value);
            if (data[index].flags != 0)
                throw new InvalidOperationException("Duplicate data");
            data[index] = value;
        }
    }

    private static readonly (ushort Value, OpCodeData Data)[] initValues =
    [
        // @formatter:off
        (OpCodeValues.Add,            new OpCodeData { flags = OpCodeFlags.Arithmetic }),
        (OpCodeValues.Add_Ovf,        new OpCodeData { flags = OpCodeFlags.Arithmetic | OpCodeFlags.CanThrow_Overflow }),
        (OpCodeValues.Add_Ovf_Un,     new OpCodeData { flags = OpCodeFlags.Arithmetic | OpCodeFlags.CanThrow_Overflow }),
        (OpCodeValues.And,            new OpCodeData { flags = OpCodeFlags.Arithmetic }),
        (OpCodeValues.Arglist,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(RuntimeArgumentHandle) }),
        (OpCodeValues.Beq,            new OpCodeData { flags = OpCodeFlags.Default }),
        (OpCodeValues.Beq_S,          new OpCodeData { flags = OpCodeFlags.Default, canonical = OpCodeValues.Beq }),
        (OpCodeValues.Bge,            new OpCodeData { flags = OpCodeFlags.Default }),
        (OpCodeValues.Bge_S,          new OpCodeData { flags = OpCodeFlags.Default, canonical = OpCodeValues.Bge }),
        (OpCodeValues.Bge_Un,         new OpCodeData { flags = OpCodeFlags.Default }),
        (OpCodeValues.Bge_Un_S,       new OpCodeData { flags = OpCodeFlags.Default, canonical = OpCodeValues.Bge_Un }),
        (OpCodeValues.Bgt,            new OpCodeData { flags = OpCodeFlags.Default }),
        (OpCodeValues.Bgt_S,          new OpCodeData { flags = OpCodeFlags.Default, canonical = OpCodeValues.Bgt }),
        (OpCodeValues.Bgt_Un,         new OpCodeData { flags = OpCodeFlags.Default }),
        (OpCodeValues.Bgt_Un_S,       new OpCodeData { flags = OpCodeFlags.Default, canonical = OpCodeValues.Bgt_Un }),
        (OpCodeValues.Ble,            new OpCodeData { flags = OpCodeFlags.Default }),
        (OpCodeValues.Ble_S,          new OpCodeData { flags = OpCodeFlags.Default, canonical = OpCodeValues.Ble }),
        (OpCodeValues.Ble_Un,         new OpCodeData { flags = OpCodeFlags.Default }),
        (OpCodeValues.Ble_Un_S,       new OpCodeData { flags = OpCodeFlags.Default, canonical = OpCodeValues.Ble_Un }),
        (OpCodeValues.Blt,            new OpCodeData { flags = OpCodeFlags.Default }),
        (OpCodeValues.Blt_S,          new OpCodeData { flags = OpCodeFlags.Default, canonical = OpCodeValues.Blt }),
        (OpCodeValues.Blt_Un,         new OpCodeData { flags = OpCodeFlags.Default }),
        (OpCodeValues.Blt_Un_S,       new OpCodeData { flags = OpCodeFlags.Default, canonical = OpCodeValues.Blt_Un }),
        (OpCodeValues.Bne_Un,         new OpCodeData { flags = OpCodeFlags.Default }),
        (OpCodeValues.Bne_Un_S,       new OpCodeData { flags = OpCodeFlags.Default, canonical = OpCodeValues.Bne_Un }),
        (OpCodeValues.Box,            new OpCodeData { flags = OpCodeFlags.CanThrow_OutOfMemory, resultType = typeof(object) }),
        (OpCodeValues.Br,             new OpCodeData { flags = OpCodeFlags.Default }),
        (OpCodeValues.Br_S,           new OpCodeData { flags = OpCodeFlags.Default, canonical = OpCodeValues.Br }),
        (OpCodeValues.Brfalse,        new OpCodeData { flags = OpCodeFlags.Default }),
        (OpCodeValues.Brfalse_S,      new OpCodeData { flags = OpCodeFlags.Default, canonical = OpCodeValues.Brfalse }),
        (OpCodeValues.Brtrue,         new OpCodeData { flags = OpCodeFlags.Default }),
        (OpCodeValues.Brtrue_S,       new OpCodeData { flags = OpCodeFlags.Default, canonical = OpCodeValues.Brtrue }),
        (OpCodeValues.Call,           new OpCodeData { flags = OpCodeFlags.CanThrow | OpCodeFlags.HasSideEffects }),
        (OpCodeValues.Calli,          new OpCodeData { flags = OpCodeFlags.CanThrow | OpCodeFlags.HasSideEffects }),
        (OpCodeValues.Callvirt,       new OpCodeData { flags = OpCodeFlags.CanThrow | OpCodeFlags.HasSideEffects }),
        (OpCodeValues.Castclass,      new OpCodeData { flags = OpCodeFlags.TypeFromOperand | OpCodeFlags.CanThrow_InvalidCast }),
        (OpCodeValues.Ceq,            new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (OpCodeValues.Cgt,            new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (OpCodeValues.Cgt_Un,         new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (OpCodeValues.Ckfinite,       new OpCodeData { flags = OpCodeFlags.PushesInput | OpCodeFlags.CanThrow_Arithmetic }),
        (OpCodeValues.Clt,            new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (OpCodeValues.Clt_Un,         new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (OpCodeValues.Conv_I,         new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(IntPtr) }),
        (OpCodeValues.Conv_I1,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (OpCodeValues.Conv_I2,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (OpCodeValues.Conv_I4,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (OpCodeValues.Conv_I8,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(long) }),
        (OpCodeValues.Conv_Ovf_I,     new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(IntPtr) }),
        (OpCodeValues.Conv_Ovf_I_Un,  new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(IntPtr) }),
        (OpCodeValues.Conv_Ovf_I1,    new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (OpCodeValues.Conv_Ovf_I1_Un, new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (OpCodeValues.Conv_Ovf_I2,    new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (OpCodeValues.Conv_Ovf_I2_Un, new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (OpCodeValues.Conv_Ovf_I4,    new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (OpCodeValues.Conv_Ovf_I4_Un, new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (OpCodeValues.Conv_Ovf_I8,    new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(long) }),
        (OpCodeValues.Conv_Ovf_I8_Un, new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(long) }),
        (OpCodeValues.Conv_Ovf_U,     new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(IntPtr) }),
        (OpCodeValues.Conv_Ovf_U_Un,  new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(IntPtr) }),
        (OpCodeValues.Conv_Ovf_U1,    new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (OpCodeValues.Conv_Ovf_U1_Un, new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (OpCodeValues.Conv_Ovf_U2,    new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (OpCodeValues.Conv_Ovf_U2_Un, new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (OpCodeValues.Conv_Ovf_U4,    new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (OpCodeValues.Conv_Ovf_U4_Un, new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (OpCodeValues.Conv_Ovf_U8,    new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(long) }),
        (OpCodeValues.Conv_Ovf_U8_Un, new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(long) }),
        (OpCodeValues.Conv_R_Un,      new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(double) }),
        (OpCodeValues.Conv_R4,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(double) }),
        (OpCodeValues.Conv_R8,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(double) }),
        (OpCodeValues.Conv_U,         new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(IntPtr) }),
        (OpCodeValues.Conv_U1,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (OpCodeValues.Conv_U2,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (OpCodeValues.Conv_U4,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (OpCodeValues.Conv_U8,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(long) }),
        (OpCodeValues.Cpblk,          new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (OpCodeValues.Cpobj,          new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (OpCodeValues.Div,            new OpCodeData { flags = OpCodeFlags.Arithmetic | OpCodeFlags.CanThrow_Arithmetic }),
        (OpCodeValues.Div_Un,         new OpCodeData { flags = OpCodeFlags.Arithmetic | OpCodeFlags.CanThrow_Arithmetic }),
        (OpCodeValues.Dup,            new OpCodeData { flags = OpCodeFlags.PushesInput }),
        (OpCodeValues.Initblk,        new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (OpCodeValues.Initobj,        new OpCodeData { flags = OpCodeFlags.HasSideEffects, resultType = typeof(void) }),
        (OpCodeValues.Isinst,         new OpCodeData { flags = OpCodeFlags.TypeFromOperand }),
        (OpCodeValues.Jmp,            new OpCodeData { flags = OpCodeFlags.HasSideEffects }),
        (OpCodeValues.Ldarg,          new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Argument }),
        (OpCodeValues.Ldarg_0,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Argument | OpCodeFlags.FixedOperand, operand = 0, canonical = OpCodeValues.Ldarg }),
        (OpCodeValues.Ldarg_1,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Argument | OpCodeFlags.FixedOperand, operand = 1, canonical = OpCodeValues.Ldarg }),
        (OpCodeValues.Ldarg_2,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Argument | OpCodeFlags.FixedOperand, operand = 2, canonical = OpCodeValues.Ldarg }),
        (OpCodeValues.Ldarg_3,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Argument | OpCodeFlags.FixedOperand, operand = 3, canonical = OpCodeValues.Ldarg }),
        (OpCodeValues.Ldarg_S,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Argument, canonical = OpCodeValues.Ldarg }),
        (OpCodeValues.Ldarga,         new OpCodeData { flags = OpCodeFlags.LoadAddress | OpCodeFlags.Argument }),
        (OpCodeValues.Ldarga_S,       new OpCodeData { flags = OpCodeFlags.LoadAddress | OpCodeFlags.Argument, canonical = OpCodeValues.Ldarga }),
        (OpCodeValues.Ldc_I4,         new OpCodeData { flags = OpCodeFlags.Constant, resultType = typeof(int) }),
        (OpCodeValues.Ldc_I4_0,       new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(int), operand = 0, canonical = OpCodeValues.Ldc_I4 }),
        (OpCodeValues.Ldc_I4_1,       new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(int), operand = 1, canonical = OpCodeValues.Ldc_I4 }),
        (OpCodeValues.Ldc_I4_2,       new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(int), operand = 2, canonical = OpCodeValues.Ldc_I4 }),
        (OpCodeValues.Ldc_I4_3,       new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(int), operand = 3, canonical = OpCodeValues.Ldc_I4 }),
        (OpCodeValues.Ldc_I4_4,       new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(int), operand = 4, canonical = OpCodeValues.Ldc_I4 }),
        (OpCodeValues.Ldc_I4_5,       new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(int), operand = 5, canonical = OpCodeValues.Ldc_I4 }),
        (OpCodeValues.Ldc_I4_6,       new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(int), operand = 6, canonical = OpCodeValues.Ldc_I4 }),
        (OpCodeValues.Ldc_I4_7,       new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(int), operand = 7, canonical = OpCodeValues.Ldc_I4 }),
        (OpCodeValues.Ldc_I4_8,       new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(int), operand = 8, canonical = OpCodeValues.Ldc_I4 }),
        (OpCodeValues.Ldc_I4_M1,      new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(int), operand = -1, canonical = OpCodeValues.Ldc_I4 }),
        (OpCodeValues.Ldc_I4_S,       new OpCodeData { flags = OpCodeFlags.Constant, resultType = typeof(int), canonical = OpCodeValues.Ldc_I4 }),
        (OpCodeValues.Ldc_I8,         new OpCodeData { flags = OpCodeFlags.Constant, resultType = typeof(long) }),
        (OpCodeValues.Ldc_R4,         new OpCodeData { flags = OpCodeFlags.Constant, resultType = typeof(double) }),
        (OpCodeValues.Ldc_R8,         new OpCodeData { flags = OpCodeFlags.Constant, resultType = typeof(double) }),
        (OpCodeValues.Ldelem,         new OpCodeData { flags = OpCodeFlags.TypeFromOperand | OpCodeFlags.CanThrow_IndexOutOfRange }),
        (OpCodeValues.Ldelem_I,       new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange, resultType = typeof(IntPtr) }),
        (OpCodeValues.Ldelem_I1,      new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange, resultType = typeof(int) }),
        (OpCodeValues.Ldelem_I2,      new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange, resultType = typeof(int) }),
        (OpCodeValues.Ldelem_I4,      new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange, resultType = typeof(int) }),
        (OpCodeValues.Ldelem_I8,      new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange, resultType = typeof(long) }),
        (OpCodeValues.Ldelem_R4,      new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange, resultType = typeof(double) }),
        (OpCodeValues.Ldelem_R8,      new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange, resultType = typeof(double) }),
        (OpCodeValues.Ldelem_Ref,     new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange }),
        (OpCodeValues.Ldelem_U1,      new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange, resultType = typeof(int) }),
        (OpCodeValues.Ldelem_U2,      new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange, resultType = typeof(int) }),
        (OpCodeValues.Ldelem_U4,      new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange, resultType = typeof(int) }),
        (OpCodeValues.Ldelema,        new OpCodeData { flags = OpCodeFlags.TypeFromOperandRef | OpCodeFlags.CanThrow }),
        (OpCodeValues.Ldfld,          new OpCodeData { flags = OpCodeFlags.CanThrow_NullReference }),
        (OpCodeValues.Ldflda,         new OpCodeData { flags = OpCodeFlags.CanThrow_NullReference }),
        (OpCodeValues.Ldftn,          new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(IntPtr) }),
        (OpCodeValues.Ldind_I,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(IntPtr) }),
        (OpCodeValues.Ldind_I1,       new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(int) }),
        (OpCodeValues.Ldind_I2,       new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(int) }),
        (OpCodeValues.Ldind_I4,       new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(int) }),
        (OpCodeValues.Ldind_I8,       new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(long) }),
        (OpCodeValues.Ldind_R4,       new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(double) }),
        (OpCodeValues.Ldind_R8,       new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(double) }),
        (OpCodeValues.Ldind_Ref,      new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference }),
        (OpCodeValues.Ldind_U1,       new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(int) }),
        (OpCodeValues.Ldind_U2,       new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(int) }),
        (OpCodeValues.Ldind_U4,       new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(int) }),
        (OpCodeValues.Ldlen,          new OpCodeData { flags = OpCodeFlags.CanThrow_NullReference, resultType = typeof(IntPtr) }),
        (OpCodeValues.Ldloc,          new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Local }),
        (OpCodeValues.Ldloc_0,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Local | OpCodeFlags.FixedOperand, operand = 0, canonical = OpCodeValues.Ldloc }),
        (OpCodeValues.Ldloc_1,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Local | OpCodeFlags.FixedOperand, operand = 1, canonical = OpCodeValues.Ldloc }),
        (OpCodeValues.Ldloc_2,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Local | OpCodeFlags.FixedOperand, operand = 2, canonical = OpCodeValues.Ldloc }),
        (OpCodeValues.Ldloc_3,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Local | OpCodeFlags.FixedOperand, operand = 3, canonical = OpCodeValues.Ldloc }),
        (OpCodeValues.Ldloc_S,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Local, canonical = OpCodeValues.Ldloc }),
        (OpCodeValues.Ldloca,         new OpCodeData { flags = OpCodeFlags.LoadAddress | OpCodeFlags.Local }),
        (OpCodeValues.Ldloca_S,       new OpCodeData { flags = OpCodeFlags.LoadAddress | OpCodeFlags.Local, canonical = OpCodeValues.Ldloca }),
        (OpCodeValues.Ldnull,         new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(TypeLattice.NullPtr), operand = 0 }),
        (OpCodeValues.Ldobj,          new OpCodeData { flags = OpCodeFlags.TypeFromOperand | OpCodeFlags.CanThrow_NullReference }),
        (OpCodeValues.Ldsfld,         new OpCodeData { flags = OpCodeFlags.Default }),
        (OpCodeValues.Ldsflda,        new OpCodeData { flags = OpCodeFlags.Default }),
        (OpCodeValues.Ldstr,          new OpCodeData { flags = OpCodeFlags.Constant, resultType = typeof(string) }),
        (OpCodeValues.Ldtoken,        new OpCodeData { flags = OpCodeFlags.Default }),
        (OpCodeValues.Ldvirtftn,      new OpCodeData { flags = OpCodeFlags.CanThrow_NullReference, resultType = typeof(IntPtr) }),
        (OpCodeValues.Leave,          new OpCodeData { flags = OpCodeFlags.Default }),
        (OpCodeValues.Leave_S,        new OpCodeData { flags = OpCodeFlags.Default, canonical = OpCodeValues.Leave }),
        (OpCodeValues.Localloc,       new OpCodeData { flags = OpCodeFlags.CanThrow_OutOfMemory, resultType = typeof(IntPtr) }),
        (OpCodeValues.Mkrefany,       new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(TypedReference) }),
        (OpCodeValues.Mul,            new OpCodeData { flags = OpCodeFlags.Arithmetic }),
        (OpCodeValues.Mul_Ovf,        new OpCodeData { flags = OpCodeFlags.Arithmetic | OpCodeFlags.CanThrow_Overflow }),
        (OpCodeValues.Mul_Ovf_Un,     new OpCodeData { flags = OpCodeFlags.Arithmetic | OpCodeFlags.CanThrow_Overflow }),
        (OpCodeValues.Neg,            new OpCodeData { flags = OpCodeFlags.Arithmetic }),
        (OpCodeValues.Newarr,         new OpCodeData { flags = OpCodeFlags.CanThrow_OutOfMemory | OpCodeFlags.CanThrow_Overflow }),
        (OpCodeValues.Newobj,         new OpCodeData { flags = OpCodeFlags.CanThrow_OutOfMemory | OpCodeFlags.HasSideEffects }),
        (OpCodeValues.Nop,            new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(void) }),
        (OpCodeValues.Not,            new OpCodeData { flags = OpCodeFlags.Arithmetic }),
        (OpCodeValues.Or,             new OpCodeData { flags = OpCodeFlags.Arithmetic }),
        (OpCodeValues.Pop,            new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(void) }),
        (OpCodeValues.Refanytype,     new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(RuntimeTypeHandle) }),
        (OpCodeValues.Refanyval,      new OpCodeData { flags = OpCodeFlags.TypeFromOperandRef | OpCodeFlags.CanThrow_InvalidCast }),
        (OpCodeValues.Rem,            new OpCodeData { flags = OpCodeFlags.Arithmetic | OpCodeFlags.CanThrow_Arithmetic }),
        (OpCodeValues.Rem_Un,         new OpCodeData { flags = OpCodeFlags.Arithmetic | OpCodeFlags.CanThrow_Arithmetic }),
        (OpCodeValues.Shl,            new OpCodeData { flags = OpCodeFlags.Shift }),
        (OpCodeValues.Shr,            new OpCodeData { flags = OpCodeFlags.Shift }),
        (OpCodeValues.Shr_Un,         new OpCodeData { flags = OpCodeFlags.Shift }),
        (OpCodeValues.Sizeof,         new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(IntPtr) }),
        (OpCodeValues.Starg,          new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Argument, resultType = typeof(void) }),
        (OpCodeValues.Starg_S,        new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Argument, resultType = typeof(void), canonical = OpCodeValues.Starg }),
        (OpCodeValues.Stelem,         new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow, resultType = typeof(void) }),
        (OpCodeValues.Stelem_I,       new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow, resultType = typeof(void) }),
        (OpCodeValues.Stelem_I1,      new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow, resultType = typeof(void) }),
        (OpCodeValues.Stelem_I2,      new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow, resultType = typeof(void) }),
        (OpCodeValues.Stelem_I4,      new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow, resultType = typeof(void) }),
        (OpCodeValues.Stelem_I8,      new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow, resultType = typeof(void) }),
        (OpCodeValues.Stelem_R4,      new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow, resultType = typeof(void) }),
        (OpCodeValues.Stelem_R8,      new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow, resultType = typeof(void) }),
        (OpCodeValues.Stelem_Ref,     new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow, resultType = typeof(void) }),
        (OpCodeValues.Stfld,          new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (OpCodeValues.Stind_I,        new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (OpCodeValues.Stind_I1,       new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (OpCodeValues.Stind_I2,       new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (OpCodeValues.Stind_I4,       new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (OpCodeValues.Stind_I8,       new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (OpCodeValues.Stind_R4,       new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (OpCodeValues.Stind_R8,       new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (OpCodeValues.Stind_Ref,      new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (OpCodeValues.Stloc,          new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Local, resultType = typeof(void) }),
        (OpCodeValues.Stloc_0,        new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Local | OpCodeFlags.FixedOperand, resultType = typeof(void), operand = 0, canonical = OpCodeValues.Stloc }),
        (OpCodeValues.Stloc_1,        new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Local | OpCodeFlags.FixedOperand, resultType = typeof(void), operand = 1, canonical = OpCodeValues.Stloc }),
        (OpCodeValues.Stloc_2,        new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Local | OpCodeFlags.FixedOperand, resultType = typeof(void), operand = 2, canonical = OpCodeValues.Stloc }),
        (OpCodeValues.Stloc_3,        new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Local | OpCodeFlags.FixedOperand, resultType = typeof(void), operand = 3, canonical = OpCodeValues.Stloc }),
        (OpCodeValues.Stloc_S,        new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Local, resultType = typeof(void), canonical = OpCodeValues.Stloc }),
        (OpCodeValues.Stobj,          new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (OpCodeValues.Stsfld,         new OpCodeData { flags = OpCodeFlags.HasSideEffects, resultType = typeof(void) }),
        (OpCodeValues.Sub,            new OpCodeData { flags = OpCodeFlags.Arithmetic }),
        (OpCodeValues.Sub_Ovf,        new OpCodeData { flags = OpCodeFlags.Arithmetic | OpCodeFlags.CanThrow_Overflow }),
        (OpCodeValues.Sub_Ovf_Un,     new OpCodeData { flags = OpCodeFlags.Arithmetic | OpCodeFlags.CanThrow_Overflow }),
        (OpCodeValues.Unbox,          new OpCodeData { flags = OpCodeFlags.TypeFromOperandRef | OpCodeFlags.CanThrow_NullReference | OpCodeFlags.CanThrow_InvalidCast }),
        (OpCodeValues.Unbox_Any,      new OpCodeData { flags = OpCodeFlags.TypeFromOperand | OpCodeFlags.CanThrow_NullReference | OpCodeFlags.CanThrow_InvalidCast }),
        (OpCodeValues.Xor,            new OpCodeData { flags = OpCodeFlags.Arithmetic }),
        // @formatter:on
    ];
}
