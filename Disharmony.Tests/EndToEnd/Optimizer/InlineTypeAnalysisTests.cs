namespace Disharmony.Tests.EndToEnd.Optimizer;

[TestFixture]
#if NETFRAMEWORK
[Timeout(5000)]
#endif
public sealed class InlineTypeAnalysisTests : PatchTestBase
{
    [SetUp]
    public void EnableOptimizer() =>
        HarmonyInterface.Instance.optimizerEnabled = true;

    [TearDown]
    public void DisableOptimizer() =>
        HarmonyInterface.Instance.optimizerEnabled = false;

    private static void ApplyInlinePatch(string patchMethodName, PatchConfig patchConfig,
        MethodBase target)
    {
        MethodInfo patch = typeof(InlineTypeAnalysisPatches).GetMethod(patchMethodName)!;
        Patcher.Patch(patchConfig.With(patch)
            .Options(PatchOptions.Optimize | PatchOptions.Inline).Of(target));
    }

    [Test]
    public void Prefix_Is_Local_KnownSuccess()
    {
        ApplyInlinePatch(
            nameof(InlineTypeAnalysisPatches.Prefix_Is_Local_KnownSuccess),
            Patch.Prefix,
            typeof(OptimizerMixedTargets).GetMethod(nameof(OptimizerMixedTargets.IsOperatorOnLocal))!);

        Assert.That(OptimizerMixedTargets.IsOperatorOnLocal(false), Is.True);
        Assert.That(OptimizerMixedTargets.IsOperatorOnLocal(true), Is.True);
    }

    [Test]
    public void Prefix_Is_Local_KnownFailure()
    {
        ApplyInlinePatch(
            nameof(InlineTypeAnalysisPatches.Prefix_Is_Local_KnownFailure),
            Patch.Prefix,
            typeof(OptimizerMixedTargets).GetMethod(nameof(OptimizerMixedTargets.IsOperatorOnLocal))!);

        Assert.That(OptimizerMixedTargets.IsOperatorOnLocal(false), Is.False);
        Assert.That(OptimizerMixedTargets.IsOperatorOnLocal(true), Is.False);
    }

    [Test]
    public void Prefix_Is_EvaluationStack_KnownSuccess()
    {
        ApplyInlinePatch(
            nameof(InlineTypeAnalysisPatches.Prefix_Is_EvaluationStack_KnownSuccess),
            Patch.Prefix,
            typeof(OptimizerMixedTargets).GetMethod(nameof(OptimizerMixedTargets.IsOperatorOnEvaluationStack))!);

        Assert.That(OptimizerMixedTargets.IsOperatorOnEvaluationStack(false), Is.True);
        Assert.That(OptimizerMixedTargets.IsOperatorOnEvaluationStack(true), Is.True);
    }

    [Test]
    public void Prefix_Is_EvaluationStack_KnownFailure()
    {
        ApplyInlinePatch(
            nameof(InlineTypeAnalysisPatches.Prefix_Is_EvaluationStack_KnownFailure),
            Patch.Prefix,
            typeof(OptimizerMixedTargets).GetMethod(nameof(OptimizerMixedTargets.IsOperatorOnEvaluationStack))!);

        Assert.That(OptimizerMixedTargets.IsOperatorOnEvaluationStack(false), Is.False);
        Assert.That(OptimizerMixedTargets.IsOperatorOnEvaluationStack(true), Is.False);
    }

    [Test]
    public void Prefix_As_Local_KnownSuccess()
    {
        ApplyInlinePatch(
            nameof(InlineTypeAnalysisPatches.Prefix_As_Local_KnownSuccess),
            Patch.Prefix,
            typeof(OptimizerMixedTargets).GetMethod(nameof(OptimizerMixedTargets.AsOperatorOnLocal))!);

        Assert.That(OptimizerMixedTargets.AsOperatorOnLocal(false), Is.EqualTo(7));
        Assert.That(OptimizerMixedTargets.AsOperatorOnLocal(true), Is.EqualTo(7));
    }

    [Test]
    public void Prefix_As_Local_KnownFailure()
    {
        ApplyInlinePatch(
            nameof(InlineTypeAnalysisPatches.Prefix_As_Local_KnownFailure),
            Patch.Prefix,
            typeof(OptimizerMixedTargets).GetMethod(nameof(OptimizerMixedTargets.AsOperatorOnLocal))!);

        Assert.That(OptimizerMixedTargets.AsOperatorOnLocal(false), Is.Null);
        Assert.That(OptimizerMixedTargets.AsOperatorOnLocal(true), Is.Null);
    }

    [Test]
    public void Prefix_As_EvaluationStack_KnownSuccess()
    {
        ApplyInlinePatch(
            nameof(InlineTypeAnalysisPatches.Prefix_As_EvaluationStack_KnownSuccess),
            Patch.Prefix,
            typeof(OptimizerMixedTargets).GetMethod(nameof(OptimizerMixedTargets.AsOperatorOnEvaluationStack))!);

        Assert.That(OptimizerMixedTargets.AsOperatorOnEvaluationStack(false), Is.EqualTo(7));
        Assert.That(OptimizerMixedTargets.AsOperatorOnEvaluationStack(true), Is.EqualTo(7));
    }

    [Test]
    public void Prefix_As_EvaluationStack_KnownFailure()
    {
        ApplyInlinePatch(
            nameof(InlineTypeAnalysisPatches.Prefix_As_EvaluationStack_KnownFailure),
            Patch.Prefix,
            typeof(OptimizerMixedTargets).GetMethod(nameof(OptimizerMixedTargets.AsOperatorOnEvaluationStack))!);

        Assert.That(OptimizerMixedTargets.AsOperatorOnEvaluationStack(false), Is.Null);
        Assert.That(OptimizerMixedTargets.AsOperatorOnEvaluationStack(true), Is.Null);
    }

}
