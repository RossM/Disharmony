namespace Disharmony.Tests.EndToEnd.RuleBuilders;

public static class ExceptionHandlingPatches
{
    public static int executionCount;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Void))]
    [Target(typeof(ExceptionHandlingTargets), nameof(ExceptionHandlingTargets.CallInTryBlock))]
    public static void InnerPrefix_TryBlock_ExecutesOnlyWhenExceptionIsNotThrown() => executionCount++;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Void))]
    [Target(typeof(ExceptionHandlingTargets), nameof(ExceptionHandlingTargets.CallInCatchBlock))]
    public static void InnerPrefix_CatchBlock_ExecutesOnlyWhenExceptionIsThrown() => executionCount++;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Void))]
    [Target(typeof(ExceptionHandlingTargets), nameof(ExceptionHandlingTargets.CallInFinallyBlock))]
    public static void InnerPrefix_FinallyBlock_ExecutesWhetherExceptionIsThrownOrNot() => executionCount++;
}

[TestFixture]
public sealed class ExceptionHandlingTests : PatchTestBase
{
    [Test]
    public void InnerPrefix_TryBlock_ExecutesOnlyWhenExceptionIsNotThrown()
    {
        ExceptionHandlingPatches.executionCount = 0;
        ApplyPatch(
            typeof(ExceptionHandlingPatches),
            nameof(ExceptionHandlingPatches.InnerPrefix_TryBlock_ExecutesOnlyWhenExceptionIsNotThrown));

        ExceptionHandlingTargets.CallInTryBlock(false);

        Assert.That(ExceptionHandlingPatches.executionCount, Is.EqualTo(1));

        ExceptionHandlingTargets.CallInTryBlock(true);

        Assert.That(ExceptionHandlingPatches.executionCount, Is.EqualTo(1));
    }

    [Test]
    public void InnerPrefix_CatchBlock_ExecutesOnlyWhenExceptionIsThrown()
    {
        ExceptionHandlingPatches.executionCount = 0;
        ApplyPatch(
            typeof(ExceptionHandlingPatches),
            nameof(ExceptionHandlingPatches.InnerPrefix_CatchBlock_ExecutesOnlyWhenExceptionIsThrown));

        ExceptionHandlingTargets.CallInCatchBlock(false);

        Assert.That(ExceptionHandlingPatches.executionCount, Is.Zero);

        ExceptionHandlingTargets.CallInCatchBlock(true);

        Assert.That(ExceptionHandlingPatches.executionCount, Is.EqualTo(1));
    }

    [Test]
    public void InnerPrefix_FinallyBlock_ExecutesWhetherExceptionIsThrownOrNot()
    {
        ExceptionHandlingPatches.executionCount = 0;
        ApplyPatch(
            typeof(ExceptionHandlingPatches),
            nameof(ExceptionHandlingPatches.InnerPrefix_FinallyBlock_ExecutesWhetherExceptionIsThrownOrNot));

        ExceptionHandlingTargets.CallInFinallyBlock(false);

        Assert.That(ExceptionHandlingPatches.executionCount, Is.EqualTo(1));

        Assert.Throws<InvalidOperationException>(() => ExceptionHandlingTargets.CallInFinallyBlock(true));

        Assert.That(ExceptionHandlingPatches.executionCount, Is.EqualTo(2));
    }
}
