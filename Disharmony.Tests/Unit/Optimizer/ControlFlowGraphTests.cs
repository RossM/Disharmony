namespace Disharmony.Tests.Unit.Optimizer;

[TestFixture]
public sealed class ControlFlowGraphTests
{
    private static readonly ILInstruction Ret = new(OpCodes.Ret, null!, []);

    [Test]
    public void Constructor_PreservesComponentsAndIndexesBlocks()
    {
        RootRegion root = new(new BlockLabel(0));
        BasicBlock block = new(root.EntryLabel, [], root, new Return(Ret, new VoidOp()));
        Argument argument = new(0, typeof(int));
        Local local = new(typeof(string), 0);
        BasicBlock[] blocks = [block];
        Edge[] edges = [];
        Argument[] arguments = [argument];
        Local[] locals = [local];

        ControlFlowGraph graph = new(root, blocks, edges, arguments, locals);

        Assert.Multiple(() =>
        {
            Assert.That(graph.RootRegion, Is.SameAs(root));
            Assert.That(graph.BasicBlocks, Is.SameAs(blocks));
            Assert.That(graph.Edges, Is.Empty);
            Assert.That(graph.Arguments, Is.SameAs(arguments));
            Assert.That(graph.Locals, Is.SameAs(locals));
            Assert.That(graph.GetBlock(block.Label), Is.SameAs(block));
            Assert.That(graph.ExceptionGroups, Is.Empty);
        });
    }

    [Test]
    public void EdgeQueries_BlockAndLabelOverloadsReturnIndexedRelationships()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel middleLabel = new(1);
        BlockLabel targetLabel = new(2);
        StackSlot condition = new(0, typeof(int), 0);
        BasicBlock source = new(root.EntryLabel, [], root,
            new ConditionalBranch(OpCodes.Brtrue, [condition], [middleLabel, targetLabel]));
        BasicBlock middle = new(middleLabel, [], root, new UnconditionalBranch(targetLabel));
        BasicBlock target = new(targetLabel, [], root, new Return(Ret, new VoidOp()));
        Edge sourceToMiddle = new(source.Label, middle.Label, []);
        Edge sourceToTarget = new(source.Label, target.Label, []);
        Edge middleToTarget = new(middle.Label, target.Label, []);
        ControlFlowGraph graph = new(root, [source, middle, target],
            [sourceToMiddle, sourceToTarget, middleToTarget], [], []);

