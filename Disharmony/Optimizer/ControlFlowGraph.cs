using System.Diagnostics;

namespace Disharmony.Optimizer;

internal record ControlFlowGraph : Node
{
    public IEnumerable<ExceptionGroup> ExceptionGroups => exceptionGroups;
    public IEnumerable<Edge> Edges => edgesFrom.Values.SelectMany(edges => edges.Values);

    public static readonly ControlFlowGraph Empty = new(new RootRegion(new BlockLabel(0)), [], [], [], []);

    private readonly HashSet<ExceptionGroup> exceptionGroups = [];
    private readonly Dictionary<ExceptionRegion, ExceptionGroup> exceptionGroupsByRegion = [];
    private readonly Dictionary<ExceptionRegion, ExceptionRegion?> nextRegion = [];
    private readonly Dictionary<BlockLabel, Dictionary<BlockLabel, Edge>> edgesFrom = [];
    private readonly Dictionary<BlockLabel, Dictionary<BlockLabel, Edge>> edgesTo = [];
    private readonly Dictionary<BlockLabel, BasicBlock> basicBlocks = [];

    public ControlFlowGraph(
        RootRegion RootRegion,
        IReadOnlyList<BasicBlock> BasicBlocks,
        IReadOnlyList<Edge> Edges,
        IReadOnlyList<Argument> Arguments,
        IReadOnlyList<Local> Locals,
        bool validate = true)
    {
        this.RootRegion = RootRegion;
        this.BasicBlocks = BasicBlocks;
        this.Arguments = Arguments;
        this.Locals = Locals;

        foreach (var block in BasicBlocks)
            AddBlock(block);
        foreach (var edge in Edges)
            AddEdge(edge);

        if (validate)
            Validate();
    }

    public RootRegion RootRegion { get; }
    public IReadOnlyList<BasicBlock> BasicBlocks { get; }
    public IReadOnlyList<Argument> Arguments { get; }
    public IReadOnlyList<Local> Locals { get; }

    /// <summary>
    ///     Returns all incoming <see cref="Edge" />s for a <see cref="BasicBlock" />.
    /// </summary>
    /// <param name="block">The <see cref="BasicBlock" /> whose incoming <see cref="Edge" />s to return.</param>
    /// <returns>The <see cref="Edge" />s whose destination is <paramref name="block" />.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="block" /> is not present in the graph.</exception>
    public IEnumerable<Edge> IncomingEdges(BasicBlock block) => edgesTo[block.Label].Values;

    /// <summary>
    ///     Returns all incoming <see cref="Edge" />s for the <see cref="BasicBlock" /> with the given
    ///     <see cref="BlockLabel" />.
    /// </summary>
    /// <param name="label">The label of the <see cref="BasicBlock" /> whose incoming <see cref="Edge" />s to return.</param>
    /// <returns>The <see cref="Edge" />s whose destination is <paramref name="label" />.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="label" /> is not present in the graph.</exception>
    public IEnumerable<Edge> IncomingEdges(BlockLabel label) => edgesTo[label].Values;

    /// <summary>
    ///     Returns all outgoing <see cref="Edge" />s for a <see cref="BasicBlock" />.
    /// </summary>
    /// <param name="block">The <see cref="BasicBlock" /> whose outgoing <see cref="Edge" />s to return.</param>
    /// <returns>The <see cref="Edge" />s whose source is <paramref name="block" />.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="block" /> is not present in the graph.</exception>
    public IEnumerable<Edge> OutgoingEdges(BasicBlock block) => edgesFrom[block.Label].Values;

    /// <summary>
    ///     Returns all outgoing <see cref="Edge" />s for the <see cref="BasicBlock" /> with the given
    ///     <see cref="BlockLabel" />.
    /// </summary>
    /// <param name="label">The label of the <see cref="BasicBlock" /> whose outgoing <see cref="Edge" />s to return.</param>
    /// <returns>The <see cref="Edge" />s whose source is <paramref name="label" />.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="label" /> is not present in the graph.</exception>
    public IEnumerable<Edge> OutgoingEdges(BlockLabel label) => edgesFrom[label].Values;

    /// <summary>
    ///     Gets all <see cref="BasicBlock" />s with edges to <paramref name="block" />.
    /// </summary>
    /// <param name="block">The <see cref="BasicBlock" /> whose predecessors to return.</param>
    /// <returns>The <see cref="BasicBlock" />s with <see cref="Edge" />s to <paramref name="block" />.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="block" /> is not present in the graph.</exception>
    public IEnumerable<BasicBlock> Predecessors(BasicBlock block) => IncomingEdges(block.Label).Select(edge => basicBlocks[edge.Source]);

    /// <summary>
    ///     Gets the <see cref="BlockLabel" />s of all <see cref="BasicBlock" />s with edges to the block with
    ///     <see cref="BlockLabel" /> <paramref name="label" />.
    /// </summary>
    /// <param name="label">The label of the block whose predecessor labels to return.</param>
    /// <returns>
    ///     The labels of the <see cref="BasicBlock" />s with <see cref="Edge" />s to the block identified by
    ///     <paramref name="label" />.
    /// </returns>
    /// <exception cref="KeyNotFoundException"><paramref name="label" /> is not present in the graph.</exception>
    public IEnumerable<BlockLabel> Predecessors(BlockLabel label) => IncomingEdges(label).Select(edge => edge.Source);

