namespace Disharmony.Tests.EndToEnd.ParameterBinding;

public static class BaseMethodBindingPatches
{
    [Prefix] [Inner(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.DescribeNonVirtual))]
    [Target(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.CallInner))]
    public static void InnerPrefix_BaseMethod_DefaultScope_UsesInnerInstance(
        Func<int, string> __base) => resultObserved = __base(5);

    [Postfix] [Inner(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.DescribeNonVirtual))]
    [Target(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.CallInner))]
    public static void InnerPostfix_BaseMethod_DefaultScope_UsesInnerInstance(
        Func<int, string> __base) => resultObserved = __base(5);

    [Prefix] [Inner(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.DescribeNonVirtual))]
    [Target(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.CallInner))]
    public static void InnerPrefix_BaseMethodAttribute_DefaultScope_UsesInnerInstance(
        [BaseMethod] Func<int, string> baseMethod) => resultObserved = baseMethod(5);

    [Prefix] [Inner(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.DescribeNonVirtual))]
    [Target(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.CallInner))]
    public static void InnerPrefix_BaseMethodAttribute_InnerScope_UsesInnerInstance(
        [BaseMethod(Scope.Inner)] Func<int, string> baseMethod) => resultObserved = baseMethod(5);

    [Prefix] [Inner(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.DescribeNonVirtual))]
    [Target(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.CallInner))]
    public static void InnerPrefix_BaseMethodAttribute_OuterScope_UsesOuterInstance(
        DerivedMethodTargets inner, [BaseMethod(Scope.Outer)] Func<DerivedMethodTargets, int, string> baseMethod) => resultObserved = baseMethod(inner, 5);

    [Prefix] [Inner(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.DescribeNonVirtual))]
    [Target(typeof(StaticMethodTargets), nameof(StaticMethodTargets.EnumerateDescription))]
    public static void IteratorInnerPrefix_BaseMethodAttribute_InnerScope_UsesInnerInstance(
        [BaseMethod(Scope.Inner)] Func<int, string> baseMethod) => resultObserved = baseMethod(5);

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Void))]
    [Target(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.EnumerateDescription))]
    public static void IteratorInnerPrefix_BaseMethodAttribute_OuterScope_DeclaredSignature_Rejected(
        [BaseMethod(Scope.Outer)] Func<int, IEnumerable<string>> baseMethod) { }

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Void))]
    [Target(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.EnumerateDescription))]
    public static void IteratorInnerPrefix_BaseMethodAttribute_OuterScope_MoveNextSignature_Rejected(
        [BaseMethod(Scope.Outer)] Func<bool> baseMethod) { }

    public static string? resultObserved;

    [Prefix]
    [Target(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.Describe))]
    public static bool Prefix_BaseMethod_Parameter_Primitive_Result_ReferenceType_Invokes(
        int value,
        Func<int, string> __base,
        ref string __result)
    {
        __result = __base(value);
        return false;
    }

    [Postfix]
    [Target(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.Describe))]
    public static void Postfix_BaseMethod_Parameter_Primitive_Result_ReferenceType_Invokes(
        int value,
        Func<int, string> __base,
        ref string __result) =>
        __result = $"{__result}|{__base(value)}";

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Void))]
    [Target(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.DescribeWithInnerCall))]
    public static void InnerPrefix_BaseMethodAttribute_OuterScope_Parameter_Primitive_Result_ReferenceType_Invokes(
        int value,
        [BaseMethod(Scope.Outer)] Func<int, string> __base) =>
        resultObserved = __base(value);

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Void))]
    [Target(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.DescribeWithInnerCall))]
    public static void InnerPostfix_BaseMethodAttribute_OuterScope_Parameter_Primitive_Result_ReferenceType_Invokes(
        int value,
        [BaseMethod(Scope.Outer)] Func<int, string> __base) =>
        resultObserved = __base(value);

    [Prefix]
    [Target(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.Describe))]
    public static void Prefix_BaseMethod_Delegate_ParameterTypeMismatch_RejectedByPatch(Func<string, string> __base) { }

    [Prefix]
    [Target(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.Describe))]
    public static void Prefix_BaseMethod_Delegate_ParameterCountMismatch_RejectedByPatch(Func<string> __base) { }

    [Prefix]
    [Target(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.Describe))]
    public static void Prefix_BaseMethod_Delegate_ReturnTypeMismatch_RejectedByPatch(Func<int, int> __base) { }

    [Prefix]
    [Target(typeof(DerivedMethodTargets), nameof(DerivedMethodTargets.Describe))]
    public static bool Prefix_BaseMethodAttribute_ReturnValueAttribute_NonReservedNames_Invokes(
        int value,
        [BaseMethod] Func<int, string> baseMethod,
        [ReturnValue] ref string result)
    {
        result = baseMethod(value);
        return false;
    }

    [Prefix]
    [Target(typeof(BaseMethodOverloadDerivedTargets), nameof(BaseMethodOverloadDerivedTargets.Describe),
        parameterTypes: [typeof(int)])]
    public static bool Prefix_BaseMethod_OverloadedMethod_SelectsMatchingBaseOverload(
        int value,
        Func<int, string> __base,
        ref string __result)
    {
        __result = __base(value);
        return false;
    }

    [Prefix]
    [Target(typeof(BaseMethodGrandchildTargets), nameof(BaseMethodGrandchildTargets.Describe))]
    public static bool Prefix_BaseMethod_InheritedGrandparentImplementation_Invokes(
        int value,
        Func<int, string> __base,
        ref string __result)
    {
        __result = __base(value);
        return false;
    }

    [Prefix]
    [Target(typeof(MethodBindingStaticTargets), nameof(MethodBindingStaticTargets.TargetStaticMethod))]
    public static void Prefix_BaseMethod_StaticTarget_RejectedByPatch(Func<int> __base) { }

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.IntResult))]
    public static void Prefix_BaseMethod_NoBaseImplementation_RejectedByPatch(Func<int> __base) { }

    [Prefix]
    [Target(typeof(ConstructorTargets), memberType: MemberType.Constructor, parameterTypes: [])]
    public static void Prefix_BaseMethod_ConstructorTarget_RejectedByPatch(Action __base) { }

    [Prefix]
    [Target(typeof(BaseMethodAbstractDerivedTargets), nameof(BaseMethodAbstractDerivedTargets.Describe))]
    public static void Prefix_BaseMethod_AbstractBaseImplementation_RejectedByPatch(Func<int, string> __base) { }
}

