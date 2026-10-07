namespace Disharmony.RuleBuilders;

/// <summary>
///     This class generates rules implementing the <see cref="PatchOptions.Inline" /> patch option for a method.
/// </summary>
internal class InlineRuleBuilder : RuleBuilder
{
    private readonly MethodBase method;
    private readonly LocalTracker[] argumentLocals;
    private LocalTracker? returnLocal = null;
    private readonly Dictionary<int, LocalTracker> localMap = [];
    private readonly Dictionary<Label, Label> labelMap = [];
    private readonly Type[] parameterTypes;
    private readonly List<LocalVariableInfo>? locals;

    public InlineRuleBuilder(RuleBuilderContext context, MethodInvocation patch) : base(context, EmptyInvocation.Instance)
    {
        method = patch.MethodInfo;

        parameterTypes = patch.ParameterTypes;
        argumentLocals = new LocalTracker[parameterTypes.Length];
        locals = method.GetMethodBody()?.LocalVariables.ToList();
    }

    private bool EmitReplacement()
    {
        if (locals is null)
            throw new InvalidOperationException();

        var instructions = PatchProcessor.GetOriginalInstructions(method);
        if (instructions == null)
            return false;

        // Inlining a method containing exception blocks may result in a stack slot being live
        // across an exception region boundary, which the CLI rejects.
        if (instructions.Any(i => i.blocks.Count > 0))
            return false;

        return EmitReplacement(instructions);
    }

    // Internal for testing
    internal bool EmitReplacement(List<CodeInstruction> instructions)
    {
        for (int i = parameterTypes.Length - 1; i >= 0; i--)
        {
            argumentLocals[i] = output.AddLocal(parameterTypes[i]);
            output.Add(argumentLocals[i].Store());
        }

        if (method is MethodInfo m && m.ReturnType != typeof(void))
            returnLocal = output.AddLocal(m.ReturnType);

        Label returnLabel = generator.DefineLabel();

        output.Add(CodeInstruction.Annotation("Begin inlined method body"));

        foreach (var inst in instructions)
        {
            ushort canonicalOpcode = OpCodeData.GetCanonicalOpcode(inst);

            if (canonicalOpcode == OpCodeValues.Ret && returnLocal != null)
            {
                output.Add(returnLocal.Store().WithLabels(inst.labels.Select(GetLabel))
                    .WithBlocks(inst.blocks.Where(b => b.blockType != ExceptionBlockType.EndExceptionBlock)));
                output.Add(new CodeInstruction(OpCodes.Br, returnLabel).WithBlocks(inst.blocks.Where(b =>
                    b.blockType == ExceptionBlockType.EndExceptionBlock)));
                continue;
            }

            CodeInstruction translated = canonicalOpcode switch
            {
                // @formatter:off
                OpCodeValues.Ldarg    => GetArgument(inst).Load(),
                OpCodeValues.Ldarga   => GetArgument(inst).Load(true),
                OpCodeValues.Starg    => GetArgument(inst).Store(),
                OpCodeValues.Ldloc    => GetLocal(inst).Load(),
                OpCodeValues.Ldloca   => GetLocal(inst).Load(true),
                OpCodeValues.Stloc    => GetLocal(inst).Store(),
                OpCodeValues.Ret      => new(OpCodes.Br, returnLabel),
                _ when inst.operand is Label label => new(inst.opcode, GetLabel(label)),
                _ when inst.operand is Label[] labels => new(inst.opcode, labels.Select(GetLabel).ToArray()),
                _ => inst,
                // @formatter:on
            };

            translated.labels = [.. inst.labels.Select(GetLabel)];
            translated.blocks = inst.blocks;

            output.Add(translated);
        }

        output.Add(CodeInstruction.Annotation("End inlined method body"));

        output.Add(new(OpCodes.Nop) { labels = [returnLabel] });

        if (returnLocal != null)
            output.Add(returnLocal.Load());

        return true;
    }

    private LocalTracker GetArgument(CodeInstruction inst) => argumentLocals[OpCodeData.GetIntOperand(inst)];

    private LocalTracker GetLocal(CodeInstruction inst) => GetLocal(LocalTracker.IndexFrom(inst));

    private LocalTracker GetLocal(int index)
    {
        if (locals is null)
            throw new InvalidOperationException();

        if (!localMap.TryGetValue(index, out var value))
            localMap[index] = value = output.AddLocal(locals[index].LocalType);
        return value;
    }

    private Label GetLabel(Label label)
    {
        if (!labelMap.TryGetValue(label, out var value))
            labelMap[label] = value = output.generator.DefineLabel();
        return value;
    }

    public override IEnumerable<Rule> BuildRules()
    {
        if (locals == null)
            yield break;

        if (!EmitReplacement())
            yield break;

        yield return new Rule
        {
            Min = 0,
            Max = 0,
            Phase = 2,
            Mode = OutputMode.Replace,
            Pattern = [new(OpCodes.Call, method)],
            Output = [.. output.instructions],
            Name = method.FullName,
        };
    }
}
