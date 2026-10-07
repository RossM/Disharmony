namespace Disharmony.Optimizer.Passes;

internal class CreateControlFlowGraph : Pass
{
    public MethodBase Method => Optimizer.method;
    public IReadOnlyList<CodeInstruction> CodeInstructions => Optimizer.Instructions;

    public CreateControlFlowGraph(Optimizer optimizer) : base(optimizer)
    {
        if (Method.HasThis)
            ParameterTypes = [Method.DeclaringType!.CallableType, .. Method.GetParameters().Types()];
        else
            ParameterTypes = [.. Method.GetParameters().Types()];

        MethodBody = GetMethodBodyOrNull(Method);
        ReturnType = Method is MethodInfo methodInfo ? methodInfo.ReturnType : typeof(void);
    }

    public RootRegion RootRegion { get; } = new(new(Id: 0));
    public Dictionary<Label, BlockLabel> BlockLabels { get; } = [];
    public Dictionary<BlockLabel, (List<StackSlot> IncomingStack, List<StackSlot> OutgoingStack)> BlockStacks { get; } = [];
    public List<Local> Locals { get; } = [];
    public MethodBody? MethodBody { get; }
    public List<Argument> Arguments { get; } = [];
    public List<Type> ParameterTypes { get; }
    public List<BasicBlock> BasicBlocks { get; } = [];
    public List<Edge> Edges { get; } = [];
    public Dictionary<ProtectedRegion, ExceptionGroup> ExceptionGroups { get; } = [];

    public Type ReturnType { get; }

    private int NextStackSlotId { get; set; }
    private int NextBlockLabelId { get; set; } = 1;

