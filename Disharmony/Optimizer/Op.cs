namespace Disharmony.Optimizer;

/// <summary>
///     Represents an abstract operation.
/// </summary>
/// <remarks>
///     Unlike CIL instructions, <see cref="Op" />s are represented as a tree, where each operation has multiple
///     inputs and a single output. Assignments to variables, including arguments, locals, and stack slots, are represented
///     explicitly. Operations which might throw are only allowed at top level or as the input to an
///     <see cref="AssignmentOp" /> or <see cref="Branch" />.
/// </remarks>
/// <param name="Type">The type of value produced by the <see cref="Op" />.</param>
internal abstract record Op(Type Type) : Node;

/// <summary>
///     Represents an assignment to a <see cref="Variable" />.
/// </summary>
/// <param name="Output">The <see cref="Variable" /> that receives the value.</param>
/// <param name="Input">The <see cref="Op" /> that produces the value.</param>
internal sealed record AssignmentOp(Variable Output, Op Input) : Op(typeof(void))
{
    public override string ToString() => $"{Output} := {Input}";
}

/// <summary>
///     Represents a potentially bit-changing conversion, such as integer widening or narrowing.
/// </summary>
/// <remarks>
///     <para>
///         <c>ConversionOp</c> is generated when promoting locals and arguments that have a different
///         storage type from their stack type.
///     </para>
///     <para>
///         Conversions between different managed reference types (for example, a class and a subclass)
///         don't use <c>ConversionOp</c>.
///     </para>
/// </remarks>
/// <param name="Input"></param>
/// <param name="Type"></param>
internal sealed record ConversionOp(Op Input, Type Type) : Op(Type)
{
    public override string ToString() => $".convert ({Input}) :{Type}";

    public bool IsNarrowing =>
        System.Runtime.InteropServices.Marshal.SizeOf(Type) < System.Runtime.InteropServices.Marshal.SizeOf(Input.Type);
}

/// <summary>
///     Represents an IL prefix.
/// </summary>
/// <param name="OpCode">The prefix opcode.</param>
/// <param name="Operand">The prefix operand.</param>
internal sealed record Prefix(OpCode OpCode, object? Operand);

/// <summary>
///     Represents an IL instruction.
/// </summary>
/// <param name="OpCode"></param>
/// <param name="Operand"></param>
/// <param name="Prefixes"></param>
internal sealed record ILInstruction(OpCode OpCode, object? Operand, IReadOnlyList<Prefix> Prefixes)
{
    private string OperandDescription(object operand)
    {
        var data = OpCodeData.Get(OpCode);
        return operand switch
        {
            LocalBuilder b => $"Local{b.LocalIndex} :{b.LocalType}",
            _ when (data.flags & OpCodeFlags.Argument) != 0 => $"Arg{OpCodeData.GetIntOperand(this)}",
            _ when (data.flags & OpCodeFlags.Local) != 0 => $"Local{OpCodeData.GetIntOperand(this)}",
            _ => operand.ToString(),
        };
    }

    public override string ToString()
    {
        if (Operand != null)
            return $"{OpCode} {{{OperandDescription(Operand)}}}";
        return $"{OpCode}";
    }

    public bool Equals(ILInstruction? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return OpCode.Equals(other.OpCode) && Equals(Operand, other.Operand) && Prefixes.SequenceEqual(other.Prefixes);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = OpCode.GetHashCode();
            hashCode = (hashCode * 397) ^ (Operand != null ? Operand.GetHashCode() : 0);
            foreach (var prefix in Prefixes)
                hashCode = (hashCode * 397) ^ prefix.GetHashCode();
            return hashCode;
        }
    }
}

/// <summary>
///     Represents an IL operation.
/// </summary>
/// <remarks>
///     There is one input per stack slot popped by the IL opcode, and the output type is the type of the value pushed by
///     the IL opcode,
///     or <see langword="void" /> if it does not push a value. Numeric values use <see langword="int" />,
///     <see langword="long" />,
///     <see cref="IntPtr" />, or <see langword="double" /> to represent the IL stack types.
/// </remarks>
/// <param name="Inputs">The <see cref="Op" />s that produce the instruction's stack inputs.</param>
/// <param name="Type">The type of value produced by the instruction.</param>
internal sealed record ILOp(ILInstruction IL, IReadOnlyList<Op> Inputs, Type Type) : Op(Type)
{
    public override string ToString() => Inputs.Count > 0 ? $"{IL} ({string.Join(", ", Inputs)}) :{Type}" : $"{IL} :{Type}";

