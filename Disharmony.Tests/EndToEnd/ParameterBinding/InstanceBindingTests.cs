namespace Disharmony.Tests.EndToEnd.ParameterBinding;

public static partial class InstanceBindingPatches
{
    public static ClassMethodTargets? instanceObserved;
    public static ClassMethodTargets? replacementInstance;
    public static ClassMethodTargets? callerObserved;
    public static ClassMethodTargets? replacementCaller;
    public static int structInstanceFieldObserved;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_InstanceParameter_ReferenceType_ReadByValue(ClassMethodTargets __instance) => instanceObserved = __instance;

    [Postfix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Postfix_InstanceParameter_ReferenceType_ReadByValue(ClassMethodTargets __instance) => instanceObserved = __instance;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Self))]
    public static void Prefix_InstanceParameter_ReferenceType_WriteByReference(ref ClassMethodTargets __instance) =>
        __instance = replacementInstance!;

    [Prefix]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.IntResult))]
    public static void Prefix_InstanceParameter_Struct_ReadByValue(StructMethodTargets __instance) =>
        structInstanceFieldObserved = __instance.foo;

    [Prefix]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.ToString), parameterTypes: [])]
    public static void Prefix_InstanceParameter_Struct_VirtualMethod_ReadByValue(StructMethodTargets __instance) =>
        structInstanceFieldObserved = __instance.foo;

    [Prefix]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.IntResult))]
    public static void Prefix_InstanceParameter_Struct_WriteByReference(ref StructMethodTargets __instance) =>
        __instance.foo = 42;

    [Postfix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Self))]
    public static void Postfix_InstanceParameter_ReferenceType_WriteByReference_Rejected(
        ref ClassMethodTargets __instance,
        ref ClassMethodTargets __result)
    {
        __instance = replacementInstance!;
        __result = __instance;
    }

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallStaticVoid))]
    public static void InnerPrefix_CallerParameter_ReferenceType_ReadByValue(ClassMethodTargets __caller) => callerObserved = __caller;

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallStaticVoid))]
    public static void InnerPostfix_CallerParameter_ReferenceType_ReadByValue(ClassMethodTargets __caller) => callerObserved = __caller;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallStaticVoidAndReturnValue))]
    public static void InnerPrefix_CallerParameter_ReferenceType_WriteByReference_Rejected(ref ClassMethodTargets __caller) =>
        __caller = replacementCaller!;

    [Prefix] [Inner(typeof(InstanceMethodTargetsWithoutFields), nameof(InstanceMethodTargetsWithoutFields.Void))]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.CallInnerWithoutField))]
    public static void InnerPrefix_CallerParameter_Struct_ReadByValue(StructMethodTargets __caller) =>
        structInstanceFieldObserved = __caller.foo;

    [Prefix] [Inner(typeof(InstanceMethodTargetsWithoutFields), nameof(InstanceMethodTargetsWithoutFields.Void))]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.CallInnerWithoutField))]
    public static void InnerPrefix_CallerParameter_Struct_WriteByReference(ref StructMethodTargets __caller) =>
        __caller.foo = 42;

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallStaticVoidAndReturnValue))]
    public static void InnerPostfix_CallerParameter_ReferenceType_WriteByReference_Rejected(ref ClassMethodTargets __caller) =>
        __caller = replacementCaller!;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Void))]
    public static void Prefix_InstanceAttribute_ReferenceType_ReadByValue([Instance] ClassMethodTargets target) =>
        instanceObserved = target;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Self))]
    public static void Prefix_InstanceAttribute_ReferenceType_WriteByReference(
        [Instance] ref ClassMethodTargets target) => target = replacementInstance!;

    [Prefix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallInnerWithField))]
    public static void InnerPrefix_InstanceAttribute_OuterScope_ReferenceType_ReadByValue(
        [Instance(Scope.Outer)] ClassMethodTargets target) =>
        instanceObserved = target;

    [Prefix] [Inner(typeof(InstanceMethodTargetsWithoutFields), nameof(InstanceMethodTargetsWithoutFields.Void))]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.CallInnerWithoutField))]
    public static void InnerPrefix_InstanceAttribute_OuterScope_Struct_ReadByValue(
        [Instance(Scope.Outer)] StructMethodTargets target) => structInstanceFieldObserved = target.foo;

    [Prefix] [Inner(typeof(InstanceMethodTargetsWithoutFields), nameof(InstanceMethodTargetsWithoutFields.Void))]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.CallInnerWithoutField))]
    public static void InnerPrefix_InstanceAttribute_OuterScope_Struct_WriteByReference(
        [Instance(Scope.Outer)] ref StructMethodTargets target) => target.foo = 42;

    [Prefix]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.IntResult))]
    public static void Prefix_InstanceAttribute_Struct_WriteByReference([Instance] ref StructMethodTargets target) =>
        target.foo = 42;

    [Postfix]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.IntResult))]
    public static void Prefix_InstanceAttribute_Struct_ReadByValue([Instance] StructMethodTargets target) =>
        structInstanceFieldObserved = target.foo;
}

