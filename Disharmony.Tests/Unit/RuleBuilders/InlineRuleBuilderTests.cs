using Disharmony.RuleBuilders;
using Disharmony.RulesEngine;
using static Disharmony.Tests.Support.RuleAssertions;

namespace Disharmony.Tests.Unit.RuleBuilders;

[TestFixture]
#if NETFRAMEWORK
[Timeout(5000)]
#endif
public sealed class InlineRuleBuilderTests
{
    // EmitReplacement tests supply explicit IL; TestTargets methods provide only signature/local metadata.
    // Non-void returns store before branching to the common exit, which only reloads the value.
    private sealed class InspectableInlineRuleBuilder(RuleBuilderContext context, MethodInvocation method)
        : InlineRuleBuilder(context, method)
    {
        public CodeInstruction[] Output => [.. output.instructions];
    }

    private static MethodInvocation Method(string name) => new(
        typeof(InlineRuleBuilderUnitTargets).GetMethod(name)!);

    [Test]
    public void BuildRules_EmptyVoidMethod_EmitsCallReplacementRule()
    {
        var method = Method(nameof(InlineRuleBuilderUnitTargets.Empty));
        var context = new RuleBuilderContext();
        var builder = new InlineRuleBuilder(context, method);

        Rule[] rules = [.. builder.BuildRules()];
        Label exit = rules.Single().Output!.SelectMany(i => i.labels).Single();

        AssertRules(rules,
        [
            new Rule
            {
                Mode = OutputMode.Replace, Name = method.FullName, Min = 0, Max = 0, Phase = 2,
                Pattern = [new(OpCodes.Call, method.MethodInfo)],
                Output =
                [
                    CodeInstruction.Annotation("Begin inlined method body"),
                    new(OpCodes.Br, exit),
                    CodeInstruction.Annotation("End inlined method body"),
                    new CodeInstruction(OpCodes.Nop).WithLabels(exit),
                ],
            },
        ]);
        Assert.That(context.locals, Is.Empty);
        Assert.That(builder.CrossRuleLabels, Is.Empty);
    }

    [Test]
    public void EmitReplacement_EmptyVoidMethod_BranchesToInlineExitWithoutLocals()
    {
        var method = Method(nameof(InlineRuleBuilderUnitTargets.Empty));
        var context = new RuleBuilderContext();
        var builder = new InspectableInlineRuleBuilder(context, method);

        Assert.That(builder.EmitReplacement(
        [
            new(OpCodes.Ret),
        ]), Is.True);
        CodeInstruction[] output = builder.Output;
        Label exit = output.SelectMany(i => i.labels).Single();

        AssertInstructions(output,
        [
            CodeInstruction.Annotation("Begin inlined method body"),
            new(OpCodes.Br, exit),
            CodeInstruction.Annotation("End inlined method body"),
            new CodeInstruction(OpCodes.Nop).WithLabels(exit),
        ]);
        Assert.That(context.locals, Is.Empty);
        Assert.That(builder.CrossRuleLabels, Is.Empty);
    }

    [Test]
    public void EmitReplacement_NarrowReturn_PreservesConversionAndTypedReturnLocal()
    {
        var method = Method(nameof(InlineRuleBuilderUnitTargets.IntToByte));
        var context = new RuleBuilderContext();
        var builder = new InspectableInlineRuleBuilder(context, method);

        Assert.That(builder.EmitReplacement(
        [
            new(OpCodes.Ldarg_0),
            new(OpCodes.Conv_U1),
            new(OpCodes.Ret),
        ]), Is.True);
        CodeInstruction[] output = builder.Output;
        Label exit = output.SelectMany(i => i.labels).Single();
        Assert.That(context.locals.Select(l => l.Type), Is.EqualTo(new[] { typeof(int), typeof(byte) }));
        LocalBuilder argument = context.locals[0].Builder;
        LocalBuilder result = context.locals[1].Builder;

        AssertInstructions(output,
        [
            new(OpCodes.Stloc_S, argument),
            CodeInstruction.Annotation("Begin inlined method body"),
            new(OpCodes.Ldloc_S, argument),
            new(OpCodes.Conv_U1),
            new(OpCodes.Stloc_S, result),
            new(OpCodes.Br, exit),
            CodeInstruction.Annotation("End inlined method body"),
            new CodeInstruction(OpCodes.Nop).WithLabels(exit),
            new(OpCodes.Ldloc_S, result),
        ]);
    }

