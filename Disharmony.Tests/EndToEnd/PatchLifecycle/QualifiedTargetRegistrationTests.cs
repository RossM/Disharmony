namespace Disharmony.Tests.EndToEnd.PatchLifecycle;

[TestFixture]
public sealed class QualifiedTargetRegistrationTests : PatchTestBase
{
    [TestCase("qualified-target-colon", typeof(TargetAttribute), "Disharmony.Tests.StaticMethodTargets:RegistrationResultA")]
    [TestCase("qualified-target-dot", typeof(TargetAttribute), "Disharmony.Tests.StaticMethodTargets.RegistrationResultA")]
    [TestCase("qualified-targets-colon", typeof(TargetsAttribute), "Disharmony.Tests.StaticMethodTargets:RegistrationResultA")]
    [TestCase("qualified-targets-dot", typeof(TargetsAttribute), "Disharmony.Tests.StaticMethodTargets.RegistrationResultA")]
    public void PatchCategoryResolvesQualifiedNameWithoutDefaultType(string category, Type attributeType, string qualifiedName)
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName("QualifiedCategoryPatches_" + Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.Run);
        var type = assembly.DefineDynamicModule("Patches").DefineType("QualifiedPatches",
            TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
        // Neither Patch nor Target/Targets supplies a default declaring type.
        type.SetCustomAttribute(new CustomAttributeBuilder(
            typeof(PatchAttribute).GetConstructor([typeof(Type)])!, new object[] { null! }));
        type.SetCustomAttribute(new CustomAttributeBuilder(
            typeof(CategoryAttribute).GetConstructor([typeof(string)])!, new object[] { category }));
        var postfix = type.DefineMethod("Postfix", MethodAttributes.Public | MethodAttributes.Static,
            typeof(void), [typeof(int).MakeByRefType()]);
        postfix.DefineParameter(1, ParameterAttributes.None, "__result");
        postfix.SetCustomAttribute(new CustomAttributeBuilder(typeof(PostfixAttribute).GetConstructor(Type.EmptyTypes)!, []));
        postfix.SetCustomAttribute(new CustomAttributeBuilder(
            attributeType.GetConstructor([typeof(string)])!, new object[] { qualifiedName }));
        ILGenerator il = postfix.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldc_I4, 42);
        il.Emit(OpCodes.Stind_I4);
        il.Emit(OpCodes.Ret);
        type.CreateType();

        Patcher.PatchCategory(assembly, category);

        Assert.That(StaticMethodTargets.RegistrationResultA(), Is.EqualTo(42));
    }

    [Test]
    public void PatchAllResolvesQualifiedNamesWithoutDefaultTypes()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName("QualifiedPatchAllPatches"), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("Patches");
        // Separate targets ensure that each qualified-name form must resolve and apply independently.
        (string ClassName, Type AttributeType, string QualifiedName)[] containers =
        [
            ("TargetColon", typeof(TargetAttribute), "Disharmony.Tests.StaticMethodTargets:RegistrationResultA"),
            ("TargetDot", typeof(TargetAttribute), "Disharmony.Tests.StaticMethodTargets.RegistrationResultB"),
            ("TargetsColon", typeof(TargetsAttribute), "Disharmony.Tests.StaticMethodTargets:IntResult"),
            ("TargetsDot", typeof(TargetsAttribute), "Disharmony.Tests.StaticMethodTargets.IntIdentity"),
        ];
        foreach (var container in containers)
        {
            var type = module.DefineType(container.ClassName,
                TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
            type.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(PatchAttribute).GetConstructor([typeof(Type)])!, new object[] { null! }));
            var postfix = type.DefineMethod("Postfix", MethodAttributes.Public | MethodAttributes.Static,
                typeof(void), [typeof(int).MakeByRefType()]);
            postfix.DefineParameter(1, ParameterAttributes.None, "__result");
            postfix.SetCustomAttribute(new CustomAttributeBuilder(typeof(PostfixAttribute).GetConstructor(Type.EmptyTypes)!, []));
            postfix.SetCustomAttribute(new CustomAttributeBuilder(
                container.AttributeType.GetConstructor([typeof(string)])!, new object[] { container.QualifiedName }));
            ILGenerator il = postfix.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4, 42);
            il.Emit(OpCodes.Stind_I4);
            il.Emit(OpCodes.Ret);
            type.CreateType();
        }

        Patcher.PatchAll(assembly);

        Assert.That(StaticMethodTargets.RegistrationResultA(), Is.EqualTo(42));
        Assert.That(StaticMethodTargets.RegistrationResultB(), Is.EqualTo(42));
        Assert.That(StaticMethodTargets.IntResult(), Is.EqualTo(42));
        Assert.That(StaticMethodTargets.IntIdentity(1), Is.EqualTo(42));
    }
}
