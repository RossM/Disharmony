namespace Disharmony.Tests.EndToEnd.PatchLifecycle;

public static class PatchRollbackPatches
{
    public static void Patch_MultipleConfigs_RollsBackEarlierConfigWhenLaterConfigIsInvalid_GroupedPostfix(
        ref int __result) => __result = 42;

    public static void Patch_MultipleConfigs_RollsBackEarlierConfigWhenLaterConfigIsInvalid_IndependentPostfix(
        ref int __result) => __result = 43;

    public static void Patch_MultipleTargets_RollsBackEarlierTargetWhenLaterTargetIsInvalid(ref int __result) =>
        __result = 42;

    public static void Patch_MultipleTargets_ApplicationFailures_ReportAllContinueAndRollBack(ref int __result) =>
        __result = 42;

    public static void Patch_ApplicationFailure_WithThrowingHandler_CleansPendingUpdates_FailedPostfix(
        ref int __result) => __result = 42;

    public static void Patch_ApplicationFailure_WithThrowingHandler_CleansPendingUpdates_SubsequentPostfix(
        ref int __result) => __result = 43;

    public static void Patch_ApplicationFailure_DoesNotStrandRegistration_ExistingPostfix(ref int __result) =>
        __result = 40;

    public static void Patch_ApplicationFailure_DoesNotStrandRegistration_FailedPostfix(ref int __result) =>
        __result = 42;

    public static void Patch_ValidationFailure_ClearsPendingUpdatesBeforeNextPatch_RolledBackPostfix(
        ref int __result) => __result = 42;

    public static void Patch_ValidationFailure_ClearsPendingUpdatesBeforeNextPatch_SubsequentPostfix(
        ref int __result) => __result = 43;
}

public sealed class PatchRollbackPatchAllPatches
{
    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.RegistrationResultA))]
    public static void PatchAll_Type_RollsBackEarlierPatchWhenLaterPatchIsInvalid_ValidPostfix(ref int __result) =>
        __result = 42;

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public void PatchAll_Type_RollsBackEarlierPatchWhenLaterPatchIsInvalid_InvalidInstancePrefix() { }
}

[TestFixture]
public sealed class PatchRollbackTests : PatchTestBase
{
    [Test]
    public void Patch_MultipleConfigs_RollsBackEarlierConfigWhenLaterConfigIsInvalid()
    {
        MethodInfo groupedPatch = typeof(PatchRollbackPatches)
            .GetMethod(nameof(PatchRollbackPatches
                .Patch_MultipleConfigs_RollsBackEarlierConfigWhenLaterConfigIsInvalid_GroupedPostfix))!;
        MethodInfo independentPatch = typeof(PatchRollbackPatches)
            .GetMethod(nameof(PatchRollbackPatches
                .Patch_MultipleConfigs_RollsBackEarlierConfigWhenLaterConfigIsInvalid_IndependentPostfix))!;
        MethodInfo groupedTarget = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.RegistrationResultA))!;
        MethodInfo independentTarget = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.RegistrationResultB))!;
        MethodInfo invalidTarget = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.Void))!;

        Patcher.Patch(Patch.Postfix.With(independentPatch).Of(independentTarget));

        PatchConfig validPatch = Patch.Postfix.With(groupedPatch).Of(groupedTarget);
        PatchConfig invalidPatch = Patch.Prefix.Of(invalidTarget);
        Assert.Throws<ArgumentException>(() => Patcher.Patch(validPatch, invalidPatch));

        Patcher.ForceApply();

        Assert.That(StaticMethodTargets.RegistrationResultA(), Is.EqualTo(1));
        Assert.That(StaticMethodTargets.RegistrationResultB(), Is.EqualTo(43));
    }

    [Test]
    public void Patch_MultipleTargets_RollsBackEarlierTargetWhenLaterTargetIsInvalid()
    {
        MethodInfo patch = typeof(PatchRollbackPatches)
            .GetMethod(nameof(PatchRollbackPatches
                .Patch_MultipleTargets_RollsBackEarlierTargetWhenLaterTargetIsInvalid))!;
        MethodInfo validTarget = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.RegistrationResultA))!;
        MethodInfo invalidTarget = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.Void))!;

        Assert.Throws<PatchException>(() =>
            Patcher.Patch(Patch.Postfix.With(patch), validTarget, invalidTarget));

        Patcher.ForceApply();

        Assert.That(StaticMethodTargets.RegistrationResultA(), Is.EqualTo(1));
    }

    [Test]
    public void PatchAll_Type_RollsBackEarlierPatchWhenLaterPatchIsInvalid()
    {
        Assert.Throws<PatchException>(() => Patcher.PatchAll(typeof(PatchRollbackPatchAllPatches)));

        Patcher.ForceApply();

        Assert.That(StaticMethodTargets.RegistrationResultA(), Is.EqualTo(1));
    }

