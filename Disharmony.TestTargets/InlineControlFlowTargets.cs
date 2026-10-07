namespace Disharmony.Tests;

public static class InlineControlFlowTargets
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int PrimitiveIdentity(int value) => value;

}

public static class InlineControlFlowPatches
{
    public static int FinallyExecutions;

    public static void Prefix_ControlFlow_MultipleReturns(ref int value)
    {
        if (value < 0)
        {
            value = -1;
            return;
        }

        if (value == 0)
        {
            value = 7;
            return;
        }

        value = 42;
    }

    public static void Prefix_ExceptionHandling_TryFinally(ref int value)
    {
        try
        {
            value = 42;
        }
        finally
        {
            FinallyExecutions++;
        }
    }

}
