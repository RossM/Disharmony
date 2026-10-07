using Disharmony.Optimizer.Passes;

namespace Disharmony.Tests.Unit.Optimizer.Passes;

[TestFixture]
#if NETFRAMEWORK
[Timeout(1000)]
#endif
public sealed class DeduceTypesTests
{
    private static readonly MethodInfo ReturnIntMethod =
        typeof(ControlFlowGraphTargets).GetMethod(nameof(ControlFlowGraphTargets.ReturnInt))!;

    private static readonly ILInstruction Ret = new(OpCodes.Ret, null!, []);

    [Test]
    public void Constant_AssignmentAndReturnAreRewrittenWithDeducedType()
    {
        RootRegion root = new(new BlockLabel(0));
        StackSlot result = new(0, TypeLattice.Unknown, 0);
        ILOp constant = new(new ILInstruction(OpCodes.Ldc_I4_1, null!, []), [], TypeLattice.Unknown);
        BasicBlock block = new(root.EntryLabel, [new AssignmentOp(result, constant)], root,
            new Return(Ret, result));
        ControlFlowGraph graph = new(root, [block], [], [], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(
            ReturnIntMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new DeduceTypes(optimizer).RunInternal();

        BasicBlock rewrittenBlock = optimizer.cfg.GetBlock(block.Label);
        AssignmentOp rewrittenAssignment = rewrittenBlock.Ops.OfType<AssignmentOp>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(rewrittenAssignment.Input.Type, Is.EqualTo(typeof(int)));
            Assert.That(rewrittenAssignment.Output.Type, Is.EqualTo(typeof(int)));
            Assert.That(((Return)rewrittenBlock.Branch).Value, Is.EqualTo(rewrittenAssignment.Output));
        });
    }

    [Test]
    public void ArgumentLoad_UsesDeclaredArgumentType()
    {
        RootRegion root = new(new BlockLabel(0));
        StackSlot result = new(0, TypeLattice.Unknown, 0);
        ILOp load = new(new ILInstruction(OpCodes.Ldarg_0, null!, []), [], TypeLattice.Unknown);
        BasicBlock block = new(root.EntryLabel, [new AssignmentOp(result, load)], root,
            new Return(Ret, result));
        ControlFlowGraph graph = new(root, [block], [], [new Argument(0, typeof(int))], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(
            ReturnIntMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new DeduceTypes(optimizer).RunInternal();

        AssignmentOp rewrittenAssignment = optimizer.cfg.GetBlock(block.Label).Ops.OfType<AssignmentOp>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(rewrittenAssignment.Input.Type, Is.EqualTo(typeof(int)));
            Assert.That(rewrittenAssignment.Output.Type, Is.EqualTo(typeof(int)));
        });
    }

