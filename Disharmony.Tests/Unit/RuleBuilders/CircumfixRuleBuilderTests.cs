using System.Runtime.ExceptionServices;
using Disharmony.RuleBuilders;
using Disharmony.RulesEngine;
using static Disharmony.Tests.Support.RuleAssertions;
using BoundParameter = Disharmony.ParameterBinding;

namespace Disharmony.Tests.Unit.RuleBuilders;

[TestFixture]
public sealed class CircumfixRuleBuilderTests
{
    private static readonly MethodInvocation Target = new(
        typeof(CircumfixRuleBuilderTargets).GetMethod(nameof(CircumfixRuleBuilderTargets.Target))!);
    private static readonly MethodInvocation RefTarget = new(
        typeof(CircumfixRuleBuilderTargets).GetMethod(nameof(CircumfixRuleBuilderTargets.RefTarget))!);
    private static readonly MethodInvocation VoidTarget = new(
        typeof(CircumfixRuleBuilderTargets).GetMethod(nameof(CircumfixRuleBuilderTargets.VoidTarget))!);
    private static readonly MethodInvocation PrefixLow = new(
        typeof(CircumfixRuleBuilderTargets).GetMethod(nameof(CircumfixRuleBuilderTargets.PrefixLow))!);
    private static readonly MethodInvocation PrefixHigh = new(
        typeof(CircumfixRuleBuilderTargets).GetMethod(nameof(CircumfixRuleBuilderTargets.PrefixHigh))!);
    private static readonly MethodInvocation PostfixLow = new(
        typeof(CircumfixRuleBuilderTargets).GetMethod(nameof(CircumfixRuleBuilderTargets.PostfixLow))!);
    private static readonly MethodInvocation PostfixHigh = new(
        typeof(CircumfixRuleBuilderTargets).GetMethod(nameof(CircumfixRuleBuilderTargets.PostfixHigh))!);
    private static readonly MethodInvocation BooleanPrefix = new(
        typeof(CircumfixRuleBuilderTargets).GetMethod(nameof(CircumfixRuleBuilderTargets.BooleanPrefix))!);
    private static readonly MethodInvocation SecondBooleanPrefix = new(
        typeof(CircumfixRuleBuilderTargets).GetMethod(nameof(CircumfixRuleBuilderTargets.SecondBooleanPrefix))!);
    private static readonly MethodInvocation WriteArgument = new(
        typeof(CircumfixRuleBuilderTargets).GetMethod(nameof(CircumfixRuleBuilderTargets.WriteArgument))!);
    private static readonly MethodInvocation ReadArgument = new(
        typeof(CircumfixRuleBuilderTargets).GetMethod(nameof(CircumfixRuleBuilderTargets.ReadArgument))!);
    private static readonly MethodInvocation WriteResult = new(
        typeof(CircumfixRuleBuilderTargets).GetMethod(nameof(CircumfixRuleBuilderTargets.WriteResult))!);
    private static readonly MethodInvocation ReadResult = new(
        typeof(CircumfixRuleBuilderTargets).GetMethod(nameof(CircumfixRuleBuilderTargets.ReadResult))!);
    private static readonly MethodInvocation WriteState = new(
        typeof(CircumfixRuleBuilderTargets).GetMethod(nameof(CircumfixRuleBuilderTargets.WriteState))!);
    private static readonly MethodInvocation ReadState = new(
        typeof(CircumfixRuleBuilderTargets).GetMethod(nameof(CircumfixRuleBuilderTargets.ReadState))!);
    private static readonly MethodInvocation AlwaysPrefix = new(
        typeof(CircumfixRuleBuilderTargets).GetMethod(nameof(CircumfixRuleBuilderTargets.AlwaysPrefix))!);
    private static readonly MethodInvocation AlwaysPostfix = new(
        typeof(CircumfixRuleBuilderTargets).GetMethod(nameof(CircumfixRuleBuilderTargets.AlwaysPostfix))!);

