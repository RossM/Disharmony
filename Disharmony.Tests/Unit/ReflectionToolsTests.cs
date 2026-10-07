using Disharmony.Tests.ReflectionFixtures;

// ReSharper disable UnassignedField.Global
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Disharmony.Tests.Unit;

[TestFixture]
public sealed class ReflectionToolsTests
{
    [Test]
    public void GetMemberFindsNestedMethodFromFullyQualifiedName()
    {
        const string name = "Disharmony.Tests.ReflectionFixtures.LookupTarget.NestedTarget.Method";
        MethodInfo expected = typeof(LookupTarget.NestedTarget).GetMethod(nameof(LookupTarget.NestedTarget.Method))!;

        AssertAllMethodOptions(null, name, expected);
    }

    [Test]
    public void GetMemberFindsNestedMethodRelativeToDeclaringType()
    {
        const string name = "NestedTarget.Method";
        MethodInfo expected = typeof(LookupTarget.NestedTarget).GetMethod(nameof(LookupTarget.NestedTarget.Method))!;

        AssertAllMethodOptions(typeof(LookupTarget), name, expected);
    }

    [Test]
    public void GetMemberFindsNestedMethodFromNestedType()
    {
        const string name = nameof(LookupTarget.NestedTarget.Method);
        MethodInfo expected = typeof(LookupTarget.NestedTarget).GetMethod(nameof(LookupTarget.NestedTarget.Method))!;

        AssertAllMethodOptions(typeof(LookupTarget.NestedTarget), name, expected);
    }

    [Test]
    public void GetMemberFindsNonNestedMethodFromFullyQualifiedName()
    {
        const string name = "Disharmony.Tests.ReflectionFixtures.LookupTarget.Method";
        MethodInfo expected = typeof(LookupTarget).GetMethod(nameof(LookupTarget.Method))!;

        AssertAllMethodOptions(null, name, expected);
    }

    [Test]
    public void GetMemberFindsNonNestedMethoHarmonyCompatible()
    {
        const string name = "Disharmony.Tests.ReflectionFixtures.LookupTarget:Method";
        MethodInfo expected = typeof(LookupTarget).GetMethod(nameof(LookupTarget.Method))!;

        AssertAllMethodOptions(null, name, expected);
    }

    [Test]
    public void GetMemberFindsNonNestedMethodFromDeclaringType()
    {
        const string name = nameof(LookupTarget.Method);
        MethodInfo expected = typeof(LookupTarget).GetMethod(nameof(LookupTarget.Method))!;

        AssertAllMethodOptions(typeof(LookupTarget), name, expected);
    }

