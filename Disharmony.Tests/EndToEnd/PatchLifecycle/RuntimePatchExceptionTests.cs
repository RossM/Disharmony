// ReSharper disable UnusedTypeParameter
// ReSharper disable UnusedParameter.Global
namespace Disharmony.Tests.EndToEnd.PatchLifecycle;

public class RuntimePatchExceptionPatches
{
    public static void Infix_Prefix_AllowMissingInnerTarget_AbsentOperation_PreservesOriginalBehavior() => patchCalls++;
    public static void Infix_Prefix_AllowMissingInnerTarget_PresentOperation_ExecutesPatch() => patchCalls++;
    public static void Infix_Postfix_AllowMissingInnerTarget_AbsentOperation_PreservesOriginalBehavior() => patchCalls++;
    public static void Infix_Postfix_AllowMissingInnerTarget_PresentOperation_ExecutesPatch() => patchCalls++;
    public static int patchCalls;

    public static void RuleBuilder_IncompatibleParameterConversion_IsRejectedBeforeUpdateMethod(string value) { }

    public static void RuleBuilder_OutOfRangeParameterIndex_IsRejectedBeforeUpdateMethod(
        [Argument(10)] int value) { }

    public static void Circumfix_ResultBindingForVoidTarget_IsRejectedBeforeUpdateMethod(int __result) { }

    public static void RuleBuilder_BaseMethodBindingForStaticTarget_IsRejectedBeforeUpdateMethod(Action __base) { }

    public static void RuleBuilder_BaseMethodBindingWithoutBaseImplementation_IsRejectedBeforeUpdateMethod(Action __base) { }

    public static void PatchWorker_InvalidPatchType_IsRejectedBeforeUpdateMethod() { }

    public void Circumfix_InstancePatchMethod_IsRejectedBeforeUpdateMethod() { }

    public static void Circumfix_GenericPatchMethod_IsRejectedBeforeUpdateMethod<T>() { }

    public static void Circumfix_ConstructedGenericPatchMethod_IsAllowed<T>() => patchCalls++;

    public static BindingStruct Circumfix_PrefixReturningStruct_IsRejectedBeforeUpdateMethod() => default;

    public static BindingStruct Infix_InnerPrefixReturningStruct_IsRejectedBeforeUpdateMethod() => default;

    public static void ForceApply_ApplicationFailure_ReportsAndPreservesOriginalBehavior() { }

    public static void ApplyThenForceApply_ApplicationFailure_ReportsAndPreservesOriginalBehavior() { }

    public static void TrampolineResolution_ApplicationFailure_ReportsAndRestoresOriginalMethod() { }

    public static void StateBuilder_SameKeyWithDifferentTypes_IsRejectedDuringPatch(
        [State("shared")] out int primitive,
        [State("shared")] out string reference)
    {
        primitive = 42;
        reference = "state";
    }
}

public static class RuntimePatchExceptionGenericPatches<T>
{
    public static void Circumfix_OpenGenericDeclaringType_IsRejectedBeforeUpdateMethod() { }

    public static void Circumfix_ClosedGenericDeclaringType_IsAllowed() { }
}

[TestFixture]
public sealed class RuntimePatchExceptionTests : PatchTestBase
{
    [Test]
    public void Infix_Prefix_AllowMissingInnerTarget_AbsentOperation_PreservesOriginalBehavior()
    {
        RuntimePatchExceptionPatches.patchCalls = 0;
        MethodInfo patch = typeof(RuntimePatchExceptionPatches).GetMethod(
            nameof(RuntimePatchExceptionPatches.Infix_Prefix_AllowMissingInnerTarget_AbsentOperation_PreservesOriginalBehavior))!;
        MethodInfo inner = typeof(InnerStaticMethodTargets).GetMethod(nameof(InnerStaticMethodTargets.IntResult))!;
        MethodInfo outer = typeof(StaticMethodTargets).GetMethod(
            nameof(StaticMethodTargets.IntResult))!;

        Patcher.Patch(Patch.Prefix.Inner(inner).With(patch).Of(outer).Options(PatchOptions.AllowMissingInnerTarget));
        Patcher.ForceApply();

        Assert.That(StaticMethodTargets.IntResult(), Is.EqualTo(1));
        Assert.That(RuntimePatchExceptionPatches.patchCalls, Is.EqualTo(0));
    }

