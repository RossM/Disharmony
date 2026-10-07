namespace Disharmony.RuleBuilders;

/// <summary>
///     This class manages the state variables used by patches and generates rules to initialize them at the beginning of
///     the method.
/// </summary>
/// <param name="context"></param>
internal class StateBuilder(RuleBuilderContext context) : RuleBuilder(context, EmptyInvocation.Instance)
{
    private readonly Dictionary<string, LocalTrackerBuilder> stateMap = [];
    
    // Use a separate list rather than stateMap.Values so that initialization order is deterministic
    private readonly List<LocalTrackerBuilder> stateLocals = [];

    private LocalTrackerBuilder GetOrAddStateLocal(string stateKey, Type localType)
    {
        localType = localType.NoRefType;

        if (stateMap.TryGetValue(stateKey, out var local))
        {
            if (localType != local.Type)
                throw new InvalidOperationException($"Incompatible state types: {localType} and {local.Type}");
            return local;
        }

        local = output.AddLocal(localType);
        stateMap.Add(stateKey, local);
        stateLocals.Add(local);
        return local;
    }

    public override IEnumerable<Rule> BuildRules()
    {
        if (stateMap.Count == 0)
            yield break;

        foreach (var local in stateLocals)
            output.EmitLocalInitializer(local);

        yield return new Rule
        {
            Priority = 100,
            Mode = OutputMode.MethodPrefix,
            Output = [.. output.instructions],
            Name = "state variable initialization",
        };
    }

    public void AssignStateVariableIndexes(IReadOnlyList<PatchInfo> patches)
    {
        foreach (var patch in patches)
        {
            ParameterBinding[] parameters = patch.parameters;
            foreach (ParameterBinding parameter in parameters)
            {
                if (parameter.bindingType != BindingType.State)
                    continue;

                if (parameter.stateKey is null)
                    throw new InvalidOperationException("Null StateKey");
                
                parameter.local = GetOrAddStateLocal(parameter.stateKey, parameter.parameter.ParameterType);
            }
        }
    }

    public static void ValidateState(IReadOnlyList<PatchInfo> patches)
    {
        Dictionary<string, Type> stateTypes = [];

        foreach (var patch in patches)
        {
            try
            {
                ParameterBinding[] parameters = patch.parameters;
                foreach (ParameterBinding parameter in parameters)
                {
                    if (parameter.bindingType != BindingType.State)
                        continue;

                    if (parameter.stateKey is null)
                        throw new ParameterBindingException(parameter.parameter.Name, "Null StateKey");

                    var localType = parameter.parameter.ParameterType.NoRefType;
                    if (stateTypes.TryGetValue(parameter.stateKey, out var existingType))
                    {
                        if (localType != existingType)
                            throw new ParameterBindingException(parameter.parameter.Name,
                                $"Incompatible state types: {localType} and {existingType}");
                    }
                    else
                    {
                        stateTypes.Add(parameter.stateKey, localType);
                    }
                }
            }
            catch (Exception e)
            {
                throw new PatchException($"Error processing {patch.patch.FullName}", e);
            }
        }

    }
}
