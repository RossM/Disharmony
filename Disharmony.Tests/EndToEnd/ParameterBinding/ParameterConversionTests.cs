namespace Disharmony.Tests.EndToEnd.ParameterBinding;

public static class ParameterConversionPatches
{
    public static object? objectObserved;
    public static int? nullableObserved;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntArgument))]
    public static void Prefix_ValueTypeParameter_Object(object value) => objectObserved = value;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntArgument))]
    public static void Prefix_ValueTypeParameter_Nullable(int? value) => nullableObserved = value;
}

[TestFixture]
public sealed class ParameterConversionTests : PatchTestBase
{
    [Test]
    public void Prefix_ValueTypeParameter_Object()
    {
        ParameterConversionPatches.objectObserved = null;
        ApplyPatch(
            typeof(ParameterConversionPatches),
            nameof(ParameterConversionPatches.Prefix_ValueTypeParameter_Object));

        StaticMethodTargets.IntArgument(42);

        Assert.That(ParameterConversionPatches.objectObserved, Is.TypeOf<int>());
        Assert.That(ParameterConversionPatches.objectObserved, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_ValueTypeParameter_Nullable()
    {
        ParameterConversionPatches.nullableObserved = null;
        ApplyPatch(
            typeof(ParameterConversionPatches),
            nameof(ParameterConversionPatches.Prefix_ValueTypeParameter_Nullable));

        StaticMethodTargets.IntArgument(42);

        Assert.That(ParameterConversionPatches.nullableObserved, Is.EqualTo((int?)42));
    }
}
