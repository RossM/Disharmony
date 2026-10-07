using System.Runtime.ExceptionServices;
using Disharmony.RuleBuilders;
using Disharmony.RulesEngine;
using static Disharmony.Tests.Support.RuleAssertions;
using BoundParameter = Disharmony.ParameterBinding;

namespace Disharmony.Tests.Unit.RuleBuilders;

[TestFixture]
public sealed class InfixRuleBuilderTests
{
    private static readonly MethodInvocation Outer = new(
        typeof(InfixRuleBuilderTargets).GetMethod(nameof(InfixRuleBuilderTargets.Outer))!);

    private static readonly MethodInvocation InnerVoid = new(
        typeof(InfixRuleBuilderTargets).GetMethod(nameof(InfixRuleBuilderTargets.InnerVoid))!);

    private static readonly MethodInvocation InnerInt = new(
        typeof(InfixRuleBuilderTargets).GetMethod(nameof(InfixRuleBuilderTargets.InnerInt))!);

    private static readonly MethodInvocation Combine = new(
        typeof(InfixRuleBuilderTargets).GetMethod(nameof(InfixRuleBuilderTargets.Combine))!);

    private static readonly MethodInvocation Increment = new(
        typeof(InfixRuleBuilderTargets).GetMethod(nameof(InfixRuleBuilderTargets.Increment))!);

    private static readonly MethodInvocation InstanceInner = new(
        typeof(InfixRuleBuilderTargets).GetMethod(nameof(InfixRuleBuilderTargets.InstanceInner))!);

    private static readonly MethodInvocation PrefixLow = new(
        typeof(InfixRuleBuilderTargets).GetMethod(nameof(InfixRuleBuilderTargets.PrefixLow))!);

    private static readonly MethodInvocation PrefixHigh = new(
        typeof(InfixRuleBuilderTargets).GetMethod(nameof(InfixRuleBuilderTargets.PrefixHigh))!);

    private static readonly MethodInvocation PostfixLow = new(
        typeof(InfixRuleBuilderTargets).GetMethod(nameof(InfixRuleBuilderTargets.PostfixLow))!);

    private static readonly MethodInvocation PostfixHigh = new(
        typeof(InfixRuleBuilderTargets).GetMethod(nameof(InfixRuleBuilderTargets.PostfixHigh))!);

    private static readonly MethodInvocation BooleanPrefix = new(
        typeof(InfixRuleBuilderTargets).GetMethod(nameof(InfixRuleBuilderTargets.BooleanPrefix))!);

    private static readonly MethodInvocation InnerArgumentsPrefix = new(
        typeof(InfixRuleBuilderTargets).GetMethod(nameof(InfixRuleBuilderTargets.InnerArgumentsPrefix))!);

    private static readonly MethodInvocation ReadIntPrefix = new(
        typeof(InfixRuleBuilderTargets).GetMethod(nameof(InfixRuleBuilderTargets.ReadIntPrefix))!);

    private static readonly MethodInvocation ReadOuterPrefix = new(
        typeof(InfixRuleBuilderTargets).GetMethod(nameof(InfixRuleBuilderTargets.ReadOuterPrefix))!);

    private static readonly MethodInvocation ReadInstancePrefix = new(
        typeof(InfixRuleBuilderTargets).GetMethod(nameof(InfixRuleBuilderTargets.ReadInstancePrefix))!);

    private static readonly MethodInvocation ResultPostfix = new(
        typeof(InfixRuleBuilderTargets).GetMethod(nameof(InfixRuleBuilderTargets.ResultPostfix))!);

    private static readonly MethodInvocation AlwaysPrefix = new(
        typeof(InfixRuleBuilderTargets).GetMethod(nameof(InfixRuleBuilderTargets.AlwaysPrefix))!);

    private static readonly MethodInvocation AlwaysPostfix = new(
        typeof(InfixRuleBuilderTargets).GetMethod(nameof(InfixRuleBuilderTargets.AlwaysPostfix))!);

    private static PatchInfo CreatePatch(
        Invocation patch,
        PatchKind patchKind,
        Invocation inner,
        BoundParameter[]? parameters = null,
        PatchOptions options = PatchOptions.Default,
        int priority = 0) => new()
    {
        unpatchKey = 0,
        inner = inner,
        patch = patch,
        patchKind = patchKind,
        parameters = parameters ?? [],
        options = options,
        priority = priority,
    };