    public bool Equals(ILOp? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return base.Equals(other) && IL.Equals(other.IL) && Inputs.SequenceEqual(other.Inputs);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = base.GetHashCode();
            hashCode = (hashCode * 397) ^ IL.GetHashCode();
            foreach (var input in Inputs)
                hashCode = (hashCode * 397) ^ input.GetHashCode();
            return hashCode;
        }
    }
}

/// <summary>
///     Represents a variable which can store a value during execution, including <see cref="Local" />s,
///     <see cref="Argument" />s, <see cref="StackSlot" />s, and <see cref="Temporary" /> variables.
/// </summary>
/// <param name="Type">The type of value stored by the <see cref="Variable" />.</param>
internal abstract record Variable(Type Type) : Op(Type);

/// <summary>
///     Represents a stack slot from the incoming IL.
/// </summary>
/// <param name="Depth">The stack depth, where zero is the bottom of the stack.</param>
/// <param name="Type">The type of value stored in the slot.</param>
internal sealed record StackSlot(int Depth, Type Type, int Id) : Variable(Type)
{
    public override string ToString() => $"Stack{Id}_{Depth} :{Type}";
}

/// <summary>
///     Base class for <see cref="Argument" /> and <see cref="Local" /> variables.
/// </summary>
/// <param name="Type">The type of value stored by the variable.</param>
internal abstract record MemoryVariable(Type Type) : Variable(Type)
{
    public abstract int Index { get; }
}

/// <summary>
///     Represents an IL argument.
/// </summary>
/// <param name="Index">The argument index.</param>
/// <param name="Type">The argument type.</param>
internal sealed record Argument(int Index, Type Type) : MemoryVariable(Type)
{
    public override int Index { get; } = Index;

    public override string ToString() => $"Arg{Index} :{Type}";
}

/// <summary>
///     Represents an IL local.
/// </summary>
/// <remarks>
///     The type may be <see cref="TypeLattice.AnyType" /> if the method's metadata does not specify a type for the local.
///     In this
///     case, the local has to be optimized conservatively, retaining all loads and stores.
/// </remarks>
internal sealed record Local : MemoryVariable
{
    public override int Index => Tracker.Index;

    public Local(LocalBuilder builder) : this(builder.LocalType ?? TypeLattice.Any, new LocalTrackerBuilder(builder)) { }
    public Local(Type type, int index) : this(type, new LocalTrackerIndex(index)) { }
    public Local(LocalVariableInfo info) : this(info.LocalType ?? TypeLattice.Any, new LocalTrackerIndex(info.LocalIndex)) { }

    /// <summary>
    ///     Represents an IL local.
    /// </summary>
    /// <remarks>
    ///     The type may be <see cref="TypeLattice.AnyType" /> if the method's metadata does not specify a type for the local.
    ///     In this
    ///     case, the local has to be optimized conservatively, retaining all loads and stores.
    /// </remarks>
    /// <param name="Type">The local type.</param>
    /// <param name="Tracker"></param>
    private Local(Type Type, LocalTracker Tracker) : base(Type)
    {
        this.Tracker = Tracker;
    }

    public LocalTracker Tracker { get; }

    public override string ToString() => $"Local{Index} :{Type}";
}

/// <summary>
///     Represents a temporary variable created during optimization.
/// </summary>
/// <remarks>
///     This might be something that didn't exist in the original IL, or a replacement for a <see cref="StackSlot" /> or
///     <see cref="Local" /> that was removed
///     during optimization but needs to be recreated during IL emission.
/// </remarks>
/// <param name="Type">The type of value stored by the temporary.</param>
internal sealed record Temporary(Type Type, int Id) : Variable(Type)
{
    public override string ToString() => $"Temp{Id} :{Type}";
}

/// <summary>
///     Represents a lack of value, such as the return value of a void method.
/// </summary>
internal sealed record VoidOp() : Op(typeof(void))
{
    public override string ToString() => "Void";
}
