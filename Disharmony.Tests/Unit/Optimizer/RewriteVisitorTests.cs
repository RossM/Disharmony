namespace Disharmony.Tests.Unit.Optimizer;

[TestFixture]
public sealed class RewriteVisitorTests
{
    private static readonly ILInstruction Nop = new(OpCodes.Nop, null!, []);
    private static readonly ILInstruction Ret = new(OpCodes.Ret, null!, []);

    private sealed class RootRegionReplacingVisitor(RootRegion original, RootRegion replacement) : RewriteVisitor
    {
        protected override Region Visit(RootRegion region) => ReferenceEquals(region, original) ? replacement : region;
    }

    [Test]
    public void ReplaceVisitor_LeafOp_ReturnsConfiguredReplacement()
    {
        StackSlot original = new(0, typeof(int), 0);
        StackSlot replacement = new(0, typeof(int), 1);
        ReplaceVisitor visitor = new()
        {
            Replacements =
            {
                [original] = replacement,
            },
        };

        Assert.Multiple(() =>
        {
            Assert.That(visitor.Visit((Op)original), Is.SameAs(replacement));
            Assert.That(visitor.Visit((Op)replacement), Is.SameAs(replacement));
        });
    }

    [Test]
    public void Assignment_RewritesItsInputAndOutput()
    {
        StackSlot originalInput = new(0, typeof(int), 0);
        StackSlot replacementInput = new(0, typeof(int), 1);
        Temporary originalOutput = new(typeof(int), 0);
        Temporary replacementOutput = new(typeof(int), 1);
        AssignmentOp assignment = new(originalOutput, originalInput);
        ReplaceVisitor visitor = new()
        {
            Replacements =
            {
                [originalInput] = replacementInput,
                [originalOutput] = replacementOutput,
            },
        };

        var rewritten = (AssignmentOp)visitor.Visit((Op)assignment);

        Assert.Multiple(() =>
        {
            Assert.That(rewritten, Is.Not.SameAs(assignment));
            Assert.That(rewritten.Input, Is.SameAs(replacementInput));
            Assert.That(rewritten.Output, Is.SameAs(replacementOutput));
        });
    }

    [Test]
    public void ILOp_RewritesEachInput()
    {
        StackSlot original = new(0, typeof(int), 0);
        StackSlot replacement = new(0, typeof(int), 1);
        ILOp operation = new(new ILInstruction(OpCodes.Add, null!, []), [original, original], typeof(int));
        ReplaceVisitor visitor = new()
        {
            Replacements =
            {
                [original] = replacement,
            },
        };

        var rewritten = (ILOp)visitor.Visit((Op)operation);

        Assert.Multiple(() =>
        {
            Assert.That(rewritten, Is.Not.SameAs(operation));
            Assert.That(rewritten.Inputs, Is.EqualTo(new[] { replacement, replacement }));
            Assert.That(rewritten.IL, Is.SameAs(operation.IL));
            Assert.That(rewritten.Type, Is.EqualTo(typeof(int)));
        });
    }

    [Test]
    public void TerminalBranches_RewriteTheirValues()
    {
        StackSlot original = new(0, typeof(int), 0);
        StackSlot replacement = new(0, typeof(int), 1);
        ReplaceVisitor visitor = new()
        {
            Replacements =
            {
                [original] = replacement,
            },
        };

        var rewrittenThrow = (Throw)visitor.Visit((Branch)new Throw(original));
        var rewrittenReturn = (Return)visitor.Visit((Branch)new Return(Ret, original));
        var rewrittenJump = (Jump)visitor.Visit((Branch)new Jump(original));

        Assert.Multiple(() =>
        {
            Assert.That(rewrittenThrow.Exception, Is.SameAs(replacement));
            Assert.That(rewrittenReturn.Value, Is.SameAs(replacement));
            Assert.That(rewrittenJump.Value, Is.SameAs(replacement));
        });
    }

    [Test]
    public void ConditionalBranch_RewritesEachInput()
    {
        StackSlot original = new(0, typeof(int), 0);
        StackSlot replacement = new(0, typeof(int), 1);
        ConditionalBranch branch = new(OpCodes.Beq, [original, original], [new BlockLabel(1), new BlockLabel(2)]);
        ReplaceVisitor visitor = new()
        {
            Replacements =
            {
                [original] = replacement,
            },
        };

        var rewritten = (ConditionalBranch)visitor.Visit((Branch)branch);

        Assert.Multiple(() =>
        {
            Assert.That(rewritten, Is.Not.SameAs(branch));
            Assert.That(rewritten.Inputs, Is.EqualTo(new[] { replacement, replacement }));
            Assert.That(rewritten.Labels, Is.SameAs(branch.Labels));
        });
    }

