namespace Disharmony.Tests.Unit.ParameterBinding;

internal static class MethodLookupBindingPatches
{
    public delegate int RefMethod(ref int value);
    public delegate int InMethod(in int value);
    public delegate int OutMethod(out int value);

    public static void UnqualifiedIntOverload_BindsConvertInt(
        [Method("Convert")] Func<int, int> method) { }

    public static void ShortTypeName_BindsConvertInt(
        [Method("MethodLookupTarget.Convert")] Func<int, int> method) { }

    public static void FullyQualifiedTypeName_BindsConvertInt(
        [Method("Disharmony.Tests.MethodLookupTarget.Convert")] Func<int, int> method) { }

    public static void StringDelegate_BindsConvertString(
        [Method("Convert")] Func<string, string> method) { }

    public static void ParameterlessDelegate_BindsConvertWithoutArguments(
        [Method("Convert")] Func<int> method) { }

    public static void UnqualifiedInheritedMethod_BindsBaseDeclaration(
        [Method("Inherited")] Func<int, int> method) { }

    public static void ExplicitBaseType_BindsBaseDeclaration(
        [Method("MethodLookupBase.Inherited")] Func<int, int> method) { }

    public static void ExplicitInterface_BindsInterfaceDeclaration(
        [Method("IMethodLookupTarget.Convert")] Func<int, int> method) { }

    public static void RefDelegate_BindsByRef(
        [Method("ByRef")] MethodLookupBindingPatches.RefMethod method) { }

    public static void InDelegate_BindsByIn(
        [Method("ByIn")] MethodLookupBindingPatches.InMethod method) { }

    public static void OutDelegate_BindsByOut(
        [Method("ByOut")] MethodLookupBindingPatches.OutMethod method) { }

    public static void UnrelatedStaticMethod_BindsMathAbs(
        [Method("System.Math.Abs")] Func<int, int> method) { }

    public static void NestedShortTypeName_BindsNestedStaticMethod(
        [Method("MethodLookupTarget.Nested.StaticMethod")] Func<int, int> method) { }

    public static void NestedFullyQualifiedName_BindsNestedStaticMethod(
        [Method("Disharmony.Tests.MethodLookupTarget.Nested.StaticMethod")] Func<int, int> method) { }

    public static void NestedColonSyntax_BindsNestedStaticMethod(
        [Method("MethodLookupTarget:Nested.StaticMethod")] Func<int, int> method) { }

    public static void DeepNestedColonSyntax_BindsDeepNestedStaticMethod(
        [Method("Disharmony.Tests.MethodLookupTarget:Nested.DeepNested.StaticMethod")] Func<int, int> method) { }

    public static void DeepNestedDottedName_BindsDeepNestedStaticMethod(
        [Method("Disharmony.Tests.MethodLookupTarget.Nested.DeepNested.StaticMethod")] Func<int, int> method) { }

    public static void UnqualifiedInheritedMethodOnNestedInstance_BindsBaseDeclaration(
        [Method("Inherited")] Func<int, int> method) { }

    public static void DerivedOnlyMethodOnBaseInstance_ThrowsInstanceTypeMismatch(
        [Method("MethodLookupDerived.DerivedOnly")] Func<int, int> method) { }

    public static void UnrelatedInstanceMethod_ThrowsInstanceTypeMismatch(
        [Method("System.String.CompareTo")] Func<string, int> method) { }

    public static void OutDelegateForRefMethod_ThrowsMethodNotFound(
        [Method("ByRef")] MethodLookupBindingPatches.OutMethod method) { }

    public static void ExplicitNestedTypeWithInheritedMethod_ThrowsMethodNotFound(
        [Method("MethodLookupTarget.Nested.Inherited")] Func<int, int> method) { }

    public static void ExplicitShortTypeWithInheritedMethod_ThrowsMethodNotFound(
        [Method("MethodLookupTarget.Inherited")] Func<int, int> method) { }

    public static void ExplicitFullyQualifiedTypeWithInheritedMethod_ThrowsMethodNotFound(
        [Method("Disharmony.Tests.MethodLookupTarget.Inherited")] Func<int, int> method) { }

    public static void ExplicitColonTypeWithInheritedMethod_ThrowsMethodNotFound(
        [Method("MethodLookupTarget:Inherited")] Func<int, int> method) { }

    public static void QualifiedStaticMethodOnStaticTarget_BindsMathAbsWithoutInstance(
        [Method("System.Math.Abs")] Func<int, int> method) { }
}