    [Test]
    public void BuildRules_AllowMissingInnerTarget_PrefixOnly_Minimum0()
    {
        var context = new RuleBuilderContext();
        PatchInfo[] patches =
        [
            CreatePatch(PrefixLow, PatchKind.Prefix, InnerVoid, options: PatchOptions.AllowMissingInnerTarget),
        ];
        var builder = new InfixRuleBuilder(context, Outer, InnerVoid, [.. patches]);

        Rule[] rules = [.. builder.BuildRules()];

        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.Replace, Name = InnerVoid.FullName, Min = 0, Max = 0,
                Pattern = [new(OpCodes.Call, InnerVoid.MethodInfo)],
                Output =
                [
                    CodeInstruction.Annotation($"Prefix {PrefixLow.FullName}"),
                    new(OpCodes.Call, PrefixLow.MethodInfo),
                    new(OpCodes.Call, InnerVoid.MethodInfo),
                ],
            },
        ]);
    }

    [Test]
    public void BuildRules_AllowMissingInnerTarget_PostfixOnly_Minimum0()
    {
        var context = new RuleBuilderContext();
        PatchInfo[] patches =
        [
            CreatePatch(PostfixLow, PatchKind.Postfix, InnerVoid, options: PatchOptions.AllowMissingInnerTarget),
        ];
        var builder = new InfixRuleBuilder(context, Outer, InnerVoid, [.. patches]);

        Rule[] rules = [.. builder.BuildRules()];

        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.Replace, Name = InnerVoid.FullName, Min = 0, Max = 0,
                Pattern = [new(OpCodes.Call, InnerVoid.MethodInfo)],
                Output =
                [
                    new(OpCodes.Call, InnerVoid.MethodInfo),
                    CodeInstruction.Annotation($"Postfix {PostfixLow.FullName}"),
                    new(OpCodes.Call, PostfixLow.MethodInfo),
                ],
            },
        ]);
    }

    [Test]
    public void BuildRules_AllowMissingInnerTarget_BothAllow_Minimum0()
    {
        var context = new RuleBuilderContext();
        PatchInfo[] patches =
        [
            CreatePatch(PrefixLow, PatchKind.Prefix, InnerVoid, options: PatchOptions.AllowMissingInnerTarget),
            CreatePatch(PostfixLow, PatchKind.Postfix, InnerVoid, options: PatchOptions.AllowMissingInnerTarget),
        ];
        var builder = new InfixRuleBuilder(context, Outer, InnerVoid, [.. patches]);

        Rule[] rules = [.. builder.BuildRules()];

        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.Replace, Name = InnerVoid.FullName, Min = 0, Max = 0,
                Pattern = [new(OpCodes.Call, InnerVoid.MethodInfo)],
                Output =
                [
                    CodeInstruction.Annotation($"Prefix {PrefixLow.FullName}"),
                    new(OpCodes.Call, PrefixLow.MethodInfo),
                    new(OpCodes.Call, InnerVoid.MethodInfo),
                    CodeInstruction.Annotation($"Postfix {PostfixLow.FullName}"),
                    new(OpCodes.Call, PostfixLow.MethodInfo),
                ],
            },
        ]);
    }

    [Test]
    public void BuildRules_AllowMissingInnerTarget_NeitherAllows_Minimum1()
    {
        var context = new RuleBuilderContext();
        PatchInfo[] patches =
        [
            CreatePatch(PrefixLow, PatchKind.Prefix, InnerVoid, options: PatchOptions.Default),
            CreatePatch(PostfixLow, PatchKind.Postfix, InnerVoid, options: PatchOptions.Default),
        ];
        var builder = new InfixRuleBuilder(context, Outer, InnerVoid, [.. patches]);

        Rule[] rules = [.. builder.BuildRules()];

        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.Replace, Name = InnerVoid.FullName, Min = 1, Max = 0,
                Pattern = [new(OpCodes.Call, InnerVoid.MethodInfo)],
                Output =
                [
                    CodeInstruction.Annotation($"Prefix {PrefixLow.FullName}"),
                    new(OpCodes.Call, PrefixLow.MethodInfo),
                    new(OpCodes.Call, InnerVoid.MethodInfo),
                    CodeInstruction.Annotation($"Postfix {PostfixLow.FullName}"),
                    new(OpCodes.Call, PostfixLow.MethodInfo),
                ],
            },
        ]);
    }

    [Test]
    public void BuildRules_AllowMissingInnerTarget_OnlyPrefixAllows_Minimum1()
    {
        var context = new RuleBuilderContext();
        PatchInfo[] patches =
        [
            CreatePatch(PrefixLow, PatchKind.Prefix, InnerVoid, options: PatchOptions.AllowMissingInnerTarget),
            CreatePatch(PostfixLow, PatchKind.Postfix, InnerVoid, options: PatchOptions.Default),
        ];
        var builder = new InfixRuleBuilder(context, Outer, InnerVoid, [.. patches]);

        Rule[] rules = [.. builder.BuildRules()];

        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.Replace, Name = InnerVoid.FullName, Min = 1, Max = 0,
                Pattern = [new(OpCodes.Call, InnerVoid.MethodInfo)],
                Output =
                [
                    CodeInstruction.Annotation($"Prefix {PrefixLow.FullName}"),
                    new(OpCodes.Call, PrefixLow.MethodInfo),
                    new(OpCodes.Call, InnerVoid.MethodInfo),
                    CodeInstruction.Annotation($"Postfix {PostfixLow.FullName}"),
                    new(OpCodes.Call, PostfixLow.MethodInfo),
                ],
            },
        ]);
    }

    [Test]
    public void BuildRules_AllowMissingInnerTarget_OnlyPostfixAllows_Minimum1()
    {
        var context = new RuleBuilderContext();
        PatchInfo[] patches =
        [
            CreatePatch(PrefixLow, PatchKind.Prefix, InnerVoid, options: PatchOptions.Default),
            CreatePatch(PostfixLow, PatchKind.Postfix, InnerVoid, options: PatchOptions.AllowMissingInnerTarget),
        ];
        var builder = new InfixRuleBuilder(context, Outer, InnerVoid, [.. patches]);

        Rule[] rules = [.. builder.BuildRules()];

        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.Replace, Name = InnerVoid.FullName, Min = 1, Max = 0,
                Pattern = [new(OpCodes.Call, InnerVoid.MethodInfo)],
                Output =
                [
                    CodeInstruction.Annotation($"Prefix {PrefixLow.FullName}"),
                    new(OpCodes.Call, PrefixLow.MethodInfo),
                    new(OpCodes.Call, InnerVoid.MethodInfo),
                    CodeInstruction.Annotation($"Postfix {PostfixLow.FullName}"),
                    new(OpCodes.Call, PostfixLow.MethodInfo),
                ],
            },
        ]);
    }

    [Test]
    public void BuildRules_NoPatches_PreservesInnerInvocationStackContract()
    {
        var context = new RuleBuilderContext();
        var builder = new InfixRuleBuilder(context, Outer, Combine, []);

        Rule[] rules = [.. builder.BuildRules()];

        Assert.Multiple(() =>
        {
            AssertRules(rules,
            [
                new Rule
                {
                    Mode = OutputMode.Replace, Name = Combine.FullName, Min = 0, Max = 0,
                    Pattern = [new(OpCodes.Call, Combine.MethodInfo)],
                    Output =
                    [
                        new(OpCodes.Stloc_S, context.locals[0].Builder),
                        new(OpCodes.Stloc_S, context.locals[1].Builder),
                        new(OpCodes.Ldloc_S, context.locals[1].Builder),
                        new(OpCodes.Ldloc_S, context.locals[0].Builder),
                        new(OpCodes.Call, Combine.MethodInfo),
                    ],
                },
            ]);
            Assert.That(context.locals.Select(l => l.Type), Is.EqualTo(new[] { typeof(string), typeof(int) }));
        });
    }

    [Test]
    public void BuildRules_Priorities_RunHigherPrefixFirstAndHigherPostfixLast()
    {
        var context = new RuleBuilderContext();
        PatchInfo[] patches =
        [
            CreatePatch(PrefixLow, PatchKind.Prefix, InnerVoid, priority: -10),
            CreatePatch(PostfixLow, PatchKind.Postfix, InnerVoid, priority: -10),
            CreatePatch(PrefixHigh, PatchKind.Prefix, InnerVoid, priority: 10),
            CreatePatch(PostfixHigh, PatchKind.Postfix, InnerVoid, priority: 10),
            CreatePatch(PrefixLow, PatchKind.Prefix, EmptyInvocation.Instance),
            CreatePatch(PostfixHigh, PatchKind.Postfix, EmptyInvocation.Instance),
        ];
        var builder = new InfixRuleBuilder(context, Outer, InnerVoid, [.. patches]);

        Rule[] rules = [.. builder.BuildRules()];
        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.Replace, Name = InnerVoid.FullName, Min = 1, Max = 0,
                Pattern = [new(OpCodes.Call, InnerVoid.MethodInfo)],
                Output =
                [
                    CodeInstruction.Annotation($"Prefix {PrefixHigh.FullName}"),
                    new(OpCodes.Call, PrefixHigh.MethodInfo),
                    CodeInstruction.Annotation($"Prefix {PrefixLow.FullName}"),
                    new(OpCodes.Call, PrefixLow.MethodInfo),
                    new(OpCodes.Call, InnerVoid.MethodInfo),
                    CodeInstruction.Annotation($"Postfix {PostfixLow.FullName}"),
                    new(OpCodes.Call, PostfixLow.MethodInfo),
                    CodeInstruction.Annotation($"Postfix {PostfixHigh.FullName}"),
                    new(OpCodes.Call, PostfixHigh.MethodInfo),
                ],
            },
        ]);
    }

    [Test]
    public void BuildRules_EqualPriorityPatchPairs_NestInRegistrationOrder()
    {
        var context = new RuleBuilderContext();
        PatchInfo[] patches =
        [
            CreatePatch(PrefixLow, PatchKind.Prefix, InnerVoid),
            CreatePatch(PostfixLow, PatchKind.Postfix, InnerVoid),
            CreatePatch(PrefixHigh, PatchKind.Prefix, InnerVoid),
            CreatePatch(PostfixHigh, PatchKind.Postfix, InnerVoid),
        ];
        var builder = new InfixRuleBuilder(context, Outer, InnerVoid, [.. patches]);

        Rule[] rules = [.. builder.BuildRules()];
        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.Replace, Name = InnerVoid.FullName, Min = 1, Max = 0,
                Pattern = [new(OpCodes.Call, InnerVoid.MethodInfo)],
                Output =
                [
                    CodeInstruction.Annotation($"Prefix {PrefixHigh.FullName}"),
                    new(OpCodes.Call, PrefixHigh.MethodInfo),
                    CodeInstruction.Annotation($"Prefix {PrefixLow.FullName}"),
                    new(OpCodes.Call, PrefixLow.MethodInfo),
                    new(OpCodes.Call, InnerVoid.MethodInfo),
                    CodeInstruction.Annotation($"Postfix {PostfixLow.FullName}"),
                    new(OpCodes.Call, PostfixLow.MethodInfo),
                    CodeInstruction.Annotation($"Postfix {PostfixHigh.FullName}"),
                    new(OpCodes.Call, PostfixHigh.MethodInfo),
                ],
            },
        ]);
    }

    [Test]
    public void BuildRules_VoidPrefix_DoesNotCreateResultLocalOrSkipBranch()
    {
        var context = new RuleBuilderContext();
        PatchInfo prefix = CreatePatch(PrefixLow, PatchKind.Prefix, InnerInt);
        var builder = new InfixRuleBuilder(context, Outer, InnerInt, [prefix]);

        Rule[] rules = [.. builder.BuildRules()];

        Assert.Multiple(() =>
        {
            Assert.That(context.locals, Has.Count.EqualTo(1));
            Assert.That(context.locals[0].Type, Is.EqualTo(typeof(int)));
            AssertRules(rules,
            [
                new Rule
                {
                    Mode = OutputMode.Replace, Name = InnerInt.FullName, Min = 1, Max = 0,
                    Pattern = [new(OpCodes.Call, InnerInt.MethodInfo)],
                    Output =
                    [
                        new(OpCodes.Stloc_S, context.locals[0].Builder),
                        CodeInstruction.Annotation($"Prefix {PrefixLow.FullName}"),
                        new(OpCodes.Call, PrefixLow.MethodInfo),
                        new(OpCodes.Ldloc_S, context.locals[0].Builder),
                        new(OpCodes.Call, InnerInt.MethodInfo),
                    ],
                },
            ]);
        });
    }

    [Test]
    public void BuildRules_BooleanPrefix_BranchesAroundInnerInvocationAndReturnsInitializedResult()
    {
        var context = new RuleBuilderContext();
        PatchInfo prefix = CreatePatch(BooleanPrefix, PatchKind.Prefix, InnerInt);
        var builder = new InfixRuleBuilder(context, Outer, InnerInt, [prefix]);

        Rule[] rules = [.. builder.BuildRules()];
        Label skip = rules[0].Output!.SelectMany(i => i.labels).Single();

        Assert.Multiple(() =>
        {
            Assert.That(context.locals.Select(l => l.Type), Is.EqualTo(new[] { typeof(int), typeof(int) }));
            AssertRules(rules,
            [
                new Rule
                {
                    Mode = OutputMode.Replace, Name = InnerInt.FullName, Min = 1, Max = 0,
                    Pattern = [new(OpCodes.Call, InnerInt.MethodInfo)],
                    Output =
                    [
                        new(OpCodes.Stloc_S, context.locals[0].Builder),
                        new(OpCodes.Ldc_I4_0),
                        new(OpCodes.Stloc_S, context.locals[1].Builder),
                        CodeInstruction.Annotation($"Prefix {BooleanPrefix.FullName}"),
                        new(OpCodes.Call, BooleanPrefix.MethodInfo),
                        new(OpCodes.Brfalse, skip),
                        new(OpCodes.Ldloc_S, context.locals[0].Builder),
                        new(OpCodes.Call, InnerInt.MethodInfo),
                        new(OpCodes.Stloc_S, context.locals[1].Builder),
                        new CodeInstruction(OpCodes.Nop).WithLabels(skip),
                        new(OpCodes.Ldloc_S, context.locals[1].Builder),
                    ],
                },
            ]);
        });
    }

    [Test]
    public void BuildRules_InnerParameters_ReadByValueAndWriteByReferenceUseSavedArguments()
    {
        var context = new RuleBuilderContext();
        ParameterInfo[] patchParameters = InnerArgumentsPrefix.MethodInfo.GetParameters();
        var number = new BoundParameter
        {
            parameter = patchParameters[0],
            bindingType = BindingType.Argument,
            scope = Scope.Inner,
            index = 0,
        };
        var text = new BoundParameter
        {
            parameter = patchParameters[1],
            bindingType = BindingType.Argument,
            scope = Scope.Inner,
            index = 1,
        };
        PatchInfo prefix = CreatePatch(
            InnerArgumentsPrefix, PatchKind.Prefix, Combine, parameters: [number, text]);
        var builder = new InfixRuleBuilder(context, Outer, Combine, [prefix]);

        Rule[] rules = [.. builder.BuildRules()];

        Assert.Multiple(() =>
        {
            AssertRules(rules,
            [
                new Rule
                {
                    Mode = OutputMode.Replace, Name = Combine.FullName, Min = 1, Max = 0,
                    Pattern = [new(OpCodes.Call, Combine.MethodInfo)],
                    Output =
                    [
                        new(OpCodes.Stloc_S, context.locals[0].Builder),
                        new(OpCodes.Stloc_S, context.locals[1].Builder),
                        new(OpCodes.Ldloc_S, context.locals[1].Builder),
                        new(OpCodes.Ldloca_S, context.locals[0].Builder),
                        CodeInstruction.Annotation($"Prefix {InnerArgumentsPrefix.FullName}"),
                        new(OpCodes.Call, InnerArgumentsPrefix.MethodInfo),
                        new(OpCodes.Ldloc_S, context.locals[1].Builder),
                        new(OpCodes.Ldloc_S, context.locals[0].Builder),
                        new(OpCodes.Call, Combine.MethodInfo),
                    ],
                },
            ]);
        });
    }

    [Test]
    public void BuildRules_ByRefInnerParameterReadByValue_DereferencesSavedReference()
    {
        var context = new RuleBuilderContext();
        var value = new BoundParameter
        {
            parameter = ReadIntPrefix.MethodInfo.GetParameters()[0],
            bindingType = BindingType.Argument,
            scope = Scope.Inner,
            index = 0,
        };
        PatchInfo prefix = CreatePatch(ReadIntPrefix, PatchKind.Prefix, Increment, parameters: [value]);
        var builder = new InfixRuleBuilder(context, Outer, Increment, [prefix]);

        Rule[] rules = [.. builder.BuildRules()];

        Assert.Multiple(() =>
        {
            Assert.That(context.locals.Single().Type, Is.EqualTo(typeof(int).MakeByRefType()));
            AssertRules(rules,
            [
                new Rule
                {
                    Mode = OutputMode.Replace, Name = Increment.FullName, Min = 1, Max = 0,
                    Pattern = [new(OpCodes.Call, Increment.MethodInfo)],
                    Output =
                    [
                        new(OpCodes.Stloc_S, context.locals[0].Builder),
                        new(OpCodes.Ldloc_S, context.locals[0].Builder),
                        new(OpCodes.Ldobj, typeof(int)),
                        CodeInstruction.Annotation($"Prefix {ReadIntPrefix.FullName}"),
                        new(OpCodes.Call, ReadIntPrefix.MethodInfo),
                        new(OpCodes.Ldloc_S, context.locals[0].Builder),
                        new(OpCodes.Call, Increment.MethodInfo),
                    ],
                },
            ]);
        });
    }

    [Test]
    public void BuildRules_OuterParameterBinding_LoadsOuterArgument()
    {
        var context = new RuleBuilderContext();
        var outerValue = new BoundParameter
        {
            parameter = ReadOuterPrefix.MethodInfo.GetParameters()[0],
            bindingType = BindingType.Argument,
            scope = Scope.Outer,
            index = 0,
        };
        PatchInfo prefix = CreatePatch(ReadOuterPrefix, PatchKind.Prefix, InnerVoid, parameters: [outerValue]);
        var builder = new InfixRuleBuilder(context, Outer, InnerVoid, [prefix]);

        Rule[] rules = [.. builder.BuildRules()];

        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.Replace, Name = InnerVoid.FullName, Min = 1, Max = 0,
                Pattern = [new(OpCodes.Call, InnerVoid.MethodInfo)],
                Output =
                [
                    new(OpCodes.Ldarg_0),
                    CodeInstruction.Annotation($"Prefix {ReadOuterPrefix.FullName}"),
                    new(OpCodes.Call, ReadOuterPrefix.MethodInfo),
                    new(OpCodes.Call, InnerVoid.MethodInfo),
                ],
            },
        ]);
    }

    [Test]
    public void BuildRules_InnerInstanceBinding_LoadsSavedReceiver()
    {
        var context = new RuleBuilderContext();
        var instance = new BoundParameter
        {
            parameter = ReadInstancePrefix.MethodInfo.GetParameters()[0],
            bindingType = BindingType.Instance,
            scope = Scope.Inner,
            index = 0,
        };
        PatchInfo prefix = CreatePatch(
            ReadInstancePrefix, PatchKind.Prefix, InstanceInner, parameters: [instance]);
        var builder = new InfixRuleBuilder(context, Outer, InstanceInner, [prefix]);

        Rule[] rules = [.. builder.BuildRules()];

        Assert.Multiple(() =>
        {
            Assert.That(context.locals.Select(l => l.Type),
                Is.EqualTo(new[] { typeof(int), typeof(InfixRuleBuilderTargets) }));
            AssertRules(rules,
            [
                new Rule
                {
                    Mode = OutputMode.Replace, Name = InstanceInner.FullName, Min = 1, Max = 0,
                    Pattern = [new(OpCodes.Callvirt, InstanceInner.MethodInfo)],
                    Output =
                    [
                        new(OpCodes.Stloc_S, context.locals[0].Builder),
                        new(OpCodes.Stloc_S, context.locals[1].Builder),
                        new(OpCodes.Ldloc_S, context.locals[1].Builder),
                        CodeInstruction.Annotation($"Prefix {ReadInstancePrefix.FullName}"),
                        new(OpCodes.Call, ReadInstancePrefix.MethodInfo),
                        new(OpCodes.Ldloc_S, context.locals[1].Builder),
                        new(OpCodes.Ldloc_S, context.locals[0].Builder),
                        new(OpCodes.Callvirt, InstanceInner.MethodInfo),
                    ],
                },
            ]);
        });
    }

    [Test]
    public void BuildRules_PostfixResultBinding_StoresPassesAndReloadsResult()
    {
        var context = new RuleBuilderContext();
        var result = new BoundParameter
        {
            parameter = ResultPostfix.MethodInfo.GetParameters()[0],
            bindingType = BindingType.Result,
            scope = Scope.Inner,
        };
        PatchInfo postfix = CreatePatch(ResultPostfix, PatchKind.Postfix, InnerInt, parameters: [result]);
        var builder = new InfixRuleBuilder(context, Outer, InnerInt, [postfix]);

        Rule[] rules = [.. builder.BuildRules()];

        Assert.Multiple(() =>
        {
            Assert.That(context.locals.Select(l => l.Type), Is.EqualTo(new[] { typeof(int), typeof(int) }));
            AssertRules(rules,
            [
                new Rule
                {
                    Mode = OutputMode.Replace, Name = InnerInt.FullName, Min = 1, Max = 0,
                    Pattern = [new(OpCodes.Call, InnerInt.MethodInfo)],
                    Output =
                    [
                        new(OpCodes.Stloc_S, context.locals[0].Builder),
                        new(OpCodes.Ldc_I4_0),
                        new(OpCodes.Stloc_S, context.locals[1].Builder),
                        new(OpCodes.Ldloc_S, context.locals[0].Builder),
                        new(OpCodes.Call, InnerInt.MethodInfo),
                        new(OpCodes.Stloc_S, context.locals[1].Builder),
                        new(OpCodes.Ldloca_S, context.locals[1].Builder),
                        CodeInstruction.Annotation($"Postfix {ResultPostfix.FullName}"),
                        new(OpCodes.Call, ResultPostfix.MethodInfo),
                        new(OpCodes.Ldloc_S, context.locals[1].Builder),
                    ],
                },
            ]);
        });
    }

    [Test]
    public void BuildRules_StateBinding_LoadsLocalAssignedByStateBuilder()
    {
        var context = new RuleBuilderContext();
        var state = new BoundParameter
        {
            parameter = ReadIntPrefix.MethodInfo.GetParameters()[0],
            bindingType = BindingType.State,
            scope = Scope.Outer,
            stateKey = "shared",
        };
        PatchInfo prefix = CreatePatch(ReadIntPrefix, PatchKind.Prefix, InnerVoid, parameters: [state]);
        var stateBuilder = new StateBuilder(context);
        stateBuilder.AssignStateVariableIndexes([prefix]);
        var builder = new InfixRuleBuilder(context, Outer, InnerVoid, [prefix]);

        Rule[] rules = [.. builder.BuildRules()];

        Assert.Multiple(() =>
        {
            Assert.That(state.local, Is.Not.Null);
            AssertRules(rules,
            [
                new Rule
                {
                    Mode = OutputMode.Replace, Name = InnerVoid.FullName, Min = 1, Max = 0,
                    Pattern = [new(OpCodes.Call, InnerVoid.MethodInfo)],
                    Output =
                    [
                        new(OpCodes.Ldloc_S, state.local!.Builder),
                        CodeInstruction.Annotation($"Prefix {ReadIntPrefix.FullName}"),
                        new(OpCodes.Call, ReadIntPrefix.MethodInfo),
                        new(OpCodes.Call, InnerVoid.MethodInfo),
                    ],
                },
            ]);
        });
    }

    [Test]
    public void BuildRules_AlwaysRunPatches_EncloseRegularWorkAndRunAtOuterEdges()
    {
        var context = new RuleBuilderContext();
        var exception = new BoundParameter
        {
            parameter = AlwaysPostfix.MethodInfo.GetParameters()[0],
            bindingType = BindingType.Exception,
            scope = Scope.Any,
        };
        PatchInfo[] patches =
        [
            CreatePatch(PrefixLow, PatchKind.Prefix, InnerInt),
            CreatePatch(PostfixLow, PatchKind.Postfix, InnerInt),
            CreatePatch(
                AlwaysPrefix,
                PatchKind.Prefix,
                InnerInt,
                options: PatchOptions.AlwaysRun),
            CreatePatch(
                AlwaysPostfix,
                PatchKind.Postfix,
                InnerInt,
                parameters: [exception],
                options: PatchOptions.AlwaysRun),
        ];
        var builder = new InfixRuleBuilder(context, Outer, InnerInt, [.. patches]);

        Rule[] rules = [.. builder.BuildRules()];
        LocalBuilder argumentLocal = context.locals[0].Builder;
        LocalBuilder resultLocal = context.locals[1].Builder;
        LocalBuilder exceptionLocal = context.locals[2].Builder;
        LocalBuilder dispatchInfoLocal = context.locals[3].Builder;
        Label noThrowLabel = rules[0].Output!.SelectMany(i => i.labels).Single();
        Rule[] expected =
        [
            new Rule
            {
                Mode = OutputMode.Replace, Name = InnerInt.FullName, Min = 1, Max = 0,
                Pattern = [new(OpCodes.Call, InnerInt.MethodInfo)],
                Output =
                [
                    new(OpCodes.Stloc_S, argumentLocal),
                    new(OpCodes.Ldc_I4_0),
                    new(OpCodes.Stloc_S, resultLocal),
                    new(OpCodes.Ldnull),
                    new(OpCodes.Stloc_S, exceptionLocal),
                    new(OpCodes.Ldnull),
                    new(OpCodes.Stloc_S, dispatchInfoLocal),
                    CodeInstruction.Annotation($"Prefix {AlwaysPrefix.FullName}"),
                    new(OpCodes.Call, AlwaysPrefix.MethodInfo),
                    new CodeInstruction(OpCodes.Nop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
                    CodeInstruction.Annotation($"Prefix {PrefixLow.FullName}"),
                    new(OpCodes.Call, PrefixLow.MethodInfo),
                    new(OpCodes.Ldloc_S, argumentLocal),
                    new(OpCodes.Call, InnerInt.MethodInfo),
                    new(OpCodes.Stloc_S, resultLocal),
                    CodeInstruction.Annotation($"Postfix {PostfixLow.FullName}"),
                    new(OpCodes.Call, PostfixLow.MethodInfo),
                    new CodeInstruction(OpCodes.Nop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginCatchBlock, typeof(Exception))),
                    new(OpCodes.Dup),
                    new(OpCodes.Stloc_S, exceptionLocal),
                    new(OpCodes.Call, InfoOf.ExceptionDispatchInfo_Capture),
                    new(OpCodes.Stloc_S, dispatchInfoLocal),
                    new CodeInstruction(OpCodes.Nop).WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
                    new(OpCodes.Ldloc_S, exceptionLocal),
                    CodeInstruction.Annotation($"Postfix {AlwaysPostfix.FullName}"),
                    new(OpCodes.Call, AlwaysPostfix.MethodInfo),
                    new(OpCodes.Ldloc_S, exceptionLocal),
                    new(OpCodes.Brfalse_S, noThrowLabel),
                    new(OpCodes.Ldloc_S, exceptionLocal),
                    new(OpCodes.Ldloc_S, dispatchInfoLocal),
                    new(OpCodes.Call, InfoOf.RuntimeHelpers_RethrowException),
                    new CodeInstruction(OpCodes.Nop).WithLabels(noThrowLabel),
                    new(OpCodes.Ldloc_S, resultLocal),
                ],
            },
        ];

        Assert.Multiple(() =>
        {
            Assert.That(context.locals.Select(l => l.Type), Is.EqualTo(new[]
            {
                typeof(int),
                typeof(int),
                typeof(Exception),
                typeof(ExceptionDispatchInfo),
            }));
            AssertRules(rules, expected);
        });
    }

    [Test]
    public void BuildRules_AnyScopeParameterBinding_ThrowsArgumentOutOfRangeException()
    {
        var context = new RuleBuilderContext();
        var value = new BoundParameter
        {
            parameter = ReadIntPrefix.MethodInfo.GetParameters()[0],
            bindingType = BindingType.Argument,
            scope = Scope.Any,
            index = 0,
        };
        PatchInfo prefix = CreatePatch(ReadIntPrefix, PatchKind.Prefix, InnerInt, parameters: [value]);
        var builder = new InfixRuleBuilder(context, Outer, InnerInt, [prefix]);

        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            builder.BuildRules().Single())!;

        Assert.That(exception.ParamName, Is.EqualTo("scope"));
    }
}