#if DEBUG
    [Test]
    public void Patch_MultipleTargets_ApplicationFailures_ReportAllContinueAndRollBack()
    {
        MethodInfo patch = typeof(PatchRollbackPatches)
            .GetMethod(nameof(PatchRollbackPatches
                .Patch_MultipleTargets_ApplicationFailures_ReportAllContinueAndRollBack))!;
        MethodInfo firstTarget = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.RegistrationResultA))!;
        MethodInfo secondTarget = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.RegistrationResultB))!;
        MethodInfo thirdTarget = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.IntResult))!;
        var firstFailure = new InvalidOperationException("First injected application failure");
        var secondFailure = new InvalidOperationException("Second injected application failure");
        int applicationAttempts = 0;
        Action hook = () =>
        {
            applicationAttempts++;
            if (applicationAttempts == 1)
                throw firstFailure;
            if (applicationAttempts == 3)
                throw secondFailure;
        };
        List<Exception> reportedExceptions = [];
        Action<Exception> handler = reportedExceptions.Add;
        Patcher.RuntimeExceptionHandler -= ThrowRuntimeException;
        Patcher.RuntimeExceptionHandler += handler;
        HarmonyInterface.Instance.ApplyPatchHookForTesting += hook;

        try
        {
            RuntimePatchException? exception = Assert.Throws<RuntimePatchException>(() =>
                Patcher.Patch(Patch.Postfix.With(patch), firstTarget, secondTarget, thirdTarget));

            Assert.Multiple(() =>
            {
                Assert.That(applicationAttempts, Is.EqualTo(3));
                Assert.That(reportedExceptions, Has.Count.EqualTo(2));
                Assert.That(reportedExceptions[0], Is.SameAs(firstFailure));
                Assert.That(reportedExceptions[1], Is.SameAs(secondFailure));
                Assert.That(exception!.InnerException, Is.SameAs(firstFailure));
            });
        }
        finally
        {
            HarmonyInterface.Instance.ApplyPatchHookForTesting -= hook;
            Patcher.RuntimeExceptionHandler -= handler;
            Patcher.RuntimeExceptionHandler += ThrowRuntimeException;
        }

        Assert.Multiple(() =>
        {
            Assert.That(StaticMethodTargets.RegistrationResultA(), Is.EqualTo(1));
            Assert.That(StaticMethodTargets.RegistrationResultB(), Is.EqualTo(2));
            Assert.That(StaticMethodTargets.IntResult(), Is.EqualTo(1));
        });
    }

    [Test]
    public void Patch_ApplicationFailure_WithThrowingHandler_CleansPendingUpdates()
    {
        MethodInfo failedPatch = typeof(PatchRollbackPatches)
            .GetMethod(nameof(PatchRollbackPatches
                .Patch_ApplicationFailure_WithThrowingHandler_CleansPendingUpdates_FailedPostfix))!;
        MethodInfo subsequentPatch = typeof(PatchRollbackPatches)
            .GetMethod(nameof(PatchRollbackPatches
                .Patch_ApplicationFailure_WithThrowingHandler_CleansPendingUpdates_SubsequentPostfix))!;
        MethodInfo failedTarget = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.RegistrationResultA))!;
        MethodInfo subsequentTarget = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.RegistrationResultB))!;
        var injectedFailure = new InvalidOperationException("Injected application failure");
        Action hook = () => throw injectedFailure;
        HarmonyInterface.Instance.ApplyPatchHookForTesting += hook;

        try
        {
            InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() =>
                Patcher.Patch(Patch.Postfix.With(failedPatch).Of(failedTarget)));
            Assert.That(exception!.InnerException, Is.SameAs(injectedFailure));
        }
        finally
        {
            HarmonyInterface.Instance.ApplyPatchHookForTesting -= hook;
        }

        PatchHandle handle = Patcher.Patch(Patch.Postfix.With(subsequentPatch).Of(subsequentTarget));

        Assert.Multiple(() =>
        {
            Assert.That(StaticMethodTargets.RegistrationResultA(), Is.EqualTo(1));
            Assert.That(StaticMethodTargets.RegistrationResultB(), Is.EqualTo(43));
        });

        Patcher.Unpatch(handle);

        Assert.That(StaticMethodTargets.RegistrationResultB(), Is.EqualTo(2));
    }

    [Test]
    public void Patch_ApplicationFailure_DoesNotStrandRegistration()
    {
        MethodInfo existingPatch = typeof(PatchRollbackPatches)
            .GetMethod(nameof(PatchRollbackPatches
                .Patch_ApplicationFailure_DoesNotStrandRegistration_ExistingPostfix))!;
        MethodInfo failedPatch = typeof(PatchRollbackPatches)
            .GetMethod(nameof(PatchRollbackPatches
                .Patch_ApplicationFailure_DoesNotStrandRegistration_FailedPostfix))!;
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.RegistrationResultA))!;
        PatchHandle existingHandle = Patcher.Patch(Patch.Postfix.With(existingPatch).Of(target));

        Assert.That(StaticMethodTargets.RegistrationResultA(), Is.EqualTo(40));

        var injectedFailure = new InvalidOperationException("Injected application failure");
        bool failureInjected = false;
        Action hook = () =>
        {
            if (failureInjected)
                return;

            failureInjected = true;
            throw injectedFailure;
        };
        List<Exception> reportedExceptions = [];
        Action<Exception> handler = reportedExceptions.Add;
        Patcher.RuntimeExceptionHandler -= ThrowRuntimeException;
        Patcher.RuntimeExceptionHandler += handler;
        HarmonyInterface.Instance.ApplyPatchHookForTesting += hook;

        try
        {
            RuntimePatchException? exception = Assert.Throws<RuntimePatchException>(() =>
                Patcher.Patch(Patch.Postfix.With(failedPatch).Of(target)));

            Assert.Multiple(() =>
            {
                Assert.That(reportedExceptions, Is.EqualTo(new[] { injectedFailure }));
                Assert.That(exception!.InnerException, Is.SameAs(injectedFailure));
            });
        }
        finally
        {
            HarmonyInterface.Instance.ApplyPatchHookForTesting -= hook;
            Patcher.RuntimeExceptionHandler -= handler;
            Patcher.RuntimeExceptionHandler += ThrowRuntimeException;
        }

        Assert.That(StaticMethodTargets.RegistrationResultA(), Is.EqualTo(40));

        Patcher.Unpatch(existingHandle);

        Assert.That(StaticMethodTargets.RegistrationResultA(), Is.EqualTo(1));
    }

    [Test]
    public void Patch_ValidationFailure_ClearsPendingUpdatesBeforeNextPatch()
    {
        MethodInfo rolledBackPatch = typeof(PatchRollbackPatches)
            .GetMethod(nameof(PatchRollbackPatches
                .Patch_ValidationFailure_ClearsPendingUpdatesBeforeNextPatch_RolledBackPostfix))!;
        MethodInfo subsequentPatch = typeof(PatchRollbackPatches)
            .GetMethod(nameof(PatchRollbackPatches
                .Patch_ValidationFailure_ClearsPendingUpdatesBeforeNextPatch_SubsequentPostfix))!;
        MethodInfo rolledBackTarget = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.RegistrationResultA))!;
        MethodInfo subsequentTarget = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.RegistrationResultB))!;
        MethodInfo invalidTarget = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.Void))!;
        PatchConfig validPatch = Patch.Postfix.With(rolledBackPatch).Of(rolledBackTarget);
        PatchConfig invalidPatch = Patch.Prefix.Of(invalidTarget);

        Assert.Throws<ArgumentException>(() => Patcher.Patch(validPatch, invalidPatch));

        int applicationAttempts = 0;
        Action hook = () => applicationAttempts++;
        HarmonyInterface.Instance.ApplyPatchHookForTesting += hook;
        PatchHandle handle;

        try
        {
            handle = Patcher.Patch(Patch.Postfix.With(subsequentPatch).Of(subsequentTarget));
        }
        finally
        {
            HarmonyInterface.Instance.ApplyPatchHookForTesting -= hook;
        }

        Assert.Multiple(() =>
        {
            Assert.That(applicationAttempts, Is.EqualTo(1));
            Assert.That(StaticMethodTargets.RegistrationResultA(), Is.EqualTo(1));
            Assert.That(StaticMethodTargets.RegistrationResultB(), Is.EqualTo(43));
        });

        Patcher.Unpatch(handle);
    }
#endif
}
