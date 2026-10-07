namespace Disharmony.RulesEngine;

internal class Processor(
    Ruleset ruleset,
    MethodBase method,
    List<CodeInstruction> inInstructions,
    ILGenerator generator,
    bool debug)
{
    internal class MatchData
    {
        public required Rule rule;
        public int start, end;
        public required Dictionary<int, LocalTracker> localMap_Match;
        public required Dictionary<Label, Label> labelMap_Match;
        public bool emitted = false;
    }

    private readonly Dictionary<int, LocalTracker> localMap_Method = [];
    private readonly Dictionary<Label, Label> labelMap_Method = [];
    private readonly List<MatchData> matches = [];

    public List<CodeInstruction> outInstructions = [];
    private List<CodeInstruction> instructions = inInstructions;

    public void MatchAndReplace()
    {
        if (debug)
            FileLog.Log($"## InfixPatcher {method.FullName}");

        if (ruleset.Rules.Count == 0)
        {
            outInstructions = instructions;
            return;
        }

        foreach (var phase in ruleset.Rules.GroupBy(r => r.Phase).OrderBy(p => p.Key))
        {
            matches.Clear();
            outInstructions = [];

            // Check and make sure that all the substitutions apply. Also work out the indexes of all locals.
            foreach (Rule rule in phase)
            {
                switch (rule)
                {
                    case { Mode: OutputMode.MethodPrefix or OutputMode.MethodPostfix, Pattern.Length: > 0 }:
                        throw new InvalidOperationException($"{rule.Mode} cannot have a Pattern");
                    case { Mode: not (OutputMode.MethodPrefix or OutputMode.MethodPostfix), Pattern: not { Length: > 0 } }:
                        throw new InvalidOperationException($"{rule.Mode} rule must have a Pattern");
                }

                switch (rule.Mode)
                {
                    case OutputMode.MethodPrefix:
                    {
                        matches.Add(new MatchData
                        {
                            rule = rule,
                            start = 0,
                            end = 0,
                            localMap_Match = [],
                            labelMap_Match = [],
                        });
                        continue;
                    }
                    case OutputMode.MethodPostfix:
                    {
                        matches.Add(new MatchData
                        {
                            rule = rule,
                            start = instructions.Count,
                            end = instructions.Count,
                            localMap_Match = [],
                            labelMap_Match = [],
                        });
                        continue;
                    }
                }

                var matchCount = 0;

                // rule.Pattern was checked to be non-null during the error checking above
                if (rule.Pattern is null)
                    throw new InvalidOperationException();

                for (int instructionIndex = 0;
                     instructionIndex <= instructions.Count - rule.Pattern.Length;
                     instructionIndex++)
                {
                    if (!MatchPattern(rule, instructionIndex, out var localMap_Match, out var labelMap_Match))
                        continue;

                    var matchData = new MatchData
                    {
                        rule = rule,
                        start = instructionIndex,
                        end = instructionIndex + rule.Pattern.Length,
                        localMap_Match = localMap_Match,
                        labelMap_Match = labelMap_Match,
                    };
                    if (debug)
                        FileLog.Log($"MATCH {rule.Name} ({matchData.start} .. {matchData.end - 1})");

                    if (rule.Output != null)
                        matches.Add(matchData);

                    matchCount++;
                    if (rule.Max > 0 && matchCount >= rule.Max)
                        break;
                }

                if (matchCount < rule.Min)
                    throw new InvalidOperationException($"Not enough matches found for substitution {rule.Name}");
            }

            var sortedMatches = matches.OrderBy(m => m.start).ThenBy(m => m.end).ThenByDescending(m => m.rule.Priority).ToList();
            for (var i = 0; i < sortedMatches.Count - 1; i++)
            {
                if (sortedMatches[i].end > sortedMatches[i + 1].start)
                    throw new InvalidOperationException("Overlapping matches");
            }

            if (matches.Count == 0)
            {
                outInstructions = instructions;
                continue;
            }

            // Make the substitutions
            for (var instructionIndex = 0;; instructionIndex++)
            {
                int index = instructionIndex;
                var match = sortedMatches.FirstOrDefault(r => r.start == index && !r.emitted);

                if (match == null)
                {
                    if (instructionIndex >= instructions.Count)
                        break;

                    Emit(instructions[instructionIndex]);

                    continue;
                }

                match.emitted = true;
                instructionIndex = match.end - 1;

                if (match.start == match.end)
                {
                    EmitReplacement(match);
                    continue;
                }

                static bool IsBlockStart(ExceptionBlock b) => b.blockType != ExceptionBlockType.EndExceptionBlock;
                static bool IsBlockEnd(ExceptionBlock b) => b.blockType == ExceptionBlockType.EndExceptionBlock;

                switch (match.rule.Mode)
                {
                    case OutputMode.Replace:
                    {
                        if (instructions[match.start].labels.Count > 0 || instructions[match.start].blocks.Any(IsBlockStart))
                            Emit(OpCodes.Nop,
                                labels: instructions[match.start].labels,
                                blocks: [.. instructions[match.start].blocks.Where(IsBlockStart)]);

                        EmitReplacement(match);

                        if (instructions[match.end - 1].blocks.Any(IsBlockEnd))
                            Emit(OpCodes.Nop,
                                blocks: [.. instructions[match.end - 1].blocks.Where(IsBlockEnd)]);

                        break;
                    }
                    case OutputMode.InsertBefore:
                    {
                        if (instructions[match.start].labels.Count > 0 || instructions[match.start].blocks.Any(IsBlockStart))
                            Emit(OpCodes.Nop,
                                labels: instructions[match.start].labels,
                                blocks: [.. instructions[match.start].blocks.Where(IsBlockStart)]);

                        EmitReplacement(match);

                        for (int i = match.start; i < match.end; i++)
                        {
                            Emit(instructions[i]);

                            if (i == match.start)
                            {
                                outInstructions[^1].labels.Clear();
                                outInstructions[^1].blocks.RemoveAll(IsBlockStart);
                            }
                        }

                        break;
                    }
                    case OutputMode.InsertAfter:
                    {
                        for (int i = match.start; i < match.end; i++)
                        {
                            Emit(instructions[i]);

                            if (i == match.end - 1)
                                outInstructions[^1].blocks.RemoveAll(IsBlockEnd);
                        }

                        EmitReplacement(match);

                        if (instructions[match.end - 1].blocks.Any(IsBlockEnd))
                            Emit(OpCodes.Nop,
                                blocks: [.. instructions[match.end - 1].blocks.Where(IsBlockEnd)]);

                        break;
                    }
                    default: throw new InvalidOperationException();
                }
            }

            instructions = outInstructions;
        }

        if (debug)
        {
            LogInstructions();
            FileLog.Log("");
        }
    }

    private void LogInstructions()
    {
        int codePos = 0;

        foreach (var codeInstruction in outInstructions)
        {
            foreach (var label in codeInstruction.labels)
                FileLog.LogIL(codePos, label);
            foreach (var block in codeInstruction.blocks)
                FileLog.LogILBlockBegin(codePos, block);

            var code = codeInstruction.opcode;
            var operand = codeInstruction.operand;

            var realCode = true;
            switch (code.OperandType)
            {
                case OperandType.InlineNone:
                    if (code == OpCodes.Nop && operand is string s)
                    {
                        FileLog.LogILComment(codePos, s);
                        realCode = false;
                    }
                    else
                    {
                        FileLog.LogIL(codePos, code);
                    }

                    break;

                //case OperandType.InlineSig:
                //    FileLog.LogIL(codePos, code, (ICallSiteGenerator)operand);
                //    break;

                default: FileLog.LogIL(codePos, code, operand); break;
            }

            foreach (var block in codeInstruction.blocks)
                FileLog.LogILBlockEnd(codePos, block);
            if (realCode)
                codePos += ReflectionTools.ILSize(codeInstruction.opcode);
        }

        FileLog.FlushBuffer();
    }

    private void EmitReplacement(MatchData match)
    {
        Emit(CodeInstruction.Annotation($"Begin {match.rule.Name}"));

        // Rules with null Output aren't added to matches so should never get here
        if (match.rule.Output is null)
            throw new InvalidOperationException();

        foreach (CodeInstruction replaceInst in match.rule.Output)
            EmitReplacement(replaceInst, match);

        Emit(CodeInstruction.Annotation($"End {match.rule.Name}"));
    }

    private bool MatchPattern(
        Rule rule,
        int instructionIndex,
        out Dictionary<int, LocalTracker> localMap_Match,
        out Dictionary<Label, Label> labelMap_Match)
    {
        localMap_Match = [];
        labelMap_Match = [];

        // rule.Pattern is checked to be non-null before this is called
        if (rule.Pattern is null)
            throw new InvalidOperationException();

        bool noOutput = rule.Output is { Length: 0 };

        for (var patternIndex = 0; patternIndex < rule.Pattern.Length; patternIndex++)
        {
            if (!MatchInstruction(instructions[instructionIndex + patternIndex], rule.Pattern[patternIndex], localMap_Match,
                    labelMap_Match))
                return false;

            // Check for exception blocks or labels not at the start (or end, for EndExceptionBlock) of the match
            // which would be lost when the instructions are replaced
            if (rule is { Mode: OutputMode.Replace, Output: not null })
            {
                CodeInstruction inst = instructions[instructionIndex + patternIndex];
                if ((patternIndex > 0 || noOutput) && inst.blocks.Any(b => b.blockType != ExceptionBlockType.EndExceptionBlock))
                    return false;
                if ((patternIndex > 0 || noOutput) && inst.labels.Count > 0)
                    return false;
                if ((patternIndex < rule.Pattern.Length - 1 || noOutput) &&
                    inst.blocks.Any(b => b.blockType == ExceptionBlockType.EndExceptionBlock))
                    return false;
            }
        }

        return true;
    }

    private bool MatchInstruction(
        CodeInstruction inst,
        CodeInstruction patternInst,
        Dictionary<int, LocalTracker> localMap_Match,
        Dictionary<Label, Label> labelMap_Match)
    {
        var canonicalInst = OpCodeData.GetCanonicalOpcode(inst);
        var canonicalPattern = OpCodeData.GetCanonicalOpcode(patternInst);

        if (canonicalInst != canonicalPattern &&
            !(canonicalInst == OpCodeValues.Callvirt && canonicalPattern == OpCodeValues.Call))
            return false;

        switch (canonicalPattern)
        {
            case OpCodeValues.Stloc:
            case OpCodeValues.Ldloc:
            case OpCodeValues.Ldloca:
            {
                int localIndex = LocalTracker.IndexFrom(patternInst);
                var targetLocal = LocalTracker.From(inst);

                if (localMap_Match.TryGetValue(localIndex, out var substituteLocal))
                    return targetLocal == substituteLocal;

                localMap_Match.Add(localIndex, targetLocal);
                return true;
            }

            case OpCodeValues.Starg:
            case OpCodeValues.Ldarg:
            case OpCodeValues.Ldarga:
            case OpCodeValues.Ldc_I4:
                return OpCodeData.GetIntOperand(inst) == OpCodeData.GetIntOperand(patternInst);

            case var _ when patternInst.operand is Label label:
            {
                var targetLabel = (Label)inst.operand;
                if (labelMap_Match.TryGetValue(label, out var substituteLabel))
                    return targetLabel == substituteLabel;

                labelMap_Match.Add(label, targetLabel);
                return true;
            }

            case var _ when patternInst.operand is Label[] labels:
            {
                var targetLabels = (Label[])inst.operand;

                if (labels.Length != targetLabels.Length)
                    return false;

                Dictionary<Label, Label> tempLabelMap = [];
                for (int i = 0; i < labels.Length; i++)
                {
                    if (labelMap_Match.TryGetValue(labels[i], out var substituteLabel) && substituteLabel != targetLabels[i])
                        return false;
                    if (tempLabelMap.TryGetValue(labels[i], out substituteLabel) && substituteLabel != targetLabels[i])
                        return false;
                    tempLabelMap[labels[i]] = targetLabels[i];
                }

                for (int i = 0; i < labels.Length; i++)
                    labelMap_Match[labels[i]] = targetLabels[i];
                return true;
            }

            default: return OperandsMatch(patternInst.operand, inst.operand);
        }
    }

    private static bool OperandsMatch(object? a, object? b)
    {
        if (a == null)
            return b == null;
        return Type.GetTypeCode(a.GetType()) switch
        {
            >= TypeCode.Boolean and <= TypeCode.Int64 => Convert.ToInt64(a) == Convert.ToInt64(b),
            TypeCode.Single or TypeCode.Double => OperandsMatch(Convert.ToDouble(a), Convert.ToDouble(b)),
            _ => Equals(a, b),
        };
    }

    private static bool OperandsMatch(double a, double b)
    {
        if (a == b)
            return true;
        return BitConverter.DoubleToInt64Bits(a) == BitConverter.DoubleToInt64Bits(b);
    }

    private void EmitReplacement(CodeInstruction replaceInst, MatchData match)
    {
        CodeInstruction inst = OpCodeData.GetCanonicalOpcode(replaceInst) switch
        {
            OpCodeValues.Stloc => GetReplacementLocal(replaceInst, match).Store(),
            OpCodeValues.Ldloca => GetReplacementLocal(replaceInst, match).Load(true),
            OpCodeValues.Ldloc => GetReplacementLocal(replaceInst, match).Load(),
            _ when replaceInst.operand is Label label => new(replaceInst.opcode, GetReplacementLabel(label, match)),
            _ when replaceInst.operand is Label[] labels => new(replaceInst.opcode,
                labels.Select(label2 => GetReplacementLabel(label2, match)).ToArray()),
            _ => new(replaceInst.opcode, replaceInst.operand),
        };

        if (replaceInst.blocks.Count > 0)
            inst.blocks.AddRange(replaceInst.blocks);

        if (replaceInst.labels.Count > 0)
            inst.labels.AddRange(replaceInst.labels.Select(label => GetReplacementLabel(label, match)));

        Emit(inst);
    }

    private void Emit(CodeInstruction instruction)
    {
        Emit(instruction.opcode, instruction.operand, instruction.labels, instruction.blocks);
    }

    private void Emit(
        OpCode opcode,
        object? operand = null,
        List<Label>? labels = null,
        List<ExceptionBlock>? blocks = null)
    {
        CodeInstruction newInstruction = new(opcode, operand);
        if (labels != null)
            newInstruction.labels.AddRange(labels);
        if (blocks != null)
            newInstruction.blocks.AddRange(blocks);

        outInstructions.Add(newInstruction);
    }

    private Label GetReplacementLabel(Label label, MatchData match)
    {
        Dictionary<Label, Label> labelMap = ruleset.CrossRuleLabels.Contains(label) ? labelMap_Method : match.labelMap_Match;
        if (!labelMap.TryGetValue(label, out Label replacementLabel))
        {
            replacementLabel = generator.DefineLabel();
            labelMap.Add(label, replacementLabel);
        }

        return replacementLabel;
    }

    private LocalTracker GetReplacementLocal(CodeInstruction replaceInst, MatchData match)
    {
        int localIndex = LocalTracker.IndexFrom(replaceInst);

        if (replaceInst.operand is LocalBuilder { LocalType: Type } builder)
        {
            var localMap = ruleset.CrossRuleLocals.Contains(builder) ? localMap_Method : match.localMap_Match;
            if (!localMap.TryGetValue(localIndex, out var substituteLocal))
            {
                substituteLocal = new LocalTrackerBuilder(generator.DeclareLocal(builder.LocalType));
                localMap.Add(localIndex, substituteLocal);
            }

            return substituteLocal;
        }
        else
        {
            if (match.localMap_Match.TryGetValue(localIndex, out var substituteLocal))
                return substituteLocal;
        }

        throw new InvalidOperationException($"Can't replace local #{localIndex} because its type is unknown");
    }
}