    /// <summary>
    ///     Gets all <see cref="BasicBlock" />s with edges from <paramref name="block" />.
    /// </summary>
    /// <param name="block">The <see cref="BasicBlock" /> whose successors to return.</param>
    /// <returns>The <see cref="BasicBlock" />s with <see cref="Edge" />s from <paramref name="block" />.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="block" /> is not present in the graph.</exception>
    public IEnumerable<BasicBlock> Successors(BasicBlock block) => OutgoingEdges(block.Label).Select(edge => basicBlocks[edge.Destination]);

    /// <summary>
    ///     Gets the <see cref="BlockLabel" />s of all <see cref="BasicBlock" />s with edges from the block with
    ///     <see cref="BlockLabel" /> <paramref name="label" />.
    /// </summary>
    /// <param name="label">The label of the block whose successor labels to return.</param>
    /// <returns>
    ///     The labels of the <see cref="BasicBlock" />s with <see cref="Edge" />s from the block identified by
    ///     <paramref name="label" />.
    /// </returns>
    /// <exception cref="KeyNotFoundException"><paramref name="label" /> is not present in the graph.</exception>
    public IEnumerable<BlockLabel> Successors(BlockLabel label) => OutgoingEdges(label).Select(edge => edge.Destination);

    /// <summary>
    ///     Gets the edge from the block with the given source <see cref="BlockLabel" /> to the block with the given
    ///     destination <see cref="BlockLabel" />.
    /// </summary>
    /// <param name="source">The source block label.</param>
    /// <param name="destination">The destination block label.</param>
    /// <returns>The <see cref="Edge" /> from <paramref name="source" /> to <paramref name="destination" />.</returns>
    /// <exception cref="KeyNotFoundException">No edge exists between the specified blocks.</exception>
    public Edge GetEdge(BlockLabel source, BlockLabel destination) => edgesFrom[source][destination];

    /// <summary>
    ///     Gets the <see cref="Edge" /> from the given source <see cref="BasicBlock" /> to the given destination
    ///     <see cref="BasicBlock" />.
    /// </summary>
    /// <param name="source">The source <see cref="BasicBlock" />.</param>
    /// <param name="destination">The destination <see cref="BasicBlock" />.</param>
    /// <returns>The <see cref="Edge" /> from <paramref name="source" /> to <paramref name="destination" />.</returns>
    /// <exception cref="KeyNotFoundException">No edge exists between the specified blocks.</exception>
    public Edge GetEdge(BasicBlock source, BasicBlock destination) => edgesFrom[source.Label][destination.Label];

    /// <summary>
    ///     Gets the <see cref="BasicBlock" /> with the given <see cref="BlockLabel" />.
    /// </summary>
    /// <param name="label">The label of the block to return.</param>
    /// <returns>The <see cref="BasicBlock" /> identified by <paramref name="label" />.</returns>
    /// <exception cref="KeyNotFoundException">No block with the specified label exists.</exception>
    public BasicBlock GetBlock(BlockLabel label) => basicBlocks[label];

    /// <summary>
    ///     Gets the <see cref="ExceptionGroup" /> containing the given <see cref="ExceptionRegion" />.
    /// </summary>
    /// <param name="region">The exception region whose group to return.</param>
    /// <returns>The <see cref="ExceptionGroup" /> containing <paramref name="region" />.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="region" /> does not belong to a group in the graph.</exception>
    public ExceptionGroup GetExceptionGroup(ExceptionRegion region) => exceptionGroupsByRegion[region];

    /// <summary>
    ///     Gets the next <see cref="ExceptionRegion" /> in order in the <see cref="ExceptionGroup" /> containing the given
    ///     <see cref="ExceptionRegion" />, or <see langword="null" /> if it is the last region in the group.
    /// </summary>
    /// <param name="region">The exception region whose successor to return.</param>
    /// <returns>The next region in the group, or <see langword="null" /> if <paramref name="region" /> is the last region.</returns>
    /// <exception cref="KeyNotFoundException"><paramref name="region" /> does not belong to a group in the graph.</exception>
    public ExceptionRegion? GetNextRegion(ExceptionRegion region) => nextRegion[region];

    private void AddBlock(BasicBlock block)
    {
        basicBlocks.Add(block.Label, block);
        edgesFrom.Add(block.Label, []);
        edgesTo.Add(block.Label, []);

        for (ExceptionRegion? region = block.Region as ExceptionRegion; region != null; region = region.Parent as ExceptionRegion)
        {
            if (region is ProtectedRegion protectedRegion && !exceptionGroupsByRegion.ContainsKey(protectedRegion))
                AddProtectedRegion(protectedRegion);
        }
    }

