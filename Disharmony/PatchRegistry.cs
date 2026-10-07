using Disharmony.RuleBuilders;

namespace Disharmony;

internal class ParameterBinding
{
    /// <summary>
    ///     The <see cref="ParameterInfo" /> of the parameter from the patch function.
    /// </summary>
    /// <remarks>
    ///     This is used to get the parameter's type, as well as its name for logging.
    /// </remarks>
    public required ParameterInfo parameter;

    /// <summary>
    ///     Whether this applies to the outer method (caller) or inner method (target).
    /// </summary>
    public required Scope scope;

    /// <summary>
    ///     The type of binding.
    /// </summary>
    public required BindingType bindingType;

    /// <summary>
    ///     Depending on <see cref="bindingType" /> and <see cref="scope" /> this can be either a local variable index or an
    ///     index into the caller or target argument lists.
    /// </summary>
    /// <remarks>
    ///     For argument lists of instance methods, index 0 is the instance and formal arguments start from index 1; for static
    ///     methods, formal arguments start from index 0.
    /// </remarks>
    public int index;

    public LocalTrackerBuilder? local;

    public FieldInfo[]? fields;

    public string? stateKey;

    public MemberInfo? memberInfo;

    public bool useVirtualDispatch;
}

internal struct PatchInfo
{
    public required int unpatchKey;
    public required Invocation inner;
    public required Invocation patch;
    public required PatchKind patchKind;
    public required ParameterBinding[] parameters;
    public required PatchOptions options;
    public required int priority;

    public readonly bool Inline => (options & PatchOptions.Inline) != 0;
    public readonly bool Debug => (options & PatchOptions.Debug) != 0;
    public readonly bool Optimize => (options & PatchOptions.Optimize) != 0;
    public readonly bool AlwaysRun => (options & PatchOptions.AlwaysRun) != 0;
    public readonly bool AllowMissingInnerTarget => (options & PatchOptions.AllowMissingInnerTarget) != 0;
    public readonly bool HasBindingType(BindingType bindingType) => parameters.Any(p => p.bindingType == bindingType);
}

internal class PatchRegistry
{
    internal static HarmonyInterface Harmony => HarmonyInterface.Instance;

    public static readonly PatchRegistry Instance = new();

    // When another lock is also needed, this must be taken before Harmony's lock.
    private readonly object syncRoot = new();
    
    // A set of methods that need updating in ApplyPendingChanges. This should be empty at the end of
    // every call of a public method on this class (Patch*/Unpatch*).
    private readonly UniqueQueue<MethodBaseInvocation> methodsToUpdate = [];

    private readonly MultiDictionary<MethodBaseInvocation, PatchInfo> patchesByMethod = [];
    private readonly Dictionary<int, HashSet<MethodBaseInvocation>> methodsByUnpatchKey = [];

    private PatchRegistry() { }

    private void ProcessAssembly(Assembly assembly, int unpatchKey)
    {
        foreach (TypeInfo type in assembly.DefinedTypes)
        {
            if (type.GetCustomAttribute<PatchAttribute>() != null || type.GetCustomAttribute<HarmonyPatch>() != null)
                ProcessType(type, unpatchKey, type.FullName ?? "");
        }
    }

    private void ProcessAssembly(Assembly assembly, string? category, int unpatchKey)
    {
        foreach (TypeInfo type in assembly.DefinedTypes)
        {
            if (type.GetCustomAttribute<PatchAttribute>() == null && type.GetCustomAttribute<HarmonyPatch>() == null)
                continue;

            string[] categories = [
                .. type.GetCustomAttributes<CategoryAttribute>().Select(c => c.Category),
                .. type.GetCustomAttributes<HarmonyPatchCategory>().Select(c => c.info.category)];

            if (!categories.Contains(category) && (categories.Length != 0 || category != null))
                continue;

            ProcessType(type, unpatchKey, type.FullName ?? "");
        }
    }

    private void ProcessType(TypeInfo type, int unpatchKey, string extraStateKey = "")
    {
        foreach (MethodInfo method in type.DeclaredMethods)
            ProcessMethod(method, unpatchKey, extraStateKey);
    }