    [Test]
    public void EmitReplacement_NullCoalescing_ConditionalBranchToReturnStoresCarriedValue()
    {
        var method = Method(nameof(InlineRuleBuilderUnitTargets.StringIdentity));
        var context = new RuleBuilderContext();
        var builder = new InspectableInlineRuleBuilder(context, method);

        Label originalReturn = context.generator.DefineLabel();
        Assert.That(builder.EmitReplacement(
        [
            new(OpCodes.Ldarg_0),
            new(OpCodes.Dup),
            new(OpCodes.Brtrue_S, originalReturn),
            new(OpCodes.Pop),
            new(OpCodes.Ldstr, "fallback"),
            new CodeInstruction(OpCodes.Ret).WithLabels(originalReturn),
        ]), Is.True);
        CodeInstruction[] output = builder.Output;
        Label store = (Label)output.Single(i => i.opcode == OpCodes.Brtrue_S).operand;
        Label exit = output.SelectMany(i => i.labels).Except([store]).Single();
        Assert.That(context.locals.Select(l => l.Type), Is.EqualTo(new[] { typeof(string), typeof(string) }));
        LocalBuilder argument = context.locals[0].Builder;
        LocalBuilder result = context.locals[1].Builder;

        AssertInstructions(output,
        [
            new(OpCodes.Stloc_S, argument),
            CodeInstruction.Annotation("Begin inlined method body"),
            new(OpCodes.Ldloc_S, argument),
            new(OpCodes.Dup),
            new(OpCodes.Brtrue_S, store),
            new(OpCodes.Pop),
            new(OpCodes.Ldstr, "fallback"),
            new CodeInstruction(OpCodes.Stloc_S, result).WithLabels(store),
            new(OpCodes.Br, exit),
            CodeInstruction.Annotation("End inlined method body"),
            new CodeInstruction(OpCodes.Nop).WithLabels(exit),
            new(OpCodes.Ldloc_S, result),
        ]);
    }

    [Test]
    public void EmitReplacement_MixedArguments_SavesInReverseOrderAndLoadsByOriginalIndex()
    {
        var method = Method(nameof(InlineRuleBuilderUnitTargets.MixedArguments));
        MethodInfo sink = Method(nameof(InlineRuleBuilderUnitTargets.MixedArguments)).MethodInfo;
        var context = new RuleBuilderContext();
        var builder = new InspectableInlineRuleBuilder(context, method);

        Assert.That(builder.EmitReplacement(
        [
            new(OpCodes.Ldarg_0),
            new(OpCodes.Ldarg_1),
            new(OpCodes.Ldarg_2),
            new(OpCodes.Call, sink),
            new(OpCodes.Ret),
        ]), Is.True);
        CodeInstruction[] output = builder.Output;
        Label exit = output.SelectMany(i => i.labels).Single();
        Assert.That(context.locals.Select(l => l.Type), Is.EqualTo(new[] { typeof(string), typeof(long), typeof(int) }));
        LocalBuilder text = context.locals[0].Builder;
        LocalBuilder wide = context.locals[1].Builder;
        LocalBuilder number = context.locals[2].Builder;

        AssertInstructions(output,
        [
            new(OpCodes.Stloc_S, text),
            new(OpCodes.Stloc_S, wide),
            new(OpCodes.Stloc_S, number),
            CodeInstruction.Annotation("Begin inlined method body"),
            new(OpCodes.Ldloc_S, number),
            new(OpCodes.Ldloc_S, wide),
            new(OpCodes.Ldloc_S, text),
            new(OpCodes.Call, sink),
            new(OpCodes.Br, exit),
            CodeInstruction.Annotation("End inlined method body"),
            new CodeInstruction(OpCodes.Nop).WithLabels(exit),
        ]);
    }

