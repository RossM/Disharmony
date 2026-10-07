namespace Disharmony.Optimizer;

internal class ReplaceVisitor : RewriteVisitor
{
    public Mapping<Op> Replacements { get; init; } = [];

    protected override Op DefaultVisit(Op op) => Replacements[op];
}

internal class RewriteVisitor
{
    public Op Visit(Op op) => op switch
    {
        AssignmentOp assignmentOp => Visit(assignmentOp),
        Argument argument => Visit(argument),
        ConversionOp conversionOp => Visit(conversionOp),
        ILOp ilOp => Visit(ilOp),
        Local local => Visit(local),
        StackSlot stackSlot => Visit(stackSlot),
        Temporary temporary => Visit(temporary),
        VoidOp voidOp => Visit(voidOp),
        _ => throw new ArgumentOutOfRangeException(nameof(op), op, null),
    };

    public Branch Visit(Branch branch) => branch switch
    {
        ConditionalBranch conditionalBranch => Visit(conditionalBranch),
        Jump jump => Visit(jump),
        Leave leave => Visit(leave),
        Rethrow rethrow => Visit(rethrow),
        Return @return => Visit(@return),
        Throw @throw => Visit(@throw),
        UnconditionalBranch unconditionalBranch => Visit(unconditionalBranch),
        _ => throw new ArgumentOutOfRangeException(nameof(branch), branch, null),
    };

    public Region Visit(Region region) => region switch
    {
        CatchRegion catchRegion => Visit(catchRegion),
        FaultRegion faultRegion => Visit(faultRegion),
        FinallyRegion finallyRegion => Visit(finallyRegion),
        HandlerRegion handlerRegion => Visit(handlerRegion),
        ProtectedRegion protectedRegion => Visit(protectedRegion),
        RootRegion rootRegion => Visit(rootRegion),
        _ => throw new ArgumentOutOfRangeException(nameof(region), region, null),
    };

    protected virtual Op DefaultVisit(Op op) => op;

    protected virtual Op Visit(AssignmentOp op)
    {
        var input = Visit(op.Input);
        var output = (Variable)Visit(op.Output);
        if (input == op.Input && output == op.Output)
            return DefaultVisit(op);
        return DefaultVisit(new AssignmentOp(output, input));
    }

    protected virtual Op Visit(ConversionOp op)
    {
        var input = Visit(op.Input);
        if (input == op.Input)
            return DefaultVisit(op);
        return DefaultVisit(new ConversionOp(input, op.Type));
    }

    protected virtual Op Visit(ILOp op)
    {
        var inputs = op.Inputs.Select(Visit).ToList();
        if (inputs.SequenceEqual(op.Inputs))
            return DefaultVisit(op);
        return DefaultVisit(new ILOp(op.IL, inputs, op.Type));
    }

    protected virtual Op Visit(StackSlot op) => DefaultVisit(op);
    protected virtual Op Visit(Argument op) => DefaultVisit(op);
    protected virtual Op Visit(Local op) => DefaultVisit(op);
    protected virtual Op Visit(Temporary op) => DefaultVisit(op);
    protected virtual Op Visit(VoidOp op) => DefaultVisit(op);
    protected virtual Region Visit(RootRegion region) => region;

    protected virtual Region Visit(ProtectedRegion region)
    {
        var parent = Visit(region.Parent);
        var group = Visit(region.Group);
        if (parent == region.Parent && group == region.Group)
            return region;
        return new ProtectedRegion(region.EntryLabel, parent, group);
    }

    protected virtual Region Visit(CatchRegion region)
    {
        var parent = Visit(region.Parent);
        var incomingException = (StackSlot)Visit(region.IncomingException);
        if (parent == region.Parent && incomingException == region.IncomingException)
            return region;
        return new CatchRegion(region.EntryLabel, parent, incomingException);
    }

    protected virtual Region Visit(FinallyRegion region)
    {
        var parent = Visit(region.Parent);
        if (parent == region.Parent)
            return region;
        return new FinallyRegion(region.EntryLabel, parent);
    }

    protected virtual Region Visit(FaultRegion region)
    {
        var parent = Visit(region.Parent);
        if (parent == region.Parent)
            return region;
        return new FaultRegion(region.EntryLabel, parent);
    }

    public virtual ExceptionGroup Visit(ExceptionGroup group)
    {
        var handlerRegions = group.HandlerRegions.Select(region => (HandlerRegion)Visit(region)).ToList();
        if (handlerRegions.SequenceEqual(group.HandlerRegions))
            return group;
        return new ExceptionGroup(handlerRegions);
    }

    protected virtual Branch Visit(UnconditionalBranch branch) => branch;
    protected virtual Branch Visit(Leave branch) => branch;

    protected virtual Branch Visit(ConditionalBranch branch)
    {
        var inputs = branch.Inputs.Select(Visit).ToList();
        if (inputs.SequenceEqual(branch.Inputs))
            return branch;
        return new ConditionalBranch(branch.OpCode, inputs, branch.Labels);
    }

    protected virtual Branch Visit(Throw branch)
    {
        var exception = Visit(branch.Exception);
        if (exception == branch.Exception)
            return branch;
        return new Throw(exception);
    }

    protected virtual Branch Visit(Rethrow branch) => branch;

    protected virtual Branch Visit(Return branch)
    {
        var value = Visit(branch.Value);
        if (value == branch.Value)
            return branch;
        return new Return(branch.IL, value);
    }

    protected virtual Branch Visit(Jump branch)
    {
        var value = Visit(branch.Value);
        if (value == branch.Value)
            return branch;
        return new Jump(value);
    }

    public virtual BasicBlock Visit(BasicBlock block)
    {
        var ops = block.Ops.Select(Visit).ToList();
        var branch = Visit(block.Branch);
        var region = Visit(block.Region);
        if (ops.SequenceEqual(block.Ops) && branch == block.Branch && region == block.Region)
            return block;
        return new BasicBlock(block.Label, ops, region, branch);
    }

    public virtual Edge Visit(Edge edge)
    {
        var edgeAssignments = edge.EdgeAssignments.Select(op => (AssignmentOp)Visit(op)).Where(op => op.Input != op.Output)
            .ToList();
        if (edgeAssignments.SequenceEqual(edge.EdgeAssignments))
            return edge;
        return new Edge(edge.Source, edge.Destination, edgeAssignments);
    }

    public virtual ControlFlowGraph Visit(ControlFlowGraph cfg)
    {
        var rootRegion = (RootRegion)Visit(cfg.RootRegion);
        var blocks = cfg.BasicBlocks.Select(Visit).ToList();
        var edges = cfg.Edges.Select(Visit).ToList();
        var arguments = cfg.Arguments.Select(argument => (Argument)Visit(argument)).ToList();
        var locals = cfg.Locals.Select(local => (Local)Visit(local)).ToList();

        if (rootRegion == cfg.RootRegion && blocks.SequenceEqual(cfg.BasicBlocks) && edges.SequenceEqual(cfg.Edges) &&
            arguments.SequenceEqual(cfg.Arguments) && locals.SequenceEqual(cfg.Locals))
            return cfg;

        return new ControlFlowGraph(rootRegion, blocks, edges, arguments, locals);
    }
}