    [Test]
    public void Infix_Prefix_AllowMissingInnerTarget_PresentOperation_ExecutesPatch()
    {
        RuntimePatchExceptionPatches.patchCalls = 0;
        MethodInfo patch = typeof(RuntimePatchExceptionPatches).GetMethod(
            nameof(RuntimePatchExceptionPatches.Infix_Prefix_AllowMissingInnerTarget_PresentOperation_ExecutesPatch))!;
        MethodInfo inner = typeof(InnerStaticMethodTargets).GetMethod(nameof(InnerStaticMethodTargets.IntResult))!;
        MethodInfo outer = typeof(OuterStaticMethodTargets).GetMethod(
            nameof(OuterStaticMethodTargets.IntResult))!;

        Patcher.Patch(Patch.Prefix.Inner(inner).With(patch).Of(outer).Options(PatchOptions.AllowMissingInnerTarget));
        Patcher.ForceApply();

        Assert.That(OuterStaticMethodTargets.IntResult(), Is.EqualTo(1));
        Assert.That(RuntimePatchExceptionPatches.patchCalls, Is.EqualTo(1));
    }

    [Test]
    public void Infix_Postfix_AllowMissingInnerTarget_AbsentOperation_PreservesOriginalBehavior()
    {
        RuntimePatchExceptionPatches.patchCalls = 0;
        MethodInfo patch = typeof(RuntimePatchExceptionPatches).GetMethod(
            nameof(RuntimePatchExceptionPatches.Infix_Postfix_AllowMissingInnerTarget_AbsentOperation_PreservesOriginalBehavior))!;
        MethodInfo inner = typeof(InnerStaticMethodTargets).GetMethod(nameof(InnerStaticMethodTargets.IntResult))!;
        MethodInfo outer = typeof(StaticMethodTargets).GetMethod(
            nameof(StaticMethodTargets.IntResult))!;

        Patcher.Patch(Patch.Postfix.Inner(inner).With(patch).Of(outer).Options(PatchOptions.AllowMissingInnerTarget));
        Patcher.ForceApply();

        Assert.That(StaticMethodTargets.IntResult(), Is.EqualTo(1));
        Assert.That(RuntimePatchExceptionPatches.patchCalls, Is.EqualTo(0));
    }

    [Test]
    public void Infix_Postfix_AllowMissingInnerTarget_PresentOperation_ExecutesPatch()
    {
        RuntimePatchExceptionPatches.patchCalls = 0;
        MethodInfo patch = typeof(RuntimePatchExceptionPatches).GetMethod(
            nameof(RuntimePatchExceptionPatches.Infix_Postfix_AllowMissingInnerTarget_PresentOperation_ExecutesPatch))!;
        MethodInfo inner = typeof(InnerStaticMethodTargets).GetMethod(nameof(InnerStaticMethodTargets.IntResult))!;
        MethodInfo outer = typeof(OuterStaticMethodTargets).GetMethod(
            nameof(OuterStaticMethodTargets.IntResult))!;

        Patcher.Patch(Patch.Postfix.Inner(inner).With(patch).Of(outer).Options(PatchOptions.AllowMissingInnerTarget));
        Patcher.ForceApply();

        Assert.That(OuterStaticMethodTargets.IntResult(), Is.EqualTo(1));
        Assert.That(RuntimePatchExceptionPatches.patchCalls, Is.EqualTo(1));
    }

