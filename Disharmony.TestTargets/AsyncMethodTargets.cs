using System.Threading.Tasks;

namespace Disharmony.Tests;

public static class AsyncInnerMethodTargets
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int BeforeAwait(int value) => value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int AfterAwait(int value) => value;
}

public sealed class AsyncMethodTargets
{
    public int Field;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static async Task<int> CallBeforeAndAfterAwait(Task gate, int outerValue)
    {
        int result = AsyncInnerMethodTargets.BeforeAwait(outerValue);
        await gate;
        return AsyncInnerMethodTargets.AfterAwait(result);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public async Task<int> CallAfterAwait(Task gate, int outerValue)
    {
        await gate;
        return AsyncInnerMethodTargets.AfterAwait(outerValue + Field);
    }
}
