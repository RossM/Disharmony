namespace Disharmony.Tests.Unit.Optimizer;

[TestFixture]
public sealed class NodeEqualityTests
{
    [Test]
    public void ILInstruction_Prefixes_UseElementEquality()
    {
        ILInstruction first = new(OpCodes.Ldind_I4, null,
            [new Prefix(OpCodes.Unaligned, (byte)1), new Prefix(OpCodes.Volatile, null)]);
        ILInstruction equal = new(OpCodes.Ldind_I4, null,
            [new Prefix(OpCodes.Unaligned, (byte)1), new Prefix(OpCodes.Volatile, null)]);
        ILInstruction reordered = new(OpCodes.Ldind_I4, null,
            [new Prefix(OpCodes.Volatile, null), new Prefix(OpCodes.Unaligned, (byte)1)]);
        ILInstruction different = new(OpCodes.Ldind_I4, null,
            [new Prefix(OpCodes.Unaligned, (byte)2), new Prefix(OpCodes.Volatile, null)]);

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(equal));
            Assert.That(first.GetHashCode(), Is.EqualTo(equal.GetHashCode()));
            Assert.That(first, Is.Not.EqualTo(reordered));
            Assert.That(first, Is.Not.EqualTo(different));
        });
    }

    [Test]
    public void ILOp_Inputs_UseElementEquality()
    {
        ILInstruction add = new(OpCodes.Add, null, []);
        ILOp first = new(add,
            [new StackSlot(0, typeof(int), 0), new StackSlot(1, typeof(int), 1)], typeof(int));
        ILOp equal = new(new ILInstruction(OpCodes.Add, null, []),
            [new StackSlot(0, typeof(int), 0), new StackSlot(1, typeof(int), 1)], typeof(int));
        ILOp reordered = new(add,
            [new StackSlot(1, typeof(int), 1), new StackSlot(0, typeof(int), 0)], typeof(int));
        ILOp different = new(add,
            [new StackSlot(0, typeof(int), 0), new StackSlot(1, typeof(long), 1)], typeof(int));

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(equal));
            Assert.That(first.GetHashCode(), Is.EqualTo(equal.GetHashCode()));
            Assert.That(first, Is.Not.EqualTo(reordered));
            Assert.That(first, Is.Not.EqualTo(different));
        });
    }

    [Test]
    public void ExceptionGroup_HandlerRegions_UseElementEquality()
    {
        RootRegion parent = new(new BlockLabel(0));
        ExceptionGroup first = new([
            new FinallyRegion(new BlockLabel(1), parent),
            new FaultRegion(new BlockLabel(2), parent),
        ]);
        ExceptionGroup equal = new([
            new FinallyRegion(new BlockLabel(1), new RootRegion(new BlockLabel(0))),
            new FaultRegion(new BlockLabel(2), new RootRegion(new BlockLabel(0))),
        ]);
        ExceptionGroup reordered = new([
            new FaultRegion(new BlockLabel(2), parent),
            new FinallyRegion(new BlockLabel(1), parent),
        ]);
        ExceptionGroup different = new([
            new FinallyRegion(new BlockLabel(1), parent),
            new FaultRegion(new BlockLabel(3), parent),
        ]);

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(equal));
            Assert.That(first.GetHashCode(), Is.EqualTo(equal.GetHashCode()));
            Assert.That(first, Is.Not.EqualTo(reordered));
            Assert.That(first, Is.Not.EqualTo(different));
        });
    }

    [Test]
    public void Branch_Labels_UseElementEquality()
    {
        UnconditionalBranch first = new(new BlockLabel(1));
        UnconditionalBranch equal = new(new BlockLabel(1));
        UnconditionalBranch different = new(new BlockLabel(2));

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(equal));
            Assert.That(first.GetHashCode(), Is.EqualTo(equal.GetHashCode()));
            Assert.That(first, Is.Not.EqualTo(different));
        });
    }

    [Test]
    public void ConditionalBranch_InputsAndLabels_UseElementEquality()
    {
        ConditionalBranch first = new(OpCodes.Beq,
            [new StackSlot(0, typeof(int), 0), new StackSlot(1, typeof(int), 1)],
            [new BlockLabel(1), new BlockLabel(2)]);
        ConditionalBranch equal = new(OpCodes.Beq,
            [new StackSlot(0, typeof(int), 0), new StackSlot(1, typeof(int), 1)],
            [new BlockLabel(1), new BlockLabel(2)]);
        ConditionalBranch reorderedInputs = new(OpCodes.Beq,
            [new StackSlot(1, typeof(int), 1), new StackSlot(0, typeof(int), 0)],
            [new BlockLabel(1), new BlockLabel(2)]);
        ConditionalBranch reorderedLabels = new(OpCodes.Beq,
            [new StackSlot(0, typeof(int), 0), new StackSlot(1, typeof(int), 1)],
            [new BlockLabel(2), new BlockLabel(1)]);

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(equal));
            Assert.That(first.GetHashCode(), Is.EqualTo(equal.GetHashCode()));
            Assert.That(first, Is.Not.EqualTo(reorderedInputs));
            Assert.That(first, Is.Not.EqualTo(reorderedLabels));
        });
    }

    [Test]
    public void BasicBlock_Ops_UseElementEquality()
    {
        RootRegion region = new(new BlockLabel(0));
        ILInstruction ret = new(OpCodes.Ret, null, []);
        BasicBlock first = new(region.EntryLabel,
            [new AssignmentOp(new Temporary(typeof(int), 0), new StackSlot(0, typeof(int), 0))],
            region, new Return(ret, new VoidOp()));
        BasicBlock equal = new(new BlockLabel(0),
            [new AssignmentOp(new Temporary(typeof(int), 0), new StackSlot(0, typeof(int), 0))],
            new RootRegion(new BlockLabel(0)), new Return(new ILInstruction(OpCodes.Ret, null, []), new VoidOp()));
        BasicBlock different = new(region.EntryLabel,
            [new AssignmentOp(new Temporary(typeof(int), 0), new StackSlot(0, typeof(long), 0))],
            region, new Return(ret, new VoidOp()));

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(equal));
            Assert.That(first.GetHashCode(), Is.EqualTo(equal.GetHashCode()));
            Assert.That(first, Is.Not.EqualTo(different));
        });
    }

    [Test]
    public void Edge_Assignments_UseElementEquality()
    {
        Edge first = new(new BlockLabel(0), new BlockLabel(1), [
            new AssignmentOp(new StackSlot(0, typeof(int), 1), new StackSlot(0, typeof(int), 0)),
            new AssignmentOp(new StackSlot(1, typeof(int), 3), new StackSlot(1, typeof(int), 2)),
        ]);
        Edge equal = new(new BlockLabel(0), new BlockLabel(1), [
            new AssignmentOp(new StackSlot(0, typeof(int), 1), new StackSlot(0, typeof(int), 0)),
            new AssignmentOp(new StackSlot(1, typeof(int), 3), new StackSlot(1, typeof(int), 2)),
        ]);
        Edge reordered = new(new BlockLabel(0), new BlockLabel(1), [
            new AssignmentOp(new StackSlot(1, typeof(int), 3), new StackSlot(1, typeof(int), 2)),
            new AssignmentOp(new StackSlot(0, typeof(int), 1), new StackSlot(0, typeof(int), 0)),
        ]);
        Edge different = new(new BlockLabel(0), new BlockLabel(1), [
            new AssignmentOp(new StackSlot(0, typeof(int), 1), new StackSlot(0, typeof(long), 0)),
            new AssignmentOp(new StackSlot(1, typeof(int), 3), new StackSlot(1, typeof(int), 2)),
        ]);

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(equal));
            Assert.That(first.GetHashCode(), Is.EqualTo(equal.GetHashCode()));
            Assert.That(first, Is.Not.EqualTo(reordered));
            Assert.That(first, Is.Not.EqualTo(different));
        });
    }

    [Test]
    public void ControlFlowGraph_Collections_UseElementEqualityAndEdgesIgnoreOrder()
    {
        RootRegion firstRoot = new(new BlockLabel(0));
        BlockLabel firstExitLabel = new(1);
        BasicBlock firstEntry = new(firstRoot.EntryLabel, [], firstRoot, new UnconditionalBranch(firstExitLabel));
        BasicBlock firstExit = new(firstExitLabel, [], firstRoot,
            new Return(new ILInstruction(OpCodes.Ret, null, []), new VoidOp()));
        Edge firstEdge = new(firstEntry.Label, firstExit.Label, []);
        ControlFlowGraph first = new(firstRoot, [firstEntry, firstExit], [firstEdge],
            [new Argument(0, typeof(int))], [new Local(typeof(string), 0)]);

        RootRegion equalRoot = new(new BlockLabel(0));
        BlockLabel equalExitLabel = new(1);
        BasicBlock equalEntry = new(equalRoot.EntryLabel, [], equalRoot, new UnconditionalBranch(equalExitLabel));
        BasicBlock equalExit = new(equalExitLabel, [], equalRoot,
            new Return(new ILInstruction(OpCodes.Ret, null, []), new VoidOp()));
        Edge equalEdge = new(equalEntry.Label, equalExit.Label, []);
        ControlFlowGraph equal = new(equalRoot, [equalEntry, equalExit], [equalEdge],
            [new Argument(0, typeof(int))], [new Local(typeof(string), 0)]);

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(equal));
            Assert.That(equal, Is.EqualTo(first));
            Assert.That(first.GetHashCode(), Is.EqualTo(equal.GetHashCode()));
        });
    }

    [Test]
    public void ControlFlowGraph_Edges_UseSetEquality()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel fallthroughLabel = new(1);
        BlockLabel targetLabel = new(2);
        BasicBlock entry = new(root.EntryLabel, [], root,
            new ConditionalBranch(OpCodes.Brtrue, [new StackSlot(0, typeof(int), 0)],
                [fallthroughLabel, targetLabel]));
        BasicBlock fallthrough = new(fallthroughLabel, [], root,
            new Return(new ILInstruction(OpCodes.Ret, null, []), new VoidOp()));
        BasicBlock target = new(targetLabel, [], root,
            new Return(new ILInstruction(OpCodes.Ret, null, []), new VoidOp()));
        Edge fallthroughEdge = new(entry.Label, fallthrough.Label, []);
        Edge targetEdge = new(entry.Label, target.Label, []);
        ControlFlowGraph first = new(root, [entry, fallthrough, target], [fallthroughEdge, targetEdge], [], []);
        ControlFlowGraph reversed = new(root, [entry, fallthrough, target], [
            new Edge(entry.Label, target.Label, []),
            new Edge(entry.Label, fallthrough.Label, []),
        ], [], []);

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(reversed));
            Assert.That(first.GetHashCode(), Is.EqualTo(reversed.GetHashCode()));
        });
    }

    [Test]
    public void ControlFlowGraph_CollectionElementDifferences_BreakEquality()
    {
        RootRegion root = new(new BlockLabel(0));
        BasicBlock block = new(root.EntryLabel, [], root,
            new Return(new ILInstruction(OpCodes.Ret, null, []), new VoidOp()));
        ControlFlowGraph original = new(root, [block], [],
            [new Argument(0, typeof(int))], [new Local(typeof(string), 0)]);
        ControlFlowGraph differentBlock = new(root, [new BasicBlock(root.EntryLabel,
            [new AssignmentOp(new Temporary(typeof(int), 0), new StackSlot(0, typeof(int), 0))], root,
            new Return(new ILInstruction(OpCodes.Ret, null, []), new VoidOp()))], [],
            [new Argument(0, typeof(int))], [new Local(typeof(string), 0)]);
        ControlFlowGraph differentArgument = new(root, [block], [],
            [new Argument(0, typeof(long))], [new Local(typeof(string), 0)]);
        ControlFlowGraph differentLocal = new(root, [block], [],
            [new Argument(0, typeof(int))], [new Local(typeof(object), 0)]);

        Assert.Multiple(() =>
        {
            Assert.That(original, Is.Not.EqualTo(differentBlock));
            Assert.That(original, Is.Not.EqualTo(differentArgument));
            Assert.That(original, Is.Not.EqualTo(differentLocal));
        });
    }
}
