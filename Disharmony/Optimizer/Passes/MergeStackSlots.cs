namespace Disharmony.Optimizer.Passes;

internal class MergeStackSlots(Optimizer optimizer) : Pass(optimizer)
{
    protected internal override void RunInternal()
    {
        // Precondition: All edge assignments are between stack slots; no stack slot is live in multiple basic blocks
        // Postcondition: There are no edge assignments

        DisjointSetUnion<Op> tree = new();

        foreach (var edge in Optimizer.cfg.Edges)
        foreach (var assignment in edge.EdgeAssignments)
        {
            tree.Add(assignment.Input);
            tree.Add(assignment.Output);
        }

        foreach (var edge in Optimizer.cfg.Edges)
        foreach (var assignment in edge.EdgeAssignments)
            tree.Merge(assignment.Output, assignment.Input);

        ReplaceVisitor visitor = new();
        foreach (var group in tree)
        foreach (var op in group)
        {
            if (op != group.Key)
                visitor.Replacements[op] = group.Key;
        }

        Optimizer.cfg = visitor.Visit(Optimizer.cfg);
    }
}
