namespace Disharmony.Optimizer;

internal static class OpcodeUtilities
{
    /// <summary>
    ///     Gets the output type of a CIL operation, given its input types.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Numeric results should be normalized to the corresponding CIL stack type.
    ///     </para>
    ///     <para>
    ///         For operations with the <see cref="OpCodeFlags.Argument" /> or <see cref="OpCodeFlags.Local" /> flags,
    ///         the type of the corresponding argument or local should be the first element of <paramref name="inputTypes" />,
    ///         with the types of values popped from the stack following it.
    ///     </para>
    /// </remarks>
    /// <param name="op"></param>
    /// <param name="inputTypes"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static Type GetOutputType(ILInstruction op, params Type[] inputTypes)
    {
        Type outputType = GetOutputTypeCore(op.OpCode, op.Operand, inputTypes);
        return GetStackType(outputType);
    }

    public static Type GetOutputType(OpCode opcode, object? operand, params Type[] inputTypes)
    {
        Type outputType = GetOutputTypeCore(opcode, operand, inputTypes);
        return GetStackType(outputType);
    }

    public static Type GetStackType(Type type)
    {
        if (type.IsEnum)
            type = type.GetEnumUnderlyingType();

        return Type.GetTypeCode(type) switch
        {
            >= TypeCode.Boolean and <= TypeCode.UInt32 => typeof(int),
            TypeCode.Int64 or TypeCode.UInt64 => typeof(long),
            TypeCode.Single or TypeCode.Double => typeof(double),
            _ when type == typeof(IntPtr) || type == typeof(UIntPtr) => typeof(IntPtr),
            _ => type,
        };
    }

    /// <summary>
    ///     Determines whether storing a value from the stack to a local of this type, or
    ///     vice versa, could change the bits of the value.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool RequiresConversion(Type type)
    {
        return Type.GetTypeCode(type) switch
        {
            >= TypeCode.Boolean and <= TypeCode.UInt16 => true,
            TypeCode.Single or TypeCode.Double => true,
            _ => false,
        };
    }

    private static Type GetOutputTypeCore(OpCode opcode, object? operand, Type[] inputTypes)
    {
        OpCodeData data = OpCodeData.Get(opcode);
        if (data.resultType is { } resultType)
            return resultType;

        if ((data.flags & (OpCodeFlags.Load | OpCodeFlags.Indirect)) == (OpCodeFlags.Load | OpCodeFlags.Indirect))
        {
            if (inputTypes[0] == TypeLattice.Null || inputTypes[0] == TypeLattice.Unknown || inputTypes[0] == TypeLattice.UnknownRef)
                return TypeLattice.Unknown;
            if (inputTypes[0] == TypeLattice.Any || inputTypes[0] == TypeLattice.AnyRef || inputTypes[0] == typeof(IntPtr))
                return typeof(object);
            return inputTypes[0].GetElementType() ?? typeof(object);
        }

        if ((data.flags & (OpCodeFlags.Load | OpCodeFlags.Shift | OpCodeFlags.PushesInput)) != 0)
            return inputTypes[0];
        if ((data.flags & OpCodeFlags.LoadAddress) != 0)
            return inputTypes[0].MakeByRefType();

        // See comments on OpCodeFlags.Arithmetic
        if ((data.flags & OpCodeFlags.Arithmetic) != 0)
        {
            if (inputTypes.Length == 1)
                return inputTypes[0];

            // Double can only combine with another double, so we know both operands must be double
            if (inputTypes.Contains(typeof(double)))
                return typeof(double);

            if (inputTypes.Contains(TypeLattice.Unknown))
                return TypeLattice.Unknown;
            if (inputTypes.Contains(TypeLattice.Any))
                return TypeLattice.Any;

            if (!inputTypes[0].IsValueType && !inputTypes[1].IsValueType)
                return typeof(IntPtr);
            if (!inputTypes[0].IsValueType)
                return inputTypes[0];
            if (!inputTypes[1].IsValueType)
                return inputTypes[1];

            if (inputTypes.Contains(typeof(IntPtr)))
                return typeof(IntPtr);

            return inputTypes[0];
        }

        if ((data.flags & OpCodeFlags.TypeFromOperand) != 0 && operand is Type operandType)
            return operandType;
        if ((data.flags & OpCodeFlags.TypeFromOperandRef) != 0 && operand is Type operandType2)
            return operandType2.MakeByRefType();

        return data.canonical switch
        {
            OpCodeValues.Call or OpCodeValues.Callvirt when operand is MethodInfo method => method.ReturnType,
            OpCodeValues.Call when operand is ConstructorInfo => typeof(void),
            OpCodeValues.Calli => (Type)HarmonyInterface.InlineSignature_ReturnType(operand!),
            OpCodeValues.Ldelem_Ref when inputTypes[0].IsArray => inputTypes[0].GetElementType()!,
            OpCodeValues.Ldelem_Ref when inputTypes[0] == TypeLattice.Unknown || inputTypes[0] == TypeLattice.Null => TypeLattice.Unknown,
            OpCodeValues.Ldelem_Ref => typeof(object),
            OpCodeValues.Ldfld or OpCodeValues.Ldsfld when operand is FieldInfo field => field.FieldType,
            OpCodeValues.Ldflda or OpCodeValues.Ldsflda when operand is FieldInfo field => field.FieldType.MakeByRefType(),
            OpCodeValues.Ldtoken when operand is FieldInfo => typeof(RuntimeFieldHandle),
            OpCodeValues.Ldtoken when operand is MethodBase => typeof(RuntimeMethodHandle),
            OpCodeValues.Ldtoken when operand is Type => typeof(RuntimeTypeHandle),
            OpCodeValues.Newarr when operand is Type type => type.MakeArrayType(),
            OpCodeValues.Newobj when operand is ConstructorInfo constructor => constructor.DeclaringType!,
            _ => throw new NotImplementedException($"{opcode} ({operand?.GetType()}) is not implemented"),
        };
    }
}