        Assert.Multiple(() =>
        {
            Assert.That(graph.IncomingEdges(target), Is.EquivalentTo([sourceToTarget, middleToTarget]));
            Assert.That(graph.IncomingEdges(target.Label), Is.EquivalentTo([sourceToTarget, middleToTarget]));
            Assert.That(graph.OutgoingEdges(source), Is.EquivalentTo([sourceToMiddle, sourceToTarget]));
            Assert.That(graph.OutgoingEdges(source.Label), Is.EquivalentTo([sourceToMiddle, sourceToTarget]));
            Assert.That(graph.Predecessors(target), Is.EquivalentTo([source, middle]));
            Assert.That(graph.Predecessors(target.Label), Is.EquivalentTo([source.Label, middle.Label]));
            Assert.That(graph.Successors(source), Is.EquivalentTo([middle, target]));
            Assert.That(graph.Successors(source.Label), Is.EquivalentTo([middle.Label, target.Label]));
        });
    }

    [Test]
    public void GetEdge_BlockAndLabelOverloadsReturnTheSameEdge()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel targetLabel = new(1);
        BasicBlock source = new(root.EntryLabel, [], root, new UnconditionalBranch(targetLabel));
        BasicBlock target = new(targetLabel, [], root, new Return(Ret, new VoidOp()));
        Edge edge = new(source.Label, target.Label, []);
        ControlFlowGraph graph = new(root, [source, target], [edge], [], []);

        Assert.Multiple(() =>
        {
            Assert.That(graph.GetEdge(source, target), Is.SameAs(edge));
            Assert.That(graph.GetEdge(source.Label, target.Label), Is.SameAs(edge));
        });
    }

    [Test]
    public void MissingBlockAndRelationships_ThrowKeyNotFoundException()
    {
        RootRegion root = new(new BlockLabel(0));
        BasicBlock block = new(root.EntryLabel, [], root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [block], [], [], []);
        BlockLabel missingLabel = new(1);
        BasicBlock missingBlock = new(missingLabel, [], root, new Return(Ret, new VoidOp()));

        Assert.Multiple(() =>
        {
            Assert.That(() => graph.GetBlock(missingLabel), Throws.TypeOf<KeyNotFoundException>());
            Assert.That(() => graph.GetEdge(block.Label, missingLabel), Throws.TypeOf<KeyNotFoundException>());
            Assert.That(() => graph.GetEdge(block, missingBlock), Throws.TypeOf<KeyNotFoundException>());
            Assert.That(() => graph.IncomingEdges(missingLabel).ToArray(), Throws.TypeOf<KeyNotFoundException>());
            Assert.That(() => graph.IncomingEdges(missingBlock).ToArray(), Throws.TypeOf<KeyNotFoundException>());
            Assert.That(() => graph.OutgoingEdges(missingLabel).ToArray(), Throws.TypeOf<KeyNotFoundException>());
            Assert.That(() => graph.OutgoingEdges(missingBlock).ToArray(), Throws.TypeOf<KeyNotFoundException>());
            Assert.That(() => graph.Predecessors(missingLabel).ToArray(), Throws.TypeOf<KeyNotFoundException>());
            Assert.That(() => graph.Predecessors(missingBlock).ToArray(), Throws.TypeOf<KeyNotFoundException>());
            Assert.That(() => graph.Successors(missingLabel).ToArray(), Throws.TypeOf<KeyNotFoundException>());
            Assert.That(() => graph.Successors(missingBlock).ToArray(), Throws.TypeOf<KeyNotFoundException>());
        });
    }

    [Test]
    public void ExceptionRegionQueries_ReturnGroupAndHandlerOrder()
    {
        RootRegion root = new(new BlockLabel(0));
        CatchRegion catchRegion = new(new BlockLabel(1), root, new StackSlot(0, typeof(Exception), 0));
        FinallyRegion finallyRegion = new(new BlockLabel(2), root);
        FaultRegion faultRegion = new(new BlockLabel(3), root);
        ExceptionGroup group = new([catchRegion, finallyRegion, faultRegion]);
        ProtectedRegion protectedRegion = new(root.EntryLabel, root, group);
        BasicBlock block = new(root.EntryLabel, [], protectedRegion, new Return(Ret, new VoidOp()));

        ControlFlowGraph graph = new(root, [block], [], [], []);

        Assert.Multiple(() =>
        {
            Assert.That(graph.ExceptionGroups, Is.EqualTo(new[] { group }));
            Assert.That(graph.GetExceptionGroup(protectedRegion), Is.SameAs(group));
            Assert.That(graph.GetExceptionGroup(catchRegion), Is.SameAs(group));
            Assert.That(graph.GetExceptionGroup(finallyRegion), Is.SameAs(group));
            Assert.That(graph.GetExceptionGroup(faultRegion), Is.SameAs(group));
            Assert.That(graph.GetNextRegion(protectedRegion), Is.SameAs(catchRegion));
            Assert.That(graph.GetNextRegion(catchRegion), Is.SameAs(finallyRegion));
            Assert.That(graph.GetNextRegion(finallyRegion), Is.SameAs(faultRegion));
            Assert.That(graph.GetNextRegion(faultRegion), Is.Null);
        });
    }

    [Test]
    public void NestedProtectedRegions_IndexInnerAndOuterGroups()
    {
        RootRegion root = new(new BlockLabel(0));
        FinallyRegion outerFinally = new(new BlockLabel(1), root);
        ExceptionGroup outerGroup = new([outerFinally]);
        ProtectedRegion outerProtected = new(root.EntryLabel, root, outerGroup);
        CatchRegion innerCatch = new(new BlockLabel(2), outerProtected,
            new StackSlot(0, typeof(Exception), 0));
        ExceptionGroup innerGroup = new([innerCatch]);
        ProtectedRegion innerProtected = new(root.EntryLabel, outerProtected, innerGroup);
        BasicBlock block = new(root.EntryLabel, [], innerProtected, new Return(Ret, new VoidOp()));

        ControlFlowGraph graph = new(root, [block], [], [], []);

        Assert.Multiple(() =>
        {
            Assert.That(graph.ExceptionGroups, Is.EquivalentTo([innerGroup, outerGroup]));
            Assert.That(graph.GetExceptionGroup(innerProtected), Is.SameAs(innerGroup));
            Assert.That(graph.GetExceptionGroup(innerCatch), Is.SameAs(innerGroup));
            Assert.That(graph.GetExceptionGroup(outerProtected), Is.SameAs(outerGroup));
            Assert.That(graph.GetExceptionGroup(outerFinally), Is.SameAs(outerGroup));
        });
    }

    [Test]
    public void ExceptionRegionQueries_UnindexedRegionThrowsKeyNotFoundException()
    {
        RootRegion root = new(new BlockLabel(0));
        BasicBlock block = new(root.EntryLabel, [], root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [block], [], [], []);
        FinallyRegion missingRegion = new(new BlockLabel(1), root);

        Assert.Multiple(() =>
        {
            Assert.That(() => graph.GetExceptionGroup(missingRegion), Throws.TypeOf<KeyNotFoundException>());
            Assert.That(() => graph.GetNextRegion(missingRegion), Throws.TypeOf<KeyNotFoundException>());
        });
    }

    [Test]
    public void Constructor_DuplicateBlockLabelsThrowArgumentException()
    {
        RootRegion root = new(new BlockLabel(0));
        BasicBlock first = new(root.EntryLabel, [], root, new Return(Ret, new VoidOp()));
        BasicBlock second = new(root.EntryLabel, [], root, new Return(Ret, new VoidOp()));

        Assert.That(() => new ControlFlowGraph(root, [first, second], [], [], []),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Constructor_DuplicateEdgesThrowArgumentException()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel targetLabel = new(1);
        BasicBlock source = new(root.EntryLabel, [], root, new UnconditionalBranch(targetLabel));
        BasicBlock target = new(targetLabel, [], root, new Return(Ret, new VoidOp()));
        Edge first = new(source.Label, target.Label, []);
        Edge second = new(source.Label, target.Label, []);

        Assert.That(() => new ControlFlowGraph(root, [source, target], [first, second], [], []),
            Throws.TypeOf<ArgumentException>());
    }

#if DEBUG
    [Test]
    public void Constructor_ValidationRejectsMissingBranchEdge()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel targetLabel = new(1);
        BasicBlock source = new(root.EntryLabel, [], root, new UnconditionalBranch(targetLabel));
        BasicBlock target = new(targetLabel, [], root, new Return(Ret, new VoidOp()));

        Assert.That(() => new ControlFlowGraph(root, [source, target], [], [], []),
            Throws.InvalidOperationException.With.Message.EqualTo("Edge not found"));
    }

    [Test]
    public void Constructor_ValidationRejectsUnreferencedEdge()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel targetLabel = new(1);
        BasicBlock source = new(root.EntryLabel, [], root, new Return(Ret, new VoidOp()));
        BasicBlock target = new(targetLabel, [], root, new Return(Ret, new VoidOp()));
        Edge edge = new(source.Label, target.Label, []);

        Assert.That(() => new ControlFlowGraph(root, [source, target], [edge], [], []),
            Throws.InvalidOperationException.With.Message.EqualTo("Edge not referenced"));
    }

    [Test]
    public void Constructor_ValidationRejectsHandlerWithoutExceptionGroup()
    {
        RootRegion root = new(new BlockLabel(0));
        FinallyRegion finallyRegion = new(root.EntryLabel, root);
        BasicBlock block = new(root.EntryLabel, [], finallyRegion, new Return(Ret, new VoidOp()));

        Assert.That(() => new ControlFlowGraph(root, [block], [], [], []),
            Throws.InvalidOperationException.With.Message.EqualTo("Region not in group"));
    }
#endif

    [Test]
    public void Constructor_ValidationCanBeDisabledForAnIncompleteGraph()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel missingTarget = new(1);
        BasicBlock block = new(root.EntryLabel, [], root, new UnconditionalBranch(missingTarget));

        ControlFlowGraph graph = new(root, [block], [], [], [], validate: false);

        Assert.Multiple(() =>
        {
            Assert.That(graph.GetBlock(block.Label), Is.SameAs(block));
            Assert.That(graph.OutgoingEdges(block), Is.Empty);
        });
    }

    [Test]
    public void Equality_SameComponentsAreEqualAndHaveEqualHashCodes()
    {
        RootRegion root = new(new BlockLabel(0));
        BasicBlock block = new(root.EntryLabel, [], root, new Return(Ret, new VoidOp()));
        BasicBlock[] blocks = [block];
        Edge[] edges = [];
        ControlFlowGraph first = new(root, blocks, edges, [], []);
        ControlFlowGraph second = new(root, blocks, edges, [], []);

        Assert.Multiple(() =>
        {
            Assert.That(first.Equals(second), Is.True);
            Assert.That(second.Equals(first), Is.True);
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
            Assert.That(first.Equals(null), Is.False);
        });
    }
}
