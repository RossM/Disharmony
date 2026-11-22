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
    // The encoder uses the handle. Stack analysis and exception boundaries use the
    // instruction index, which is -1 until the label is marked.
    private record Target(LabelHandle Handle, int InstructionIndex);

    // One entry per open try block. Each handler becomes a separate metadata clause
    // with the same try range. Nested try blocks have their own entries on the stack.
    private sealed class Region(Target tryStart, Label leaveTarget)
    {
        public readonly Target tryStart = tryStart;
        // Normal exits from the try or a catch jump past the entire handler sequence.
        public readonly Label leaveTarget = leaveTarget;
        public Target? tryEnd;
        public Target? pendingFilter;
        public Handler? activeHandler;
    }

    private readonly record struct Handler(ExceptionRegionKind Kind, Target Start, Type? CatchType = null,
        Target? FilterStart = null);

    private readonly InstructionEncoder il = new(new BlobBuilder(), new ControlFlowBuilder());
    private readonly Dictionary<Label, Target> targets = [];
    // Stack analysis follows the emitted instructions, including any added at exception boundaries.
    private readonly List<CodeInstruction> instructions = [];
    private readonly List<(int index, int depth)> handlerRegionEntries = [];
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
            targets.Add(label, target = new Target(il.DefineLabel(), -1));
        return target;
    }

    private void Mark(Label label)
    {
        var target = GetTarget(label);
        if (target.InstructionIndex >= 0)
            throw new InvalidOperationException($"Label {label.GetHashCode()} was marked twice.");
        targets[label] = MarkTarget(target.Handle);
    }

    private Target MarkBoundary() => MarkTarget(il.DefineLabel());

    private Target MarkTarget(LabelHandle handle)
    {
        il.MarkLabel(handle);
        return new Target(handle, instructions.Count);
    }

    private void BeginBlock(ExceptionBlock block)
    {
        if (block.blockType == ExceptionBlockType.BeginExceptionBlock)
        {
            regions.Push(new Region(MarkBoundary(), allocator.DefineLabel()));
            return;
        }

        var region = regions.Peek();
        if (region.pendingFilter is { } filter)
        {
            // Harmony uses an untyped catch marker for the body selected by a filter.
            // The filter and that body belong to a single clause.
            if (block.blockType != ExceptionBlockType.BeginCatchBlock || block.catchType is not null)
                throw new InvalidOperationException("An exception filter must be followed by its handler.");
            EnsureTerminator(filter, OpCodes.Endfilter);
            BeginHandler(region, ExceptionRegionKind.Filter, filterStart: filter);
            region.pendingFilter = null;
            return;
        }

        // A new filter or handler ends the try on the first transition and the preceding
        // handler on subsequent transitions. Emit any needed exit before marking the boundary.
        if (region.tryEnd is null)
        {
            LeaveIfNeeded(region.tryStart, region.leaveTarget);
            region.tryEnd = MarkBoundary();
        }
        else
        {
            CloseHandler(region);
        }

        switch (block.blockType)
        {
            case ExceptionBlockType.BeginExceptFilterBlock:
                var start = MarkBoundary();
                region.pendingFilter = start;
                handlerRegionEntries.Add((start.InstructionIndex, 1));
                break;
            case ExceptionBlockType.BeginCatchBlock:
                BeginHandler(region, ExceptionRegionKind.Catch, catchType: block.catchType);
                break;
            case ExceptionBlockType.BeginFinallyBlock:
                BeginHandler(region, ExceptionRegionKind.Finally, depth: 1);
                break;
            case ExceptionBlockType.BeginFaultBlock:
                BeginHandler(region, ExceptionRegionKind.Fault, depth: 1);
                break;
            default:
                throw new NotSupportedException($"Unsupported exception block {block.blockType}.");
        }
    }

    private void BeginHandler(Region region, ExceptionRegionKind kind, Type? catchType = null, Target? filterStart = null, int depth = 0)
    {
        var start = MarkBoundary();
        region.activeHandler = new Handler(kind, start, catchType, filterStart);
        handlerRegionEntries.Add((start.InstructionIndex, depth));
    }

    private void EndBlock()
    {
        var region = regions.Pop();
        CloseHandler(region);
        Mark(region.leaveTarget);
    }

    private void CloseHandler(Region region)
    {
        if (region.activeHandler is not { } handler)
            throw new InvalidOperationException("An exception block ended without a handler.");
        if (region.tryEnd is not { } tryEnd)
            throw new InvalidOperationException("An exception region was not marked.");

        if (handler.Kind is ExceptionRegionKind.Finally or ExceptionRegionKind.Fault)
            EnsureTerminator(handler.Start, OpCodes.Endfinally);
        else
            LeaveIfNeeded(handler.Start, region.leaveTarget);

        // The range includes any terminator emitted above. Its end is the first byte
        // outside the handler, which can also be the start of the next filter or handler.
        var handlerEnd = MarkBoundary();
        var flow = il.ControlFlowBuilder!;
        switch (handler.Kind)
        {
            case ExceptionRegionKind.Catch:
                flow.AddCatchRegion(region.tryStart.Handle, tryEnd.Handle, handler.Start.Handle, handlerEnd.Handle,
                    MetadataTokens.EntityHandle(builder.GetTokenFor(handler.CatchType!)));
                break;
            case ExceptionRegionKind.Filter when handler.FilterStart is { } filterStart:
                flow.AddFilterRegion(region.tryStart.Handle, tryEnd.Handle, handler.Start.Handle, handlerEnd.Handle, filterStart.Handle);
                break;
            case ExceptionRegionKind.Finally:
                flow.AddFinallyRegion(region.tryStart.Handle, tryEnd.Handle, handler.Start.Handle, handlerEnd.Handle);
                break;
            case ExceptionRegionKind.Fault:
                flow.AddFaultRegion(region.tryStart.Handle, tryEnd.Handle, handler.Start.Handle, handlerEnd.Handle);
                break;
            default: throw new InvalidOperationException("Unknown handler kind");
        }

        region.activeHandler = null;
    }

    private void LeaveIfNeeded(Target start, Label target)
    {
        // A rule can supply an empty body or one that falls through. Both need a leave
        // to skip the remaining handlers.
        if (LastOpcodeIn(start) is not { } last || FallsThrough(last))
            WriteInstruction(new CodeInstruction(OpCodes.Leave, target));
    }

    private void EnsureTerminator(Target start, OpCode opcode)
    {
        // The incoming IL may already have the terminator. Only fill in a missing one
        // at the boundary, preserving explicit exits earlier in the block.
        if (LastOpcodeIn(start) != opcode)
            WriteInstruction(new CodeInstruction(opcode));
    }

    private OpCode? LastOpcodeIn(Target start) =>
        instructions.Count > start.InstructionIndex ? instructions[^1].opcode : null;

    private static bool FallsThrough(OpCode opcode) =>
        opcode.FlowControl is not (FlowControl.Branch or FlowControl.Return or FlowControl.Throw) && opcode != OpCodes.Jmp;

    private void WriteInstruction(CodeInstruction instruction)
    {
        OpCode opcode = instruction.opcode;
        object? operand = instruction.operand;

        instructions.Add(instruction);
        hasLocalloc |= opcode == OpCodes.Localloc;

        if (opcode.OperandType is OperandType.InlineBrTarget or OperandType.ShortInlineBrTarget)
        {
            // Rules can expand the distance to a branch target beyond a short branch's range.
            il.Branch(OpCodeData.GetCanonicalOpcode(opcode), GetTarget((Label)operand!).Handle);
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

        il.OpCode(unchecked((ILOpCode)opcode.Value));
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
        var stackDepth = new int[instructions.Count];
        // -1 means no control flow path has reached this instruction yet. Each reachable
        // instruction is queued once because all incoming paths must agree on its stack depth.
        Array.Fill(stackDepth, -1);
        var pending = new Queue<int>();
        Enqueue(0, 0);

        // The CLR enters handlers separately from normal control flow. Filters and catches
        // start with the exception object on the stack. Finally and fault handlers start empty.
        foreach ((int index, int stackDepthOnEntry) in handlerRegionEntries)
            Enqueue(index, stackDepthOnEntry);

        while (pending.TryDequeue(out int index))
        {
            int depth = stackDepth[index];
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
                Enqueue(GetTarget(label).InstructionIndex, nextDepth);
            else if (instruction.operand is Label[] labels)
                foreach (Label target in labels)
                    Enqueue(GetTarget(target).InstructionIndex, nextDepth);

            if (FallsThrough(opcode))
                Enqueue(index + 1, nextDepth);
        }

        return maxStack;

        void Enqueue(int index, int depth)
        {
            if ((uint)index >= (uint)stackDepth.Length)
                throw new InvalidOperationException("Control flow reaches a position without an instruction.");
            if (stackDepth[index] == depth)
                return;
            if (stackDepth[index] >= 0)
                throw new InvalidOperationException($"Conflicting stack depths at {instructions[index]}.");
            stackDepth[index] = depth;
            pending.Enqueue(index);
        }
    }
}