    private void ProcessMethod(MethodInfo method, int unpatchKey, string extraStateKey = "")
    {
        try
        {
            List<Attribute> attributes = GetAttributes(method);

            var defaultTargetType =
                attributes.OfType<PatchAttribute>().Select(p => p.Type).FirstOrDefault(t => t is not null) ??
                attributes.OfType<HarmonyPatch>().Select(p => p.info.declaringType).FirstOrDefault(t => t is not null);
            var patchTypeAttribute = attributes.OfType<PatchTypeAttribute>().SingleOrDefault();
            var innerAttribute = attributes.OfType<InnerAttributeBase>().SingleOrDefault();
            var targetAttributes = attributes.OfType<TargetAttribute>().ToList();
            var priority = attributes.OfType<PriorityAttribute>().FirstOrDefault()?.Priority ?? PatchPriority.Default;
            var options = attributes.OfType<PatchOptionsAttribute>().FirstOrDefault()?.Options ?? PatchOptions.Default;

            if (patchTypeAttribute == null)
                return;

            PatchKind patchKind = patchTypeAttribute.PatchKind;

            Invocation inner = GetInnerInvocation(innerAttribute);

            foreach (var targetAttribute in targetAttributes)
            {
                var patchedType = targetAttribute.Type ?? defaultTargetType;

                var candidates = ReflectionTools.GetMembers(patchedType, targetAttribute.MethodName,
                    targetAttribute.MemberType, targetAttribute.ParameterTypes, null);

                var nameForErrors = targetAttribute.MemberType == MemberType.Constructor ? ".ctor" : targetAttribute.MethodName;

                switch (candidates.Count)
                {
                    case > 1 when targetAttribute is not TargetsAttribute:
                        throw new AmbiguousMatchException($"{nameForErrors}: Ambiguous match");
                    case 0: throw new ReflectionException($"{nameForErrors}: Member not found");
                }

                foreach (var result in candidates)
                {
                    MethodBase target = result as MethodBase ??
                                        throw new ReflectionException($"{nameForErrors}: Couldn't locate method");
                    var patch = new PatchConfig()
                    {
                        PatchMethod = method,
                        Kind = patchKind,
                        Target = GetOuterInvocation(target),
                        InnerTarget = inner,
                        Options = options,
                        Priority = priority
                    };
                    ProcessPatch(patch, unpatchKey, extraStateKey);
                }
            }
        }
        catch (Exception e)
        {
            if (e.GetType() == typeof(PatchException))
                throw;
            throw new PatchException($"Error processing {method.FullName}", e);
        }
    }

    private void ProcessMethods(IEnumerable<MethodInfo> methods, int unpatchKey)
    {
        foreach (var method in methods)
            ProcessMethod(method, unpatchKey);
    }

    private static List<Attribute> GetAttributes(MethodInfo method)
    {
        var typeAttributes = method.DeclaringType?.GetCustomAttributes().ToList() ?? [];
        var methodAttributes = method.GetCustomAttributes();

        List<Attribute> attributes = [.. methodAttributes, .. typeAttributes];
        return attributes;
    }

    private void ProcessPatch(PatchConfig patch, int unpatchKey, string extraStateKey = "")
    {
        if (patch.PatchMethod is null)
            throw new ArgumentException("Patch method not set; call Patch.With()", nameof(patch));
        if (patch.Kind is not { } patchKind)
            throw new ArgumentException("Patch type not set; call Patch.Prefix or Patch.Postfix", nameof(patch));
        if (patch.Target is not MethodBaseInvocation target)
            throw new ArgumentException("Patch target not set; call Patch.Of()", nameof(patch));

        try
        {
            MethodInvocation patchMethod = new(patch.PatchMethod);
            Validate(patchKind, patch.Options, patchMethod.MethodInfo, target.MethodBase);

            MethodBaseInvocation outer = target;
            Invocation inner = patch.InnerTarget;

            if (inner is not EmptyInvocation && outer is MethodInvocation outerMethod)
            {
                var moveNext = outerMethod.MethodInfo.GetStateMachineImplementation();
                if (moveNext != null)
                    outer = new MethodInvocation(moveNext);
            }

            var parameterBinder = new ParameterBinder(target, outer, inner, patchKind, patch.Options, $"{extraStateKey}#{unpatchKey}");

            var parameters = patchMethod.MethodInfo.GetParameters().Select(parameterBinder.Bind).ToArray();

            PatchInfo patchInfo = new()
            {
                unpatchKey = unpatchKey,
                inner = inner,
                patch = patchMethod,
                patchKind = patchKind,
                parameters = parameters,
                options = patch.Options,
                priority = patch.Priority,
            };

            methodsToUpdate.Enqueue(outer);
            patchesByMethod.Add(outer, patchInfo);

            if (!methodsByUnpatchKey.TryGetValue(unpatchKey, out var methodSet))
                methodSet = methodsByUnpatchKey[unpatchKey] = [];
            methodSet.Add(outer);
        }
        catch (Exception e)
        {
            if (e.GetType() == typeof(PatchException))
                throw;
            throw new PatchException($"Error processing {patch.PatchMethod.FullName}", e);
        }
    }

