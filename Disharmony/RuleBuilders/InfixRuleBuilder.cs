namespace Disharmony.RuleBuilders;

/// <summary>
///     This class generates rules implementing inner <see cref="PatchKind.Prefix" /> and
///     <see cref="PatchKind.Postfix" /> patches for a method.
/// </summary>
internal class InfixRuleBuilder : PrefixPostfixRuleBuilder
{
    private readonly Invocation inner;

    private readonly Type[] innerParameterTypes;
    private readonly LocalTracker[] innerParameterLocals;

    public InfixRuleBuilder(
        RuleBuilderContext context,
        Invocation outer,
        Invocation inner,
        List<PatchInfo> patches) : base(context, outer)
    {
        prefixes =
        [
            // Prefixes are sorted by priority and then reversed, so prefix-postfix pairs will nest naturally
            // even if priority isn't set
            .. patches.Where(patch => patch is { patchKind: PatchKind.Prefix, inner: not EmptyInvocation })
                .OrderBy(patch => patch.priority).Reverse(),
        ];
        postfixes =
        [
            .. patches.Where(patch => patch is { patchKind: PatchKind.Postfix, inner: not EmptyInvocation })
                .OrderBy(patch => patch.priority),
        ];

        this.inner = inner;

        innerParameterTypes = this.inner.ParameterTypes;
        innerParameterLocals = new LocalTracker[innerParameterTypes.Length];

        targetType = this.inner.ReturnType;
    }

    private void EmitReplacement()
    {
        EmitPrelude();
        InitializeLocals();
        EmitPrefixes();

        for (int i = 0; i < innerParameterTypes.Length; i++)
        {
            Type type = innerParameterTypes[i];
            EmitInnerParameter(i, type);
        }

        output.AddRange(inner.GetCodeInstructions());

        if (skipLabel == null && postfixes.Count == 0)
            return;

        if (resultLocal != null)
            output.Add(resultLocal.Store());

        EmitPostfixes();
    }

    private void EmitPrelude()
    {
        // Save all parameters to local. The matcher will handle renumbering the locals to new
        // unused local indexes.
        for (int i = innerParameterTypes.Length - 1; i >= 0; i--)
        {
            innerParameterLocals[i] = output.AddLocal(innerParameterTypes[i]);
            output.Add(innerParameterLocals[i].Store());
        }
    }

    protected override Type GetParameterType(ParameterBinding parameter) => parameter.scope switch
    {
        Scope.Outer => outerParameterTypes[parameter.index],
        Scope.Inner => innerParameterTypes[parameter.index],
        _ => throw new ArgumentOutOfRangeException(nameof(parameter.scope)),
    };

    protected override Invocation GetInvocation(ParameterBinding parameter) => parameter.scope switch
    {
        Scope.Inner => inner,
        _ => base.GetInvocation(parameter),
    };

    protected override void EmitParameterLookup(Scope scope, int index, Type resultType)
    {
        switch (scope)
        {
            case Scope.Outer: EmitOuterParameter(index, resultType); break;
            case Scope.Inner: EmitInnerParameter(index, resultType); break;
            case Scope.Any:
            default:
                throw new ArgumentOutOfRangeException(nameof(scope));
        }
    }


    public override IEnumerable<Rule> BuildRules()
    {
        EmitReplacement();

        // Only allow silently skipping the rule if all patches agree to AllowMissingInnerTarget.
        bool allowMissingInnerTarget = prefixes.All(p => p.AllowMissingInnerTarget) && postfixes.All(p => p.AllowMissingInnerTarget);

        yield return new Rule
        {
            Min = allowMissingInnerTarget ? 0 : 1,
            Max = 0,
            Mode = OutputMode.Replace,
            Pattern = [.. inner.GetCodeInstructions()],
            Output = [.. output.instructions],
            Name = inner.FullName,
        };
    }

    private void EmitInnerParameter(int index, Type resultType)
    {
        Type parameterType = innerParameterTypes[index];
        output.Add(innerParameterLocals[index].Load(resultType.IsByRef && !parameterType.IsByRef));
        if (!resultType.IsByRef && parameterType.IsByRef)
            output.Add(new(OpCodes.Ldobj, parameterType.GetElementType()));
    }
}
