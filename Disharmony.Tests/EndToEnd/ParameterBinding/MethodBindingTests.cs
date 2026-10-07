namespace Disharmony.Tests.EndToEnd.ParameterBinding;

public static class MethodBindingPatches
{
    [Prefix]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_PropertyGetter_Named([Method(nameof(MethodBindingInstanceTargets.InstanceValue), memberType: MemberType.Getter)] Func<int> accessor) =>
        ResultObserved = accessor();

    [Prefix]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_PropertySetter_Named([Method(nameof(MethodBindingInstanceTargets.InstanceValue), memberType: MemberType.Setter)] Action<int> accessor) =>
        accessor(42);

    [Prefix]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_PropertyGetter_ExplicitTypeNullName([Method(typeof(MethodBindingInstanceTargets), null, memberType: MemberType.Getter)] Func<int> InstanceValue) =>
        ResultObserved = InstanceValue();

    [Prefix]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_PropertySetter_ExplicitTypeNullName([Method(typeof(MethodBindingInstanceTargets), null, memberType: MemberType.Setter)] Action<int> InstanceValue) =>
        InstanceValue(42);

    [Prefix]
    [Inner(typeof(MethodBindingInnerTargets), nameof(MethodBindingInnerTargets.TargetInstanceMethod))]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.CallInnerInstanceMethod))]
    public static void InnerPrefix_MethodAttribute_PropertyGetter_ImplicitName([Method(Scope.Inner, memberType: MemberType.Getter)] Func<int> InstanceValue) =>
        ResultObserved = InstanceValue();

    [Prefix]
    [Inner(typeof(MethodBindingInnerTargets), nameof(MethodBindingInnerTargets.TargetInstanceMethod))]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.CallInnerInstanceMethod))]
    public static void InnerPrefix_MethodAttribute_PropertySetter_ImplicitName([Method(Scope.Inner, memberType: MemberType.Setter)] Action<int> InstanceValue) =>
        InstanceValue(42);

    [Prefix]
    [Target(typeof(MethodBindingVirtualBaseTargets), nameof(MethodBindingVirtualBaseTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_ExplicitType_Named_NonVirtualCall(
        [Method(typeof(MethodBindingVirtualBaseTargets), nameof(MethodBindingVirtualBaseTargets.Describe), virtualCall: false)] Func<string, string> method) =>
        DescriptionObserved = method("patch");

    [Prefix]
    [Target(typeof(MethodBindingVirtualBaseTargets), nameof(MethodBindingVirtualBaseTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_ExplicitType_NullName_NonVirtualCall(
        [Method((string?)null, virtualCall: false)] Func<string, string> Describe) =>
        DescriptionObserved = Describe("patch");

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Prefix_MethodAttribute_ExplicitType_Named_UnrelatedStaticMethod(
        [Method(typeof(MethodBindingStaticTargets), nameof(MethodBindingStaticTargets.BoundStaticMethod))] Func<int, int> method) =>
        ResultObserved = method(5);

    [Prefix]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.Void))]
    public static void Prefix_MethodAttribute_ExplicitType_NullName_UnrelatedStaticMethod(
        [Method(typeof(MethodBindingStaticTargets), null)] Func<int, int> BoundStaticMethod) =>
        ResultObserved = BoundStaticMethod(5);

    public static int ResultObserved;
    public static int ArgumentObserved;
    public static string? DescriptionObserved;
    public static string? VirtualDescriptionObserved;

    [Prefix]
    [Target(typeof(MethodBindingVirtualBaseTargets), nameof(MethodBindingVirtualBaseTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_MixedDispatch_DelegatesRemainIndependent(
        [Method(nameof(MethodBindingVirtualBaseTargets.Describe), virtualCall: false)] Func<string, string> directMethod,
        [Method(nameof(MethodBindingVirtualBaseTargets.Describe), virtualCall: true)] Func<string, string> virtualMethod)
    {
        DescriptionObserved = directMethod("patch");
        VirtualDescriptionObserved = virtualMethod("patch");
    }

    [Prefix]
    [Target(typeof(MethodBindingVirtualBaseTargets), nameof(MethodBindingVirtualBaseTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_NonVirtualCall_UsesDeclaredImplementation(
        [Method(nameof(MethodBindingVirtualBaseTargets.Describe), virtualCall: false)] Func<string, string> method) =>
        DescriptionObserved = method("patch");

    [Prefix]
    [Target(typeof(MethodBindingVirtualBaseTargets), nameof(MethodBindingVirtualBaseTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_NonVirtualCall_NullNameUsesParameterName(
        [Method((string?)null, virtualCall: false)] Func<string, string> Describe) =>
        DescriptionObserved = Describe("patch");

    [Prefix]
    [Target(typeof(MethodBindingVirtualDerivedTargets), nameof(MethodBindingVirtualDerivedTargets.TargetDerivedInstanceMethod))]
    public static void Prefix_MethodAttribute_NonVirtualCall_UsesDeclaredOverride(
        [Method(nameof(MethodBindingVirtualDerivedTargets.Describe), virtualCall: false)] Func<string, string> method) =>
        DescriptionObserved = method("patch");

    [Prefix]
    [Inner(typeof(MethodBindingVirtualBaseTargets), nameof(MethodBindingVirtualBaseTargets.Describe))]
    [Target(typeof(MethodBindingVirtualBaseTargets), nameof(MethodBindingVirtualBaseTargets.CallInnerVirtualMethod))]
    public static void InnerPrefix_MethodAttribute_NonVirtualCall_InnerScopeUsesInnerInstance(
        [Method(nameof(MethodBindingVirtualBaseTargets.Describe), Scope.Inner, virtualCall: false)] Func<string, string> method) =>
        DescriptionObserved = method("patch");

    [Prefix]
    [Inner(typeof(MethodBindingVirtualBaseTargets), nameof(MethodBindingVirtualBaseTargets.Describe))]
    [Target(typeof(MethodBindingVirtualBaseTargets), nameof(MethodBindingVirtualBaseTargets.CallInnerVirtualMethod))]
    public static void InnerPrefix_MethodAttribute_VirtualCall_InnerScopeUsesRuntimeOverride(
        [Method(nameof(MethodBindingVirtualBaseTargets.Describe), Scope.Inner, virtualCall: true)] Func<string, string> method) =>
        DescriptionObserved = method("patch");

    [Prefix]
    [Inner(typeof(MethodBindingVirtualBaseTargets), nameof(MethodBindingVirtualBaseTargets.Describe))]
    [Target(typeof(MethodBindingVirtualBaseTargets), nameof(MethodBindingVirtualBaseTargets.CallInnerVirtualMethod))]
    public static void InnerPrefix_MethodAttribute_NonVirtualCall_OuterScopeUsesOuterInstance(
        [Method(nameof(MethodBindingVirtualBaseTargets.Describe), Scope.Outer, virtualCall: false)] Func<string, string> method) =>
        DescriptionObserved = method("patch");

    [Prefix]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_InstanceMethodOnOuterInstance_Invokes(
        [Method(nameof(MethodBindingInstanceTargets.BoundInstanceMethod))] Func<int, int> method) =>
        ResultObserved = method(5);

    [Prefix]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_NullName_UsesParameterName(
        [Method((string?)null)] Func<int, int> BoundInstanceMethod) =>
        ResultObserved = BoundInstanceMethod(5);

    [Prefix]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_VoidInstanceMethod_Invokes(
        [Method(nameof(MethodBindingInstanceTargets.BoundVoidMethod))] Action method) => method();

    [Prefix]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_RefParameter_Invokes(
        [Method(nameof(MethodBindingInstanceTargets.BoundRefMethod))] RefIntMethod method)
    {
        int argument = 5;
        ResultObserved = method(ref argument);
        ArgumentObserved = argument;
    }

    [Prefix]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_Delegate_ParameterTypeMismatch_RejectedByPatch(
        [Method(nameof(MethodBindingInstanceTargets.BoundInstanceMethod))] Func<string, int> method) { }

    [Prefix]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_Delegate_ParameterCountMismatch_RejectedByPatch(
        [Method(nameof(MethodBindingInstanceTargets.BoundInstanceMethod))] Func<int, int, int> method) { }

    [Prefix]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_Delegate_ReturnTypeMismatch_RejectedByPatch(
        [Method(nameof(MethodBindingInstanceTargets.BoundInstanceMethod))] Func<int, string> method) { }

    [Prefix]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_DelegateWithoutInvoke_RejectedByPatch(
        [Method(nameof(MethodBindingInstanceTargets.BoundInstanceMethod))] Delegate method) { }

    [Prefix]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_PrivateInstanceMethod_Invokes(
        [Method("BoundPrivateInstanceMethod")] Func<int, int> method) =>
        ResultObserved = method(5);

    [Prefix]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_PrivateStaticMethod_Invokes(
        [Method("BoundPrivateStaticMethod")] Func<int, int> method) =>
        ResultObserved = method(5);

    [Prefix]
    [Target(typeof(MethodBindingVirtualBaseTargets), nameof(MethodBindingVirtualBaseTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_VirtualMethod_DispatchesOnRuntimeInstance(
        [Method(nameof(MethodBindingVirtualBaseTargets.BoundVirtualMethod))] Func<int, int> method) =>
        ResultObserved = method(5);

    [Prefix]
    [Target(typeof(MethodBindingStructTargets), nameof(MethodBindingStructTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_StructInstanceMethod_IsRejected(
        [Method(nameof(MethodBindingStructTargets.BoundInstanceMethod))] Func<int, int> method) =>
        ResultObserved = method(5);

    [Prefix]
    [Target(typeof(MethodBindingStructTargets), nameof(MethodBindingStructTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_StaticMethodOnStruct_Invokes(
        [Method(nameof(MethodBindingStructTargets.BoundStaticMethod))] Func<int, int> method) =>
        ResultObserved = method(5);

    [Prefix]
    [Target(typeof(MethodBindingStructTargets), nameof(MethodBindingStructTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_ReadonlyInstanceMethodOnMutableStruct_Invokes(
        [Method(nameof(MethodBindingStructTargets.BoundReadonlyInstanceMethod))] Func<int, int> method) =>
        ResultObserved = method(5);

    [Prefix]
    [Target(typeof(MethodBindingReadonlyStructTargets), nameof(MethodBindingReadonlyStructTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_InstanceMethodOnReadonlyStruct_Invokes(
        [Method(nameof(MethodBindingReadonlyStructTargets.BoundInstanceMethod))] Func<int, int> method) =>
        ResultObserved = method(5);

    [Prefix]
    [Inner(typeof(MethodBindingReadonlyStructTargets), nameof(MethodBindingReadonlyStructTargets.TargetInstanceMethod))]
    [Target(typeof(MethodBindingStaticTargets), nameof(MethodBindingStaticTargets.CallReadonlyStructInstanceMethod))]
    public static void InnerPrefix_MethodAttribute_InstanceMethodOnReadonlyInnerStruct_Invokes(
        [Method(nameof(MethodBindingReadonlyStructTargets.BoundInstanceMethod))] Func<int, int> method) =>
        ResultObserved = method(5);

    [Prefix]
    [Inner(typeof(MethodBindingStructTargets), nameof(MethodBindingStructTargets.TargetInstanceMethod))]
    [Target(typeof(MethodBindingReadonlyStructTargets),
        nameof(MethodBindingReadonlyStructTargets.CallMutableStructInstanceMethod))]
    public static void InnerPrefix_MethodAttribute_ReadonlyOuterDoesNotAllowMutableInnerStructMethod(
        [Method(nameof(MethodBindingStructTargets.BoundMutatingInstanceMethod))] Func<int, int> method) =>
        ResultObserved = method(5);

    [Prefix]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_OverloadedMethod_SelectsDelegateSignature(
        [Method(nameof(MethodBindingInstanceTargets.BoundOverloadedMethod))] Func<int, int> method) => ResultObserved = method(5);

    [Prefix]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_MissingMethod_IsRejected(
        [Method("MissingMethod")] Func<int, int> method) { }

    [Prefix]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.TargetStaticMethod))]
    public static void Prefix_MethodAttribute_InstanceMethodOnStaticTarget_IsRejected(
        [Method(nameof(MethodBindingInstanceTargets.BoundInstanceMethod))] Func<int, int> method) { }

    [Prefix]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.TargetInstanceMethod))]
    public static void Prefix_MethodAttribute_InnerScopeWithoutInnerPatch_IsRejected(
        [Method(nameof(MethodBindingInnerTargets.BoundInstanceMethod), Scope.Inner)] Func<int, int> method) { }

    [Prefix]
    [Inner(typeof(MethodBindingInnerTargets), nameof(MethodBindingInnerTargets.TargetInstanceMethod))]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.CallInnerInstanceMethod))]
    public static void InnerPrefix_MethodAttribute_StaticMethodOnInnerInstanceType_Invokes(
        [Method(nameof(MethodBindingInnerTargets.BoundStaticMethod))] Func<int, int> method) =>
        ResultObserved = method(5);

    [Prefix]
    [Inner(typeof(MethodBindingInnerTargets), nameof(MethodBindingInnerTargets.TargetInstanceMethod))]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.CallInnerInstanceMethod))]
    public static void InnerPrefix_MethodAttribute_InstanceMethodOnInnerInstance_BindsInnerInstance(
        [Method(nameof(MethodBindingInnerTargets.BoundInstanceMethod))] Func<int, int> method) =>
        ResultObserved = method(5);

    [Prefix]
    [Inner(typeof(MethodBindingInnerTargets), nameof(MethodBindingInnerTargets.TargetInstanceMethod))]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.CallInnerInstanceMethod))]
    public static void InnerPrefix_MethodAttribute_InnerScope_InstanceMethodOnInnerInstance_BindsInnerInstance(
        [Method(nameof(MethodBindingInnerTargets.BoundInstanceMethod), Scope.Inner)] Func<int, int> method) =>
        ResultObserved = method(5);

    [Prefix]
    [Target(typeof(MethodBindingStaticTargets), nameof(MethodBindingStaticTargets.TargetStaticMethod))]
    public static void Prefix_MethodAttribute_StaticMethodOnStaticType_Invokes(
        [Method(nameof(MethodBindingStaticTargets.BoundStaticMethod))] Func<int, int> method) =>
        ResultObserved = method(5);

    [Prefix]
    [Inner(typeof(MethodBindingInnerTargets), nameof(MethodBindingInnerTargets.TargetInstanceMethod))]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.CallInnerInstanceMethod))]
    public static void InnerPrefix_MethodAttribute_OuterScope_InstanceMethodOnOuterInstance_Invokes(
        [Method(nameof(MethodBindingInstanceTargets.BoundInstanceMethod), Scope.Outer)] Func<int, int> method) =>
        ResultObserved = method(5);

    [Prefix]
    [Inner(typeof(MethodBindingInnerTargets), nameof(MethodBindingInnerTargets.TargetInstanceMethod))]
    [Target(typeof(MethodBindingIteratorTargets), nameof(MethodBindingIteratorTargets.EnumerateInnerInstanceMethod))]
    public static void IteratorInnerPrefix_MethodAttribute_InnerScope_BindsInnerInstance(
        [Method(nameof(MethodBindingInnerTargets.BoundInstanceMethod), Scope.Inner)] Func<int, int> method) =>
        ResultObserved = method(5);

    [Prefix]
    [Inner(typeof(MethodBindingInnerTargets), nameof(MethodBindingInnerTargets.TargetInstanceMethod))]
    [Target(typeof(MethodBindingIteratorTargets), nameof(MethodBindingIteratorTargets.EnumerateInnerInstanceMethod))]
    public static void IteratorInnerPrefix_MethodAttribute_OuterScopeInstanceMethod_IsRejected(
        [Method(nameof(MethodBindingIteratorTargets.BoundInstanceMethod), Scope.Outer)] Func<int, int> method) =>
        ResultObserved = method(5);

    [Prefix]
    [Inner(typeof(MethodBindingInnerTargets), nameof(MethodBindingInnerTargets.TargetInstanceMethod))]
    [Target(typeof(MethodBindingIteratorTargets), nameof(MethodBindingIteratorTargets.EnumerateInnerInstanceMethod))]
    public static void IteratorInnerPrefix_MethodAttribute_OuterScopeStaticMethod_Invokes(
        [Method(nameof(MethodBindingIteratorTargets.BoundStaticMethod), Scope.Outer)] Func<int, int> method) =>
        ResultObserved = method(5);

    [Prefix]
    [Inner(typeof(MethodBindingInstanceTargets), "CallLocalFunction.LocalFunction")]
    [Target(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.CallLocalFunction))]
    public static void LocalFunctionInnerPrefix_MethodAttribute_OuterScope_BindsDeclaringInstance(
        [Method(nameof(MethodBindingInstanceTargets.BoundInstanceMethod), Scope.Outer)] Func<int, int> method) =>
        ResultObserved = method(5);
}

[TestFixture]
public sealed class MethodBindingTests : PatchTestBase
{
    [Test]
    public void Prefix_MethodAttribute_PropertyGetter_Named()
    {
        MethodBindingPatches.ResultObserved = 0;
        var target = new MethodBindingInstanceTargets { InstanceValue = 11 };
        ApplyPatch(typeof(MethodBindingPatches), nameof(MethodBindingPatches.Prefix_MethodAttribute_PropertyGetter_Named));

        int result = target.TargetInstanceMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(10));
            Assert.That(target.InstanceValue, Is.EqualTo(11));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(11));
        });
    }

    [Test]
    public void Prefix_MethodAttribute_PropertySetter_Named()
    {
        MethodBindingPatches.ResultObserved = 0;
        var target = new MethodBindingInstanceTargets { InstanceValue = 11 };
        ApplyPatch(typeof(MethodBindingPatches), nameof(MethodBindingPatches.Prefix_MethodAttribute_PropertySetter_Named));

        int result = target.TargetInstanceMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(10));
            Assert.That(target.InstanceValue, Is.EqualTo(42));
        });
    }

    [Test]
    public void Prefix_MethodAttribute_PropertyGetter_ExplicitTypeNullName()
    {
        MethodBindingPatches.ResultObserved = 0;
        var target = new MethodBindingInstanceTargets { InstanceValue = 11 };
        ApplyPatch(typeof(MethodBindingPatches), nameof(MethodBindingPatches.Prefix_MethodAttribute_PropertyGetter_ExplicitTypeNullName));

        int result = target.TargetInstanceMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(10));
            Assert.That(target.InstanceValue, Is.EqualTo(11));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(11));
        });
    }

    [Test]
    public void Prefix_MethodAttribute_PropertySetter_ExplicitTypeNullName()
    {
        MethodBindingPatches.ResultObserved = 0;
        var target = new MethodBindingInstanceTargets { InstanceValue = 11 };
        ApplyPatch(typeof(MethodBindingPatches), nameof(MethodBindingPatches.Prefix_MethodAttribute_PropertySetter_ExplicitTypeNullName));

        int result = target.TargetInstanceMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(10));
            Assert.That(target.InstanceValue, Is.EqualTo(42));
        });
    }

    [Test]
    public void InnerPrefix_MethodAttribute_PropertyGetter_ImplicitName()
    {
        MethodBindingPatches.ResultObserved = 0;
        var target = new MethodBindingInstanceTargets { InstanceValue = 11 };
        var inner = new MethodBindingInnerTargets { InstanceValue = 23 };
        ApplyPatch(typeof(MethodBindingPatches), nameof(MethodBindingPatches.InnerPrefix_MethodAttribute_PropertyGetter_ImplicitName));

        int result = target.CallInnerInstanceMethod(inner);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(20));
            Assert.That(target.InstanceValue, Is.EqualTo(11));
            Assert.That(inner.InstanceValue, Is.EqualTo(23));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(23));
        });
    }

    [Test]
    public void InnerPrefix_MethodAttribute_PropertySetter_ImplicitName()
    {
        MethodBindingPatches.ResultObserved = 0;
        var target = new MethodBindingInstanceTargets { InstanceValue = 11 };
        var inner = new MethodBindingInnerTargets { InstanceValue = 23 };
        ApplyPatch(typeof(MethodBindingPatches), nameof(MethodBindingPatches.InnerPrefix_MethodAttribute_PropertySetter_ImplicitName));

        int result = target.CallInnerInstanceMethod(inner);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(20));
            Assert.That(target.InstanceValue, Is.EqualTo(11));
            Assert.That(inner.InstanceValue, Is.EqualTo(42));
        });
    }

    [Test]
    public void Prefix_MethodAttribute_ExplicitType_Named_NonVirtualCall()
    {
        MethodBindingPatches.DescriptionObserved = null;
        ApplyPatch(typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_ExplicitType_Named_NonVirtualCall));
        MethodBindingVirtualBaseTargets target = new MethodBindingVirtualDerivedTargets { InstanceName = "outer" };

        int result = target.TargetInstanceMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(50));
            Assert.That(MethodBindingPatches.DescriptionObserved, Is.EqualTo("base:outer:patch"));
        });
    }

    [Test]
    public void Prefix_MethodAttribute_ExplicitType_NullName_NonVirtualCall()
    {
        MethodBindingPatches.DescriptionObserved = null;
        ApplyPatch(typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_ExplicitType_NullName_NonVirtualCall));
        MethodBindingVirtualBaseTargets target = new MethodBindingVirtualDerivedTargets { InstanceName = "outer" };

        int result = target.TargetInstanceMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(50));
            Assert.That(MethodBindingPatches.DescriptionObserved, Is.EqualTo("base:outer:patch"));
        });
    }

    [Test]
    public void Prefix_MethodAttribute_ExplicitType_Named_UnrelatedStaticMethod()
    {
        MethodBindingPatches.ResultObserved = 0;
        ApplyPatch(typeof(MethodBindingPatches), nameof(MethodBindingPatches.Prefix_MethodAttribute_ExplicitType_Named_UnrelatedStaticMethod));

        StaticMethodTargets.Void();

        Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(305));
    }

    [Test]
    public void Prefix_MethodAttribute_ExplicitType_NullName_UnrelatedStaticMethod()
    {
        MethodBindingPatches.ResultObserved = 0;
        ApplyPatch(typeof(MethodBindingPatches), nameof(MethodBindingPatches.Prefix_MethodAttribute_ExplicitType_NullName_UnrelatedStaticMethod));

        StaticMethodTargets.Void();

        Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(305));
    }

    [Test]
    public void Prefix_MethodAttribute_InstanceMethodOnOuterInstance_Invokes()
    {
        MethodBindingPatches.ResultObserved = 0;
        ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_InstanceMethodOnOuterInstance_Invokes));
        MethodBindingInstanceTargets target = new() { InstanceValue = 7 };

        int result = target.TargetInstanceMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(10));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(12));
        });
    }

    [Test]
    public void Prefix_MethodAttribute_NullName_UsesParameterName()
    {
        MethodBindingPatches.ResultObserved = 0;
        ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_NullName_UsesParameterName));
        MethodBindingInstanceTargets target = new() { InstanceValue = 7 };

        int result = target.TargetInstanceMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(10));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(12));
        });
    }

    [Test]
    public void Prefix_MethodAttribute_VoidInstanceMethod_Invokes()
    {
        ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_VoidInstanceMethod_Invokes));
        MethodBindingInstanceTargets target = new() { InstanceValue = 7 };

        int result = target.TargetInstanceMethod();

        Assert.That(result, Is.EqualTo(10));
        Assert.That(target.InstanceValue, Is.EqualTo(8));
    }

    [Test]
    public void Prefix_MethodAttribute_RefParameter_Invokes()
    {
        MethodBindingPatches.ResultObserved = 0;
        MethodBindingPatches.ArgumentObserved = 0;
        ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_RefParameter_Invokes));
        MethodBindingInstanceTargets target = new() { InstanceValue = 7 };

        int result = target.TargetInstanceMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(10));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(12));
            Assert.That(MethodBindingPatches.ArgumentObserved, Is.EqualTo(12));
        });
    }

    [Test]
    public void Prefix_MethodAttribute_Delegate_ParameterTypeMismatch_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_Delegate_ParameterTypeMismatch_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("method: Method not found"));
    }

    [Test]
    public void Prefix_MethodAttribute_Delegate_ParameterCountMismatch_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_Delegate_ParameterCountMismatch_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("method: Method not found"));
    }

    [Test]
    public void Prefix_MethodAttribute_Delegate_ReturnTypeMismatch_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_Delegate_ReturnTypeMismatch_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("method: Return type mismatch"));
    }

    [Test]
    public void Prefix_MethodAttribute_DelegateWithoutInvoke_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_DelegateWithoutInvoke_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("method: Delegate.Invoke not found"));
    }

    [Test]
    public void Prefix_MethodAttribute_PrivateInstanceMethod_Invokes()
    {
        MethodBindingPatches.ResultObserved = 0;
        ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_PrivateInstanceMethod_Invokes));
        MethodBindingInstanceTargets target = new();

        int result = target.TargetInstanceMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(10));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(105));
        });
    }

    [Test]
    public void Prefix_MethodAttribute_PrivateStaticMethod_Invokes()
    {
        MethodBindingPatches.ResultObserved = 0;
        ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_PrivateStaticMethod_Invokes));
        MethodBindingInstanceTargets target = new();

        int result = target.TargetInstanceMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(10));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(205));
        });
    }

    [Test]
    public void Prefix_MethodAttribute_MixedDispatch_DelegatesRemainIndependent()
    {
        MethodBindingPatches.DescriptionObserved = null;
        MethodBindingPatches.VirtualDescriptionObserved = null;
        ApplyPatch(typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_MixedDispatch_DelegatesRemainIndependent));
        MethodBindingVirtualBaseTargets target = new MethodBindingVirtualDerivedTargets { InstanceName = "outer" };

        int result = target.TargetInstanceMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(50));
            Assert.That(MethodBindingPatches.DescriptionObserved, Is.EqualTo("base:outer:patch"));
            Assert.That(MethodBindingPatches.VirtualDescriptionObserved, Is.EqualTo("derived:outer:patch"));
        });
    }

    [Test]
    public void Prefix_MethodAttribute_NonVirtualCall_UsesDeclaredImplementation()
    {
        MethodBindingPatches.DescriptionObserved = null;
        ApplyPatch(typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_NonVirtualCall_UsesDeclaredImplementation));
        MethodBindingVirtualBaseTargets target = new MethodBindingVirtualDerivedTargets { InstanceName = "outer" };

        int result = target.TargetInstanceMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(50));
            Assert.That(MethodBindingPatches.DescriptionObserved, Is.EqualTo("base:outer:patch"));
        });
    }

    [Test]
    public void Prefix_MethodAttribute_NonVirtualCall_NullNameUsesParameterName()
    {
        MethodBindingPatches.DescriptionObserved = null;
        ApplyPatch(typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_NonVirtualCall_NullNameUsesParameterName));
        MethodBindingVirtualBaseTargets target = new MethodBindingVirtualDerivedTargets { InstanceName = "outer" };

        int result = target.TargetInstanceMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(50));
            Assert.That(MethodBindingPatches.DescriptionObserved, Is.EqualTo("base:outer:patch"));
        });
    }

    [Test]
    public void Prefix_MethodAttribute_NonVirtualCall_UsesDeclaredOverride()
    {
        MethodBindingPatches.DescriptionObserved = null;
        ApplyPatch(typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_NonVirtualCall_UsesDeclaredOverride));
        MethodBindingVirtualDerivedTargets target = new MethodBindingVirtualDerivedTargets { InstanceName = "outer" };

        int result = target.TargetDerivedInstanceMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(60));
            Assert.That(MethodBindingPatches.DescriptionObserved, Is.EqualTo("derived:outer:patch"));
        });
    }

    [Test]
    public void InnerPrefix_MethodAttribute_NonVirtualCall_InnerScopeUsesInnerInstance()
    {
        MethodBindingPatches.DescriptionObserved = null;
        ApplyPatch(typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.InnerPrefix_MethodAttribute_NonVirtualCall_InnerScopeUsesInnerInstance));
        MethodBindingVirtualBaseTargets target = new MethodBindingVirtualDerivedTargets { InstanceName = "outer" };
        MethodBindingVirtualBaseTargets inner = new MethodBindingVirtualDerivedTargets { InstanceName = "inner" };

        string result = target.CallInnerVirtualMethod(inner, "original");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("derived:inner:original"));
            Assert.That(MethodBindingPatches.DescriptionObserved, Is.EqualTo("base:inner:patch"));
        });
    }

    [Test]
    public void InnerPrefix_MethodAttribute_VirtualCall_InnerScopeUsesRuntimeOverride()
    {
        MethodBindingPatches.DescriptionObserved = null;
        ApplyPatch(typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.InnerPrefix_MethodAttribute_VirtualCall_InnerScopeUsesRuntimeOverride));
        MethodBindingVirtualBaseTargets target = new MethodBindingVirtualDerivedTargets { InstanceName = "outer" };
        MethodBindingVirtualBaseTargets inner = new MethodBindingVirtualDerivedTargets { InstanceName = "inner" };

        string result = target.CallInnerVirtualMethod(inner, "original");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("derived:inner:original"));
            Assert.That(MethodBindingPatches.DescriptionObserved, Is.EqualTo("derived:inner:patch"));
        });
    }

    [Test]
    public void InnerPrefix_MethodAttribute_NonVirtualCall_OuterScopeUsesOuterInstance()
    {
        MethodBindingPatches.DescriptionObserved = null;
        ApplyPatch(typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.InnerPrefix_MethodAttribute_NonVirtualCall_OuterScopeUsesOuterInstance));
        MethodBindingVirtualBaseTargets target = new MethodBindingVirtualDerivedTargets { InstanceName = "outer" };
        MethodBindingVirtualBaseTargets inner = new MethodBindingVirtualDerivedTargets { InstanceName = "inner" };

        string result = target.CallInnerVirtualMethod(inner, "original");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("derived:inner:original"));
            Assert.That(MethodBindingPatches.DescriptionObserved, Is.EqualTo("base:outer:patch"));
        });
    }

    [Test]
    public void Prefix_MethodAttribute_VirtualMethod_DispatchesOnRuntimeInstance()
    {
        MethodBindingPatches.ResultObserved = 0;
        ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_VirtualMethod_DispatchesOnRuntimeInstance));
        MethodBindingVirtualBaseTargets target = new MethodBindingVirtualDerivedTargets();

        int result = target.TargetInstanceMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(50));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(605));
        });
    }

    [Test]
    public void Prefix_MethodAttribute_StructInstanceMethod_IsRejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(MethodBindingPatches),
                nameof(MethodBindingPatches.Prefix_MethodAttribute_StructInstanceMethod_IsRejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("method: [Method] is not supported for non-static methods on structs"));
    }

    [Test]
    public void Prefix_MethodAttribute_StaticMethodOnStruct_Invokes()
    {
        MethodBindingPatches.ResultObserved = 0;
        ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_StaticMethodOnStruct_Invokes));
        MethodBindingStructTargets target = new() { InstanceValue = 40 };

        int result = target.TargetInstanceMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(40));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(405));
        });
    }

    [Test]
    public void Prefix_MethodAttribute_ReadonlyInstanceMethodOnMutableStruct_Invokes()
    {
        MethodBindingPatches.ResultObserved = 0;
        ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_ReadonlyInstanceMethodOnMutableStruct_Invokes));
        MethodBindingStructTargets target = new() { InstanceValue = 40 };

        int result = target.TargetInstanceMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(40));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(45));
        });
    }

    [Test]
    public void Prefix_MethodAttribute_InstanceMethodOnReadonlyStruct_Invokes()
    {
        MethodBindingPatches.ResultObserved = 0;
        ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_InstanceMethodOnReadonlyStruct_Invokes));
        MethodBindingReadonlyStructTargets target = new(40);

        int result = target.TargetInstanceMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(60));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(45));
        });
    }

    [Test]
    public void InnerPrefix_MethodAttribute_InstanceMethodOnReadonlyInnerStruct_Invokes()
    {
        MethodBindingPatches.ResultObserved = 0;
        ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.InnerPrefix_MethodAttribute_InstanceMethodOnReadonlyInnerStruct_Invokes));
        MethodBindingReadonlyStructTargets inner = new(40);

        int result = MethodBindingStaticTargets.CallReadonlyStructInstanceMethod(inner);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(60));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(45));
        });
    }

    [Test]
    public void InnerPrefix_MethodAttribute_ReadonlyOuterDoesNotAllowMutableInnerStructMethod()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(MethodBindingPatches),
                nameof(MethodBindingPatches.InnerPrefix_MethodAttribute_ReadonlyOuterDoesNotAllowMutableInnerStructMethod)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("method: [Method] is not supported for non-static methods on structs"));
    }

    [Test]
    public void Prefix_MethodAttribute_OverloadedMethod_SelectsDelegateSignature()
    {
        MethodBindingPatches.ResultObserved = 0;
        ApplyPatch(typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_OverloadedMethod_SelectsDelegateSignature));

        new MethodBindingInstanceTargets().TargetInstanceMethod();

        Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(5));
    }

    [Test]
    public void Prefix_MethodAttribute_MissingMethod_IsRejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(MethodBindingPatches),
                nameof(MethodBindingPatches.Prefix_MethodAttribute_MissingMethod_IsRejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("method: Method not found"));
    }

    [Test]
    public void Prefix_MethodAttribute_InstanceMethodOnStaticTarget_IsRejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(MethodBindingPatches),
                nameof(MethodBindingPatches.Prefix_MethodAttribute_InstanceMethodOnStaticTarget_IsRejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("method: Instance required"));
    }

    [Test]
    public void Prefix_MethodAttribute_InnerScopeWithoutInnerPatch_IsRejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(MethodBindingPatches),
                nameof(MethodBindingPatches.Prefix_MethodAttribute_InnerScopeWithoutInnerPatch_IsRejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("method: Invalid scope"));
    }

    [Test]
    public void InnerPrefix_MethodAttribute_StaticMethodOnInnerInstanceType_Invokes()
    {
        MethodBindingPatches.ResultObserved = 0;
        ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.InnerPrefix_MethodAttribute_StaticMethodOnInnerInstanceType_Invokes));
        MethodBindingInstanceTargets target = new();
        MethodBindingInnerTargets inner = new();

        int result = target.CallInnerInstanceMethod(inner);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(20));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(205));
        });
    }

    [Test]
    public void InnerPrefix_MethodAttribute_InstanceMethodOnInnerInstance_BindsInnerInstance()
    {
        MethodBindingPatches.ResultObserved = 0;
        ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.InnerPrefix_MethodAttribute_InstanceMethodOnInnerInstance_BindsInnerInstance));
        MethodBindingInstanceTargets target = new() { InstanceValue = 7 };
        MethodBindingInnerTargets inner = new() { InstanceValue = 40 };

        int result = target.CallInnerInstanceMethod(inner);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(20));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(45));
        });
    }

    [Test]
    public void InnerPrefix_MethodAttribute_InnerScope_InstanceMethodOnInnerInstance_BindsInnerInstance()
    {
        MethodBindingPatches.ResultObserved = 0;
        ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.InnerPrefix_MethodAttribute_InnerScope_InstanceMethodOnInnerInstance_BindsInnerInstance));
        MethodBindingInstanceTargets target = new() { InstanceValue = 7 };
        MethodBindingInnerTargets inner = new() { InstanceValue = 40 };

        int result = target.CallInnerInstanceMethod(inner);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(20));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(45));
        });
    }

    [Test]
    public void Prefix_MethodAttribute_StaticMethodOnStaticType_Invokes()
    {
        MethodBindingPatches.ResultObserved = 0;
        ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.Prefix_MethodAttribute_StaticMethodOnStaticType_Invokes));

        int result = MethodBindingStaticTargets.TargetStaticMethod();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(30));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(305));
        });
    }

    [Test]
    public void InnerPrefix_MethodAttribute_OuterScope_InstanceMethodOnOuterInstance_Invokes()
    {
        MethodBindingPatches.ResultObserved = 0;
        ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.InnerPrefix_MethodAttribute_OuterScope_InstanceMethodOnOuterInstance_Invokes));
        MethodBindingInstanceTargets target = new() { InstanceValue = 7 };
        MethodBindingInnerTargets inner = new();

        int result = target.CallInnerInstanceMethod(inner);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(20));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(12));
        });
    }

    [Test]
    public void IteratorInnerPrefix_MethodAttribute_InnerScope_BindsInnerInstance()
    {
        MethodBindingPatches.ResultObserved = 0;
        ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.IteratorInnerPrefix_MethodAttribute_InnerScope_BindsInnerInstance));
        MethodBindingIteratorTargets target = new() { InstanceValue = 7 };
        MethodBindingInnerTargets inner = new() { InstanceValue = 40 };

        int result = target.EnumerateInnerInstanceMethod(inner).Single();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(20));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(45));
        });
    }

    [Test]
    public void IteratorInnerPrefix_MethodAttribute_OuterScopeInstanceMethod_IsRejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(MethodBindingPatches),
                nameof(MethodBindingPatches.IteratorInnerPrefix_MethodAttribute_OuterScopeInstanceMethod_IsRejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo(
                "method: [Method] is not supported for iterator state machines"));
    }

    [Test]
    public void IteratorInnerPrefix_MethodAttribute_OuterScopeStaticMethod_Invokes()
    {
        MethodBindingPatches.ResultObserved = 0;
        ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.IteratorInnerPrefix_MethodAttribute_OuterScopeStaticMethod_Invokes));
        MethodBindingIteratorTargets target = new();
        MethodBindingInnerTargets inner = new();

        int result = target.EnumerateInnerInstanceMethod(inner).Single();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(20));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(705));
        });
    }

    [Test]
    public void LocalFunctionInnerPrefix_MethodAttribute_OuterScope_BindsDeclaringInstance()
    {
        MethodBindingPatches.ResultObserved = 0;
        ApplyPatch(
            typeof(MethodBindingPatches),
            nameof(MethodBindingPatches.LocalFunctionInnerPrefix_MethodAttribute_OuterScope_BindsDeclaringInstance));
        MethodBindingInstanceTargets target = new() { InstanceValue = 7 };

        int result = target.CallLocalFunction();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(8));
            Assert.That(MethodBindingPatches.ResultObserved, Is.EqualTo(12));
        });
    }
}
