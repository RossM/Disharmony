namespace Disharmony.Tests;

public static class UnpatchPatchTargets
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int TargetA() => 1;
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int TargetB() => 2;
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ApplyUnpatchApplyTarget() { }
}