    [Test]
    public void GetMemberMatchesRefParameterWithRefMarker()
    {
        MethodInfo expected = typeof(LookupTarget).GetMethod(nameof(LookupTarget.RefMethod))!;

        MemberInfo actual = ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.RefMethod),
            MemberType.Method,
            [typeof(Ref<int>)],
            null);

        Assert.That(actual, Is.SameAs(expected));
    }

    [Test]
    public void GetMemberMatchesInParameterWithInMarker()
    {
        MethodInfo expected = typeof(LookupTarget).GetMethod(nameof(LookupTarget.InMethod))!;

        MemberInfo actual = ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.InMethod),
            MemberType.Method,
            [typeof(In<int>)],
            null);

        Assert.That(actual, Is.SameAs(expected));
    }

    [Test]
    public void GetMemberMatchesOutParameterWithOutMarker()
    {
        MethodInfo expected = typeof(LookupTarget).GetMethod(nameof(LookupTarget.OutMethod))!;

        MemberInfo actual = ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.OutMethod),
            MemberType.Method,
            [typeof(Out<int>)],
            null);

        Assert.That(actual, Is.SameAs(expected));
    }

    [Test]
    public void GetMemberReturnsPropertyGetterForAnyMemberType()
    {
        MethodInfo expected = typeof(LookupTarget).GetProperty(nameof(LookupTarget.Property))!.GetMethod!;

        MemberInfo actual = ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.Property),
            MemberType.Any,
            null,
            null);

        Assert.That(actual, Is.SameAs(expected));
    }

    [Test]
    public void GetMemberReturnsPropertyGetterForGetterMemberType()
    {
        MethodInfo expected = typeof(LookupTarget).GetProperty(nameof(LookupTarget.Property))!.GetMethod!;

        MemberInfo actual = ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.Property),
            MemberType.Getter,
            null,
            null);

        Assert.That(actual, Is.SameAs(expected));
    }

    [Test]
    public void GetMemberReturnsPropertySetterForSetterMemberType()
    {
        MethodInfo expected = typeof(LookupTarget).GetProperty(nameof(LookupTarget.Property))!.SetMethod!;

        MemberInfo actual = ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.Property),
            MemberType.Setter,
            null,
            null);

        Assert.That(actual, Is.SameAs(expected));
    }

    [Test]
    public void GetMemberReturnsFieldForAnyMemberType()
    {
        FieldInfo expected = typeof(LookupTarget).GetField(nameof(LookupTarget.Field))!;

        MemberInfo actual = ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.Field),
            MemberType.Any,
            null,
            null);

        Assert.That(actual, Is.SameAs(expected));
    }

    [Test]
    public void GetMemberReturnsFieldForGetterMemberType()
    {
        FieldInfo expected = typeof(LookupTarget).GetField(nameof(LookupTarget.Field))!;

        MemberInfo actual = ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.Field),
            MemberType.Getter,
            null,
            null);

        Assert.That(actual, Is.SameAs(expected));
    }

    [Test]
    public void GetMemberUsesParameterTypesToSelectIntOverload()
    {
        MethodInfo expected = typeof(LookupTarget).GetMethod(
            nameof(LookupTarget.OverloadedMethod),
            [typeof(int)])!;

        MemberInfo actual = ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.OverloadedMethod),
            MemberType.Method,
            [typeof(int)],
            null);

        Assert.That(actual, Is.SameAs(expected));
    }

    [Test]
    public void GetMemberUsesParameterTypesToSelectStringOverload()
    {
        MethodInfo expected = typeof(LookupTarget).GetMethod(
            nameof(LookupTarget.OverloadedMethod),
            [typeof(string)])!;

        MemberInfo actual = ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.OverloadedMethod),
            MemberType.Method,
            [typeof(string)],
            null);

        Assert.That(actual, Is.SameAs(expected));
    }

    [Test]
    public void GetMemberThrowsAmbiguousMatchExceptionForOverloadWithoutParameterTypes()
    {
        Assert.Throws<AmbiguousMatchException>(() => ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.OverloadedMethod),
            MemberType.Method,
            null,
            null));
    }

    [Test]
    public void GetMemberReturnsGenericMethodDefinitionWithoutTypeFilters()
    {
        MethodInfo expected = typeof(LookupTarget).GetMethod(nameof(LookupTarget.GenericMethod))!;

        MemberInfo actual = ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.GenericMethod),
            MemberType.Method,
            null,
            null);

        Assert.That(actual, Is.SameAs(expected));
        Assert.That(((MethodInfo)actual).IsGenericMethodDefinition, Is.True);
    }

    [Test]
    public void GetMemberDoesNotMatchGenericMethodUsingOnlyConstructedParameterTypes()
    {
        Assert.Throws<ReflectionException>(() => ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.GenericMethod),
            MemberType.Method,
            [typeof(int)],
            null));
    }

    [Test]
    public void GetMemberConstructsGenericMethodUsingGenericTypes()
    {
        MethodInfo expected = typeof(LookupTarget).GetMethod(nameof(LookupTarget.GenericMethod))!
            .MakeGenericMethod(typeof(int));

        MemberInfo actual = ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.GenericMethod),
            MemberType.Method,
            null,
            [typeof(int)]);

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void GetMemberConstructsGenericMethodUsingGenericAndParameterTypes()
    {
        MethodInfo expected = typeof(LookupTarget).GetMethod(nameof(LookupTarget.GenericMethod))!
            .MakeGenericMethod(typeof(int));

        MemberInfo actual = ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.GenericMethod),
            MemberType.Method,
            [typeof(int)],
            [typeof(int)]);

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void GetMemberDoesNotMatchGenericMethodWithWrongGenericArity()
    {
        Assert.Throws<ReflectionException>(() => ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.GenericMethod),
            MemberType.Method,
            null,
            [typeof(int), typeof(string)]));
    }

    [Test]
    public void GetMemberDoesNotMatchNonGenericMethodWhenGenericTypesAreProvided()
    {
        Assert.Throws<ReflectionException>(() => ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.NonGenericMethod),
            MemberType.Method,
            null,
            [typeof(int)]));
    }

    [Test]
    public void GetMemberSelectsNonGenericOverloadWhenOnlyParameterTypesAreProvided()
    {
        MethodInfo expected = Array.Find(
            typeof(LookupTarget).GetMethods(),
            method => method.Name == nameof(LookupTarget.MixedMethod) && !method.IsGenericMethod)!;

        MemberInfo actual = ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.MixedMethod),
            MemberType.Method,
            [typeof(int)],
            null);

        Assert.That(actual, Is.SameAs(expected));
    }

    [Test]
    public void GetMemberSelectsGenericOverloadWhenGenericTypesAreProvided()
    {
        MethodInfo definition = Array.Find(
            typeof(LookupTarget).GetMethods(),
            method => method.Name == nameof(LookupTarget.MixedMethod) && method.IsGenericMethodDefinition)!;
        MethodInfo expected = definition.MakeGenericMethod(typeof(int));

        MemberInfo actual = ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.MixedMethod),
            MemberType.Method,
            null,
            [typeof(int)]);

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void GetMemberThrowsAmbiguousMatchExceptionForMixedOverloadsWithoutTypeFilters()
    {
        Assert.Throws<AmbiguousMatchException>(() => ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.MixedMethod),
            MemberType.Method,
            null,
            null));
    }

    [Test]
    public void GetMemberFindsStaticLocalMethodOnContainingType()
    {
        MethodInfo expected = LookupTarget.StaticLocalMethodContainer().Method;

        MemberInfo actual = ReflectionTools.GetMember(
            typeof(LookupTarget),
            "StaticLocalMethodContainer.StaticLocalMethod",
            MemberType.Method,
            null,
            null);

        Assert.That(actual, Is.SameAs(expected));
    }

    [Test]
    public void GetMemberFindsCapturedLocalMethodOnClosureType()
    {
        MethodInfo expected = LookupTarget.CapturedLocalMethodContainer(42).Method;

        MemberInfo actual = ReflectionTools.GetMember(
            typeof(LookupTarget),
            "CapturedLocalMethodContainer.CapturedLocalMethod",
            MemberType.Any,
            null,
            null);

        Assert.That(actual, Is.SameAs(expected));
    }

    [Test]
    public void GetMemberFindsLambdaUsingWildcardLocalName()
    {
        MethodInfo expected = LookupTarget.LambdaContainer().Method;

        MemberInfo actual = ReflectionTools.GetMember(
            typeof(LookupTarget),
            "LambdaContainer.*",
            MemberType.Method,
            null,
            null);

        Assert.That(actual, Is.SameAs(expected));
    }

    [Test]
    public void GetMembersReturnsEveryMatchingOverload()
    {
        var actual = ReflectionTools.GetMembers(
            typeof(LookupTarget),
            nameof(LookupTarget.OverloadedMethod),
            MemberType.Method,
            null,
            null);

        Assert.That(actual, Has.Count.EqualTo(2));
        Assert.That(actual, Has.All.InstanceOf<MethodInfo>());
    }

    [Test]
    public void GetMemberReturnsFieldForSetterMemberType()
    {
        FieldInfo expected = typeof(LookupTarget).GetField(nameof(LookupTarget.Field))!;

        MemberInfo actual = ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.Field),
            MemberType.Setter,
            null,
            null);

        Assert.That(actual, Is.SameAs(expected));
    }

    [Test]
    public void GetMemberReportsMissingGetterForWriteOnlyProperty()
    {
        Assert.Throws<ReflectionException>(() => ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.WriteOnlyProperty),
            MemberType.Getter,
            null,
            null));
    }

    [Test]
    public void GetMemberReportsMissingSetterForReadOnlyProperty()
    {
        Assert.Throws<ReflectionException>(() => ReflectionTools.GetMember(
            typeof(LookupTarget),
            nameof(LookupTarget.ReadOnlyProperty),
            MemberType.Setter,
            null,
            null));
    }

    private static void AssertAllMethodOptions(Type? type, string name, MethodInfo expected)
    {
        AssertLookup(type, name, MemberType.Any, null, expected);
        AssertLookup(type, name, MemberType.Any, [typeof(int)], expected);
        AssertLookup(type, name, MemberType.Method, null, expected);
        AssertLookup(type, name, MemberType.Method, [typeof(int)], expected);
    }

    private static void AssertLookup(
        Type? type,
        string name,
        MemberType memberType,
        Type[]? parameterTypes,
        MethodInfo expected)
    {
        MemberInfo actual = ReflectionTools.GetMember(type, name, memberType, parameterTypes, null);

        Assert.That(
            actual,
            Is.SameAs(expected),
            $"Lookup failed for memberType={memberType}, parameterTypes={FormatParameterTypes(parameterTypes)}");
    }

    private static string FormatParameterTypes(Type[]? parameterTypes) =>
        parameterTypes is null ? "null" : "[typeof(int)]";
}
