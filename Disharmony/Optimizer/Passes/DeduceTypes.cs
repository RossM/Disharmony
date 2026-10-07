namespace Disharmony.Optimizer.Passes;

internal class DeduceTypes(Optimizer optimizer) : Pass(optimizer)
{
    protected internal override void RunInternal()
    {
        var visitor = new TypeVisitor(Optimizer);
        Optimizer.cfg = visitor.Visit(Optimizer.cfg);
    }
}

internal class TypeVisitor(Optimizer optimizer) : RewriteVisitor
{
    private bool dirty = true;

    private readonly Dictionary<int, StackSlot> stackSlots = [];

    protected override Op Visit(AssignmentOp op)
    {
        var input = Visit(op.Input);

        if (op.Output is StackSlot stackSlot)
        {
            StackSlot replacement = GetReplacement(stackSlot);
            var mergedType = TypeLattice.Merge(replacement.Type, input.Type);
            if (replacement.Type != mergedType)
            {
                dirty = true;
                stackSlots[stackSlot.Id] = stackSlot with { Type = mergedType };
            }
        }

        var output = (Variable)Visit(op.Output);

        if (input == op.Input && output == op.Output)
            return DefaultVisit(op);
        return DefaultVisit(new AssignmentOp(output, input));
    }

    protected override Op Visit(ILOp op)
    {
        var inputs = op.Inputs.Select(Visit).ToList();

        var data = OpCodeData.Get(op.IL.OpCode);

        IEnumerable<Type> inputTypes = inputs.Select(input => input.Type);

        Type[] types;
        if ((data.flags & OpCodeFlags.Argument) != 0)
            types = [optimizer.cfg.GetArgument(op.IL).Type, .. inputTypes];
        else if ((data.flags & OpCodeFlags.Local) != 0)
            types = [optimizer.cfg.GetLocal(op.IL).Type, .. inputTypes];
        else
            types = [.. inputTypes];

        var resultType = OpcodeUtilities.GetOutputType(op.IL, types);

        if (inputs.SequenceEqual(op.Inputs) && op.Type == resultType)
            return DefaultVisit(op);
        return DefaultVisit(new ILOp(op.IL, inputs, resultType));
    }

    protected override Op Visit(StackSlot op) => GetReplacement(op);

    private StackSlot GetReplacement(StackSlot op) => stackSlots.TryGetValue(op.Id, out StackSlot? replacement) ? replacement : op;

    public override ControlFlowGraph Visit(ControlFlowGraph cfg)
    {
        while (true)
        {
            dirty = false;

            var blocks = cfg.BasicBlocks.Select(Visit).ToList();
            var edges = cfg.Edges.Select(Visit).ToList();

            if (!dirty)
                return new ControlFlowGraph(cfg.RootRegion, blocks, edges, cfg.Arguments, cfg.Locals);
        }
    }
}
