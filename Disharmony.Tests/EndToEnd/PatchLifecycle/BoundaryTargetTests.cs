namespace Disharmony.Tests.EndToEnd.PatchLifecycle;

public static class BoundaryTargetPatches
{
    [Prefix]
    [Target(typeof(MemberInfoGenericTargets<int>), nameof(MemberInfoGenericTargets<int>.StaticIdentity))]
    public static void Prefix_Method_ClosedGenericType_Static_Int32_IsRejectedBeforeRuntimePatching([MemberInfo] MethodInfo member) { }

    [Prefix]
    [Target(typeof(MemberInfoGenericTargets<string>), nameof(MemberInfoGenericTargets<string>.StaticIdentity))]
    public static void Prefix_Method_ClosedGenericType_Static_String_IsRejectedBeforeRuntimePatching([MemberInfo] MethodInfo member) { }

    [Postfix]
    [Target(typeof(MemberInfoGenericTargets<int>), nameof(MemberInfoGenericTargets<int>.StaticIdentity))]
    public static void Postfix_Method_ClosedGenericType_Static_Int32_IsRejectedBeforeRuntimePatching([MemberInfo] MethodInfo member) { }

    [Postfix]
    [Target(typeof(MemberInfoGenericTargets<string>), nameof(MemberInfoGenericTargets<string>.StaticIdentity))]
    public static void Postfix_Method_ClosedGenericType_Static_String_IsRejectedBeforeRuntimePatching([MemberInfo] MethodInfo member) { }

    [Prefix]
    [Target(typeof(MemberInfoGenericTargets<int>), nameof(MemberInfoGenericTargets<int>.Identity))]
    public static void Prefix_Method_ClosedGenericType_Instance_Int32_IsRejectedBeforeRuntimePatching([MemberInfo] MethodInfo member) { }

    [Prefix]
    [Target(typeof(MemberInfoGenericTargets<string>), nameof(MemberInfoGenericTargets<string>.Identity))]
    public static void Prefix_Method_ClosedGenericType_Instance_String_IsRejectedBeforeRuntimePatching([MemberInfo] MethodInfo member) { }

    [Postfix]
    [Target(typeof(MemberInfoGenericTargets<int>), nameof(MemberInfoGenericTargets<int>.Identity))]
    public static void Postfix_Method_ClosedGenericType_Instance_Int32_IsRejectedBeforeRuntimePatching([MemberInfo] MethodInfo member) { }

    [Postfix]
    [Target(typeof(MemberInfoGenericTargets<string>), nameof(MemberInfoGenericTargets<string>.Identity))]
    public static void Postfix_Method_ClosedGenericType_Instance_String_IsRejectedBeforeRuntimePatching([MemberInfo] MethodInfo member) { }

    [Prefix]
    [Inner(typeof(MemberInfoGenericTargets<int>), nameof(MemberInfoGenericTargets<int>.Identity))]
    [Target(typeof(MemberInfoGenericTargets<int>), nameof(MemberInfoGenericTargets<int>.CallIdentity))]
    public static void InnerPrefix_Method_ClosedGenericType_InnerScope_Int32_IsRejectedBeforeRuntimePatching([MemberInfo(Scope.Inner)] MethodInfo member) { }

    [Prefix]
    [Inner(typeof(MemberInfoGenericTargets<string>), nameof(MemberInfoGenericTargets<string>.Identity))]
    [Target(typeof(MemberInfoGenericTargets<string>), nameof(MemberInfoGenericTargets<string>.CallIdentity))]
    public static void InnerPrefix_Method_ClosedGenericType_InnerScope_String_IsRejectedBeforeRuntimePatching([MemberInfo(Scope.Inner)] MethodInfo member) { }

    [Postfix]
    [Inner(typeof(MemberInfoGenericTargets<int>), nameof(MemberInfoGenericTargets<int>.Identity))]
    [Target(typeof(MemberInfoGenericTargets<int>), nameof(MemberInfoGenericTargets<int>.CallIdentity))]
    public static void InnerPostfix_Method_ClosedGenericType_InnerScope_Int32_IsRejectedBeforeRuntimePatching([MemberInfo(Scope.Inner)] MethodInfo member) { }

