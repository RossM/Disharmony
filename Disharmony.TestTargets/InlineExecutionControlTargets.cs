namespace Disharmony.Tests;

public static class InlineExecutionControlTargets
{
    public static int TargetCalls;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int OuterPrefix_AlwaysTrue_RunsTarget()
    {
        TargetCalls++;
        return 10;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int InnerPrefix_AlwaysTrue_RunsTarget() =>
        InnerPrefix_AlwaysTrue_RunsTarget_Inner();

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int InnerPrefix_AlwaysTrue_RunsTarget_Inner()
    {
        TargetCalls++;
        return 10;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int OuterPrefix_AlwaysFalse_SkipsTarget()
    {
        TargetCalls++;
        return 10;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int InnerPrefix_AlwaysFalse_SkipsTarget() =>
        InnerPrefix_AlwaysFalse_SkipsTarget_Inner();

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int InnerPrefix_AlwaysFalse_SkipsTarget_Inner()
    {
        TargetCalls++;
        return 10;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int OuterPrefix_ParameterControlsWhetherTargetRuns(bool runOriginal)
    {
        TargetCalls++;
        return 10;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int InnerPrefix_ParameterControlsWhetherTargetRuns(bool runOriginal) =>
        InnerPrefix_ParameterControlsWhetherTargetRuns_Inner(runOriginal);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int InnerPrefix_ParameterControlsWhetherTargetRuns_Inner(bool runOriginal)
    {
        TargetCalls++;
        return 10;
    }
}

public static class InlineExecutionControlPatches
{
    public static bool OuterPrefix_AlwaysTrue_RunsTarget() => true;
    public static bool InnerPrefix_AlwaysTrue_RunsTarget() => true;
    public static bool OuterPrefix_AlwaysFalse_SkipsTarget() => false;
    public static bool InnerPrefix_AlwaysFalse_SkipsTarget() => false;
    public static bool OuterPrefix_ParameterControlsWhetherTargetRuns(bool runOriginal) => runOriginal;
    public static bool InnerPrefix_ParameterControlsWhetherTargetRuns(bool runOriginal) => runOriginal;
}