    [Test]
    public void BasicBlock_RewritesOpsAndBranch()
    {
        RootRegion region = new(new BlockLabel(-1));
        StackSlot original = new(0, typeof(int), 0);
        StackSlot replacement = new(0, typeof(int), 1);
        AssignmentOp assignment = new(new Temporary(typeof(int), 0), original);
        BasicBlock block = new(region.EntryLabel, [assignment], region, new Return(Ret, original));
        ReplaceVisitor visitor = new()
        {
            Replacements =
            {
                [original] = replacement,
            },
        };

        var rewritten = (BasicBlock)(visitor).Visit(block);

        Assert.Multiple(() =>
        {
            Assert.That(rewritten, Is.Not.SameAs(block));
            Assert.That(((AssignmentOp)rewritten.Ops.Single()).Input, Is.SameAs(replacement));
            Assert.That(((Return)rewritten.Branch).Value, Is.SameAs(replacement));
            Assert.That(rewritten.Region, Is.SameAs(region));
        });
    }

    [Test]
    public void Edge_RewritesAssignmentsAndRemovesIdentityAssignments()
    {
        StackSlot original = new(0, typeof(int), 0);
        StackSlot replacement = new(0, typeof(int), 1);
        Temporary retainedOutput = new(typeof(int), 0);
        AssignmentOp becomesIdentity = new(replacement, original);
        AssignmentOp retained = new(retainedOutput, original);
        Edge edge = new(new BlockLabel(1), new BlockLabel(-1), [becomesIdentity, retained]);
        ReplaceVisitor visitor = new()
        {
            Replacements =
            {
                [original] = replacement,
            },
        };

        var rewritten = (Edge)(visitor).Visit(edge);

        Assert.Multiple(() =>
        {
            Assert.That(rewritten, Is.Not.SameAs(edge));
            Assert.That(rewritten.EdgeAssignments, Has.Count.EqualTo(1));
            Assert.That(rewritten.EdgeAssignments[0].Output, Is.SameAs(retainedOutput));
            Assert.That(rewritten.EdgeAssignments[0].Input, Is.SameAs(replacement));
        });
    }

    [Test]
    public void ExceptionGroup_RewritesCatchIncomingException()
    {
        RootRegion root = new(new BlockLabel(0));
        StackSlot original = new(0, typeof(Exception), 0);
        StackSlot replacement = new(0, typeof(Exception), 1);
        CatchRegion catchRegion = new(new BlockLabel(1), root, original);
        FinallyRegion finallyRegion = new(new BlockLabel(2), root);
        FaultRegion faultRegion = new(new BlockLabel(3), root);
        ExceptionGroup group = new([catchRegion, finallyRegion, faultRegion]);
        ReplaceVisitor visitor = new()
        {
            Replacements =
            {
                [original] = replacement,
            },
        };

        var rewritten = (ExceptionGroup)(visitor).Visit(group);

        Assert.Multiple(() =>
        {
            Assert.That(rewritten, Is.Not.SameAs(group));
            Assert.That(((CatchRegion)rewritten.HandlerRegions[0]).IncomingException, Is.SameAs(replacement));
            Assert.That(rewritten.HandlerRegions[1], Is.SameAs(finallyRegion));
            Assert.That(rewritten.HandlerRegions[2], Is.SameAs(faultRegion));
        });
    }

    [Test]
    public void ExceptionRegions_RewriteTheirParentRegion()
    {
        RootRegion originalParent = new(new BlockLabel(1));
        RootRegion replacementParent = new(new BlockLabel(2));
        ProtectedRegion protectedRegion = new(new BlockLabel(3), originalParent, new ExceptionGroup([]));
        CatchRegion catchRegion = new(new BlockLabel(4), originalParent,
            new StackSlot(0, typeof(Exception), 0));
        FinallyRegion finallyRegion = new(new BlockLabel(5), originalParent);
        FaultRegion faultRegion = new(new BlockLabel(6), originalParent);
        RootRegionReplacingVisitor visitor = new(originalParent, replacementParent);

        var rewrittenProtected = (ProtectedRegion)visitor.Visit((Region)protectedRegion);
        var rewrittenCatch = (CatchRegion)visitor.Visit((Region)catchRegion);
        var rewrittenFinally = (FinallyRegion)visitor.Visit((Region)finallyRegion);
        var rewrittenFault = (FaultRegion)visitor.Visit((Region)faultRegion);

        Assert.Multiple(() =>
        {
            Assert.That(rewrittenProtected.Parent, Is.SameAs(replacementParent));
            Assert.That(rewrittenCatch.Parent, Is.SameAs(replacementParent));
            Assert.That(rewrittenFinally.Parent, Is.SameAs(replacementParent));
            Assert.That(rewrittenFault.Parent, Is.SameAs(replacementParent));
        });
    }

