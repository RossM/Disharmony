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
    public ILOpCode canonical;

    private static readonly OpCodeData[] data = new OpCodeData[0x200];

    public static OpCodeData Get(ILOpCode value)
    {
        var opCodeData = data[GetIndex(value)];
        return opCodeData.flags != 0 ? opCodeData : new OpCodeData { canonical = value };
    }

    public static OpCodeData Get(OpCode opCode) => Get(unchecked((ILOpCode)opCode.Value));

    public static ILOpCode GetCanonicalOpcode(OpCode opCode) => Get(opCode).canonical;
    public static ILOpCode GetCanonicalOpcode(CodeInstruction inst) => Get(inst.opcode).canonical;
    public static ILOpCode GetCanonicalOpcode(ILInstruction inst) => Get(inst.OpCode).canonical;

    public static int GetIntOperand(CodeInstruction inst) => GetIntOperand(inst.opcode, inst.operand);
    public static int GetIntOperand(ILInstruction inst) => GetIntOperand(inst.OpCode, inst.Operand);

    public static int GetIntOperand(OpCode opcode, object? operand)
    {
        var opcodeData = Get(opcode);
        return (opcodeData.flags & OpCodeFlags.FixedOperand) != 0 ? opcodeData.operand : Convert.ToInt32(operand);
    }

    private static int GetIndex(ILOpCode opcode)
    {
        ushort value = (ushort)opcode;
        return value >= 0xFE00 ? value - (0xFE00 - 0x100) : value;
    }

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

    private static readonly (ILOpCode Value, OpCodeData Data)[] initValues =
    [
        // @formatter:off
        (ILOpCode.Add,            new OpCodeData { flags = OpCodeFlags.Arithmetic }),
        (ILOpCode.Add_ovf,        new OpCodeData { flags = OpCodeFlags.Arithmetic | OpCodeFlags.CanThrow_Overflow }),
        (ILOpCode.Add_ovf_un,     new OpCodeData { flags = OpCodeFlags.Arithmetic | OpCodeFlags.CanThrow_Overflow }),
        (ILOpCode.And,            new OpCodeData { flags = OpCodeFlags.Arithmetic }),
        (ILOpCode.Arglist,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(RuntimeArgumentHandle) }),
        (ILOpCode.Beq,            new OpCodeData { flags = OpCodeFlags.Default }),
        (ILOpCode.Beq_s,          new OpCodeData { flags = OpCodeFlags.Default, canonical = ILOpCode.Beq }),
        (ILOpCode.Bge,            new OpCodeData { flags = OpCodeFlags.Default }),
        (ILOpCode.Bge_s,          new OpCodeData { flags = OpCodeFlags.Default, canonical = ILOpCode.Bge }),
        (ILOpCode.Bge_un,         new OpCodeData { flags = OpCodeFlags.Default }),
        (ILOpCode.Bge_un_s,       new OpCodeData { flags = OpCodeFlags.Default, canonical = ILOpCode.Bge_un }),
        (ILOpCode.Bgt,            new OpCodeData { flags = OpCodeFlags.Default }),
        (ILOpCode.Bgt_s,          new OpCodeData { flags = OpCodeFlags.Default, canonical = ILOpCode.Bgt }),
        (ILOpCode.Bgt_un,         new OpCodeData { flags = OpCodeFlags.Default }),
        (ILOpCode.Bgt_un_s,       new OpCodeData { flags = OpCodeFlags.Default, canonical = ILOpCode.Bgt_un }),
        (ILOpCode.Ble,            new OpCodeData { flags = OpCodeFlags.Default }),
        (ILOpCode.Ble_s,          new OpCodeData { flags = OpCodeFlags.Default, canonical = ILOpCode.Ble }),
        (ILOpCode.Ble_un,         new OpCodeData { flags = OpCodeFlags.Default }),
        (ILOpCode.Ble_un_s,       new OpCodeData { flags = OpCodeFlags.Default, canonical = ILOpCode.Ble_un }),
        (ILOpCode.Blt,            new OpCodeData { flags = OpCodeFlags.Default }),
        (ILOpCode.Blt_s,          new OpCodeData { flags = OpCodeFlags.Default, canonical = ILOpCode.Blt }),
        (ILOpCode.Blt_un,         new OpCodeData { flags = OpCodeFlags.Default }),
        (ILOpCode.Blt_un_s,       new OpCodeData { flags = OpCodeFlags.Default, canonical = ILOpCode.Blt_un }),
        (ILOpCode.Bne_un,         new OpCodeData { flags = OpCodeFlags.Default }),
        (ILOpCode.Bne_un_s,       new OpCodeData { flags = OpCodeFlags.Default, canonical = ILOpCode.Bne_un }),
        (ILOpCode.Box,            new OpCodeData { flags = OpCodeFlags.CanThrow_OutOfMemory, resultType = typeof(object) }),
        (ILOpCode.Br,             new OpCodeData { flags = OpCodeFlags.Default }),
        (ILOpCode.Br_s,           new OpCodeData { flags = OpCodeFlags.Default, canonical = ILOpCode.Br }),
        (ILOpCode.Brfalse,        new OpCodeData { flags = OpCodeFlags.Default }),
        (ILOpCode.Brfalse_s,      new OpCodeData { flags = OpCodeFlags.Default, canonical = ILOpCode.Brfalse }),
        (ILOpCode.Brtrue,         new OpCodeData { flags = OpCodeFlags.Default }),
        (ILOpCode.Brtrue_s,       new OpCodeData { flags = OpCodeFlags.Default, canonical = ILOpCode.Brtrue }),
        (ILOpCode.Call,           new OpCodeData { flags = OpCodeFlags.CanThrow | OpCodeFlags.HasSideEffects }),
        (ILOpCode.Calli,          new OpCodeData { flags = OpCodeFlags.CanThrow | OpCodeFlags.HasSideEffects }),
        (ILOpCode.Callvirt,       new OpCodeData { flags = OpCodeFlags.CanThrow | OpCodeFlags.HasSideEffects }),
        (ILOpCode.Castclass,      new OpCodeData { flags = OpCodeFlags.TypeFromOperand | OpCodeFlags.CanThrow_InvalidCast }),
        (ILOpCode.Ceq,            new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (ILOpCode.Cgt,            new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (ILOpCode.Cgt_un,         new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (ILOpCode.Ckfinite,       new OpCodeData { flags = OpCodeFlags.PushesInput | OpCodeFlags.CanThrow_Arithmetic }),
        (ILOpCode.Clt,            new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (ILOpCode.Clt_un,         new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (ILOpCode.Conv_i,         new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(IntPtr) }),
        (ILOpCode.Conv_i1,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (ILOpCode.Conv_i2,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (ILOpCode.Conv_i4,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (ILOpCode.Conv_i8,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(long) }),
        (ILOpCode.Conv_ovf_i,     new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(IntPtr) }),
        (ILOpCode.Conv_ovf_i_un,  new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(IntPtr) }),
        (ILOpCode.Conv_ovf_i1,    new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (ILOpCode.Conv_ovf_i1_un, new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (ILOpCode.Conv_ovf_i2,    new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (ILOpCode.Conv_ovf_i2_un, new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (ILOpCode.Conv_ovf_i4,    new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (ILOpCode.Conv_ovf_i4_un, new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (ILOpCode.Conv_ovf_i8,    new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(long) }),
        (ILOpCode.Conv_ovf_i8_un, new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(long) }),
        (ILOpCode.Conv_ovf_u,     new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(IntPtr) }),
        (ILOpCode.Conv_ovf_u_un,  new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(IntPtr) }),
        (ILOpCode.Conv_ovf_u1,    new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (ILOpCode.Conv_ovf_u1_un, new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (ILOpCode.Conv_ovf_u2,    new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (ILOpCode.Conv_ovf_u2_un, new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (ILOpCode.Conv_ovf_u4,    new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (ILOpCode.Conv_ovf_u4_un, new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(int) }),
        (ILOpCode.Conv_ovf_u8,    new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(long) }),
        (ILOpCode.Conv_ovf_u8_un, new OpCodeData { flags = OpCodeFlags.CanThrow_Overflow, resultType = typeof(long) }),
        (ILOpCode.Conv_r_un,      new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(double) }),
        (ILOpCode.Conv_r4,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(double) }),
        (ILOpCode.Conv_r8,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(double) }),
        (ILOpCode.Conv_u,         new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(IntPtr) }),
        (ILOpCode.Conv_u1,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (ILOpCode.Conv_u2,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (ILOpCode.Conv_u4,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(int) }),
        (ILOpCode.Conv_u8,        new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(long) }),
        (ILOpCode.Cpblk,          new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (ILOpCode.Cpobj,          new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (ILOpCode.Div,            new OpCodeData { flags = OpCodeFlags.Arithmetic | OpCodeFlags.CanThrow_Arithmetic }),
        (ILOpCode.Div_un,         new OpCodeData { flags = OpCodeFlags.Arithmetic | OpCodeFlags.CanThrow_Arithmetic }),
        (ILOpCode.Dup,            new OpCodeData { flags = OpCodeFlags.PushesInput }),
        (ILOpCode.Initblk,        new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (ILOpCode.Initobj,        new OpCodeData { flags = OpCodeFlags.HasSideEffects, resultType = typeof(void) }),
        (ILOpCode.Isinst,         new OpCodeData { flags = OpCodeFlags.TypeFromOperand }),
        (ILOpCode.Jmp,            new OpCodeData { flags = OpCodeFlags.HasSideEffects }),
        (ILOpCode.Ldarg,          new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Argument }),
        (ILOpCode.Ldarg_0,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Argument | OpCodeFlags.FixedOperand, operand = 0, canonical = ILOpCode.Ldarg }),
        (ILOpCode.Ldarg_1,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Argument | OpCodeFlags.FixedOperand, operand = 1, canonical = ILOpCode.Ldarg }),
        (ILOpCode.Ldarg_2,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Argument | OpCodeFlags.FixedOperand, operand = 2, canonical = ILOpCode.Ldarg }),
        (ILOpCode.Ldarg_3,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Argument | OpCodeFlags.FixedOperand, operand = 3, canonical = ILOpCode.Ldarg }),
        (ILOpCode.Ldarg_s,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Argument, canonical = ILOpCode.Ldarg }),
        (ILOpCode.Ldarga,         new OpCodeData { flags = OpCodeFlags.LoadAddress | OpCodeFlags.Argument }),
        (ILOpCode.Ldarga_s,       new OpCodeData { flags = OpCodeFlags.LoadAddress | OpCodeFlags.Argument, canonical = ILOpCode.Ldarga }),
        (ILOpCode.Ldc_i4,         new OpCodeData { flags = OpCodeFlags.Constant, resultType = typeof(int) }),
        (ILOpCode.Ldc_i4_0,       new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(int), operand = 0, canonical = ILOpCode.Ldc_i4 }),
        (ILOpCode.Ldc_i4_1,       new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(int), operand = 1, canonical = ILOpCode.Ldc_i4 }),
        (ILOpCode.Ldc_i4_2,       new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(int), operand = 2, canonical = ILOpCode.Ldc_i4 }),
        (ILOpCode.Ldc_i4_3,       new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(int), operand = 3, canonical = ILOpCode.Ldc_i4 }),
        (ILOpCode.Ldc_i4_4,       new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(int), operand = 4, canonical = ILOpCode.Ldc_i4 }),
        (ILOpCode.Ldc_i4_5,       new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(int), operand = 5, canonical = ILOpCode.Ldc_i4 }),
        (ILOpCode.Ldc_i4_6,       new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(int), operand = 6, canonical = ILOpCode.Ldc_i4 }),
        (ILOpCode.Ldc_i4_7,       new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(int), operand = 7, canonical = ILOpCode.Ldc_i4 }),
        (ILOpCode.Ldc_i4_8,       new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(int), operand = 8, canonical = ILOpCode.Ldc_i4 }),
        (ILOpCode.Ldc_i4_m1,      new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(int), operand = -1, canonical = ILOpCode.Ldc_i4 }),
        (ILOpCode.Ldc_i4_s,       new OpCodeData { flags = OpCodeFlags.Constant, resultType = typeof(int), canonical = ILOpCode.Ldc_i4 }),
        (ILOpCode.Ldc_i8,         new OpCodeData { flags = OpCodeFlags.Constant, resultType = typeof(long) }),
        (ILOpCode.Ldc_r4,         new OpCodeData { flags = OpCodeFlags.Constant, resultType = typeof(double) }),
        (ILOpCode.Ldc_r8,         new OpCodeData { flags = OpCodeFlags.Constant, resultType = typeof(double) }),
        (ILOpCode.Ldelem,         new OpCodeData { flags = OpCodeFlags.TypeFromOperand | OpCodeFlags.CanThrow_IndexOutOfRange }),
        (ILOpCode.Ldelem_i,       new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange, resultType = typeof(IntPtr) }),
        (ILOpCode.Ldelem_i1,      new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange, resultType = typeof(int) }),
        (ILOpCode.Ldelem_i2,      new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange, resultType = typeof(int) }),
        (ILOpCode.Ldelem_i4,      new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange, resultType = typeof(int) }),
        (ILOpCode.Ldelem_i8,      new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange, resultType = typeof(long) }),
        (ILOpCode.Ldelem_r4,      new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange, resultType = typeof(double) }),
        (ILOpCode.Ldelem_r8,      new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange, resultType = typeof(double) }),
        (ILOpCode.Ldelem_ref,     new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange }),
        (ILOpCode.Ldelem_u1,      new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange, resultType = typeof(int) }),
        (ILOpCode.Ldelem_u2,      new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange, resultType = typeof(int) }),
        (ILOpCode.Ldelem_u4,      new OpCodeData { flags = OpCodeFlags.CanThrow_IndexOutOfRange, resultType = typeof(int) }),
        (ILOpCode.Ldelema,        new OpCodeData { flags = OpCodeFlags.TypeFromOperandRef | OpCodeFlags.CanThrow }),
        (ILOpCode.Ldfld,          new OpCodeData { flags = OpCodeFlags.CanThrow_NullReference }),
        (ILOpCode.Ldflda,         new OpCodeData { flags = OpCodeFlags.CanThrow_NullReference }),
        (ILOpCode.Ldftn,          new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(IntPtr) }),
        (ILOpCode.Ldind_i,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(IntPtr) }),
        (ILOpCode.Ldind_i1,       new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(int) }),
        (ILOpCode.Ldind_i2,       new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(int) }),
        (ILOpCode.Ldind_i4,       new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(int) }),
        (ILOpCode.Ldind_i8,       new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(long) }),
        (ILOpCode.Ldind_r4,       new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(double) }),
        (ILOpCode.Ldind_r8,       new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(double) }),
        (ILOpCode.Ldind_ref,      new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference }),
        (ILOpCode.Ldind_u1,       new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(int) }),
        (ILOpCode.Ldind_u2,       new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(int) }),
        (ILOpCode.Ldind_u4,       new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(int) }),
        (ILOpCode.Ldlen,          new OpCodeData { flags = OpCodeFlags.CanThrow_NullReference, resultType = typeof(IntPtr) }),
        (ILOpCode.Ldloc,          new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Local }),
        (ILOpCode.Ldloc_0,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Local | OpCodeFlags.FixedOperand, operand = 0, canonical = ILOpCode.Ldloc }),
        (ILOpCode.Ldloc_1,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Local | OpCodeFlags.FixedOperand, operand = 1, canonical = ILOpCode.Ldloc }),
        (ILOpCode.Ldloc_2,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Local | OpCodeFlags.FixedOperand, operand = 2, canonical = ILOpCode.Ldloc }),
        (ILOpCode.Ldloc_3,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Local | OpCodeFlags.FixedOperand, operand = 3, canonical = ILOpCode.Ldloc }),
        (ILOpCode.Ldloc_s,        new OpCodeData { flags = OpCodeFlags.Load | OpCodeFlags.Local, canonical = ILOpCode.Ldloc }),
        (ILOpCode.Ldloca,         new OpCodeData { flags = OpCodeFlags.LoadAddress | OpCodeFlags.Local }),
        (ILOpCode.Ldloca_s,       new OpCodeData { flags = OpCodeFlags.LoadAddress | OpCodeFlags.Local, canonical = ILOpCode.Ldloca }),
        (ILOpCode.Ldnull,         new OpCodeData { flags = OpCodeFlags.Constant | OpCodeFlags.FixedOperand, resultType = typeof(TypeLattice.NullPtr), operand = 0 }),
        (ILOpCode.Ldobj,          new OpCodeData { flags = OpCodeFlags.TypeFromOperand | OpCodeFlags.CanThrow_NullReference }),
        (ILOpCode.Ldsfld,         new OpCodeData { flags = OpCodeFlags.Default }),
        (ILOpCode.Ldsflda,        new OpCodeData { flags = OpCodeFlags.Default }),
        (ILOpCode.Ldstr,          new OpCodeData { flags = OpCodeFlags.Constant, resultType = typeof(string) }),
        (ILOpCode.Ldtoken,        new OpCodeData { flags = OpCodeFlags.Default }),
        (ILOpCode.Ldvirtftn,      new OpCodeData { flags = OpCodeFlags.CanThrow_NullReference, resultType = typeof(IntPtr) }),
        (ILOpCode.Leave,          new OpCodeData { flags = OpCodeFlags.Default }),
        (ILOpCode.Leave_s,        new OpCodeData { flags = OpCodeFlags.Default, canonical = ILOpCode.Leave }),
        (ILOpCode.Localloc,       new OpCodeData { flags = OpCodeFlags.CanThrow_OutOfMemory, resultType = typeof(IntPtr) }),
        (ILOpCode.Mkrefany,       new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(TypedReference) }),
        (ILOpCode.Mul,            new OpCodeData { flags = OpCodeFlags.Arithmetic }),
        (ILOpCode.Mul_ovf,        new OpCodeData { flags = OpCodeFlags.Arithmetic | OpCodeFlags.CanThrow_Overflow }),
        (ILOpCode.Mul_ovf_un,     new OpCodeData { flags = OpCodeFlags.Arithmetic | OpCodeFlags.CanThrow_Overflow }),
        (ILOpCode.Neg,            new OpCodeData { flags = OpCodeFlags.Arithmetic }),
        (ILOpCode.Newarr,         new OpCodeData { flags = OpCodeFlags.CanThrow_OutOfMemory | OpCodeFlags.CanThrow_Overflow }),
        (ILOpCode.Newobj,         new OpCodeData { flags = OpCodeFlags.CanThrow_OutOfMemory | OpCodeFlags.HasSideEffects }),
        (ILOpCode.Nop,            new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(void) }),
        (ILOpCode.Not,            new OpCodeData { flags = OpCodeFlags.Arithmetic }),
        (ILOpCode.Or,             new OpCodeData { flags = OpCodeFlags.Arithmetic }),
        (ILOpCode.Pop,            new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(void) }),
        (ILOpCode.Refanytype,     new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(RuntimeTypeHandle) }),
        (ILOpCode.Refanyval,      new OpCodeData { flags = OpCodeFlags.TypeFromOperandRef | OpCodeFlags.CanThrow_InvalidCast }),
        (ILOpCode.Rem,            new OpCodeData { flags = OpCodeFlags.Arithmetic | OpCodeFlags.CanThrow_Arithmetic }),
        (ILOpCode.Rem_un,         new OpCodeData { flags = OpCodeFlags.Arithmetic | OpCodeFlags.CanThrow_Arithmetic }),
        (ILOpCode.Shl,            new OpCodeData { flags = OpCodeFlags.Shift }),
        (ILOpCode.Shr,            new OpCodeData { flags = OpCodeFlags.Shift }),
        (ILOpCode.Shr_un,         new OpCodeData { flags = OpCodeFlags.Shift }),
        (ILOpCode.Sizeof,         new OpCodeData { flags = OpCodeFlags.Default, resultType = typeof(IntPtr) }),
        (ILOpCode.Starg,          new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Argument, resultType = typeof(void) }),
        (ILOpCode.Starg_s,        new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Argument, resultType = typeof(void), canonical = ILOpCode.Starg }),
        (ILOpCode.Stelem,         new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow, resultType = typeof(void) }),
        (ILOpCode.Stelem_i,       new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow, resultType = typeof(void) }),
        (ILOpCode.Stelem_i1,      new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow, resultType = typeof(void) }),
        (ILOpCode.Stelem_i2,      new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow, resultType = typeof(void) }),
        (ILOpCode.Stelem_i4,      new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow, resultType = typeof(void) }),
        (ILOpCode.Stelem_i8,      new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow, resultType = typeof(void) }),
        (ILOpCode.Stelem_r4,      new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow, resultType = typeof(void) }),
        (ILOpCode.Stelem_r8,      new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow, resultType = typeof(void) }),
        (ILOpCode.Stelem_ref,     new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow, resultType = typeof(void) }),
        (ILOpCode.Stfld,          new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (ILOpCode.Stind_i,        new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (ILOpCode.Stind_i1,       new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (ILOpCode.Stind_i2,       new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (ILOpCode.Stind_i4,       new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (ILOpCode.Stind_i8,       new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (ILOpCode.Stind_r4,       new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (ILOpCode.Stind_r8,       new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (ILOpCode.Stind_ref,      new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Indirect | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (ILOpCode.Stloc,          new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Local, resultType = typeof(void) }),
        (ILOpCode.Stloc_0,        new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Local | OpCodeFlags.FixedOperand, resultType = typeof(void), operand = 0, canonical = ILOpCode.Stloc }),
        (ILOpCode.Stloc_1,        new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Local | OpCodeFlags.FixedOperand, resultType = typeof(void), operand = 1, canonical = ILOpCode.Stloc }),
        (ILOpCode.Stloc_2,        new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Local | OpCodeFlags.FixedOperand, resultType = typeof(void), operand = 2, canonical = ILOpCode.Stloc }),
        (ILOpCode.Stloc_3,        new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Local | OpCodeFlags.FixedOperand, resultType = typeof(void), operand = 3, canonical = ILOpCode.Stloc }),
        (ILOpCode.Stloc_s,        new OpCodeData { flags = OpCodeFlags.Store | OpCodeFlags.Local, resultType = typeof(void), canonical = ILOpCode.Stloc }),
        (ILOpCode.Stobj,          new OpCodeData { flags = OpCodeFlags.HasSideEffects | OpCodeFlags.CanThrow_NullReference, resultType = typeof(void) }),
        (ILOpCode.Stsfld,         new OpCodeData { flags = OpCodeFlags.HasSideEffects, resultType = typeof(void) }),
        (ILOpCode.Sub,            new OpCodeData { flags = OpCodeFlags.Arithmetic }),
        (ILOpCode.Sub_ovf,        new OpCodeData { flags = OpCodeFlags.Arithmetic | OpCodeFlags.CanThrow_Overflow }),
        (ILOpCode.Sub_ovf_un,     new OpCodeData { flags = OpCodeFlags.Arithmetic | OpCodeFlags.CanThrow_Overflow }),
        (ILOpCode.Unbox,          new OpCodeData { flags = OpCodeFlags.TypeFromOperandRef | OpCodeFlags.CanThrow_NullReference | OpCodeFlags.CanThrow_InvalidCast }),
        (ILOpCode.Unbox_any,      new OpCodeData { flags = OpCodeFlags.TypeFromOperand | OpCodeFlags.CanThrow_NullReference | OpCodeFlags.CanThrow_InvalidCast }),
        (ILOpCode.Xor,            new OpCodeData { flags = OpCodeFlags.Arithmetic }),
        // @formatter:on
    ];
}