    private void ProcessPatches(IEnumerable<PatchConfig> patches, int unpatchKey)
    {
        foreach (var patch in patches)
            ProcessPatch(patch, unpatchKey);
    }

    private static Invocation GetInnerInvocation(InnerAttributeBase? patchTypeAttribute)
    {
        if (patchTypeAttribute == null)
            return EmptyInvocation.Instance;

        switch (patchTypeAttribute)
        {
            case InnerConstantAttribute { Value: int value }: return new ConstantIntInvocation(value);
            case InnerConstantAttribute { Value: long value }: return new ConstantLongInvocation(value);
            case InnerConstantAttribute { Value: float value }: return new ConstantFloatInvocation(value);
            case InnerConstantAttribute { Value: double value }: return new ConstantDoubleInvocation(value);
            case InnerConstantAttribute { Value: string value }: return new ConstantStringInvocation(value);
            case InnerAttribute innerMember:
            {
                MemberInfo inner = ReflectionTools.GetMember(innerMember.Type, innerMember.MemberName,
                    innerMember.MemberType, innerMember.ParameterTypes, innerMember.GenericTypes);

                return GetInnerInvocation(inner, innerMember.MemberType);
            }
            default: throw new InvalidOperationException();
        }
    }

    private static Invocation GetInnerInvocation(MemberInfo? inner, MemberType memberType)
    {
        return inner switch
        {
            FieldInfo field => memberType is MemberType.Setter
                ? new SetFieldInvocation(field)
                : new GetFieldInvocation(field),
            MethodInfo method => new MethodInvocation(method),
            ConstructorInfo constructor => new InnerConstructorInvocation(constructor),
            _ => throw new ArgumentOutOfRangeException(nameof(inner), inner, null)
        };
    }

