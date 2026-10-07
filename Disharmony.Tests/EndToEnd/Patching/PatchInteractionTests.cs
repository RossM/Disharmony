namespace Disharmony.Tests.EndToEnd.Patching;

public static class PatchInteractionPatches
{
    public static int observed;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntArgument))]
    public static bool PostfixObservesArgumentWrittenByPrefixWhenTargetRuns_Prefix(ref int value)
    {
        value = 42;
        return true;
    }

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntArgument))]
    public static void PostfixObservesArgumentWrittenByPrefixWhenTargetRuns_Postfix(int value) => observed = value;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowingIntArgument))]
    public static bool PostfixObservesArgumentWrittenByPrefixWhenTargetIsSkipped_Prefix(ref int value)
    {
        value = 42;
        return false;
    }

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowingIntArgument))]
    public static void PostfixObservesArgumentWrittenByPrefixWhenTargetIsSkipped_Postfix(int value) => observed = value;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntResult))]
    public static bool PostfixObservesTargetResultWhenPrefixWritesResultAndTargetRuns_Prefix(ref int __result)
    {
        __result = 42;
        return true;
    }

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntResult))]
    public static void PostfixObservesTargetResultWhenPrefixWritesResultAndTargetRuns_Postfix(int __result) => observed = __result;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowingIntResult))]
    public static bool PostfixObservesPrefixResultWhenPrefixWritesResultAndTargetIsSkipped_Prefix(ref int __result)
    {
        __result = 42;
        return false;
    }

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowingIntResult))]
    public static void PostfixObservesPrefixResultWhenPrefixWritesResultAndTargetIsSkipped_Postfix(int __result) => observed = __result;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowingIntResult))]
    public static bool Outer_PostfixObservesDefaultResultWhenPrefixSkipsWithoutBindingResult_Prefix() => false;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowingIntResult))]
    public static void Outer_PostfixObservesDefaultResultWhenPrefixSkipsWithoutBindingResult_Postfix(int __result) =>
        observed = __result;

    [Prefix]
    [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.IntResult))]
    public static bool Inner_PostfixObservesDefaultResultWhenPrefixSkipsWithoutBindingResult_Prefix() => false;

    [Postfix]
    [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.IntResult))]
    public static void Inner_PostfixObservesDefaultResultWhenPrefixSkipsWithoutBindingResult_Postfix(int __result) =>
        observed = __result;
    [Prefix]
    [Priority(PatchPriority.High)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.PriorityTarget))]
    public static void Outer_AttributePriority_NestsHigherPriorityOutsideLowerPriority_HighPrefix() =>
        StaticMethodTargets.PriorityEvents.Add("high-prefix");

    [Prefix]
    [Priority(PatchPriority.Default)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.PriorityTarget))]
    public static void Outer_AttributePriority_NestsHigherPriorityOutsideLowerPriority_DefaultPrefix() =>
        StaticMethodTargets.PriorityEvents.Add("default-prefix");

    [Prefix]
    [Priority(PatchPriority.Low)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.PriorityTarget))]
    public static void Outer_AttributePriority_NestsHigherPriorityOutsideLowerPriority_LowPrefix() =>
        StaticMethodTargets.PriorityEvents.Add("low-prefix");

    [Postfix]
    [Priority(PatchPriority.High)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.PriorityTarget))]
    public static void Outer_AttributePriority_NestsHigherPriorityOutsideLowerPriority_HighPostfix() =>
        StaticMethodTargets.PriorityEvents.Add("high-postfix");

    [Postfix]
    [Priority(PatchPriority.Default)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.PriorityTarget))]
    public static void Outer_AttributePriority_NestsHigherPriorityOutsideLowerPriority_DefaultPostfix() =>
        StaticMethodTargets.PriorityEvents.Add("default-postfix");

    [Postfix]
    [Priority(PatchPriority.Low)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.PriorityTarget))]
    public static void Outer_AttributePriority_NestsHigherPriorityOutsideLowerPriority_LowPostfix() =>
        StaticMethodTargets.PriorityEvents.Add("low-postfix");

    public static void Outer_ConfigPriority_NestsHigherPriorityOutsideLowerPriority_HighPrefix() =>
        StaticMethodTargets.PriorityEvents.Add("high-prefix");

    public static void Outer_ConfigPriority_NestsHigherPriorityOutsideLowerPriority_LowPrefix() =>
        StaticMethodTargets.PriorityEvents.Add("low-prefix");

    public static void Outer_ConfigPriority_NestsHigherPriorityOutsideLowerPriority_HighPostfix() =>
        StaticMethodTargets.PriorityEvents.Add("high-postfix");

    public static void Outer_ConfigPriority_NestsHigherPriorityOutsideLowerPriority_LowPostfix() =>
        StaticMethodTargets.PriorityEvents.Add("low-postfix");

    public static void Inner_ConfigPriority_NestsHigherPriorityOutsideLowerPriority_HighPrefix() =>
        StaticMethodTargets.PriorityEvents.Add("high-prefix");

    public static void Inner_ConfigPriority_NestsHigherPriorityOutsideLowerPriority_LowPrefix() =>
        StaticMethodTargets.PriorityEvents.Add("low-prefix");

    public static void Inner_ConfigPriority_NestsHigherPriorityOutsideLowerPriority_HighPostfix() =>
        StaticMethodTargets.PriorityEvents.Add("high-postfix");

    public static void Inner_ConfigPriority_NestsHigherPriorityOutsideLowerPriority_LowPostfix() =>
        StaticMethodTargets.PriorityEvents.Add("low-postfix");

    public static void Outer_DefaultPriority_SecondPatchPairWrapsFirstPatchPair_FirstPrefix() =>
        StaticMethodTargets.PriorityEvents.Add("first-prefix");

    public static void Outer_DefaultPriority_SecondPatchPairWrapsFirstPatchPair_FirstPostfix() =>
        StaticMethodTargets.PriorityEvents.Add("first-postfix");

    public static void Outer_DefaultPriority_SecondPatchPairWrapsFirstPatchPair_SecondPrefix() =>
        StaticMethodTargets.PriorityEvents.Add("second-prefix");

    public static void Outer_DefaultPriority_SecondPatchPairWrapsFirstPatchPair_SecondPostfix() =>
        StaticMethodTargets.PriorityEvents.Add("second-postfix");

    public static void Inner_DefaultPriority_SecondPatchPairWrapsFirstPatchPair_FirstPrefix() =>
        StaticMethodTargets.PriorityEvents.Add("first-prefix");

    public static void Inner_DefaultPriority_SecondPatchPairWrapsFirstPatchPair_FirstPostfix() =>
        StaticMethodTargets.PriorityEvents.Add("first-postfix");

    public static void Inner_DefaultPriority_SecondPatchPairWrapsFirstPatchPair_SecondPrefix() =>
        StaticMethodTargets.PriorityEvents.Add("second-prefix");

    public static void Inner_DefaultPriority_SecondPatchPairWrapsFirstPatchPair_SecondPostfix() =>
        StaticMethodTargets.PriorityEvents.Add("second-postfix");

    public static bool Outer_HigherPrioritySkippingPrefix_SkipsLowerPriorityPrefixAndTarget_HighPrefix()
    {
        StaticMethodTargets.PriorityEvents.Add("high-prefix");
        return false;
    }

    public static bool Outer_HigherPrioritySkippingPrefix_SkipsLowerPriorityPrefixAndTarget_LowPrefix()
    {
        StaticMethodTargets.PriorityEvents.Add("low-prefix");
        return true;
    }

    public static void Outer_HigherPrioritySkippingPrefix_SkipsLowerPriorityPrefixAndTarget_HighPostfix() =>
        StaticMethodTargets.PriorityEvents.Add("high-postfix");

    public static void Outer_HigherPrioritySkippingPrefix_SkipsLowerPriorityPrefixAndTarget_LowPostfix() =>
        StaticMethodTargets.PriorityEvents.Add("low-postfix");
}