    [Test]
    public void EmitReplacement_ShortArgumentForms_MapLoadsAddressesAndStoresToSameLocal()
    {
        var method = Method(nameof(InlineRuleBuilderUnitTargets.FiveIntArguments));
        MethodInfo touch = Method(nameof(InlineRuleBuilderUnitTargets.RefInt)).MethodInfo;
        var context = new RuleBuilderContext();
        var builder = new InspectableInlineRuleBuilder(context, method);

        Assert.That(builder.EmitReplacement(
        [
            new(OpCodes.Ldarga_S, (byte)4),
            new(OpCodes.Call, touch),
            new(OpCodes.Ldarg_0),
            new(OpCodes.Starg_S, (byte)4),
            new(OpCodes.Ldarga_S, (byte)4),
            new(OpCodes.Call, touch),
            new(OpCodes.Ldarg_S, (byte)4),
            new(OpCodes.Ret),
        ]), Is.True);
        CodeInstruction[] output = builder.Output;
        Label exit = output.SelectMany(i => i.labels).Single();
        Assert.That(context.locals.Select(l => l.Type), Is.EqualTo(Enumerable.Repeat(typeof(int), 6)));
        LocalBuilder fifth = context.locals[0].Builder;
        LocalBuilder fourth = context.locals[1].Builder;
        LocalBuilder third = context.locals[2].Builder;
        LocalBuilder second = context.locals[3].Builder;
        LocalBuilder first = context.locals[4].Builder;
        LocalBuilder result = context.locals[5].Builder;

        AssertInstructions(output,
        [
            new(OpCodes.Stloc_S, fifth),
            new(OpCodes.Stloc_S, fourth),
            new(OpCodes.Stloc_S, third),
            new(OpCodes.Stloc_S, second),
            new(OpCodes.Stloc_S, first),
            CodeInstruction.Annotation("Begin inlined method body"),
            new(OpCodes.Ldloca_S, fifth),
            new(OpCodes.Call, touch),
            new(OpCodes.Ldloc_S, first),
            new(OpCodes.Stloc_S, fifth),
            new(OpCodes.Ldloca_S, fifth),
            new(OpCodes.Call, touch),
            new(OpCodes.Ldloc_S, fifth),
            new(OpCodes.Stloc_S, result),
            new(OpCodes.Br, exit),
            CodeInstruction.Annotation("End inlined method body"),
            new CodeInstruction(OpCodes.Nop).WithLabels(exit),
            new(OpCodes.Ldloc_S, result),
        ]);
    }

