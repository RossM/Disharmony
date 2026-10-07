using System.Threading;

namespace Disharmony;

internal static class ReflectionTools
{
    // Each cached assembly list and alias index belongs to one generation. Loads replace it instead of
    // clearing fields that a concurrent index build might subsequently overwrite.
    private sealed class TypeLookup
    {
        internal readonly Lazy<Assembly[]> Assemblies = new(() =>
            [.. AccessTools.AllAssemblies().OrderBy(a => a.FullName, StringComparer.Ordinal)]);

        internal readonly Lazy<Dictionary<string, Type>> Aliases;

        internal TypeLookup() => Aliases = new(BuildAliases);

        private Dictionary<string, Type> BuildAliases()
        {
            // Preserve assembly order through PLINQ; worker completion order must not
            // select the winner of an ambiguous alias. FullName is read only on collisions.
            Type[] types = [.. Assemblies.Value.AsParallel().AsOrdered().SelectMany(AccessTools.GetTypesFromAssembly)];
            Dictionary<string, Type> aliases = new(StringComparer.Ordinal);
            foreach (Type type in types)
            {
                string name = type.Name;
                if (!aliases.TryGetValue(name, out Type? previous) || ComesBefore(type, previous))
                    aliases[name] = type;
            }

            return aliases;
        }

        internal Assembly? ResolveAssembly(AssemblyName requested)
        {
            foreach (Assembly assembly in Assemblies.Value)
            {
                AssemblyName actual = assembly.GetName();
                if (!StringComparer.OrdinalIgnoreCase.Equals(requested.Name, actual.Name))
                    continue;
                if (requested.Version is not null && requested.Version != actual.Version)
                    continue;
                if (requested.CultureName is not null &&
                    !StringComparer.OrdinalIgnoreCase.Equals(requested.CultureName, actual.CultureName))
                    continue;
                if (requested.GetPublicKeyToken() is { } token &&
                    !token.SequenceEqual(actual.GetPublicKeyToken() ?? []))
                    continue;
                return assembly;
            }

            return null;
        }

        private static bool ComesBefore(Type candidate, Type previous)
        {
            int assemblyOrder = StringComparer.Ordinal.Compare(candidate.Assembly.FullName, previous.Assembly.FullName);
            return assemblyOrder < 0 || (assemblyOrder == 0 &&
                                         StringComparer.Ordinal.Compare(candidate.FullName, previous.FullName) < 0);
        }
    }

    private static readonly BindingFlags DeclaredOnly = AccessTools.all | BindingFlags.DeclaredOnly;

    private static TypeLookup _typeLookup = new();

    static ReflectionTools()
    {
        AppDomain.CurrentDomain.AssemblyLoad += AssemblyLoadHandler;
    }

    private static void AssemblyLoadHandler(object? sender, AssemblyLoadEventArgs args)
    {
        Interlocked.Exchange(ref _typeLookup, new TypeLookup());
    }

    private static Type WrappedType(ParameterInfo parameter)
    {
        Type parameterType = parameter.ParameterType;
        if (!parameterType.IsByRef)
            return parameterType;
        Type marker = parameter.IsOut ? typeof(Out<>) : parameter.IsIn ? typeof(In<>) : typeof(Ref<>);
        return marker.MakeGenericType(parameterType.GetElementType()!);
    }

    public static Type[] WrapParameterTypes(MethodBase method) => [.. method.GetParameters().Select(WrappedType)];

    public static MemberInfo GetMember(
        Type? type,
        string? name,
        MemberType memberType,
        Type[]? parameterTypes,
        Type[]? genericTypes,
        bool searchBaseTypes = false)
    {
        var candidates = GetMembers(type, name, memberType, parameterTypes, genericTypes, searchBaseTypes);

        switch (candidates.Count)
        {
            case > 1: throw new AmbiguousMatchException($"Ambiguous match: {name}");
            case 0: throw new ReflectionException($"Member not found: {name}");
        }

        var result = candidates.Single();
        return result;
    }

