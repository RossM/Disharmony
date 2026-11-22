using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using HarmonyLib;
using WaylithClr;

namespace Disharmony.WaylithCLR;

internal sealed class MethodEmitter(MethodBase method, MethodBodyBuilder builder, InstructionAllocator allocator,
    List<(int offset, CodeInstruction instruction)> log)
{
    private sealed class Target(LabelHandle handle)
    {
        public readonly LabelHandle Handle = handle;
        // Index in the emitted instruction list for stack analysis. A forward branch can
        // create the label handle before the target is emitted, so its index starts at -1.
        public int Index = -1;
    }

    private sealed class Region(LabelHandle tryStart, Label end)
    {
        public readonly LabelHandle TryStart = tryStart;
        public readonly Label End = end;
        public LabelHandle TryEnd;
        public LabelHandle FilterStart;
        public LabelHandle HandlerStart;
        public ExceptionRegionKind Kind;
        public Type? CatchType;
    }

    private readonly InstructionEncoder il = new(new BlobBuilder(), new ControlFlowBuilder());
    private readonly Dictionary<Label, Target> targets = [];
    // Stack analysis follows the emitted instructions, including any added at exception boundaries.
    private readonly List<CodeInstruction> instructions = [];
    private readonly List<(int index, int depth)> handlerEntries = [];
    private readonly Stack<Region> regions = [];
    private bool hasLocalloc;

    public void Write(List<CodeInstruction> code, StandaloneSignatureHandle locals, bool initLocals)
    {
        foreach (var instruction in code)
        {
            foreach (var block in instruction.blocks)
                if (block.blockType != ExceptionBlockType.EndExceptionBlock)
                    BeginBlock(block);

            // A handler transition may emit a terminator for the preceding block.
            // Branches to this instruction must land after that terminator.
            foreach (Label label in instruction.labels)
                Mark(label);

            log.Add((il.Offset, instruction));
            if (!(instruction.opcode == OpCodes.Nop && instruction.operand is string))
                WriteInstruction(instruction);

            foreach (var block in instruction.blocks)
                if (block.blockType == ExceptionBlockType.EndExceptionBlock)
                    EndBlock();
        }

        if (regions.Count != 0)
            throw new InvalidOperationException("An exception block was left open.");

        var output = new BlobBuilder();
        new MethodBodyStreamEncoder(output).AddMethodBody(il, GetMaxStack(), locals,
            initLocals ? MethodBodyAttributes.InitLocals : MethodBodyAttributes.None, hasLocalloc);
        builder.Write(output.ToArray());
    }

    private Target GetTarget(Label label)
    {
        if (!targets.TryGetValue(label, out var target))
            targets.Add(label, target = new Target(il.DefineLabel()));
        return target;
    }

    private void Mark(Label label)
    {
        var target = GetTarget(label);
        if (target.Index >= 0)
            throw new InvalidOperationException($"Label {label.GetHashCode()} was marked twice.");
        target.Index = instructions.Count;
        il.MarkLabel(target.Handle);
    }

    private LabelHandle MarkBoundary()
    {
        var label = il.DefineLabel();
        il.MarkLabel(label);
        return label;
    }

    private void BeginBlock(ExceptionBlock block)
    {
        if (block.blockType == ExceptionBlockType.BeginExceptionBlock)
        {
            regions.Push(new Region(MarkBoundary(), allocator.DefineLabel()));
            return;
        }

        var region = regions.Peek();
        if (!region.FilterStart.IsNil && region.HandlerStart.IsNil)
        {
            // Harmony marks the handler of a filter with a catch block that has no catch type.
            if (block.blockType != ExceptionBlockType.BeginCatchBlock || block.catchType is not null)
                throw new InvalidOperationException("An exception filter must be followed by its handler.");
            EnsureTerminator(OpCodes.Endfilter);
            region.HandlerStart = MarkBoundary();
            handlerEntries.Add((instructions.Count, 1));
            return;
        }

        if (region.TryEnd.IsNil)
        {
            LeaveIfNeeded(region.End);
            region.TryEnd = MarkBoundary();
        }
        else
        {
            CloseHandler(region);
        }

        region.FilterStart = default;
        region.HandlerStart = default;
        if (block.blockType == ExceptionBlockType.BeginExceptFilterBlock)
        {
            region.Kind = ExceptionRegionKind.Filter;
            region.FilterStart = MarkBoundary();
            handlerEntries.Add((instructions.Count, 1));
            return;
        }

        region.Kind = block.blockType switch
        {
            ExceptionBlockType.BeginCatchBlock => ExceptionRegionKind.Catch,
            ExceptionBlockType.BeginFinallyBlock => ExceptionRegionKind.Finally,
            ExceptionBlockType.BeginFaultBlock => ExceptionRegionKind.Fault,
            _ => throw new NotSupportedException($"Unsupported exception block {block.blockType}."),
        };
        region.CatchType = block.catchType;
        region.HandlerStart = MarkBoundary();
        handlerEntries.Add((instructions.Count, region.Kind == ExceptionRegionKind.Catch ? 1 : 0));
    }

    private void EndBlock()
    {
        var region = regions.Pop();
        CloseHandler(region);
        Mark(region.End);
    }

    private void CloseHandler(Region region)
    {
        if (region.HandlerStart.IsNil)
            throw new InvalidOperationException("An exception block ended without a handler.");

        if (region.Kind is ExceptionRegionKind.Finally or ExceptionRegionKind.Fault)
            EnsureTerminator(OpCodes.Endfinally);
        else
            LeaveIfNeeded(region.End);

        var end = MarkBoundary();
        var flow = il.ControlFlowBuilder!;
        switch (region.Kind)
        {
            case ExceptionRegionKind.Catch:
                flow.AddCatchRegion(region.TryStart, region.TryEnd, region.HandlerStart, end,
                    MetadataTokens.EntityHandle(builder.GetTokenFor(region.CatchType!)));
                break;
            case ExceptionRegionKind.Filter:
                flow.AddFilterRegion(region.TryStart, region.TryEnd, region.HandlerStart, end, region.FilterStart);
                break;
            case ExceptionRegionKind.Finally:
                flow.AddFinallyRegion(region.TryStart, region.TryEnd, region.HandlerStart, end);
                break;
            case ExceptionRegionKind.Fault:
                flow.AddFaultRegion(region.TryStart, region.TryEnd, region.HandlerStart, end);
                break;
        }
    }

    private void LeaveIfNeeded(Label target)
    {
        // Rule-generated try and catch bodies may fall through to the next block marker.
        if (instructions.Count == 0 || FallsThrough(instructions[^1].opcode))
            WriteInstruction(new CodeInstruction(OpCodes.Leave, target));
    }

    private void EnsureTerminator(OpCode opcode)
    {
        if (instructions.Count == 0 || instructions[^1].opcode != opcode)
            WriteInstruction(new CodeInstruction(opcode));
    }

    private static bool FallsThrough(OpCode opcode) =>
        opcode.FlowControl is not (FlowControl.Branch or FlowControl.Return or FlowControl.Throw) && opcode != OpCodes.Jmp;

    private void WriteInstruction(CodeInstruction instruction)
    {
        OpCode opcode = instruction.opcode;
        object? operand = instruction.operand;
        if (operand is SignatureHelper helper && opcode.OperandType == OperandType.InlineSig)
        {
            operand = InlineSignature.FromHelper(method, builder, helper);
            instruction = new CodeInstruction(opcode, operand);
        }

        instructions.Add(instruction);
        hasLocalloc |= opcode == OpCodes.Localloc;

        if (opcode.OperandType is OperandType.InlineBrTarget or OperandType.ShortInlineBrTarget)
        {
            // Rules can expand the distance to a branch target beyond a short branch's range.
            il.Branch((ILOpCode)OpCodeData.GetCanonicalOpcode(opcode), GetTarget((Label)operand!).Handle);
            return;
        }

        if (opcode.OperandType == OperandType.InlineSwitch)
        {
            var labels = (Label[])operand!;
            if (labels.Length == 0)
            {
                il.OpCode(ILOpCode.Switch);
                il.CodeBuilder.WriteInt32(0);
                return;
            }

            // The encoder requires all target handles to exist before opening a switch table.
            foreach (Label label in labels)
                GetTarget(label);
            var branches = il.Switch(labels.Length);
            foreach (Label label in labels)
                branches.Branch(targets[label].Handle);
            return;
        }

        il.OpCode((ILOpCode)(ushort)opcode.Value);
        if (opcode.OperandType == OperandType.InlineNone)
            return;

        if (operand is null)
            throw new InvalidOperationException($"Missing operand in {instruction}.");

        var bytes = il.CodeBuilder;
        switch (opcode.OperandType)
        {
            case OperandType.InlineI: bytes.WriteInt32(Convert.ToInt32(operand)); break;
            case OperandType.InlineI8: bytes.WriteInt64(Convert.ToInt64(operand)); break;
            case OperandType.ShortInlineR: bytes.WriteSingle(Convert.ToSingle(operand)); break;
            case OperandType.InlineR: bytes.WriteDouble(Convert.ToDouble(operand)); break;
            case OperandType.ShortInlineI when opcode == OpCodes.Ldc_I4_S:
                bytes.WriteSByte(Convert.ToSByte(operand));
                break;
            case OperandType.ShortInlineI: bytes.WriteByte(Convert.ToByte(operand)); break;
            case OperandType.ShortInlineVar:
                bytes.WriteByte(operand is LocalBuilder shortLocal ? checked((byte)shortLocal.LocalIndex) : Convert.ToByte(operand));
                break;
            case OperandType.InlineVar:
                bytes.WriteUInt16(operand is LocalBuilder local ? checked((ushort)local.LocalIndex) : Convert.ToUInt16(operand));
                break;
            case OperandType.InlineMethod or OperandType.InlineTok when operand is MethodBase target:
                il.Token(builder.GetTokenFor(target));
                break;
            case OperandType.InlineField or OperandType.InlineTok when operand is FieldInfo field:
                il.Token(builder.GetTokenFor(field));
                break;
            case OperandType.InlineType or OperandType.InlineTok when operand is Type type:
                il.Token(builder.GetTokenFor(type));
                break;
            case OperandType.InlineString when operand is string text:
                il.Token(builder.GetTokenFor(text));
                break;
            case OperandType.InlineSig when operand is InlineSignature signature:
                il.Token(signature.GetToken(builder));
                break;
            default: throw new NotSupportedException($"Unsupported operand in {instruction}.");
        }
    }

    private int GetMaxStack()
    {
        Type returnType = method is MethodInfo info ? info.ReturnType : typeof(void);
        int maxStack = 0;
        var entries = new int[instructions.Count];
        // -1 means no control flow path has reached this instruction yet. Each reachable
        // instruction is queued once because all incoming paths must agree on its stack depth.
        Array.Fill(entries, -1);
        var pending = new Queue<int>();
        Enqueue(0, 0);

        // The CLR enters handlers separately from normal control flow. Filters and catches
        // start with the exception object on the stack. Finally and fault handlers start empty.
        foreach (var (index, depth) in handlerEntries)
            Enqueue(index, depth);

        while (pending.TryDequeue(out int index))
        {
            int depth = entries[index];
            maxStack = Math.Max(maxStack, depth);
            var instruction = instructions[index];
            var opcode = instruction.opcode;
            if (opcode == OpCodes.Jmp)
                continue;

            int remaining = depth - instruction.PopCount(returnType);
            if (remaining < 0)
                throw new InvalidOperationException($"Stack underflow at {instruction}.");
            int nextDepth = opcode == OpCodes.Leave || opcode == OpCodes.Leave_S
                ? 0
                : remaining + instruction.PushCount();
            maxStack = Math.Max(maxStack, nextDepth);

            if (instruction.operand is Label label)
                Enqueue(GetTarget(label).Index, nextDepth);
            else if (instruction.operand is Label[] labels)
                foreach (Label target in labels)
                    Enqueue(GetTarget(target).Index, nextDepth);

            if (FallsThrough(opcode))
                Enqueue(index + 1, nextDepth);
        }

        return maxStack;

        void Enqueue(int index, int depth)
        {
            if ((uint)index >= (uint)entries.Length)
                throw new InvalidOperationException("Control flow reaches a position without an instruction.");
            if (entries[index] == depth)
                return;
            if (entries[index] >= 0)
                throw new InvalidOperationException($"Conflicting stack depths at {instructions[index]}.");
            entries[index] = depth;
            pending.Enqueue(index);
        }
    }
}