[TestFixture]
public sealed class PatchInteractionTests : PatchTestBase
{
    [Test]
    public void PostfixObservesArgumentWrittenByPrefixWhenTargetRuns()
    {
        PatchInteractionPatches.observed = 0;
        ApplyPatch(typeof(PatchInteractionPatches),
            nameof(PatchInteractionPatches.PostfixObservesArgumentWrittenByPrefixWhenTargetRuns_Prefix));
        ApplyPatch(typeof(PatchInteractionPatches),
            nameof(PatchInteractionPatches.PostfixObservesArgumentWrittenByPrefixWhenTargetRuns_Postfix));

        StaticMethodTargets.IntArgument(1);

        Assert.That(PatchInteractionPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void PostfixObservesArgumentWrittenByPrefixWhenTargetIsSkipped()
    {
        PatchInteractionPatches.observed = 0;
        ApplyPatch(typeof(PatchInteractionPatches),
            nameof(PatchInteractionPatches.PostfixObservesArgumentWrittenByPrefixWhenTargetIsSkipped_Prefix));
        ApplyPatch(typeof(PatchInteractionPatches),
            nameof(PatchInteractionPatches.PostfixObservesArgumentWrittenByPrefixWhenTargetIsSkipped_Postfix));

        StaticMethodTargets.ThrowingIntArgument(1);

        Assert.That(PatchInteractionPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void PostfixObservesTargetResultWhenPrefixWritesResultAndTargetRuns()
    {
        PatchInteractionPatches.observed = 0;
        ApplyPatch(typeof(PatchInteractionPatches),
            nameof(PatchInteractionPatches.PostfixObservesTargetResultWhenPrefixWritesResultAndTargetRuns_Prefix));
        ApplyPatch(typeof(PatchInteractionPatches),
            nameof(PatchInteractionPatches.PostfixObservesTargetResultWhenPrefixWritesResultAndTargetRuns_Postfix));

        StaticMethodTargets.IntResult();

        Assert.That(PatchInteractionPatches.observed, Is.EqualTo(1));
    }

    [Test]
    public void PostfixObservesPrefixResultWhenPrefixWritesResultAndTargetIsSkipped()
    {
        PatchInteractionPatches.observed = 0;
        ApplyPatch(typeof(PatchInteractionPatches),
            nameof(PatchInteractionPatches.PostfixObservesPrefixResultWhenPrefixWritesResultAndTargetIsSkipped_Prefix));
        ApplyPatch(typeof(PatchInteractionPatches),
            nameof(PatchInteractionPatches.PostfixObservesPrefixResultWhenPrefixWritesResultAndTargetIsSkipped_Postfix));

        StaticMethodTargets.ThrowingIntResult();

        Assert.That(PatchInteractionPatches.observed, Is.EqualTo(42));
    }

    [Test]
    public void Outer_PostfixObservesDefaultResultWhenPrefixSkipsWithoutBindingResult()
    {
        PatchInteractionPatches.observed = 42;
        ApplyPatch(typeof(PatchInteractionPatches),
            nameof(PatchInteractionPatches.Outer_PostfixObservesDefaultResultWhenPrefixSkipsWithoutBindingResult_Prefix));
        ApplyPatch(typeof(PatchInteractionPatches),
            nameof(PatchInteractionPatches.Outer_PostfixObservesDefaultResultWhenPrefixSkipsWithoutBindingResult_Postfix));
        int result = StaticMethodTargets.ThrowingIntResult();

        Assert.That(PatchInteractionPatches.observed, Is.Zero);
        Assert.That(result, Is.Zero);
    }

    [Test]
    public void Inner_PostfixObservesDefaultResultWhenPrefixSkipsWithoutBindingResult()
    {
        PatchInteractionPatches.observed = 42;
        ApplyPatch(typeof(PatchInteractionPatches),
            nameof(PatchInteractionPatches.Inner_PostfixObservesDefaultResultWhenPrefixSkipsWithoutBindingResult_Prefix));
        ApplyPatch(typeof(PatchInteractionPatches),
            nameof(PatchInteractionPatches.Inner_PostfixObservesDefaultResultWhenPrefixSkipsWithoutBindingResult_Postfix));
        int result = OuterStaticMethodTargets.IntResult();

        Assert.That(PatchInteractionPatches.observed, Is.Zero);
        Assert.That(result, Is.Zero);
    }
    [Test]
    public void Outer_AttributePriority_NestsHigherPriorityOutsideLowerPriority()
    {
        StaticMethodTargets.PriorityEvents.Clear();
        ApplyPatch(typeof(PatchInteractionPatches),
            nameof(PatchInteractionPatches.Outer_AttributePriority_NestsHigherPriorityOutsideLowerPriority_LowPostfix));
        ApplyPatch(typeof(PatchInteractionPatches),
            nameof(PatchInteractionPatches.Outer_AttributePriority_NestsHigherPriorityOutsideLowerPriority_DefaultPrefix));
        ApplyPatch(typeof(PatchInteractionPatches),
            nameof(PatchInteractionPatches.Outer_AttributePriority_NestsHigherPriorityOutsideLowerPriority_HighPostfix));
        ApplyPatch(typeof(PatchInteractionPatches),
            nameof(PatchInteractionPatches.Outer_AttributePriority_NestsHigherPriorityOutsideLowerPriority_LowPrefix));
        ApplyPatch(typeof(PatchInteractionPatches),
            nameof(PatchInteractionPatches.Outer_AttributePriority_NestsHigherPriorityOutsideLowerPriority_DefaultPostfix));
        ApplyPatch(typeof(PatchInteractionPatches),
            nameof(PatchInteractionPatches.Outer_AttributePriority_NestsHigherPriorityOutsideLowerPriority_HighPrefix));

        StaticMethodTargets.PriorityTarget();

        Assert.That(StaticMethodTargets.PriorityEvents, Is.EqualTo(new[]
        {
            "high-prefix",
            "default-prefix",
            "low-prefix",
            "target",
            "low-postfix",
            "default-postfix",
            "high-postfix",
        }));
    }

    [Test]
    public void Outer_ConfigPriority_NestsHigherPriorityOutsideLowerPriority()
    {
        StaticMethodTargets.PriorityEvents.Clear();
        MethodInfo target = typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.PriorityTarget))!;
        MethodInfo highPrefix = typeof(PatchInteractionPatches)
            .GetMethod(nameof(PatchInteractionPatches.Outer_ConfigPriority_NestsHigherPriorityOutsideLowerPriority_HighPrefix))!;
        MethodInfo lowPrefix = typeof(PatchInteractionPatches)
            .GetMethod(nameof(PatchInteractionPatches.Outer_ConfigPriority_NestsHigherPriorityOutsideLowerPriority_LowPrefix))!;
        MethodInfo highPostfix = typeof(PatchInteractionPatches)
            .GetMethod(nameof(PatchInteractionPatches.Outer_ConfigPriority_NestsHigherPriorityOutsideLowerPriority_HighPostfix))!;
        MethodInfo lowPostfix = typeof(PatchInteractionPatches)
            .GetMethod(nameof(PatchInteractionPatches.Outer_ConfigPriority_NestsHigherPriorityOutsideLowerPriority_LowPostfix))!;
        Patcher.Patch(
            Patch.Postfix.With(highPostfix).Priority(PatchPriority.High).Of(target),
            Patch.Prefix.With(lowPrefix).Priority(PatchPriority.Low).Of(target),
            Patch.Postfix.With(lowPostfix).Priority(PatchPriority.Low).Of(target),
            Patch.Prefix.With(highPrefix).Priority(PatchPriority.High).Of(target));

        StaticMethodTargets.PriorityTarget();

        Assert.That(StaticMethodTargets.PriorityEvents, Is.EqualTo(new[]
        {
            "high-prefix",
            "low-prefix",
            "target",
            "low-postfix",
            "high-postfix",
        }));
    }

    [Test]
    public void Inner_ConfigPriority_NestsHigherPriorityOutsideLowerPriority()
    {
        StaticMethodTargets.PriorityEvents.Clear();
        MethodInfo outer = typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.PriorityOuter))!;
        MethodInfo inner = typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.PriorityInner))!;
        MethodInfo highPrefix = typeof(PatchInteractionPatches)
            .GetMethod(nameof(PatchInteractionPatches.Inner_ConfigPriority_NestsHigherPriorityOutsideLowerPriority_HighPrefix))!;
        MethodInfo lowPrefix = typeof(PatchInteractionPatches)
            .GetMethod(nameof(PatchInteractionPatches.Inner_ConfigPriority_NestsHigherPriorityOutsideLowerPriority_LowPrefix))!;
        MethodInfo highPostfix = typeof(PatchInteractionPatches)
            .GetMethod(nameof(PatchInteractionPatches.Inner_ConfigPriority_NestsHigherPriorityOutsideLowerPriority_HighPostfix))!;
        MethodInfo lowPostfix = typeof(PatchInteractionPatches)
            .GetMethod(nameof(PatchInteractionPatches.Inner_ConfigPriority_NestsHigherPriorityOutsideLowerPriority_LowPostfix))!;
        Patcher.Patch(
            Patch.Postfix.Inner(inner).With(highPostfix).Priority(PatchPriority.High).Of(outer),
            Patch.Prefix.Inner(inner).With(lowPrefix).Priority(PatchPriority.Low).Of(outer),
            Patch.Postfix.Inner(inner).With(lowPostfix).Priority(PatchPriority.Low).Of(outer),
            Patch.Prefix.Inner(inner).With(highPrefix).Priority(PatchPriority.High).Of(outer));

        StaticMethodTargets.PriorityOuter();

        Assert.That(StaticMethodTargets.PriorityEvents, Is.EqualTo(new[]
        {
            "high-prefix",
            "low-prefix",
            "inner-target",
            "low-postfix",
            "high-postfix",
        }));
    }

    [Test]
    public void Outer_DefaultPriority_SecondPatchPairWrapsFirstPatchPair()
    {
        StaticMethodTargets.PriorityEvents.Clear();
        MethodInfo target = typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.PriorityTarget))!;
        MethodInfo firstPrefix = typeof(PatchInteractionPatches)
            .GetMethod(nameof(PatchInteractionPatches.Outer_DefaultPriority_SecondPatchPairWrapsFirstPatchPair_FirstPrefix))!;
        MethodInfo firstPostfix = typeof(PatchInteractionPatches)
            .GetMethod(nameof(PatchInteractionPatches.Outer_DefaultPriority_SecondPatchPairWrapsFirstPatchPair_FirstPostfix))!;
        MethodInfo secondPrefix = typeof(PatchInteractionPatches)
            .GetMethod(nameof(PatchInteractionPatches.Outer_DefaultPriority_SecondPatchPairWrapsFirstPatchPair_SecondPrefix))!;
        MethodInfo secondPostfix = typeof(PatchInteractionPatches)
            .GetMethod(nameof(PatchInteractionPatches.Outer_DefaultPriority_SecondPatchPairWrapsFirstPatchPair_SecondPostfix))!;
        Patcher.Patch(
            Patch.Prefix.With(firstPrefix).Of(target),
            Patch.Postfix.With(firstPostfix).Of(target));
        Patcher.Patch(
            Patch.Prefix.With(secondPrefix).Of(target),
            Patch.Postfix.With(secondPostfix).Of(target));

        StaticMethodTargets.PriorityTarget();

        Assert.That(StaticMethodTargets.PriorityEvents, Is.EqualTo(new[]
        {
            "second-prefix",
            "first-prefix",
            "target",
            "first-postfix",
            "second-postfix",
        }));
    }

    [Test]
    public void Inner_DefaultPriority_SecondPatchPairWrapsFirstPatchPair()
    {
        StaticMethodTargets.PriorityEvents.Clear();
        MethodInfo outer = typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.PriorityOuter))!;
        MethodInfo inner = typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.PriorityInner))!;
        MethodInfo firstPrefix = typeof(PatchInteractionPatches)
            .GetMethod(nameof(PatchInteractionPatches.Inner_DefaultPriority_SecondPatchPairWrapsFirstPatchPair_FirstPrefix))!;
        MethodInfo firstPostfix = typeof(PatchInteractionPatches)
            .GetMethod(nameof(PatchInteractionPatches.Inner_DefaultPriority_SecondPatchPairWrapsFirstPatchPair_FirstPostfix))!;
        MethodInfo secondPrefix = typeof(PatchInteractionPatches)
            .GetMethod(nameof(PatchInteractionPatches.Inner_DefaultPriority_SecondPatchPairWrapsFirstPatchPair_SecondPrefix))!;
        MethodInfo secondPostfix = typeof(PatchInteractionPatches)
            .GetMethod(nameof(PatchInteractionPatches.Inner_DefaultPriority_SecondPatchPairWrapsFirstPatchPair_SecondPostfix))!;
        Patcher.Patch(
            Patch.Prefix.Inner(inner).With(firstPrefix).Of(outer),
            Patch.Postfix.Inner(inner).With(firstPostfix).Of(outer));
        Patcher.Patch(
            Patch.Prefix.Inner(inner).With(secondPrefix).Of(outer),
            Patch.Postfix.Inner(inner).With(secondPostfix).Of(outer));

        StaticMethodTargets.PriorityOuter();

        Assert.That(StaticMethodTargets.PriorityEvents, Is.EqualTo(new[]
        {
            "second-prefix",
            "first-prefix",
            "inner-target",
            "first-postfix",
            "second-postfix",
        }));
    }

    [Test]
    public void Outer_HigherPrioritySkippingPrefix_SkipsLowerPriorityPrefixAndTarget()
    {
        StaticMethodTargets.PriorityEvents.Clear();
        MethodInfo target = typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.PriorityTarget))!;
        MethodInfo highPrefix = typeof(PatchInteractionPatches)
            .GetMethod(nameof(PatchInteractionPatches.Outer_HigherPrioritySkippingPrefix_SkipsLowerPriorityPrefixAndTarget_HighPrefix))!;
        MethodInfo lowPrefix = typeof(PatchInteractionPatches)
            .GetMethod(nameof(PatchInteractionPatches.Outer_HigherPrioritySkippingPrefix_SkipsLowerPriorityPrefixAndTarget_LowPrefix))!;
        MethodInfo highPostfix = typeof(PatchInteractionPatches)
            .GetMethod(nameof(PatchInteractionPatches.Outer_HigherPrioritySkippingPrefix_SkipsLowerPriorityPrefixAndTarget_HighPostfix))!;
        MethodInfo lowPostfix = typeof(PatchInteractionPatches)
            .GetMethod(nameof(PatchInteractionPatches.Outer_HigherPrioritySkippingPrefix_SkipsLowerPriorityPrefixAndTarget_LowPostfix))!;
        Patcher.Patch(
            Patch.Prefix.With(lowPrefix).Priority(PatchPriority.Low).Of(target),
            Patch.Postfix.With(highPostfix).Priority(PatchPriority.High).Of(target),
            Patch.Prefix.With(highPrefix).Priority(PatchPriority.High).Of(target),
            Patch.Postfix.With(lowPostfix).Priority(PatchPriority.Low).Of(target));

        StaticMethodTargets.PriorityTarget();

        Assert.That(StaticMethodTargets.PriorityEvents, Is.EqualTo(new[]
        {
            "high-prefix",
            "low-postfix",
            "high-postfix",
        }));
    }
}