public static partial class InstanceBindingPatches
{
    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Self))]
    public static void Prefix_InstanceParameter_ReferenceType_AsObjectByWriteableReference_Rejected(
        ref object __instance) { }

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Self))]
    public static void Prefix_InstanceParameter_ReferenceType_AsObjectByReadonlyReference(
        in object __instance) => instanceObserved = (ClassMethodTargets)__instance;

    [Prefix]
    [Target(typeof(BaseMethodTargets), nameof(BaseMethodTargets.Describe))]
    public static void Prefix_InstanceParameter_ReferenceType_AsDerivedByOutReference(
        out DerivedMethodTargets __instance) => __instance = new DerivedMethodTargets { InstanceValue = 42 };

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Self))]
    public static void Prefix_InstanceParameter_ReferenceType_ReadByReference(ref ClassMethodTargets __instance) =>
        instanceObserved = __instance;

    [Prefix]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.IntResult))]
    public static void Prefix_InstanceParameter_Struct_ReadByReference(ref StructMethodTargets __instance) =>
        structInstanceFieldObserved = __instance.foo;

    [Postfix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Self))]
    public static void Postfix_InstanceParameter_ReferenceType_ReadByReference_Rejected(
        ref ClassMethodTargets __instance,
        ref ClassMethodTargets __result) => instanceObserved = __instance;

    [Prefix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallStaticVoidAndReturnValue))]
    public static void InnerPrefix_CallerParameter_ReferenceType_ReadByReference_Rejected(ref ClassMethodTargets __caller) =>
        callerObserved = __caller;

    [Prefix] [Inner(typeof(InstanceMethodTargetsWithoutFields), nameof(InstanceMethodTargetsWithoutFields.Void))]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.CallInnerWithoutField))]
    public static void InnerPrefix_CallerParameter_Struct_ReadByReference(ref StructMethodTargets __caller) =>
        structInstanceFieldObserved = __caller.foo;

    [Postfix] [Inner(typeof(InnerStaticMethodTargets), nameof(InnerStaticMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallStaticVoidAndReturnValue))]
    public static void InnerPostfix_CallerParameter_ReferenceType_ReadByReference_Rejected(ref ClassMethodTargets __caller) =>
        callerObserved = __caller;

    [Prefix]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.Self))]
    public static void Prefix_InstanceAttribute_ReferenceType_ReadByReference(
        [Instance] ref ClassMethodTargets target) => instanceObserved = target;

    [Prefix] [Inner(typeof(InstanceMethodTargetsWithoutFields), nameof(InstanceMethodTargetsWithoutFields.Void))]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.CallInnerWithoutField))]
    public static void InnerPrefix_InstanceAttribute_OuterScope_Struct_ReadByReference(
        [Instance(Scope.Outer)] ref StructMethodTargets target) => structInstanceFieldObserved = target.foo;

    [Prefix]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.IntResult))]
    public static void Prefix_InstanceAttribute_Struct_ReadByReference(
        [Instance] ref StructMethodTargets target) => structInstanceFieldObserved = target.foo;
}

[TestFixture]
public sealed partial class InstanceBindingTests
{
    [Test]
    public void Prefix_InstanceParameter_ReferenceType_AsObjectByWriteableReference_Rejected()
    {
        Assert.Throws<PatchException>(() => ApplyPatch(
            typeof(InstanceBindingPatches),
            nameof(InstanceBindingPatches.Prefix_InstanceParameter_ReferenceType_AsObjectByWriteableReference_Rejected)));
    }

