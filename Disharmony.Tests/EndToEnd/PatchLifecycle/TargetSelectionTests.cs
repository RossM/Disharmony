namespace Disharmony.Tests.EndToEnd.PatchLifecycle;

public static class TargetSelectionPatches
{
    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.MutableProperty), MemberType.Setter)]
    public static void TargetAttributeCanSelectPropertySetter(ref int value) => value = 42;

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.MutableProperty), MemberType.Getter)]
    public static void TargetAttributeCanSelectPropertyGetter(ref int __result) => __result = 42;

    [Postfix]
    [Target(
        typeof(StaticMethodTargets),
        nameof(StaticMethodTargets.GenericIdentity),
        parameterTypes: [typeof(int)])]
    public static void TargetAttributeCanSelectClosedGenericMethod(ref int __result) => __result = 42;
}

[TestFixture]
public sealed class TargetSelectionTests : PatchTestBase
{
    [Test]
    public void TargetAttributeCanSelectPropertySetter()
    {
        StaticMethodTargets.MutableProperty = 0;
        ApplyPatch(typeof(TargetSelectionPatches), nameof(TargetSelectionPatches.TargetAttributeCanSelectPropertySetter));

        StaticMethodTargets.MutableProperty = 1;

        Assert.That(StaticMethodTargets.MutableProperty, Is.EqualTo(42));
    }

    [Test]
    public void TargetAttributeCanSelectPropertyGetter()
    {
        StaticMethodTargets.MutableProperty = 1;
        ApplyPatch(typeof(TargetSelectionPatches), nameof(TargetSelectionPatches.TargetAttributeCanSelectPropertyGetter));

        Assert.That(StaticMethodTargets.MutableProperty, Is.EqualTo(42));
    }

    [Test]
    public void TargetAttributeCanSelectClosedGenericMethod()
    {
        Assert.Throws<PatchException>(() =>
            ApplyPatch(typeof(TargetSelectionPatches), nameof(TargetSelectionPatches.TargetAttributeCanSelectClosedGenericMethod)));
    }
}
