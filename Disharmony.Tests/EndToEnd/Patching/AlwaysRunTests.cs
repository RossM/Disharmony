namespace Disharmony.Tests.EndToEnd.Patching;

public static class AlwaysRunPatches
{
    public static readonly List<string> events = [];
    public static Exception? observedException;
    public static Exception? replacementException;
    public static object? observedObject;
    public static int observedResult;
    public static string? observedState;

    [Prefix]
    [Priority(PatchPriority.Low)]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.PriorityTarget))]
    public static void Ordering_AlwaysRunPatchesNestOutsideRegularPatches_AlwaysPrefix() =>
        StaticMethodTargets.PriorityEvents.Add("always-prefix");

    [Prefix]
    [Priority(PatchPriority.High)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.PriorityTarget))]
    public static void Ordering_AlwaysRunPatchesNestOutsideRegularPatches_RegularPrefix() =>
        StaticMethodTargets.PriorityEvents.Add("regular-prefix");

    [Postfix]
    [Priority(PatchPriority.High)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.PriorityTarget))]
    public static void Ordering_AlwaysRunPatchesNestOutsideRegularPatches_RegularPostfix() =>
        StaticMethodTargets.PriorityEvents.Add("regular-postfix");

    [Postfix]
    [Priority(PatchPriority.Low)]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.PriorityTarget))]
    public static void Ordering_AlwaysRunPatchesNestOutsideRegularPatches_AlwaysPostfix() =>
        StaticMethodTargets.PriorityEvents.Add("always-postfix");

    [Prefix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static bool Validation_AlwaysRunPrefixReturningBoolIsRejected() => true;

    [Prefix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntResult))]
    public static void Validation_AlwaysRunPrefixBindingResultIsRejected(int __result) { }

    [Prefix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.IntResult))]
    public static bool Validation_AlwaysRunInnerPrefixReturningBoolIsRejected() => true;

    [Prefix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.IntResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.IntResult))]
    public static void Validation_AlwaysRunInnerPrefixBindingResultIsRejected(int __result) { }

    [Prefix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Validation_AlwaysRunPrefixBindingExceptionIsRejected(
        [ExceptionAttribute] Exception? exception) { }

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Validation_RegularPostfixBindingExceptionIsRejected(
        [ExceptionAttribute] Exception? exception) { }

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Validation_ExceptionBindingWithIncompatibleTypeIsRejected(
        [ExceptionAttribute] string? exception) { }

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Validation_ExceptionBindingWithVariantWriteableReferenceIsRejected(
        [ExceptionAttribute] ref object? exception) { }

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowInvalidOperationException))]
    public static void ExceptionBinding_ThrownException_ReadAsObjectByReadonlyReference(
        [ExceptionAttribute] in object? exception) => observedObject = exception;

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void ExceptionBinding_NoException_PassesNull(Exception? __exception) =>
        observedException = __exception;

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowInvalidOperationException))]
    public static void ExceptionBinding_ThrownException_ReadByValueAndRethrown(Exception __exception) =>
        observedException = __exception;

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowInvalidOperationException))]
    public static void ExceptionBinding_ThrownException_WriteReplacementByReference(
        [ExceptionAttribute] ref Exception? exception)
    {
        replacementException = new ArgumentException("replacement");
        exception = replacementException;
    }

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowInvalidOperationException))]
    public static void ExceptionBinding_ThrownException_SuppressByReference(ref Exception? __exception)
    {
        observedException = __exception;
        __exception = null;
    }

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowInvalidOperationException))]
    public static void ExceptionBinding_ThrownException_ReadByReadonlyReference(in Exception? __exception) =>
        observedException = __exception;

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowInvalidOperationException))]
    public static void ExceptionBinding_ThrownException_ReadAsObjectByValue(
        [ExceptionAttribute] object? exception) => observedObject = exception;

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowInvalidOperationException))]
    public static void ExceptionBinding_ThrownException_WriteDerivedByOutReference(
        [ExceptionAttribute] out InvalidOperationException? exception)
    {
        exception = new InvalidOperationException("derived-replacement");
        replacementException = exception;
    }

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void ExceptionBinding_NoException_WriteNewExceptionByReference(ref Exception? __exception)
    {
        replacementException = new ArgumentException("introduced");
        __exception = replacementException;
    }

    [Postfix]
    [Priority(PatchPriority.Low)]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowInvalidOperationException))]
    public static void ExceptionChain_ReplacementIsVisibleToLaterAlwaysRunPostfix_Replace(ref Exception? __exception)
    {
        events.Add("replace");
        replacementException = new ArgumentException("chain-replacement");
        __exception = replacementException;
    }

    [Postfix]
    [Priority(PatchPriority.High)]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowInvalidOperationException))]
    public static void ExceptionChain_ReplacementIsVisibleToLaterAlwaysRunPostfix_ObserveAndSuppress(
        ref Exception? __exception)
    {
        events.Add("observe-and-suppress");
        observedException = __exception;
        __exception = null;
    }

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowInvalidOperationExceptionWithResult))]
    public static void ResultBinding_ThrownTarget_SuppressExceptionAndWriteResult(
        ref Exception? __exception,
        ref int __result)
    {
        observedException = __exception;
        __exception = null;
        __result = 42;
    }

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntResult))]
    public static void ResultBinding_SuccessfulTarget_ReadExceptionAndWriteResult(
        Exception? __exception,
        ref int __result)
    {
        observedException = __exception;
        observedResult = __result;
        __result = 42;
    }

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowingIntResult))]
    public static bool ResultBinding_SkippedTarget_AlwaysRunPostfixObservesDefaultResult_Prefix() => false;

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowingIntResult))]
    public static void ResultBinding_SkippedTarget_AlwaysRunPostfixObservesDefaultResult_Postfix(
        Exception? __exception,
        ref int __result)
    {
        observedException = __exception;
        observedResult = __result;
        __result = 42;
    }

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntResult))]
    public static void ResultBinding_ThrowingRegularPrefix_AlwaysRunPostfixSuppliesResult_Prefix() =>
        throw new InvalidOperationException("regular-prefix-result");

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.IntResult))]
    public static void ResultBinding_ThrowingRegularPrefix_AlwaysRunPostfixSuppliesResult_Postfix(
        ref Exception? __exception,
        ref int __result)
    {
        observedException = __exception;
        __exception = null;
        __result = 42;
    }

    public static void StateBinding_ExceptionalTarget_Prefix(out string __state) =>
        __state = "created-before-target";

    public static void StateBinding_ExceptionalTarget_Postfix(string __state, ref Exception? __exception)
    {
        observedState = __state;
        observedException = __exception;
        __exception = null;
    }

    [Prefix]
    [Priority(PatchPriority.High)]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.PriorityTarget))]
    public static void Ordering_MultipleAlwaysRunPatchesHonorPriorityWithinOuterGroup_HighPrefix() =>
        StaticMethodTargets.PriorityEvents.Add("always-high-prefix");

    [Prefix]
    [Priority(PatchPriority.Low)]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.PriorityTarget))]
    public static void Ordering_MultipleAlwaysRunPatchesHonorPriorityWithinOuterGroup_LowPrefix() =>
        StaticMethodTargets.PriorityEvents.Add("always-low-prefix");

    [Postfix]
    [Priority(PatchPriority.Low)]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.PriorityTarget))]
    public static void Ordering_MultipleAlwaysRunPatchesHonorPriorityWithinOuterGroup_LowPostfix() =>
        StaticMethodTargets.PriorityEvents.Add("always-low-postfix");

    [Postfix]
    [Priority(PatchPriority.High)]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.PriorityTarget))]
    public static void Ordering_MultipleAlwaysRunPatchesHonorPriorityWithinOuterGroup_HighPostfix() =>
        StaticMethodTargets.PriorityEvents.Add("always-high-postfix");

    [Prefix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void RegularPrefixException_DoesNotPreventAlwaysRunPatches_AlwaysPrefix() =>
        events.Add("always-prefix");

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void RegularPrefixException_DoesNotPreventAlwaysRunPatches_RegularPrefix()
    {
        events.Add("regular-prefix");
        throw new InvalidOperationException("regular-prefix");
    }

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void RegularPrefixException_DoesNotPreventAlwaysRunPatches_AlwaysPostfix()
    {
        events.Add("always-postfix");
    }

    [Prefix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void RegularPostfixException_DoesNotPreventAlwaysRunPatches_AlwaysPrefix() =>
        events.Add("always-prefix");

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void RegularPostfixException_DoesNotPreventAlwaysRunPatches_RegularPostfix()
    {
        events.Add("regular-postfix");
        throw new InvalidOperationException("regular-postfix");
    }

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void RegularPostfixException_DoesNotPreventAlwaysRunPatches_AlwaysPostfix()
    {
        events.Add("always-postfix");
    }

    [Postfix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowInvalidOperationException))]
    public static void TargetException_SkipsRegularPostfixButRunsAlwaysRunPostfix_RegularPostfix() =>
        events.Add("regular-postfix");

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.ThrowInvalidOperationException))]
    public static void TargetException_SkipsRegularPostfixButRunsAlwaysRunPostfix_AlwaysPostfix(Exception __exception)
    {
        events.Add("always-postfix");
        observedException = __exception;
    }

    [Prefix]
    [Priority(PatchPriority.High)]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void AlwaysRunPrefixException_PreventsOtherAlwaysRunPatches_ThrowingPrefix()
    {
        events.Add("throwing-always-prefix");
        throw new InvalidOperationException("always-prefix");
    }

    [Prefix]
    [Priority(PatchPriority.Low)]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void AlwaysRunPrefixException_PreventsOtherAlwaysRunPatches_LaterPrefix() =>
        events.Add("later-always-prefix");

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void AlwaysRunPrefixException_PreventsOtherAlwaysRunPatches_Postfix() =>
        events.Add("always-postfix");

    [Postfix]
    [Priority(PatchPriority.Low)]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void AlwaysRunPostfixException_PreventsLaterAlwaysRunPostfix_ThrowingPostfix()
    {
        events.Add("throwing-always-postfix");
        throw new InvalidOperationException("always-postfix");
    }

    [Postfix]
    [Priority(PatchPriority.High)]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void AlwaysRunPostfixException_PreventsLaterAlwaysRunPostfix_LaterPostfix() =>
        events.Add("later-always-postfix");

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.ThrowInvalidOperationException))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.CallThrowingVoid))]
    public static void InnerPostfix_ThrownException_SuppressByReference(ref Exception? __exception)
    {
        observedException = __exception;
        __exception = null;
    }

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.ThrowInvalidOperationExceptionWithResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.CallThrowingIntResult))]
    public static void InnerPostfix_ThrownException_SuppressAndWriteResult(
        ref Exception? __exception,
        ref int __result)
    {
        observedException = __exception;
        __exception = null;
        __result = 42;
    }

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Void))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.CallVoidThenThrow))]
    public static void InnerPostfix_OuterExceptionAfterInnerCall_IsNotCaptured(Exception? __exception) =>
        observedException = __exception;

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(
        typeof(ConstructorTargets),
        memberType: MemberType.Constructor,
        parameterTypes: [typeof(bool)])]
    public static void ConstructorPostfix_ThrownException_SuppressByReference(ref Exception? __exception)
    {
        observedException = __exception;
        __exception = null;
    }

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Inner(
        typeof(ConstructorTargets),
        memberType: MemberType.Constructor,
        parameterTypes: [typeof(bool)])]
    [Target(typeof(ConstructorTargets), nameof(ConstructorTargets.Create), parameterTypes: [typeof(bool)])]
    public static void InnerConstructorPostfix_ThrownException_SuppressAndWriteResult(
        ref Exception? __exception,
        ref ConstructorTargets? __result)
    {
        observedException = __exception;
        __exception = null;
        __result = new ConstructorTargets();
    }

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.EnumerateThenThrow))]
    public static void IteratorPostfix_EnumerationException_IsNotCaptured(Exception? __exception) =>
        observedException = __exception;

    [Postfix]
    [PatchOptions(PatchOptions.AlwaysRun)]
    [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.ThrowInvalidOperationExceptionWithResult))]
    [Target(typeof(OuterStaticMethodTargets), nameof(OuterStaticMethodTargets.EnumerateThrowingInnerResult))]
    public static void IteratorInnerPostfix_ThrownException_SuppressAndWriteResult(
        ref Exception? __exception,
        ref int __result)
    {
        observedException = __exception;
        __exception = null;
        __result = 42;
    }
}

