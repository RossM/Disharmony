namespace Disharmony.Tests.EndToEnd.Patching;

public static class ExecutionControlPatches
{
    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntResult))]
    public static bool PrefixResultIsReplacedByValueTypeTargetWhenReturningTrue(ref int __result)
    {
        __result = 42;
        return true;
    }

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowingIntResult))]
    public static bool PrefixResultIsRetainedForValueTypeTargetWhenReturningFalse(ref int __result)
    {
        __result = 42;
        return false;
    }

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StringResult))]
    public static bool PrefixResultIsReplacedByReferenceTypeTargetWhenReturningTrue(ref string? __result)
    {
        __result = "patched";
        return true;
    }

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowingStringResult))]
    public static bool PrefixResultIsRetainedForReferenceTypeTargetWhenReturningFalse(ref string? __result)
    {
        __result = "patched";
        return false;
    }

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntResult))]
    public static bool PrefixReturningTrueRunsValueTypeTarget() => true;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.StringResult))]
    public static bool PrefixReturningTrueRunsReferenceTypeTarget() => true;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowingIntResult))]
    public static bool PrefixReturningFalseSkipsValueTypeTarget() => false;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowingStringResult))]
    public static bool PrefixReturningFalseSkipsReferenceTypeTarget() => false;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.IntResult))]
    public static bool InnerPrefixReturningTrueRunsInnerTarget() => true;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.IntResult))]
    public static bool InnerPrefixReturningFalseSkipsInnerTarget() => false;
}

[TestFixture]
public sealed partial class ExecutionControlTests
{
    [Test]
    public void PrefixResultIsReplacedByValueTypeTargetWhenReturningTrue()
    {
        ApplyPatch(typeof(ExecutionControlPatches),
            nameof(ExecutionControlPatches.PrefixResultIsReplacedByValueTypeTargetWhenReturningTrue));
        Assert.That(StaticMethodTargets.IntResult(), Is.EqualTo(1));
    }

    [Test]
    public void PrefixResultIsRetainedForValueTypeTargetWhenReturningFalse()
    {
        ApplyPatch(typeof(ExecutionControlPatches),
            nameof(ExecutionControlPatches.PrefixResultIsRetainedForValueTypeTargetWhenReturningFalse));
        Assert.That(StaticMethodTargets.ThrowingIntResult(), Is.EqualTo(42));
    }

    [Test]
    public void PrefixResultIsReplacedByReferenceTypeTargetWhenReturningTrue()
    {
        ApplyPatch(typeof(ExecutionControlPatches),
            nameof(ExecutionControlPatches.PrefixResultIsReplacedByReferenceTypeTargetWhenReturningTrue));
        Assert.That(StaticMethodTargets.StringResult(), Is.EqualTo("original"));
    }

    [Test]
    public void PrefixResultIsRetainedForReferenceTypeTargetWhenReturningFalse()
    {
        ApplyPatch(typeof(ExecutionControlPatches),
            nameof(ExecutionControlPatches.PrefixResultIsRetainedForReferenceTypeTargetWhenReturningFalse));
        Assert.That(StaticMethodTargets.ThrowingStringResult(), Is.EqualTo("patched"));
    }
}

[TestFixture]
public sealed partial class ExecutionControlTests : PatchTestBase
{
    [Test]
    public void PrefixReturningTrueRunsValueTypeTarget()
    {
        ApplyPatch(typeof(ExecutionControlPatches), nameof(ExecutionControlPatches.PrefixReturningTrueRunsValueTypeTarget));
        Assert.That(StaticMethodTargets.IntResult(), Is.EqualTo(1));
    }

    [Test]
    public void PrefixReturningTrueRunsReferenceTypeTarget()
    {
        ApplyPatch(typeof(ExecutionControlPatches), nameof(ExecutionControlPatches.PrefixReturningTrueRunsReferenceTypeTarget));
        Assert.That(StaticMethodTargets.StringResult(), Is.EqualTo("original"));
    }

    [Test]
    public void PrefixReturningFalseSkipsValueTypeTarget()
    {
        ApplyPatch(typeof(ExecutionControlPatches), nameof(ExecutionControlPatches.PrefixReturningFalseSkipsValueTypeTarget));
        Assert.That(StaticMethodTargets.ThrowingIntResult(), Is.Zero);
    }

    [Test]
    public void PrefixReturningFalseSkipsReferenceTypeTarget()
    {
        ApplyPatch(typeof(ExecutionControlPatches), nameof(ExecutionControlPatches.PrefixReturningFalseSkipsReferenceTypeTarget));
        Assert.That(StaticMethodTargets.ThrowingStringResult(), Is.Null);
    }
}

[TestFixture]
public sealed partial class ExecutionControlTests
{
    [Test]
    public void InnerPrefixReturningTrueRunsInnerTarget()
    {
        ApplyPatch(typeof(ExecutionControlPatches), nameof(ExecutionControlPatches.InnerPrefixReturningTrueRunsInnerTarget));
        Assert.That(OuterStaticMethodTargets.IntResult(), Is.EqualTo(1));
    }

    [Test]
    public void InnerPrefixReturningFalseSkipsInnerTarget()
    {
        ApplyPatch(typeof(ExecutionControlPatches), nameof(ExecutionControlPatches.InnerPrefixReturningFalseSkipsInnerTarget));
        Assert.That(OuterStaticMethodTargets.IntResult(), Is.Zero);
    }
}