    [Test]
    public void EmitReplacement_LocalMacroAndShortForms_ReuseLocalsAndPreserveTheirAddresses()
    {
        var method = Method(nameof(InlineRuleBuilderUnitTargets.FiveIntLocals));
        MethodInfo touch = Method(nameof(InlineRuleBuilderUnitTargets.RefInt)).MethodInfo;
        var context = new RuleBuilderContext();
        var builder = new InspectableInlineRuleBuilder(context, method);

        Assert.That(builder.EmitReplacement(
        [
            new(OpCodes.Ldc_I4_1), new(OpCodes.Stloc_0),
            new(OpCodes.Ldc_I4_2), new(OpCodes.Stloc_1),
            new(OpCodes.Ldc_I4_3), new(OpCodes.Stloc_2),
            new(OpCodes.Ldc_I4_4), new(OpCodes.Stloc_3),
            new(OpCodes.Ldc_I4_5), new(OpCodes.Stloc_S, (byte)4),
            new(OpCodes.Ldloca_S, (byte)0), new(OpCodes.Call, touch),
            new(OpCodes.Ldloca_S, (byte)1), new(OpCodes.Call, touch),
            new(OpCodes.Ldloca_S, (byte)2), new(OpCodes.Call, touch),
            new(OpCodes.Ldloca_S, (byte)3), new(OpCodes.Call, touch),
            new(OpCodes.Ldloca_S, (byte)4), new(OpCodes.Call, touch),
            new(OpCodes.Ldloc_0),
            new(OpCodes.Ldloc_S, (byte)4),
            new(OpCodes.Add),
            new(OpCodes.Ret),
        ]), Is.True);
        CodeInstruction[] output = builder.Output;
        Label exit = output.SelectMany(i => i.labels).Single();
        Assert.That(context.locals.Select(l => l.Type), Is.EqualTo(Enumerable.Repeat(typeof(int), 6)));
        LocalBuilder result = context.locals[0].Builder;
        LocalBuilder a = context.locals[1].Builder;
        LocalBuilder b = context.locals[2].Builder;
        LocalBuilder c = context.locals[3].Builder;
        LocalBuilder d = context.locals[4].Builder;
        LocalBuilder e = context.locals[5].Builder;

        AssertInstructions(output,
        [
            CodeInstruction.Annotation("Begin inlined method body"),
            new(OpCodes.Ldc_I4_1), new(OpCodes.Stloc_S, a),
            new(OpCodes.Ldc_I4_2), new(OpCodes.Stloc_S, b),
            new(OpCodes.Ldc_I4_3), new(OpCodes.Stloc_S, c),
            new(OpCodes.Ldc_I4_4), new(OpCodes.Stloc_S, d),
            new(OpCodes.Ldc_I4_5), new(OpCodes.Stloc_S, e),
            new(OpCodes.Ldloca_S, a), new(OpCodes.Call, touch),
            new(OpCodes.Ldloca_S, b), new(OpCodes.Call, touch),
            new(OpCodes.Ldloca_S, c), new(OpCodes.Call, touch),
            new(OpCodes.Ldloca_S, d), new(OpCodes.Call, touch),
            new(OpCodes.Ldloca_S, e), new(OpCodes.Call, touch),
            new(OpCodes.Ldloc_S, a),
            new(OpCodes.Ldloc_S, e),
            new(OpCodes.Add),
            new(OpCodes.Stloc_S, result),
            new(OpCodes.Br, exit),
            CodeInstruction.Annotation("End inlined method body"),
            new CodeInstruction(OpCodes.Nop).WithLabels(exit),
            new(OpCodes.Ldloc_S, result),
        ]);
    }

    [Test]
    public void EmitReplacement_ConditionalReturns_RemapBranchTargetAndConvergeOnOneExit()
    {
        var method = Method(nameof(InlineRuleBuilderUnitTargets.BoolToString));
        var context = new RuleBuilderContext();
        var builder = new InspectableInlineRuleBuilder(context, method);

        Label originalOtherwise = context.generator.DefineLabel();
        Assert.That(builder.EmitReplacement(
        [
            new(OpCodes.Ldarg_0),
            new(OpCodes.Brfalse_S, originalOtherwise),
            new(OpCodes.Ldstr, "yes"),
            new(OpCodes.Ret),
            new CodeInstruction(OpCodes.Ldstr, "no").WithLabels(originalOtherwise),
            new(OpCodes.Ret),
        ]), Is.True);
        CodeInstruction[] output = builder.Output;
        Label otherwise = (Label)output.Single(i => i.opcode == OpCodes.Brfalse_S).operand;
        Label exit = output.SelectMany(i => i.labels).Except([otherwise]).Single();
        Assert.That(context.locals.Select(l => l.Type), Is.EqualTo(new[] { typeof(bool), typeof(string) }));
        LocalBuilder condition = context.locals[0].Builder;
        LocalBuilder result = context.locals[1].Builder;

        AssertInstructions(output,
        [
            new(OpCodes.Stloc_S, condition),
            CodeInstruction.Annotation("Begin inlined method body"),
            new(OpCodes.Ldloc_S, condition),
            new(OpCodes.Brfalse_S, otherwise),
            new(OpCodes.Ldstr, "yes"),
            new(OpCodes.Stloc_S, result),
            new(OpCodes.Br, exit),
            new CodeInstruction(OpCodes.Ldstr, "no").WithLabels(otherwise),
            new(OpCodes.Stloc_S, result),
            new(OpCodes.Br, exit),
            CodeInstruction.Annotation("End inlined method body"),
            new CodeInstruction(OpCodes.Nop).WithLabels(exit),
            new(OpCodes.Ldloc_S, result),
        ]);
    }

