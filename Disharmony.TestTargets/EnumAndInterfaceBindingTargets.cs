namespace Disharmony.Tests;

public enum BindingEnum : long
{
    Original = 0x1_0000_0000,
    Replacement = 0x2_0000_0000,
}

public interface IBindingInterface
{
    int Value { get; }
}

public sealed class BindingInterfaceValue(int value) : IBindingInterface
{
    public int Value { get; } = value;
}

public sealed class EnumAndInterfaceBindingTargets : IBindingInterface
{
    public BindingEnum EnumField = BindingEnum.Original;
    public IBindingInterface InterfaceField = new BindingInterfaceValue(1);
    public BindingInterfaceValue ConcreteInterfaceField = new(1);

    public int Value { get; set; }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Void() { }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static BindingEnum EnumIdentity(BindingEnum value) => value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static IBindingInterface InterfaceIdentity(IBindingInterface value) => value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static BindingInterfaceValue ConcreteInterfaceIdentity(BindingInterfaceValue value) => value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static BindingEnum EnumResult() => BindingEnum.Original;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static IBindingInterface InterfaceResult() => new BindingInterfaceValue(1);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static BindingInterfaceValue ConcreteInterfaceResult() => new(1);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public BindingEnum BoundEnumMethod(BindingEnum value) => value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public IBindingInterface BoundInterfaceMethod(IBindingInterface value) => value;
}

public static class EnumAndInterfaceCapturedVariableTargets
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static BindingEnum EnumCapturedVariable(BindingEnum value)
    {
        BindingEnum captured = value;

        [MethodImpl(MethodImplOptions.NoInlining)]
        BindingEnum LocalMethod() => captured;

        return LocalMethod();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static IBindingInterface InterfaceCapturedVariable(IBindingInterface value)
    {
        IBindingInterface captured = value;

        [MethodImpl(MethodImplOptions.NoInlining)]
        IBindingInterface LocalMethod() => captured;

        return LocalMethod();
    }
}
