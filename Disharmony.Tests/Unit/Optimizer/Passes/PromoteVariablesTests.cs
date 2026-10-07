using Disharmony.Optimizer.Passes;

namespace Disharmony.Tests.Unit.Optimizer.Passes;

[TestFixture]
#if NETFRAMEWORK
[Timeout(1000)]
#endif
public sealed class PromoteVariablesTests
{
    private enum ShortEnum : short
    {
        Value = 1,
    }

    private static readonly MethodInfo VoidMethod =
        typeof(ControlFlowGraphTargets).GetMethod(nameof(ControlFlowGraphTargets.ReturnVoid))!;

    private static readonly ILInstruction Ret = new(OpCodes.Ret, null!, []);
    private static readonly ILInstruction Endfinally = new(OpCodes.Endfinally, null!, []);

    [Test]
    public void NonEscapingArgumentAndLocal_LoadsAndStoresBecomeDirectAccesses()
    {
        RootRegion root = new(new BlockLabel(0));
        Argument argument = new(0, typeof(int));
        Local local = new(typeof(string), 0);
        StackSlot argumentLoadResult = new(0, typeof(int), 0);
        StackSlot argumentStoreInput = new(0, typeof(int), 1);
        StackSlot localLoadResult = new(0, typeof(string), 2);
        StackSlot localStoreInput = new(0, typeof(string), 3);
        BasicBlock block = new(root.EntryLabel,
        [
            new AssignmentOp(argumentLoadResult, new ILOp(new ILInstruction(OpCodes.Ldarg_0, null!, []), [], typeof(int))),
            new ILOp(new ILInstruction(OpCodes.Starg_S, 0, []), [argumentStoreInput], typeof(void)),
            new AssignmentOp(localLoadResult, new ILOp(new ILInstruction(OpCodes.Ldloc_0, null!, []), [], typeof(string))),
            new ILOp(new ILInstruction(OpCodes.Stloc_0, null!, []), [localStoreInput], typeof(void)),
        ], root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [block], [], [argument], [local]);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph,
        };

        new PromoteVariables(optimizer).RunInternal();