    private static MethodBaseInvocation GetOuterInvocation(MethodBase target)
    {
        return target switch
        {
            MethodInfo outerMethod => new MethodInvocation(outerMethod),
            ConstructorInfo outerConstructor => new OuterConstructorInvocation(outerConstructor),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null),
        };
    }

    private static void Validate(PatchKind patchKind, PatchOptions options, MethodInfo method, MethodBase target)
    {
        if (method.ContainsGenericParameters)
            throw new PatchDefinitionException(method, "Generic patch functions are not supported");
        if (!method.IsStatic)
            throw new PatchDefinitionException(method, "Patch methods must be static");
        
        if (target.IsAbstract)
            throw new PatchDefinitionException(method, "Target method is abstract");
        if (target.ContainsGenericParameters)
            throw new PatchDefinitionException(method, "Can't patch uninstantiated generic method");

        // This is a limitation of MonoMod
        if (target.IsGenericMethod)
            throw new PatchDefinitionException(method, "Can't patch instantiated generic method");
        // MonoMod can sometimes patch methods in instantiated generic types, but only if all type arguments
        // are value types. We don't want to rely on this, so reject all generic types.
        if (target.DeclaringType != null && TypeOrAnyContainingTypeIsGeneric(target.DeclaringType))
            throw new PatchDefinitionException(method, "Can't patch method on generic type");
        
        if ((target.Attributes & MethodAttributes.PinvokeImpl) != 0)
            throw new PatchDefinitionException(method, "Can't patch native method");
        
        // This is a limitation of Harmony
        if ((target.CallingConvention & CallingConventions.VarArgs) != 0)
            throw new PatchDefinitionException(method, "Can't patch varargs method");

        switch (patchKind)
        {
            case PatchKind.Prefix:
            {
                if ((options & PatchOptions.AlwaysRun) != 0 && method.ReturnType != typeof(void))
                    throw new PatchDefinitionException(method, "Prefix with AlwaysRun option must return 'void'");
                if (method.ReturnType != typeof(void) && method.ReturnType != typeof(bool))
                    throw new PatchDefinitionException(method, "Prefix must return 'bool' or 'void'");
                break;
            }
            case PatchKind.Postfix:
            {
                if (method.ReturnType != typeof(void))
                    throw new PatchDefinitionException(method, "Postfix must return 'void'");
                break;
            }
            default: throw new ArgumentOutOfRangeException(nameof(patchKind), patchKind, null);
        }
    }

    private static bool TypeOrAnyContainingTypeIsGeneric(Type type)
    {
        if (type.IsGenericType)
            return true;
        if (type.DeclaringType is { } declaringType)
            return TypeOrAnyContainingTypeIsGeneric(declaringType);
        return false;
    }

    private void UnpatchAllInternal()
    {
        foreach (var group in patchesByMethod)
        {
            if (group.Any())
                methodsToUpdate.Enqueue(group.Key);
        }

        patchesByMethod.Clear();
        methodsByUnpatchKey.Clear();
    }

    private void UnpatchInternal(int unpatchKey)
    {
        if (!methodsByUnpatchKey.TryGetValue(unpatchKey, out var methods))
            return;

        foreach (var method in methods)
        {
            int count = patchesByMethod.RemoveAll(method, p => p.unpatchKey == unpatchKey);
            if (count > 0)
                methodsToUpdate.Enqueue(method);
        }

        methodsByUnpatchKey.Remove(unpatchKey);
    }

    private void ValidatePatchGroup(int unpatchKey)
    {
        if (!methodsByUnpatchKey.TryGetValue(unpatchKey, out var methods))
            return;

        var patches = methods.SelectMany(m => patchesByMethod[m].Where(p => p.unpatchKey == unpatchKey)).ToList();
        StateBuilder.ValidateState(patches);
    }

    private void ApplyPendingChanges()
    {
        Exception? patchException = null;

        foreach (var patchedMethod in methodsToUpdate)
        {
            try
            {
                IReadOnlyList<PatchInfo> patches = patchesByMethod[patchedMethod];

                if (patches.Count == 0)
                {
                    Harmony.Unpatch(patchedMethod.MethodBase);
                }
                else
                {
                    Ruleset ruleset = RulesetGenerator.MakeRuleset(patchedMethod, patches);

                    bool debug = patches.Any(p => p.Debug);
                    bool optimize = patches.Any(p => p.Optimize);

                    Harmony.ApplyPatch(patchedMethod, ruleset, true, debug, optimize);
                }
            }
            catch (Exception e)
            {
                Patcher.ReportException(e);
                patchException ??= e;
            }
        }

        methodsToUpdate.Clear();

        // If any patch failed, throw the exception to indicate that the patching process was not fully successful.
        if (patchException is not null)
            throw new RuntimePatchException("Patch failed", patchException);
    }

    public void PatchAll(Assembly assembly, PatchHandle handle)
    {
        lock (syncRoot)
        {
            try
            {
                ProcessAssembly(assembly, handle.id);
                ValidatePatchGroup(handle.id);
                ApplyPendingChanges();
            }
            catch (Exception)
            {
                Unpatch(handle);
                throw;
            }
        }
    }

    public void PatchCategory(Assembly assembly, string? category, PatchHandle handle)
    {
        lock (syncRoot)
        {
            try
            {
                ProcessAssembly(assembly, category, handle.id);
                ValidatePatchGroup(handle.id);
                ApplyPendingChanges();
            }
            catch (Exception)
            {
                Unpatch(handle);
                throw;
            }
        }
    }

    public void PatchAll(Type type, PatchHandle handle)
    {
        lock (syncRoot)
        {
            try
            {
                ProcessType(type.GetTypeInfo(), handle.id);
                ValidatePatchGroup(handle.id);
                ApplyPendingChanges();
            }
            catch (Exception)
            {
                Unpatch(handle);
                throw;
            }
        }
    }

    public void Patch(IEnumerable<MethodInfo> methods, PatchHandle handle)
    {
        lock (syncRoot)
        {
            try
            {
                ProcessMethods(methods, handle.id);
                ValidatePatchGroup(handle.id);
                ApplyPendingChanges();
            }
            catch (Exception)
            {
                Unpatch(handle);
                throw;
            }
        }
    }

    public void Patch(IEnumerable<PatchConfig> patches, PatchHandle handle)
    {
        lock (syncRoot)
        {
            try
            {
                ProcessPatches(patches, handle.id);
                ValidatePatchGroup(handle.id);
                ApplyPendingChanges();
            }
            catch (Exception)
            {
                Unpatch(handle);
                throw;
            }
        }
    }

    public void Unpatch(PatchHandle handle)
    {
        lock (syncRoot)
        {
            UnpatchInternal(handle.id);
            ApplyPendingChanges();
        }
    }

    public void UnpatchAll()
    {
        lock (syncRoot)
        {
            UnpatchAllInternal();
            ApplyPendingChanges();
        }
    }
}
