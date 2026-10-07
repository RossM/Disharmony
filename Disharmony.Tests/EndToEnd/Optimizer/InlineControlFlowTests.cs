namespace Disharmony.Tests.EndToEnd.Optimizer;

[TestFixture]
#if NETFRAMEWORK
[Timeout(5000)]
#endif
public sealed class InlineControlFlowTests : PatchTestBase
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
        MethodInfo patch = typeof(InlineControlFlowPatches).GetMethod(patchMethodName)!;
        Patcher.Patch(patchConfig.With(patch)
            .Options(PatchOptions.Optimize | PatchOptions.Inline).Of(target));
    }

    [Test]
    public void Prefix_ControlFlow_MultipleReturns()
    {
        ApplyInlinePatch(
            nameof(InlineControlFlowPatches.Prefix_ControlFlow_MultipleReturns),
            Patch.Prefix,
            typeof(InlineControlFlowTargets).GetMethod(nameof(InlineControlFlowTargets.PrimitiveIdentity))!);

        Assert.That(InlineControlFlowTargets.PrimitiveIdentity(-10), Is.EqualTo(-1));
        Assert.That(InlineControlFlowTargets.PrimitiveIdentity(0), Is.EqualTo(7));
        Assert.That(InlineControlFlowTargets.PrimitiveIdentity(1), Is.EqualTo(42));
    }

    [Test]
    public void Prefix_ExceptionHandling_TryFinally()
    {
        InlineControlFlowPatches.FinallyExecutions = 0;
        ApplyInlinePatch(
            nameof(InlineControlFlowPatches.Prefix_ExceptionHandling_TryFinally),
            Patch.Prefix,
            typeof(InlineControlFlowTargets).GetMethod(nameof(InlineControlFlowTargets.PrimitiveIdentity))!);

        int result = InlineControlFlowTargets.PrimitiveIdentity(7);

        Assert.That(result, Is.EqualTo(42));
        Assert.That(InlineControlFlowPatches.FinallyExecutions, Is.EqualTo(1));
    }

}
