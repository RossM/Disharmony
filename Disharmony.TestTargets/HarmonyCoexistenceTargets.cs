namespace Disharmony.Tests;

public static class HarmonyCoexistenceTargets
{
    public static List<string> Events { get; } = [];

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int OrderedTarget()
    {
        Events.Add("target");
        return 10;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int TranspilerOuterTarget() => OriginalValue();

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int TranspilerInnerTarget() => Inner(OriginalValue());

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int Inner(int value) => value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int OriginalValue() => 1;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int ReplacementValue() => 2;
}
