namespace Disharmony.Tests.EndToEnd.Optimizer;

[TestFixture]
#if NETFRAMEWORK
[Timeout(5000)]
#endif
public sealed class InlineExecutionControlTests : PatchTestBase
{
    [SetUp]
    public void EnableOptimizer()
    {
        HarmonyInterface.Instance.optimizerEnabled = true;
        InlineExecutionControlTargets.TargetCalls = 0;
    }

    [TearDown]
    public void DisableOptimizer() =>
        HarmonyInterface.Instance.optimizerEnabled = false;

    private static void ApplyInlinePatch(string patchMethodName, PatchConfig patchConfig,
        string targetMethodName, string? innerMethodName = null)
    {
        MethodInfo patch = typeof(InlineExecutionControlPatches).GetMethod(patchMethodName)!;
        MethodInfo target = typeof(InlineExecutionControlTargets).GetMethod(targetMethodName)!;
        MethodInfo? innerTarget = innerMethodName == null
            ? null
            : typeof(InlineExecutionControlTargets).GetMethod(innerMethodName)!;
        if (innerTarget is not null)
            patchConfig = patchConfig.Inner(innerTarget);
        Patcher.Patch(patchConfig.With(patch)
            .Options(PatchOptions.Optimize | PatchOptions.Inline).Of(target));
    }

    [Test]
    public void OuterPrefix_AlwaysTrue_RunsTarget()
    {
        ApplyInlinePatch(
            nameof(InlineExecutionControlPatches.OuterPrefix_AlwaysTrue_RunsTarget),
            Patch.Prefix,
            nameof(InlineExecutionControlTargets.OuterPrefix_AlwaysTrue_RunsTarget));

        int result = InlineExecutionControlTargets.OuterPrefix_AlwaysTrue_RunsTarget();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(10));
            Assert.That(InlineExecutionControlTargets.TargetCalls, Is.EqualTo(1));
        });
    }

    [Test]
    public void InnerPrefix_AlwaysTrue_RunsTarget()
    {
        ApplyInlinePatch(
            nameof(InlineExecutionControlPatches.InnerPrefix_AlwaysTrue_RunsTarget),
            Patch.Prefix,
            nameof(InlineExecutionControlTargets.InnerPrefix_AlwaysTrue_RunsTarget),
            nameof(InlineExecutionControlTargets.InnerPrefix_AlwaysTrue_RunsTarget_Inner));

        int result = InlineExecutionControlTargets.InnerPrefix_AlwaysTrue_RunsTarget();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(10));
            Assert.That(InlineExecutionControlTargets.TargetCalls, Is.EqualTo(1));
        });
    }

    [Test]
    public void OuterPrefix_AlwaysFalse_SkipsTarget()
    {
        ApplyInlinePatch(
            nameof(InlineExecutionControlPatches.OuterPrefix_AlwaysFalse_SkipsTarget),
            Patch.Prefix,
            nameof(InlineExecutionControlTargets.OuterPrefix_AlwaysFalse_SkipsTarget));

        int result = InlineExecutionControlTargets.OuterPrefix_AlwaysFalse_SkipsTarget();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Zero);
            Assert.That(InlineExecutionControlTargets.TargetCalls, Is.Zero);
        });
    }

    [Test]
    public void InnerPrefix_AlwaysFalse_SkipsTarget()
    {
        ApplyInlinePatch(
            nameof(InlineExecutionControlPatches.InnerPrefix_AlwaysFalse_SkipsTarget),
            Patch.Prefix,
            nameof(InlineExecutionControlTargets.InnerPrefix_AlwaysFalse_SkipsTarget),
            nameof(InlineExecutionControlTargets.InnerPrefix_AlwaysFalse_SkipsTarget_Inner));

        int result = InlineExecutionControlTargets.InnerPrefix_AlwaysFalse_SkipsTarget();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Zero);
            Assert.That(InlineExecutionControlTargets.TargetCalls, Is.Zero);
        });
    }

    [Test]
    public void OuterPrefix_ParameterControlsWhetherTargetRuns()
    {
        ApplyInlinePatch(
            nameof(InlineExecutionControlPatches.OuterPrefix_ParameterControlsWhetherTargetRuns),
            Patch.Prefix,
            nameof(InlineExecutionControlTargets.OuterPrefix_ParameterControlsWhetherTargetRuns));

        int skippedResult = InlineExecutionControlTargets.OuterPrefix_ParameterControlsWhetherTargetRuns(false);
        int runResult = InlineExecutionControlTargets.OuterPrefix_ParameterControlsWhetherTargetRuns(true);

        Assert.Multiple(() =>
        {
            Assert.That(skippedResult, Is.Zero);
            Assert.That(runResult, Is.EqualTo(10));
            Assert.That(InlineExecutionControlTargets.TargetCalls, Is.EqualTo(1));
        });
    }

    [Test]
    public void InnerPrefix_ParameterControlsWhetherTargetRuns()
    {
        ApplyInlinePatch(
            nameof(InlineExecutionControlPatches.InnerPrefix_ParameterControlsWhetherTargetRuns),
            Patch.Prefix,
            nameof(InlineExecutionControlTargets.InnerPrefix_ParameterControlsWhetherTargetRuns),
            nameof(InlineExecutionControlTargets.InnerPrefix_ParameterControlsWhetherTargetRuns_Inner));

        int skippedResult = InlineExecutionControlTargets.InnerPrefix_ParameterControlsWhetherTargetRuns(false);
        int runResult = InlineExecutionControlTargets.InnerPrefix_ParameterControlsWhetherTargetRuns(true);

        Assert.Multiple(() =>
        {
            Assert.That(skippedResult, Is.Zero);
            Assert.That(runResult, Is.EqualTo(10));
            Assert.That(InlineExecutionControlTargets.TargetCalls, Is.EqualTo(1));
        });
    }

}