    [Test]
    public void EmitReplacement_Loop_RemapForwardAndBackwardBranchesIncludingTranslatedTargets()
    {
        var method = Method(nameof(InlineRuleBuilderUnitTargets.IntIdentity));
        var context = new RuleBuilderContext();
        var builder = new InspectableInlineRuleBuilder(context, method);

        Label originalCondition = context.generator.DefineLabel();
        Label originalBody = context.generator.DefineLabel();
        Assert.That(builder.EmitReplacement(
        [
            new(OpCodes.Br_S, originalCondition),
            new CodeInstruction(OpCodes.Ldarg_0).WithLabels(originalBody),
            new(OpCodes.Ldc_I4_1),
            new(OpCodes.Sub),
            new(OpCodes.Starg_S, (byte)0),
            new CodeInstruction(OpCodes.Ldarg_0).WithLabels(originalCondition),
            new(OpCodes.Ldc_I4_0),
            new(OpCodes.Bgt_S, originalBody),
            new(OpCodes.Ldarg_0),
            new(OpCodes.Ret),
        ]), Is.True);
        CodeInstruction[] output = builder.Output;
        Label condition = (Label)output.Single(i => i.opcode == OpCodes.Br_S).operand;
        Label body = (Label)output.Single(i => i.opcode == OpCodes.Bgt_S).operand;
        Label exit = output.SelectMany(i => i.labels).Except([condition, body]).Single();
        Assert.That(new[] { condition, body, exit }, Is.Unique);
        Assert.That(context.locals.Select(l => l.Type), Is.EqualTo(new[] { typeof(int), typeof(int) }));
        LocalBuilder argument = context.locals[0].Builder;
        LocalBuilder result = context.locals[1].Builder;

        AssertInstructions(output,
        [
            new(OpCodes.Stloc_S, argument),
            CodeInstruction.Annotation("Begin inlined method body"),
            new(OpCodes.Br_S, condition),
            new CodeInstruction(OpCodes.Ldloc_S, argument).WithLabels(body),
            new(OpCodes.Ldc_I4_1),
            new(OpCodes.Sub),
            new(OpCodes.Stloc_S, argument),
            new CodeInstruction(OpCodes.Ldloc_S, argument).WithLabels(condition),
            new(OpCodes.Ldc_I4_0),
            new(OpCodes.Bgt_S, body),
            new(OpCodes.Ldloc_S, argument),
            new(OpCodes.Stloc_S, result),
            new(OpCodes.Br, exit),
            CodeInstruction.Annotation("End inlined method body"),
            new CodeInstruction(OpCodes.Nop).WithLabels(exit),
            new(OpCodes.Ldloc_S, result),
        ]);
    }