[TestFixture]
public sealed class BaseMethodBindingTests : PatchTestBase
{
    [Test]
    public void InnerPrefix_BaseMethod_DefaultScope_UsesInnerInstance()
    {
        BaseMethodBindingPatches.resultObserved = null;
        var outer = new DerivedMethodTargets { InstanceValue = 7 };
        var inner = new DerivedMethodTargets { InstanceValue = 19 };
        ApplyPatch(typeof(BaseMethodBindingPatches),
            nameof(BaseMethodBindingPatches.InnerPrefix_BaseMethod_DefaultScope_UsesInnerInstance));

        string result = outer.CallInner(inner, 41);

        Assert.That(result, Is.EqualTo("derived:41:19"));
        Assert.That(BaseMethodBindingPatches.resultObserved, Is.EqualTo("base:5:19"));
    }

    [Test]
    public void InnerPostfix_BaseMethod_DefaultScope_UsesInnerInstance()
    {
        BaseMethodBindingPatches.resultObserved = null;
        var outer = new DerivedMethodTargets { InstanceValue = 7 };
        var inner = new DerivedMethodTargets { InstanceValue = 19 };
        ApplyPatch(typeof(BaseMethodBindingPatches),
            nameof(BaseMethodBindingPatches.InnerPostfix_BaseMethod_DefaultScope_UsesInnerInstance));

        string result = outer.CallInner(inner, 41);

        Assert.That(result, Is.EqualTo("derived:41:19"));
        Assert.That(BaseMethodBindingPatches.resultObserved, Is.EqualTo("base:5:19"));
    }

    [Test]
    public void InnerPrefix_BaseMethodAttribute_DefaultScope_UsesInnerInstance()
    {
        BaseMethodBindingPatches.resultObserved = null;
        var outer = new DerivedMethodTargets { InstanceValue = 7 };
        var inner = new DerivedMethodTargets { InstanceValue = 19 };
        ApplyPatch(typeof(BaseMethodBindingPatches),
            nameof(BaseMethodBindingPatches.InnerPrefix_BaseMethodAttribute_DefaultScope_UsesInnerInstance));

        string result = outer.CallInner(inner, 41);

        Assert.That(result, Is.EqualTo("derived:41:19"));
        Assert.That(BaseMethodBindingPatches.resultObserved, Is.EqualTo("base:5:19"));
    }

    [Test]
    public void InnerPrefix_BaseMethodAttribute_InnerScope_UsesInnerInstance()
    {
        BaseMethodBindingPatches.resultObserved = null;
        var outer = new DerivedMethodTargets { InstanceValue = 7 };
        var inner = new DerivedMethodTargets { InstanceValue = 19 };
        ApplyPatch(typeof(BaseMethodBindingPatches),
            nameof(BaseMethodBindingPatches.InnerPrefix_BaseMethodAttribute_InnerScope_UsesInnerInstance));

        string result = outer.CallInner(inner, 41);

        Assert.That(result, Is.EqualTo("derived:41:19"));
        Assert.That(BaseMethodBindingPatches.resultObserved, Is.EqualTo("base:5:19"));
    }