    [Test]
    public void UnchangedCompositeNodes_PreserveTheirInstances()
    {
        RootRegion root = new(new BlockLabel(0));
        StackSlot value = new(0, typeof(int), 0);
        AssignmentOp assignment = new(new Temporary(typeof(int), 0), value);
        ILOp operation = new(Nop, [value], typeof(void));
        ConditionalBranch conditional = new(OpCodes.Brtrue, [value], [new BlockLabel(1), new BlockLabel(2)]);
        BasicBlock block = new(root.EntryLabel, [assignment, operation], root, conditional);
        Edge edge = new(block.Label, conditional.Labels[0], [assignment]);
        RewriteVisitor visitor = new();

        Assert.Multiple(() =>
        {
            Assert.That(visitor.Visit((Op)assignment), Is.SameAs(assignment));
            Assert.That(visitor.Visit((Op)operation), Is.SameAs(operation));
            Assert.That(visitor.Visit((Branch)conditional), Is.SameAs(conditional));
            Assert.That((visitor).Visit(block), Is.SameAs(block));
            Assert.That((visitor).Visit(edge), Is.SameAs(edge));
            Assert.That(visitor.Visit((Region)root), Is.SameAs(root));
            Assert.That(visitor.Visit((Branch)new UnconditionalBranch(new BlockLabel(1))),
                Is.TypeOf<UnconditionalBranch>());
            Assert.That(visitor.Visit((Branch)new Leave(new BlockLabel(2))), Is.TypeOf<Leave>());
            Assert.That(visitor.Visit((Branch)new Rethrow()), Is.TypeOf<Rethrow>());
        });
    }

    [Test]
    public void ControlFlowGraph_ReplacesChangedBlocksEdgesAndExceptionGroups()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel destination = new(1);
        StackSlot original = new(0, typeof(Exception), 0);
        StackSlot replacement = new(0, typeof(Exception), 1);
        CatchRegion catchRegion = new(new BlockLabel(2), root, original);
        ExceptionGroup group = new([catchRegion]);
        ProtectedRegion protectedRegion = new(root.EntryLabel, root, group);
        BasicBlock source = new(root.EntryLabel, [], protectedRegion,
            new UnconditionalBranch(destination));
        BasicBlock target = new(destination, [], root, new Return(Ret, original));
        Edge edge = new(source.Label, target.Label, [new AssignmentOp(replacement, original)]);
        ControlFlowGraph graph = new(root, [source, target], [edge], [], []);
        ReplaceVisitor visitor = new()
        {
            Replacements =
            {
                [original] = replacement,
            },
        };

        var rewritten = (ControlFlowGraph)visitor.Visit(graph);

        Assert.Multiple(() =>
        {
            Assert.That(rewritten.GetBlock(source.Label), Is.Not.SameAs(source));
            Assert.That(rewritten.GetBlock(target.Label), Is.Not.SameAs(target));
            Assert.That(((Return)rewritten.GetBlock(target.Label).Branch).Value, Is.SameAs(replacement));
            Assert.That(rewritten.GetEdge(source.Label, target.Label).EdgeAssignments, Is.Empty);
            Assert.That(((CatchRegion)rewritten.ExceptionGroups.Single().HandlerRegions.Single()).IncomingException,
                Is.SameAs(replacement));
        });
    }

    [Test]
    public void ControlFlowGraph_RewritesArgumentsAndLocalsWithoutLosingUnchangedMetadata()
    {
        RootRegion root = new(new BlockLabel(0));
        BasicBlock block = new(root.EntryLabel, [], root, new Return(Ret, new VoidOp()));
        Argument originalArgument = new(0, typeof(int));
        Argument replacementArgument = new(0, typeof(long));
        Argument unchangedArgument = new(1, typeof(string));
        Local originalLocal = new(typeof(int), 0);
        Local replacementLocal = new(typeof(long), 0);
        Local unchangedLocal = new(typeof(string), 1);
        ControlFlowGraph graph = new(root, [block], [],
            [originalArgument, unchangedArgument], [originalLocal, unchangedLocal]);
        ReplaceVisitor visitor = new()
        {
            Replacements =
            {
                [originalArgument] = replacementArgument,
                [originalLocal] = replacementLocal,
            },
        };

        var rewritten = (ControlFlowGraph)visitor.Visit(graph);

        Assert.Multiple(() =>
        {
            Assert.That(rewritten, Is.Not.SameAs(graph));
            Assert.That(rewritten.Arguments, Is.EqualTo(new[] { replacementArgument, unchangedArgument }));
            Assert.That(rewritten.Arguments[0], Is.SameAs(replacementArgument));
            Assert.That(rewritten.Arguments[1], Is.SameAs(unchangedArgument));
            Assert.That(rewritten.Locals, Is.EqualTo(new[] { replacementLocal, unchangedLocal }));
            Assert.That(rewritten.Locals[0], Is.SameAs(replacementLocal));
            Assert.That(rewritten.Locals[1], Is.SameAs(unchangedLocal));
            Assert.That(rewritten.RootRegion, Is.SameAs(root));
            Assert.That(rewritten.GetBlock(block.Label), Is.SameAs(block));
        });
    }
}