    [Test]
    public void EmitReplacement_Switch_RemapEveryCaseAndUnifyAllReturns()
    {
        var method = Method(nameof(InlineRuleBuilderUnitTargets.IntIdentity));
        var context = new RuleBuilderContext();
        var builder = new InspectableInlineRuleBuilder(context, method);

        Label[] originalCases = [context.generator.DefineLabel(), context.generator.DefineLabel(),
            context.generator.DefineLabel(), context.generator.DefineLabel(), context.generator.DefineLabel()];
        Label originalFallback = context.generator.DefineLabel();
        Assert.That(builder.EmitReplacement(
        [
            new(OpCodes.Ldarg_0),
            new(OpCodes.Switch, originalCases),
            new(OpCodes.Br_S, originalFallback),
            new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)10).WithLabels(originalCases[0]),
            new(OpCodes.Ret),
            new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)11).WithLabels(originalCases[1]),
            new(OpCodes.Ret),
            new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)12).WithLabels(originalCases[2]),
            new(OpCodes.Ret),
            new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)13).WithLabels(originalCases[3]),
            new(OpCodes.Ret),
            new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)14).WithLabels(originalCases[4]),
            new(OpCodes.Ret),
            new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)99).WithLabels(originalFallback),
            new(OpCodes.Ret),
        ]), Is.True);
        CodeInstruction[] output = builder.Output;
        Label[] cases = (Label[])output.Single(i => i.opcode == OpCodes.Switch).operand;
        Assert.That(cases, Has.Length.EqualTo(5).And.Unique);
        Label fallback = (Label)output.Single(i => i.opcode == OpCodes.Br_S).operand;
        Label exit = output.SelectMany(i => i.labels).Except(cases).Except([fallback]).Single();
        Assert.That(context.locals.Select(l => l.Type), Is.EqualTo(new[] { typeof(int), typeof(int) }));
        LocalBuilder argument = context.locals[0].Builder;
        LocalBuilder result = context.locals[1].Builder;

        AssertInstructions(output,
        [
            new(OpCodes.Stloc_S, argument),
            CodeInstruction.Annotation("Begin inlined method body"),
            new(OpCodes.Ldloc_S, argument),
            new(OpCodes.Switch, new[] { cases[0], cases[1], cases[2], cases[3], cases[4] }),
            new(OpCodes.Br_S, fallback),
            new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)10).WithLabels(cases[0]),
            new(OpCodes.Stloc_S, result),
            new(OpCodes.Br, exit),
            new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)11).WithLabels(cases[1]),
            new(OpCodes.Stloc_S, result),
            new(OpCodes.Br, exit),
            new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)12).WithLabels(cases[2]),
            new(OpCodes.Stloc_S, result),
            new(OpCodes.Br, exit),
            new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)13).WithLabels(cases[3]),
            new(OpCodes.Stloc_S, result),
            new(OpCodes.Br, exit),
            new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)14).WithLabels(cases[4]),
            new(OpCodes.Stloc_S, result),
            new(OpCodes.Br, exit),
            new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)99).WithLabels(fallback),
            new(OpCodes.Stloc_S, result),
            new(OpCodes.Br, exit),
            CodeInstruction.Annotation("End inlined method body"),
            new CodeInstruction(OpCodes.Nop).WithLabels(exit),
            new(OpCodes.Ldloc_S, result),
        ]);
    }

    [Test]
    public void EmitReplacement_ClassInstance_SavesReceiverAndPreservesFieldAccess()
    {
        var method = Method(nameof(InlineRuleBuilderUnitTargets.InstanceValue));
        FieldInfo field = typeof(InlineRuleBuilderUnitTargets).GetField(nameof(InlineRuleBuilderUnitTargets.Value))!;
        var context = new RuleBuilderContext();
        var builder = new InspectableInlineRuleBuilder(context, method);

        Assert.That(builder.EmitReplacement(
        [
            new(OpCodes.Ldarg_0),
            new(OpCodes.Ldfld, field),
            new(OpCodes.Ret),
        ]), Is.True);
        CodeInstruction[] output = builder.Output;
        Label exit = output.SelectMany(i => i.labels).Single();
        Assert.That(context.locals.Select(l => l.Type), Is.EqualTo(new[] { typeof(InlineRuleBuilderUnitTargets), typeof(int) }));
        LocalBuilder receiver = context.locals[0].Builder;
        LocalBuilder result = context.locals[1].Builder;

        AssertInstructions(output,
        [
            new(OpCodes.Stloc_S, receiver),
            CodeInstruction.Annotation("Begin inlined method body"),
            new(OpCodes.Ldloc_S, receiver),
            new(OpCodes.Ldfld, field),
            new(OpCodes.Stloc_S, result),
            new(OpCodes.Br, exit),
            CodeInstruction.Annotation("End inlined method body"),
            new CodeInstruction(OpCodes.Nop).WithLabels(exit),
            new(OpCodes.Ldloc_S, result),
        ]);
    }

    [Test]
    public void EmitReplacement_StructInstance_PreservesByRefReceiver()
    {
        var method = new MethodInvocation(typeof(InlineRuleBuilderStructTarget)
            .GetMethod(nameof(InlineRuleBuilderStructTarget.InstanceValue))!);
        FieldInfo field = typeof(InlineRuleBuilderStructTarget).GetField(nameof(InlineRuleBuilderStructTarget.Value))!;
        var context = new RuleBuilderContext();
        var builder = new InspectableInlineRuleBuilder(context, method);

        Assert.That(builder.EmitReplacement(
        [
            new(OpCodes.Ldarg_0),
            new(OpCodes.Ldfld, field),
            new(OpCodes.Ret),
        ]), Is.True);
        CodeInstruction[] output = builder.Output;
        Label exit = output.SelectMany(i => i.labels).Single();
        Assert.That(context.locals.Select(l => l.Type),
            Is.EqualTo(new[] { typeof(InlineRuleBuilderStructTarget).MakeByRefType(), typeof(int) }));
        LocalBuilder receiver = context.locals[0].Builder;
        LocalBuilder result = context.locals[1].Builder;

        AssertInstructions(output,
        [
            new(OpCodes.Stloc_S, receiver),
            CodeInstruction.Annotation("Begin inlined method body"),
            new(OpCodes.Ldloc_S, receiver),
            new(OpCodes.Ldfld, field),
            new(OpCodes.Stloc_S, result),
            new(OpCodes.Br, exit),
            CodeInstruction.Annotation("End inlined method body"),
            new CodeInstruction(OpCodes.Nop).WithLabels(exit),
            new(OpCodes.Ldloc_S, result),
        ]);
    }

    [Test]
    public void EmitReplacement_ExistingContextLocals_UsesFreshLongIndexLocals()
    {
        var method = Method(nameof(InlineRuleBuilderUnitTargets.IntToByte));
        var context = new RuleBuilderContext();
        var existing = context.NewInstructionList();
        for (int i = 0; i < 256; i++)
            existing.AddLocal(typeof(string));
        var originalLocals = context.locals.ToArray();
        var builder = new InspectableInlineRuleBuilder(context, method);

        Assert.That(builder.EmitReplacement(
        [
            new(OpCodes.Ldarg_0),
            new(OpCodes.Conv_U1),
            new(OpCodes.Ret),
        ]), Is.True);
        CodeInstruction[] output = builder.Output;
        Label exit = output.SelectMany(i => i.labels).Single();
        Assert.That(context.locals.Take(256), Is.EqualTo(originalLocals));
        Assert.That(context.locals, Has.Count.EqualTo(258));
        LocalBuilder argument = context.locals[256].Builder;
        LocalBuilder result = context.locals[257].Builder;
        Assert.That(argument.LocalType, Is.EqualTo(typeof(int)));
        Assert.That(result.LocalType, Is.EqualTo(typeof(byte)));

        AssertInstructions(output,
        [
            new(OpCodes.Stloc, argument),
            CodeInstruction.Annotation("Begin inlined method body"),
            new(OpCodes.Ldloc, argument),
            new(OpCodes.Conv_U1),
            new(OpCodes.Stloc, result),
            new(OpCodes.Br, exit),
            CodeInstruction.Annotation("End inlined method body"),
            new CodeInstruction(OpCodes.Nop).WithLabels(exit),
            new(OpCodes.Ldloc, result),
        ]);
    }

    [Test]
    public void BuildRules_AbstractMethod_EmitsNoRule()
    {
        var method = new MethodInvocation(typeof(InlineRuleBuilderAbstractTarget)
            .GetMethod(nameof(InlineRuleBuilderAbstractTarget.Method))!);
        var context = new RuleBuilderContext();
        var builder = new InlineRuleBuilder(context, method);

        Rule[] rules = [.. builder.BuildRules()];

        AssertRules(rules, []);
        Assert.That(context.locals, Is.Empty);
        Assert.That(builder.CrossRuleLabels, Is.Empty);
    }

    [Test]
    public void BuildRules_CatchRegion_EmitsNoRule()
    {
        var method = Method(nameof(InlineRuleBuilderUnitTargets.Catch));
        var context = new RuleBuilderContext();
        var builder = new InlineRuleBuilder(context, method);

        Rule[] rules = [.. builder.BuildRules()];

        AssertRules(rules, []);
        Assert.That(builder.CrossRuleLabels, Is.Empty);
    }

    [Test]
    public void BuildRules_FinallyRegion_EmitsNoRule()
    {
        var method = Method(nameof(InlineRuleBuilderUnitTargets.Finally));
        var context = new RuleBuilderContext();
        var builder = new InlineRuleBuilder(context, method);

        Rule[] rules = [.. builder.BuildRules()];

        AssertRules(rules, []);
        Assert.That(builder.CrossRuleLabels, Is.Empty);
    }
}
