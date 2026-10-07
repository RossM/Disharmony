namespace Disharmony.Tests;

public abstract class BoundaryAbstractTargets
{
    public abstract int AbstractMethod(int value);
}

public interface IBoundaryInterfaceTargets
{
    int InterfaceMethod(int value);
}

public sealed class BoundaryExplicitInterfaceTargets : IBoundaryInterfaceTargets
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    int IBoundaryInterfaceTargets.InterfaceMethod(int value) => value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public int CallInterfaceMethod(int value) => ((IBoundaryInterfaceTargets)this).InterfaceMethod(value);
}

public static class BoundaryGenericTargets<T>
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int NonGenericMethod(int value) => value;
}

public static class BoundaryTargets
{
    private static int refReturnStorage;

    public static int RefReturnStorage
    {
        get => refReturnStorage;
        set => refReturnStorage = value;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static T GenericMethod<T>(T value) => value;

    [System.Runtime.InteropServices.DllImport("kernel32.dll", EntryPoint = "GetCurrentProcessId")]
    public static extern uint PInvokeMethod();

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int VarArgsMethod(int required, __arglist)
    {
        var iterator = new ArgIterator(__arglist);
        return required + iterator.GetRemainingCount();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int CallVarArgsMethod() => VarArgsMethod(1, __arglist(2, 3));

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static ref int RefReturnMethod() => ref refReturnStorage;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static unsafe int PointerParameterMethod(int* value) => *value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static unsafe int CallPointerParameterMethod(int value) => PointerParameterMethod(&value);
}
