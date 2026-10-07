using Disharmony.Optimizer.Passes;

namespace Disharmony.Tests.Unit.Optimizer.Passes;

[TestFixture]
public sealed class MergeStackSlotsTests
{
    private static readonly MethodInfo VoidMethod =
        typeof(ControlFlowGraphTargets).GetMethod(nameof(ControlFlowGraphTargets.ReturnVoid))!;

    private static readonly ILInstruction Ret = new(OpCodes.Ret, null!, []);

    [Test]
    public void MergeStackSlots_NoEdgeAssignments_PreservesTheGraph()
    {
        RootRegion root = new(new BlockLabel(0));
        BasicBlock block = new(root.EntryLabel, [], root,
            new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [block], [], [], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new MergeStackSlots(optimizer).RunInternal();
        graph = optimizer.cfg;

        Assert.Multiple(() =>
        {
            Assert.That(graph.GetBlock(block.Label), Is.SameAs(block));
            Assert.That(graph.Edges.SelectMany(edge => edge.EdgeAssignments), Is.Empty);
        });
    }

    [Test]
    public void MergeStackSlots_OneEdge_RewritesTheTargetAndRemovesTheCopy()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel destination = new(1);
        StackSlot sourceSlot = new(0, typeof(int), 0);
        StackSlot targetSlot = new(0, typeof(int), 1);
        BasicBlock source = new(root.EntryLabel, [], root,
            new UnconditionalBranch(destination));
        BasicBlock target = new(destination, [], root, new Return(Ret, targetSlot));
        ControlFlowGraph graph = new(root, [source, target],
            [new Edge(source.Label, target.Label, [new AssignmentOp(targetSlot, sourceSlot)])], [], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new MergeStackSlots(optimizer).RunInternal();
        graph = optimizer.cfg;

        Assert.Multiple(() =>
        {
            Assert.That(((Return)graph.GetBlock(target.Label).Branch).Value, Is.SameAs(sourceSlot));
            Assert.That(graph.Edges.SelectMany(edge => edge.EdgeAssignments), Is.Empty);
        });
    }

    [Test]
    public void MergeStackSlots_ChainOfEdges_RewritesEverySlotToTheOriginalSource()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel middleLabel = new(1);
        BlockLabel targetLabel = new(2);
        StackSlot sourceSlot = new(0, typeof(int), 0);
        StackSlot middleSlot = new(0, typeof(int), 1);
        StackSlot targetSlot = new(0, typeof(int), 2);
        BasicBlock source = new(root.EntryLabel, [], root,
            new UnconditionalBranch(middleLabel));
        BasicBlock middle = new(middleLabel, [], root, new UnconditionalBranch(targetLabel));
        BasicBlock target = new(targetLabel, [], root, new Return(Ret, targetSlot));
        ControlFlowGraph graph = new(root, [source, middle, target],
        [
            new Edge(source.Label, middle.Label, [new AssignmentOp(middleSlot, sourceSlot)]),
            new Edge(middle.Label, target.Label, [new AssignmentOp(targetSlot, middleSlot)]),
        ], [], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new MergeStackSlots(optimizer).RunInternal();
        graph = optimizer.cfg;

        Assert.Multiple(() =>
        {
            Assert.That(((Return)graph.GetBlock(target.Label).Branch).Value, Is.SameAs(sourceSlot));
            Assert.That(graph.Edges.SelectMany(edge => edge.EdgeAssignments), Is.Empty);
        });
    }

    [Test]
    public void MergeStackSlots_RewritesSlotsUsedByILOperations()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel destination = new(1);
        StackSlot sourceSlot = new(0, typeof(int), 0);
        StackSlot targetSlot = new(0, typeof(int), 1);
        ILOp pop = new(new ILInstruction(OpCodes.Pop, null!, []), [targetSlot], typeof(void));
        BasicBlock source = new(root.EntryLabel, [], root,
            new UnconditionalBranch(destination));
        BasicBlock target = new(destination, [pop], root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [source, target],
            [new Edge(source.Label, target.Label, [new AssignmentOp(targetSlot, sourceSlot)])], [], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new MergeStackSlots(optimizer).RunInternal();
        graph = optimizer.cfg;

        var rewritten = (ILOp)graph.GetBlock(target.Label).Ops.Single();
        Assert.Multiple(() =>
        {
            Assert.That(rewritten.Inputs.Single(), Is.SameAs(sourceSlot));
            Assert.That(graph.Edges.SelectMany(edge => edge.EdgeAssignments), Is.Empty);
        });
    }

    [Test]
    public void MergeStackSlots_RewritesSlotsUsedByConditionalBranches()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel conditionLabel = new(1);
        BlockLabel fallthroughLabel = new(2);
        BlockLabel takenLabel = new(3);
        StackSlot sourceSlot = new(0, typeof(int), 0);
        StackSlot conditionSlot = new(0, typeof(int), 1);
        BasicBlock source = new(root.EntryLabel, [], root,
            new UnconditionalBranch(conditionLabel));
        BasicBlock condition = new(conditionLabel, [], root,
            new ConditionalBranch(OpCodes.Brtrue, [conditionSlot], [fallthroughLabel, takenLabel]));
        BasicBlock fallthrough = new(fallthroughLabel, [], root, new Return(Ret, new VoidOp()));
        BasicBlock taken = new(takenLabel, [], root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [source, condition, fallthrough, taken],
        [
            new Edge(source.Label, condition.Label, [new AssignmentOp(conditionSlot, sourceSlot)]),
            new Edge(condition.Label, fallthrough.Label, []),
            new Edge(condition.Label, taken.Label, []),
        ], [], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new MergeStackSlots(optimizer).RunInternal();
        graph = optimizer.cfg;

        var rewritten = (ConditionalBranch)graph.GetBlock(condition.Label).Branch;
        Assert.Multiple(() =>
        {
            Assert.That(rewritten.Inputs.Single(), Is.SameAs(sourceSlot));
            Assert.That(graph.Edges.SelectMany(edge => edge.EdgeAssignments), Is.Empty);
        });
    }

    [Test]
    public void MergeStackSlots_DisconnectedSets_RemainDistinct()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel secondLabel = new(1);
        BlockLabel thirdLabel = new(2);
        StackSlot firstSource = new(0, typeof(int), 0);
        StackSlot firstTarget = new(0, typeof(int), 1);
        StackSlot secondSource = new(1, typeof(int), 2);
        StackSlot secondTarget = new(1, typeof(int), 3);
        BasicBlock first = new(root.EntryLabel, [], root,
            new UnconditionalBranch(secondLabel));
        BasicBlock second = new(secondLabel, [], root, new UnconditionalBranch(thirdLabel));
        ILOp add = new(new ILInstruction(OpCodes.Add, null!, []), [firstTarget, secondTarget], typeof(int));
        BasicBlock third = new(thirdLabel, [add], root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [first, second, third],
        [
            new Edge(first.Label, second.Label, [new AssignmentOp(firstTarget, firstSource)]),
            new Edge(second.Label, third.Label, [new AssignmentOp(secondTarget, secondSource)]),
        ], [], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new MergeStackSlots(optimizer).RunInternal();
        graph = optimizer.cfg;

        var rewritten = (ILOp)graph.GetBlock(third.Label).Ops.Single();
        Assert.Multiple(() =>
        {
            Assert.That(rewritten.Inputs[0], Is.SameAs(firstSource));
            Assert.That(rewritten.Inputs[1], Is.SameAs(secondSource));
            Assert.That(rewritten.Inputs[0], Is.Not.SameAs(rewritten.Inputs[1]));
            Assert.That(graph.Edges.SelectMany(edge => edge.EdgeAssignments), Is.Empty);
        });
    }

    [Test]
    public void MergeStackSlots_MultipleIncomingEdgesMergeEverySourceWithTheTarget()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel secondLabel = new(1);
        BlockLabel targetLabel = new(2);
        StackSlot firstSource = new(0, typeof(int), 0);
        StackSlot secondSource = new(0, typeof(int), 1);
        StackSlot targetSlot = new(0, typeof(int), 2);
        BasicBlock first = new(root.EntryLabel, [], root, new UnconditionalBranch(targetLabel));
        BasicBlock second = new(secondLabel, [], root, new UnconditionalBranch(targetLabel));
        BasicBlock target = new(targetLabel, [], root, new Return(Ret, targetSlot));
        ControlFlowGraph graph = new(root, [first, second, target],
        [
            new Edge(first.Label, target.Label, [new AssignmentOp(targetSlot, firstSource)]),
            new Edge(second.Label, target.Label, [new AssignmentOp(targetSlot, secondSource)]),
        ], [], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new MergeStackSlots(optimizer).RunInternal();

        Op rewrittenReturn = ((Return)optimizer.cfg.GetBlock(target.Label).Branch).Value;
        Assert.Multiple(() =>
        {
            Assert.That(new[] { firstSource, secondSource }, Does.Contain(rewrittenReturn));
            Assert.That(optimizer.cfg.Edges.SelectMany(edge => edge.EdgeAssignments), Is.Empty);
        });
    }

    [Test]
    public void MergeStackSlots_MultipleAssignmentsOnOneEdgeKeepIndependentSetsDistinct()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel targetLabel = new(1);
        StackSlot firstSource = new(0, typeof(int), 0);
        StackSlot secondSource = new(1, typeof(string), 1);
        StackSlot firstTarget = new(0, typeof(int), 2);
        StackSlot secondTarget = new(1, typeof(string), 3);
        BasicBlock source = new(root.EntryLabel, [], root, new UnconditionalBranch(targetLabel));
        ILOp consume = new(new ILInstruction(OpCodes.Pop, null!, []), [firstTarget, secondTarget], typeof(void));
        BasicBlock target = new(targetLabel, [consume], root, new Return(Ret, new VoidOp()));
        Edge edge = new(source.Label, target.Label,
        [
            new AssignmentOp(firstTarget, firstSource),
            new AssignmentOp(secondTarget, secondSource),
        ]);
        ControlFlowGraph graph = new(root, [source, target], [edge], [], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new MergeStackSlots(optimizer).RunInternal();

        var rewrittenConsume = (ILOp)optimizer.cfg.GetBlock(target.Label).Ops.Single();
        Assert.Multiple(() =>
        {
            Assert.That(rewrittenConsume.Inputs[0], Is.SameAs(firstSource));
            Assert.That(rewrittenConsume.Inputs[1], Is.SameAs(secondSource));
            Assert.That(rewrittenConsume.Inputs[0], Is.Not.SameAs(rewrittenConsume.Inputs[1]));
            Assert.That(optimizer.cfg.Edges.Single().EdgeAssignments, Is.Empty);
        });
    }

    [Test]
    public void MergeStackSlots_RewritesSlotsReadByAssignmentsInsideBlocks()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel targetLabel = new(1);
        StackSlot sourceSlot = new(0, typeof(int), 0);
        StackSlot targetSlot = new(0, typeof(int), 1);
        Temporary temporary = new(typeof(int), 0);
        BasicBlock source = new(root.EntryLabel, [], root, new UnconditionalBranch(targetLabel));
        BasicBlock target = new(targetLabel, [new AssignmentOp(temporary, targetSlot)], root,
            new Return(Ret, new VoidOp()));
        Edge edge = new(source.Label, target.Label, [new AssignmentOp(targetSlot, sourceSlot)]);
        ControlFlowGraph graph = new(root, [source, target], [edge], [], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new MergeStackSlots(optimizer).RunInternal();

        AssignmentOp rewrittenAssignment = optimizer.cfg.GetBlock(target.Label).Ops.OfType<AssignmentOp>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(rewrittenAssignment.Output, Is.SameAs(temporary));
            Assert.That(rewrittenAssignment.Input, Is.SameAs(sourceSlot));
            Assert.That(optimizer.cfg.Edges.Single().EdgeAssignments, Is.Empty);
        });
    }

    [Test]
    public void MergeStackSlots_CatchEntryCopyIsRemovedAndCatchBodyUsesIncomingException()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel catchEntryLabel = new(1);
        BlockLabel catchBodyLabel = new(2);
        StackSlot incomingException = new(0, typeof(Exception), 0);
        StackSlot catchBodyException = new(0, typeof(Exception), 1);
        CatchRegion catchRegion = new(catchEntryLabel, root, incomingException);
        ExceptionGroup group = new([catchRegion]);
        ProtectedRegion protectedRegion = new(root.EntryLabel, root, group);
        BasicBlock protectedBlock = new(root.EntryLabel, [], protectedRegion,
            new Return(Ret, new VoidOp()));
        BasicBlock catchEntry = new(catchEntryLabel, [], catchRegion,
            new UnconditionalBranch(catchBodyLabel));
        BasicBlock catchBody = new(catchBodyLabel, [], catchRegion, new Throw(catchBodyException));
        Edge edge = new(catchEntry.Label, catchBody.Label,
            [new AssignmentOp(catchBodyException, incomingException)]);
        ControlFlowGraph graph = new(root, [protectedBlock, catchEntry, catchBody], [edge], [], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new MergeStackSlots(optimizer).RunInternal();

        ControlFlowGraph rewritten = optimizer.cfg;
        CatchRegion rewrittenCatch = rewritten.ExceptionGroups.Single().HandlerRegions.Cast<CatchRegion>().Single();
        var rewrittenThrow = (Throw)rewritten.GetBlock(catchBody.Label).Branch;
        Assert.Multiple(() =>
        {
            Assert.That(rewrittenCatch.IncomingException, Is.SameAs(incomingException));
            Assert.That(rewrittenThrow.Exception, Is.SameAs(incomingException));
            Assert.That(rewritten.GetEdge(catchEntry.Label, catchBody.Label).EdgeAssignments, Is.Empty);
        });
    }

    [Test]
    public void MergeStackSlots_Dup_PreservesTheExplicitCopyBetweenDistinctSlots()
    {
        ILGenerator il = PatchProcessor.CreateILGenerator();
        Label targetLabel = il.DefineLabel();
        List<CodeInstruction> instructions =
        [
            new CodeInstruction(OpCodes.Ldc_I4_1),
            new CodeInstruction(OpCodes.Dup),
            new CodeInstruction(OpCodes.Br, targetLabel),
            new CodeInstruction(OpCodes.Pop).WithLabels(targetLabel),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Ret),
        ];
        var optimizer = new global::Disharmony.Optimizer.Optimizer(VoidMethod, instructions, il, false);
        CreateControlFlowGraph generator = new(optimizer);
        generator.RunInternal();
        ControlFlowGraph graph = generator.ControlFlowGraph;
        BasicBlock source = graph.BasicBlocks.Single(block => block.Ops.OfType<AssignmentOp>()
            .Any(assignment => assignment.Input is StackSlot));
        AssignmentOp copyBeforeMerge = source.Ops.OfType<AssignmentOp>()
            .Single(assignment => assignment.Input is StackSlot);

        new MergeStackSlots(optimizer).RunInternal();
        graph = optimizer.cfg;

        AssignmentOp copyAfterMerge = graph.GetBlock(source.Label).Ops.OfType<AssignmentOp>()
            .Single(assignment => assignment.Input is StackSlot);
        BasicBlock target = graph.BasicBlocks.Single(block =>
            block.Ops.OfType<ILOp>().Count(operation => operation.IL.OpCode == OpCodes.Pop) == 2);
        ILOp[] pops = [.. target.Ops.OfType<ILOp>().Where(operation => operation.IL.OpCode == OpCodes.Pop)];
        Assert.Multiple(() =>
        {
            Assert.That(copyAfterMerge, Is.SameAs(copyBeforeMerge));
            Assert.That(copyAfterMerge.Input, Is.Not.EqualTo(copyAfterMerge.Output));
            Assert.That(pops[0].Inputs.Single(), Is.SameAs(copyAfterMerge.Output));
            Assert.That(pops[1].Inputs.Single(), Is.SameAs(copyAfterMerge.Input));
            Assert.That(graph.Edges.SelectMany(edge => edge.EdgeAssignments), Is.Empty);
        });
    }

    [Test]
    public void MergeStackSlots_RewritePreservesUnrelatedGraphAndInstructionState()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel targetLabel = new(1);
        CatchRegion catchRegion = new(new BlockLabel(-1), root, new StackSlot(0, typeof(Exception), 2));
        ExceptionGroup group = new([catchRegion]);
        ProtectedRegion protectedRegion = new(root.EntryLabel, root, group);
        StackSlot sourceSlot = new(0, typeof(int).MakeByRefType(), 0);
        StackSlot targetSlot = new(0, typeof(int).MakeByRefType(), 1);
        Temporary loadedValue = new(typeof(int), 0);
        Prefix[] prefixes = [new(OpCodes.Unaligned, (byte)1), new(OpCodes.Volatile, null)];
        ILInstruction instruction = new(OpCodes.Ldind_I4, null!, prefixes);
        ILOp load = new(instruction, [targetSlot], typeof(int));
        AssignmentOp loadAssignment = new(loadedValue, load);
        Return returnBranch = new(Ret, new VoidOp());
        BasicBlock source = new(root.EntryLabel, [], protectedRegion, new UnconditionalBranch(targetLabel));
        BasicBlock target = new(targetLabel, [loadAssignment], protectedRegion, returnBranch);
        Edge edge = new(source.Label, target.Label, [new AssignmentOp(targetSlot, sourceSlot)]);
        Argument argument = new(0, typeof(string));
        Local local = new(typeof(long), 0);
        List<Argument> arguments = [argument];
        List<Local> locals = [local];
        ControlFlowGraph graph = new(root, [source, target], [edge], arguments, locals);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new MergeStackSlots(optimizer).RunInternal();

        ControlFlowGraph rewritten = optimizer.cfg;
        BasicBlock rewrittenTarget = rewritten.GetBlock(target.Label);
        AssignmentOp rewrittenLoadAssignment = rewrittenTarget.Ops.OfType<AssignmentOp>().Single();
        var rewrittenLoad = (ILOp)rewrittenLoadAssignment.Input;
        Assert.Multiple(() =>
        {
            Assert.That(rewritten.RootRegion, Is.SameAs(root));
            Assert.That(rewritten.Arguments, Is.EqualTo(arguments));
            Assert.That(rewritten.Arguments[0], Is.SameAs(argument));
            Assert.That(rewritten.Locals, Is.EqualTo(locals));
            Assert.That(rewritten.Locals[0], Is.SameAs(local));
            Assert.That(rewrittenTarget.Label, Is.SameAs(target.Label));
            Assert.That(rewrittenTarget.Region, Is.SameAs(protectedRegion));
            Assert.That(rewritten.ExceptionGroups.Single(), Is.SameAs(group));
            Assert.That(rewrittenTarget.Branch, Is.SameAs(returnBranch));
            Assert.That(rewrittenLoad.IL, Is.SameAs(instruction));
            Assert.That(rewrittenLoad.IL.Prefixes, Is.SameAs(prefixes));
            Assert.That(rewrittenLoad.Inputs.Single(), Is.SameAs(sourceSlot));
            Assert.That(rewrittenLoadAssignment.Output, Is.SameAs(loadedValue));
            Assert.That(rewritten.Edges.Single().EdgeAssignments, Is.Empty);
        });
    }
}