[TestFixture]
public sealed class AlwaysRunTests : PatchTestBase
{
    [Test]
    public void Ordering_AlwaysRunPatchesNestOutsideRegularPatches()
    {
        StaticMethodTargets.PriorityEvents.Clear();
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.Ordering_AlwaysRunPatchesNestOutsideRegularPatches_RegularPostfix));
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.Ordering_AlwaysRunPatchesNestOutsideRegularPatches_AlwaysPrefix));
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.Ordering_AlwaysRunPatchesNestOutsideRegularPatches_AlwaysPostfix));
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.Ordering_AlwaysRunPatchesNestOutsideRegularPatches_RegularPrefix));

        StaticMethodTargets.PriorityTarget();

        Assert.That(StaticMethodTargets.PriorityEvents, Is.EqualTo(new[]
        {
            "always-prefix",
            "regular-prefix",
            "target",
            "regular-postfix",
            "always-postfix",
        }));
    }

    [Test]
    public void Validation_AlwaysRunPrefixReturningBoolIsRejected()
    {
        Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.Validation_AlwaysRunPrefixReturningBoolIsRejected)));
    }

    [Test]
    public void Validation_AlwaysRunPrefixBindingResultIsRejected()
    {
        Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.Validation_AlwaysRunPrefixBindingResultIsRejected)));
    }

    [Test]
    public void Validation_AlwaysRunInnerPrefixReturningBoolIsRejected()
    {
        Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.Validation_AlwaysRunInnerPrefixReturningBoolIsRejected)));
    }

    [Test]
    public void Validation_AlwaysRunInnerPrefixBindingResultIsRejected()
    {
        Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.Validation_AlwaysRunInnerPrefixBindingResultIsRejected)));
    }

    [Test]
    public void Validation_AlwaysRunPrefixBindingExceptionIsRejected()
    {
        Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.Validation_AlwaysRunPrefixBindingExceptionIsRejected)));
    }

    [Test]
    public void Validation_RegularPostfixBindingExceptionIsRejected()
    {
        Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.Validation_RegularPostfixBindingExceptionIsRejected)));
    }

    [Test]
    public void ExceptionBinding_NoException_PassesNull()
    {
        AlwaysRunPatches.observedException = new Exception("sentinel");
        ApplyPatch(typeof(AlwaysRunPatches), nameof(AlwaysRunPatches.ExceptionBinding_NoException_PassesNull));

        StaticMethodTargets.Void();

        Assert.That(AlwaysRunPatches.observedException, Is.Null);
    }

    [Test]
    public void ExceptionBinding_ThrownException_ReadByValueAndRethrown()
    {
        AlwaysRunPatches.observedException = null;
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.ExceptionBinding_ThrownException_ReadByValueAndRethrown));

        var exception = Assert.Throws<InvalidOperationException>(StaticMethodTargets.ThrowInvalidOperationException);

        Assert.That(exception!.Message, Is.EqualTo("target"));
        Assert.That(AlwaysRunPatches.observedException, Is.SameAs(exception));
        Assert.That(exception.StackTrace, Does.Contain(nameof(StaticMethodTargets.ThrowInvalidOperationException)));
    }

    [Test]
    public void ExceptionBinding_ThrownException_WriteReplacementByReference()
    {
        AlwaysRunPatches.replacementException = null;
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.ExceptionBinding_ThrownException_WriteReplacementByReference));

        var exception = Assert.Throws<ArgumentException>(StaticMethodTargets.ThrowInvalidOperationException);

        Assert.That(exception, Is.SameAs(AlwaysRunPatches.replacementException));
        Assert.That(exception!.Message, Is.EqualTo("replacement"));
    }

    [Test]
    public void ExceptionBinding_ThrownException_SuppressByReference()
    {
        AlwaysRunPatches.observedException = null;
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.ExceptionBinding_ThrownException_SuppressByReference));

        Assert.DoesNotThrow(StaticMethodTargets.ThrowInvalidOperationException);
        Assert.That(AlwaysRunPatches.observedException, Is.TypeOf<InvalidOperationException>()
            .With.Message.EqualTo("target"));
    }

    [Test]
    public void RegularPrefixException_DoesNotPreventAlwaysRunPatches()
    {
        AlwaysRunPatches.events.Clear();
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.RegularPrefixException_DoesNotPreventAlwaysRunPatches_RegularPrefix));
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.RegularPrefixException_DoesNotPreventAlwaysRunPatches_AlwaysPostfix));
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.RegularPrefixException_DoesNotPreventAlwaysRunPatches_AlwaysPrefix));

        var exception = Assert.Throws<InvalidOperationException>(StaticMethodTargets.Void);
        Assert.That(exception!.Message, Is.EqualTo("regular-prefix"));
        Assert.That(AlwaysRunPatches.events, Is.EqualTo(new[]
        {
            "always-prefix",
            "regular-prefix",
            "always-postfix",
        }));
    }

    [Test]
    public void RegularPostfixException_DoesNotPreventAlwaysRunPatches()
    {
        AlwaysRunPatches.events.Clear();
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.RegularPostfixException_DoesNotPreventAlwaysRunPatches_AlwaysPostfix));
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.RegularPostfixException_DoesNotPreventAlwaysRunPatches_AlwaysPrefix));
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.RegularPostfixException_DoesNotPreventAlwaysRunPatches_RegularPostfix));

        var exception = Assert.Throws<InvalidOperationException>(StaticMethodTargets.Void);
        Assert.That(exception!.Message, Is.EqualTo("regular-postfix"));
        Assert.That(AlwaysRunPatches.events, Is.EqualTo(new[]
        {
            "always-prefix",
            "regular-postfix",
            "always-postfix",
        }));
    }

    [Test]
    public void AlwaysRunPrefixException_PreventsOtherAlwaysRunPatches()
    {
        AlwaysRunPatches.events.Clear();
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.AlwaysRunPrefixException_PreventsOtherAlwaysRunPatches_LaterPrefix));
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.AlwaysRunPrefixException_PreventsOtherAlwaysRunPatches_Postfix));
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.AlwaysRunPrefixException_PreventsOtherAlwaysRunPatches_ThrowingPrefix));

        var exception = Assert.Throws<InvalidOperationException>(StaticMethodTargets.Void);

        Assert.That(exception!.Message, Is.EqualTo("always-prefix"));
        Assert.That(AlwaysRunPatches.events, Is.EqualTo(new[] { "throwing-always-prefix" }));
    }

    [Test]
    public void AlwaysRunPostfixException_PreventsLaterAlwaysRunPostfix()
    {
        AlwaysRunPatches.events.Clear();
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.AlwaysRunPostfixException_PreventsLaterAlwaysRunPostfix_LaterPostfix));
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.AlwaysRunPostfixException_PreventsLaterAlwaysRunPostfix_ThrowingPostfix));

        var exception = Assert.Throws<InvalidOperationException>(StaticMethodTargets.Void);

        Assert.That(exception!.Message, Is.EqualTo("always-postfix"));
        Assert.That(AlwaysRunPatches.events, Is.EqualTo(new[] { "throwing-always-postfix" }));
    }

    [Test]
    public void InnerPostfix_ThrownException_SuppressByReference()
    {
        AlwaysRunPatches.observedException = null;
        ApplyPatch(typeof(AlwaysRunPatches), nameof(AlwaysRunPatches.InnerPostfix_ThrownException_SuppressByReference));

        int result = OuterStaticMethodTargets.CallThrowingVoid();

        Assert.That(result, Is.EqualTo(42));
        Assert.That(AlwaysRunPatches.observedException, Is.TypeOf<InvalidOperationException>()
            .With.Message.EqualTo("inner"));
    }

    [Test]
    public void Validation_ExceptionBindingWithIncompatibleTypeIsRejected()
    {
        Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.Validation_ExceptionBindingWithIncompatibleTypeIsRejected)));
    }

    [Test]
    public void Validation_ExceptionBindingWithVariantWriteableReferenceIsRejected()
    {
        Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.Validation_ExceptionBindingWithVariantWriteableReferenceIsRejected)));
    }

    [Test]
    public void ExceptionBinding_ThrownException_ReadByReadonlyReference()
    {
        AlwaysRunPatches.observedException = null;
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.ExceptionBinding_ThrownException_ReadByReadonlyReference));

        var exception = Assert.Throws<InvalidOperationException>(StaticMethodTargets.ThrowInvalidOperationException);

        Assert.That(AlwaysRunPatches.observedException, Is.SameAs(exception));
    }

    [Test]
    public void ExceptionBinding_ThrownException_ReadAsObjectByValue()
    {
        AlwaysRunPatches.observedObject = null;
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.ExceptionBinding_ThrownException_ReadAsObjectByValue));

        var exception = Assert.Throws<InvalidOperationException>(StaticMethodTargets.ThrowInvalidOperationException);

        Assert.That(AlwaysRunPatches.observedObject, Is.SameAs(exception));
    }

    [Test]
    public void ExceptionBinding_ThrownException_ReadAsObjectByReadonlyReference()
    {
        AlwaysRunPatches.observedObject = null;
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.ExceptionBinding_ThrownException_ReadAsObjectByReadonlyReference));

        var exception = Assert.Throws<InvalidOperationException>(StaticMethodTargets.ThrowInvalidOperationException);

        Assert.That(AlwaysRunPatches.observedObject, Is.SameAs(exception));
    }

    [Test]
    public void ExceptionBinding_ThrownException_WriteDerivedByOutReference()
    {
        AlwaysRunPatches.replacementException = null;
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.ExceptionBinding_ThrownException_WriteDerivedByOutReference));

        var exception = Assert.Throws<InvalidOperationException>(StaticMethodTargets.ThrowInvalidOperationException);

        Assert.That(exception, Is.SameAs(AlwaysRunPatches.replacementException));
        Assert.That(exception!.Message, Is.EqualTo("derived-replacement"));
    }

    [Test]
    public void ExceptionBinding_NoException_WriteNewExceptionByReference()
    {
        AlwaysRunPatches.replacementException = null;
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.ExceptionBinding_NoException_WriteNewExceptionByReference));

        var exception = Assert.Throws<ArgumentException>(StaticMethodTargets.Void);

        Assert.That(exception, Is.SameAs(AlwaysRunPatches.replacementException));
        Assert.That(exception!.Message, Is.EqualTo("introduced"));
    }

    [Test]
    public void ExceptionChain_ReplacementIsVisibleToLaterAlwaysRunPostfix()
    {
        AlwaysRunPatches.events.Clear();
        AlwaysRunPatches.observedException = null;
        AlwaysRunPatches.replacementException = null;
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.ExceptionChain_ReplacementIsVisibleToLaterAlwaysRunPostfix_ObserveAndSuppress));
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.ExceptionChain_ReplacementIsVisibleToLaterAlwaysRunPostfix_Replace));

        Assert.DoesNotThrow(StaticMethodTargets.ThrowInvalidOperationException);
        Assert.That(AlwaysRunPatches.events, Is.EqualTo(new[] { "replace", "observe-and-suppress" }));
        Assert.That(AlwaysRunPatches.observedException, Is.SameAs(AlwaysRunPatches.replacementException));
    }

    [Test]
    public void ResultBinding_ThrownTarget_SuppressExceptionAndWriteResult()
    {
        AlwaysRunPatches.observedException = null;
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.ResultBinding_ThrownTarget_SuppressExceptionAndWriteResult));

        int result = StaticMethodTargets.ThrowInvalidOperationExceptionWithResult();

        Assert.That(result, Is.EqualTo(42));
        Assert.That(AlwaysRunPatches.observedException, Is.TypeOf<InvalidOperationException>()
            .With.Message.EqualTo("target-result"));
    }

    [Test]
    public void ResultBinding_SuccessfulTarget_ReadExceptionAndWriteResult()
    {
        AlwaysRunPatches.observedException = new Exception("sentinel");
        AlwaysRunPatches.observedResult = 0;
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.ResultBinding_SuccessfulTarget_ReadExceptionAndWriteResult));

        int result = StaticMethodTargets.IntResult();

        Assert.That(AlwaysRunPatches.observedException, Is.Null);
        Assert.That(AlwaysRunPatches.observedResult, Is.EqualTo(1));
        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void ResultBinding_SkippedTarget_AlwaysRunPostfixObservesDefaultResult()
    {
        AlwaysRunPatches.observedException = new Exception("sentinel");
        AlwaysRunPatches.observedResult = -1;
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.ResultBinding_SkippedTarget_AlwaysRunPostfixObservesDefaultResult_Prefix));
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.ResultBinding_SkippedTarget_AlwaysRunPostfixObservesDefaultResult_Postfix));

        int result = StaticMethodTargets.ThrowingIntResult();

        Assert.That(AlwaysRunPatches.observedException, Is.Null);
        Assert.That(AlwaysRunPatches.observedResult, Is.Zero);
        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void ResultBinding_ThrowingRegularPrefix_AlwaysRunPostfixSuppliesResult()
    {
        AlwaysRunPatches.observedException = null;
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.ResultBinding_ThrowingRegularPrefix_AlwaysRunPostfixSuppliesResult_Postfix));
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.ResultBinding_ThrowingRegularPrefix_AlwaysRunPostfixSuppliesResult_Prefix));

        int result = StaticMethodTargets.IntResult();

        Assert.That(result, Is.EqualTo(42));
        Assert.That(AlwaysRunPatches.observedException, Is.TypeOf<InvalidOperationException>()
            .With.Message.EqualTo("regular-prefix-result"));
    }

    [Test]
    public void TargetException_SkipsRegularPostfixButRunsAlwaysRunPostfix()
    {
        AlwaysRunPatches.events.Clear();
        AlwaysRunPatches.observedException = null;
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.TargetException_SkipsRegularPostfixButRunsAlwaysRunPostfix_RegularPostfix));
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.TargetException_SkipsRegularPostfixButRunsAlwaysRunPostfix_AlwaysPostfix));

        var exception = Assert.Throws<InvalidOperationException>(StaticMethodTargets.ThrowInvalidOperationException);

        Assert.That(AlwaysRunPatches.events, Is.EqualTo(new[] { "always-postfix" }));
        Assert.That(AlwaysRunPatches.observedException, Is.SameAs(exception));
    }

    [Test]
    public void StateBinding_ExceptionalTarget_StateFlowsFromAlwaysRunPrefixToPostfix()
    {
        AlwaysRunPatches.observedException = null;
        AlwaysRunPatches.observedState = null;
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.ThrowInvalidOperationException))!;
        MethodInfo prefix = typeof(AlwaysRunPatches)
            .GetMethod(nameof(AlwaysRunPatches.StateBinding_ExceptionalTarget_Prefix))!;
        MethodInfo postfix = typeof(AlwaysRunPatches)
            .GetMethod(nameof(AlwaysRunPatches.StateBinding_ExceptionalTarget_Postfix))!;
        Patcher.Patch(
            target,
            Patch.Prefix.With(prefix).Options(PatchOptions.AlwaysRun),
            Patch.Postfix.With(postfix).Options(PatchOptions.AlwaysRun));

        Assert.DoesNotThrow(StaticMethodTargets.ThrowInvalidOperationException);
        Assert.That(AlwaysRunPatches.observedState, Is.EqualTo("created-before-target"));
        Assert.That(AlwaysRunPatches.observedException, Is.TypeOf<InvalidOperationException>()
            .With.Message.EqualTo("target"));
    }

    [Test]
    public void Ordering_MultipleAlwaysRunPatchesHonorPriorityWithinOuterGroup()
    {
        StaticMethodTargets.PriorityEvents.Clear();
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.Ordering_MultipleAlwaysRunPatchesHonorPriorityWithinOuterGroup_LowPostfix));
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.Ordering_AlwaysRunPatchesNestOutsideRegularPatches_RegularPrefix));
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.Ordering_MultipleAlwaysRunPatchesHonorPriorityWithinOuterGroup_HighPrefix));
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.Ordering_MultipleAlwaysRunPatchesHonorPriorityWithinOuterGroup_HighPostfix));
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.Ordering_AlwaysRunPatchesNestOutsideRegularPatches_RegularPostfix));
        ApplyPatch(typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.Ordering_MultipleAlwaysRunPatchesHonorPriorityWithinOuterGroup_LowPrefix));

        StaticMethodTargets.PriorityTarget();

        Assert.That(StaticMethodTargets.PriorityEvents, Is.EqualTo(new[]
        {
            "always-high-prefix",
            "always-low-prefix",
            "regular-prefix",
            "target",
            "regular-postfix",
            "always-low-postfix",
            "always-high-postfix",
        }));
    }

    [Test]
    public void InnerPostfix_ThrownException_SuppressAndWriteResult()
    {
        AlwaysRunPatches.observedException = null;
        ApplyPatch(typeof(AlwaysRunPatches), nameof(AlwaysRunPatches.InnerPostfix_ThrownException_SuppressAndWriteResult));

        int result = OuterStaticMethodTargets.CallThrowingIntResult();

        Assert.That(result, Is.EqualTo(42));
        Assert.That(AlwaysRunPatches.observedException, Is.TypeOf<InvalidOperationException>()
            .With.Message.EqualTo("inner-result"));
    }

    [Test]
    public void InnerPostfix_OuterExceptionAfterInnerCall_IsNotCaptured()
    {
        AlwaysRunPatches.observedException = new Exception("sentinel");
        ApplyPatch(typeof(AlwaysRunPatches), nameof(AlwaysRunPatches.InnerPostfix_OuterExceptionAfterInnerCall_IsNotCaptured));

        var exception = Assert.Throws<InvalidOperationException>(OuterStaticMethodTargets.CallVoidThenThrow);

        Assert.That(exception!.Message, Is.EqualTo("outer"));
        Assert.That(AlwaysRunPatches.observedException, Is.Null);
    }

    [Test]
    public void ConstructorPostfix_ThrownException_SuppressByReference()
    {
        AlwaysRunPatches.observedException = null;
        ApplyPatch(typeof(AlwaysRunPatches), nameof(AlwaysRunPatches.ConstructorPostfix_ThrownException_SuppressByReference));
        ConstructorTargets? result = null;

        Assert.DoesNotThrow(() => result = new ConstructorTargets(true));
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ConstructorExecuted, Is.False);
        Assert.That(AlwaysRunPatches.observedException, Is.TypeOf<InvalidOperationException>()
            .With.Message.EqualTo("constructor"));
    }

    [Test]
    public void InnerConstructorPostfix_ThrownException_SuppressAndWriteResult()
    {
        AlwaysRunPatches.observedException = null;
        ApplyPatch(
            typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.InnerConstructorPostfix_ThrownException_SuppressAndWriteResult));

        ConstructorTargets result = ConstructorTargets.Create(true);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ConstructorExecuted, Is.True);
        Assert.That(AlwaysRunPatches.observedException, Is.TypeOf<InvalidOperationException>()
            .With.Message.EqualTo("constructor"));
    }

    [Test]
    public void IteratorPostfix_EnumerationException_IsNotCaptured()
    {
        AlwaysRunPatches.observedException = new Exception("sentinel");
        ApplyPatch(typeof(AlwaysRunPatches), nameof(AlwaysRunPatches.IteratorPostfix_EnumerationException_IsNotCaptured));

        IEnumerable<int> enumerable = StaticMethodTargets.EnumerateThenThrow();

        Assert.That(AlwaysRunPatches.observedException, Is.Null);
        using IEnumerator<int> enumerator = enumerable.GetEnumerator();
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(1));
        var exception = Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        Assert.That(exception!.Message, Is.EqualTo("iterator"));
    }

    [Test]
    public void IteratorInnerPostfix_ThrownException_SuppressAndWriteResult()
    {
        AlwaysRunPatches.observedException = null;
        ApplyPatch(
            typeof(AlwaysRunPatches),
            nameof(AlwaysRunPatches.IteratorInnerPostfix_ThrownException_SuppressAndWriteResult));

        int result = OuterStaticMethodTargets.EnumerateThrowingInnerResult().Single();

        Assert.That(result, Is.EqualTo(42));
        Assert.That(AlwaysRunPatches.observedException, Is.TypeOf<InvalidOperationException>()
            .With.Message.EqualTo("inner-result"));
    }

    [Test]
    public void ExceptionBinding_InlinePostfix_Suppresses()
    {
        AlwaysRunInlinePatches.ObservedException = null;
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.ThrowInvalidOperationException))!;
        MethodInfo patch = typeof(AlwaysRunInlinePatches)
            .GetMethod(nameof(AlwaysRunInlinePatches.ExceptionBinding_InlinePostfix_Suppresses))!;
        Patcher.Patch(Patch.Postfix.With(patch)
            .Options(PatchOptions.AlwaysRun | PatchOptions.Inline)
            .Of(target));

        Assert.DoesNotThrow(StaticMethodTargets.ThrowInvalidOperationException);
        Assert.That(AlwaysRunInlinePatches.ObservedException, Is.TypeOf<InvalidOperationException>()
            .With.Message.EqualTo("target"));
    }
}
