namespace Disharmony.Tests;

public static class HarmonyInterfaceTargets
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int Value() => 10;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int Trampoline() => 20;

    public static bool Prefix(ref int __result)
    {
        __result = 30;
        return false;
    }

    public static void IncrementResult(ref int __result) => __result++;
}