    [Test]
    public void InnerPrefix_BaseMethodAttribute_OuterScope_UsesOuterInstance()
    {
        BaseMethodBindingPatches.resultObserved = null;
        var outer = new DerivedMethodTargets { InstanceValue = 7 };
        var inner = new DerivedMethodTargets { InstanceValue = 19 };
        ApplyPatch(typeof(BaseMethodBindingPatches),
            nameof(BaseMethodBindingPatches.InnerPrefix_BaseMethodAttribute_OuterScope_UsesOuterInstance));

        string result = outer.CallInner(inner, 41);

        Assert.That(result, Is.EqualTo("derived:41:19"));
        Assert.That(BaseMethodBindingPatches.resultObserved, Is.EqualTo("outer-base:5:7"));
    }

    [Test]
    public void IteratorInnerPrefix_BaseMethodAttribute_InnerScope_UsesInnerInstance()
    {
        BaseMethodBindingPatches.resultObserved = null;
        var inner = new DerivedMethodTargets { InstanceValue = 19 };
        ApplyPatch(typeof(BaseMethodBindingPatches),
            nameof(BaseMethodBindingPatches.IteratorInnerPrefix_BaseMethodAttribute_InnerScope_UsesInnerInstance));

        string result = StaticMethodTargets.EnumerateDescription(inner, 41).Single();

        Assert.That(result, Is.EqualTo("derived:41:19"));
        Assert.That(BaseMethodBindingPatches.resultObserved, Is.EqualTo("base:5:19"));
    }

    [Test]
    public void IteratorInnerPrefix_BaseMethodAttribute_OuterScope_DeclaredSignature_Rejected()
    {
        // Explicit outer base binding is unsupported on an iterator inner patch.
        // The delegate matches the declared iterator, which has a concrete base implementation.
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(BaseMethodBindingPatches),
            nameof(BaseMethodBindingPatches.IteratorInnerPrefix_BaseMethodAttribute_OuterScope_DeclaredSignature_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("baseMethod: Not supported for state machine methods"));
    }

    [Test]
    public void IteratorInnerPrefix_BaseMethodAttribute_OuterScope_MoveNextSignature_Rejected()
    {
        // Func<bool> matches MoveNext rather than the declared iterator. Verify the explicit state-machine
        // guard rejects it, rather than relying on signature validation or a missing base implementation.
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(BaseMethodBindingPatches),
            nameof(BaseMethodBindingPatches.IteratorInnerPrefix_BaseMethodAttribute_OuterScope_MoveNextSignature_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("baseMethod: Not supported for state machine methods"));
    }

    [Test]
    public void Prefix_BaseMethod_Parameter_Primitive_Result_ReferenceType_Invokes()
    {
        ApplyPatch(
            typeof(BaseMethodBindingPatches),
            nameof(BaseMethodBindingPatches.Prefix_BaseMethod_Parameter_Primitive_Result_ReferenceType_Invokes));
        var target = new DerivedMethodTargets { InstanceValue = 1 };

        string result = target.Describe(41);

        Assert.That(result, Is.EqualTo("base:41:1"));
    }

    [Test]
    public void Postfix_BaseMethod_Parameter_Primitive_Result_ReferenceType_Invokes()
    {
        ApplyPatch(
            typeof(BaseMethodBindingPatches),
            nameof(BaseMethodBindingPatches.Postfix_BaseMethod_Parameter_Primitive_Result_ReferenceType_Invokes));
        var target = new DerivedMethodTargets { InstanceValue = 1 };

        string result = target.Describe(41);

        Assert.That(result, Is.EqualTo("derived:41:1|base:41:1"));
    }

    [Test]
    public void InnerPrefix_BaseMethodAttribute_OuterScope_Parameter_Primitive_Result_ReferenceType_Invokes()
    {
        BaseMethodBindingPatches.resultObserved = null;
        ApplyPatch(
            typeof(BaseMethodBindingPatches),
            nameof(BaseMethodBindingPatches.InnerPrefix_BaseMethodAttribute_OuterScope_Parameter_Primitive_Result_ReferenceType_Invokes));
        var target = new DerivedMethodTargets { InstanceValue = 1 };

        string result = target.DescribeWithInnerCall(41);

        Assert.That(result, Is.EqualTo("derived:41:1"));
        Assert.That(BaseMethodBindingPatches.resultObserved, Is.EqualTo("base:41:1"));
    }