    [Postfix]
    [Inner(typeof(MemberInfoGenericTargets<string>), nameof(MemberInfoGenericTargets<string>.Identity))]
    [Target(typeof(MemberInfoGenericTargets<string>), nameof(MemberInfoGenericTargets<string>.CallIdentity))]
    public static void InnerPostfix_Method_ClosedGenericType_InnerScope_String_IsRejectedBeforeRuntimePatching([MemberInfo(Scope.Inner)] MethodInfo member) { }

    [Prefix]
    [Inner(typeof(MemberInfoGenericTargets<int>), nameof(MemberInfoGenericTargets<int>.Identity))]
    [Target(typeof(MemberInfoGenericTargets<int>), nameof(MemberInfoGenericTargets<int>.CallIdentity))]
    public static void InnerPrefix_Method_ClosedGenericType_OuterScope_Int32_IsRejectedBeforeRuntimePatching([MemberInfo(Scope.Outer)] MethodInfo member) { }

    [Prefix]
    [Inner(typeof(MemberInfoGenericTargets<string>), nameof(MemberInfoGenericTargets<string>.Identity))]
    [Target(typeof(MemberInfoGenericTargets<string>), nameof(MemberInfoGenericTargets<string>.CallIdentity))]
    public static void InnerPrefix_Method_ClosedGenericType_OuterScope_String_IsRejectedBeforeRuntimePatching([MemberInfo(Scope.Outer)] MethodInfo member) { }

    [Postfix]
    [Inner(typeof(MemberInfoGenericTargets<int>), nameof(MemberInfoGenericTargets<int>.Identity))]
    [Target(typeof(MemberInfoGenericTargets<int>), nameof(MemberInfoGenericTargets<int>.CallIdentity))]
    public static void InnerPostfix_Method_ClosedGenericType_OuterScope_Int32_IsRejectedBeforeRuntimePatching([MemberInfo(Scope.Outer)] MethodInfo member) { }

    [Postfix]
    [Inner(typeof(MemberInfoGenericTargets<string>), nameof(MemberInfoGenericTargets<string>.Identity))]
    [Target(typeof(MemberInfoGenericTargets<string>), nameof(MemberInfoGenericTargets<string>.CallIdentity))]
    public static void InnerPostfix_Method_ClosedGenericType_OuterScope_String_IsRejectedBeforeRuntimePatching([MemberInfo(Scope.Outer)] MethodInfo member) { }

    public static bool PatchExecuted;

    public static void AbstractMethod_IsRejectedBeforeRuntimePatching() => PatchExecuted = true;

    public static void InterfaceMethod_IsRejectedBeforeRuntimePatching() => PatchExecuted = true;

    public static void PInvokeMethod_IsRejectedBeforeRuntimePatching() => PatchExecuted = true;

    public static void OpenGenericDeclaringType_IsRejectedBeforeRuntimePatching() => PatchExecuted = true;

    public static void GenericMethodDefinition_IsRejectedBeforeRuntimePatching() => PatchExecuted = true;

    public static void ConstructedGenericMethod_IsRejectedBeforeRuntimePatching() => PatchExecuted = true;

    public static void ClosedGenericDeclaringType_IsRejectedBeforeRuntimePatching() => PatchExecuted = true;

    public static void ExplicitInterfaceImplementation_ExecutesPatch() => PatchExecuted = true;

    public static void PointerParameterMethod_ExecutesPatchAndPreservesPointer() => PatchExecuted = true;

    public static void VarArgsMethod_IsRejectedBeforeRuntimePatching() => PatchExecuted = true;
}