    private static PatchInfo CreatePatch(
        Invocation patch,
        PatchKind patchKind,
        BoundParameter[]? parameters = null,
        PatchOptions options = PatchOptions.Default,
        int priority = 0,
        Invocation? inner = null) => new()
    {
        unpatchKey = 0,
        inner = inner ?? EmptyInvocation.Instance,
        patch = patch,
        patchKind = patchKind,
        parameters = parameters ?? [],
        options = options,
        priority = priority,
    };

    [Test]
    public void BuildRules_NoPatches_EmitsNoRulesOrLocalsOrLabels()
    {
        var context = new RuleBuilderContext();
        var builder = new CircumfixRuleBuilder(context, Target, []);

        Rule[] rules = [.. builder.BuildRules()];

        AssertRules(rules, []);
        Assert.That(context.locals, Is.Empty);
        Assert.That(builder.CrossRuleLabels, Is.Empty);
    }

    [Test]
    public void BuildRules_InnerPatches_AreExcluded()
    {
        var context = new RuleBuilderContext();
        var builder = new CircumfixRuleBuilder(context, Target,
        [
            CreatePatch(BooleanPrefix, PatchKind.Prefix, inner: VoidTarget),
            CreatePatch(PostfixLow, PatchKind.Postfix, inner: VoidTarget),
        ]);

        Rule[] rules = [.. builder.BuildRules()];

        AssertRules(rules, []);
        Assert.That(context.locals, Is.Empty);
        Assert.That(builder.CrossRuleLabels, Is.Empty);
    }

