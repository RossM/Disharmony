using Disharmony.Tests.ReflectionFixtures;

namespace Disharmony.Tests.Unit;

[TestFixture]
public sealed class ReflectionTypeResolutionTests
{
    [Test]
    public void TopLevelAndNestedTypesCanBeSelectedByTheirFullNames()
    {
        Type top = typeof(ReflectionFixtures.Ambiguity.Target);
        Type nested = typeof(ReflectionFixtures.Ambiguity.Outer.Target);
        Assert.That(ReflectionTools.GetTypeByName(top.FullName!), Is.SameAs(top));
        Assert.That(ReflectionTools.GetTypeByName(nested.FullName!), Is.SameAs(nested));

        foreach (Type type in new[] { top, nested })
        {
            MemberInfo expected = type.GetMethod("Method")!;
            Assert.That(ReflectionTools.GetMember(null, type.AssemblyQualifiedName + ":Method",
                MemberType.Method, [typeof(int)], null), Is.EqualTo(expected));
            Assert.That(ReflectionTools.GetMember(type, "Method",
                MemberType.Method, [typeof(int)], null), Is.EqualTo(expected));
        }
    }

    [Test]
    public void NestedTypeNamesKeepContainingTypesWhenQualifiedByNamespace()
    {
        Type nested = typeof(LookupTarget.NestedTarget);
        string incomplete = nested.Namespace + "." + nested.Name;
        MethodInfo expected = nested.GetMethod("Method")!;

        Assert.That(ReflectionTools.GetTypeByName(incomplete), Is.Null);
        Assert.Throws<ReflectionException>(() => ReflectionTools.GetMember(null, incomplete + ".Method",
            MemberType.Method, [typeof(int)], null));
        Assert.That(ReflectionTools.GetTypeByName(nested.FullName!), Is.SameAs(nested));
        Assert.That(ReflectionTools.GetTypeByName(nested.Name), Is.SameAs(nested));
        foreach (string name in new[]
        {
            nested.FullName!.Replace('+', '.') + ".Method",
            nested.FullName + ".Method",
            "LookupTarget.NestedTarget.Method",
            "NestedTarget.Method",
        })
            Assert.That(ReflectionTools.GetMember(null, name, MemberType.Method, [typeof(int)], null),
                Is.EqualTo(expected), name);
    }

    [Test]
    public void ExactPrefixWinsOverAnEarlierShortAlias()
    {
        string prefix = "Prefix" + Guid.NewGuid().ToString("N");
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(prefix), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("Main");
        module.DefineType("Other." + prefix, TypeAttributes.Public).CreateType();
        var builder = module.DefineType(prefix + ".Target", TypeAttributes.Public);
        builder.DefineField("Value", typeof(int), FieldAttributes.Public);
        Type exact = builder.CreateType()!;

        Assert.That(ReflectionTools.GetMember(null, prefix + ".Target.Value",
            MemberType.Any, null, null).DeclaringType, Is.SameAs(exact));
    }

    [Test]
    public void CompleteDottedTypeNameWinsOverAShorterExactTypePrefix()
    {
        string prefix = "Complete" + Guid.NewGuid().ToString("N");
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(prefix), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("Main");
        module.DefineType(prefix, TypeAttributes.Public).CreateType();
        var builder = module.DefineType(prefix + ".Target", TypeAttributes.Public);
        builder.DefineField("Value", typeof(int), FieldAttributes.Public);
        Type exact = builder.CreateType()!;

        Assert.That(ReflectionTools.GetMember(null, prefix + ".Target.Value",
            MemberType.Any, null, null).DeclaringType, Is.SameAs(exact));
    }

    [Test]
    public void CompleteDottedNestedLocalNameSelectsTheContainingHierarchy()
    {
        Type target = typeof(ReflectionFixtures.Ambiguity.Outer.Target);
        MethodInfo expected = ReflectionFixtures.Ambiguity.Outer.Target.Container().Method;
        string name = target.FullName!.Replace('+', '.') + ".Container.Local";

        Assert.That(ReflectionTools.GetMember(null, name, MemberType.Method, null, null), Is.EqualTo(expected));
        Assert.That(ReflectionTools.GetMember(null, target.FullName + ".Container.Local",
            MemberType.Method, null, null), Is.EqualTo(expected));
    }