    private void AddEdge(Edge edge)
    {
        edgesFrom[edge.Source].Add(edge.Destination, edge);
        edgesTo[edge.Destination].Add(edge.Source, edge);
    }

    private void AddProtectedRegion(ProtectedRegion protectedRegion)
    {
        var group = protectedRegion.Group;
        exceptionGroups.Add(group);

        ExceptionRegion[] regions = [protectedRegion, .. group.HandlerRegions];
        for (int i = 0; i < regions.Length; i++)
        {
            var region = regions[i];
            var next = i + 1 < regions.Length ? regions[i + 1] : null;
            exceptionGroupsByRegion[region] = group;
            nextRegion[region] = next;
        }
    }

    /// <summary>
    ///     Validates the current state.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     The graph is missing an edge or contains an edge that is not referenced by
    ///     its source block.
    /// </exception>
    [Conditional("DEBUG")]
    private void Validate()
    {
        foreach (var block in BasicBlocks)
        foreach (var successor in block.Branch.Labels)
        {
            if (!edgesFrom[block.Label].ContainsKey(successor))
                throw new InvalidOperationException("Edge not found");
        }

        foreach (var edge in Edges)
        {
            if (!basicBlocks[edge.Source].Branch.Labels.Contains(edge.Destination))
                throw new InvalidOperationException("Edge not referenced");
        }

        foreach (var block in BasicBlocks)
        {
            var region = block.Region;
            while (region is ExceptionRegion exceptionRegion)
            {
                if (!exceptionGroupsByRegion.ContainsKey(exceptionRegion))
                    throw new InvalidOperationException("Region not in group");
                region = exceptionRegion.Parent;
            }
        }
    }

    public override void DebugPrint()
    {
        FileLog.LogBuffered("ControlFlowGraph {");
        FileLog.ChangeIndent(1);

        DebugPrintVariables("Arguments", Arguments);
        DebugPrintVariables("Locals", Locals);

        DebugPrintRegion(RootRegion);

        FileLog.ChangeIndent(-1);
        FileLog.LogBuffered("}");
    }

    private static void DebugPrintVariables(object name, IReadOnlyList<Node> values)
    {
        if (values.Count == 0)
        {
            FileLog.LogBuffered($"{name} {{ }}");
            return;
        }

        FileLog.LogBuffered($"{name} {{");
        FileLog.ChangeIndent(1);
        foreach (var argument in values)
            argument.DebugPrint();
        FileLog.ChangeIndent(-1);
        FileLog.LogBuffered("}");
    }

    private void DebugPrintBlock(BasicBlock basicBlock)
    {
        basicBlock.DebugPrint();
        foreach (var successor in basicBlock.Branch.Labels.Distinct())
        {
            var edge = GetEdge(basicBlock.Label, successor);
            FileLog.LogBuffered(edge.EdgeAssignments.Count > 0
                ? $"-> {edge.Destination} {{ {string.Join(", ", edge.EdgeAssignments)} }}"
                : $"-> {edge.Destination}");
        }
    }

    private void DebugPrintRegion(Region region)
    {
        if (region is ProtectedRegion or Disharmony.Optimizer.RootRegion)
            FileLog.LogBuffered($"{region} {{");
        else
            FileLog.LogBuffered($"}} {region} {{");
        FileLog.ChangeIndent(1);

        foreach (var block in BasicBlocks.Where(b => b.Region == region).OrderByDescending(b => region.EntryLabel == b.Label))
            DebugPrintBlock(block);

        foreach (var child in exceptionGroupsByRegion.Keys.OfType<ProtectedRegion>().Where(r => r.Parent == region))
            DebugPrintRegion(child);

        FileLog.ChangeIndent(-1);
        if (region is ExceptionRegion e && GetNextRegion(e) is { } next)
            DebugPrintRegion(next);
        else
            FileLog.LogBuffered("}");
    }

    public virtual bool Equals(ControlFlowGraph? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return base.Equals(other) && RootRegion.Equals(other.RootRegion) && BasicBlocks.SequenceEqual(other.BasicBlocks) &&
               Edges.ToHashSet().SetEquals(other.Edges.ToHashSet()) &&
               Arguments.SequenceEqual(other.Arguments) && Locals.SequenceEqual(other.Locals);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = base.GetHashCode();
            hashCode = (hashCode * 397) ^ RootRegion.GetHashCode();
            foreach (var block in BasicBlocks)
                hashCode = (hashCode * 397) ^ block.GetHashCode();
            foreach (var edge in Edges.OrderBy(e => e.GetHashCode()))
                hashCode = (hashCode * 397) ^ edge.GetHashCode();
            foreach (var argument in Arguments)
                hashCode = (hashCode * 397) ^ argument.GetHashCode();
            foreach (var local in Locals)
                hashCode = (hashCode * 397) ^ local.GetHashCode();
            return hashCode;
        }
    }

    public Argument GetArgument(ILInstruction ilInstruction) => Arguments[OpCodeData.GetIntOperand(ilInstruction)];
    public Local GetLocal(ILInstruction ilInstruction) => Locals[LocalTracker.IndexFrom(ilInstruction)];
}