    [Test]
    public void BuildRules_VoidPrefix_EmitsOnlyMethodPrefix()
    {
        var context = new RuleBuilderContext();
        var builder = new CircumfixRuleBuilder(context, Target, [CreatePatch(PrefixLow, PatchKind.Prefix)]);

        Rule[] rules = [.. builder.BuildRules()];

        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.MethodPrefix, Name = "prefixes",
                Output =
                [
                    CodeInstruction.Annotation($"Prefix {PrefixLow.FullName}"),
                    new(OpCodes.Call, PrefixLow.MethodInfo),
                ],
            },
        ]);
        Assert.That(context.locals, Is.Empty);
        Assert.That(builder.CrossRuleLabels, Is.Empty);
    }

    [Test]
    public void BuildRules_BooleanPrefix_ValueTarget_SkipsToInitializedResultWithoutRewritingReturns()
    {
        var context = new RuleBuilderContext();
        var builder = new CircumfixRuleBuilder(context, Target, [CreatePatch(BooleanPrefix, PatchKind.Prefix)]);

        Rule[] rules = [.. builder.BuildRules()];
        Label skip = builder.CrossRuleLabels.Single();
        LocalBuilder result = context.locals.Single().Builder;

        Assert.That(result.LocalType, Is.EqualTo(typeof(int)));
        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.MethodPrefix, Name = "prefixes",
                Output =
                [
                    new(OpCodes.Ldc_I4_0),
                    new(OpCodes.Stloc_S, result),
                    CodeInstruction.Annotation($"Prefix {BooleanPrefix.FullName}"),
                    new(OpCodes.Call, BooleanPrefix.MethodInfo),
                    new(OpCodes.Brfalse, skip),
                ],
            },
            new Rule
            {
                Mode = OutputMode.MethodPostfix, Name = "postfixes",
                Output =
                [
                    new CodeInstruction(OpCodes.Nop).WithLabels(skip),
                    new(OpCodes.Ldloc_S, result),
                    new(OpCodes.Ret),
                ],
            },
        ]);
    }

    [Test]
    public void BuildRules_BooleanPrefix_VoidTarget_SkipsWithoutResultLocal()
    {
        var context = new RuleBuilderContext();
        var builder = new CircumfixRuleBuilder(context, VoidTarget, [CreatePatch(BooleanPrefix, PatchKind.Prefix)]);

        Rule[] rules = [.. builder.BuildRules()];
        Label skip = builder.CrossRuleLabels.Single();

        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.MethodPrefix, Name = "prefixes",
                Output =
                [
                    CodeInstruction.Annotation($"Prefix {BooleanPrefix.FullName}"),
                    new(OpCodes.Call, BooleanPrefix.MethodInfo),
                    new(OpCodes.Brfalse, skip),
                ],
            },
            new Rule
            {
                Mode = OutputMode.MethodPostfix, Name = "postfixes",
                Output = [new CodeInstruction(OpCodes.Nop).WithLabels(skip), new(OpCodes.Ret)],
            },
        ]);
        Assert.That(context.locals, Is.Empty);
    }

    [Test]
    public void BuildRules_PostfixWithoutResultBinding_KeepsReturnValueOnStack()
    {
        var context = new RuleBuilderContext();
        var builder = new CircumfixRuleBuilder(context, Target, [CreatePatch(PostfixLow, PatchKind.Postfix)]);

        Rule[] rules = [.. builder.BuildRules()];
        Label end = builder.CrossRuleLabels.Single();

        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.Replace, Name = "return", Min = 0, Max = 0,
                Pattern = [new(OpCodes.Ret)],
                Output = [new(OpCodes.Br, end)],
            },
            new Rule
            {
                Mode = OutputMode.MethodPostfix, Name = "postfixes",
                Output =
                [
                    new CodeInstruction(OpCodes.Nop).WithLabels(end),
                    CodeInstruction.Annotation($"Postfix {PostfixLow.FullName}"),
                    new(OpCodes.Call, PostfixLow.MethodInfo),
                    new(OpCodes.Ret),
                ],
            },
        ]);
        Assert.That(context.locals, Is.Empty);
    }

    [Test]
    public void BuildRules_PostfixResultByReference_StoresAtEachReturnBeforeBranching()
    {
        var context = new RuleBuilderContext();
        var binding = new BoundParameter
        {
            parameter = WriteResult.MethodInfo.GetParameters()[0],
            bindingType = BindingType.Result, scope = Scope.Outer,
        };
        var builder = new CircumfixRuleBuilder(context, Target,
            [CreatePatch(WriteResult, PatchKind.Postfix, parameters: [binding])]);

        Rule[] rules = [.. builder.BuildRules()];
        Label end = builder.CrossRuleLabels.Single();
        LocalBuilder result = context.locals.Single().Builder;

        Assert.That(result.LocalType, Is.EqualTo(typeof(int)));
        // Min = 0 allows methods which only throw; the appended block must not pop a nonexistent return value.
        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.MethodPrefix, Name = "prefixes",
                Output = [new(OpCodes.Ldc_I4_0), new(OpCodes.Stloc_S, result)],
            },
            new Rule
            {
                Mode = OutputMode.Replace, Name = "return", Min = 0, Max = 0,
                Pattern = [new(OpCodes.Ret)],
                Output = [new(OpCodes.Stloc_S, result), new(OpCodes.Br, end)],
            },
            new Rule
            {
                Mode = OutputMode.MethodPostfix, Name = "postfixes",
                Output =
                [
                    new CodeInstruction(OpCodes.Nop).WithLabels(end),
                    new(OpCodes.Ldloca_S, result),
                    CodeInstruction.Annotation($"Postfix {WriteResult.FullName}"),
                    new(OpCodes.Call, WriteResult.MethodInfo),
                    new(OpCodes.Ldloc_S, result),
                    new(OpCodes.Ret),
                ],
            },
        ]);
    }

    [Test]
    public void BuildRules_MultipleSkippingPrefixesAndPostfix_ShareSkipLabelAndResultLocal()
    {
        var context = new RuleBuilderContext();
        var binding = new BoundParameter
        {
            parameter = ReadResult.MethodInfo.GetParameters()[0],
            bindingType = BindingType.Result, scope = Scope.Outer,
        };
        var builder = new CircumfixRuleBuilder(context, Target,
        [
            CreatePatch(BooleanPrefix, PatchKind.Prefix),
            CreatePatch(SecondBooleanPrefix, PatchKind.Prefix),
            CreatePatch(ReadResult, PatchKind.Postfix, parameters: [binding]),
        ]);

        Rule[] rules = [.. builder.BuildRules()];
        Label[] labels = [.. builder.CrossRuleLabels];
        Assert.That(labels, Has.Length.EqualTo(2));
        Label skip = labels[0];
        Label end = labels[1];
        Assert.That(skip, Is.Not.EqualTo(end));
        LocalBuilder result = context.locals.Single().Builder;

        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.MethodPrefix, Name = "prefixes",
                Output =
                [
                    new(OpCodes.Ldc_I4_0),
                    new(OpCodes.Stloc_S, result),
                    CodeInstruction.Annotation($"Prefix {SecondBooleanPrefix.FullName}"),
                    new(OpCodes.Call, SecondBooleanPrefix.MethodInfo),
                    new(OpCodes.Brfalse, skip),
                    CodeInstruction.Annotation($"Prefix {BooleanPrefix.FullName}"),
                    new(OpCodes.Call, BooleanPrefix.MethodInfo),
                    new(OpCodes.Brfalse, skip),
                ],
            },
            new Rule
            {
                Mode = OutputMode.Replace, Name = "return", Min = 0, Max = 0,
                Pattern = [new(OpCodes.Ret)],
                Output = [new(OpCodes.Stloc_S, result), new(OpCodes.Br, end)],
            },
            new Rule
            {
                Mode = OutputMode.MethodPostfix, Name = "postfixes",
                Output =
                [
                    new CodeInstruction(OpCodes.Nop).WithLabels(end),
                    new CodeInstruction(OpCodes.Nop).WithLabels(skip),
                    new(OpCodes.Ldloc_S, result),
                    CodeInstruction.Annotation($"Postfix {ReadResult.FullName}"),
                    new(OpCodes.Call, ReadResult.MethodInfo),
                    new(OpCodes.Ldloc_S, result),
                    new(OpCodes.Ret),
                ],
            },
        ]);
    }

    [Test]
    public void BuildRules_ResultBindingWithoutSkipping_InitializesResultButLeavesOriginalReturns()
    {
        var context = new RuleBuilderContext();
        var binding = new BoundParameter
        {
            parameter = WriteResult.MethodInfo.GetParameters()[0],
            bindingType = BindingType.Result, scope = Scope.Outer,
        };
        var builder = new CircumfixRuleBuilder(context, Target,
            [CreatePatch(WriteResult, PatchKind.Prefix, parameters: [binding])]);

        Rule[] rules = [.. builder.BuildRules()];
        LocalBuilder result = context.locals.Single().Builder;

        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.MethodPrefix, Name = "prefixes",
                Output =
                [
                    new(OpCodes.Ldc_I4_0),
                    new(OpCodes.Stloc_S, result),
                    new(OpCodes.Ldloca_S, result),
                    CodeInstruction.Annotation($"Prefix {WriteResult.FullName}"),
                    new(OpCodes.Call, WriteResult.MethodInfo),
                ],
            },
        ]);
        Assert.That(builder.CrossRuleLabels, Is.Empty);
    }

    [Test]
    public void BuildRules_Priorities_RunHigherPrefixFirstAndHigherPostfixLast()
    {
        var context = new RuleBuilderContext();
        var builder = new CircumfixRuleBuilder(context, VoidTarget,
        [
            CreatePatch(PrefixHigh, PatchKind.Prefix, priority: 10),
            CreatePatch(PostfixHigh, PatchKind.Postfix, priority: 10),
            CreatePatch(PrefixLow, PatchKind.Prefix, priority: -10),
            CreatePatch(PostfixLow, PatchKind.Postfix, priority: -10),
        ]);

        Rule[] rules = [.. builder.BuildRules()];
        Label end = builder.CrossRuleLabels.Single();

        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.MethodPrefix, Name = "prefixes",
                Output =
                [
                    CodeInstruction.Annotation($"Prefix {PrefixHigh.FullName}"),
                    new(OpCodes.Call, PrefixHigh.MethodInfo),
                    CodeInstruction.Annotation($"Prefix {PrefixLow.FullName}"),
                    new(OpCodes.Call, PrefixLow.MethodInfo),
                ],
            },
            new Rule
            {
                Mode = OutputMode.Replace, Name = "return", Min = 0, Max = 0,
                Pattern = [new(OpCodes.Ret)], Output = [new(OpCodes.Br, end)],
            },
            new Rule
            {
                Mode = OutputMode.MethodPostfix, Name = "postfixes",
                Output =
                [
                    new CodeInstruction(OpCodes.Nop).WithLabels(end),
                    CodeInstruction.Annotation($"Postfix {PostfixLow.FullName}"),
                    new(OpCodes.Call, PostfixLow.MethodInfo),
                    CodeInstruction.Annotation($"Postfix {PostfixHigh.FullName}"),
                    new(OpCodes.Call, PostfixHigh.MethodInfo),
                    new(OpCodes.Ret),
                ],
            },
        ]);
        Assert.That(context.locals, Is.Empty);
    }

    [Test]
    public void BuildRules_EqualPriorityPatchPairs_NestInRegistrationOrder()
    {
        var context = new RuleBuilderContext();
        var builder = new CircumfixRuleBuilder(context, VoidTarget,
        [
            CreatePatch(PrefixLow, PatchKind.Prefix),
            CreatePatch(PostfixLow, PatchKind.Postfix),
            CreatePatch(PrefixHigh, PatchKind.Prefix),
            CreatePatch(PostfixHigh, PatchKind.Postfix),
        ]);

        Rule[] rules = [.. builder.BuildRules()];
        Label end = builder.CrossRuleLabels.Single();

        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.MethodPrefix, Name = "prefixes",
                Output =
                [
                    CodeInstruction.Annotation($"Prefix {PrefixHigh.FullName}"),
                    new(OpCodes.Call, PrefixHigh.MethodInfo),
                    CodeInstruction.Annotation($"Prefix {PrefixLow.FullName}"),
                    new(OpCodes.Call, PrefixLow.MethodInfo),
                ],
            },
            new Rule
            {
                Mode = OutputMode.Replace, Name = "return", Min = 0, Max = 0,
                Pattern = [new(OpCodes.Ret)], Output = [new(OpCodes.Br, end)],
            },
            new Rule
            {
                Mode = OutputMode.MethodPostfix, Name = "postfixes",
                Output =
                [
                    new CodeInstruction(OpCodes.Nop).WithLabels(end),
                    CodeInstruction.Annotation($"Postfix {PostfixLow.FullName}"),
                    new(OpCodes.Call, PostfixLow.MethodInfo),
                    CodeInstruction.Annotation($"Postfix {PostfixHigh.FullName}"),
                    new(OpCodes.Call, PostfixHigh.MethodInfo),
                    new(OpCodes.Ret),
                ],
            },
        ]);
    }

    [Test]
    public void BuildRules_ArgumentByReference_LoadsOriginalArgumentAddress()
    {
        var context = new RuleBuilderContext();
        var binding = new BoundParameter
        {
            parameter = WriteArgument.MethodInfo.GetParameters()[0],
            bindingType = BindingType.Argument, scope = Scope.Outer, index = 0,
        };
        var builder = new CircumfixRuleBuilder(context, Target,
            [CreatePatch(WriteArgument, PatchKind.Prefix, parameters: [binding])]);

        Rule[] rules = [.. builder.BuildRules()];

        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.MethodPrefix, Name = "prefixes",
                Output =
                [
                    new(OpCodes.Ldarga_S, (byte)0),
                    CodeInstruction.Annotation($"Prefix {WriteArgument.FullName}"),
                    new(OpCodes.Call, WriteArgument.MethodInfo),
                ],
            },
        ]);
        Assert.That(context.locals, Is.Empty);
    }

    [Test]
    public void BuildRules_ByRefArgumentReadByValue_DereferencesOriginalArgument()
    {
        var context = new RuleBuilderContext();
        var binding = new BoundParameter
        {
            parameter = ReadArgument.MethodInfo.GetParameters()[0],
            bindingType = BindingType.Argument, scope = Scope.Outer, index = 0,
        };
        var builder = new CircumfixRuleBuilder(context, RefTarget,
            [CreatePatch(ReadArgument, PatchKind.Prefix, parameters: [binding])]);

        Rule[] rules = [.. builder.BuildRules()];

        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.MethodPrefix, Name = "prefixes",
                Output =
                [
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldobj, typeof(int)),
                    CodeInstruction.Annotation($"Prefix {ReadArgument.FullName}"),
                    new(OpCodes.Call, ReadArgument.MethodInfo),
                ],
            },
        ]);
        Assert.That(context.locals, Is.Empty);
    }

    [Test]
    public void BuildRules_SharedState_PrefixAndPostfixAccessSameExistingLocal()
    {
        var context = new RuleBuilderContext();
        var write = new BoundParameter
        {
            parameter = WriteState.MethodInfo.GetParameters()[0],
            bindingType = BindingType.State, scope = Scope.Outer, stateKey = "shared",
        };
        var read = new BoundParameter
        {
            parameter = ReadState.MethodInfo.GetParameters()[0],
            bindingType = BindingType.State, scope = Scope.Outer, stateKey = "shared",
        };
        PatchInfo[] patches =
        [
            CreatePatch(WriteState, PatchKind.Prefix, parameters: [write]),
            CreatePatch(ReadState, PatchKind.Postfix, parameters: [read]),
        ];
        new StateBuilder(context).AssignStateVariableIndexes(patches);
        LocalBuilder state = write.local!.Builder;
        var builder = new CircumfixRuleBuilder(context, VoidTarget, patches);

        Rule[] rules = [.. builder.BuildRules()];
        Label end = builder.CrossRuleLabels.Single();

        Assert.That(context.locals.Single().Builder, Is.SameAs(state));
        Assert.That(read.local!.Builder, Is.SameAs(state));
        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.MethodPrefix, Name = "prefixes",
                Output =
                [
                    new(OpCodes.Ldloca_S, state),
                    CodeInstruction.Annotation($"Prefix {WriteState.FullName}"),
                    new(OpCodes.Call, WriteState.MethodInfo),
                ],
            },
            new Rule
            {
                Mode = OutputMode.Replace, Name = "return", Min = 0, Max = 0,
                Pattern = [new(OpCodes.Ret)], Output = [new(OpCodes.Br, end)],
            },
            new Rule
            {
                Mode = OutputMode.MethodPostfix, Name = "postfixes",
                Output =
                [
                    new CodeInstruction(OpCodes.Nop).WithLabels(end),
                    new(OpCodes.Ldloc_S, state),
                    CodeInstruction.Annotation($"Postfix {ReadState.FullName}"),
                    new(OpCodes.Call, ReadState.MethodInfo),
                    new(OpCodes.Ret),
                ],
            },
        ]);
    }

    [Test]
    public void BuildRules_AlwaysRunPrefixWithoutAlwaysRunPostfix_DoesNotOpenExceptionRegion()
    {
        var context = new RuleBuilderContext();
        var builder = new CircumfixRuleBuilder(context, Target,
        [
            CreatePatch(AlwaysPrefix, PatchKind.Prefix, options: PatchOptions.AlwaysRun, priority: -100),
            CreatePatch(PrefixLow, PatchKind.Prefix, priority: 100),
        ]);

        Rule[] rules = [.. builder.BuildRules()];

        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.MethodPrefix, Name = "prefixes",
                Output =
                [
                    CodeInstruction.Annotation($"Prefix {AlwaysPrefix.FullName}"),
                    new(OpCodes.Call, AlwaysPrefix.MethodInfo),
                    CodeInstruction.Annotation($"Prefix {PrefixLow.FullName}"),
                    new(OpCodes.Call, PrefixLow.MethodInfo),
                ],
            },
        ]);
        Assert.That(context.locals, Is.Empty);
        Assert.That(builder.CrossRuleLabels, Is.Empty);
    }

    [Test]
    public void BuildRules_AlwaysRunPostfix_ExceptionRegionSpansRulesAndSkipStillRunsPostfixes()
    {
        var context = new RuleBuilderContext();
        var binding = new BoundParameter
        {
            parameter = AlwaysPostfix.MethodInfo.GetParameters()[0],
            bindingType = BindingType.Exception, scope = Scope.Any,
        };
        var builder = new CircumfixRuleBuilder(context, Target,
        [
            CreatePatch(AlwaysPrefix, PatchKind.Prefix, options: PatchOptions.AlwaysRun, priority: -100),
            CreatePatch(BooleanPrefix, PatchKind.Prefix, priority: 100),
            CreatePatch(AlwaysPostfix, PatchKind.Postfix, parameters: [binding], options: PatchOptions.AlwaysRun, priority: -100),
            CreatePatch(PostfixLow, PatchKind.Postfix, priority: 100),
        ]);

        Rule[] rules = [.. builder.BuildRules()];
        Label[] labels = [.. builder.CrossRuleLabels];
        Assert.That(labels, Has.Length.EqualTo(2));
        Label skip = labels[0];
        Label end = labels[1];
        Label noThrow = rules[2].Output!.SelectMany(i => i.labels).Except(labels).Single();
        Assert.That(new[] { skip, end, noThrow }, Is.Unique);
        Assert.That(context.locals.Select(l => l.Type),
            Is.EqualTo(new[] { typeof(int), typeof(Exception), typeof(ExceptionDispatchInfo) }));
        LocalBuilder result = context.locals[0].Builder;
        LocalBuilder exception = context.locals[1].Builder;
        LocalBuilder dispatchInfo = context.locals[2].Builder;

        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.MethodPrefix, Name = "prefixes",
                Output =
                [
                    new(OpCodes.Ldc_I4_0),
                    new(OpCodes.Stloc_S, result),
                    new(OpCodes.Ldnull),
                    new(OpCodes.Stloc_S, exception),
                    new(OpCodes.Ldnull),
                    new(OpCodes.Stloc_S, dispatchInfo),
                    CodeInstruction.Annotation($"Prefix {AlwaysPrefix.FullName}"),
                    new(OpCodes.Call, AlwaysPrefix.MethodInfo),
                    new CodeInstruction(OpCodes.Nop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
                    CodeInstruction.Annotation($"Prefix {BooleanPrefix.FullName}"),
                    new(OpCodes.Call, BooleanPrefix.MethodInfo),
                    new(OpCodes.Brfalse, skip),
                ],
            },
            new Rule
            {
                Mode = OutputMode.Replace, Name = "return", Min = 0, Max = 0,
                Pattern = [new(OpCodes.Ret)], Output = [new(OpCodes.Stloc_S, result), new(OpCodes.Br, end)],
            },
            new Rule
            {
                Mode = OutputMode.MethodPostfix, Name = "postfixes",
                Output =
                [
                    new CodeInstruction(OpCodes.Nop).WithLabels(end),
                    new CodeInstruction(OpCodes.Nop).WithLabels(skip),
                    CodeInstruction.Annotation($"Postfix {PostfixLow.FullName}"),
                    new(OpCodes.Call, PostfixLow.MethodInfo),
                    new CodeInstruction(OpCodes.Nop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginCatchBlock, typeof(Exception))),
                    new(OpCodes.Dup),
                    new(OpCodes.Stloc_S, exception),
                    new(OpCodes.Call, InfoOf.ExceptionDispatchInfo_Capture),
                    new(OpCodes.Stloc_S, dispatchInfo),
                    new CodeInstruction(OpCodes.Nop).WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
                    new(OpCodes.Ldloca_S, exception),
                    CodeInstruction.Annotation($"Postfix {AlwaysPostfix.FullName}"),
                    new(OpCodes.Call, AlwaysPostfix.MethodInfo),
                    new(OpCodes.Ldloc_S, exception),
                    new(OpCodes.Brfalse_S, noThrow),
                    new(OpCodes.Ldloc_S, exception),
                    new(OpCodes.Ldloc_S, dispatchInfo),
                    new(OpCodes.Call, InfoOf.RuntimeHelpers_RethrowException),
                    new CodeInstruction(OpCodes.Nop).WithLabels(noThrow),
                    new(OpCodes.Ldloc_S, result),
                    new(OpCodes.Ret),
                ],
            },
        ]);
    }
}
