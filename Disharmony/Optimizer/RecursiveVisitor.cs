namespace Disharmony.Optimizer;

internal abstract class RecursiveVisitor
{
    public void Visit(Op op)
    {
        switch (op)
        {
            case AssignmentOp assignmentOp: Visit(assignmentOp); break;
            case Argument argument: Visit(argument); break;
            case ConversionOp conversionOp: Visit(conversionOp); break;
            case ILOp ilOp: Visit(ilOp); break;
            case Local local: Visit(local); break;
            case StackSlot stackSlot: Visit(stackSlot); break;
            case Temporary temporary: Visit(temporary); break;
            case VoidOp voidOp: Visit(voidOp); break;
            default: throw new ArgumentOutOfRangeException(nameof(op), op, null);
        }
    }

    public void Visit(Branch branch)
    {
        switch (branch)
        {
            case ConditionalBranch conditionalBranch: Visit(conditionalBranch); break;
            case Jump jump: Visit(jump); break;
            case Leave leave: Visit(leave); break;
            case Rethrow rethrow: Visit(rethrow); break;
            case Return @return: Visit(@return); break;
            case Throw @throw: Visit(@throw); break;
            case UnconditionalBranch unconditionalBranch: Visit(unconditionalBranch); break;
            default: throw new ArgumentOutOfRangeException(nameof(branch), branch, null);
        }
    }

    public void Visit(Region region)
    {
        switch (region)
        {
            case CatchRegion catchRegion: Visit(catchRegion); break;
            case FaultRegion faultRegion: Visit(faultRegion); break;
            case FinallyRegion finallyRegion: Visit(finallyRegion); break;
            case HandlerRegion handlerRegion: Visit(handlerRegion); break;
            case ProtectedRegion protectedRegion: Visit(protectedRegion); break;
            case RootRegion rootRegion: Visit(rootRegion); break;
            default: throw new ArgumentOutOfRangeException(nameof(region), region, null);
        }
    }

    protected virtual void DefaultVisit(Op op) { }

    protected virtual void Visit(AssignmentOp op)
    {
        Visit(op.Input);
        Visit(op.Output);
    }

    protected virtual void Visit(ConversionOp op)
    {
        Visit(op.Input);
    }

    protected virtual void Visit(ILOp op)
    {
        foreach (var input in op.Inputs)
            Visit(input);
    }

    protected virtual void Visit(StackSlot op) => DefaultVisit(op);
    protected virtual void Visit(Argument op) => DefaultVisit(op);
    protected virtual void Visit(Local op) => DefaultVisit(op);
    protected virtual void Visit(Temporary op) => DefaultVisit(op);
    protected virtual void Visit(VoidOp op) => DefaultVisit(op);
    protected virtual void Visit(RootRegion region) { }

    protected virtual void Visit(ProtectedRegion region)
    {
        Visit(region.Parent);
        Visit(region.Group);
    }

    protected virtual void Visit(CatchRegion region)
    {
        Visit(region.Parent);
        Visit(region.IncomingException);
    }

    protected virtual void Visit(FinallyRegion region)
    {
        Visit(region.Parent);
    }

    protected virtual void Visit(FaultRegion region)
    {
        Visit(region.Parent);
    }

    protected virtual void Visit(ExceptionGroup group)
    {
        foreach (var region in group.HandlerRegions)
            Visit(region);
    }

    protected virtual void Visit(UnconditionalBranch branch) { }
    protected virtual void Visit(Leave branch) { }

    protected virtual void Visit(ConditionalBranch branch)
    {
        foreach (var input in branch.Inputs)
            Visit(input);
    }

    protected virtual void Visit(Throw branch)
    {
        Visit(branch.Exception);
    }

    protected virtual void Visit(Rethrow branch) { }

    protected virtual void Visit(Return branch)
    {
        Visit(branch.Value);
    }

    protected virtual void Visit(Jump branch)
    {
        Visit(branch.Value);
    }

    public virtual void Visit(BasicBlock block)
    {
        foreach (var op in block.Ops)
            Visit(op);
        Visit(block.Branch);
        Visit(block.Region);
    }

    public virtual void Visit(Edge edge)
    {
        foreach (var edgeAssignment in edge.EdgeAssignments)
            Visit(edgeAssignment);
    }

    public virtual void Visit(ControlFlowGraph cfg)
    {
        Visit(cfg.RootRegion);
        foreach (var block in cfg.BasicBlocks)
            Visit(block);
        foreach (var edge in cfg.Edges)
            Visit(edge);
        foreach (var argument in cfg.Arguments)
            Visit(argument);
        foreach (var local in cfg.Locals)
            Visit(local);
    }
}