    public static IReadOnlyList<MemberInfo> GetMembers(
        Type? type,
        string? name,
        MemberType memberType,
        Type[]? parameterTypes,
        Type[]? genericTypes,
        bool searchBaseTypes = false)
    {
        if (name is null && memberType is not MemberType.Constructor)
            throw new ArgumentException("name expected");

        if (parameterTypes != null && memberType is not (MemberType.Any or MemberType.Method or MemberType.Constructor))
            throw new ArgumentException($"parameterTypes is not supported for memberType {memberType}");

        if (genericTypes != null && memberType is not (MemberType.Any or MemberType.Method))
            throw new ArgumentException($"genericTypes is not supported for memberType {memberType}");

        // Resolve explicit types first, then exact type prefixes before a short type alias,
        // then nested types and members/local functions. A declared first segment suppresses
        // global type lookup even when it is not a method (including nested types).
        int colon = name?.IndexOf(':') ?? -1;
        if (colon >= 0)
        {
            string explicitType = name!.Substring(0, colon);
            type = GetTypeByName(explicitType) ??
                   throw new ReflectionException($"Type not found: {explicitType}");
            name = name.Substring(colon + 1);
            searchBaseTypes = false;
        }

        int firstDot = name?.IndexOf('.') ?? -1;
        int memberStart = 0;
        if (firstDot >= 0)
        {
            string firstPart = name!.Substring(0, firstDot);
            if (type?.GetMember(firstPart, DeclaredOnly).Any(m => m.Name == firstPart) is not true &&
                FindTypePrefix(name) is { } resolved)
            {
                type = resolved.Type;
                memberStart = resolved.MemberStart;
                searchBaseTypes = false;
            }
        }

        if (type is null)
            throw new ReflectionException($"type not found: {name}");

        if (name is not null)
            for (int dot = name.IndexOf('.', memberStart); dot >= 0; dot = name.IndexOf('.', memberStart))
            {
                Type? nested = type.GetNestedType(name.Substring(memberStart, dot - memberStart), AccessTools.all);
                if (nested is null)
                    break;
                type = nested;
                memberStart = dot + 1;
            }

        var nameParts = name is null ? new List<string>() : name.Substring(memberStart).Split('.').ToList();

        return GetResults(type, nameParts, memberType, parameterTypes, genericTypes, searchBaseTypes);
    }

    private static (Type Type, int MemberStart)? FindTypePrefix(string name)
    {
        // Prefer the most completely specified runtime type. A shorter exact type
        // or alias must not hide a longer namespace-qualified type prefix.
        for (int dot = name.LastIndexOf('.'); dot >= 0; dot = dot == 0 ? -1 : name.LastIndexOf('.', dot - 1))
        {
            if (GetExactTypeByName(name.Substring(0, dot)) is { } type)
                return (type, dot + 1);
        }

        int firstDot = name.IndexOf('.');
        if (firstDot >= 0 && GetTypeByAlias(name.Substring(0, firstDot)) is { } shortType)
            return (shortType, firstDot + 1);
        return null;
    }

    private static IReadOnlyList<MemberInfo> GetResults(
        Type type,
        List<string> nameParts,
        MemberType memberType,
        Type[]? parameterTypes,
        Type[]? genericTypes,
        bool searchBaseTypes = false)
    {
        IEnumerable<MemberInfo> candidates = nameParts.Count switch
        {
            0 => type.GetConstructors(),
            1 => type.GetMember(nameParts[0], DeclaredOnly),
            2 => GetLocalMethods(type, nameParts[1] == "*"
                ? $"<{nameParts[0]}>b__"
                : $"<{nameParts[0]}>g__{nameParts[1]}|"),
            _ => throw new NotSupportedException("Nested local functions are not supported"),
        };

        if (candidates is ICollection<MemberInfo> { Count: 0 })
        {
            if (searchBaseTypes && type.BaseType is { } baseType)
                return GetResults(baseType, nameParts, memberType, parameterTypes, genericTypes, true);
            return [];
        }

        MemberTypes kinds = memberType switch
        {
            MemberType.Any => MemberTypes.Method | MemberTypes.Field | MemberTypes.Property,
            MemberType.Method => MemberTypes.Method,
            MemberType.Getter or MemberType.Setter => MemberTypes.Field | MemberTypes.Property,
            MemberType.Constructor => MemberTypes.Constructor,
            _ => throw new ArgumentOutOfRangeException(nameof(memberType), memberType, null),
        };

        List<MemberInfo> matches = [];
        foreach (MemberInfo candidate in candidates)
            if ((candidate.MemberType & kinds) != 0)
                matches.Add(candidate);

        // Name/kind shadowing precedes signature filtering and accessor conversion.
        if (matches.Count == 0 && searchBaseTypes && type.BaseType is { } baseType2)
            return GetResults(baseType2, nameParts, memberType, parameterTypes, genericTypes, true);

        if (parameterTypes is not null || genericTypes is not null)
            return [.. FilterMethods(matches, parameterTypes, genericTypes)];

        for (int i = matches.Count - 1; i >= 0; i--)
        {
            if (matches[i] is PropertyInfo property)
            {
                MethodInfo? accessor = memberType == MemberType.Setter ? property.SetMethod : property.GetMethod;
                if (accessor is null)
                    matches.RemoveAt(i);
                else
                    matches[i] = accessor;
            }
        }

        return matches;
    }

