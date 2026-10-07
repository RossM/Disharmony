using Disharmony.Optimizer;

namespace Disharmony.Utilities;

// This was originally intended to just be a way of keeping track of the LocalBuilder associated
// with a local if we have one, but it turns out to dramatically simplify code dealing with locals.

/// <summary>
///     Abstract base class representing a local variable where we may or may not have a LocalBuilder and
///     corresponding type information.
/// </summary>
internal abstract record LocalTracker
{
    public abstract int Index { get; }

    /// <summary>
    ///     Gets an appropriate LocalTracker for the local referenced in an instruction.
    /// </summary>
    /// <param name="instruction"></param>
    /// <returns></returns>
    public static LocalTracker From(CodeInstruction instruction)
    {
        return instruction.operand is LocalBuilder builder
            ? new LocalTrackerBuilder(builder)
            : new LocalTrackerIndex(OpCodeData.GetIntOperand(instruction));
    }

    public static LocalTracker From(ILInstruction instruction)
    {
        return instruction.Operand is LocalBuilder builder
            ? new LocalTrackerBuilder(builder)
            : new LocalTrackerIndex(OpCodeData.GetIntOperand(instruction));
    }

    /// <summary>
    ///     Gives the same result as <c>LocalTracker.From(instruction).Index</c>, but without creating a LocalTracker object.
    /// </summary>
    /// <param name="instruction"></param>
    /// <returns></returns>
    public static int IndexFrom(CodeInstruction instruction)
    {
        return instruction.operand is LocalBuilder builder
            ? builder.LocalIndex
            : OpCodeData.GetIntOperand(instruction);
    }

    public static int IndexFrom(ILInstruction instruction)
    {
        return instruction.Operand is LocalBuilder builder
            ? builder.LocalIndex
            : OpCodeData.GetIntOperand(instruction);
    }

    /// <summary>
    ///     Gets a CodeInstruction that stores the top of the stack into this local variable.
    /// </summary>
    /// <returns></returns>
    public abstract CodeInstruction Store();

    /// <summary>
    ///     Gets a CodeInstruction that loads this local variable onto the stack, or its address if useAddress is true.
    /// </summary>
    /// <param name="useAddress"></param>
    /// <returns></returns>
    public abstract CodeInstruction Load(bool useAddress = false);
}

/// <summary>
///     Represents a local variable that has a LocalBuilder available.
/// </summary>
/// <param name="Builder"></param>
internal record LocalTrackerBuilder(LocalBuilder Builder) : LocalTracker
{
    public override int Index => Builder.LocalIndex;
    public Type Type => Builder.LocalType;

    public override CodeInstruction Store() =>
        new(Index <= byte.MaxValue ? OpCodes.Stloc_S : OpCodes.Stloc, Builder);

    public override CodeInstruction Load(bool useAddress = false) => useAddress
        ? new(Index <= byte.MaxValue ? OpCodes.Ldloca_S : OpCodes.Ldloca, Builder)
        : new(Index <= byte.MaxValue ? OpCodes.Ldloc_S : OpCodes.Ldloc, Builder);
}

/// <summary>
///     Represents a local variable by its index.
/// </summary>
/// <param name="Index"></param>
internal record LocalTrackerIndex(int Index) : LocalTracker
{
    public override int Index { get; } = Index;

    public override CodeInstruction Store() => CodeInstruction.StoreLocal(Index);

    public override CodeInstruction Load(bool useAddress = false) => CodeInstruction.LoadLocal(Index, useAddress);
}