    [Test]
    public void PlusDisambiguatesNamespaceAndNestingBoundariesWithinOneAssembly()
    {
        string prefix = "Nesting" + Guid.NewGuid().ToString("N");
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(prefix), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("Main");
        var outer = module.DefineType(prefix + ".Outer", TypeAttributes.Public);
        var inner = outer.DefineNestedType("Inner", TypeAttributes.NestedPublic);
        inner.DefineField("Value", typeof(int), FieldAttributes.Public);
        outer.CreateType();
        Type nested = inner.CreateType()!;
        var builder = module.DefineType(prefix + ".Outer.Inner", TypeAttributes.Public);
        builder.DefineField("Value", typeof(int), FieldAttributes.Public);
        Type namespaced = builder.CreateType()!;

        Assert.That(ReflectionTools.GetMember(null, prefix + ".Outer.Inner.Value",
            MemberType.Any, null, null).DeclaringType, Is.SameAs(namespaced));
        Assert.That(ReflectionTools.GetMember(null, prefix + ".Outer+Inner.Value",
            MemberType.Any, null, null).DeclaringType, Is.SameAs(nested));
        Assert.That(ReflectionTools.GetMember(null, nested.AssemblyQualifiedName + ":Value",
            MemberType.Any, null, null).DeclaringType, Is.SameAs(nested));
    }

    [Test]
    public void AliasWinnerIsStableAcrossConcurrentRebuildsAndUnrelatedAssemblyLoads()
    {
        string name = "StableAlias" + Guid.NewGuid().ToString("N");
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("Main");
        Type later = module.DefineType("Z." + name, TypeAttributes.Public).CreateType()!;
        Type earlier = module.DefineType("A." + name, TypeAttributes.Public).CreateType()!;

        for (int rebuild = 0; rebuild < 4; rebuild++)
        {
            AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.Run)
                .DefineDynamicModule("Main").DefineType("Unrelated", TypeAttributes.Public).CreateType();
            var results = new Type?[32];
            System.Threading.Tasks.Parallel.For(0, results.Length,
                i => results[i] = ReflectionTools.GetTypeByName(name));
            Assert.That(results, Is.All.SameAs(earlier));
            Assert.That(ReflectionTools.GetTypeByName(later.FullName!), Is.SameAs(later));
        }
    }

    [Test]
    public void DuplicateFullNamesUseAssemblyIdentityOrderAndQualifiedStringsDisambiguate()
    {
        string suffix = Guid.NewGuid().ToString("N");
        string name = "Duplicate.Target" + suffix;
        var lastAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Z" + suffix), AssemblyBuilderAccess.Run);
        var lastBuilder = lastAssembly.DefineDynamicModule("Main").DefineType(name, TypeAttributes.Public);
        lastBuilder.DefineField("Value", typeof(int), FieldAttributes.Public);
        Type last = lastBuilder.CreateType()!;
        var firstAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("A" + suffix), AssemblyBuilderAccess.Run);
        var firstBuilder = firstAssembly.DefineDynamicModule("Main").DefineType(name, TypeAttributes.Public);
        firstBuilder.DefineField("Value", typeof(int), FieldAttributes.Public);
        Type first = firstBuilder.CreateType()!;

        for (int lookup = 0; lookup < 8; lookup++)
        {
            Assert.That(ReflectionTools.GetTypeByName(name), Is.SameAs(first));
            Assert.That(ReflectionTools.GetTypeByName(first.Name), Is.SameAs(first));
        }
        foreach (Type type in new[] { first, last })
        {
            Assert.That(ReflectionTools.GetMember(null, type.AssemblyQualifiedName + ":Value", MemberType.Any, null, null).DeclaringType,
                Is.SameAs(type));
            Assert.That(ReflectionTools.GetMember(null, type.FullName + ", " + type.Assembly.GetName().Name + ":Value",
                MemberType.Any, null, null).DeclaringType, Is.SameAs(type));
        }
    }

    [Test]
    public void ExactLookupSeesNewTypesInAnAlreadyLoadedDynamicAssemblyAfterAMiss()
    {
        string name = "Late.Target" + Guid.NewGuid().ToString("N");
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("Main");
        module.DefineType("Seed", TypeAttributes.Public).CreateType();
        Assert.That(ReflectionTools.GetTypeByName(name), Is.Null);
        Type target = module.DefineType(name, TypeAttributes.Public).CreateType()!;

        Assert.That(ReflectionTools.GetTypeByName(name), Is.SameAs(target));
    }

    [Test]
    public void AssemblyQualifiedNamesSupportNestedLocalFunctionsAndOverloadFilters()
    {
        Type target = typeof(LookupTarget);
        MethodInfo expected = LookupTarget.CapturedLocalMethodContainer(42).Method;
        Assert.That(ReflectionTools.GetMember(null,
            target.AssemblyQualifiedName + ":CapturedLocalMethodContainer.CapturedLocalMethod",
            MemberType.Method, null, null), Is.EqualTo(expected));
        Assert.That(ReflectionTools.GetMember(null, target.AssemblyQualifiedName + ":OverloadedMethod",
            MemberType.Method, [typeof(string)], null),
            Is.EqualTo(target.GetMethod("OverloadedMethod", [typeof(string)])));
    }
}
