namespace Disharmony.Tests.EndToEnd.Patching;

public static class PatchReturnTypeValidationPatches
{
    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntResult))]
    public static int PostfixReturningNonVoidIsRejected() => 42;

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.IntResult))]
    public static int InnerPostfixReturningNonVoidIsRejected() => 42;
}

[TestFixture]
public sealed partial class PatchReturnTypeValidationTests : PatchTestBase
{
    [Test]
    public void PostfixReturningNonVoidIsRejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(PatchReturnTypeValidationPatches),
                nameof(PatchReturnTypeValidationPatches.PostfixReturningNonVoidIsRejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<PatchDefinitionException>());
        Assert.That(exception.InnerException!.Message, Does.EndWith("Postfix must return 'void'"));
    }
}

[TestFixture]
public sealed partial class PatchReturnTypeValidationTests
{
    [Test]
    public void InnerPostfixReturningNonVoidIsRejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(PatchReturnTypeValidationPatches),
                nameof(PatchReturnTypeValidationPatches.InnerPostfixReturningNonVoidIsRejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<PatchDefinitionException>());
        Assert.That(exception.InnerException!.Message, Does.EndWith("Postfix must return 'void'"));
    }
}
