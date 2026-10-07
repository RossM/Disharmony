namespace Disharmony.Tests.Support;

internal sealed record MockInvocation(
    Type InstanceType,
    Type ReturnType,
    Type[] ParameterTypes,
    string[] ParameterNames,
    bool IsStatic)
    : Invocation
{
    public override string FullName => nameof(MockInvocation);
    public override Type InstanceType { get; } = InstanceType;
    public override Type ReturnType { get; } = ReturnType;
    public override Type[] ParameterTypes { get; } = ParameterTypes;
    public override string[] ParameterNames { get; } = ParameterNames;
    public override bool IsStatic { get; } = IsStatic;
    protected override CodeInstruction GetCodeInstruction() => new(OpCodes.Nop);
}