[TestFixture]
public sealed class BoundaryTargetTests : PatchTestBase
{
    [Test]
    public void Prefix_Method_ClosedGenericType_Static_Int32_IsRejectedBeforeRuntimePatching()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(typeof(BoundaryTargetPatches), nameof(BoundaryTargetPatches.Prefix_Method_ClosedGenericType_Static_Int32_IsRejectedBeforeRuntimePatching)));

        Assert.That(exception!.InnerException, Is.TypeOf<PatchDefinitionException>());
    }

    [Test]
    public void Prefix_Method_ClosedGenericType_Static_String_IsRejectedBeforeRuntimePatching()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(typeof(BoundaryTargetPatches), nameof(BoundaryTargetPatches.Prefix_Method_ClosedGenericType_Static_String_IsRejectedBeforeRuntimePatching)));

        Assert.That(exception!.InnerException, Is.TypeOf<PatchDefinitionException>());
    }

    [Test]
    public void Postfix_Method_ClosedGenericType_Static_Int32_IsRejectedBeforeRuntimePatching()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(typeof(BoundaryTargetPatches), nameof(BoundaryTargetPatches.Postfix_Method_ClosedGenericType_Static_Int32_IsRejectedBeforeRuntimePatching)));

        Assert.That(exception!.InnerException, Is.TypeOf<PatchDefinitionException>());
    }

    [Test]
    public void Postfix_Method_ClosedGenericType_Static_String_IsRejectedBeforeRuntimePatching()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(typeof(BoundaryTargetPatches), nameof(BoundaryTargetPatches.Postfix_Method_ClosedGenericType_Static_String_IsRejectedBeforeRuntimePatching)));

        Assert.That(exception!.InnerException, Is.TypeOf<PatchDefinitionException>());
    }

    [Test]
    public void Prefix_Method_ClosedGenericType_Instance_Int32_IsRejectedBeforeRuntimePatching()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(typeof(BoundaryTargetPatches), nameof(BoundaryTargetPatches.Prefix_Method_ClosedGenericType_Instance_Int32_IsRejectedBeforeRuntimePatching)));

        Assert.That(exception!.InnerException, Is.TypeOf<PatchDefinitionException>());
    }

    [Test]
    public void Prefix_Method_ClosedGenericType_Instance_String_IsRejectedBeforeRuntimePatching()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(typeof(BoundaryTargetPatches), nameof(BoundaryTargetPatches.Prefix_Method_ClosedGenericType_Instance_String_IsRejectedBeforeRuntimePatching)));

        Assert.That(exception!.InnerException, Is.TypeOf<PatchDefinitionException>());
    }

    [Test]
    public void Postfix_Method_ClosedGenericType_Instance_Int32_IsRejectedBeforeRuntimePatching()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(typeof(BoundaryTargetPatches), nameof(BoundaryTargetPatches.Postfix_Method_ClosedGenericType_Instance_Int32_IsRejectedBeforeRuntimePatching)));

        Assert.That(exception!.InnerException, Is.TypeOf<PatchDefinitionException>());
    }

    [Test]
    public void Postfix_Method_ClosedGenericType_Instance_String_IsRejectedBeforeRuntimePatching()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(typeof(BoundaryTargetPatches), nameof(BoundaryTargetPatches.Postfix_Method_ClosedGenericType_Instance_String_IsRejectedBeforeRuntimePatching)));

        Assert.That(exception!.InnerException, Is.TypeOf<PatchDefinitionException>());
    }

    [Test]
    public void InnerPrefix_Method_ClosedGenericType_InnerScope_Int32_IsRejectedBeforeRuntimePatching()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(typeof(BoundaryTargetPatches), nameof(BoundaryTargetPatches.InnerPrefix_Method_ClosedGenericType_InnerScope_Int32_IsRejectedBeforeRuntimePatching)));

        Assert.That(exception!.InnerException, Is.TypeOf<PatchDefinitionException>());
    }

    [Test]
    public void InnerPrefix_Method_ClosedGenericType_InnerScope_String_IsRejectedBeforeRuntimePatching()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(typeof(BoundaryTargetPatches), nameof(BoundaryTargetPatches.InnerPrefix_Method_ClosedGenericType_InnerScope_String_IsRejectedBeforeRuntimePatching)));

        Assert.That(exception!.InnerException, Is.TypeOf<PatchDefinitionException>());
    }

    [Test]
    public void InnerPostfix_Method_ClosedGenericType_InnerScope_Int32_IsRejectedBeforeRuntimePatching()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(typeof(BoundaryTargetPatches), nameof(BoundaryTargetPatches.InnerPostfix_Method_ClosedGenericType_InnerScope_Int32_IsRejectedBeforeRuntimePatching)));

        Assert.That(exception!.InnerException, Is.TypeOf<PatchDefinitionException>());
    }

    [Test]
    public void InnerPostfix_Method_ClosedGenericType_InnerScope_String_IsRejectedBeforeRuntimePatching()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(typeof(BoundaryTargetPatches), nameof(BoundaryTargetPatches.InnerPostfix_Method_ClosedGenericType_InnerScope_String_IsRejectedBeforeRuntimePatching)));

        Assert.That(exception!.InnerException, Is.TypeOf<PatchDefinitionException>());
    }

    [Test]
    public void InnerPrefix_Method_ClosedGenericType_OuterScope_Int32_IsRejectedBeforeRuntimePatching()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(typeof(BoundaryTargetPatches), nameof(BoundaryTargetPatches.InnerPrefix_Method_ClosedGenericType_OuterScope_Int32_IsRejectedBeforeRuntimePatching)));

        Assert.That(exception!.InnerException, Is.TypeOf<PatchDefinitionException>());
    }

    [Test]
    public void InnerPrefix_Method_ClosedGenericType_OuterScope_String_IsRejectedBeforeRuntimePatching()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(typeof(BoundaryTargetPatches), nameof(BoundaryTargetPatches.InnerPrefix_Method_ClosedGenericType_OuterScope_String_IsRejectedBeforeRuntimePatching)));

        Assert.That(exception!.InnerException, Is.TypeOf<PatchDefinitionException>());
    }

    [Test]
    public void InnerPostfix_Method_ClosedGenericType_OuterScope_Int32_IsRejectedBeforeRuntimePatching()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(typeof(BoundaryTargetPatches), nameof(BoundaryTargetPatches.InnerPostfix_Method_ClosedGenericType_OuterScope_Int32_IsRejectedBeforeRuntimePatching)));

        Assert.That(exception!.InnerException, Is.TypeOf<PatchDefinitionException>());
    }

    [Test]
    public void InnerPostfix_Method_ClosedGenericType_OuterScope_String_IsRejectedBeforeRuntimePatching()
    {
        var exception = Assert.Throws<PatchException>(() =>
            ApplyPatch(typeof(BoundaryTargetPatches), nameof(BoundaryTargetPatches.InnerPostfix_Method_ClosedGenericType_OuterScope_String_IsRejectedBeforeRuntimePatching)));

        Assert.That(exception!.InnerException, Is.TypeOf<PatchDefinitionException>());
    }

    [Test]
    public void AbstractMethod_IsRejectedBeforeRuntimePatching()
    {
        MethodInfo target = typeof(BoundaryAbstractTargets)
            .GetMethod(nameof(BoundaryAbstractTargets.AbstractMethod))!;
        MethodInfo patch = typeof(BoundaryTargetPatches)
            .GetMethod(nameof(BoundaryTargetPatches.AbstractMethod_IsRejectedBeforeRuntimePatching))!;

        Assert.Throws<PatchException>(() => Patcher.Patch(Patch.Prefix.With(patch).Of(target)));
    }

    [Test]
    public void InterfaceMethod_IsRejectedBeforeRuntimePatching()
    {
        MethodInfo target = typeof(IBoundaryInterfaceTargets)
            .GetMethod(nameof(IBoundaryInterfaceTargets.InterfaceMethod))!;
        MethodInfo patch = typeof(BoundaryTargetPatches)
            .GetMethod(nameof(BoundaryTargetPatches.InterfaceMethod_IsRejectedBeforeRuntimePatching))!;

        Assert.Throws<PatchException>(() => Patcher.Patch(Patch.Prefix.With(patch).Of(target)));
    }

    [Test]
    public void PInvokeMethod_IsRejectedBeforeRuntimePatching()
    {
        MethodInfo target = typeof(BoundaryTargets)
            .GetMethod(nameof(BoundaryTargets.PInvokeMethod))!;
        MethodInfo patch = typeof(BoundaryTargetPatches)
            .GetMethod(nameof(BoundaryTargetPatches.PInvokeMethod_IsRejectedBeforeRuntimePatching))!;

        Assert.Throws<PatchException>(() => Patcher.Patch(Patch.Prefix.With(patch).Of(target)));
    }

    [Test]
    public void OpenGenericDeclaringType_IsRejectedBeforeRuntimePatching()
    {
        MethodInfo target = typeof(BoundaryGenericTargets<>)
            .GetMethod(nameof(BoundaryGenericTargets<int>.NonGenericMethod))!;
        MethodInfo patch = typeof(BoundaryTargetPatches)
            .GetMethod(nameof(BoundaryTargetPatches.OpenGenericDeclaringType_IsRejectedBeforeRuntimePatching))!;

        Assert.Throws<PatchException>(() => Patcher.Patch(Patch.Prefix.With(patch).Of(target)));
    }

    [Test]
    public void GenericMethodDefinition_IsRejectedBeforeRuntimePatching()
    {
        MethodInfo target = typeof(BoundaryTargets)
            .GetMethod(nameof(BoundaryTargets.GenericMethod))!;
        MethodInfo patch = typeof(BoundaryTargetPatches)
            .GetMethod(nameof(BoundaryTargetPatches.GenericMethodDefinition_IsRejectedBeforeRuntimePatching))!;

        Assert.Throws<PatchException>(() => Patcher.Patch(Patch.Prefix.With(patch).Of(target)));
    }

    [Test]
    public void ConstructedGenericMethod_IsRejectedBeforeRuntimePatching()
    {
        MethodInfo target = typeof(BoundaryTargets)
            .GetMethod(nameof(BoundaryTargets.GenericMethod))!
            .MakeGenericMethod(typeof(int));
        MethodInfo patch = typeof(BoundaryTargetPatches)
            .GetMethod(nameof(BoundaryTargetPatches.ConstructedGenericMethod_IsRejectedBeforeRuntimePatching))!;

        Assert.Throws<PatchException>(() => Patcher.Patch(Patch.Prefix.With(patch).Of(target)));
    }

    [Test]
    public void ClosedGenericDeclaringType_IsRejectedBeforeRuntimePatching()
    {
        BoundaryTargetPatches.PatchExecuted = false;
        MethodInfo target = typeof(BoundaryGenericTargets<int>)
            .GetMethod(nameof(BoundaryGenericTargets<int>.NonGenericMethod))!;
        MethodInfo patch = typeof(BoundaryTargetPatches)
            .GetMethod(nameof(BoundaryTargetPatches.ClosedGenericDeclaringType_IsRejectedBeforeRuntimePatching))!;

        Assert.Throws<PatchException>(() => Patcher.Patch(Patch.Prefix.With(patch).Of(target)));
    }

    [Test]
    public void ExplicitInterfaceImplementation_ExecutesPatch()
    {
        BoundaryTargetPatches.PatchExecuted = false;
        InterfaceMapping mapping = typeof(BoundaryExplicitInterfaceTargets)
            .GetInterfaceMap(typeof(IBoundaryInterfaceTargets));
        MethodInfo target = mapping.TargetMethods.Single();
        MethodInfo patch = typeof(BoundaryTargetPatches)
            .GetMethod(nameof(BoundaryTargetPatches.ExplicitInterfaceImplementation_ExecutesPatch))!;
        var instance = new BoundaryExplicitInterfaceTargets();

        Patcher.Patch(Patch.Prefix.With(patch).Of(target));
        int result = instance.CallInterfaceMethod(42);

        Assert.That(result, Is.EqualTo(42));
        Assert.That(BoundaryTargetPatches.PatchExecuted, Is.True);
    }

    [Test]
    public void PointerParameterMethod_ExecutesPatchAndPreservesPointer()
    {
        BoundaryTargetPatches.PatchExecuted = false;
        MethodInfo target = typeof(BoundaryTargets)
            .GetMethod(nameof(BoundaryTargets.PointerParameterMethod))!;
        MethodInfo patch = typeof(BoundaryTargetPatches)
            .GetMethod(nameof(BoundaryTargetPatches.PointerParameterMethod_ExecutesPatchAndPreservesPointer))!;

        Patcher.Patch(Patch.Prefix.With(patch).Of(target));
        int result = BoundaryTargets.CallPointerParameterMethod(42);

        Assert.That(result, Is.EqualTo(42));
        Assert.That(BoundaryTargetPatches.PatchExecuted, Is.True);
    }

    [Test]
    public void VarArgsMethod_IsRejectedBeforeRuntimePatching()
    {
        MethodInfo target = typeof(BoundaryTargets)
            .GetMethod(nameof(BoundaryTargets.VarArgsMethod))!;
        MethodInfo patch = typeof(BoundaryTargetPatches)
            .GetMethod(nameof(BoundaryTargetPatches.VarArgsMethod_IsRejectedBeforeRuntimePatching))!;

        Assert.Throws<PatchException>(() => Patcher.Patch(Patch.Prefix.With(patch).Of(target)));
    }
}
