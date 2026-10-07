namespace Disharmony.Tests.EndToEnd.RuleBuilders;

[TestFixture]
#if NETFRAMEWORK
[Timeout(5000)]
#endif
public sealed class InlineRuleBuilderTests : PatchTestBase
{
    [Test]
    public void Prefix_UnconditionalThrowWithoutReturn_PropagatesException()
    {
        MethodInfo patch = typeof(InlineRuleBuilderPatches)
            .GetMethod(nameof(InlineRuleBuilderPatches.Prefix_UnconditionalThrowWithoutReturn_PropagatesException))!;
        List<CodeInstruction> instructions = PatchProcessor.GetOriginalInstructions(patch);
        Assert.That(instructions.Any(instruction => instruction.opcode == OpCodes.Ret), Is.False);
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.Void))!;
        Patcher.Patch(Patch.Prefix.With(patch).Options(PatchOptions.Inline).Of(target));

        var exception = Assert.Throws<ApplicationException>(() => StaticMethodTargets.Void());

        Assert.That(exception!.Message, Is.EqualTo("Unconditional inline patch exception"));
    }

    [Test]
    public void Prefix_BranchAndRefWrite_AreInlined()
    {
        MethodInfo patch = typeof(InlineRuleBuilderPatches)
            .GetMethod(nameof(InlineRuleBuilderPatches.Prefix_BranchAndRefWrite_AreInlined))!;
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.IntIdentity))!;
        Patcher.Patch(Patch.Prefix.With(patch).Options(PatchOptions.Inline).Of(target));

        int negativeResult = StaticMethodTargets.IntIdentity(-42);
        int positiveResult = StaticMethodTargets.IntIdentity(7);

        Assert.That(negativeResult, Is.EqualTo(42));
        Assert.That(positiveResult, Is.EqualTo(7));
    }

    [Test]
    public void Prefix_ArgumentOpcodeForms_AreRemapped()
    {
        InlineRuleBuilderPatches.ArgumentLoaded = 0;
        InlineRuleBuilderPatches.ArgumentAddressed = 0;
        InlineRuleBuilderPatches.ArgumentStored = 0;
        MethodInfo patch = typeof(InlineRuleBuilderPatches)
            .GetMethod(nameof(InlineRuleBuilderPatches.Prefix_ArgumentOpcodeForms_AreRemapped))!;
        List<CodeInstruction> instructions = PatchProcessor.GetOriginalInstructions(patch);
        Assert.That(instructions.Any(instruction => instruction.opcode == OpCodes.Ldarg_S), Is.True);
        Assert.That(instructions.Any(instruction => instruction.opcode == OpCodes.Ldarga_S), Is.True);
        Assert.That(instructions.Any(instruction => instruction.opcode == OpCodes.Starg_S), Is.True);
        MethodInfo target = typeof(InlineRuleBuilderTargets)
            .GetMethod(nameof(InlineRuleBuilderTargets.FiveArguments))!;
        Patcher.Patch(Patch.Prefix.With(patch).Options(PatchOptions.Inline).Of(target));

        InlineRuleBuilderTargets.FiveArguments(1, 2, 3, 4, 5);

        Assert.That(InlineRuleBuilderPatches.ArgumentLoaded, Is.EqualTo(5));
        Assert.That(InlineRuleBuilderPatches.ArgumentAddressed, Is.EqualTo(6));
        Assert.That(InlineRuleBuilderPatches.ArgumentStored, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_LocalOpcodeForms_AreRemapped()
    {
        InlineRuleBuilderPatches.Local0Observed = 0;
        InlineRuleBuilderPatches.Local4Observed = 0;
        MethodInfo patch = typeof(InlineRuleBuilderPatches)
            .GetMethod(nameof(InlineRuleBuilderPatches.Prefix_LocalOpcodeForms_AreRemapped))!;
        List<CodeInstruction> instructions = PatchProcessor.GetOriginalInstructions(patch);
        Assert.That(instructions.Any(instruction => instruction.opcode == OpCodes.Stloc_0), Is.True);
        Assert.That(instructions.Any(instruction => instruction.opcode == OpCodes.Stloc_S), Is.True);
        Assert.That(instructions.Any(instruction => instruction.opcode == OpCodes.Ldloc_S), Is.True);
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.Void))!;
        Patcher.Patch(Patch.Prefix.With(patch).Options(PatchOptions.Inline).Of(target));

        StaticMethodTargets.Void();

        Assert.That(InlineRuleBuilderPatches.Local0Observed, Is.EqualTo(10));
        Assert.That(InlineRuleBuilderPatches.Local4Observed, Is.EqualTo(14));
    }

    [Test]
    public void Prefix_SwitchTargets_AreRemapped()
    {
        InlineRuleBuilderPatches.SwitchObserved = 0;
        MethodInfo patch = typeof(InlineRuleBuilderPatches)
            .GetMethod(nameof(InlineRuleBuilderPatches.Prefix_SwitchTargets_AreRemapped))!;
        List<CodeInstruction> instructions = PatchProcessor.GetOriginalInstructions(patch);
        Assert.That(instructions.Any(instruction => instruction.opcode == OpCodes.Switch), Is.True);
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.IntIdentity))!;
        Patcher.Patch(Patch.Prefix.With(patch).Options(PatchOptions.Inline).Of(target));

        Assert.That(StaticMethodTargets.IntIdentity(0), Is.EqualTo(0));
        Assert.That(InlineRuleBuilderPatches.SwitchObserved, Is.EqualTo(10));
        Assert.That(StaticMethodTargets.IntIdentity(1), Is.EqualTo(1));
        Assert.That(InlineRuleBuilderPatches.SwitchObserved, Is.EqualTo(11));
        Assert.That(StaticMethodTargets.IntIdentity(2), Is.EqualTo(2));
        Assert.That(InlineRuleBuilderPatches.SwitchObserved, Is.EqualTo(12));
        Assert.That(StaticMethodTargets.IntIdentity(3), Is.EqualTo(3));
        Assert.That(InlineRuleBuilderPatches.SwitchObserved, Is.EqualTo(13));
        Assert.That(StaticMethodTargets.IntIdentity(4), Is.EqualTo(4));
        Assert.That(InlineRuleBuilderPatches.SwitchObserved, Is.EqualTo(14));
        Assert.That(StaticMethodTargets.IntIdentity(5), Is.EqualTo(5));
        Assert.That(InlineRuleBuilderPatches.SwitchObserved, Is.EqualTo(99));
    }

    [Test]
    public void Prefix_ExceptionHandling_TryCatch_WithoutCarriedStack()
    {
        MethodInfo patch = typeof(InlineRuleBuilderPatches)
            .GetMethod(nameof(InlineRuleBuilderPatches.Prefix_ExceptionHandling_TryCatch_WithoutCarriedStack))!;
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.IntIdentity))!;
        Patcher.Patch(Patch.Prefix.With(patch).Options(PatchOptions.Inline).Of(target));

        int tryResult = StaticMethodTargets.IntIdentity(7);
        int catchResult = StaticMethodTargets.IntIdentity(-7);

        Assert.That(tryResult, Is.EqualTo(42));
        Assert.That(catchResult, Is.EqualTo(-1));
    }

    [Test]
    public void InnerPrefix_ExceptionHandling_TryCatch_WithCarriedStack()
    {
        MethodInfo patch = typeof(InlineRuleBuilderPatches)
            .GetMethod(nameof(InlineRuleBuilderPatches.InnerPrefix_ExceptionHandling_TryCatch_WithCarriedStack))!;
        MethodInfo target = typeof(InlineRuleBuilderTargets)
            .GetMethod(nameof(InlineRuleBuilderTargets.InnerPrefix_ExceptionHandling_TryCatch_WithCarriedStack))!;
        MethodInfo inner = typeof(InlineRuleBuilderTargets)
            .GetMethod(nameof(InlineRuleBuilderTargets.InnerPrefix_ExceptionHandling_TryCatch_WithCarriedStack_Inner))!;
        Patcher.Patch(Patch.Prefix.Inner(inner).With(patch)
            .Options(PatchOptions.Inline).Of(target));

        int tryResult = InlineRuleBuilderTargets.InnerPrefix_ExceptionHandling_TryCatch_WithCarriedStack(false);
        int catchResult = InlineRuleBuilderTargets.InnerPrefix_ExceptionHandling_TryCatch_WithCarriedStack(true);

        Assert.That(tryResult, Is.EqualTo(42));
        Assert.That(catchResult, Is.EqualTo(42));
    }
}