    private static MethodBody? GetMethodBodyOrNull(MethodBase method)
    {
        try
        {
            return method.GetMethodBody();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
    }

    private StackSlot CreateStackSlot(int depth, Type type) => new(depth, type, NextStackSlotId++);
    private StackSlot CreateStackSlot(int depth) => CreateStackSlot(depth, TypeLattice.Unknown);

    protected internal override void RunInternal()
    {
        CreateArguments();

        CreateLocals();

        List<(BlockLabel Label, List<CodeInstruction> Instructions)> instructionBlocks = FindBasicBlocks();

        CreateBasicBlocks(instructionBlocks);

        CreateEdges();

        // The initial CFG has invalid exception information because we haven't yet rewritten the protected regions,
        // so disable validation.
        Optimizer.cfg = new ControlFlowGraph(RootRegion, BasicBlocks, Edges, Arguments, Locals, validate: false);

        ProtectedRegionRewriteVisitor visitor = new(ExceptionGroups);
        Optimizer.cfg = visitor.Visit(Optimizer.cfg);
    }

    private void CreateArguments()
    {
        // Get arguments from MethodInfo
        for (int index = 0; index < ParameterTypes.Count; index++)
            Arguments.Add(new Argument(index, ParameterTypes[index]));
    }

    private void CreateLocals()
    {
        Dictionary<int, Local> locals = [];

        // Get locals from MethodInfo
        if (MethodBody != null)
            foreach (var local in MethodBody.LocalVariables)
                locals.Add(local.LocalIndex, new Local(local));

        // Get locals from LocalBuilders
        foreach (var instruction in CodeInstructions)
        {
            if (instruction.operand is not LocalBuilder localBuilder)
                continue;

            if (locals.TryGetValue(localBuilder.LocalIndex, out var local) && local.Type != localBuilder.LocalType)
                throw new InvalidOperationException($"Conflicting types for local #{localBuilder.LocalIndex}");

            locals[localBuilder.LocalIndex] = new Local(localBuilder);
        }

        if (locals.Count > 0)
            for (int i = 0; i <= locals.Keys.Max(); i++)
                Locals.Add(locals.TryGetValue(i, out Local? local) ? local : new Local(TypeLattice.Any, i));
    }

    private BlockLabel CreateBlockLabel() => new(Id: NextBlockLabelId++);

    private BlockLabel CreateBlockLabel(CodeInstruction instruction)
    {
        if (instruction.labels.Count <= 0)
            return CreateBlockLabel();

        var blockLabel = new BlockLabel(NextBlockLabelId++, instruction.labels[0]);
        foreach (var label in instruction.labels)
            BlockLabels.Add(label, blockLabel);
        return blockLabel;
    }

    private List<(BlockLabel Label, List<CodeInstruction> Instructions)> FindBasicBlocks()
    {
        // Find basic blocks
        List<(BlockLabel Label, List<CodeInstruction> Instructions)> instructionBlocks = [];
        bool newBlock = true;
        foreach (CodeInstruction instruction in CodeInstructions)
        {
            // Drop nops early so that annotations don't create spurious basic blocks
            if (instruction.opcode == OpCodes.Nop && instruction.labels.Count == 0 && instruction.blocks.Count == 0)
                continue;

            if (instruction.labels.Count > 0)
                newBlock = true;
            if (instruction.blocks.Any(b => b.blockType != ExceptionBlockType.EndExceptionBlock))
                newBlock = true;

            if (newBlock)
            {
                var label = CreateBlockLabel(instruction);
                instructionBlocks.Add(new(label, []));
                newBlock = false;
            }

            instructionBlocks[^1].Instructions.Add(instruction);

            if (EndsBasicBlock(instruction))
                newBlock = true;
            if (instruction.blocks.Any(b => b.blockType == ExceptionBlockType.EndExceptionBlock))
                newBlock = true;
        }

        return instructionBlocks;
    }

    private void CreateBasicBlocks(List<(BlockLabel Label, List<CodeInstruction> Instructions)> instructionBlocks)
    {
        // Exception region data
        Stack<Region> regionStack = [];
        Stack<(ProtectedRegion ProtectedRegion, List<HandlerRegion> HandlerRegions)> exceptionGroupStack = [];
        regionStack.Push(RootRegion);

        AddSyntheticEntryBlock(RootRegion, instructionBlocks[0].Label);

        // Translate basic blocks
        Dictionary<BlockLabel, int> incomingStackSize = [];
        for (var index = 0; index < instructionBlocks.Count; index++)
        {
            var (label, instructions) = instructionBlocks[index];
            foreach (var exceptionBlock in instructions[0].blocks)
            {
                switch (exceptionBlock.blockType)
                {
                    case ExceptionBlockType.BeginExceptionBlock:
                    {
                        // We don't have all the handler regions yet, so we create the ProtectedRegion with an empty ExceptionGroup,
                        // then rewrite it later to have the correct handler regions.
                        var protectedRegion = new ProtectedRegion(label, regionStack.Peek(), new ExceptionGroup([]));

                        regionStack.Push(protectedRegion);
                        exceptionGroupStack.Push((protectedRegion, []));

                        // A protected region can have real incoming edges and doesn't need a synthetic entry block

                        break;
                    }
                    case ExceptionBlockType.BeginCatchBlock:
                    {
                        regionStack.Pop();
                        var region = new CatchRegion(CreateBlockLabel(), regionStack.Peek(), CreateStackSlot(0, exceptionBlock.catchType));
                        regionStack.Push(region);
                        exceptionGroupStack.Peek().HandlerRegions.Add(region);
                        incomingStackSize[label] = 1;
                        AddSyntheticEntryBlock(region, label, region.IncomingException);
                        break;
                    }
                    case ExceptionBlockType.BeginExceptFilterBlock: throw new NotSupportedException();
                    case ExceptionBlockType.BeginFaultBlock:
                    {
                        regionStack.Pop();
                        var region = new FaultRegion(CreateBlockLabel(), regionStack.Peek());
                        regionStack.Push(region);
                        exceptionGroupStack.Peek().HandlerRegions.Add(region);
                        AddSyntheticEntryBlock(region, label);
                        break;
                    }
                    case ExceptionBlockType.BeginFinallyBlock:
                    {
                        regionStack.Pop();
                        var region = new FinallyRegion(CreateBlockLabel(), regionStack.Peek());
                        regionStack.Push(region);
                        exceptionGroupStack.Peek().HandlerRegions.Add(region);
                        AddSyntheticEntryBlock(region, label);
                        break;
                    }
                    case ExceptionBlockType.EndExceptionBlock:
                        // Handled later
                        break;
                    default: throw new InvalidOperationException();
                }
            }

            var fallthroughLabel = index + 1 < instructionBlocks.Count ? instructionBlocks[index + 1].Label : null;
            incomingStackSize.TryGetValue(label, out int blockStartStackSize);
            BasicBlock block = ConvertBasicBlock(label, instructions, fallthroughLabel, regionStack.Peek(), blockStartStackSize,
                out var stacks);
            BasicBlocks.Add(block);
            BlockStacks[label] = stacks;
            foreach (var successor in block.Branch.Labels)
            {
                if (incomingStackSize.TryGetValue(successor, out int curValue))
                {
                    if (stacks.OutgoingStack.Count != curValue)
                        throw new InvalidOperationException("Stack size mismatch");
                }
                else
                {
                    incomingStackSize[successor] = stacks.OutgoingStack.Count;
                }
            }

            foreach (var exceptionBlock in instructions[^1].blocks)
            {
                if (exceptionBlock.blockType == ExceptionBlockType.EndExceptionBlock)
                {
                    regionStack.Pop();
                    var (protectedRegion, handlerRegions) = exceptionGroupStack.Pop();
                    ExceptionGroups[protectedRegion] = new ExceptionGroup(handlerRegions);
                }
            }
        }
    }

    private void AddSyntheticEntryBlock(Region region, BlockLabel blockLabel, params StackSlot[] stackSlots)
    {
        BasicBlocks.Add(new(region.EntryLabel, [], region, new UnconditionalBranch(blockLabel)));
        BlockStacks[region.EntryLabel] = ([.. stackSlots], [.. stackSlots]);
    }

    private void CreateEdges()
    {
        foreach (var block in BasicBlocks)
        {
            var label = block.Label;
            foreach (var successor in block.Branch.Labels.Distinct())
            {
                var edgeAssignments = BlockStacks[successor].IncomingStack.Zip(BlockStacks[label].OutgoingStack,
                    (incoming, outgoing) => new AssignmentOp(incoming, outgoing)).Where(op => op.Input != op.Output).ToList();
                var edge = new Edge(label, successor, edgeAssignments);
                Edges.Add(edge);
            }
        }
    }

    private static bool EndsBasicBlock(CodeInstruction instruction) =>
        instruction.opcode.FlowControl is FlowControl.Branch or FlowControl.Cond_Branch or FlowControl.Return or FlowControl.Throw ||
        instruction.opcode == OpCodes.Jmp;

    private BasicBlock ConvertBasicBlock(
        BlockLabel blockLabel,
        IReadOnlyList<CodeInstruction> instructions,
        BlockLabel? fallthroughLabel,
        Region region,
        int incomingStackSize,
        out (List<StackSlot> IncomingStack, List<StackSlot> OutgoingStack) stacks)
    {
        List<StackSlot> incomingStack = [];
        for (int i = 0; i < incomingStackSize; i++)
            incomingStack.Add(CreateStackSlot(i));
        List<StackSlot> curStack = [.. incomingStack];

        List<Op> ops = [];
        List<Prefix> prefixes = [];
        Branch? branch = null;
        foreach (var instruction in instructions)
        {
            if (instruction.opcode.OpCodeType == OpCodeType.Prefix)
            {
                prefixes.Add(new(instruction.opcode, instruction.operand));
                continue;
            }

            int popCount = instruction.PopCount(ReturnType);
            
            // Avoid stack underflow. This is invalid but can happen in unreachable basic blocks if something before
            // us messes up.
            while (curStack.Count < popCount)
                curStack.Insert(0, CreateStackSlot(-1));

            List<StackSlot> popped = curStack.GetRange(curStack.Count - popCount, popCount);
            curStack.RemoveRange(curStack.Count - popCount, popCount);

            // Dup is the only instruction that pushes multiple values, handle it specially
            if (instruction.opcode == OpCodes.Dup)
            {
                curStack.Add(popped[0]);
                var result = CreateStackSlot(curStack.Count);
                ops.Add(new AssignmentOp(result, popped[0]));
                curStack.Add(result);
                continue;
            }

            ILInstruction il = new(instruction.opcode, instruction.operand, prefixes);
            prefixes = [];

            if (EndsBasicBlock(instruction))
            {
                branch = ConvertBranch(instruction, il, popped, fallthroughLabel);
                break;
            }

            switch (instruction.PushCount())
            {
                case 0:
                {
                    ops.Add(new ILOp(il, popped, typeof(void)));
                    break;
                }
                case 1:
                {
                    StackSlot result = CreateStackSlot(curStack.Count);
                    ops.Add(new AssignmentOp(result, new ILOp(il, popped, TypeLattice.Unknown)));
                    curStack.Add(result);
                    break;
                }
                default: throw new InvalidOperationException();
            }
        }

        branch ??= new UnconditionalBranch(fallthroughLabel ?? throw new InvalidOperationException());

        stacks = (incomingStack, curStack);
        return new BasicBlock(blockLabel, ops, region, branch);
    }

    private Branch ConvertBranch(CodeInstruction instruction, ILInstruction il, List<StackSlot> popped, BlockLabel? fallthroughLabel)
    {
        Branch branch = OpCodeData.GetCanonicalOpcode(instruction) switch
        {
            OpCodeValues.Br when instruction.operand is Label label => new UnconditionalBranch(BlockLabels[label]),
            OpCodeValues.Leave when instruction.operand is Label label => new Leave(BlockLabels[label]),
            OpCodeValues.Throw => new Throw(popped[0]),
            OpCodeValues.Rethrow => new Rethrow(),
            OpCodeValues.Jmp => new Jump(new ILOp(il, popped, typeof(void))),
            _ => instruction.opcode.FlowControl switch
            {
                FlowControl.Cond_Branch when instruction.operand is Label label =>
                    new ConditionalBranch(instruction.opcode, popped,
                        [fallthroughLabel ?? throw new InvalidOperationException(), BlockLabels[label]]),
                FlowControl.Cond_Branch when instruction.operand is Label[] labels =>
                    new ConditionalBranch(instruction.opcode, popped,
                        [fallthroughLabel ?? throw new InvalidOperationException(), .. labels.Select(label => BlockLabels[label])]),
                FlowControl.Return when popped.Count == 0 => new Return(il, new VoidOp()),
                FlowControl.Return => new Return(il, popped[0]),
                _ => throw new ArgumentOutOfRangeException(nameof(instruction), instruction, null),
            },
        };
        return branch;
    }
}

internal class ProtectedRegionRewriteVisitor(Dictionary<ProtectedRegion, ExceptionGroup> exceptionGroups) : RewriteVisitor
{
    private readonly Dictionary<ProtectedRegion, ProtectedRegion> replacements = [];

    protected override Region Visit(ProtectedRegion region)
    {
        if (replacements.TryGetValue(region, out var value))
            return value;

        var parent = Visit(region.Parent);
        var exceptionGroup = Visit(exceptionGroups[region]);

        return replacements[region] = new ProtectedRegion(region.EntryLabel, parent, exceptionGroup);
    }
}
