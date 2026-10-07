using System.Globalization;
// ReSharper disable TypeWithSuspiciousEqualityIsUsedInRecord.Global

namespace Disharmony;

/// <summary>
///     Class representing a possible patch method or patch target.
/// </summary>
/// <remarks>
///     <para>
///         Invocation represents a very generic idea of an IL instruction that takes zero or more inputs and
///         produces a result.
///         Method calls are represented with <see cref="MethodInvocation" />, and field references are represented with
///         <see cref="GetFieldInvocation" />.
///         For convenience there is also a singleton <see cref="EmptyInvocation" /> which represents something that takes
///         no inputs and returns void, intended for use instead of <see langword="null" /> when there is no actual value.
///     </para>
///     <para>
///         All subclasses of Invocation must implement value semantics, so that two invocations that represent calling the
///         same function or accessing the same field compare equal.
///     </para>
/// </remarks>
internal abstract record Invocation
{
    public virtual bool HasThis => !IsStatic;

    /// <summary>
    ///     A string for use as the parameter name of the instance argument of a non-static invocation.
    ///     Users should <b>not</b> depend on this value! It is only for use in error messages and so on.
    /// </summary>
    protected const string InstanceParameterName = "<instance>";

    public abstract string FullName { get; }
    public abstract Type ReturnType { get; }
    public abstract Type[] ParameterTypes { get; }
    public abstract bool IsStatic { get; }
    public abstract string[] ParameterNames { get; }

    /// <summary>
    ///     If <see cref="IsStatic" /> is false, this returns the type of the instance parameter to this invocation.
    ///     If <see cref="IsStatic" /> is true, its behavior is subclass-specific.
    /// </summary>
    public abstract Type InstanceType { get; }

    protected abstract CodeInstruction GetCodeInstruction();

    public virtual IEnumerable<CodeInstruction> GetCodeInstructions() => [GetCodeInstruction()];

    public override string ToString() => $"[{GetType().FullName}({FullName})]";
}

internal record EmptyInvocation : Invocation
{
    public override string FullName => "";

    public override Type ReturnType => typeof(void);

    public override Type[] ParameterTypes => [];

    public override bool IsStatic => true;

    public override string[] ParameterNames => [];

    public override Type InstanceType => typeof(void);

    public static readonly EmptyInvocation Instance = new();

    private EmptyInvocation() { }

    protected override CodeInstruction GetCodeInstruction() => throw new NotSupportedException();
    public override IEnumerable<CodeInstruction> GetCodeInstructions() => [];
}

internal abstract record FieldInvocation(FieldInfo FieldInfo) : Invocation
{
    public override string FullName => FieldInfo.FullName;
    public override bool IsStatic => FieldInfo.IsStatic;
    public override Type InstanceType => FieldInfo.DeclaringType ?? throw new NotSupportedException();
}

internal record GetFieldInvocation(FieldInfo FieldInfo) : FieldInvocation(FieldInfo)
{
    public override Type ReturnType => FieldInfo.FieldType;
    public override Type[] ParameterTypes => field ??= FieldInfo.IsStatic ? [] : [FieldInfo.DeclaringType!];
    public override string[] ParameterNames => field ??= FieldInfo.IsStatic ? [] : [InstanceParameterName];

    protected override CodeInstruction GetCodeInstruction() => new(FieldInfo.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, FieldInfo);

    public virtual bool Equals(GetFieldInvocation? other) => base.Equals(other);

    public override int GetHashCode() => base.GetHashCode();
}

internal record SetFieldInvocation(FieldInfo FieldInfo) : FieldInvocation(FieldInfo)
{
    public override Type ReturnType => typeof(void);

    public override Type[] ParameterTypes =>
        field ??= FieldInfo.IsStatic ? [FieldInfo.FieldType] : [FieldInfo.DeclaringType!.CallableType, FieldInfo.FieldType];

    public override string[] ParameterNames => field ??= FieldInfo.IsStatic ? [ValueFieldName] : [InstanceParameterName, ValueFieldName];
    public const string ValueFieldName = "value";

    protected override CodeInstruction GetCodeInstruction() => new(FieldInfo.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, FieldInfo);

    public virtual bool Equals(SetFieldInvocation? other) => base.Equals(other);

    public override int GetHashCode() => base.GetHashCode();
}

internal abstract record MethodBaseInvocation : Invocation
{
    public abstract MethodBase MethodBase { get; }
}

internal record MethodInvocation(MethodInfo MethodInfo) : MethodBaseInvocation
{
    public override string FullName => MethodInfo.FullName;
    public override Type ReturnType => MethodInfo.ReturnType;
    public override bool IsStatic => MethodInfo.IsStatic;
    public override bool HasThis => MethodInfo.HasThis;
    public override Type InstanceType => MethodInfo.DeclaringType ?? throw new NotSupportedException();

    public override MethodBase MethodBase => MethodInfo;

    public override Type[] ParameterTypes => field ??=
        MethodInfo.HasThis
            ? [MethodInfo.DeclaringType!.CallableType, .. MethodInfo.GetParameters().Select(p => p.ParameterType)]
            : [.. MethodInfo.GetParameters().Select(p => p.ParameterType)];

    public override string[] ParameterNames => field ??=
        MethodInfo.HasThis
            ? [InstanceParameterName, .. MethodInfo.GetParameters().Select(p => p.Name)]
            : [.. MethodInfo.GetParameters().Select(p => p.Name)];

