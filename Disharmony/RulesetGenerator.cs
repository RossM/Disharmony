using Disharmony.RuleBuilders;

namespace Disharmony;

internal static class RulesetGenerator
{
    public static Ruleset MakeRuleset(MethodBaseInvocation outer, IReadOnlyList<PatchInfo> patches)
    {
        List<RuleBuilder> ruleBuilders = [];

        var context = new RuleBuilderContext();

        StateBuilder stateBuilder = new(context);
        stateBuilder.AssignStateVariableIndexes(patches);
        ruleBuilders.Add(stateBuilder);

        ruleBuilders.Add(new CircumfixRuleBuilder(context, outer, patches));

        foreach (IGrouping<Invocation, PatchInfo> targetGroup in patches
                     .Where(patch => patch.inner is not EmptyInvocation).GroupBy(patch => patch.inner))
        {
            Invocation inner = targetGroup.Key;
            ruleBuilders.Add(new InfixRuleBuilder(context, outer, inner, [.. targetGroup]));
        }

        foreach (var patch in patches.Where(p => p.Inline))
        {
            if (patch.patch is not MethodInvocation method)
                continue;
            ruleBuilders.Add(new InlineRuleBuilder(context, method));
        }

        List<Rule> rules = [];
        List<Label> labels = [];
        foreach (var ruleBuilder in ruleBuilders)
        {
            rules.AddRange(ruleBuilder.BuildRules());
            labels.AddRange(ruleBuilder.CrossRuleLabels);
        }

        if (rules.Count == 0)
            throw new InvalidOperationException("No rules generated");

        var ruleset = new Ruleset
        {
            Rules = rules,
            CrossRuleLocals = [.. context.locals.Select(l => l.Builder)],
            CrossRuleLabels = labels,
        };

        return ruleset;
    }
}