[TestFixture]
public sealed class MethodLookupBindingTests
{
    [Test]
    public void UnqualifiedIntOverload_BindsConvertInt()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget), typeof(void),
            [typeof(MethodLookupTarget)], ["<instance>"], false);

        MethodInfo expectedMethod = typeof(MethodLookupTarget).GetMethod("Convert", [typeof(int)])!;

        var binding = Bind(
            nameof(MethodLookupBindingPatches.UnqualifiedIntOverload_BindsConvertInt), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.memberInfo, Is.EqualTo(expectedMethod));
        });
    }

    [Test]
    public void ShortTypeName_BindsConvertInt()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget), typeof(void),
            [typeof(MethodLookupTarget)], ["<instance>"], false);

        MethodInfo expectedMethod = typeof(MethodLookupTarget).GetMethod("Convert", [typeof(int)])!;

        var binding = Bind(
            nameof(MethodLookupBindingPatches.ShortTypeName_BindsConvertInt), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.memberInfo, Is.EqualTo(expectedMethod));
        });
    }

    [Test]
    public void FullyQualifiedTypeName_BindsConvertInt()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget), typeof(void),
            [typeof(MethodLookupTarget)], ["<instance>"], false);

        MethodInfo expectedMethod = typeof(MethodLookupTarget).GetMethod("Convert", [typeof(int)])!;

        var binding = Bind(
            nameof(MethodLookupBindingPatches.FullyQualifiedTypeName_BindsConvertInt), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.memberInfo, Is.EqualTo(expectedMethod));
        });
    }

    [Test]
    public void StringDelegate_BindsConvertString()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget), typeof(void),
            [typeof(MethodLookupTarget)], ["<instance>"], false);

        MethodInfo expectedMethod = typeof(MethodLookupTarget).GetMethod("Convert", [typeof(string)])!;

        var binding = Bind(
            nameof(MethodLookupBindingPatches.StringDelegate_BindsConvertString), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.memberInfo, Is.EqualTo(expectedMethod));
        });
    }

    [Test]
    public void ParameterlessDelegate_BindsConvertWithoutArguments()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget), typeof(void),
            [typeof(MethodLookupTarget)], ["<instance>"], false);

        MethodInfo expectedMethod = typeof(MethodLookupTarget).GetMethod("Convert", [])!;

        var binding = Bind(
            nameof(MethodLookupBindingPatches.ParameterlessDelegate_BindsConvertWithoutArguments), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.memberInfo, Is.EqualTo(expectedMethod));
        });
    }

    [Test]
    public void UnqualifiedInheritedMethod_BindsBaseDeclaration()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget), typeof(void),
            [typeof(MethodLookupTarget)], ["<instance>"], false);

        MethodInfo expectedMethod = typeof(MethodLookupBase).GetMethod("Inherited", [typeof(int)])!;

        var binding = Bind(
            nameof(MethodLookupBindingPatches.UnqualifiedInheritedMethod_BindsBaseDeclaration), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.memberInfo, Is.EqualTo(expectedMethod));
        });
    }

    [Test]
    public void ExplicitBaseType_BindsBaseDeclaration()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget), typeof(void),
            [typeof(MethodLookupTarget)], ["<instance>"], false);

        MethodInfo expectedMethod = typeof(MethodLookupBase).GetMethod("Inherited", [typeof(int)])!;

        var binding = Bind(
            nameof(MethodLookupBindingPatches.ExplicitBaseType_BindsBaseDeclaration), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.memberInfo, Is.EqualTo(expectedMethod));
        });
    }

    [Test]
    public void ExplicitInterface_BindsInterfaceDeclaration()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget), typeof(void),
            [typeof(MethodLookupTarget)], ["<instance>"], false);

        MethodInfo expectedMethod = typeof(IMethodLookupTarget).GetMethod("Convert", [typeof(int)])!;

        var binding = Bind(
            nameof(MethodLookupBindingPatches.ExplicitInterface_BindsInterfaceDeclaration), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.memberInfo, Is.EqualTo(expectedMethod));
        });
    }

    [Test]
    public void RefDelegate_BindsByRef()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget), typeof(void),
            [typeof(MethodLookupTarget)], ["<instance>"], false);

        MethodInfo expectedMethod = typeof(MethodLookupTarget).GetMethod("ByRef", [typeof(int).MakeByRefType()])!;

        var binding = Bind(
            nameof(MethodLookupBindingPatches.RefDelegate_BindsByRef), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.memberInfo, Is.EqualTo(expectedMethod));
        });
    }

    [Test]
    public void InDelegate_BindsByIn()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget), typeof(void),
            [typeof(MethodLookupTarget)], ["<instance>"], false);

        MethodInfo expectedMethod = typeof(MethodLookupTarget).GetMethod("ByIn", [typeof(int).MakeByRefType()])!;

        var binding = Bind(
            nameof(MethodLookupBindingPatches.InDelegate_BindsByIn), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.memberInfo, Is.EqualTo(expectedMethod));
        });
    }

    [Test]
    public void OutDelegate_BindsByOut()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget), typeof(void),
            [typeof(MethodLookupTarget)], ["<instance>"], false);

        MethodInfo expectedMethod = typeof(MethodLookupTarget).GetMethod("ByOut", [typeof(int).MakeByRefType()])!;

        var binding = Bind(
            nameof(MethodLookupBindingPatches.OutDelegate_BindsByOut), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.memberInfo, Is.EqualTo(expectedMethod));
        });
    }

    [Test]
    public void UnrelatedStaticMethod_BindsMathAbs()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget), typeof(void),
            [typeof(MethodLookupTarget)], ["<instance>"], false);

        MethodInfo expectedMethod = typeof(Math).GetMethod("Abs", [typeof(int)])!;

        var binding = Bind(
            nameof(MethodLookupBindingPatches.UnrelatedStaticMethod_BindsMathAbs), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.memberInfo, Is.EqualTo(expectedMethod));
        });
    }

    [Test]
    public void NestedShortTypeName_BindsNestedStaticMethod()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget.Nested), typeof(void),
            [typeof(MethodLookupTarget.Nested)], ["<instance>"], false);

        MethodInfo expectedMethod = typeof(MethodLookupTarget.Nested).GetMethod("StaticMethod", [typeof(int)])!;

        var binding = Bind(
            nameof(MethodLookupBindingPatches.NestedShortTypeName_BindsNestedStaticMethod), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.memberInfo, Is.EqualTo(expectedMethod));
        });
    }

    [Test]
    public void NestedFullyQualifiedName_BindsNestedStaticMethod()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget.Nested), typeof(void),
            [typeof(MethodLookupTarget.Nested)], ["<instance>"], false);

        MethodInfo expectedMethod = typeof(MethodLookupTarget.Nested).GetMethod("StaticMethod", [typeof(int)])!;

        var binding = Bind(
            nameof(MethodLookupBindingPatches.NestedFullyQualifiedName_BindsNestedStaticMethod), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.memberInfo, Is.EqualTo(expectedMethod));
        });
    }

    [Test]
    public void NestedColonSyntax_BindsNestedStaticMethod()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget.Nested), typeof(void),
            [typeof(MethodLookupTarget.Nested)], ["<instance>"], false);

        MethodInfo expectedMethod = typeof(MethodLookupTarget.Nested).GetMethod("StaticMethod", [typeof(int)])!;

        var binding = Bind(
            nameof(MethodLookupBindingPatches.NestedColonSyntax_BindsNestedStaticMethod), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.memberInfo, Is.EqualTo(expectedMethod));
        });
    }

    [Test]
    public void DeepNestedColonSyntax_BindsDeepNestedStaticMethod()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget.Nested), typeof(void),
            [typeof(MethodLookupTarget.Nested)], ["<instance>"], false);

        MethodInfo expectedMethod = typeof(MethodLookupTarget.Nested.DeepNested).GetMethod("StaticMethod", [typeof(int)])!;

        var binding = Bind(
            nameof(MethodLookupBindingPatches.DeepNestedColonSyntax_BindsDeepNestedStaticMethod), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.memberInfo, Is.EqualTo(expectedMethod));
        });
    }

    [Test]
    public void DeepNestedDottedName_BindsDeepNestedStaticMethod()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget.Nested), typeof(void),
            [typeof(MethodLookupTarget.Nested)], ["<instance>"], false);

        MethodInfo expectedMethod = typeof(MethodLookupTarget.Nested.DeepNested).GetMethod("StaticMethod", [typeof(int)])!;

        var binding = Bind(
            nameof(MethodLookupBindingPatches.DeepNestedDottedName_BindsDeepNestedStaticMethod), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.memberInfo, Is.EqualTo(expectedMethod));
        });
    }

    [Test]
    public void UnqualifiedInheritedMethodOnNestedInstance_BindsBaseDeclaration()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget.Nested), typeof(void),
            [typeof(MethodLookupTarget.Nested)], ["<instance>"], false);

        MethodInfo expectedMethod = typeof(MethodLookupBase).GetMethod("Inherited", [typeof(int)])!;

        var binding = Bind(
            nameof(MethodLookupBindingPatches.UnqualifiedInheritedMethodOnNestedInstance_BindsBaseDeclaration), invocation);

        Assert.Multiple(() =>
        {
            Assert.That(binding.bindingType, Is.EqualTo(BindingType.Delegate));
            Assert.That(binding.memberInfo, Is.EqualTo(expectedMethod));
        });
    }

    [Test]
    public void DerivedOnlyMethodOnBaseInstance_ThrowsInstanceTypeMismatch()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget), typeof(void),
            [typeof(MethodLookupTarget)], ["<instance>"], false);

        Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(MethodLookupBindingPatches.DerivedOnlyMethodOnBaseInstance_ThrowsInstanceTypeMismatch), invocation));
    }

    [Test]
    public void UnrelatedInstanceMethod_ThrowsInstanceTypeMismatch()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget), typeof(void),
            [typeof(MethodLookupTarget)], ["<instance>"], false);

        Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(MethodLookupBindingPatches.UnrelatedInstanceMethod_ThrowsInstanceTypeMismatch), invocation));
    }

    [Test]
    public void OutDelegateForRefMethod_ThrowsMethodNotFound()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget), typeof(void),
            [typeof(MethodLookupTarget)], ["<instance>"], false);

        var exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(MethodLookupBindingPatches.OutDelegateForRefMethod_ThrowsMethodNotFound), invocation));
    }

    [Test]
    public void ExplicitNestedTypeWithInheritedMethod_ThrowsMethodNotFound()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget.Nested), typeof(void),
            [typeof(MethodLookupTarget.Nested)], ["<instance>"], false);

        var exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(MethodLookupBindingPatches.ExplicitNestedTypeWithInheritedMethod_ThrowsMethodNotFound), invocation));
    }

    [Test]
    public void ExplicitShortTypeWithInheritedMethod_ThrowsMethodNotFound()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget), typeof(void),
            [typeof(MethodLookupTarget)], ["<instance>"], false);

        var exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(MethodLookupBindingPatches.ExplicitShortTypeWithInheritedMethod_ThrowsMethodNotFound), invocation));
    }

    [Test]
    public void ExplicitFullyQualifiedTypeWithInheritedMethod_ThrowsMethodNotFound()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget), typeof(void),
            [typeof(MethodLookupTarget)], ["<instance>"], false);

        var exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(MethodLookupBindingPatches.ExplicitFullyQualifiedTypeWithInheritedMethod_ThrowsMethodNotFound), invocation));
    }

    [Test]
    public void ExplicitColonTypeWithInheritedMethod_ThrowsMethodNotFound()
    {
        var invocation = new MockInvocation(typeof(MethodLookupTarget), typeof(void),
            [typeof(MethodLookupTarget)], ["<instance>"], false);

        var exception = Assert.Throws<ParameterBindingException>(() =>
            Bind(nameof(MethodLookupBindingPatches.ExplicitColonTypeWithInheritedMethod_ThrowsMethodNotFound), invocation));
    }

    [Test]
    public void QualifiedStaticMethodOnStaticTarget_BindsMathAbsWithoutInstance()
    {
        var invocation = new MethodInvocation(typeof(MethodLookupTarget).GetMethod("StaticTarget")!);

        var binding = Bind(
            nameof(MethodLookupBindingPatches.QualifiedStaticMethodOnStaticTarget_BindsMathAbsWithoutInstance), invocation);

        Assert.That(binding.memberInfo, Is.EqualTo(typeof(Math).GetMethod("Abs", [typeof(int)])));
    }

    private static Disharmony.ParameterBinding Bind(string patchMethodName, Invocation invocation)
    {
        ParameterInfo parameter = typeof(MethodLookupBindingPatches)
            .GetMethod(patchMethodName)!.GetParameters().Single();
        var binder = new ParameterBinder(invocation, invocation, EmptyInvocation.Instance,
            PatchKind.Prefix, PatchOptions.Default, "test");

        return binder.Bind(parameter);
    }
}
