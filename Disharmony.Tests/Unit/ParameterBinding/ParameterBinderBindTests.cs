using System.Threading.Tasks;
using BoundParameter = Disharmony.ParameterBinding;

namespace Disharmony.Tests.Unit.ParameterBinding;

[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class UnsupportedParameterBindingAttribute() : ParameterBindingAttribute(Scope.Any);

internal static class ParameterBinderPatchMethods
{
    public static void Method_PropertyGetter_Named([Method(nameof(MethodBindingInstanceTargets.InstanceValue), memberType: MemberType.Getter)] Func<int> accessor) { }
    public static void Method_PropertySetter_Named([Method(nameof(MethodBindingInstanceTargets.InstanceValue), memberType: MemberType.Setter)] Action<int> accessor) { }
    public static void Method_PropertyGetter_ExplicitTypeNullName([Method(typeof(MethodBindingInstanceTargets), null, memberType: MemberType.Getter)] Func<int> InstanceValue) { }
    public static void Method_PropertySetter_ExplicitTypeNullName([Method(typeof(MethodBindingInstanceTargets), null, memberType: MemberType.Setter)] Action<int> InstanceValue) { }
    public static void Method_PropertyGetter_ImplicitName([Method(Scope.Inner, memberType: MemberType.Getter)] Func<int> InstanceValue) { }
    public static void Method_PropertySetter_ImplicitName([Method(Scope.Inner, memberType: MemberType.Setter)] Action<int> InstanceValue) { }
    public static void MemberInfo_TypedMethodInfo([MemberInfo] MethodInfo member) { }
    public static void MemberInfo_InnerTypedMethodInfo([MemberInfo(Scope.Inner)] MethodInfo member) { }
    public static void MemberInfo_TypedConstructorInfo([MemberInfo] ConstructorInfo member) { }
    public static void MemberInfo_InnerTypedConstructorInfo([MemberInfo(Scope.Inner)] ConstructorInfo member) { }
    public static void MemberInfo_TypedFieldInfo([MemberInfo] FieldInfo member) { }
    public static void MemberInfo_InnerTypedFieldInfo([MemberInfo(Scope.Inner)] FieldInfo member) { }
    public static void MemberInfo_TypedMethodBase([MemberInfo] MethodBase member) { }
    public static void MemberInfo_InnerTypedMethodBase([MemberInfo(Scope.Inner)] MethodBase member) { }
    public static void Arguments_Default([Arguments] object[] values) { }
    public static void Arguments_ReservedName(object[] __args) { }
    public static void Arguments_Outer([Arguments(Scope.Outer)] object[] values) { }
    public static void Arguments_Inner([Arguments(Scope.Inner)] object[] values) { }
    public static void Arguments_Ref([Arguments] ref object[] values) { }
    public static void Arguments_In([Arguments] in object[] values) { }
    public static void Arguments_Out([Arguments] out object[] values) => values = null!;
    public static void Arguments_StringArray([Arguments] string[] values) { }
    public static void Arguments_ValueArray([Arguments] int[] values) { }
    public static void Field_ExplicitType_NullName_InnerScope(
        [Field(typeof(FieldLookupBaseTargets), null, Scope.Inner)] int Value) { }
    public static void Method_ExplicitType_NullName_InnerScope(
        [Method(typeof(MethodBindingVirtualBaseTargets), null, Scope.Inner)] Func<string, string> Describe) { }
    public static void Field_ExplicitType_NullName_OuterScope(
        [Field(typeof(FieldLookupBaseTargets), null, Scope.Outer)] int Value) { }
    public static void Method_ExplicitType_NullName_OuterScope(
        [Method(typeof(MethodBindingVirtualBaseTargets), null, Scope.Outer)] Func<string, string> Describe) { }
    public static void Method_ExplicitType_Overload(
        [Method(typeof(MethodLookupTarget), nameof(MethodLookupTarget.Convert))] Func<string, string> method) { }
    public static void Method_ExplicitType_IncompatibleInstance(
        [Method(typeof(MethodBindingInstanceTargets), nameof(MethodBindingInstanceTargets.BoundInstanceMethod))] Func<int, int> method) { }
    public static void Field_ExplicitType_IncompatibleInstance(
        [Field(typeof(ClassMethodTargets), nameof(ClassMethodTargets.primitiveField))] int field) { }
    public static void Field_QualifiedBase([Field("FieldLookupBaseTargets.Value")] int field) { }
    public static void Field_FullyQualifiedBase([Field("Disharmony.Tests.FieldLookupBaseTargets.Value")] int field) { }
    public static void Field_QualifiedDerived([Field("FieldLookupDerivedTargets.Value")] int field) { }
    public static void Field_QualifiedStatic([Field("Disharmony.Tests.InnerStaticMethodTargets.Field")] int field) { }
    public static void Field_QualifiedUnrelatedInstance([Field("Disharmony.Tests.ClassMethodTargets.primitiveField")] int field) { }
    public static void Field_QualifiedMissing([Field("FieldLookupBaseTargets.Missing")] int field) { }
    public static void MemberInfo_Default([MemberInfo] MemberInfo member) { }
    public static void MemberInfo_Inner([MemberInfo(Scope.Inner)] MemberInfo member) { }
    public static void MemberInfo_Outer([MemberInfo(Scope.Outer)] MemberInfo member) { }
    public static void MemberInfo_Object([MemberInfo] object member) { }
    public static void MemberInfo_Ref([MemberInfo] ref MemberInfo member) { }
    public static void MemberInfo_In([MemberInfo] in MemberInfo member) { }
    public static void MemberInfo_Out([MemberInfo] out MemberInfo member) => member = null!;
    public static void MemberInfo_IncompatibleType([MemberInfo] int member) { }
    public static void Parameter_ImplicitName(int value) { }
    public static void Parameter_AttributeNullName([Argument(null)] int value) { }
    public static void Parameter_AttributeExplicitName([Argument("source", Scope.Outer)] int value) { }
    public static void Parameter_AttributeStaticIndex([Argument(1)] string value) { }
    public static void Parameter_AttributeInstanceIndex([Argument(0)] int value) { }
    public static void Parameter_InnerNamePrecedence(int value) { }
    public static void Parameter_OuterNameFallback(int outerValue) { }
    public static void Parameter_ExplicitOuterScope([Argument("value", Scope.Outer)] int value) { }
    public static void Instance_Attribute([Instance] ClassMethodTargets instance) { }
    public static void Instance_InnerScope([Instance] InnerInstanceMethodTargets instance) { }
    public static void ReturnValue_Attribute([ReturnValue] int result) { }
    public static void State_ExplicitKey([State("shared")] int state) { }
    public static void State_NullKey([State(null)] int namedState) { }
    public static void Field_InnerNamePrecedence([Field("foo")] int value) { }
    public static void Field_OuterNameFallback([Field("primitiveField")] int value) { }
    public static void Field_ExplicitOuterScope([Field("foo", Scope.Outer)] int value) { }
    public static void Field_NullName([Field] int foo) { }
    public static void Field_AutoPropertyBackingField([Field(nameof(ClassMethodTargets.AutoProperty))] int value) { }
    public static void BaseMethod_Attribute([BaseMethod] Func<int, string> method) { }
    public static void BaseMethod_ExplicitInner([BaseMethod(Scope.Inner)] Func<int, string> method) { }
    public static void BaseMethod_ExplicitOuter([BaseMethod(Scope.Outer)] Func<int, string> method) { }
    public static void Method_AttributeNonVirtual(
        [Method(nameof(MethodBindingInstanceTargets.BoundInstanceMethod))] Func<int, int> method) { }
    public static void Method_AttributeVirtual(
        [Method(nameof(MethodBindingVirtualBaseTargets.BoundVirtualMethod))] Func<int, int> method) { }
    public static void Method_VirtualMethod_NonVirtualCall(
        [Method(nameof(MethodBindingVirtualBaseTargets.BoundVirtualMethod), virtualCall: false)] Func<int, int> method) { }
    public static void Method_VirtualMethod_NullNameNonVirtualCall(
        [Method((string?)null, virtualCall: false)] Func<int, int> BoundVirtualMethod) { }
    public static void Method_NullNameUsesParameterName(
        [Method((string?)null)] Func<int, int> BoundInstanceMethod) { }
    public static void Method_ExplicitInnerScope(
        [Method(nameof(MethodBindingInnerTargets.BoundInstanceMethod), Scope.Inner)] Func<int, int> method) { }
    public static void Method_MutableStructInstance(
        [Method(nameof(MethodBindingStructTargets.BoundInstanceMethod))] Func<int, int> method) { }
    public static void Exception_Attribute([Exception] Exception exception) { }
    public static void ReservedName_Caller(ClassMethodTargets __caller) { }
    public static void ReservedName_Instance(ClassMethodTargets __instance) { }
    public static void ReservedName_Result(int __result) { }
    public static void ReservedName_State(int __state) { }
    public static void ReservedName_BaseMethod(Func<int, string> __base) { }
    public static void ReservedName_Exception(Exception __exception) { }
    public static void ReservedName_Field(int ___foo) { }
    public static void Error_MultipleBindingAttributes([Argument] [Instance] int value) { }
    public static void Error_InvalidScopeValue([Instance((Scope)99)] object value) { }
    public static void Error_InvalidInnerScope([Instance(Scope.Inner)] object value) { }
    public static void Error_CallerOutsideInnerPatch(object __caller) { }
    public static void Error_ParameterIndexOutOfRange([Argument(5)] int value) { }
    public static void Error_ParameterNotFound(int missing) { }
    public static void Error_ParameterTypeMismatch(string value) { }
    public static void Error_ReturnValueForVoid([ReturnValue] int result) { }
    public static void Error_ReturnValueForAlwaysRunPrefix(int __result) { }
    public static void Error_InstanceForStaticMethod(object __instance) { }
    public static void Error_ExceptionWithoutAlwaysRun(Exception __exception) { }
    public static void Error_UnsupportedBindingAttribute([UnsupportedParameterBinding] int value) { }
    public static void Error_PostfixOuterParameterByWritableReference(ref int value) { }
    public static void Error_InnerPostfixInnerParameterByWritableReference(ref int value) { }
    public static void Error_MethodNotFound([Method("Missing")] Func<int, int> method) { }
    public static void Error_MethodRequiresInstance(
        [Method(nameof(MethodBindingInstanceTargets.BoundInstanceMethod))] Func<int, int> method) { }
    public static void Error_MethodParameterMismatch(
        [Method(nameof(MethodBindingInstanceTargets.BoundInstanceMethod))] Func<string, int> method) { }
    public static void Error_BaseMethodForNonMethodInvocation(Func<int> __base) { }
    public static void Error_BaseMethodForStaticMethod(Func<int> __base) { }
    public static void Error_BaseMethodParameterIsNotDelegate(object __base) { }
    public static void Error_BaseMethodDelegateHasNoInvoke(Delegate __base) { }
    public static void Error_BaseMethodParameterTypeMismatch(Func<string, string> __base) { }
    public static void Error_BaseMethodParameterCountMismatch(Func<string> __base) { }
    public static void Error_BaseMethodReturnTypeMismatch(Func<int, int> __base) { }
    public static void Error_BaseMethodNotFound(Func<int> __base) { }
    public static void Error_BaseMethodIsAbstract([BaseMethod] Func<int, string> method) { }
    public static void StateMachine_ParameterByName([Argument("outerValue", Scope.Outer)] int value) { }
    public static void StateMachine_ParameterByIndex([Argument(1, Scope.Outer)] int value) { }
    public static void StateMachine_Instance([Instance(Scope.Outer)] AsyncMethodTargets instance) { }
    public static void StateMachine_Field([Field(nameof(AsyncMethodTargets.Field), Scope.Outer)] int value) { }
    public static void StateMachine_Error_InstanceByWritableReference(
        [Instance(Scope.Outer)] ref AsyncMethodTargets instance) { }
    public static void StateMachine_Error_ParameterNotFound([Argument("missing", Scope.Outer)] int value) { }
    public static void StateMachine_Error_InstanceForStaticMethod([Instance(Scope.Outer)] object instance) { }
    public static void StateMachine_Error_MethodForOuterInstance(
        [Method(nameof(AsyncMethodTargets.CallAfterAwait), Scope.Outer)] Func<Task, int, Task<int>> method) { }
    public static void StateMachine_MethodForOuterStaticMethod(
        [Method(nameof(AsyncMethodTargets.CallBeforeAndAfterAwait), Scope.Outer)] Func<Task, int, Task<int>> method) { }
    public static void StateMachine_ClosureParameter(
        [Argument("enclosingValue", Scope.Outer)] int value) { }
    public static void Error_FieldNotFound([Field("missing")] int value) { }
    public static void CapturedVariable_ByName(BindingReference captured) { }
}

[TestFixture]
internal sealed class ParameterBinderBindTests
{
    private static readonly MockInvocation StaticVoid =
        new(typeof(void), typeof(void), [], [], true);

    private static readonly MockInvocation StaticIntParameter =
        new(typeof(void), typeof(void), [typeof(int)], ["value"], true);

    private static readonly MockInvocation InstanceVoid =
        new(typeof(ClassMethodTargets), typeof(void), [typeof(ClassMethodTargets)], ["<instance>"], false);

    private static readonly MockInvocation InnerInstanceVoid =
        new(typeof(InnerInstanceMethodTargets), typeof(void), [typeof(InnerInstanceMethodTargets)], ["<instance>"], false);

    private static readonly MethodInfo AsyncTargetMethod = typeof(AsyncMethodTargets).GetMethod(
        nameof(AsyncMethodTargets.CallAfterAwait),
        [typeof(Task), typeof(int)])!;

    private static readonly MethodInvocation AsyncTarget = new(AsyncTargetMethod);
    private static readonly MethodInvocation AsyncMoveNext = new(AsyncTargetMethod.GetStateMachineImplementation()!);

    private static readonly MockInvocation InnerIntParameter =
        new(typeof(void), typeof(void), [typeof(int)], ["value"], true);

    private static ParameterInfo GetParameter(string methodName) =>
        typeof(ParameterBinderPatchMethods).GetMethod(methodName)!.GetParameters().Single();

    private static BoundParameter Bind(
        string patchMethodName,
        Invocation outer,
        Invocation? inner = null,
        Invocation? target = null,
        PatchKind patchKind = PatchKind.Prefix,
        PatchOptions options = PatchOptions.Default)
    {
        var parameter = GetParameter(patchMethodName);
        var binder = new ParameterBinder(
            target ?? outer,
            outer,
            inner ?? EmptyInvocation.Instance,
            patchKind,
            options,
            "test-group");
        return binder.Bind(parameter);
    }

    [Test]
    public void MemberInfo_Method_MethodInfoParameter_IsAccepted()
    {
        var invocation = new MethodInvocation(typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.IntIdentity))!);
        MemberInfo expected = invocation.MethodInfo;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.MemberInfo_TypedMethodInfo),
            invocation);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.MemberInfo));
        Assert.That(binding.memberInfo, Is.EqualTo(expected));
    }

    [Test]
    public void MemberInfo_Method_ConstructorInfoParameter_IsRejected()
    {
        var invocation = new MethodInvocation(typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.IntIdentity))!);

        Assert.Throws<InvalidCastException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.MemberInfo_TypedConstructorInfo),
                invocation));
    }

    [Test]
    public void MemberInfo_Method_FieldInfoParameter_IsRejected()
    {
        var invocation = new MethodInvocation(typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.IntIdentity))!);

        Assert.Throws<InvalidCastException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.MemberInfo_TypedFieldInfo),
                invocation));
    }

    [Test]
    public void MemberInfo_Method_MethodBaseParameter_IsAccepted()
    {
        var invocation = new MethodInvocation(typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.IntIdentity))!);
        MemberInfo expected = invocation.MethodInfo;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.MemberInfo_TypedMethodBase),
            invocation);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.MemberInfo));
        Assert.That(binding.memberInfo, Is.EqualTo(expected));
    }

    [Test]
    public void MemberInfo_Constructor_MethodInfoParameter_IsRejected()
    {
        var invocation = new OuterConstructorInvocation(typeof(ConstructorTargets).GetConstructor([typeof(int)])!);

        Assert.Throws<InvalidCastException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.MemberInfo_TypedMethodInfo),
                invocation));
    }

    [Test]
    public void MemberInfo_Constructor_ConstructorInfoParameter_IsAccepted()
    {
        var invocation = new OuterConstructorInvocation(typeof(ConstructorTargets).GetConstructor([typeof(int)])!);
        MemberInfo expected = invocation.ConstructorInfo;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.MemberInfo_TypedConstructorInfo),
            invocation);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.MemberInfo));
        Assert.That(binding.memberInfo, Is.EqualTo(expected));
    }

    [Test]
    public void MemberInfo_Constructor_FieldInfoParameter_IsRejected()
    {
        var invocation = new OuterConstructorInvocation(typeof(ConstructorTargets).GetConstructor([typeof(int)])!);

        Assert.Throws<InvalidCastException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.MemberInfo_TypedFieldInfo),
                invocation));
    }

    [Test]
    public void MemberInfo_Constructor_MethodBaseParameter_IsAccepted()
    {
        var invocation = new OuterConstructorInvocation(typeof(ConstructorTargets).GetConstructor([typeof(int)])!);
        MemberInfo expected = invocation.ConstructorInfo;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.MemberInfo_TypedMethodBase),
            invocation);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.MemberInfo));
        Assert.That(binding.memberInfo, Is.EqualTo(expected));
    }

    [Test]
    public void MemberInfo_InnerConstructor_MethodInfoParameter_IsRejected()
    {
        var invocation = new InnerConstructorInvocation(typeof(ConstructorTargets).GetConstructor([typeof(int)])!);

        Assert.Throws<InvalidCastException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.MemberInfo_InnerTypedMethodInfo),
                StaticIntParameter, invocation));
    }

    [Test]
    public void MemberInfo_InnerConstructor_ConstructorInfoParameter_IsAccepted()
    {
        var invocation = new InnerConstructorInvocation(typeof(ConstructorTargets).GetConstructor([typeof(int)])!);
        MemberInfo expected = invocation.ConstructorInfo;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.MemberInfo_InnerTypedConstructorInfo),
            StaticIntParameter, invocation);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.MemberInfo));
        Assert.That(binding.memberInfo, Is.EqualTo(expected));
    }

    [Test]
    public void MemberInfo_InnerConstructor_FieldInfoParameter_IsRejected()
    {
        var invocation = new InnerConstructorInvocation(typeof(ConstructorTargets).GetConstructor([typeof(int)])!);

        Assert.Throws<InvalidCastException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.MemberInfo_InnerTypedFieldInfo),
                StaticIntParameter, invocation));
    }

    [Test]
    public void MemberInfo_InnerConstructor_MethodBaseParameter_IsAccepted()
    {
        var invocation = new InnerConstructorInvocation(typeof(ConstructorTargets).GetConstructor([typeof(int)])!);
        MemberInfo expected = invocation.ConstructorInfo;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.MemberInfo_InnerTypedMethodBase),
            StaticIntParameter, invocation);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.MemberInfo));
        Assert.That(binding.memberInfo, Is.EqualTo(expected));
    }

    [Test]
    public void MemberInfo_FieldGetter_MethodInfoParameter_IsRejected()
    {
        var invocation = new GetFieldInvocation(typeof(InnerStaticMethodTargets).GetField(nameof(InnerStaticMethodTargets.Field))!);

        Assert.Throws<InvalidCastException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.MemberInfo_InnerTypedMethodInfo),
                StaticIntParameter, invocation));
    }

    [Test]
    public void MemberInfo_FieldGetter_ConstructorInfoParameter_IsRejected()
    {
        var invocation = new GetFieldInvocation(typeof(InnerStaticMethodTargets).GetField(nameof(InnerStaticMethodTargets.Field))!);

        Assert.Throws<InvalidCastException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.MemberInfo_InnerTypedConstructorInfo),
                StaticIntParameter, invocation));
    }

    [Test]
    public void MemberInfo_FieldGetter_FieldInfoParameter_IsAccepted()
    {
        var invocation = new GetFieldInvocation(typeof(InnerStaticMethodTargets).GetField(nameof(InnerStaticMethodTargets.Field))!);
        MemberInfo expected = invocation.FieldInfo;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.MemberInfo_InnerTypedFieldInfo),
            StaticIntParameter, invocation);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.MemberInfo));
        Assert.That(binding.memberInfo, Is.EqualTo(expected));
    }

    [Test]
    public void MemberInfo_FieldGetter_MethodBaseParameter_IsRejected()
    {
        var invocation = new GetFieldInvocation(typeof(InnerStaticMethodTargets).GetField(nameof(InnerStaticMethodTargets.Field))!);

        Assert.Throws<InvalidCastException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.MemberInfo_InnerTypedMethodBase),
                StaticIntParameter, invocation));
    }

    [Test]
    public void MemberInfo_FieldSetter_MethodInfoParameter_IsRejected()
    {
        var invocation = new SetFieldInvocation(typeof(InnerStaticMethodTargets).GetField(nameof(InnerStaticMethodTargets.Field))!);

        Assert.Throws<InvalidCastException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.MemberInfo_InnerTypedMethodInfo),
                StaticIntParameter, invocation));
    }

    [Test]
    public void MemberInfo_FieldSetter_ConstructorInfoParameter_IsRejected()
    {
        var invocation = new SetFieldInvocation(typeof(InnerStaticMethodTargets).GetField(nameof(InnerStaticMethodTargets.Field))!);

        Assert.Throws<InvalidCastException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.MemberInfo_InnerTypedConstructorInfo),
                StaticIntParameter, invocation));
    }

    [Test]
    public void MemberInfo_FieldSetter_FieldInfoParameter_IsAccepted()
    {
        var invocation = new SetFieldInvocation(typeof(InnerStaticMethodTargets).GetField(nameof(InnerStaticMethodTargets.Field))!);
        MemberInfo expected = invocation.FieldInfo;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.MemberInfo_InnerTypedFieldInfo),
            StaticIntParameter, invocation);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.MemberInfo));
        Assert.That(binding.memberInfo, Is.EqualTo(expected));
    }

    [Test]
    public void MemberInfo_FieldSetter_MethodBaseParameter_IsRejected()
    {
        var invocation = new SetFieldInvocation(typeof(InnerStaticMethodTargets).GetField(nameof(InnerStaticMethodTargets.Field))!);

        Assert.Throws<InvalidCastException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.MemberInfo_InnerTypedMethodBase),
                StaticIntParameter, invocation));
    }

    [Test]
    public void MemberInfo_Method_SelectsTargetMember()
    {
        var expected = typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.IntIdentity))!;
        var invocation = new MethodInvocation(expected);

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.MemberInfo_Default),
            invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.MemberInfo));
            Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
            Assert.That(binding.memberInfo, Is.EqualTo(expected));
        });
    }

    [Test]
    public void MemberInfo_Constructor_SelectsTargetMember()
    {
        var expected = typeof(ConstructorTargets).GetConstructor([typeof(int)])!;
        var invocation = new OuterConstructorInvocation(expected);

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.MemberInfo_Default),
            invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.MemberInfo));
            Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
            Assert.That(binding.memberInfo, Is.EqualTo(expected));
        });
    }

    [Test]
    public void MemberInfo_InnerMethod_SelectsTargetMember()
    {
        var expected = typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.IntIdentity))!;
        var invocation = new MethodInvocation(expected);

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.MemberInfo_Inner),
            InstanceVoid, invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.MemberInfo));
            Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
            Assert.That(binding.memberInfo, Is.EqualTo(expected));
        });
    }

    [Test]
    public void MemberInfo_InnerConstructor_SelectsTargetMember()
    {
        var expected = typeof(ConstructorTargets).GetConstructor([typeof(int)])!;
        var invocation = new InnerConstructorInvocation(expected);

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.MemberInfo_Inner),
            InstanceVoid, invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.MemberInfo));
            Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
            Assert.That(binding.memberInfo, Is.EqualTo(expected));
        });
    }

    [Test]
    public void MemberInfo_FieldGetter_SelectsTargetMember()
    {
        var expected = typeof(InnerStaticMethodTargets).GetField(nameof(InnerStaticMethodTargets.Field))!;
        var invocation = new GetFieldInvocation(expected);

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.MemberInfo_Inner),
            InstanceVoid, invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.MemberInfo));
            Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
            Assert.That(binding.memberInfo, Is.EqualTo(expected));
        });
    }

    [Test]
    public void MemberInfo_FieldSetter_SelectsTargetMember()
    {
        var expected = typeof(InnerStaticMethodTargets).GetField(nameof(InnerStaticMethodTargets.Field))!;
        var invocation = new SetFieldInvocation(expected);

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.MemberInfo_Inner),
            InstanceVoid, invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.MemberInfo));
            Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
            Assert.That(binding.memberInfo, Is.EqualTo(expected));
        });
    }

    [Test]
    public void MemberInfo_DefaultScope_OnInnerPatch_SelectsOuterMethod()
    {
        var expected = typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.IntIdentity))!;
        var outer = new MethodInvocation(expected);
        var inner = new GetFieldInvocation(typeof(InnerStaticMethodTargets).GetField(nameof(InnerStaticMethodTargets.Field))!);

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.MemberInfo_Default), outer, inner);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.MemberInfo));
            Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
            Assert.That(binding.memberInfo, Is.EqualTo(expected));
        });
    }

    [Test]
    public void MemberInfo_OuterScope_OnInnerPatch_SelectsOuterMethod()
    {
        var expected = typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.IntIdentity))!;
        var outer = new MethodInvocation(expected);
        var inner = new GetFieldInvocation(typeof(InnerStaticMethodTargets).GetField(nameof(InnerStaticMethodTargets.Field))!);

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.MemberInfo_Outer), outer, inner);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.MemberInfo));
            Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
            Assert.That(binding.memberInfo, Is.EqualTo(expected));
        });
    }

    [Test]
    public void MemberInfo_ObjectParameter_AcceptsMemberInfo()
    {
        var expected = typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.IntIdentity))!;
        var outer = new MethodInvocation(expected);

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.MemberInfo_Object), outer);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.MemberInfo));
        Assert.That(binding.memberInfo, Is.EqualTo(expected));
    }

    [Test]
    public void MemberInfo_StateMachine_OuterScope_SelectsDeclaredMethod()
    {
        MethodInfo expected = typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.EnumerateIdentity))!;
        var target = new MethodInvocation(expected);
        var moveNext = new MethodInvocation(expected.GetStateMachineImplementation()!);

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.MemberInfo_Default),
            moveNext, InnerIntParameter, target);

        Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
        Assert.That(binding.memberInfo, Is.EqualTo(expected));
    }

    [Test]
    public void MemberInfo_RefParameter_IsRejected()
    {
        var invocation = new MethodInvocation(
            typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.IntIdentity))!);

        Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.MemberInfo_Ref), invocation));
    }

    [Test]
    public void MemberInfo_InParameter_IsRejected()
    {
        var invocation = new MethodInvocation(
            typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.IntIdentity))!);

        Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.MemberInfo_In), invocation));
    }

    [Test]
    public void MemberInfo_OutParameter_IsRejected()
    {
        var invocation = new MethodInvocation(
            typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.IntIdentity))!);

        Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.MemberInfo_Out), invocation));
    }

    [Test]
    public void MemberInfo_IncompatibleTypeParameter_IsRejected()
    {
        var invocation = new MethodInvocation(
            typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.IntIdentity))!);

        Assert.Throws<InvalidCastException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.MemberInfo_IncompatibleType), invocation));
    }

    [Test]
    public void MemberInfo_InnerConstant_IsRejected()
    {
        Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.MemberInfo_Inner), StaticIntParameter,
                new ConstantIntInvocation(42)));
    }

    [Test]
    public void MemberInfo_InnerScopeWithoutInnerTarget_IsRejected()
    {
        Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.MemberInfo_Inner), StaticIntParameter));
    }

    [Test]
    public void Field_ExplicitType_NullName_InnerScope_SelectsSpecifiedType()
    {
        var invocation = new MethodInvocation(typeof(FieldLookupDerivedTargets).GetMethod(nameof(FieldLookupDerivedTargets.Target))!);
        var expected = typeof(FieldLookupBaseTargets).GetField(nameof(FieldLookupBaseTargets.Value))!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Field_ExplicitType_NullName_InnerScope), invocation, invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Instance));
            Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
            Assert.That(binding.fields, Is.EqualTo(new[] { expected }));
        });
    }

    [Test]
    public void Method_ExplicitType_NullName_InnerScope_SelectsSpecifiedType()
    {
        var invocation = new MethodInvocation(typeof(MethodBindingVirtualDerivedTargets).GetMethod(nameof(MethodBindingVirtualDerivedTargets.TargetDerivedInstanceMethod))!);
        var expected = typeof(MethodBindingVirtualBaseTargets).GetMethod(nameof(MethodBindingVirtualBaseTargets.Describe))!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Method_ExplicitType_NullName_InnerScope), invocation, invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
            Assert.That(binding.memberInfo, Is.EqualTo(expected));
            Assert.That(binding.useVirtualDispatch, Is.True);
        });
    }

    [Test]
    public void Field_ExplicitType_NullName_OuterScope_SelectsSpecifiedType()
    {
        var invocation = new MethodInvocation(typeof(FieldLookupDerivedTargets).GetMethod(nameof(FieldLookupDerivedTargets.Target))!);
        var expected = typeof(FieldLookupBaseTargets).GetField(nameof(FieldLookupBaseTargets.Value))!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Field_ExplicitType_NullName_OuterScope), invocation, invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Instance));
            Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
            Assert.That(binding.fields, Is.EqualTo(new[] { expected }));
        });
    }

    [Test]
    public void Method_ExplicitType_NullName_OuterScope_SelectsSpecifiedType()
    {
        var invocation = new MethodInvocation(typeof(MethodBindingVirtualDerivedTargets).GetMethod(nameof(MethodBindingVirtualDerivedTargets.TargetDerivedInstanceMethod))!);
        var expected = typeof(MethodBindingVirtualBaseTargets).GetMethod(nameof(MethodBindingVirtualBaseTargets.Describe))!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Method_ExplicitType_NullName_OuterScope), invocation, invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
            Assert.That(binding.memberInfo, Is.EqualTo(expected));
            Assert.That(binding.useVirtualDispatch, Is.True);
        });
    }

    [Test]
    public void Method_ExplicitType_SelectsOverloadUsingDelegateSignature()
    {
        var invocation = new MethodInvocation(typeof(MethodLookupTarget).GetMethod(nameof(MethodLookupTarget.Target))!);
        var expected = typeof(MethodLookupTarget).GetMethod(nameof(MethodLookupTarget.Convert), [typeof(string)])!;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Method_ExplicitType_Overload), invocation);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
        Assert.That(binding.memberInfo, Is.EqualTo(expected));
    }

    [Test]
    public void Method_ExplicitType_IncompatibleInstance_IsRejected()
    {
        var invocation = new MethodInvocation(
            typeof(FieldLookupDerivedTargets).GetMethod(nameof(FieldLookupDerivedTargets.Target))!);

        Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Method_ExplicitType_IncompatibleInstance), invocation));
    }

    [Test]
    public void Field_ExplicitType_IncompatibleInstance_IsRejected()
    {
        var invocation = new MethodInvocation(
            typeof(FieldLookupDerivedTargets).GetMethod(nameof(FieldLookupDerivedTargets.Target))!);

        Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Field_ExplicitType_IncompatibleInstance), invocation));
    }

    [Test]
    public void Arguments_Default_OuterPatch_SelectsOuterArguments()
    {
        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Arguments_Default),
            StaticIntParameter);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.ArgumentArray));
        Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
    }

    [Test]
    public void Arguments_ReservedName_OuterPatch_SelectsOuterArguments()
    {
        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Arguments_ReservedName),
            StaticIntParameter);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.ArgumentArray));
        Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
    }

    [Test]
    public void Arguments_Default_InnerPatch_SelectsInnerArguments()
    {
        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Arguments_Default),
            StaticIntParameter, InnerIntParameter);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.ArgumentArray));
        Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
    }

    [Test]
    public void Arguments_ReservedName_InnerPatch_SelectsInnerArguments()
    {
        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Arguments_ReservedName),
            StaticIntParameter, InnerIntParameter);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.ArgumentArray));
        Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
    }

    [Test]
    public void Arguments_Outer_InnerPatch_SelectsOuterArguments()
    {
        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Arguments_Outer),
            StaticIntParameter, InnerIntParameter);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.ArgumentArray));
        Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
    }

    [Test]
    public void Arguments_Inner_InnerPatch_SelectsInnerArguments()
    {
        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Arguments_Inner),
            StaticIntParameter, InnerIntParameter);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.ArgumentArray));
        Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
    }

    [Test]
    public void Arguments_RefParameter_IsRejected()
    {
        Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Arguments_Ref), StaticIntParameter));
    }

    [Test]
    public void Arguments_InParameter_IsRejected()
    {
        Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Arguments_In), StaticIntParameter));
    }

    [Test]
    public void Arguments_OutParameter_IsRejected()
    {
        Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Arguments_Out), StaticIntParameter));
    }

    [Test]
    public void Arguments_StringArrayParameter_IsRejected()
    {
        Assert.Throws<InvalidCastException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Arguments_StringArray), StaticIntParameter));
    }

    [Test]
    public void Arguments_ValueArrayParameter_IsRejected()
    {
        Assert.Throws<InvalidCastException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Arguments_ValueArray), StaticIntParameter));
    }

    [Test]
    public void Method_PropertyGetter_Named_SelectsAccessor()
    {
        var outer = new MethodInvocation(typeof(MethodBindingInstanceTargets)
            .GetMethod(nameof(MethodBindingInstanceTargets.TargetInstanceMethod))!);
        var expected = typeof(MethodBindingInstanceTargets).GetProperty(nameof(MethodBindingInstanceTargets.InstanceValue))!.GetMethod;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Method_PropertyGetter_Named), outer);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
            Assert.That(binding.memberInfo, Is.EqualTo(expected));
        });
    }

    [Test]
    public void Method_PropertySetter_Named_SelectsAccessor()
    {
        var outer = new MethodInvocation(typeof(MethodBindingInstanceTargets)
            .GetMethod(nameof(MethodBindingInstanceTargets.TargetInstanceMethod))!);
        var expected = typeof(MethodBindingInstanceTargets).GetProperty(nameof(MethodBindingInstanceTargets.InstanceValue))!.SetMethod;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Method_PropertySetter_Named), outer);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
            Assert.That(binding.memberInfo, Is.EqualTo(expected));
        });
    }

    [Test]
    public void Method_PropertyGetter_ExplicitTypeNullName_SelectsAccessor()
    {
        var outer = new MethodInvocation(typeof(MethodBindingInstanceTargets)
            .GetMethod(nameof(MethodBindingInstanceTargets.TargetInstanceMethod))!);
        var expected = typeof(MethodBindingInstanceTargets).GetProperty(nameof(MethodBindingInstanceTargets.InstanceValue))!.GetMethod;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Method_PropertyGetter_ExplicitTypeNullName), outer);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
            Assert.That(binding.memberInfo, Is.EqualTo(expected));
        });
    }

    [Test]
    public void Method_PropertySetter_ExplicitTypeNullName_SelectsAccessor()
    {
        var outer = new MethodInvocation(typeof(MethodBindingInstanceTargets)
            .GetMethod(nameof(MethodBindingInstanceTargets.TargetInstanceMethod))!);
        var expected = typeof(MethodBindingInstanceTargets).GetProperty(nameof(MethodBindingInstanceTargets.InstanceValue))!.SetMethod;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Method_PropertySetter_ExplicitTypeNullName), outer);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
            Assert.That(binding.memberInfo, Is.EqualTo(expected));
        });
    }

    [Test]
    public void Method_PropertyGetter_ImplicitName_SelectsAccessor()
    {
        var outer = new MethodInvocation(typeof(MethodBindingInstanceTargets)
            .GetMethod(nameof(MethodBindingInstanceTargets.TargetInstanceMethod))!);
        var inner = new MethodInvocation(typeof(MethodBindingInnerTargets)
            .GetMethod(nameof(MethodBindingInnerTargets.TargetInstanceMethod))!);
        var expected = typeof(MethodBindingInnerTargets).GetProperty(nameof(MethodBindingInnerTargets.InstanceValue))!.GetMethod;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Method_PropertyGetter_ImplicitName), outer, inner);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
            Assert.That(binding.memberInfo, Is.EqualTo(expected));
        });
    }

    [Test]
    public void Method_PropertySetter_ImplicitName_SelectsAccessor()
    {
        var outer = new MethodInvocation(typeof(MethodBindingInstanceTargets)
            .GetMethod(nameof(MethodBindingInstanceTargets.TargetInstanceMethod))!);
        var inner = new MethodInvocation(typeof(MethodBindingInnerTargets)
            .GetMethod(nameof(MethodBindingInnerTargets.TargetInstanceMethod))!);
        var expected = typeof(MethodBindingInnerTargets).GetProperty(nameof(MethodBindingInnerTargets.InstanceValue))!.SetMethod;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Method_PropertySetter_ImplicitName), outer, inner);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
            Assert.That(binding.memberInfo, Is.EqualTo(expected));
        });
    }

    [Test]
    public void Parameter_ImplicitName_BindsOuterParameter()
    {
        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Parameter_ImplicitName), StaticIntParameter);

        Assert.Multiple(() =>
        {
            Assert.That(binding.parameter, Is.SameAs(GetParameter(nameof(ParameterBinderPatchMethods.Parameter_ImplicitName))));
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Argument));
            Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
            Assert.That(binding.index, Is.Zero);
        });
    }

    [Test]
    public void Parameter_AttributeNullName_UsesPatchParameterName()
    {
        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Parameter_AttributeNullName), StaticIntParameter);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Argument));
            Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
            Assert.That(binding.index, Is.Zero);
        });
    }

    [Test]
    public void Parameter_AttributeExplicitName_BindsNamedParameter()
    {
        var outer = new MockInvocation(typeof(void), typeof(void), [typeof(int)], ["source"], true);

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Parameter_AttributeExplicitName), outer);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Argument));
        Assert.That(binding.index, Is.Zero);
    }

    [Test]
    public void Parameter_AttributeStaticIndex_BindsRequestedIndex()
    {
        var outer = new MockInvocation(typeof(void), typeof(void), [typeof(int), typeof(string)], ["first", "second"], true);

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Parameter_AttributeStaticIndex), outer);

        Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
        Assert.That(binding.index, Is.EqualTo(1));
    }

    [Test]
    public void Parameter_AttributeInstanceIndex_SkipsInstanceArgument()
    {
        var outer = new MockInvocation(typeof(ClassMethodTargets), typeof(void), [typeof(ClassMethodTargets), typeof(int)], ["<instance>", "value"], false);

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Parameter_AttributeInstanceIndex), outer);

        Assert.That(binding.index, Is.EqualTo(1));
    }

    [Test]
    public void Parameter_InnerNamePrecedence_BindsInnerParameter()
    {
        var outer = new MockInvocation(typeof(void), typeof(void), [typeof(int)], ["value"], true);

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Parameter_InnerNamePrecedence), outer, InnerIntParameter);

        Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
        Assert.That(binding.index, Is.Zero);
    }

    [Test]
    public void Parameter_OuterNameFallback_BindsOuterParameter()
    {
        var outer = new MockInvocation(typeof(void), typeof(void), [typeof(int)], ["outerValue"], true);

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Parameter_OuterNameFallback), outer, InnerIntParameter);

        Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
        Assert.That(binding.index, Is.Zero);
    }

    [Test]
    public void Parameter_ExplicitOuterScope_OverridesInnerNameMatch()
    {
        var outer = new MockInvocation(typeof(void), typeof(void), [typeof(int)], ["value"], true);

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Parameter_ExplicitOuterScope), outer, InnerIntParameter);

        Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
        Assert.That(binding.index, Is.Zero);
    }

    [Test]
    public void Instance_Attribute_BindsOuterInstance()
    {
        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Instance_Attribute), InstanceVoid);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Instance));
        Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
    }

    [Test]
    public void Instance_InnerScope_DefaultsToInnerInstance()
    {
        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Instance_InnerScope), InstanceVoid, InnerInstanceVoid);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Instance));
        Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
    }

    [Test]
    public void ReturnValue_Attribute_BindsInnerResultForInnerPatch()
    {
        var inner = new MockInvocation(typeof(void), typeof(int), [], [], true);

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.ReturnValue_Attribute), StaticVoid, inner);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Result));
        Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
    }

    [Test]
    public void State_ExplicitKey_IncludesRegistrationGroup()
    {
        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.State_ExplicitKey), StaticVoid);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.State));
        Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
        Assert.That(binding.stateKey, Is.EqualTo("test-group#shared"));
    }

    [Test]
    public void State_NullKey_UsesPatchParameterName()
    {
        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.State_NullKey), StaticVoid);

        Assert.That(binding.stateKey, Is.EqualTo("test-group#namedState"));
    }

    [Test]
    public void Field_QualifiedBase_SelectsBaseRatherThanHiddenField()
    {
        var invocation = new MethodInvocation(
            typeof(FieldLookupDerivedTargets).GetMethod(nameof(FieldLookupDerivedTargets.Target))!);
        var expected = typeof(FieldLookupBaseTargets).GetField(nameof(FieldLookupBaseTargets.Value))!;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Field_QualifiedBase), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Instance));
            Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
            Assert.That(binding.fields, Is.EqualTo(new[] { expected }));
        });
    }

    [Test]
    public void Field_FullyQualifiedBase_SelectsBaseRatherThanHiddenField()
    {
        var invocation = new MethodInvocation(
            typeof(FieldLookupDerivedTargets).GetMethod(nameof(FieldLookupDerivedTargets.Target))!);
        var expected = typeof(FieldLookupBaseTargets).GetField(nameof(FieldLookupBaseTargets.Value))!;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Field_FullyQualifiedBase), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Instance));
            Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
            Assert.That(binding.fields, Is.EqualTo(new[] { expected }));
        });
    }

    [Test]
    public void Field_QualifiedStatic_OnStaticTarget_BindsUnrelatedField()
    {
        var invocation = new MethodInvocation(typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.Void))!);
        var expected = typeof(InnerStaticMethodTargets).GetField(nameof(InnerStaticMethodTargets.Field))!;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Field_QualifiedStatic), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.StaticField));
            Assert.That(binding.fields, Is.EqualTo(new[] { expected }));
        });
    }

    [Test]
    public void Field_QualifiedStatic_OnInstanceTarget_BindsUnrelatedField()
    {
        var invocation = new MethodInvocation(typeof(FieldLookupDerivedTargets).GetMethod(nameof(FieldLookupDerivedTargets.Target))!);
        var expected = typeof(InnerStaticMethodTargets).GetField(nameof(InnerStaticMethodTargets.Field))!;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Field_QualifiedStatic), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.StaticField));
            Assert.That(binding.fields, Is.EqualTo(new[] { expected }));
        });
    }

    [Test]
    public void Field_UnrelatedInstance_IsRejected()
    {
        var invocation = new MethodInvocation(typeof(FieldLookupDerivedTargets).GetMethod(nameof(FieldLookupDerivedTargets.Target))!);

        Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Field_QualifiedUnrelatedInstance), invocation));
    }

    [Test]
    public void Field_DerivedFieldOnBaseInstance_IsRejected()
    {
        var invocation = new MockInvocation(typeof(FieldLookupBaseTargets), typeof(void), [typeof(FieldLookupBaseTargets)], ["<instance>"], false);

        Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Field_QualifiedDerived), invocation));
    }

    [Test]
    public void Field_InstanceFieldOnStaticTarget_IsRejected()
    {
        var invocation = new MethodInvocation(typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.Void))!);

        Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Field_QualifiedBase), invocation));
    }

    [Test]
    public void Field_MissingQualifiedField_IsRejected()
    {
        var invocation = new MethodInvocation(typeof(FieldLookupDerivedTargets).GetMethod(nameof(FieldLookupDerivedTargets.Target))!);

        Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Field_QualifiedMissing), invocation));
    }

    [Test]
    public void Field_InnerNamePrecedence_BindsInnerField()
    {
        FieldInfo expected = typeof(InnerInstanceMethodTargets).GetField(nameof(InnerInstanceMethodTargets.foo))!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Field_InnerNamePrecedence), InstanceVoid, InnerInstanceVoid);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Instance));
        Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
        Assert.That(binding.fields, Is.EqualTo(new[] { expected }));
    }

    [Test]
    public void Field_OuterNameFallback_BindsOuterField()
    {
        FieldInfo expected = typeof(ClassMethodTargets).GetField(nameof(ClassMethodTargets.primitiveField))!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Field_OuterNameFallback), InstanceVoid, InnerInstanceVoid);

        Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
        Assert.That(binding.fields, Is.EqualTo(new[] { expected }));
    }

    [Test]
    public void Field_ExplicitOuterScope_OverridesInnerField()
    {
        FieldInfo expected = typeof(ClassMethodTargets).GetField(nameof(ClassMethodTargets.foo))!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Field_ExplicitOuterScope), InstanceVoid, InnerInstanceVoid);

        Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
        Assert.That(binding.fields, Is.EqualTo(new[] { expected }));
    }

    [Test]
    public void Field_NullName_UsesPatchParameterName()
    {
        FieldInfo expected = typeof(ClassMethodTargets).GetField(nameof(ClassMethodTargets.foo))!;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Field_NullName), InstanceVoid);

        Assert.That(binding.fields, Is.EqualTo(new[] { expected }));
    }

    [Test]
    public void Field_AutoPropertyBackingField_BindsGeneratedField()
    {
        FieldInfo expected = typeof(ClassMethodTargets).GetField("<AutoProperty>k__BackingField", AccessTools.all)!;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.Field_AutoPropertyBackingField), InstanceVoid);

        Assert.That(binding.fields, Is.EqualTo(new[] { expected }));
    }

    [Test]
    public void BaseMethod_InnerPatch_DefaultScope_SelectsInnerBaseImplementation()
    {
        var outer = new MethodInvocation(typeof(DerivedMethodTargets).GetMethod(nameof(DerivedMethodTargets.DescribeWithInnerCall))!);
        var inner = new MethodInvocation(typeof(DerivedMethodTargets).GetMethod(nameof(DerivedMethodTargets.Describe))!);
        MethodInfo expected = typeof(BaseMethodTargets).GetMethod(nameof(BaseMethodTargets.Describe))!;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.BaseMethod_Attribute), outer, inner);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
        Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
        Assert.That(binding.memberInfo, Is.EqualTo(expected));
    }

    [Test]
    public void BaseMethod_InnerPatch_ExplicitInnerScope_SelectsInnerBaseImplementation()
    {
        var outer = new MethodInvocation(typeof(DerivedMethodTargets).GetMethod(nameof(DerivedMethodTargets.DescribeWithInnerCall))!);
        var inner = new MethodInvocation(typeof(DerivedMethodTargets).GetMethod(nameof(DerivedMethodTargets.Describe))!);
        MethodInfo expected = typeof(BaseMethodTargets).GetMethod(nameof(BaseMethodTargets.Describe))!;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.BaseMethod_ExplicitInner), outer, inner);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
        Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
        Assert.That(binding.memberInfo, Is.EqualTo(expected));
    }

    [Test]
    public void BaseMethod_InnerPatch_ExplicitOuterScope_SelectsOuterBaseImplementation()
    {
        var outer = new MethodInvocation(typeof(DerivedMethodTargets).GetMethod(nameof(DerivedMethodTargets.DescribeWithInnerCall))!);
        var inner = new MethodInvocation(typeof(DerivedMethodTargets).GetMethod(nameof(DerivedMethodTargets.Describe))!);
        MethodInfo expected = typeof(BaseMethodTargets).GetMethod(nameof(BaseMethodTargets.DescribeWithInnerCall))!;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.BaseMethod_ExplicitOuter), outer, inner);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
        Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
        Assert.That(binding.memberInfo, Is.EqualTo(expected));
    }

    [Test]
    public void ReservedName_BaseMethod_InnerPatch_SelectsInnerBaseImplementation()
    {
        var outer = new MethodInvocation(typeof(DerivedMethodTargets).GetMethod(nameof(DerivedMethodTargets.DescribeWithInnerCall))!);
        var inner = new MethodInvocation(typeof(DerivedMethodTargets).GetMethod(nameof(DerivedMethodTargets.Describe))!);
        MethodInfo expected = typeof(BaseMethodTargets).GetMethod(nameof(BaseMethodTargets.Describe))!;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.ReservedName_BaseMethod), outer, inner);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
        Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
        Assert.That(binding.memberInfo, Is.EqualTo(expected));
    }

    [Test]
    public void BaseMethod_InnerScope_Iterator_SelectsInnerBaseImplementation()
    {
        var target = new MethodInvocation(typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.EnumerateDescription))!);
        var outer = new MethodInvocation(target.MethodInfo.GetStateMachineImplementation()!);
        var inner = new MethodInvocation(typeof(DerivedMethodTargets).GetMethod(nameof(DerivedMethodTargets.DescribeNonVirtual))!);
        MethodInfo expected = typeof(BaseMethodTargets).GetMethod(nameof(BaseMethodTargets.DescribeNonVirtual))!;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.BaseMethod_ExplicitInner), outer, inner, target);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
        Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
        Assert.That(binding.memberInfo, Is.EqualTo(expected));
    }

    [Test]
    public void BaseMethod_ExplicitInnerScope_WithoutInnerPatch_IsRejected()
    {
        var outer = new MethodInvocation(typeof(DerivedMethodTargets).GetMethod(nameof(DerivedMethodTargets.Describe))!);

        Assert.Throws<ParameterBindingException>(() => Bind(
            nameof(ParameterBinderPatchMethods.BaseMethod_ExplicitInner), outer));
    }

    [Test]
    public void BaseMethod_InnerPatch_StaticInner_DoesNotFallBackToOuterBase()
    {
        var outer = new MethodInvocation(typeof(DerivedMethodTargets).GetMethod(nameof(DerivedMethodTargets.Describe))!);
        var inner = new MethodInvocation(typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.StringIdentity))!);

        Assert.Throws<ParameterBindingException>(() => Bind(
            nameof(ParameterBinderPatchMethods.BaseMethod_Attribute), outer, inner));
    }

    [Test]
    public void BaseMethod_Attribute_SelectsBaseImplementation()
    {
        MethodInfo targetMethod = typeof(DerivedMethodTargets).GetMethod(nameof(DerivedMethodTargets.Describe))!;
        MethodInfo expected = typeof(BaseMethodTargets).GetMethod(nameof(BaseMethodTargets.Describe))!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.BaseMethod_Attribute), new MethodInvocation(targetMethod));

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
        Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
        Assert.That(binding.memberInfo, Is.SameAs(expected));
    }

    [Test]
    public void ReservedName_BaseMethod_SelectsInheritedGrandparentImplementation()
    {
        MethodInfo targetMethod = typeof(BaseMethodGrandchildTargets)
            .GetMethod(nameof(BaseMethodGrandchildTargets.Describe))!;
        MethodInfo expected = typeof(BaseMethodGrandparentTargets)
            .GetMethod(nameof(BaseMethodGrandparentTargets.Describe))!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.ReservedName_BaseMethod), new MethodInvocation(targetMethod));

        Assert.Multiple(() =>
        {
            Assert.That(((MethodBase)binding.memberInfo!).MethodHandle, Is.EqualTo(expected.MethodHandle));
            Assert.That(binding.memberInfo!.DeclaringType, Is.EqualTo(typeof(BaseMethodGrandparentTargets)));
        });
    }

    [Test]
    public void Method_AttributeNonVirtual_SelectsNamedMethod()
    {
        MethodInfo targetMethod = typeof(MethodBindingInstanceTargets)
            .GetMethod(nameof(MethodBindingInstanceTargets.TargetInstanceMethod))!;
        MethodInfo expected = typeof(MethodBindingInstanceTargets)
            .GetMethod(nameof(MethodBindingInstanceTargets.BoundInstanceMethod))!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Method_AttributeNonVirtual), new MethodInvocation(targetMethod));

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
        Assert.That(binding.memberInfo, Is.SameAs(expected));
        Assert.That(binding.useVirtualDispatch, Is.False);
    }

    [Test]
    public void Method_AttributeVirtual_RecordsVirtualDispatch()
    {
        MethodInfo targetMethod = typeof(MethodBindingVirtualBaseTargets)
            .GetMethod(nameof(MethodBindingVirtualBaseTargets.TargetInstanceMethod))!;
        MethodInfo expected = typeof(MethodBindingVirtualBaseTargets)
            .GetMethod(nameof(MethodBindingVirtualBaseTargets.BoundVirtualMethod))!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Method_AttributeVirtual), new MethodInvocation(targetMethod));

        Assert.That(binding.memberInfo, Is.SameAs(expected));
        Assert.That(binding.useVirtualDispatch, Is.True);
    }

    [Test]
    public void Method_VirtualMethod_NonVirtualCall_DisablesVirtualDispatch()
    {
        MethodInfo targetMethod = typeof(MethodBindingVirtualBaseTargets)
            .GetMethod(nameof(MethodBindingVirtualBaseTargets.TargetInstanceMethod))!;
        MethodInfo expected = typeof(MethodBindingVirtualBaseTargets)
            .GetMethod(nameof(MethodBindingVirtualBaseTargets.BoundVirtualMethod))!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Method_VirtualMethod_NonVirtualCall), new MethodInvocation(targetMethod));

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
            Assert.That(binding.memberInfo, Is.SameAs(expected));
            Assert.That(binding.useVirtualDispatch, Is.False);
        });
    }

    [Test]
    public void Method_VirtualMethod_NullNameNonVirtualCall_DisablesVirtualDispatch()
    {
        MethodInfo targetMethod = typeof(MethodBindingVirtualBaseTargets)
            .GetMethod(nameof(MethodBindingVirtualBaseTargets.TargetInstanceMethod))!;
        MethodInfo expected = typeof(MethodBindingVirtualBaseTargets)
            .GetMethod(nameof(MethodBindingVirtualBaseTargets.BoundVirtualMethod))!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Method_VirtualMethod_NullNameNonVirtualCall), new MethodInvocation(targetMethod));

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
            Assert.That(binding.memberInfo, Is.SameAs(expected));
            Assert.That(binding.useVirtualDispatch, Is.False);
        });
    }

    [Test]
    public void Method_NullNameUsesParameterName_SelectsMethod()
    {
        MethodInfo targetMethod = typeof(MethodBindingInstanceTargets)
            .GetMethod(nameof(MethodBindingInstanceTargets.TargetInstanceMethod))!;
        MethodInfo expected = typeof(MethodBindingInstanceTargets)
            .GetMethod(nameof(MethodBindingInstanceTargets.BoundInstanceMethod))!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Method_NullNameUsesParameterName), new MethodInvocation(targetMethod));

        Assert.That(binding.memberInfo, Is.SameAs(expected));
    }

    [Test]
    public void Method_ExplicitInnerScope_SelectsMethodOnInnerInstance()
    {
        var outer = new MockInvocation(typeof(void), typeof(void), [], [], true);
        MethodInfo innerMethod = typeof(MethodBindingInnerTargets)
            .GetMethod(nameof(MethodBindingInnerTargets.TargetInstanceMethod))!;
        MethodInfo expected = typeof(MethodBindingInnerTargets)
            .GetMethod(nameof(MethodBindingInnerTargets.BoundInstanceMethod))!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Method_ExplicitInnerScope), outer, new MethodInvocation(innerMethod));

        Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
        Assert.That(binding.memberInfo, Is.SameAs(expected));
    }

    [Test]
    public void Exception_Attribute_BindsForAlwaysRunPostfix()
    {
        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Exception_Attribute),
            StaticVoid,
            patchKind: PatchKind.Postfix,
            options: PatchOptions.AlwaysRun);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Exception));
        Assert.That(binding.scope, Is.EqualTo(Scope.Any));
    }

    [Test]
    public void ReservedName_Caller_BindsOuterInstance()
    {
        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.ReservedName_Caller), InstanceVoid, InnerInstanceVoid);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Instance));
        Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
    }

    [Test]
    public void ReservedName_Instance_BindsDefaultInstance()
    {
        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.ReservedName_Instance), InstanceVoid);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Instance));
        Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
    }

    [Test]
    public void ReservedName_Result_BindsResult()
    {
        var outer = new MockInvocation(typeof(void), typeof(int), [], [], true);

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.ReservedName_Result), outer);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Result));
    }

    [Test]
    public void ReservedName_State_UsesReservedNameAsKey()
    {
        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.ReservedName_State), StaticVoid);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.State));
        Assert.That(binding.stateKey, Is.EqualTo("test-group#__state"));
    }

    [Test]
    public void ReservedName_BaseMethod_SelectsBaseImplementation()
    {
        MethodInfo targetMethod = typeof(DerivedMethodTargets).GetMethod(nameof(DerivedMethodTargets.Describe))!;
        MethodInfo expected = typeof(BaseMethodTargets).GetMethod(nameof(BaseMethodTargets.Describe))!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.ReservedName_BaseMethod), new MethodInvocation(targetMethod));

        Assert.That(binding.memberInfo, Is.SameAs(expected));
    }

    [Test]
    public void ReservedName_Exception_BindsException()
    {
        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.ReservedName_Exception),
            StaticVoid,
            patchKind: PatchKind.Postfix,
            options: PatchOptions.AlwaysRun);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Exception));
    }

    [Test]
    public void ReservedName_Field_BindsFieldAfterTripleUnderscore()
    {
        FieldInfo expected = typeof(ClassMethodTargets).GetField(nameof(ClassMethodTargets.foo))!;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.ReservedName_Field), InstanceVoid);

        Assert.That(binding.fields, Is.EqualTo(new[] { expected }));
    }

    [Test]
    public void StateMachine_ParameterByName_BindsLiftedField()
    {
        FieldInfo expected = AsyncMoveNext.InstanceType.GetField("outerValue", AccessTools.all)!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.StateMachine_ParameterByName),
            AsyncMoveNext,
            InnerIntParameter,
            AsyncTarget);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Instance));
        Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
        Assert.That(binding.fields, Is.EqualTo(new[] { expected }));
    }

    [Test]
    public void StateMachine_ParameterByIndex_UsesOriginalParameterName()
    {
        FieldInfo expected = AsyncMoveNext.InstanceType.GetField("outerValue", AccessTools.all)!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.StateMachine_ParameterByIndex),
            AsyncMoveNext,
            InnerIntParameter,
            AsyncTarget);

        Assert.That(binding.fields, Is.EqualTo(new[] { expected }));
    }

    [Test]
    public void StateMachine_Instance_BindsLiftedDeclaringInstance()
    {
        FieldInfo expected = AsyncMoveNext.InstanceType.GetFields(AccessTools.all)
            .Single(field => field.FieldType == typeof(AsyncMethodTargets));

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.StateMachine_Instance),
            AsyncMoveNext,
            InnerIntParameter,
            AsyncTarget);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Instance));
        Assert.That(binding.fields, Is.EqualTo(new[] { expected }));
    }

    [Test]
    public void StateMachine_Field_BindsThroughLiftedDeclaringInstance()
    {
        FieldInfo thisField = AsyncMoveNext.InstanceType.GetFields(AccessTools.all)
            .Single(field => field.FieldType == typeof(AsyncMethodTargets));
        FieldInfo valueField = typeof(AsyncMethodTargets).GetField(nameof(AsyncMethodTargets.Field))!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.StateMachine_Field),
            AsyncMoveNext,
            InnerIntParameter,
            AsyncTarget);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Instance));
        Assert.That(binding.fields, Is.EqualTo(new[] { thisField, valueField }));
    }

    [Test]
    public void CapturedVariable_ByName_BindsClosureParameterAndField()
    {
        MethodInfo localMethod = (MethodInfo)ReflectionTools.GetMember(
            typeof(LocalFunctionTargets),
            "CapturedReferenceVariableMethod.LocalMethod",
            MemberType.Method,
            null,
            null);
        var outer = new MethodInvocation(localMethod);
        int closureIndex = Array.FindLastIndex(outer.ParameterTypes, type => type.IsClosureType);
        FieldInfo expected = outer.ParameterTypes[closureIndex].NoRefType.GetField("captured", AccessTools.all)!;

        BoundParameter binding = Bind(nameof(ParameterBinderPatchMethods.CapturedVariable_ByName), outer);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Argument));
        Assert.That(binding.scope, Is.EqualTo(Scope.Outer));
        Assert.That(binding.index, Is.EqualTo(closureIndex));
        Assert.That(binding.fields, Is.EqualTo(new[] { expected }));
    }

    [Test]
    public void CapturedVariable_InnerScope_BindsInnerClosureParameterAndField()
    {
        MethodInfo localMethod = (MethodInfo)ReflectionTools.GetMember(
            typeof(LocalFunctionTargets),
            "CapturedReferenceVariableMethod.LocalMethod",
            MemberType.Method,
            null,
            null);
        var inner = new MethodInvocation(localMethod);
        int closureIndex = Array.FindLastIndex(inner.ParameterTypes, type => type.IsClosureType);
        FieldInfo expected = inner.ParameterTypes[closureIndex].NoRefType.GetField("captured", AccessTools.all)!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.CapturedVariable_ByName), StaticVoid, inner);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Argument));
        Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
        Assert.That(binding.index, Is.EqualTo(closureIndex));
        Assert.That(binding.fields, Is.EqualTo(new[] { expected }));
    }

    [Test]
    public void StateMachine_ClosureParameter_BindsThroughLiftedClosure()
    {
        MethodInfo targetMethod = (MethodInfo)ReflectionTools.GetMember(
            typeof(LocalFunctionTargets),
            "PrimitiveLocalIterator.LocalIterator",
            MemberType.Method,
            null,
            null);
        var target = new MethodInvocation(targetMethod);
        var moveNext = new MethodInvocation(targetMethod.GetStateMachineImplementation()!);
        FieldInfo closureField = moveNext.InstanceType.GetFields(AccessTools.all)
            .Single(field => field.FieldType.IsClosureType);
        FieldInfo valueField = closureField.FieldType.GetField("enclosingValue", AccessTools.all)!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.StateMachine_ClosureParameter),
            moveNext,
            InnerIntParameter,
            target);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Instance));
        Assert.That(binding.fields, Is.EqualTo(new[] { closureField, valueField }));
    }

    [Test]
    public void AllowUnsafe_Method_MutableStructInstance_IsAccepted()
    {
        MethodInfo targetMethod = typeof(MethodBindingStructTargets)
            .GetMethod(nameof(MethodBindingStructTargets.TargetInstanceMethod))!;

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Method_MutableStructInstance),
            new MethodInvocation(targetMethod),
            options: PatchOptions.AllowUnsafe);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
        Assert.That(binding.memberInfo, Is.EqualTo(typeof(MethodBindingStructTargets)
            .GetMethod(nameof(MethodBindingStructTargets.BoundInstanceMethod))));
    }

    [Test]
    public void Error_Method_MutableStructInstance_ThrowsParameterBindingException()
    {
        MethodInfo targetMethod = typeof(MethodBindingStructTargets)
            .GetMethod(nameof(MethodBindingStructTargets.TargetInstanceMethod))!;

        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(
                nameof(ParameterBinderPatchMethods.Method_MutableStructInstance),
                new MethodInvocation(targetMethod)))!;

        Assert.That(exception.Message,
            Is.EqualTo("method: [Method] is not supported for non-static methods on structs"));
    }

    [Test]
    public void Error_MultipleBindingAttributes_ThrowsParameterBindingException()
    {
        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Error_MultipleBindingAttributes), StaticIntParameter))!;

        Assert.That(exception.Message, Is.EqualTo("value: Multiple parameter binding attributes"));
        Assert.That(exception.InnerException, Is.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Error_InvalidScopeValue_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Error_InvalidScopeValue), InstanceVoid));
    }

    [Test]
    public void Error_InvalidInnerScope_ThrowsParameterBindingException()
    {
        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Error_InvalidInnerScope), InstanceVoid))!;

        Assert.That(exception.Message, Is.EqualTo("value: Invalid scope"));
    }

    [Test]
    public void Error_CallerOutsideInnerPatch_ThrowsParameterBindingException()
    {
        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Error_CallerOutsideInnerPatch), InstanceVoid))!;

        Assert.That(exception.Message, Is.EqualTo("__caller: Can only be used with inner patches"));
    }

    [Test]
    public void Error_ParameterIndexOutOfRange_ThrowsParameterBindingException()
    {
        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Error_ParameterIndexOutOfRange), StaticIntParameter))!;

        Assert.That(exception.Message, Is.EqualTo("value: Index is out of range"));
        Assert.That(exception.InnerException, Is.TypeOf<IndexOutOfRangeException>());
    }

    [Test]
    public void Error_ParameterNotFound_ThrowsParameterBindingException()
    {
        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Error_ParameterNotFound), StaticIntParameter))!;

        Assert.That(exception.Message, Is.EqualTo("missing: Parameter not found"));
    }

    [Test]
    public void Error_ParameterTypeMismatch_ThrowsInvalidCastException()
    {
        Assert.Throws<InvalidCastException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Error_ParameterTypeMismatch), StaticIntParameter));
    }

    [Test]
    public void Error_ReturnValueForVoid_ThrowsParameterBindingException()
    {
        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Error_ReturnValueForVoid), StaticVoid))!;

        Assert.That(exception.Message, Is.EqualTo("result: Method returns void"));
    }

    [Test]
    public void Error_ReturnValueForAlwaysRunPrefix_ThrowsParameterBindingException()
    {
        var outer = new MockInvocation(typeof(void), typeof(int), [], [], true);

        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(
                nameof(ParameterBinderPatchMethods.Error_ReturnValueForAlwaysRunPrefix),
                outer,
                options: PatchOptions.AlwaysRun))!;

        Assert.That(exception.Message,
            Is.EqualTo("__result: Binding return value not allowed for Prefix with AlwaysRun option"));
    }

    [Test]
    public void Error_InstanceForStaticMethod_ThrowsParameterBindingException()
    {
        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Error_InstanceForStaticMethod), StaticVoid))!;

        Assert.That(exception.Message, Is.EqualTo("__instance: Method is static"));
    }

    [Test]
    public void Error_ExceptionWithoutAlwaysRun_ThrowsParameterBindingException()
    {
        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(
                nameof(ParameterBinderPatchMethods.Error_ExceptionWithoutAlwaysRun),
                StaticVoid,
                patchKind: PatchKind.Postfix))!;

        Assert.That(exception.Message,
            Is.EqualTo("__exception: Accessing exception is only supported for Postfix with AlwaysRun option"));
    }

    [Test]
    public void Error_UnsupportedBindingAttribute_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Error_UnsupportedBindingAttribute), StaticIntParameter));
    }

    [Test]
    public void Error_PostfixOuterParameterByWritableReference_ThrowsParameterBindingException()
    {
        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(
                nameof(ParameterBinderPatchMethods.Error_PostfixOuterParameterByWritableReference),
                StaticIntParameter,
                patchKind: PatchKind.Postfix))!;

        Assert.That(exception.Message,
            Is.EqualTo("value: Postfix can't access outer method parameter by writeable reference"));
    }

    [Test]
    public void AllowUnsafe_PostfixOuterParameterByWritableReference_IsAccepted()
    {
        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Error_PostfixOuterParameterByWritableReference),
            StaticIntParameter,
            patchKind: PatchKind.Postfix,
            options: PatchOptions.AllowUnsafe);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Argument));
    }

    [Test]
    public void Error_InnerPostfixInnerParameterByWritableReference_ThrowsParameterBindingException()
    {
        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(
                nameof(ParameterBinderPatchMethods.Error_InnerPostfixInnerParameterByWritableReference),
                StaticVoid,
                InnerIntParameter,
                patchKind: PatchKind.Postfix))!;

        Assert.That(exception.Message,
            Is.EqualTo("value: Postfix can't access inner method parameter by writeable reference"));
    }

    [Test]
    public void AllowUnsafe_InnerPostfixInnerParameterByWritableReference_IsAccepted()
    {
        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.Error_InnerPostfixInnerParameterByWritableReference),
            StaticVoid,
            InnerIntParameter,
            patchKind: PatchKind.Postfix,
            options: PatchOptions.AllowUnsafe);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Argument));
        Assert.That(binding.scope, Is.EqualTo(Scope.Inner));
    }

    [Test]
    public void Error_MethodNotFound_ThrowsParameterBindingException()
    {
        MethodInfo targetMethod = typeof(MethodBindingInstanceTargets)
            .GetMethod(nameof(MethodBindingInstanceTargets.TargetInstanceMethod))!;

        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Error_MethodNotFound), new MethodInvocation(targetMethod)))!;

        Assert.That(exception.Message, Is.EqualTo("method: Method not found"));
    }

    [Test]
    public void Error_MethodRequiresInstance_ThrowsParameterBindingException()
    {
        MethodInfo targetMethod = typeof(MethodBindingInstanceTargets)
            .GetMethod(nameof(MethodBindingInstanceTargets.TargetStaticMethod))!;

        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Error_MethodRequiresInstance), new MethodInvocation(targetMethod)))!;

        Assert.That(exception.Message, Is.EqualTo("method: Instance required"));
    }

    [Test]
    public void Error_MethodParameterMismatch_ThrowsParameterBindingException()
    {
        MethodInfo targetMethod = typeof(MethodBindingInstanceTargets)
            .GetMethod(nameof(MethodBindingInstanceTargets.TargetInstanceMethod))!;

        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Error_MethodParameterMismatch), new MethodInvocation(targetMethod)))!;

        Assert.That(exception.Message, Is.EqualTo("method: Method not found"));
    }

    [Test]
    public void Error_BaseMethodForNonMethodInvocation_ThrowsParameterBindingException()
    {
        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Error_BaseMethodForNonMethodInvocation), InstanceVoid))!;

        Assert.That(exception.Message, Is.EqualTo("__base: Must be an instance method"));
    }

    [Test]
    public void Error_BaseMethodForStaticMethod_ThrowsParameterBindingException()
    {
        MethodInfo targetMethod = typeof(MethodBindingStaticTargets)
            .GetMethod(nameof(MethodBindingStaticTargets.TargetStaticMethod))!;

        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(
                nameof(ParameterBinderPatchMethods.Error_BaseMethodForStaticMethod),
                new MethodInvocation(targetMethod)))!;

        Assert.That(exception.Message, Is.EqualTo("__base: Must be an instance method"));
    }

    [Test]
    public void Error_BaseMethodParameterIsNotDelegate_ThrowsInvalidCastException()
    {
        MethodInfo targetMethod = typeof(DerivedMethodTargets).GetMethod(nameof(DerivedMethodTargets.Describe))!;

        InvalidCastException exception = Assert.Throws<InvalidCastException>(() =>
            Bind(
                nameof(ParameterBinderPatchMethods.Error_BaseMethodParameterIsNotDelegate),
                new MethodInvocation(targetMethod)))!;

        Assert.That(exception.Message, Is.EqualTo("__base: Can't convert System.Object to System.Delegate"));
    }

    [Test]
    public void Error_BaseMethodDelegateHasNoInvoke_ThrowsParameterBindingException()
    {
        MethodInfo targetMethod = typeof(DerivedMethodTargets).GetMethod(nameof(DerivedMethodTargets.Describe))!;

        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(
                nameof(ParameterBinderPatchMethods.Error_BaseMethodDelegateHasNoInvoke),
                new MethodInvocation(targetMethod)))!;

        Assert.That(exception.Message, Is.EqualTo("__base: Delegate.Invoke not found"));
    }

    [Test]
    public void Error_BaseMethodParameterTypeMismatch_ThrowsParameterBindingException()
    {
        MethodInfo targetMethod = typeof(DerivedMethodTargets).GetMethod(nameof(DerivedMethodTargets.Describe))!;

        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(
                nameof(ParameterBinderPatchMethods.Error_BaseMethodParameterTypeMismatch),
                new MethodInvocation(targetMethod)))!;

        Assert.That(exception.Message, Is.EqualTo("__base: Parameter type mismatch"));
    }

    [Test]
    public void Error_BaseMethodParameterCountMismatch_ThrowsParameterBindingException()
    {
        MethodInfo targetMethod = typeof(DerivedMethodTargets).GetMethod(nameof(DerivedMethodTargets.Describe))!;

        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(
                nameof(ParameterBinderPatchMethods.Error_BaseMethodParameterCountMismatch),
                new MethodInvocation(targetMethod)))!;

        Assert.That(exception.Message, Is.EqualTo("__base: Parameter type mismatch"));
    }

    [Test]
    public void Error_BaseMethodReturnTypeMismatch_ThrowsParameterBindingException()
    {
        MethodInfo targetMethod = typeof(DerivedMethodTargets).GetMethod(nameof(DerivedMethodTargets.Describe))!;

        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(
                nameof(ParameterBinderPatchMethods.Error_BaseMethodReturnTypeMismatch),
                new MethodInvocation(targetMethod)))!;

        Assert.That(exception.Message, Is.EqualTo("__base: Return type mismatch"));
    }

    [Test]
    public void Error_BaseMethodNotFound_ThrowsParameterBindingException()
    {
        MethodInfo targetMethod = typeof(MethodBindingInstanceTargets)
            .GetMethod(nameof(MethodBindingInstanceTargets.TargetInstanceMethod))!;

        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Error_BaseMethodNotFound), new MethodInvocation(targetMethod)))!;

        Assert.That(exception.Message, Is.EqualTo("__base: Base method not found"));
    }

    [Test]
    public void Error_BaseMethodIsAbstract_ThrowsParameterBindingException()
    {
        MethodInfo targetMethod = typeof(BaseMethodAbstractDerivedTargets)
            .GetMethod(nameof(BaseMethodAbstractDerivedTargets.Describe))!;

        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Error_BaseMethodIsAbstract), new MethodInvocation(targetMethod)))!;

        Assert.That(exception.Message, Is.EqualTo("method: Base method is abstract"));
    }

    [Test]
    public void StateMachine_Error_InstanceByWritableReference_ThrowsParameterBindingException()
    {
        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(
                nameof(ParameterBinderPatchMethods.StateMachine_Error_InstanceByWritableReference),
                AsyncMoveNext,
                InnerIntParameter,
                AsyncTarget))!;

        Assert.That(exception.Message,
            Is.EqualTo("instance: Accessing 'this' by reference is not supported for iterator state machine methods"));
    }

    [Test]
    public void StateMachine_Error_ParameterNotFound_ThrowsParameterBindingException()
    {
        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(
                nameof(ParameterBinderPatchMethods.StateMachine_Error_ParameterNotFound),
                AsyncMoveNext,
                InnerIntParameter,
                AsyncTarget))!;

        Assert.That(exception.Message, Is.EqualTo("value: Parameter not found"));
    }

    [Test]
    public void StateMachine_Error_InstanceForStaticMethod_ThrowsParameterBindingException()
    {
        MethodInfo targetMethod = typeof(AsyncMethodTargets).GetMethod(
            nameof(AsyncMethodTargets.CallBeforeAndAfterAwait),
            [typeof(Task), typeof(int)])!;
        var target = new MethodInvocation(targetMethod);
        var moveNext = new MethodInvocation(targetMethod.GetStateMachineImplementation()!);

        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(
                nameof(ParameterBinderPatchMethods.StateMachine_Error_InstanceForStaticMethod),
                moveNext,
                InnerIntParameter,
                target))!;

        Assert.That(exception.Message, Is.EqualTo("instance: Method is static"));
    }

    [Test]
    public void StateMachine_Error_MethodForOuterInstance_ThrowsParameterBindingException()
    {
        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(
                nameof(ParameterBinderPatchMethods.StateMachine_Error_MethodForOuterInstance),
                AsyncMoveNext,
                InnerIntParameter,
                AsyncTarget))!;

        Assert.That(exception.Message, Is.EqualTo("method: [Method] is not supported for iterator state machines"));
    }

    [Test]
    public void StateMachine_MethodForOuterStaticMethod_BindsDelegate()
    {
        MethodInfo targetMethod = typeof(AsyncMethodTargets).GetMethod(
            nameof(AsyncMethodTargets.CallBeforeAndAfterAwait),
            [typeof(Task), typeof(int)])!;
        MethodInfo expected = targetMethod;
        var target = new MethodInvocation(targetMethod);
        var moveNext = new MethodInvocation(targetMethod.GetStateMachineImplementation()!);

        BoundParameter binding = Bind(
            nameof(ParameterBinderPatchMethods.StateMachine_MethodForOuterStaticMethod),
            moveNext,
            InnerIntParameter,
            target);

        Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
        Assert.That(binding.memberInfo, Is.SameAs(expected));
    }

    [Test]
    public void Error_FieldNotFound_ThrowsParameterBindingException()
    {
        ParameterBindingException exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(ParameterBinderPatchMethods.Error_FieldNotFound), InstanceVoid))!;

        Assert.That(exception.Message, Is.EqualTo("value: Field not found"));
    }
}
