using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Disharmony.Analyzers;

internal static class Helpers
{
    internal static int GetPatchOptions(IMethodSymbol method, INamedTypeSymbol? options)
    {
        var attribute = FindAttribute(method, options) ?? FindAttribute(method.ContainingType, options);
        return attribute is not null && Argument(attribute, "options")?.Value is int value ? value : 0;
    }

    internal static AttributeData? FindAttribute(ISymbol symbol, params INamedTypeSymbol?[] types) =>
        GetAttributes(symbol).FirstOrDefault(a => IsAttribute(a, types));

    // All supported attributes are inherited. Only target selectors and HarmonyPatch allow multiples.
    internal static IEnumerable<AttributeData> GetAttributes(ISymbol symbol)
    {
        var nearerTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        bool inherited = false;
        for (ISymbol? current = symbol;
             current is not null;
             current = current switch
             {
                 IMethodSymbol method => method.OverriddenMethod,
                 INamedTypeSymbol type => type.BaseType,
                 _ => null,
             })
        {
            var attributes = current.GetAttributes();
            foreach (var attribute in attributes)
            {
                if (attribute.AttributeClass is not { } type)
                    continue;
                if (!inherited || !nearerTypes.Contains(type) || AllowsMultiple(type))
                    yield return attribute;
            }

            foreach (var attribute in attributes)
            {
                if (attribute.AttributeClass is { } type)
                    nearerTypes.Add(type);
            }

            inherited = true;
        }
    }

    private static bool AllowsMultiple(INamedTypeSymbol type) =>
        type.ToDisplayString() is "Disharmony.TargetAttribute" or "Disharmony.TargetsAttribute" or "HarmonyLib.HarmonyPatch";

    internal static bool IsAttribute(AttributeData attribute, params INamedTypeSymbol?[] types) =>
        types.Any(type => type is not null && SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, type));

    internal static TypedConstant? Argument(AttributeData? attribute, string name)
    {
        if (attribute?.AttributeConstructor?.Parameters is not { } parameters)
            return null;
        for (int i = 0; i < parameters.Length && i < attribute.ConstructorArguments.Length; i++)
        {
            if (parameters[i].Name == name)
                return attribute.ConstructorArguments[i];
        }

        return null;
    }

    public static Location? GetLocation(ISymbol type)
    {
        return type.Locations.FirstOrDefault(l => l.IsInSource);
    }

    public static bool HasGenericParameters(IMethodSymbol method)
    {
        if (method.Arity != 0)
            return true;
        for (var type = method.ContainingType; type is not null; type = type.ContainingType)
        {
            if (type.Arity != 0)
                return true;
        }

        return false;
    }

    public static bool HasNoTypeOrQualifiedName(AttributeData selector)
    {
        if (Argument(selector, "type") is { IsNull: false })
            return false;
        if ((Argument(selector, "methodName") ?? Argument(selector, "memberName"))?.Value is not string name)
            return true;
        // Both Type:Member and Namespace.Type.Member are resolved using loaded assemblies at runtime.
        return name.IndexOf(':') < 0 && name.IndexOf('.') < 0;
    }

    public static Location SelectorLocation(AttributeData attribute, Location fallback) =>
        attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? fallback;

    public static bool CanBindKnownType(CSharpCompilation compilation, IParameterSymbol parameter, ITypeSymbol source, bool allowUnsafe)
    {
        if (allowUnsafe && !parameter.Type.IsValueType && !source.IsValueType)
            return true;
        var destination = parameter.Type;
        if ((parameter.RefKind != RefKind.None && source.IsValueType) || parameter.RefKind == RefKind.Ref)
            return compilation.ClassifyConversion(source, destination).IsIdentity;
        if (parameter.RefKind == RefKind.Out)
            (source, destination) = (destination, source);
        var conversion = compilation.ClassifyConversion(source, destination);
        // Type.IsAssignableFrom permits identity, reference conversions and boxing, but not numeric
        // conversions or user-defined operators. 'in' reads; 'out' writes in the opposite direction.
        return conversion.IsIdentity || (conversion.IsImplicit && (conversion.IsReference || conversion.IsBoxing));
    }

    public static bool HasAttribute(ISymbol method, params INamedTypeSymbol?[] types) => FindAttribute(method, types) is not null;
}