        IReadOnlyList<Op> rewritten = optimizer.cfg.GetBlock(block.Label).Ops;
        Assert.Multiple(() =>
        {
            Assert.That(((AssignmentOp)rewritten[0]).Output, Is.SameAs(argumentLoadResult));
            Assert.That(((AssignmentOp)rewritten[0]).Input, Is.SameAs(argument));
            Assert.That(((AssignmentOp)rewritten[1]).Output, Is.SameAs(argument));
            Assert.That(((AssignmentOp)rewritten[1]).Input, Is.SameAs(argumentStoreInput));
            Assert.That(((AssignmentOp)rewritten[2]).Output, Is.SameAs(localLoadResult));
            Assert.That(((AssignmentOp)rewritten[2]).Input, Is.SameAs(local));
            Assert.That(((AssignmentOp)rewritten[3]).Output, Is.SameAs(local));
            Assert.That(((AssignmentOp)rewritten[3]).Input, Is.SameAs(localStoreInput));
        });
    }

    [Test]
    public void LocalBuilderOperands_LoadAndStoreBecomeDirectAccesses()
    {
        ILGenerator generator = PatchProcessor.CreateILGenerator();
        LocalBuilder builder = generator.DeclareLocal(typeof(string));
        RootRegion root = new(new BlockLabel(0));
        Local local = new(builder);
        StackSlot loadResult = new(0, typeof(string), 0);
        StackSlot storeInput = new(0, typeof(string), 1);
        BasicBlock block = new(root.EntryLabel,
        [
            new AssignmentOp(loadResult,
                new ILOp(new ILInstruction(OpCodes.Ldloc_S, builder, []), [], typeof(string))),
            new ILOp(new ILInstruction(OpCodes.Stloc_S, builder, []), [storeInput], typeof(void)),
        ], root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [block], [], [], [local]);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], generator, false) { cfg = graph };

        new PromoteVariables(optimizer).RunInternal();

        IReadOnlyList<Op> rewritten = optimizer.cfg.GetBlock(block.Label).Ops;
        Assert.Multiple(() =>
        {
            Assert.That(((AssignmentOp)rewritten[0]).Input, Is.SameAs(local));
            Assert.That(((AssignmentOp)rewritten[1]).Output, Is.SameAs(local));
            Assert.That(((AssignmentOp)rewritten[1]).Input, Is.SameAs(storeInput));
        });
    }

    [Test]
    public void StorageTypesRequiringConversion_LoadsAndStoresInsertConversions()
    {
        RootRegion root = new(new BlockLabel(0));
        Local[] locals =
        [
            new(typeof(bool), 0),
            new(typeof(byte), 1),
            new(typeof(sbyte), 2),
            new(typeof(char), 3),
            new(typeof(short), 4),
            new(typeof(ushort), 5),
            new(typeof(float), 6),
            new(typeof(double), 7),
            new(typeof(ShortEnum), 8),
        ];
        Type[] stackTypes =
        [
            typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int),
            typeof(double), typeof(double), typeof(int),
        ];
        List<StackSlot> loadResults = [];
        List<StackSlot> storeInputs = [];
        List<Op> operations = [];
        for (int i = 0; i < locals.Length; i++)
        {
            StackSlot loadResult = new(0, stackTypes[i], i * 2);
            StackSlot storeInput = new(0, stackTypes[i], i * 2 + 1);
            loadResults.Add(loadResult);
            storeInputs.Add(storeInput);
            operations.Add(new AssignmentOp(loadResult,
                new ILOp(new ILInstruction(OpCodes.Ldloc, i, []), [], stackTypes[i])));
            operations.Add(new ILOp(new ILInstruction(OpCodes.Stloc, i, []), [storeInput], typeof(void)));
        }
        BasicBlock block = new(root.EntryLabel, operations, root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [block], [], [], locals);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph,
        };

        new PromoteVariables(optimizer).RunInternal();

        IReadOnlyList<Op> rewritten = optimizer.cfg.GetBlock(block.Label).Ops;
        Assert.Multiple(() =>
        {
            for (int i = 0; i < locals.Length; i++)
            {
                var loadAssignment = (AssignmentOp)rewritten[i * 2];
                var loadConversion = (ConversionOp)loadAssignment.Input;
                Assert.That(loadAssignment.Output, Is.SameAs(loadResults[i]), $"load output for {locals[i].Type}");
                Assert.That(loadConversion.Input, Is.SameAs(locals[i]), $"load input for {locals[i].Type}");
                Assert.That(loadConversion.Type, Is.EqualTo(stackTypes[i]), $"load type for {locals[i].Type}");

                var storeAssignment = (AssignmentOp)rewritten[i * 2 + 1];
                var storeConversion = (ConversionOp)storeAssignment.Input;
                Assert.That(storeAssignment.Output, Is.SameAs(locals[i]), $"store output for {locals[i].Type}");
                Assert.That(storeConversion.Input, Is.SameAs(storeInputs[i]), $"store input for {locals[i].Type}");
                Assert.That(storeConversion.Type, Is.EqualTo(locals[i].Type), $"store type for {locals[i].Type}");
            }
        });
    }

    [Test]
    public void StorageTypesNotRequiringConversion_LoadsAndStoresRemainDirect()
    {
        RootRegion root = new(new BlockLabel(0));
        Local[] locals =
        [
            new(typeof(int), 0),
            new(typeof(uint), 1),
            new(typeof(long), 2),
            new(typeof(ulong), 3),
            new(typeof(IntPtr), 4),
            new(typeof(UIntPtr), 5),
            new(typeof(string), 6),
            new(typeof(DateTime), 7),
            new(typeof(short).MakeByRefType(), 8),
            new(typeof(short?), 9),
            new(typeof(DayOfWeek), 10),
        ];
        List<StackSlot> loadResults = [];
        List<StackSlot> storeInputs = [];
        List<Op> operations = [];
        for (int i = 0; i < locals.Length; i++)
        {
            StackSlot loadResult = new(0, locals[i].Type, i * 2);
            StackSlot storeInput = new(0, locals[i].Type, i * 2 + 1);
            loadResults.Add(loadResult);
            storeInputs.Add(storeInput);
            operations.Add(new AssignmentOp(loadResult,
                new ILOp(new ILInstruction(OpCodes.Ldloc, i, []), [], locals[i].Type)));
            operations.Add(new ILOp(new ILInstruction(OpCodes.Stloc, i, []), [storeInput], typeof(void)));
        }
        BasicBlock block = new(root.EntryLabel, operations, root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [block], [], [], locals);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph,
        };

        new PromoteVariables(optimizer).RunInternal();

        IReadOnlyList<Op> rewritten = optimizer.cfg.GetBlock(block.Label).Ops;
        Assert.Multiple(() =>
        {
            for (int i = 0; i < locals.Length; i++)
            {
                var loadAssignment = (AssignmentOp)rewritten[i * 2];
                var storeAssignment = (AssignmentOp)rewritten[i * 2 + 1];
                Assert.That(loadAssignment.Output, Is.SameAs(loadResults[i]), $"load output for {locals[i].Type}");
                Assert.That(loadAssignment.Input, Is.SameAs(locals[i]), $"load input for {locals[i].Type}");
                Assert.That(storeAssignment.Output, Is.SameAs(locals[i]), $"store output for {locals[i].Type}");
                Assert.That(storeAssignment.Input, Is.SameAs(storeInputs[i]), $"store input for {locals[i].Type}");
            }
        });
    }

    [Test]
    public void ArgumentStorageTypeRequiringConversion_LoadAndStoreInsertConversions()
    {
        RootRegion root = new(new BlockLabel(0));
        Argument argument = new(0, typeof(byte));
        StackSlot loadResult = new(0, typeof(int), 0);
        StackSlot storeInput = new(0, typeof(int), 1);
        BasicBlock block = new(root.EntryLabel,
        [
            new AssignmentOp(loadResult,
                new ILOp(new ILInstruction(OpCodes.Ldarg_0, null!, []), [], typeof(int))),
            new ILOp(new ILInstruction(OpCodes.Starg_S, 0, []), [storeInput], typeof(void)),
        ], root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [block], [], [argument], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph,
        };

        new PromoteVariables(optimizer).RunInternal();

        IReadOnlyList<Op> rewritten = optimizer.cfg.GetBlock(block.Label).Ops;
        var loadAssignment = (AssignmentOp)rewritten[0];
        var loadConversion = (ConversionOp)loadAssignment.Input;
        var storeAssignment = (AssignmentOp)rewritten[1];
        var storeConversion = (ConversionOp)storeAssignment.Input;
        Assert.Multiple(() =>
        {
            Assert.That(loadConversion.Input, Is.SameAs(argument));
            Assert.That(loadConversion.Type, Is.EqualTo(typeof(int)));
            Assert.That(storeAssignment.Output, Is.SameAs(argument));
            Assert.That(storeConversion.Input, Is.SameAs(storeInput));
            Assert.That(storeConversion.Type, Is.EqualTo(typeof(byte)));
        });
    }

    [Test]
    public void Store_NestedPromotableInput_IsRewrittenBeforeAssignment()
    {
        RootRegion root = new(new BlockLabel(0));
        Argument argument = new(0, typeof(int));
        Local local = new(typeof(int), 0);
        ILOp loadArgument = new(new ILInstruction(OpCodes.Ldarg_0, null!, []), [], typeof(int));
        ILOp storeLocal = new(new ILInstruction(OpCodes.Stloc_0, null!, []), [loadArgument], typeof(void));
        BasicBlock block = new(root.EntryLabel, [storeLocal], root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [block], [], [argument], [local]);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph,
        };

        new PromoteVariables(optimizer).RunInternal();

        var assignment = (AssignmentOp)optimizer.cfg.GetBlock(block.Label).Ops.Single();
        Assert.Multiple(() =>
        {
            Assert.That(assignment.Output, Is.SameAs(local));
            Assert.That(assignment.Input, Is.SameAs(argument));
        });
    }

    [Test]
    public void Store_NestedPromotableInput_IsRewrittenBeforeInsertedConversion()
    {
        RootRegion root = new(new BlockLabel(0));
        Argument argument = new(0, typeof(int));
        Local local = new(typeof(byte), 0);
        ILOp loadArgument = new(new ILInstruction(OpCodes.Ldarg_0, null!, []), [], typeof(int));
        ILOp storeLocal = new(new ILInstruction(OpCodes.Stloc_0, null!, []), [loadArgument], typeof(void));
        BasicBlock block = new(root.EntryLabel, [storeLocal], root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [block], [], [argument], [local]);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph,
        };

        new PromoteVariables(optimizer).RunInternal();

        var assignment = (AssignmentOp)optimizer.cfg.GetBlock(block.Label).Ops.Single();
        var conversion = (ConversionOp)assignment.Input;
        Assert.Multiple(() =>
        {
            Assert.That(assignment.Output, Is.SameAs(local));
            Assert.That(conversion.Input, Is.SameAs(argument));
            Assert.That(conversion.Type, Is.EqualTo(typeof(byte)));
        });
    }

    [Test]
    public void Local_AddressTaken_RetainsEveryMemoryAccessWithoutConversions()
    {
        RootRegion root = new(new BlockLabel(0));
        Local local = new(typeof(byte), 0);
        StackSlot addressResult = new(0, typeof(byte).MakeByRefType(), 0);
        StackSlot loadResult = new(0, typeof(int), 1);
        StackSlot storeInput = new(0, typeof(int), 2);
        ILOp loadAddress = new(new ILInstruction(OpCodes.Ldloca_S, 0, []), [], typeof(byte).MakeByRefType());
        ILOp load = new(new ILInstruction(OpCodes.Ldloc_0, null!, []), [], typeof(int));
        ILOp store = new(new ILInstruction(OpCodes.Stloc_0, null!, []), [storeInput], typeof(void));
        BasicBlock block = new(root.EntryLabel,
        [
            new AssignmentOp(addressResult, loadAddress),
            new AssignmentOp(loadResult, load),
            store,
        ], root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [block], [], [], [local]);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph,
        };

        new PromoteVariables(optimizer).RunInternal();

        IReadOnlyList<Op> rewritten = optimizer.cfg.GetBlock(block.Label).Ops;
        Assert.Multiple(() =>
        {
            Assert.That(((AssignmentOp)rewritten[0]).Input, Is.SameAs(loadAddress));
            Assert.That(((AssignmentOp)rewritten[1]).Input, Is.SameAs(load));
            Assert.That(rewritten[2], Is.SameAs(store));
            Assert.That(rewritten.SelectMany(op => op is AssignmentOp assignment ? new[] { assignment.Input } : [op]),
                Has.None.TypeOf<ConversionOp>());
        });
    }

    [Test]
    public void Argument_AddressTaken_RetainsEveryMemoryAccess()
    {
        RootRegion root = new(new BlockLabel(0));
        Argument argument = new(0, typeof(int));
        StackSlot addressResult = new(0, typeof(int).MakeByRefType(), 0);
        StackSlot loadResult = new(0, typeof(int), 1);
        StackSlot storeInput = new(0, typeof(int), 2);
        ILOp loadAddress = new(new ILInstruction(OpCodes.Ldarga_S, 0, []), [], typeof(int).MakeByRefType());
        ILOp load = new(new ILInstruction(OpCodes.Ldarg_0, null!, []), [], typeof(int));
        ILOp store = new(new ILInstruction(OpCodes.Starg_S, 0, []), [storeInput], typeof(void));
        BasicBlock block = new(root.EntryLabel,
        [
            new AssignmentOp(addressResult, loadAddress),
            new AssignmentOp(loadResult, load),
            store,
        ], root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [block], [], [argument], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph,
        };

        new PromoteVariables(optimizer).RunInternal();

        IReadOnlyList<Op> rewritten = optimizer.cfg.GetBlock(block.Label).Ops;
        Assert.Multiple(() =>
        {
            Assert.That(((AssignmentOp)rewritten[0]).Input, Is.SameAs(loadAddress));
            Assert.That(((AssignmentOp)rewritten[1]).Input, Is.SameAs(load));
            Assert.That(rewritten[2], Is.SameAs(store));
        });
    }

    [Test]
    public void Catch_LoadingLocal_MakesVariableEscapeGlobally()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel catchEntryLabel = new(1);
        BlockLabel catchBodyLabel = new(2);
        BlockLabel exitLabel = new(3);
        Local local = new(typeof(int), 0);
        StackSlot storeInput = new(0, typeof(int), 0);
        StackSlot loadResult = new(0, typeof(int), 1);
        StackSlot incomingException = new(0, typeof(Exception), 2);
        CatchRegion catchRegion = new(catchEntryLabel, root, incomingException);
        ExceptionGroup group = new([catchRegion]);
        ProtectedRegion protectedRegion = new(root.EntryLabel, root, group);
        ILOp store = new(new ILInstruction(OpCodes.Stloc_0, null!, []), [storeInput], typeof(void));
        ILOp load = new(new ILInstruction(OpCodes.Ldloc_0, null!, []), [], typeof(int));
        BasicBlock protectedBlock = new(root.EntryLabel, [store], protectedRegion, new Leave(exitLabel));
        BasicBlock catchEntry = new(catchEntryLabel, [], catchRegion, new UnconditionalBranch(catchBodyLabel));
        BasicBlock catchBody = new(catchBodyLabel, [new AssignmentOp(loadResult, load)], catchRegion, new Leave(exitLabel));
        BasicBlock exit = new(exitLabel, [], root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [protectedBlock, catchEntry, catchBody, exit],
        [
            new Edge(protectedBlock.Label, exit.Label, []),
            new Edge(catchEntry.Label, catchBody.Label, []),
            new Edge(catchBody.Label, exit.Label, []),
        ], [], [local]);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph,
        };

        new PromoteVariables(optimizer).RunInternal();

        Assert.Multiple(() =>
        {
            Assert.That(optimizer.cfg.GetBlock(protectedBlock.Label).Ops.Single(), Is.SameAs(store));
            Assert.That(((AssignmentOp)optimizer.cfg.GetBlock(catchBody.Label).Ops.Single()).Input, Is.SameAs(load));
        });
    }

    [Test]
    public void Finally_LoadingArgument_MakesVariableEscapeGlobally()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel finallyEntryLabel = new(1);
        BlockLabel finallyBodyLabel = new(2);
        BlockLabel exitLabel = new(3);
        Argument argument = new(0, typeof(int));
        StackSlot storeInput = new(0, typeof(int), 0);
        StackSlot loadResult = new(0, typeof(int), 1);
        FinallyRegion finallyRegion = new(finallyEntryLabel, root);
        ExceptionGroup group = new([finallyRegion]);
        ProtectedRegion protectedRegion = new(root.EntryLabel, root, group);
        ILOp store = new(new ILInstruction(OpCodes.Starg_S, 0, []), [storeInput], typeof(void));
        ILOp load = new(new ILInstruction(OpCodes.Ldarg_0, null!, []), [], typeof(int));
        BasicBlock protectedBlock = new(root.EntryLabel, [store], protectedRegion, new Leave(exitLabel));
        BasicBlock finallyEntry = new(finallyEntryLabel, [], finallyRegion, new UnconditionalBranch(finallyBodyLabel));
        BasicBlock finallyBody = new(finallyBodyLabel, [new AssignmentOp(loadResult, load)], finallyRegion,
            new Return(Endfinally, new VoidOp()));
        BasicBlock exit = new(exitLabel, [], root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [protectedBlock, finallyEntry, finallyBody, exit],
        [
            new Edge(protectedBlock.Label, exit.Label, []),
            new Edge(finallyEntry.Label, finallyBody.Label, []),
        ], [argument], []);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph,
        };

        new PromoteVariables(optimizer).RunInternal();

        Assert.Multiple(() =>
        {
            Assert.That(optimizer.cfg.GetBlock(protectedBlock.Label).Ops.Single(), Is.SameAs(store));
            Assert.That(((AssignmentOp)optimizer.cfg.GetBlock(finallyBody.Label).Ops.Single()).Input, Is.SameAs(load));
        });
    }

    [Test]
    public void Fault_LoadingLocal_MakesVariableEscapeGlobally()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel faultEntryLabel = new(1);
        BlockLabel faultBodyLabel = new(2);
        Local local = new(typeof(int), 0);
        StackSlot storeInput = new(0, typeof(int), 0);
        StackSlot loadResult = new(0, typeof(int), 1);
        FaultRegion faultRegion = new(faultEntryLabel, root);
        ExceptionGroup group = new([faultRegion]);
        ProtectedRegion protectedRegion = new(root.EntryLabel, root, group);
        ILOp store = new(new ILInstruction(OpCodes.Stloc_0, null!, []), [storeInput], typeof(void));
        ILOp load = new(new ILInstruction(OpCodes.Ldloc_0, null!, []), [], typeof(int));
        BasicBlock protectedBlock = new(root.EntryLabel, [store], protectedRegion,
            new Throw(new ILOp(new ILInstruction(OpCodes.Ldnull, null!, []), [], TypeLattice.Null)));
        BasicBlock faultEntry = new(faultEntryLabel, [], faultRegion, new UnconditionalBranch(faultBodyLabel));
        BasicBlock faultBody = new(faultBodyLabel, [new AssignmentOp(loadResult, load)], faultRegion,
            new Return(Endfinally, new VoidOp()));
        ControlFlowGraph graph = new(root, [protectedBlock, faultEntry, faultBody],
            [new Edge(faultEntry.Label, faultBody.Label, [])], [], [local]);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph,
        };

        new PromoteVariables(optimizer).RunInternal();

        Assert.Multiple(() =>
        {
            Assert.That(optimizer.cfg.GetBlock(protectedBlock.Label).Ops.Single(), Is.SameAs(store));
            Assert.That(((AssignmentOp)optimizer.cfg.GetBlock(faultBody.Label).Ops.Single()).Input, Is.SameAs(load));
        });
    }

    [Test]
    public void TryBlock_LoadingLocal_DoesNotMakeVariableEscape()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel finallyEntryLabel = new(1);
        BlockLabel exitLabel = new(2);
        Local local = new(typeof(int), 0);
        StackSlot loadResult = new(0, typeof(int), 0);
        StackSlot storeInput = new(0, typeof(int), 1);
        FinallyRegion finallyRegion = new(finallyEntryLabel, root);
        ExceptionGroup group = new([finallyRegion]);
        ProtectedRegion protectedRegion = new(root.EntryLabel, root, group);
        ILOp load = new(new ILInstruction(OpCodes.Ldloc_0, null!, []), [], typeof(int));
        ILOp store = new(new ILInstruction(OpCodes.Stloc_0, null!, []), [storeInput], typeof(void));
        BasicBlock protectedBlock = new(root.EntryLabel,
            [new AssignmentOp(loadResult, load), store], protectedRegion, new Leave(exitLabel));
        BasicBlock finallyEntry = new(finallyEntryLabel, [], finallyRegion, new Return(Endfinally, new VoidOp()));
        BasicBlock exit = new(exitLabel, [], root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [protectedBlock, finallyEntry, exit],
            [new Edge(protectedBlock.Label, exit.Label, [])], [], [local]);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph,
        };

        new PromoteVariables(optimizer).RunInternal();

        IReadOnlyList<Op> rewritten = optimizer.cfg.GetBlock(protectedBlock.Label).Ops;
        Assert.Multiple(() =>
        {
            Assert.That(((AssignmentOp)rewritten[0]).Input, Is.SameAs(local));
            Assert.That(((AssignmentOp)rewritten[1]).Output, Is.SameAs(local));
            Assert.That(((AssignmentOp)rewritten[1]).Input, Is.SameAs(storeInput));
        });
    }

    [Test]
    public void NestedTryWithinCatch_LoadingLocal_MakesVariableEscape()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel catchEntryLabel = new(1);
        BlockLabel innerTryLabel = new(2);
        BlockLabel innerFinallyEntryLabel = new(3);
        BlockLabel innerFinallyBodyLabel = new(4);
        BlockLabel exitLabel = new(5);
        Local local = new(typeof(int), 0);
        StackSlot incomingException = new(0, typeof(Exception), 0);
        StackSlot loadResult = new(0, typeof(int), 1);
        CatchRegion catchRegion = new(catchEntryLabel, root, incomingException);
        ExceptionGroup outerGroup = new([catchRegion]);
        ProtectedRegion outerProtectedRegion = new(root.EntryLabel, root, outerGroup);
        FinallyRegion innerFinallyRegion = new(innerFinallyEntryLabel, catchRegion);
        ExceptionGroup innerGroup = new([innerFinallyRegion]);
        ProtectedRegion innerProtectedRegion = new(innerTryLabel, catchRegion, innerGroup);
        ILOp load = new(new ILInstruction(OpCodes.Ldloc_0, null!, []), [], typeof(int));
        BasicBlock outerProtected = new(root.EntryLabel, [], outerProtectedRegion, new Leave(exitLabel));
        BasicBlock catchEntry = new(catchEntryLabel, [], catchRegion, new UnconditionalBranch(innerTryLabel));
        BasicBlock innerTry = new(innerTryLabel, [new AssignmentOp(loadResult, load)], innerProtectedRegion,
            new Leave(exitLabel));
        BasicBlock innerFinallyEntry = new(innerFinallyEntryLabel, [], innerFinallyRegion,
            new UnconditionalBranch(innerFinallyBodyLabel));
        BasicBlock innerFinallyBody = new(innerFinallyBodyLabel, [], innerFinallyRegion,
            new Return(Endfinally, new VoidOp()));
        BasicBlock exit = new(exitLabel, [], root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root,
            [outerProtected, catchEntry, innerTry, innerFinallyEntry, innerFinallyBody, exit],
        [
            new Edge(outerProtected.Label, exit.Label, []),
            new Edge(catchEntry.Label, innerTry.Label, []),
            new Edge(innerTry.Label, exit.Label, []),
            new Edge(innerFinallyEntry.Label, innerFinallyBody.Label, []),
        ], [], [local]);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph,
        };

        new PromoteVariables(optimizer).RunInternal();

        Assert.That(((AssignmentOp)optimizer.cfg.GetBlock(innerTry.Label).Ops.Single()).Input, Is.SameAs(load));
    }

    [Test]
    public void Handler_StoringWithoutLoading_DoesNotMakeVariableEscape()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel catchEntryLabel = new(1);
        BlockLabel catchBodyLabel = new(2);
        BlockLabel exitLabel = new(3);
        Local local = new(typeof(int), 0);
        StackSlot storeInput = new(0, typeof(int), 0);
        StackSlot incomingException = new(0, typeof(Exception), 1);
        CatchRegion catchRegion = new(catchEntryLabel, root, incomingException);
        ExceptionGroup group = new([catchRegion]);
        ProtectedRegion protectedRegion = new(root.EntryLabel, root, group);
        ILOp store = new(new ILInstruction(OpCodes.Stloc_0, null!, []), [storeInput], typeof(void));
        BasicBlock protectedBlock = new(root.EntryLabel, [], protectedRegion, new Leave(exitLabel));
        BasicBlock catchEntry = new(catchEntryLabel, [], catchRegion, new UnconditionalBranch(catchBodyLabel));
        BasicBlock catchBody = new(catchBodyLabel, [store], catchRegion, new Leave(exitLabel));
        BasicBlock exit = new(exitLabel, [], root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [protectedBlock, catchEntry, catchBody, exit],
        [
            new Edge(protectedBlock.Label, exit.Label, []),
            new Edge(catchEntry.Label, catchBody.Label, []),
            new Edge(catchBody.Label, exit.Label, []),
        ], [], [local]);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph,
        };

        new PromoteVariables(optimizer).RunInternal();

        var assignment = (AssignmentOp)optimizer.cfg.GetBlock(catchBody.Label).Ops.Single();
        Assert.Multiple(() =>
        {
            Assert.That(assignment.Output, Is.SameAs(local));
            Assert.That(assignment.Input, Is.SameAs(storeInput));
        });
    }

    [Test]
    public void HandlerDepth_IsRestoredBeforeVisitingAnUnrelatedBlock()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel finallyEntryLabel = new(1);
        BlockLabel finallyBodyLabel = new(2);
        BlockLabel ordinaryLabel = new(3);
        Local escapingLocal = new(typeof(int), 0);
        Local ordinaryLocal = new(typeof(int), 1);
        StackSlot handlerLoadResult = new(0, typeof(int), 0);
        StackSlot ordinaryLoadResult = new(0, typeof(int), 1);
        FinallyRegion finallyRegion = new(finallyEntryLabel, root);
        ExceptionGroup group = new([finallyRegion]);
        ProtectedRegion protectedRegion = new(root.EntryLabel, root, group);
        ILOp handlerLoad = new(new ILInstruction(OpCodes.Ldloc_0, null!, []), [], typeof(int));
        ILOp ordinaryLoad = new(new ILInstruction(OpCodes.Ldloc_1, null!, []), [], typeof(int));
        BasicBlock protectedBlock = new(root.EntryLabel, [], protectedRegion, new Return(Ret, new VoidOp()));
        BasicBlock finallyEntry = new(finallyEntryLabel, [], finallyRegion, new UnconditionalBranch(finallyBodyLabel));
        BasicBlock finallyBody = new(finallyBodyLabel, [new AssignmentOp(handlerLoadResult, handlerLoad)], finallyRegion,
            new Return(Endfinally, new VoidOp()));
        BasicBlock ordinaryBlock = new(ordinaryLabel, [new AssignmentOp(ordinaryLoadResult, ordinaryLoad)], root,
            new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [protectedBlock, finallyEntry, finallyBody, ordinaryBlock],
            [new Edge(finallyEntry.Label, finallyBody.Label, [])], [], [escapingLocal, ordinaryLocal]);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph,
        };

        new PromoteVariables(optimizer).RunInternal();

        var rewrittenHandlerLoad = (AssignmentOp)optimizer.cfg.GetBlock(finallyBody.Label).Ops.Single();
        var rewrittenOrdinaryLoad = (AssignmentOp)optimizer.cfg.GetBlock(ordinaryBlock.Label).Ops.Single();
        Assert.Multiple(() =>
        {
            Assert.That(rewrittenHandlerLoad.Input, Is.SameAs(handlerLoad));
            Assert.That(rewrittenOrdinaryLoad.Input, Is.SameAs(ordinaryLocal));
        });
    }

    [Test]
    public void LocalWithUnknownMetadataType_RetainsEveryMemoryAccess()
    {
        // CreateControlFlowGraph uses TypeLattice.Any for gaps in local metadata. The Local contract requires those
        // variables to retain their memory operations because their actual storage type is unavailable.
        RootRegion root = new(new BlockLabel(0));
        Local local = new(TypeLattice.Any, 0);
        StackSlot loadResult = new(0, TypeLattice.Any, 0);
        StackSlot storeInput = new(0, TypeLattice.Any, 1);
        ILOp load = new(new ILInstruction(OpCodes.Ldloc_0, null!, []), [], TypeLattice.Any);
        ILOp store = new(new ILInstruction(OpCodes.Stloc_0, null!, []), [storeInput], typeof(void));
        BasicBlock block = new(root.EntryLabel, [new AssignmentOp(loadResult, load), store], root,
            new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [block], [], [], [local]);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph,
        };

        new PromoteVariables(optimizer).RunInternal();

        IReadOnlyList<Op> rewritten = optimizer.cfg.GetBlock(block.Label).Ops;
        Assert.Multiple(() =>
        {
            Assert.That(((AssignmentOp)rewritten[0]).Input, Is.SameAs(load));
            Assert.That(rewritten[1], Is.SameAs(store));
        });
    }

    [Test]
    public void LoadsInBranchAndEdgeInputsArePromoted()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel fallthroughLabel = new(1);
        BlockLabel takenLabel = new(2);
        Argument argument = new(0, typeof(int));
        Local local = new(typeof(int), 0);
        StackSlot edgeOutput = new(0, typeof(int), 0);
        ILOp branchLoad = new(new ILInstruction(OpCodes.Ldarg_0, null!, []), [], typeof(int));
        ILOp edgeLoad = new(new ILInstruction(OpCodes.Ldloc_0, null!, []), [], typeof(int));
        ILOp returnLoad = new(new ILInstruction(OpCodes.Ldloc_0, null!, []), [], typeof(int));
        BasicBlock source = new(root.EntryLabel, [], root,
            new ConditionalBranch(OpCodes.Brtrue, [branchLoad], [fallthroughLabel, takenLabel]));
        BasicBlock fallthrough = new(fallthroughLabel, [], root, new Return(Ret, returnLoad));
        BasicBlock taken = new(takenLabel, [], root, new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [source, fallthrough, taken],
        [
            new Edge(source.Label, fallthrough.Label, [new AssignmentOp(edgeOutput, edgeLoad)]),
            new Edge(source.Label, taken.Label, []),
        ], [argument], [local]);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph,
        };

        new PromoteVariables(optimizer).RunInternal();

        var branch = (ConditionalBranch)optimizer.cfg.GetBlock(source.Label).Branch;
        var returnBranch = (Return)optimizer.cfg.GetBlock(fallthrough.Label).Branch;
        AssignmentOp edgeAssignment = optimizer.cfg.GetEdge(source.Label, fallthrough.Label).EdgeAssignments.Single();
        Assert.Multiple(() =>
        {
            Assert.That(branch.Inputs.Single(), Is.SameAs(argument));
            Assert.That(returnBranch.Value, Is.SameAs(local));
            Assert.That(edgeAssignment.Output, Is.SameAs(edgeOutput));
            Assert.That(edgeAssignment.Input, Is.SameAs(local));
        });
    }

    [Test]
    public void UnreachableBlock_IsStillPromoted()
    {
        RootRegion root = new(new BlockLabel(0));
        BlockLabel unreachableLabel = new(1);
        Local local = new(typeof(int), 0);
        StackSlot loadResult = new(0, typeof(int), 0);
        ILOp load = new(new ILInstruction(OpCodes.Ldloc_0, null!, []), [], typeof(int));
        BasicBlock entry = new(root.EntryLabel, [], root, new Return(Ret, new VoidOp()));
        BasicBlock unreachable = new(unreachableLabel, [new AssignmentOp(loadResult, load)], root,
            new Return(Ret, new VoidOp()));
        ControlFlowGraph graph = new(root, [entry, unreachable], [], [], [local]);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph,
        };

        new PromoteVariables(optimizer).RunInternal();

        var assignment = (AssignmentOp)optimizer.cfg.GetBlock(unreachable.Label).Ops.Single();
        Assert.That(assignment.Input, Is.SameAs(local));
    }

    [Test]
    public void Rewrite_PreservesUnrelatedGraphAndInstructionState()
    {
        RootRegion root = new(new BlockLabel(0));
        Argument argument = new(0, typeof(string));
        Local local = new(typeof(int).MakeByRefType(), 0);
        ILOp loadLocal = new(new ILInstruction(OpCodes.Ldloc_0, null!, []), [], typeof(int).MakeByRefType());
        Prefix[] prefixes = [new(OpCodes.Unaligned, (byte)1), new(OpCodes.Volatile, null)];
        ILInstruction instruction = new(OpCodes.Ldind_I4, null!, prefixes);
        ILOp indirectLoad = new(instruction, [loadLocal], typeof(int));
        StackSlot result = new(0, typeof(int), 0);
        AssignmentOp assignment = new(result, indirectLoad);
        Return returnBranch = new(Ret, new VoidOp());
        BasicBlock block = new(root.EntryLabel, [assignment], root, returnBranch);
        List<Argument> arguments = [argument];
        List<Local> locals = [local];
        ControlFlowGraph graph = new(root, [block], [], arguments, locals);
        global::Disharmony.Optimizer.Optimizer optimizer = new(VoidMethod, [], PatchProcessor.CreateILGenerator(), false)
        {
            cfg = graph,
        };

        new PromoteVariables(optimizer).RunInternal();

        ControlFlowGraph rewritten = optimizer.cfg;
        BasicBlock rewrittenBlock = rewritten.GetBlock(block.Label);
        var rewrittenAssignment = (AssignmentOp)rewrittenBlock.Ops.Single();
        var rewrittenLoad = (ILOp)rewrittenAssignment.Input;
        Assert.Multiple(() =>
        {
            Assert.That(rewritten.RootRegion, Is.SameAs(root));
            Assert.That(rewritten.Arguments, Is.EqualTo(arguments));
            Assert.That(rewritten.Arguments[0], Is.SameAs(argument));
            Assert.That(rewritten.Locals, Is.EqualTo(locals));
            Assert.That(rewritten.Locals[0], Is.SameAs(local));
            Assert.That(rewrittenBlock.Label, Is.SameAs(block.Label));
            Assert.That(rewrittenBlock.Region, Is.SameAs(root));
            Assert.That(rewrittenBlock.Branch, Is.SameAs(returnBranch));
            Assert.That(rewrittenAssignment.Output, Is.SameAs(result));
            Assert.That(rewrittenLoad.IL, Is.SameAs(instruction));
            Assert.That(rewrittenLoad.IL.Prefixes, Is.SameAs(prefixes));
            Assert.That(rewrittenLoad.Inputs.Single(), Is.SameAs(local));
        });
    }
}