    [Test]
    public void InnerPostfix_BaseMethodAttribute_OuterScope_Parameter_Primitive_Result_ReferenceType_Invokes()
    {
        BaseMethodBindingPatches.resultObserved = null;
        ApplyPatch(
            typeof(BaseMethodBindingPatches),
            nameof(BaseMethodBindingPatches.InnerPostfix_BaseMethodAttribute_OuterScope_Parameter_Primitive_Result_ReferenceType_Invokes));
        var target = new DerivedMethodTargets { InstanceValue = 1 };

        string result = target.DescribeWithInnerCall(41);

        Assert.That(result, Is.EqualTo("derived:41:1"));
        Assert.That(BaseMethodBindingPatches.resultObserved, Is.EqualTo("base:41:1"));
    }

    [Test]
    public void Prefix_BaseMethod_Delegate_ParameterTypeMismatch_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(BaseMethodBindingPatches),
                nameof(BaseMethodBindingPatches.Prefix_BaseMethod_Delegate_ParameterTypeMismatch_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
        Assert.That(exception.InnerException!.Message, Is.EqualTo("__base: Parameter type mismatch"));
    }

    [Test]
    public void Prefix_BaseMethod_Delegate_ParameterCountMismatch_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(BaseMethodBindingPatches),
                nameof(BaseMethodBindingPatches.Prefix_BaseMethod_Delegate_ParameterCountMismatch_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
        Assert.That(exception.InnerException!.Message, Is.EqualTo("__base: Parameter type mismatch"));
    }

    [Test]
    public void Prefix_BaseMethod_Delegate_ReturnTypeMismatch_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(BaseMethodBindingPatches),
                nameof(BaseMethodBindingPatches.Prefix_BaseMethod_Delegate_ReturnTypeMismatch_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
        Assert.That(exception.InnerException!.Message, Is.EqualTo("__base: Return type mismatch"));
    }

    [Test]
    public void Prefix_BaseMethodAttribute_ReturnValueAttribute_NonReservedNames_Invokes()
    {
        ApplyPatch(
            typeof(BaseMethodBindingPatches),
            nameof(BaseMethodBindingPatches.Prefix_BaseMethodAttribute_ReturnValueAttribute_NonReservedNames_Invokes));
        var target = new DerivedMethodTargets { InstanceValue = 1 };

        string result = target.Describe(41);

        Assert.That(result, Is.EqualTo("base:41:1"));
    }

    [Test]
    public void Prefix_BaseMethod_OverloadedMethod_SelectsMatchingBaseOverload()
    {
        ApplyPatch(
            typeof(BaseMethodBindingPatches),
            nameof(BaseMethodBindingPatches.Prefix_BaseMethod_OverloadedMethod_SelectsMatchingBaseOverload));
        var target = new BaseMethodOverloadDerivedTargets();

        string result = target.Describe(41);

        Assert.That(result, Is.EqualTo("base-int:41"));
    }

    [Test]
    public void Prefix_BaseMethod_InheritedGrandparentImplementation_Invokes()
    {
        ApplyPatch(
            typeof(BaseMethodBindingPatches),
            nameof(BaseMethodBindingPatches.Prefix_BaseMethod_InheritedGrandparentImplementation_Invokes));
        var target = new BaseMethodGrandchildTargets();

        string result = target.Describe(42);

        Assert.That(result, Is.EqualTo("grandparent:42"));
    }

    [Test]
    public void Prefix_BaseMethod_StaticTarget_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(BaseMethodBindingPatches),
            nameof(BaseMethodBindingPatches.Prefix_BaseMethod_StaticTarget_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("__base: Must be an instance method"));
    }

    [Test]
    public void Prefix_BaseMethod_NoBaseImplementation_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(BaseMethodBindingPatches),
            nameof(BaseMethodBindingPatches.Prefix_BaseMethod_NoBaseImplementation_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("__base: Base method not found"));
    }

    [Test]
    public void Prefix_BaseMethod_ConstructorTarget_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(BaseMethodBindingPatches),
            nameof(BaseMethodBindingPatches.Prefix_BaseMethod_ConstructorTarget_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>()
            .With.Message.EqualTo("__base: Must be an instance method"));
    }

    [Test]
    public void Prefix_BaseMethod_AbstractBaseImplementation_RejectedByPatch()
    {
        var exception = Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(BaseMethodBindingPatches),
            nameof(BaseMethodBindingPatches.Prefix_BaseMethod_AbstractBaseImplementation_RejectedByPatch)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }
}