    [Test]
    public void Prefix_InstanceParameter_ReferenceType_AsObjectByReadonlyReference()
    {
        InstanceBindingPatches.instanceObserved = null;
        var target = new ClassMethodTargets();
        ApplyPatch(
            typeof(InstanceBindingPatches),
            nameof(InstanceBindingPatches.Prefix_InstanceParameter_ReferenceType_AsObjectByReadonlyReference));

        target.Self();

        Assert.That(InstanceBindingPatches.instanceObserved, Is.SameAs(target));
    }

    [Test]
    public void Prefix_InstanceParameter_ReferenceType_AsDerivedByOutReference()
    {
        ApplyPatch(
            typeof(InstanceBindingPatches),
            nameof(InstanceBindingPatches.Prefix_InstanceParameter_ReferenceType_AsDerivedByOutReference));
        var target = new BaseMethodTargets { InstanceValue = 1 };

        string result = target.Describe(7);

        Assert.That(result, Is.EqualTo("base:7:42"));
    }

    [Test]
    public void Prefix_InstanceParameter_ReferenceType_ReadByReference()
    {
        InstanceBindingPatches.instanceObserved = null;
        var target = new ClassMethodTargets();
        ApplyPatch(typeof(InstanceBindingPatches), nameof(InstanceBindingPatches.Prefix_InstanceParameter_ReferenceType_ReadByReference));
        target.Self();
        Assert.That(InstanceBindingPatches.instanceObserved, Is.SameAs(target));
    }