    [Test]
    public void RuleBuilder_IncompatibleParameterConversion_IsRejectedBeforeUpdateMethod()
    {
        MethodInfo patch = typeof(RuntimePatchExceptionPatches)
            .GetMethod(nameof(RuntimePatchExceptionPatches.RuleBuilder_IncompatibleParameterConversion_IsRejectedBeforeUpdateMethod))!;
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.IntArgument))!;

        Assert.Throws<PatchException>(() =>
        {
            Patcher.Patch(Patch.Prefix.With(patch).Of(target));
            Patcher.ForceApply();
        });
    }

    [Test]
    public void RuleBuilder_OutOfRangeParameterIndex_IsRejectedBeforeUpdateMethod()
    {
        MethodInfo patch = typeof(RuntimePatchExceptionPatches)
            .GetMethod(nameof(RuntimePatchExceptionPatches.RuleBuilder_OutOfRangeParameterIndex_IsRejectedBeforeUpdateMethod))!;
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.IntArgument))!;

        Assert.Throws<PatchException>(() =>
        {
            Patcher.Patch(Patch.Prefix.With(patch).Of(target));
            Patcher.ForceApply();
        });
    }

    [Test]
    public void Circumfix_ResultBindingForVoidTarget_IsRejectedBeforeUpdateMethod()
    {
        MethodInfo patch = typeof(RuntimePatchExceptionPatches)
            .GetMethod(nameof(RuntimePatchExceptionPatches.Circumfix_ResultBindingForVoidTarget_IsRejectedBeforeUpdateMethod))!;
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.Void))!;

        Assert.Throws<PatchException>(() =>
        {
            Patcher.Patch(Patch.Postfix.With(patch).Of(target));
            Patcher.ForceApply();
        });
    }

    [Test]
    public void RuleBuilder_BaseMethodBindingForStaticTarget_IsRejectedBeforeUpdateMethod()
    {
        MethodInfo patch = typeof(RuntimePatchExceptionPatches)
            .GetMethod(nameof(RuntimePatchExceptionPatches.RuleBuilder_BaseMethodBindingForStaticTarget_IsRejectedBeforeUpdateMethod))!;
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.Void))!;

        Assert.Throws<PatchException>(() =>
        {
            Patcher.Patch(Patch.Prefix.With(patch).Of(target));
            Patcher.ForceApply();
        });
    }

    [Test]
    public void RuleBuilder_BaseMethodBindingWithoutBaseImplementation_IsRejectedBeforeUpdateMethod()
    {
        MethodInfo patch = typeof(RuntimePatchExceptionPatches)
            .GetMethod(nameof(RuntimePatchExceptionPatches
                .RuleBuilder_BaseMethodBindingWithoutBaseImplementation_IsRejectedBeforeUpdateMethod))!;
        MethodInfo target = typeof(InstanceMethodTargetsWithoutFields)
            .GetMethod(nameof(InstanceMethodTargetsWithoutFields.Void))!;

        Assert.Throws<PatchException>(() =>
        {
            Patcher.Patch(Patch.Prefix.With(patch).Of(target));
            Patcher.ForceApply();
        });
    }

    [Test]
    public void PatchWorker_InvalidPatchType_IsRejectedBeforeUpdateMethod()
    {
        MethodInfo patch = typeof(RuntimePatchExceptionPatches)
            .GetMethod(nameof(RuntimePatchExceptionPatches.PatchWorker_InvalidPatchType_IsRejectedBeforeUpdateMethod))!;
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.Void))!;

        Assert.Throws<PatchException>(() =>
        {
            Patcher.Patch(Patch.With(patch).Of(target) with { Kind = (PatchKind)int.MaxValue });
            Patcher.ForceApply();
        });
    }

    [Test]
    public void Circumfix_InstancePatchMethod_IsRejectedBeforeUpdateMethod()
    {
        MethodInfo patch = typeof(RuntimePatchExceptionPatches)
            .GetMethod(nameof(RuntimePatchExceptionPatches.Circumfix_InstancePatchMethod_IsRejectedBeforeUpdateMethod))!;
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.Void))!;

        Assert.Throws<PatchException>(() =>
        {
            Patcher.Patch(Patch.Prefix.With(patch).Of(target));
            Patcher.ForceApply();
        });
    }

    [Test]
    public void Circumfix_GenericPatchMethod_IsRejectedBeforeUpdateMethod()
    {
        MethodInfo patch = typeof(RuntimePatchExceptionPatches)
            .GetMethod(nameof(RuntimePatchExceptionPatches.Circumfix_GenericPatchMethod_IsRejectedBeforeUpdateMethod))!;
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.Void))!;

        Assert.Throws<PatchException>(() =>
        {
            Patcher.Patch(Patch.Prefix.With(patch).Of(target));
            Patcher.ForceApply();
        });
    }

    [Test]
    public void Circumfix_ConstructedGenericPatchMethod_IsAllowed()
    {
        RuntimePatchExceptionPatches.patchCalls = 0;
        MethodInfo patch = typeof(RuntimePatchExceptionPatches)
            .GetMethod(nameof(RuntimePatchExceptionPatches.Circumfix_ConstructedGenericPatchMethod_IsAllowed))!
            .MakeGenericMethod(typeof(int));
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.Void))!;

        Patcher.Patch(Patch.Prefix.With(patch).Of(target));
        Patcher.ForceApply();

        StaticMethodTargets.Void();

        Assert.That(RuntimePatchExceptionPatches.patchCalls, Is.EqualTo(1));
    }

    [Test]
    public void Circumfix_OpenGenericDeclaringType_IsRejectedBeforeUpdateMethod()
    {
        MethodInfo patch = typeof(RuntimePatchExceptionGenericPatches<>)
            .GetMethod(nameof(RuntimePatchExceptionGenericPatches<>.Circumfix_OpenGenericDeclaringType_IsRejectedBeforeUpdateMethod))!;
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.Void))!;

        Assert.Throws<PatchException>(() =>
        {
            Patcher.Patch(Patch.Prefix.With(patch).Of(target));
            Patcher.ForceApply();
        });
    }

    [Test]
    public void Circumfix_ClosedGenericDeclaringType_IsAllowed()
    {
        MethodInfo patch = typeof(RuntimePatchExceptionGenericPatches<int>)
            .GetMethod(nameof(RuntimePatchExceptionGenericPatches<>.Circumfix_ClosedGenericDeclaringType_IsAllowed))!;
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.Void))!;

        Assert.DoesNotThrow(() =>
        {
            Patcher.Patch(Patch.Prefix.With(patch).Of(target));
            Patcher.ForceApply();
        });
    }

    [Test]
    public void Circumfix_PrefixReturningStruct_IsRejectedBeforeUpdateMethod()
    {
        MethodInfo patch = typeof(RuntimePatchExceptionPatches)
            .GetMethod(nameof(RuntimePatchExceptionPatches.Circumfix_PrefixReturningStruct_IsRejectedBeforeUpdateMethod))!;
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.Void))!;

        Assert.Throws<PatchException>(() =>
        {
            Patcher.Patch(Patch.Prefix.With(patch).Of(target));
            Patcher.ForceApply();
        });
    }

    [Test]
    public void Infix_InnerPrefixReturningStruct_IsRejectedBeforeUpdateMethod()
    {
        MethodInfo patch = typeof(RuntimePatchExceptionPatches)
            .GetMethod(nameof(RuntimePatchExceptionPatches.Infix_InnerPrefixReturningStruct_IsRejectedBeforeUpdateMethod))!;
        MethodInfo innerTarget = typeof(InnerStaticMethodTargets)
            .GetMethod(nameof(InnerStaticMethodTargets.IntResult))!;
        MethodInfo outerTarget = typeof(OuterStaticMethodTargets)
            .GetMethod(nameof(OuterStaticMethodTargets.IntResult))!;

        Assert.Throws<PatchException>(() =>
        {
            Patcher.Patch(Patch.Prefix.Inner(innerTarget).With(patch).Of(outerTarget));
            Patcher.ForceApply();
        });
    }

    [Test]
    public void ForceApply_ApplicationFailure_ReportsAndPreservesOriginalBehavior()
    {
        MethodInfo patch = typeof(RuntimePatchExceptionPatches)
            .GetMethod(nameof(RuntimePatchExceptionPatches.ForceApply_ApplicationFailure_ReportsAndPreservesOriginalBehavior))!;
        MethodInfo innerTarget = typeof(InnerStaticMethodTargets)
            .GetMethod(nameof(InnerStaticMethodTargets.IntResult))!;
        MethodInfo outerTarget = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.IntResult))!;

        List<Exception> reportedExceptions = [];
        Action<Exception> handler = reportedExceptions.Add;
        Patcher.RuntimeExceptionHandler -= ThrowRuntimeException;
        Patcher.RuntimeExceptionHandler += handler;

        try
        {
            Patcher.Patch(Patch.With(patch).Prefix.Inner(innerTarget).Of(outerTarget));

            Assert.That(() => Patcher.ForceApply(), Throws.Nothing);
            Assert.That(reportedExceptions, Has.Count.EqualTo(1));
            Assert.That(reportedExceptions[0], Is.TypeOf<InvalidOperationException>());
            Assert.That(StaticMethodTargets.IntResult(), Is.EqualTo(1));
        }
        finally
        {
            Patcher.RuntimeExceptionHandler -= handler;
            Patcher.RuntimeExceptionHandler += ThrowRuntimeException;
        }
    }

    [Test]
    public void ApplyThenForceApply_ApplicationFailure_ReportsAndPreservesOriginalBehavior()
    {
        MethodInfo patch = typeof(RuntimePatchExceptionPatches)
            .GetMethod(nameof(RuntimePatchExceptionPatches.ApplyThenForceApply_ApplicationFailure_ReportsAndPreservesOriginalBehavior))!;
        MethodInfo innerTarget = typeof(InnerStaticMethodTargets)
            .GetMethod(nameof(InnerStaticMethodTargets.IntResult))!;
        MethodInfo outerTarget = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.IntResult))!;

        List<Exception> reportedExceptions = [];
        Action<Exception> handler = reportedExceptions.Add;
        Patcher.RuntimeExceptionHandler -= ThrowRuntimeException;
        Patcher.RuntimeExceptionHandler += handler;

        try
        {
            Patcher.Patch(Patch.Prefix.Inner(innerTarget).With(patch).Of(outerTarget));

            Assert.That(reportedExceptions, Is.Empty);
            Assert.That(() => Patcher.ForceApply(), Throws.Nothing);
            Assert.That(reportedExceptions, Has.Count.EqualTo(1));
            Assert.That(reportedExceptions[0], Is.TypeOf<InvalidOperationException>());
            Assert.That(StaticMethodTargets.IntResult(), Is.EqualTo(1));
        }
        finally
        {
            Patcher.RuntimeExceptionHandler -= handler;
            Patcher.RuntimeExceptionHandler += ThrowRuntimeException;
        }
    }

    [Test]
    public void TrampolineResolution_ApplicationFailure_ReportsAndRestoresOriginalMethod()
    {
        MethodInfo patch = typeof(RuntimePatchExceptionPatches)
            .GetMethod(nameof(RuntimePatchExceptionPatches.TrampolineResolution_ApplicationFailure_ReportsAndRestoresOriginalMethod))!;
        MethodInfo innerTarget = typeof(InnerStaticMethodTargets)
            .GetMethod(nameof(InnerStaticMethodTargets.IntResult))!;
        MethodInfo outerTarget = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.IntResult))!;
        List<Exception> reportedExceptions = [];
        Action<Exception> handler = reportedExceptions.Add;
        Patcher.RuntimeExceptionHandler -= ThrowRuntimeException;
        Patcher.RuntimeExceptionHandler += handler;

        try
        {
            Patcher.Patch(Patch.Prefix.Inner(innerTarget).With(patch).Of(outerTarget));

            Assert.That(StaticMethodTargets.IntResult(), Is.EqualTo(1));
            Assert.That(reportedExceptions, Has.Count.EqualTo(1));

            Assert.That(StaticMethodTargets.IntResult(), Is.EqualTo(1));
            Assert.That(reportedExceptions, Has.Count.EqualTo(1));
        }
        finally
        {
            Patcher.RuntimeExceptionHandler -= handler;
            Patcher.RuntimeExceptionHandler += ThrowRuntimeException;
        }
    }

    [Test]
    public void StateBuilder_SameKeyWithDifferentTypes_IsRejectedDuringPatch()
    {
        MethodInfo patch = typeof(RuntimePatchExceptionPatches)
            .GetMethod(nameof(RuntimePatchExceptionPatches.StateBuilder_SameKeyWithDifferentTypes_IsRejectedDuringPatch))!;
        MethodInfo target = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.Void))!;

        PatchException? exception = Assert.Throws<PatchException>(() =>
            Patcher.Patch(Patch.Prefix.With(patch).Of(target)));
        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("reference: Incompatible state types: System.String and System.Int32"));
    }
}
