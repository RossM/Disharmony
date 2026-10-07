namespace Disharmony.Tests;

public static class InlineRuleBuilderPatches
{
    public static int ArgumentLoaded;
    public static int ArgumentAddressed;
    public static int ArgumentStored;
    public static int Local0Observed;
    public static int Local4Observed;
    public static int SwitchObserved;

    // A non-void signature exercises the inline return-value store, but there is no ret to supply its value.
    public static bool Prefix_UnconditionalThrowWithoutReturn_PropagatesException()
    {
        throw new ApplicationException("Unconditional inline patch exception");
    }

    public static void Prefix_BranchAndRefWrite_AreInlined(ref int value)
    {
        if (value < 0)
            value = -value;
    }

    public static void Prefix_ArgumentOpcodeForms_AreRemapped(
        int first,
        int second,
        int third,
        int fourth,
        int fifth)
    {
        ArgumentLoaded = fifth;
        System.Threading.Interlocked.Increment(ref fifth);
        ArgumentAddressed = fifth;
        fifth = 42;
        ArgumentStored = fifth;
    }

    public static void Prefix_LocalOpcodeForms_AreRemapped()
    {
        int local0 = 10;
        int local1 = 11;
        int local2 = 12;
        int local3 = 13;
        int local4 = 14;
        System.Threading.Interlocked.CompareExchange(ref local0, local0, local0);
        System.Threading.Interlocked.CompareExchange(ref local1, local1, local1);
        System.Threading.Interlocked.CompareExchange(ref local2, local2, local2);
        System.Threading.Interlocked.CompareExchange(ref local3, local3, local3);
        System.Threading.Interlocked.CompareExchange(ref local4, local4, local4);
        Local0Observed = local0;
        Local4Observed = local4;
    }

    public static void Prefix_SwitchTargets_AreRemapped(int value)
    {
        switch (value)
        {
            case 0: SwitchObserved = 10; break;
            case 1: SwitchObserved = 11; break;
            case 2: SwitchObserved = 12; break;
            case 3: SwitchObserved = 13; break;
            case 4: SwitchObserved = 14; break;
            default: SwitchObserved = 99; break;
        }
    }

    public static void Prefix_ExceptionHandling_TryCatch_WithoutCarriedStack(ref int value)
    {
        try
        {
            if (value < 0)
                throw new InvalidOperationException();
            value = 42;
        }
        catch (InvalidOperationException)
        {
            value = -1;
        }
    }

    public static void InnerPrefix_ExceptionHandling_TryCatch_WithCarriedStack(bool throwInPatch)
    {
        try
        {
            if (throwInPatch)
                throw new InvalidOperationException();
        }
        catch (InvalidOperationException)
        {
        }
    }
}

public static class InlineRuleBuilderTargets
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void FiveArguments(int first, int second, int third, int fourth, int fifth) { }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int InnerPrefix_ExceptionHandling_TryCatch_WithCarriedStack(bool throwInPatch) =>
        Add(10, InnerPrefix_ExceptionHandling_TryCatch_WithCarriedStack_Inner());

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int InnerPrefix_ExceptionHandling_TryCatch_WithCarriedStack_Inner() => 32;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int Add(int left, int right) => left + right;
}