    [Test]
    public void Prefix_InstanceParameter_Struct_ReadByReference()
    {
        InstanceBindingPatches.structInstanceFieldObserved = 0;
        var target = new StructMethodTargets { foo = 42 };
        ApplyPatch(typeof(InstanceBindingPatches), nameof(InstanceBindingPatches.Prefix_InstanceParameter_Struct_ReadByReference));
        target.IntResult();
        Assert.That(InstanceBindingPatches.structInstanceFieldObserved, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_InstanceParameter_ReferenceType_ReadByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(InstanceBindingPatches),
                nameof(InstanceBindingPatches.Postfix_InstanceParameter_ReferenceType_ReadByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void InnerPrefix_CallerParameter_ReferenceType_ReadByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(InstanceBindingPatches),
                nameof(InstanceBindingPatches.InnerPrefix_CallerParameter_ReferenceType_ReadByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void InnerPrefix_CallerParameter_Struct_ReadByReference()
    {
        InstanceBindingPatches.structInstanceFieldObserved = 0;
        var target = new StructMethodTargets { foo = 42 };
        ApplyPatch(
            typeof(InstanceBindingPatches),
            nameof(InstanceBindingPatches.InnerPrefix_CallerParameter_Struct_ReadByReference));

        target.CallInnerWithoutField(new InstanceMethodTargetsWithoutFields());

        Assert.That(InstanceBindingPatches.structInstanceFieldObserved, Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfix_CallerParameter_ReferenceType_ReadByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(InstanceBindingPatches),
                nameof(InstanceBindingPatches.InnerPostfix_CallerParameter_ReferenceType_ReadByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void Prefix_InstanceAttribute_ReferenceType_ReadByReference()
    {
        InstanceBindingPatches.instanceObserved = null;
        var target = new ClassMethodTargets();
        ApplyPatch(typeof(InstanceBindingPatches), nameof(InstanceBindingPatches.Prefix_InstanceAttribute_ReferenceType_ReadByReference));
        target.Self();
        Assert.That(InstanceBindingPatches.instanceObserved, Is.SameAs(target));
    }

    [Test]
    public void InnerPrefix_InstanceAttribute_OuterScope_Struct_ReadByReference()
    {
        InstanceBindingPatches.structInstanceFieldObserved = 0;
        var target = new StructMethodTargets { foo = 42 };
        ApplyPatch(
            typeof(InstanceBindingPatches),
            nameof(InstanceBindingPatches.InnerPrefix_InstanceAttribute_OuterScope_Struct_ReadByReference));

        target.CallInnerWithoutField(new InstanceMethodTargetsWithoutFields());

        Assert.That(InstanceBindingPatches.structInstanceFieldObserved, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_InstanceAttribute_Struct_ReadByReference()
    {
        InstanceBindingPatches.structInstanceFieldObserved = 0;
        var target = new StructMethodTargets { foo = 42 };
        ApplyPatch(typeof(InstanceBindingPatches), nameof(InstanceBindingPatches.Prefix_InstanceAttribute_Struct_ReadByReference));
        target.IntResult();
        Assert.That(InstanceBindingPatches.structInstanceFieldObserved, Is.EqualTo(42));
    }
}

[TestFixture]
public sealed partial class InstanceBindingTests : PatchTestBase
{
    [Test]
    public void Prefix_InstanceParameter_ReferenceType_ReadByValue()
    {
        InstanceBindingPatches.instanceObserved = null;
        ApplyPatch(typeof(InstanceBindingPatches), nameof(InstanceBindingPatches.Prefix_InstanceParameter_ReferenceType_ReadByValue));
        var instance = new ClassMethodTargets();

        instance.Void();

        Assert.That(InstanceBindingPatches.instanceObserved, Is.SameAs(instance));
    }

    [Test]
    public void Postfix_InstanceParameter_ReferenceType_ReadByValue()
    {
        InstanceBindingPatches.instanceObserved = null;
        ApplyPatch(typeof(InstanceBindingPatches), nameof(InstanceBindingPatches.Postfix_InstanceParameter_ReferenceType_ReadByValue));
        var instance = new ClassMethodTargets();

        instance.Void();

        Assert.That(InstanceBindingPatches.instanceObserved, Is.SameAs(instance));
    }

    [Test]
    public void Prefix_InstanceParameter_ReferenceType_WriteByReference()
    {
        var original = new ClassMethodTargets();
        var replacement = new ClassMethodTargets();
        InstanceBindingPatches.replacementInstance = replacement;
        ApplyPatch(typeof(InstanceBindingPatches), nameof(InstanceBindingPatches.Prefix_InstanceParameter_ReferenceType_WriteByReference));

        ClassMethodTargets result = original.Self();

        Assert.That(result, Is.SameAs(replacement));
    }

    [Test]
    public void Prefix_InstanceParameter_Struct_ReadByValue()
    {
        InstanceBindingPatches.structInstanceFieldObserved = 0;
        ApplyPatch(typeof(InstanceBindingPatches), nameof(InstanceBindingPatches.Prefix_InstanceParameter_Struct_ReadByValue));
        var target = new StructMethodTargets { foo = 42 };

        target.IntResult();

        Assert.That(InstanceBindingPatches.structInstanceFieldObserved, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_InstanceParameter_Struct_VirtualMethod_ReadByValue()
    {
        InstanceBindingPatches.structInstanceFieldObserved = 0;
        ApplyPatch(
            typeof(InstanceBindingPatches),
            nameof(InstanceBindingPatches.Prefix_InstanceParameter_Struct_VirtualMethod_ReadByValue));
        var target = new StructMethodTargets { foo = 42 };

        string result = target.ToString();

        Assert.That(InstanceBindingPatches.structInstanceFieldObserved, Is.EqualTo(42));
        Assert.That(result, Is.EqualTo("StructMethodTargets:42"));
    }

    [Test]
    public void Prefix_InstanceParameter_Struct_WriteByReference()
    {
        ApplyPatch(typeof(InstanceBindingPatches), nameof(InstanceBindingPatches.Prefix_InstanceParameter_Struct_WriteByReference));
        var target = new StructMethodTargets { foo = 1 };

        target.IntResult();

        Assert.That(target.foo, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_InstanceParameter_ReferenceType_WriteByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(InstanceBindingPatches),
                nameof(InstanceBindingPatches.Postfix_InstanceParameter_ReferenceType_WriteByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }
}

public static partial class InstanceBindingPatches
{
    [Postfix]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.IntResult))]
    public static void Postfix_InstanceAttribute_Struct_ReadByReference(
        [Instance] ref StructMethodTargets instance) => structInstanceFieldObserved = instance.foo;

    [Postfix]
    [Target(typeof(StructMethodTargets), nameof(StructMethodTargets.IntResult))]
    public static void Postfix_InstanceAttribute_Struct_WriteByReference(
        [Instance] ref StructMethodTargets instance) => instance.foo = 42;

    [Postfix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallInnerWithField))]
    public static void InnerPostfix_InstanceAttribute_InnerScope_ReferenceType_ReadByReference_Rejected(
        [Instance(Scope.Inner)] ref InnerInstanceMethodTargets instance) => _ = instance.foo;

    [Postfix] [Inner(typeof(InnerInstanceMethodTargets), nameof(InnerInstanceMethodTargets.Void))]
    [Target(typeof(ClassMethodTargets), nameof(ClassMethodTargets.CallInnerWithField))]
    public static void InnerPostfix_InstanceAttribute_InnerScope_ReferenceType_WriteByReference_Rejected(
        [Instance(Scope.Inner)] ref InnerInstanceMethodTargets instance) =>
        instance = new InnerInstanceMethodTargets { foo = 42 };
}

[TestFixture]
public sealed partial class InstanceBindingTests
{
    [Test]
    public void Postfix_InstanceAttribute_Struct_ReadByReference()
    {
        InstanceBindingPatches.structInstanceFieldObserved = 0;
        var target = new StructMethodTargets { foo = 42 };
        ApplyPatch(
            typeof(InstanceBindingPatches),
            nameof(InstanceBindingPatches.Postfix_InstanceAttribute_Struct_ReadByReference));

        target.IntResult();

        Assert.That(InstanceBindingPatches.structInstanceFieldObserved, Is.EqualTo(42));
    }

    [Test]
    public void Postfix_InstanceAttribute_Struct_WriteByReference()
    {
        var target = new StructMethodTargets { foo = 1 };
        ApplyPatch(
            typeof(InstanceBindingPatches),
            nameof(InstanceBindingPatches.Postfix_InstanceAttribute_Struct_WriteByReference));

        target.IntResult();

        Assert.That(target.foo, Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfix_InstanceAttribute_InnerScope_ReferenceType_ReadByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(InstanceBindingPatches),
                nameof(InstanceBindingPatches.InnerPostfix_InstanceAttribute_InnerScope_ReferenceType_ReadByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void InnerPostfix_InstanceAttribute_InnerScope_ReferenceType_WriteByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(InstanceBindingPatches),
                nameof(InstanceBindingPatches.InnerPostfix_InstanceAttribute_InnerScope_ReferenceType_WriteByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }
}

[TestFixture]
public sealed partial class InstanceBindingTests
{
    [Test]
    public void InnerPrefix_CallerParameter_ReferenceType_ReadByValue()
    {
        InstanceBindingPatches.callerObserved = null;
        ApplyPatch(typeof(InstanceBindingPatches), nameof(InstanceBindingPatches.InnerPrefix_CallerParameter_ReferenceType_ReadByValue));
        var outer = new ClassMethodTargets();

        outer.CallStaticVoid();

        Assert.That(InstanceBindingPatches.callerObserved, Is.SameAs(outer));
    }

    [Test]
    public void InnerPostfix_CallerParameter_ReferenceType_ReadByValue()
    {
        InstanceBindingPatches.callerObserved = null;
        ApplyPatch(typeof(InstanceBindingPatches), nameof(InstanceBindingPatches.InnerPostfix_CallerParameter_ReferenceType_ReadByValue));
        var outer = new ClassMethodTargets();

        outer.CallStaticVoid();

        Assert.That(InstanceBindingPatches.callerObserved, Is.SameAs(outer));
    }

    [Test]
    public void InnerPrefix_CallerParameter_ReferenceType_WriteByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(InstanceBindingPatches),
                nameof(InstanceBindingPatches.InnerPrefix_CallerParameter_ReferenceType_WriteByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void InnerPrefix_CallerParameter_Struct_ReadByValue()
    {
        InstanceBindingPatches.structInstanceFieldObserved = 0;
        ApplyPatch(typeof(InstanceBindingPatches), nameof(InstanceBindingPatches.InnerPrefix_CallerParameter_Struct_ReadByValue));
        var outer = new StructMethodTargets { foo = 42 };

        outer.CallInnerWithoutField(new InstanceMethodTargetsWithoutFields());

        Assert.That(InstanceBindingPatches.structInstanceFieldObserved, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_CallerParameter_Struct_WriteByReference()
    {
        var target = new StructMethodTargets { foo = 1 };
        ApplyPatch(
            typeof(InstanceBindingPatches),
            nameof(InstanceBindingPatches.InnerPrefix_CallerParameter_Struct_WriteByReference));

        target.CallInnerWithoutField(new InstanceMethodTargetsWithoutFields());

        Assert.That(target.foo, Is.EqualTo(42));
    }

    [Test]
    public void InnerPostfix_CallerParameter_ReferenceType_WriteByReference_Rejected()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(
                typeof(InstanceBindingPatches),
                nameof(InstanceBindingPatches.InnerPostfix_CallerParameter_ReferenceType_WriteByReference_Rejected)));

        Assert.That(exception!.InnerException, Is.TypeOf<ParameterBindingException>());
    }

    [Test]
    public void Prefix_InstanceAttribute_ReferenceType_ReadByValue()
    {
        InstanceBindingPatches.instanceObserved = null;
        ApplyPatch(typeof(InstanceBindingPatches), nameof(InstanceBindingPatches.Prefix_InstanceAttribute_ReferenceType_ReadByValue));
        var target = new ClassMethodTargets();

        target.Void();

        Assert.That(InstanceBindingPatches.instanceObserved, Is.SameAs(target));
    }

    [Test]
    public void Prefix_InstanceAttribute_ReferenceType_WriteByReference()
    {
        var original = new ClassMethodTargets();
        var replacement = new ClassMethodTargets();
        InstanceBindingPatches.replacementInstance = replacement;
        ApplyPatch(typeof(InstanceBindingPatches), nameof(InstanceBindingPatches.Prefix_InstanceAttribute_ReferenceType_WriteByReference));

        ClassMethodTargets result = original.Self();

        Assert.That(result, Is.SameAs(replacement));
    }

    [Test]
    public void InnerPrefix_InstanceAttribute_OuterScope_ReferenceType_ReadByValue()
    {
        InstanceBindingPatches.instanceObserved = null;
        ApplyPatch(typeof(InstanceBindingPatches),
            nameof(InstanceBindingPatches.InnerPrefix_InstanceAttribute_OuterScope_ReferenceType_ReadByValue));
        var outer = new ClassMethodTargets();

        outer.CallInnerWithField(new InnerInstanceMethodTargets());

        Assert.That(InstanceBindingPatches.instanceObserved, Is.SameAs(outer));
    }

    [Test]
    public void InnerPrefix_InstanceAttribute_OuterScope_Struct_ReadByValue()
    {
        InstanceBindingPatches.structInstanceFieldObserved = 0;
        ApplyPatch(typeof(InstanceBindingPatches),
            nameof(InstanceBindingPatches.InnerPrefix_InstanceAttribute_OuterScope_Struct_ReadByValue));
        var outer = new StructMethodTargets { foo = 42 };

        outer.CallInnerWithoutField(new InstanceMethodTargetsWithoutFields());

        Assert.That(InstanceBindingPatches.structInstanceFieldObserved, Is.EqualTo(42));
    }

    [Test]
    public void InnerPrefix_InstanceAttribute_OuterScope_Struct_WriteByReference()
    {
        var target = new StructMethodTargets { foo = 1 };
        ApplyPatch(
            typeof(InstanceBindingPatches),
            nameof(InstanceBindingPatches.InnerPrefix_InstanceAttribute_OuterScope_Struct_WriteByReference));

        target.CallInnerWithoutField(new InstanceMethodTargetsWithoutFields());

        Assert.That(target.foo, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_InstanceAttribute_Struct_WriteByReference()
    {
        ApplyPatch(typeof(InstanceBindingPatches), nameof(InstanceBindingPatches.Prefix_InstanceAttribute_Struct_WriteByReference));
        var target = new StructMethodTargets { foo = 1 };

        target.IntResult();

        Assert.That(target.foo, Is.EqualTo(42));
    }

    [Test]
    public void Prefix_InstanceAttribute_Struct_ReadByValue()
    {
        InstanceBindingPatches.structInstanceFieldObserved = 0;
        ApplyPatch(typeof(InstanceBindingPatches), nameof(InstanceBindingPatches.Prefix_InstanceAttribute_Struct_ReadByValue));
        var target = new StructMethodTargets { foo = 42 };

        target.IntResult();

        Assert.That(InstanceBindingPatches.structInstanceFieldObserved, Is.EqualTo(42));
    }
}