    private static IEnumerable<MethodInfo> GetLocalMethods(Type type, string prefix)
    {
        foreach (Type nested in type.GetNestedTypes(DeclaredOnly))
        {
            if (nested.IsClosureType)
                foreach (MethodInfo method in nested.GetMethods(DeclaredOnly))
                    if (method.Name.StartsWith(prefix, StringComparison.Ordinal))
                        yield return method;
        }

        foreach (MethodInfo method in type.GetMethods(DeclaredOnly))
            if (method.Name.StartsWith(prefix, StringComparison.Ordinal))
                yield return method;
    }

    public static Type? GetTypeByName(string name) => GetExactTypeByName(name) ?? GetTypeByAlias(name);

    private static Type? GetExactTypeByName(string name)
    {
        if (Type.GetType(name, throwOnError: false) is { } runtimeType)
            return runtimeType;
        TypeLookup lookup = Volatile.Read(ref _typeLookup);
        // The normal binder cannot find every loaded assembly (notably emitted
        // assemblies). Let the runtime parse qualified/generic names with our resolver.
        if (name.IndexOf(',') >= 0 &&
            Type.GetType(name, lookup.ResolveAssembly, null, throwOnError: false) is { } qualifiedType)
            return qualifiedType;
        foreach (Assembly assembly in lookup.Assemblies.Value)
            if (assembly.GetType(name, throwOnError: false) is { } type)
                return type;
        return null;
    }

    private static Type? GetTypeByAlias(string name)
    {
        if (name.IndexOf('.') >= 0)
            return null;
        TypeLookup lookup = Volatile.Read(ref _typeLookup);
        return lookup.Aliases.Value.TryGetValue(name, out Type? type) ? type : null;
    }

    private static IEnumerable<MethodBase> FilterMethods(IEnumerable<MemberInfo> candidates, Type[]? parameterTypes, Type[]? genericTypes)
    {
        foreach (var candidate in candidates)
        {
            if (candidate is not MethodBase method)
                continue;

            if (method.IsGenericMethod)
            {
                if (method is not MethodInfo methodInfo)
                    throw new NotSupportedException();

                if (genericTypes is null)
                    continue;
                if (genericTypes.Length != method.GetGenericArguments().Length)
                    continue;

                try
                {
                    method = methodInfo.MakeGenericMethod(genericTypes);
                }
                catch
                {
                    continue;
                }
            }
            else if (genericTypes is not null)
            {
                continue;
            }

            if (parameterTypes != null)
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length != parameterTypes.Length)
                    continue;
                bool matches = true;
                for (int i = 0; i < parameters.Length; i++)
                    if (!ParameterTypeMatcher(parameters[i], parameterTypes[i]))
                    {
                        matches = false;
                        break;
                    }

                if (!matches)
                    continue;
            }

            yield return method;
            continue;

            static bool ParameterTypeMatcher(ParameterInfo parameter, Type type)
            {
                if (parameter.IsOut)
                    return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Out<>) &&
                           type.GetGenericArguments()[0] == parameter.ParameterType.GetElementType();

                if (parameter.IsIn)
                    return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(In<>) &&
                           type.GetGenericArguments()[0] == parameter.ParameterType.GetElementType();

                if (parameter.ParameterType.IsByRef)
                    return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Ref<>) &&
                           type.GetGenericArguments()[0] == parameter.ParameterType.GetElementType();

                return parameter.ParameterType == type;
            }
        }
    }

    internal static int ILSize(OpCode opCode)
    {
        int size = opCode.Size;
        size += opCode.OperandType switch
        {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget => 1,
            OperandType.ShortInlineI => 1,
            OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineI8 => 8,
            OperandType.InlineR => 8,
            _ => 4,
        };
        return size;
    }
}