    [Test]
    public void LocalLoad_NormalizesDeclaredLocalType()
    {
        RootRegion root = new(new BlockLabel(0));
        StackSlot result = new(0, TypeLattice.Unknown, 0);
        ILOp load = new(new ILInstruction(OpCodes.Ldloc_0, null!, []), [], TypeLattice.Unknown);
        BasicBlock block = new(root.EntryLabel, [new AssignmentOp(result, load)], root,
            new Return(Ret, result));
        ControlFlowGraph graph = new(root, [block], [], [], [new Local(typeof(byte), 0)]);
        global::Disharmony.Optimizer.Optimizer optimizer = new(
            ReturnIntMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new DeduceTypes(optimizer).RunInternal();

        AssignmentOp rewrittenAssignment = optimizer.cfg.GetBlock(block.Label).Ops.OfType<AssignmentOp>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(rewrittenAssignment.Input.Type, Is.EqualTo(typeof(int)));
            Assert.That(rewrittenAssignment.Output.Type, Is.EqualTo(typeof(int)));
        });
    }

    [Test]
    public void IncomingEdges_MergeReferenceTypesAndRewriteEveryUse()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel secondLabel = new(1);
        BlockLabel targetLabel = new(2);
        StackSlot stringValue = new(0, typeof(string), 0);
        StackSlot classValue = new(0, typeof(OpcodeUtilitiesClass), 1);
        StackSlot mergedValue = new(0, TypeLattice.Unknown, 2);
        BasicBlock first = new(root.EntryLabel, [], root, new UnconditionalBranch(targetLabel));
        BasicBlock second = new(secondLabel, [], root, new UnconditionalBranch(targetLabel));
        BasicBlock target = new(targetLabel, [], root, new Return(Ret, mergedValue));
        ControlFlowGraph graph = new(root, [first, second, target],
        [
            new Edge(first.Label, target.Label, [new AssignmentOp(mergedValue, stringValue)]),
            new Edge(second.Label, target.Label, [new AssignmentOp(mergedValue, classValue)]),
        ], [], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(
            ReturnIntMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new DeduceTypes(optimizer).RunInternal();

        ControlFlowGraph rewritten = optimizer.cfg;
        Op rewrittenReturnValue = ((Return)rewritten.GetBlock(target.Label).Branch).Value;
        AssignmentOp[] rewrittenAssignments =
        [
            .. rewritten.IncomingEdges(target.Label)
                .SelectMany(edge => edge.EdgeAssignments),
        ];
        Assert.Multiple(() =>
        {
            Assert.That(rewrittenReturnValue.Type, Is.EqualTo(typeof(object)));
            Assert.That(rewrittenAssignments.Select(assignment => assignment.Output.Type),
                Is.All.EqualTo(typeof(object)));
            Assert.That(rewrittenAssignments.Select(assignment => assignment.Output),
                Is.All.SameAs(rewrittenReturnValue));
        });
    }

    [Test]
    public void EdgeThenArithmetic_ReachesFixedPointBeforeRewritingGraph()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel targetLabel = new(1);
        StackSlot sourceValue = new(0, TypeLattice.Unknown, 0);
        StackSlot targetValue = new(0, TypeLattice.Unknown, 1);
        StackSlot result = new(0, TypeLattice.Unknown, 2);
        ILOp constant = new(new ILInstruction(OpCodes.Ldc_I4_1, null!, []), [], TypeLattice.Unknown);
        ILOp negate = new(new ILInstruction(OpCodes.Neg, null!, []), [targetValue], TypeLattice.Unknown);
        BasicBlock source = new(root.EntryLabel, [new AssignmentOp(sourceValue, constant)], root,
            new UnconditionalBranch(targetLabel));
        BasicBlock target = new(targetLabel, [new AssignmentOp(result, negate)], root,
            new Return(Ret, result));
        ControlFlowGraph graph = new(root, [source, target],
            [new Edge(source.Label, target.Label, [new AssignmentOp(targetValue, sourceValue)])], [], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(
            ReturnIntMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new DeduceTypes(optimizer).RunInternal();

        BasicBlock rewrittenTarget = optimizer.cfg.GetBlock(target.Label);
        AssignmentOp rewrittenAssignment = rewrittenTarget.Ops.OfType<AssignmentOp>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(((ILOp)rewrittenAssignment.Input).Inputs.Single().Type, Is.EqualTo(typeof(int)));
            Assert.That(rewrittenAssignment.Input.Type, Is.EqualTo(typeof(int)));
            Assert.That(rewrittenAssignment.Output.Type, Is.EqualTo(typeof(int)));
            Assert.That(((Return)rewrittenTarget.Branch).Value.Type, Is.EqualTo(typeof(int)));
        });
    }

    [Test]
    public void Loop_BackEdgeTypesReachFixedPoint()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel loopLabel = new(1);
        BlockLabel exitLabel = new(2);
        StackSlot initialValue = new(0, TypeLattice.Unknown, 0);
        StackSlot loopValue = new(0, TypeLattice.Unknown, 1);
        StackSlot nextValue = new(0, TypeLattice.Unknown, 2);
        StackSlot exitValue = new(0, TypeLattice.Unknown, 3);
        StackSlot condition = new(0, typeof(int), 4);

        ILOp constant = new(new ILInstruction(OpCodes.Ldc_I4_1, null!, []), [], TypeLattice.Unknown);
        BasicBlock entry = new(root.EntryLabel, [new AssignmentOp(initialValue, constant)], root,
            new UnconditionalBranch(loopLabel));

        ILOp negate = new(new ILInstruction(OpCodes.Neg, null!, []), [loopValue], TypeLattice.Unknown);
        BasicBlock loop = new(loopLabel, [new AssignmentOp(nextValue, negate)], root,
            new ConditionalBranch(OpCodes.Brtrue, [condition], [exitLabel, loopLabel]));
        BasicBlock exit = new(exitLabel, [], root, new Return(Ret, exitValue));

        ControlFlowGraph graph = new(root, [entry, loop, exit],
        [
            new Edge(entry.Label, loop.Label, [new AssignmentOp(loopValue, initialValue)]),
            new Edge(loop.Label, exit.Label, [new AssignmentOp(exitValue, nextValue)]),
            new Edge(loop.Label, loop.Label, [new AssignmentOp(loopValue, nextValue)]),
        ], [], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(
            ReturnIntMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new DeduceTypes(optimizer).RunInternal();

        ControlFlowGraph rewritten = optimizer.cfg;
        BasicBlock rewrittenLoop = rewritten.GetBlock(loop.Label);
        AssignmentOp rewrittenLoopAssignment = rewrittenLoop.Ops.OfType<AssignmentOp>().Single();
        Edge rewrittenBackEdge = rewritten.GetEdge(loop.Label, loop.Label);
        Assert.Multiple(() =>
        {
            Assert.That(((ILOp)rewrittenLoopAssignment.Input).Inputs.Single().Type, Is.EqualTo(typeof(int)));
            Assert.That(rewrittenLoopAssignment.Input.Type, Is.EqualTo(typeof(int)));
            Assert.That(rewrittenLoopAssignment.Output.Type, Is.EqualTo(typeof(int)));
            Assert.That(rewrittenBackEdge.EdgeAssignments.Single().Input.Type, Is.EqualTo(typeof(int)));
            Assert.That(rewrittenBackEdge.EdgeAssignments.Single().Output.Type, Is.EqualTo(typeof(int)));
            Assert.That(((Return)rewritten.GetBlock(exit.Label).Branch).Value.Type, Is.EqualTo(typeof(int)));
        });
    }

    [Test]
    public void ConditionalBranch_UsesTypeDeducedByEarlierOperation()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel fallthroughLabel = new(1);
        BlockLabel takenLabel = new(2);
        StackSlot condition = new(0, TypeLattice.Unknown, 0);
        ILOp constant = new(new ILInstruction(OpCodes.Ldc_I4_1, null!, []), [], TypeLattice.Unknown);
        BasicBlock source = new(root.EntryLabel, [new AssignmentOp(condition, constant)], root,
            new ConditionalBranch(OpCodes.Brtrue, [condition], [fallthroughLabel, takenLabel]));
        BasicBlock fallthrough = new(fallthroughLabel, [], root, new Return(Ret, new VoidOp()));
        BasicBlock taken = new(takenLabel, [], root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [source, fallthrough, taken],
        [
            new Edge(source.Label, fallthrough.Label, []),
            new Edge(source.Label, taken.Label, []),
        ], [], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(
            ReturnIntMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new DeduceTypes(optimizer).RunInternal();

        BasicBlock rewrittenSource = optimizer.cfg.GetBlock(source.Label);
        AssignmentOp rewrittenAssignment = rewrittenSource.Ops.OfType<AssignmentOp>().Single();
        var rewrittenBranch = (ConditionalBranch)rewrittenSource.Branch;
        Assert.Multiple(() =>
        {
            Assert.That(rewrittenAssignment.Output.Type, Is.EqualTo(typeof(int)));
            Assert.That(rewrittenBranch.Inputs.Single(), Is.SameAs(rewrittenAssignment.Output));
            Assert.That(rewrittenBranch.Inputs.Single().Type, Is.EqualTo(typeof(int)));
        });
    }

    [Test]
    public void Throw_UsesTypeDeducedByEarlierOperation()
    {
        RootRegion root = new(new BlockLabel(0));
        StackSlot exception = new(0, TypeLattice.Unknown, 0);
        ILOp loadNull = new(new ILInstruction(OpCodes.Ldnull, null!, []), [], TypeLattice.Unknown);
        BasicBlock block = new(root.EntryLabel, [new AssignmentOp(exception, loadNull)], root,
            new Throw(exception));
        ControlFlowGraph graph = new(root, [block], [], [], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(
            ReturnIntMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new DeduceTypes(optimizer).RunInternal();

        BasicBlock rewrittenBlock = optimizer.cfg.GetBlock(block.Label);
        AssignmentOp rewrittenAssignment = rewrittenBlock.Ops.OfType<AssignmentOp>().Single();
        var rewrittenThrow = (Throw)rewrittenBlock.Branch;
        Assert.Multiple(() =>
        {
            Assert.That(rewrittenAssignment.Input.Type, Is.EqualTo(TypeLattice.Null));
            Assert.That(rewrittenAssignment.Output.Type, Is.EqualTo(TypeLattice.Null));
            Assert.That(rewrittenThrow.Exception, Is.SameAs(rewrittenAssignment.Output));
        });
    }

    [Test]
    public void IncomingEdges_NullAndReferenceMergeToTheReferenceType()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel secondLabel = new(1);
        BlockLabel targetLabel = new(2);
        StackSlot stringValue = new(0, typeof(string), 0);
        StackSlot nullValue = new(0, TypeLattice.Null, 1);
        StackSlot mergedValue = new(0, TypeLattice.Unknown, 2);
        BasicBlock first = new(root.EntryLabel, [], root, new UnconditionalBranch(targetLabel));
        BasicBlock second = new(secondLabel, [], root, new UnconditionalBranch(targetLabel));
        BasicBlock target = new(targetLabel, [], root, new Return(Ret, mergedValue));
        ControlFlowGraph graph = new(root, [first, second, target],
        [
            new Edge(first.Label, target.Label, [new AssignmentOp(mergedValue, stringValue)]),
            new Edge(second.Label, target.Label, [new AssignmentOp(mergedValue, nullValue)]),
        ], [], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(
            ReturnIntMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new DeduceTypes(optimizer).RunInternal();

        ControlFlowGraph rewritten = optimizer.cfg;
        AssignmentOp[] assignments =
        [
            .. rewritten.IncomingEdges(target.Label)
                .SelectMany(edge => edge.EdgeAssignments),
        ];
        Op returnValue = ((Return)rewritten.GetBlock(target.Label).Branch).Value;
        Assert.Multiple(() =>
        {
            Assert.That(assignments.Select(assignment => assignment.Output.Type),
                Is.All.EqualTo(typeof(string)));
            Assert.That(assignments.Select(assignment => assignment.Output), Is.All.SameAs(returnValue));
            Assert.That(returnValue.Type, Is.EqualTo(typeof(string)));
        });
    }

    [Test]
    public void UnreachableBlock_StillHasItsTypesDeduced()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel unreachableLabel = new(1);
        BasicBlock entry = new(root.EntryLabel, [], root, new Return(Ret, new VoidOp()));
        StackSlot result = new(0, TypeLattice.Unknown, 0);
        ILOp constant = new(new ILInstruction(OpCodes.Ldc_I8, 1L, []), [], TypeLattice.Unknown);
        BasicBlock unreachable = new(unreachableLabel, [new AssignmentOp(result, constant)], root,
            new Return(Ret, result));
        ControlFlowGraph graph = new(root, [entry, unreachable], [], [], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(
            ReturnIntMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new DeduceTypes(optimizer).RunInternal();

        BasicBlock rewritten = optimizer.cfg.GetBlock(unreachable.Label);
        AssignmentOp rewrittenAssignment = rewritten.Ops.OfType<AssignmentOp>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(rewrittenAssignment.Input.Type, Is.EqualTo(typeof(long)));
            Assert.That(rewrittenAssignment.Output.Type, Is.EqualTo(typeof(long)));
            Assert.That(((Return)rewritten.Branch).Value, Is.SameAs(rewrittenAssignment.Output));
        });
    }

    [Test]
    public void AssignmentToTemporary_DeducesInputWithoutChangingDestinationType()
    {
        RootRegion root = new(new BlockLabel(0));
        Temporary destination = new(typeof(object), 0);
        ILOp constant = new(new ILInstruction(OpCodes.Ldc_I4_1, null!, []), [], TypeLattice.Unknown);
        AssignmentOp assignment = new(destination, constant);
        BasicBlock block = new(root.EntryLabel, [assignment], root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [block], [], [], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(
            ReturnIntMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new DeduceTypes(optimizer).RunInternal();

        AssignmentOp rewritten = optimizer.cfg.GetBlock(block.Label).Ops.OfType<AssignmentOp>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(rewritten.Input.Type, Is.EqualTo(typeof(int)));
            Assert.That(rewritten.Output, Is.SameAs(destination));
            Assert.That(rewritten.Output.Type, Is.EqualTo(typeof(object)));
        });
    }

    [Test]
    public void TypeRewrite_PreservesUnrelatedGraphInstructionAndRegionState()
    {
        RootRegion root = new(new BlockLabel(0));
        StackSlot exception = new(0, typeof(Exception), 0);
        CatchRegion catchRegion = new(new BlockLabel(-1), root, exception);
        ExceptionGroup group = new([catchRegion]);
        ProtectedRegion protectedRegion = new(root.EntryLabel, root, group);
        StackSlot address = new(0, typeof(int).MakeByRefType(), 1);
        StackSlot result = new(0, TypeLattice.Unknown, 2);
        Prefix[] prefixes = [new(OpCodes.Unaligned, (byte)1), new(OpCodes.Volatile, null)];
        ILInstruction instruction = new(OpCodes.Ldind_I4, null!, prefixes);
        ILOp load = new(instruction, [address], TypeLattice.Unknown);
        BasicBlock block = new(root.EntryLabel, [new AssignmentOp(result, load)], protectedRegion,
            new Return(Ret, result));
        Argument argument = new(0, typeof(string));
        Local local = new(typeof(long), 0);
        List<Argument> arguments = [argument];
        List<Local> locals = [local];
        ControlFlowGraph graph = new(root, [block], [], arguments, locals);
        global::Disharmony.Optimizer.Optimizer optimizer = new(
            ReturnIntMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph
        };

        new DeduceTypes(optimizer).RunInternal();

        ControlFlowGraph rewritten = optimizer.cfg;
        BasicBlock rewrittenBlock = rewritten.GetBlock(block.Label);
        AssignmentOp rewrittenAssignment = rewrittenBlock.Ops.OfType<AssignmentOp>().Single();
        var rewrittenLoad = (ILOp)rewrittenAssignment.Input;
        var rewrittenReturn = (Return)rewrittenBlock.Branch;
        Assert.Multiple(() =>
        {
            Assert.That(rewritten.RootRegion, Is.SameAs(root));
            Assert.That(rewritten.Arguments, Is.EqualTo(arguments));
            Assert.That(rewritten.Arguments[0], Is.SameAs(argument));
            Assert.That(rewritten.Locals, Is.EqualTo(locals));
            Assert.That(rewritten.Locals[0], Is.SameAs(local));
            Assert.That(rewrittenBlock.Label, Is.SameAs(block.Label));
            Assert.That(rewrittenBlock.Region, Is.SameAs(protectedRegion));
            Assert.That(rewritten.ExceptionGroups.Single(), Is.SameAs(group));
            Assert.That(rewrittenLoad.IL, Is.SameAs(instruction));
            Assert.That(rewrittenLoad.IL.Prefixes, Is.SameAs(prefixes));
            Assert.That(rewrittenLoad.Inputs.Single().Type, Is.EqualTo(typeof(int).MakeByRefType()));
            Assert.That(((StackSlot)rewrittenLoad.Inputs.Single()).Id, Is.EqualTo(address.Id));
            Assert.That(((StackSlot)rewrittenAssignment.Output).Depth, Is.EqualTo(result.Depth));
            Assert.That(((StackSlot)rewrittenAssignment.Output).Id, Is.EqualTo(result.Id));
            Assert.That(rewrittenAssignment.Output.Type, Is.EqualTo(typeof(int)));
            Assert.That(rewrittenReturn.IL, Is.SameAs(Ret));
            Assert.That(rewrittenReturn.Value, Is.SameAs(rewrittenAssignment.Output));
        });
    }
}