    protected override CodeInstruction GetCodeInstruction() => new(MethodInfo.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, MethodInfo);

    public virtual bool Equals(MethodInvocation? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return base.Equals(other) && MethodInfo.Equals(other.MethodInfo);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = base.GetHashCode();
            hashCode = (hashCode * 397) ^ MethodInfo.GetHashCode();
            return hashCode;
        }
    }
}

internal abstract record ConstructorInvocation(ConstructorInfo ConstructorInfo) : MethodBaseInvocation
{
    public override string FullName => ConstructorInfo.FullName;
    public override Type InstanceType => ConstructorInfo.DeclaringType ?? throw new NotSupportedException();
    public override MethodBase MethodBase => ConstructorInfo;
}

/// <summary>
///     This class represents a <see cref="OpCodes.Newobj" /> call of a constructor.
/// </summary>
/// <param name="ConstructorInfo"></param>
internal record InnerConstructorInvocation(ConstructorInfo ConstructorInfo) : ConstructorInvocation(ConstructorInfo)
{
    public override Type ReturnType => ConstructorInfo.DeclaringType!;
    public override Type[] ParameterTypes => field ??= [.. ConstructorInfo.GetParameters().Select(p => p.ParameterType)];

    public override bool IsStatic => true;
    public override string[] ParameterNames => field ??= [.. ConstructorInfo.GetParameters().Select(p => p.Name)];

    protected override CodeInstruction GetCodeInstruction() => new(OpCodes.Newobj, ConstructorInfo);

    public virtual bool Equals(InnerConstructorInvocation? other) => base.Equals(other);
    public override int GetHashCode() => base.GetHashCode();
}

/// <summary>
///     This class represents a constructor call as seen from inside the constructor itself, where it
///     functions like an ordinary instance method returning void.
/// </summary>
/// <param name="ConstructorInfo"></param>
internal record OuterConstructorInvocation(ConstructorInfo ConstructorInfo) : ConstructorInvocation(ConstructorInfo)
{
    public override Type ReturnType => typeof(void);
    public override bool IsStatic => ConstructorInfo.IsStatic;
    public override bool HasThis => ConstructorInfo.HasThis;

    public override Type[] ParameterTypes => field ??=
        ConstructorInfo.HasThis
            ? [ConstructorInfo.DeclaringType!.CallableType, .. ConstructorInfo.GetParameters().Select(p => p.ParameterType)]
            : [.. ConstructorInfo.GetParameters().Select(p => p.ParameterType)];

    public override string[] ParameterNames => field ??=
        ConstructorInfo.HasThis
            ? [InstanceParameterName, .. ConstructorInfo.GetParameters().Select(p => p.Name)]
            : [.. ConstructorInfo.GetParameters().Select(p => p.Name)];

    protected override CodeInstruction GetCodeInstruction() => throw new NotSupportedException();

    public virtual bool Equals(OuterConstructorInvocation? other) => base.Equals(other);
    public override int GetHashCode() => base.GetHashCode();
}

internal abstract record ConstantInvocation : Invocation
{
    public override bool IsStatic => true;
    public override Type InstanceType => typeof(void);

    public override Type[] ParameterTypes => [];

    public override string[] ParameterNames => [];
}

internal record ConstantIntInvocation(int Value) : ConstantInvocation
{
    public override string FullName => Value.ToString();
    public override Type ReturnType => typeof(int);

    protected override CodeInstruction GetCodeInstruction() => Value switch
    {
        -1 => new(OpCodes.Ldc_I4_M1),
        0 => new(OpCodes.Ldc_I4_0),
        1 => new(OpCodes.Ldc_I4_1),
        2 => new(OpCodes.Ldc_I4_2),
        3 => new(OpCodes.Ldc_I4_3),
        4 => new(OpCodes.Ldc_I4_4),
        5 => new(OpCodes.Ldc_I4_5),
        6 => new(OpCodes.Ldc_I4_6),
        7 => new(OpCodes.Ldc_I4_7),
        8 => new(OpCodes.Ldc_I4_8),
        >= -128 and <= 127 => new(OpCodes.Ldc_I4_S, Value),
        _ => new(OpCodes.Ldc_I4, Value),
    };
}

internal record ConstantLongInvocation(long Value) : ConstantInvocation
{
    public override string FullName => Value.ToString();
    public override Type ReturnType => typeof(long);

    protected override CodeInstruction GetCodeInstruction() => new(OpCodes.Ldc_I8, Value);
}

internal record ConstantStringInvocation(string Value) : ConstantInvocation
{
    public override string FullName => $"\"{Value}\"";
    public override Type ReturnType => typeof(string);

    protected override CodeInstruction GetCodeInstruction() => new(OpCodes.Ldstr, Value);
}

internal record ConstantFloatInvocation(float Value) : ConstantInvocation
{
    public override string FullName => Value.ToString(CultureInfo.InvariantCulture);
    public override Type ReturnType => typeof(float);

    protected override CodeInstruction GetCodeInstruction() => new(OpCodes.Ldc_R4, Value);
}

internal record ConstantDoubleInvocation(double Value) : ConstantInvocation
{
    public override string FullName => Value.ToString(CultureInfo.InvariantCulture);
    public override Type ReturnType => typeof(double);

    protected override CodeInstruction GetCodeInstruction() => new(OpCodes.Ldc_R8, Value);
}
