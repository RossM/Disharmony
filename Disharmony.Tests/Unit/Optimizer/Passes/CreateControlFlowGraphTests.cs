using System.Runtime.InteropServices;
using Disharmony.Optimizer.Passes;
using Disharmony.Utilities;

namespace Disharmony.Tests.Unit.Optimizer.Passes;

[TestFixture]
public sealed class CreateControlFlowGraphTests
{
    // These tests describe valid CIL according to CLI semantics. ControlFlowGraphGenerator is not required to validate
    // invalid CIL or provide predictable behavior for it, so do not add rejection tests for malformed instruction streams.
    // Methods whose metadata is inspected belong in Disharmony.TestTargets so the compiler-generated metadata is always
    // produced by the project's Release configuration rather than the test runner's current configuration.

    private static readonly MethodInfo VoidMethod =
        typeof(ControlFlowGraphTargets).GetMethod(nameof(ControlFlowGraphTargets.ReturnVoid))!;

    private static readonly MethodInfo IntMethod =
        typeof(ControlFlowGraphTargets).GetMethod(nameof(ControlFlowGraphTargets.ReturnInt))!;

    private static readonly MethodInfo TwoArgumentMethod =
        typeof(ControlFlowGraphTargets).GetMethod(nameof(ControlFlowGraphTargets.Add))!;

    private static readonly MethodInfo OneArgumentMethod =
        typeof(ControlFlowGraphTargets).GetMethod(nameof(ControlFlowGraphTargets.Increment))!;

    private static readonly MethodInfo VoidOneArgumentMethod =
        typeof(ControlFlowGraphTargets).GetMethod(nameof(ControlFlowGraphTargets.ConsumeOne))!;

    private static readonly MethodInfo VoidTwoArgumentMethod =
        typeof(ControlFlowGraphTargets).GetMethod(nameof(ControlFlowGraphTargets.Consume))!;

    private static readonly MethodInfo ParameterShapesMethod =
        typeof(ControlFlowGraphTargets).GetMethod(nameof(ControlFlowGraphTargets.ParameterShapes))!;

    private static readonly MethodInfo InstanceMethod =
        typeof(ControlFlowGraphInstanceTarget).GetMethod(nameof(ControlFlowGraphInstanceTarget.Add))!;

    private static readonly MethodInfo StructInstanceMethod =
        typeof(ControlFlowGraphStructTarget).GetMethod(nameof(ControlFlowGraphStructTarget.Add))!;

    private static readonly ConstructorInfo Constructor =
        typeof(ControlFlowGraphInstanceTarget).GetConstructor([typeof(int)])!;

    private static readonly FieldInfo InstanceField =
        typeof(ControlFlowGraphInstanceTarget).GetField(nameof(ControlFlowGraphInstanceTarget.Value))!;

    [Test]
    public void Metadata_StaticMethod_CreatesDeclaredArgumentsAndReturnType()
    {
        var generator = CreateGenerator(TwoArgumentMethod, PatchProcessor.CreateILGenerator(), ThrowTerminated());

        Assert.That(generator.Arguments.Count, Is.EqualTo(2));
        Assert.That(generator.Arguments[0].Type, Is.EqualTo(typeof(int)));
        Assert.That(generator.Arguments[1].Type, Is.EqualTo(typeof(int)));
        Assert.That(generator.ReturnType, Is.EqualTo(typeof(int)));
    }

    [Test]
    public void Metadata_InstanceMethod_IncludesThisBeforeDeclaredArguments()
    {
        var generator = CreateGenerator(InstanceMethod, PatchProcessor.CreateILGenerator(), ThrowTerminated());

        Assert.That(generator.Arguments.Count, Is.EqualTo(2));
        Assert.That(generator.Arguments[0].Type, Is.EqualTo(typeof(ControlFlowGraphInstanceTarget)));
        Assert.That(generator.Arguments[1].Type, Is.EqualTo(typeof(int)));
    }

    [Test]
    public void Metadata_StructInstanceMethod_UsesByRefThisBeforeDeclaredArguments()
    {
        var generator = CreateGenerator(StructInstanceMethod, PatchProcessor.CreateILGenerator(), ThrowTerminated());

        Assert.That(generator.Arguments.Count, Is.EqualTo(2));
        Assert.That(generator.Arguments[0].Type, Is.EqualTo(typeof(ControlFlowGraphStructTarget).MakeByRefType()));
        Assert.That(generator.Arguments[1].Type, Is.EqualTo(typeof(int)));
    }

    [Test]
    public void Metadata_Constructor_IncludesThisAndHasVoidReturnType()
    {
        var generator = CreateGenerator(Constructor, PatchProcessor.CreateILGenerator(), ThrowTerminated());

        Assert.That(generator.Arguments.Count, Is.EqualTo(2));
        Assert.That(generator.Arguments[0].Type, Is.EqualTo(typeof(ControlFlowGraphInstanceTarget)));
        Assert.That(generator.Arguments[1].Type, Is.EqualTo(typeof(int)));
        Assert.That(generator.ReturnType, Is.EqualTo(typeof(void)));
    }

    [Test]
    public void Metadata_DynamicMethodWithoutMethodBodyStillCreatesArguments()
    {
        var method = new DynamicMethod("ControlFlowGraphDynamicMethod", typeof(void), [typeof(int)]);

        var generator = CreateGenerator(method, PatchProcessor.CreateILGenerator(), [new CodeInstruction(OpCodes.Ret)]);

        Assert.That(generator.MethodBody, Is.Null);
        Assert.That(generator.Arguments.Count, Is.EqualTo(1));
        Assert.That(generator.Arguments[0].Type, Is.EqualTo(typeof(int)));
    }

    [Test]
    public void Metadata_ByRefParameterShapesArePreserved()
    {
        var generator = CreateGenerator(ParameterShapesMethod, PatchProcessor.CreateILGenerator(), ThrowTerminated());

        Assert.That(generator.Arguments.Count, Is.EqualTo(3));
        Assert.That(generator.Arguments.Select(argument => argument.Type),
            Is.EqualTo(new[]
            {
                typeof(int).MakeByRefType(),
                typeof(long).MakeByRefType(),
                typeof(object).MakeByRefType(),
            }));
    }

    [Test]
    public void Locals_MethodBodyLocal_IsCreatedWithoutLocalBuilder()
    {
        MethodInfo method = typeof(ControlFlowGraphTargets)
            .GetMethod(nameof(ControlFlowGraphTargets.MethodWithLocal))!;
        LocalVariableInfo metadataLocal = method.GetMethodBody()!.LocalVariables[0];
        Assert.That(metadataLocal.LocalType, Is.EqualTo(typeof(int)));

        var generator = CreateGenerator(method, PatchProcessor.CreateILGenerator(), ThrowTerminated());

        Assert.That(generator.Locals[metadataLocal.LocalIndex].Type, Is.EqualTo(typeof(int)));
        Assert.That(generator.Locals[metadataLocal.LocalIndex].Tracker, Is.TypeOf(typeof(LocalTrackerIndex)));
    }

    [Test]
    public void Locals_LocalBuilderOnly_IsCreatedWithBuilder()
    {
        ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
        LocalBuilder builder = ilGenerator.DeclareLocal(typeof(string));

        var generator = CreateGenerator(
            VoidMethod, ilGenerator, [new CodeInstruction(OpCodes.Ldloc_S, builder), new CodeInstruction(OpCodes.Pop), .. ThrowTerminated()]);

        Assert.That(generator.Locals[builder.LocalIndex].Type, Is.EqualTo(typeof(string)));
        Assert.That(generator.Locals[builder.LocalIndex].Tracker, Is.TypeOf(typeof(LocalTrackerBuilder)));
        Assert.That(((LocalTrackerBuilder)generator.Locals[builder.LocalIndex].Tracker).Builder, Is.SameAs(builder));
    }

    [Test]
    public void Locals_MetadataLocalCanBeReferencedWithoutLocalBuilder()
    {
        MethodInfo method = typeof(ControlFlowGraphTargets)
            .GetMethod(nameof(ControlFlowGraphTargets.MethodWithLocal))!;

        var generator = CreateGenerator(
            method, PatchProcessor.CreateILGenerator(), [new CodeInstruction(OpCodes.Ldloc_0), new CodeInstruction(OpCodes.Pop), .. ThrowTerminated()]);

        Assert.That(generator.Locals[0].Type, Is.EqualTo(typeof(int)));
        Assert.That(generator.Locals[0].Tracker, Is.TypeOf(typeof(LocalTrackerIndex)));
        Assert.That(GetILOp(generator, OpCodes.Ldloc_0).Inputs, Is.Empty);
    }

    [Test]
    public void Locals_MetadataLocalAndMatchingBuilder_AreMerged()
    {
        MethodInfo method = typeof(ControlFlowGraphTargets)
            .GetMethod(nameof(ControlFlowGraphTargets.MethodWithLocal))!;
        LocalVariableInfo metadataLocal = method.GetMethodBody()!.LocalVariables[0];
        Assert.That(metadataLocal.LocalType, Is.EqualTo(typeof(int)));
        ILGenerator localGenerator = PatchProcessor.CreateILGenerator();
        LocalBuilder builder = null!;
        for (var index = 0; index <= metadataLocal.LocalIndex; index++)
            builder = localGenerator.DeclareLocal(typeof(int));

        var generator = CreateGenerator(
            method, localGenerator, [new CodeInstruction(OpCodes.Ldloc_S, builder), new CodeInstruction(OpCodes.Pop), .. ThrowTerminated()]);

        Assert.That(generator.Locals[metadataLocal.LocalIndex].Tracker, Is.TypeOf(typeof(LocalTrackerBuilder)));
        Assert.That(((LocalTrackerBuilder)generator.Locals[metadataLocal.LocalIndex].Tracker).Builder, Is.SameAs(builder));
    }

    [Test]
    public void Metadata_ArgumentsAndLocalsAreOwnedByTheResultingGraph()
    {
        MethodInfo method = typeof(ControlFlowGraphTargets)
            .GetMethod(nameof(ControlFlowGraphTargets.MethodWithLocal))!;

        var generator = CreateGenerator(method, PatchProcessor.CreateILGenerator(), ThrowTerminated());

        Assert.Multiple(() =>
        {
            Assert.That(generator.ControlFlowGraph.Arguments, Is.EqualTo(generator.Arguments));
            Assert.That(generator.ControlFlowGraph.Locals, Is.EqualTo(generator.Locals));
            Assert.That(generator.ControlFlowGraph.Arguments, Has.Count.EqualTo(1));
            Assert.That(generator.ControlFlowGraph.Arguments[0], Is.SameAs(generator.Arguments[0]));
            Assert.That(generator.ControlFlowGraph.Locals, Has.Count.EqualTo(1));
            Assert.That(generator.ControlFlowGraph.Locals[0], Is.SameAs(generator.Locals[0]));
        });
    }

    [Test]
    public void StackBehaviourPop_EveryFixedFormCreatesExpectedInputs()
    {
        ILGenerator il = PatchProcessor.CreateILGenerator();
        LocalBuilder intLocal = il.DeclareLocal(typeof(int));
        LocalBuilder longLocal = il.DeclareLocal(typeof(long));
        LocalBuilder floatLocal = il.DeclareLocal(typeof(float));
        LocalBuilder doubleLocal = il.DeclareLocal(typeof(double));
        (StackBehaviour Behaviour, CodeInstruction[] Inputs, CodeInstruction Instruction)[] cases =
        [
            (StackBehaviour.Pop0, [], new CodeInstruction(OpCodes.Break)),
            (StackBehaviour.Pop1, [new CodeInstruction(OpCodes.Ldc_I4_0)], new CodeInstruction(OpCodes.Pop)),
            (StackBehaviour.Pop1_pop1,
                [new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ldc_I4_1)],
                new CodeInstruction(OpCodes.Add)),
            (StackBehaviour.Popi, [new CodeInstruction(OpCodes.Ldloca_S, intLocal)],
                new CodeInstruction(OpCodes.Initobj, typeof(int))),
            (StackBehaviour.Popi_pop1,
                [new CodeInstruction(OpCodes.Ldloca_S, intLocal), new CodeInstruction(OpCodes.Ldc_I4_1)],
                new CodeInstruction(OpCodes.Stobj, typeof(int))),
            (StackBehaviour.Popi_popi,
                [new CodeInstruction(OpCodes.Ldloca_S, intLocal), new CodeInstruction(OpCodes.Ldc_I4_1)],
                new CodeInstruction(OpCodes.Stind_I4)),
            (StackBehaviour.Popi_popi8,
                [new CodeInstruction(OpCodes.Ldloca_S, longLocal), new CodeInstruction(OpCodes.Ldc_I8, 1L)],
                new CodeInstruction(OpCodes.Stind_I8)),
            (StackBehaviour.Popi_popi_popi,
                [
                    new CodeInstruction(OpCodes.Ldloca_S, intLocal), new CodeInstruction(OpCodes.Ldloca_S, intLocal),
                    new CodeInstruction(OpCodes.Ldc_I4_4),
                ],
                new CodeInstruction(OpCodes.Cpblk)),
            (StackBehaviour.Popi_popr4,
                [new CodeInstruction(OpCodes.Ldloca_S, floatLocal), new CodeInstruction(OpCodes.Ldc_R4, 1f)],
                new CodeInstruction(OpCodes.Stind_R4)),
            (StackBehaviour.Popi_popr8,
                [new CodeInstruction(OpCodes.Ldloca_S, doubleLocal), new CodeInstruction(OpCodes.Ldc_R8, 1d)],
                new CodeInstruction(OpCodes.Stind_R8)),
            (StackBehaviour.Popref, [new CodeInstruction(OpCodes.Ldnull)],
                new CodeInstruction(OpCodes.Castclass, typeof(object))),
            (StackBehaviour.Popref_pop1,
                [new CodeInstruction(OpCodes.Ldnull), new CodeInstruction(OpCodes.Ldc_I4_1)],
                new CodeInstruction(OpCodes.Stfld, InstanceField)),
            (StackBehaviour.Popref_popi,
                [
                    new CodeInstruction(OpCodes.Ldc_I4_1), new CodeInstruction(OpCodes.Newarr, typeof(int)),
                    new CodeInstruction(OpCodes.Ldc_I4_0),
                ],
                new CodeInstruction(OpCodes.Ldelem_I4)),
            (StackBehaviour.Popref_popi_popi,
                [
                    new CodeInstruction(OpCodes.Ldc_I4_1), new CodeInstruction(OpCodes.Newarr, typeof(int)),
                    new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ldc_I4_1),
                ],
                new CodeInstruction(OpCodes.Stelem_I4)),
            (StackBehaviour.Popref_popi_popi8,
                [
                    new CodeInstruction(OpCodes.Ldc_I4_1), new CodeInstruction(OpCodes.Newarr, typeof(long)),
                    new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ldc_I8, 1L),
                ],
                new CodeInstruction(OpCodes.Stelem_I8)),
            (StackBehaviour.Popref_popi_popr4,
                [
                    new CodeInstruction(OpCodes.Ldc_I4_1), new CodeInstruction(OpCodes.Newarr, typeof(float)),
                    new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ldc_R4, 1f),
                ],
                new CodeInstruction(OpCodes.Stelem_R4)),
            (StackBehaviour.Popref_popi_popr8,
                [
                    new CodeInstruction(OpCodes.Ldc_I4_1), new CodeInstruction(OpCodes.Newarr, typeof(double)),
                    new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ldc_R8, 1d),
                ],
                new CodeInstruction(OpCodes.Stelem_R8)),
            (StackBehaviour.Popref_popi_popref,
                [
                    new CodeInstruction(OpCodes.Ldc_I4_1), new CodeInstruction(OpCodes.Newarr, typeof(object)),
                    new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ldnull),
                ],
                new CodeInstruction(OpCodes.Stelem_Ref)),
            (StackBehaviour.Popref_popi_pop1,
                [
                    new CodeInstruction(OpCodes.Ldc_I4_1), new CodeInstruction(OpCodes.Newarr, typeof(int)),
                    new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ldc_I4_1),
                ],
                new CodeInstruction(OpCodes.Stelem, typeof(int))),
        ];
        Assert.That(cases.Select(testCase => testCase.Behaviour).Distinct(), Is.EquivalentTo(
            Enum.GetValues(typeof(StackBehaviour)).Cast<StackBehaviour>()
                .Where(behaviour => behaviour != StackBehaviour.Varpop && behaviour.ToString().StartsWith("Pop"))));

        foreach (var testCase in cases)
        {
            List<CodeInstruction> instructions =
                [.. testCase.Inputs, testCase.Instruction];
            if (testCase.Instruction.opcode.StackBehaviourPush != StackBehaviour.Push0)
                instructions.Add(new CodeInstruction(OpCodes.Pop));
            instructions.AddRange(ThrowTerminated());
            var generator = CreateGenerator(VoidMethod, il, instructions);
            ILOp operation = GetILOp(generator, testCase.Instruction.opcode);
            int expectedInputCount = testCase.Behaviour switch
            {
                StackBehaviour.Pop0 => 0,
                StackBehaviour.Pop1 or StackBehaviour.Popi or StackBehaviour.Popref => 1,
                StackBehaviour.Pop1_pop1 or StackBehaviour.Popi_pop1 or StackBehaviour.Popi_popi or
                    StackBehaviour.Popi_popi8 or StackBehaviour.Popi_popr4 or StackBehaviour.Popi_popr8 or
                    StackBehaviour.Popref_pop1 or StackBehaviour.Popref_popi => 2,
                _ => 3,
            };

            Assert.That(operation.Inputs, Has.Count.EqualTo(expectedInputCount), testCase.Behaviour.ToString());
        }
    }

    [Test]
    public void StackBehaviourPop_VarpopHandlesStaticInstanceAndConstructorOperands()
    {
        (CodeInstruction[] Inputs, CodeInstruction Instruction, int InputCount)[] cases =
        [
            ([new CodeInstruction(OpCodes.Ldc_I4_1), new CodeInstruction(OpCodes.Ldc_I4_2)],
                new CodeInstruction(OpCodes.Call, TwoArgumentMethod), 2),
            ([new CodeInstruction(OpCodes.Ldnull), new CodeInstruction(OpCodes.Ldc_I4_1)],
                new CodeInstruction(OpCodes.Callvirt, InstanceMethod), 2),
            ([new CodeInstruction(OpCodes.Ldc_I4_1)], new CodeInstruction(OpCodes.Newobj, Constructor), 1),
        ];

        foreach (var testCase in cases)
        {
            List<CodeInstruction> instructions =
                [.. testCase.Inputs, testCase.Instruction, new CodeInstruction(OpCodes.Pop), .. ThrowTerminated()];
            var generator = CreateGenerator(VoidMethod, PatchProcessor.CreateILGenerator(), instructions);
            ILOp operation = GetILOp(generator, testCase.Instruction.opcode);

            Assert.That(operation.Inputs, Has.Count.EqualTo(testCase.InputCount), testCase.Instruction.opcode.Name);
        }
    }

    [Test]
    public void StackBehaviourPush_EveryFixedFormProducesExpectedStackDepth()
    {
        (StackBehaviour Behaviour, CodeInstruction[] Instructions, int StackDepth)[] cases =
        [
            (StackBehaviour.Push0, [new CodeInstruction(OpCodes.Nop)], 0),
            (StackBehaviour.Push1, [new CodeInstruction(OpCodes.Ldarg_0)], 1),
            (StackBehaviour.Pushi, [new CodeInstruction(OpCodes.Ldc_I4_0)], 1),
            (StackBehaviour.Pushi8, [new CodeInstruction(OpCodes.Ldc_I8, 0L)], 1),
            (StackBehaviour.Pushr4, [new CodeInstruction(OpCodes.Ldc_R4, 0f)], 1),
            (StackBehaviour.Pushr8, [new CodeInstruction(OpCodes.Ldc_R8, 0d)], 1),
            (StackBehaviour.Pushref, [new CodeInstruction(OpCodes.Ldnull)], 1),
            (StackBehaviour.Push1_push1,
                [new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Dup)], 2),
        ];
        Assert.That(cases.Select(testCase => testCase.Behaviour), Is.EquivalentTo(
            Enum.GetValues(typeof(StackBehaviour)).Cast<StackBehaviour>()
                .Where(behaviour => behaviour != StackBehaviour.Varpush && behaviour.ToString().StartsWith("Push"))));

        foreach (var testCase in cases)
        {
            ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
            Label targetLabel = ilGenerator.DefineLabel();
            var target = new CodeInstruction(OpCodes.Pop).WithLabels(targetLabel);
            List<CodeInstruction> instructions = [.. testCase.Instructions, new CodeInstruction(OpCodes.Br, targetLabel)];
            for (var index = 0; index < testCase.StackDepth; index++)
                instructions.Add(index == 0 ? target : new CodeInstruction(OpCodes.Pop));
            if (testCase.StackDepth == 0)
            {
                target = new CodeInstruction(OpCodes.Ret).WithLabels(targetLabel);
                instructions.Add(target);
            }
            else
            {
                instructions.Add(new CodeInstruction(OpCodes.Ret));
            }

            var generator = CreateGenerator(VoidTwoArgumentMethod, ilGenerator, instructions);
            BasicBlock entry = FirstInstructionBlock(generator);

            Assert.That(generator.BlockStacks[entry.Label].OutgoingStack, Has.Count.EqualTo(testCase.StackDepth),
                testCase.Behaviour.ToString());
        }
    }

    [Test]
    public void StackBehaviourPush_VarpushUsesMethodReturnType()
    {
        ILGenerator il = PatchProcessor.CreateILGenerator();
        Label valueTargetLabel = il.DefineLabel();
        Label voidTargetLabel = il.DefineLabel();
        var valueGenerator = CreateGenerator(
            VoidMethod, PatchProcessor.CreateILGenerator(), [
                new CodeInstruction(OpCodes.Call, IntMethod),
                new CodeInstruction(OpCodes.Br, valueTargetLabel),
                new CodeInstruction(OpCodes.Pop).WithLabels(valueTargetLabel),
                new CodeInstruction(OpCodes.Ret),
            ]);
        var voidGenerator = CreateGenerator(
            VoidMethod, il, [
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Call, VoidTwoArgumentMethod),
                new CodeInstruction(OpCodes.Br, voidTargetLabel),
                new CodeInstruction(OpCodes.Ret).WithLabels(voidTargetLabel),
            ]);

        BasicBlock valueEntry = FirstInstructionBlock(valueGenerator);
        BasicBlock voidEntry = FirstInstructionBlock(voidGenerator);
        Assert.That(valueGenerator.BlockStacks[valueEntry.Label].OutgoingStack, Has.Count.EqualTo(1));
        Assert.That(voidGenerator.BlockStacks[voidEntry.Label].OutgoingStack, Is.Empty);
    }

    [Test]
    public void StackBehaviour_VarpopAndVarpush_CalliUsesInlineSignature()
    {
        // Harmony decodes InlineSig operands to InlineSignature rather than SignatureHelper. Keep calli coverage separate
        // from ordinary MethodInfo-based calls because both its argument count and return behavior come from this object.
        Type signatureType = typeof(CodeInstruction).Assembly.GetType("HarmonyLib.InlineSignature")!;
        object signature = Activator.CreateInstance(signatureType)!;
        signatureType.GetProperty("CallingConvention")!.SetValue(signature, CallingConvention.Cdecl);
        signatureType.GetProperty("Parameters")!.SetValue(signature, new List<object> { typeof(int) });
        signatureType.GetProperty("ReturnType")!.SetValue(signature, typeof(int));
        var generator = CreateGenerator(
            VoidMethod, PatchProcessor.CreateILGenerator(), [
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Ldftn, OneArgumentMethod),
                new CodeInstruction(OpCodes.Calli, signature),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ret),
            ]);

        ILOp operation = GetILOp(generator, OpCodes.Calli);
        Assert.That(operation.Inputs, Has.Count.EqualTo(2));
        Assert.That(operation.Type, Is.Not.EqualTo(typeof(void)));
    }

    [Test]
    public void StackBehaviour_VarpopAndVarpush_VoidCalliUsesInlineSignature()
    {
        Type signatureType = typeof(CodeInstruction).Assembly.GetType("HarmonyLib.InlineSignature")!;
        object signature = Activator.CreateInstance(signatureType)!;
        signatureType.GetProperty("CallingConvention")!.SetValue(signature, CallingConvention.Cdecl);
        signatureType.GetProperty("Parameters")!.SetValue(signature, new List<object> { typeof(int) });
        signatureType.GetProperty("ReturnType")!.SetValue(signature, typeof(void));
        var generator = CreateGenerator(
            VoidMethod, PatchProcessor.CreateILGenerator(), [
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Ldftn, VoidOneArgumentMethod),
                new CodeInstruction(OpCodes.Calli, signature),
                new CodeInstruction(OpCodes.Ret),
            ]);

        ILOp operation = GetILOp(generator, OpCodes.Calli);
        Assert.That(operation.Inputs, Has.Count.EqualTo(2));
        Assert.That(operation.Type, Is.EqualTo(typeof(void)));
    }

    [Test]
    public void Dup_CreatesDistinctStackSlotsWithAnExplicitAssignment()
    {
        ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
        Label targetLabel = ilGenerator.DefineLabel();
        var generator = CreateGenerator(
            VoidMethod, ilGenerator, [
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Br, targetLabel),
                new CodeInstruction(OpCodes.Pop).WithLabels(targetLabel),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ret),
            ]);
        BasicBlock entry = FirstInstructionBlock(generator);
        List<StackSlot> outgoing = generator.BlockStacks[entry.Label].OutgoingStack;

        AssignmentOp copy = entry.Ops.OfType<AssignmentOp>()
            .Single(assignment => ReferenceEquals(assignment.Output, outgoing[1]));
        Assert.Multiple(() =>
        {
            Assert.That(outgoing, Has.Count.EqualTo(2));
            Assert.That(outgoing[0], Is.Not.EqualTo(outgoing[1]));
            Assert.That(outgoing.Select(slot => slot.Depth), Is.EqualTo(new[] { 0, 1 }));
            Assert.That(copy.Input, Is.SameAs(outgoing[0]));
            Assert.That(copy.Output, Is.SameAs(outgoing[1]));
            Assert.That(entry.Ops.SelectMany(Flatten).OfType<ILOp>()
                .Any(operation => operation.IL.OpCode == OpCodes.Dup), Is.False);
        });
    }

    [Test]
    public void Prefixes_AreAttachedInOrderToFollowingInstruction()
    {
        ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
        LocalBuilder local = ilGenerator.DeclareLocal(typeof(int));
        var generator = CreateGenerator(
            VoidMethod, ilGenerator, [
                new CodeInstruction(OpCodes.Ldloca_S, local),
                new CodeInstruction(OpCodes.Unaligned, (byte)1),
                new CodeInstruction(OpCodes.Volatile),
                new CodeInstruction(OpCodes.Ldind_I4),
                new CodeInstruction(OpCodes.Pop),
                .. ThrowTerminated(),
            ]);

        ILOp load = GetILOp(generator, OpCodes.Ldind_I4);
        Assert.That(load.IL.Prefixes.Select(prefix => (prefix.OpCode, prefix.Operand)), Is.EqualTo(new (OpCode, object?)[]
        {
            (OpCodes.Unaligned, (byte)1),
            (OpCodes.Volatile, null),
        }));
        Assert.That(GetILOp(generator, OpCodes.Pop).IL.Prefixes, Is.Empty);
    }

    [Test]
    public void Prefixes_TailIsAttachedToCallAndNotFollowingReturn()
    {
        var generator = CreateGenerator(
            VoidMethod, PatchProcessor.CreateILGenerator(), [
                new CodeInstruction(OpCodes.Tailcall),
                new CodeInstruction(OpCodes.Call, VoidMethod),
                new CodeInstruction(OpCodes.Ret),
            ]);

        ILOp call = GetILOp(generator, OpCodes.Call);
        Assert.That(call.IL.Prefixes.Select(prefix => prefix.OpCode), Is.EqualTo(new[] { OpCodes.Tailcall }));
        Return branch = (Return)FirstInstructionBlock(generator).Branch;
        Assert.That(branch.IL.Prefixes, Is.Empty);
    }

    [Test]
    public void Prefixes_LabelOnConstrainedPrefixTargetsBlockContainingCall()
    {
        ILGenerator il = PatchProcessor.CreateILGenerator();
        LocalBuilder local = il.DeclareLocal(typeof(ControlFlowGraphStructTarget));
        Label targetLabel = il.DefineLabel();
        MethodInfo toString = typeof(object).GetMethod(nameof(ToString))!;
        var generator = CreateGenerator(
            VoidMethod, il, [
                new CodeInstruction(OpCodes.Ldloca_S, local),
                new CodeInstruction(OpCodes.Br, targetLabel),
                new CodeInstruction(OpCodes.Constrained, typeof(ControlFlowGraphStructTarget))
                    .WithLabels(targetLabel),
                new CodeInstruction(OpCodes.Callvirt, toString),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ret),
            ]);

        BasicBlock target = generator.ControlFlowGraph.GetBlock(generator.BlockLabels[targetLabel]);
        ILOp call = GetILOp(generator, OpCodes.Callvirt);
        Assert.That(target.Ops.SelectMany(Flatten), Does.Contain(call));
        Assert.That(call.IL.Prefixes.Select(prefix => (prefix.OpCode, prefix.Operand)), Is.EqualTo(new (OpCode, object?)[]
        {
            (OpCodes.Constrained, typeof(ControlFlowGraphStructTarget)),
        }));
    }

    [Test]
    public void StackInputs_PreserveBottomToTopOperandOrder()
    {
        var generator = CreateGenerator(
            VoidMethod, PatchProcessor.CreateILGenerator(), [
                new CodeInstruction(OpCodes.Ldc_I4_4),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Sub),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ret),
            ]);

        ILOp subtract = GetILOp(generator, OpCodes.Sub);
        Assert.That(subtract.Inputs.Cast<StackSlot>().Select(slot => slot.Depth), Is.EqualTo(new[] { 0, 1 }));
        AssignmentOp[] constantAssignments = [.. FirstInstructionBlock(generator).Ops.OfType<AssignmentOp>().Take(2)];
        Assert.That(constantAssignments.Select(assignment => assignment.Output), Is.EqualTo(subtract.Inputs));
        Assert.That(constantAssignments.Select(assignment => ((ILOp)assignment.Input).IL.OpCode),
            Is.EqualTo(new[] { OpCodes.Ldc_I4_4, OpCodes.Ldc_I4_1 }));
    }

    [Test]
    public void ControlFlow_Ret_VoidMethodCreatesReturnWithVoidValue()
    {
        var generator = CreateGenerator(VoidMethod, PatchProcessor.CreateILGenerator(), [new CodeInstruction(OpCodes.Ret)]);

        BasicBlock block = InstructionBlocks(generator).Single();
        Assert.That(block.Branch, Is.TypeOf<Return>());
        var branch = (Return)block.Branch;
        Assert.That(branch.IL.OpCode, Is.EqualTo(OpCodes.Ret));
        Assert.That(branch.Value, Is.TypeOf<VoidOp>());
        Assert.That(generator.BlockStacks[block.Label].OutgoingStack, Is.Empty);
    }

    [Test]
    public void ControlFlow_Ret_ValueMethodConsumesReturnValue()
    {
        var generator = CreateGenerator(
            IntMethod, PatchProcessor.CreateILGenerator(), [new CodeInstruction(OpCodes.Ldc_I4_1), new CodeInstruction(OpCodes.Ret)]);

        BasicBlock block = InstructionBlocks(generator).Single();
        Assert.That(block.Branch, Is.TypeOf<Return>());
        var branch = (Return)block.Branch;
        Assert.That(branch.IL.OpCode, Is.EqualTo(OpCodes.Ret));
        Assert.That(branch.Value, Is.TypeOf<StackSlot>());
        Assert.That(generator.BlockStacks[block.Label].OutgoingStack, Is.Empty);
    }

    [Test]
    public void ControlFlow_Throw_ConsumesExceptionAndHasNoSuccessor()
    {
        var generator = CreateGenerator(
            VoidMethod, PatchProcessor.CreateILGenerator(), [new CodeInstruction(OpCodes.Ldnull), new CodeInstruction(OpCodes.Throw)]);

        BasicBlock block = InstructionBlocks(generator).Single();
        Assert.That(block.Branch, Is.TypeOf<Throw>());
        Assert.That(((Throw)block.Branch).Exception, Is.TypeOf<StackSlot>());
        Assert.That(block.Branch.Labels, Is.Empty);
        Assert.That(generator.BlockStacks[block.Label].OutgoingStack, Is.Empty);
    }

    [Test]
    public void ExceptionRegions_Catch_RethrowHasNoInputAndNoSuccessor()
    {
        var generator = CreateGenerator(
            VoidMethod, PatchProcessor.CreateILGenerator(), [
                new CodeInstruction(OpCodes.Ldnull).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
                new CodeInstruction(OpCodes.Throw),
                new CodeInstruction(OpCodes.Pop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginCatchBlock, typeof(Exception))),
                new CodeInstruction(OpCodes.Rethrow).WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
                new CodeInstruction(OpCodes.Ret),
            ]);

        CatchRegion catchRegion = generator.ControlFlowGraph.ExceptionGroups.Single().HandlerRegions.Cast<CatchRegion>().Single();
        BasicBlock syntheticEntry = generator.ControlFlowGraph.GetBlock(catchRegion.EntryLabel);
        BasicBlock block = generator.ControlFlowGraph.GetBlock(((UnconditionalBranch)syntheticEntry.Branch).Label);
        Assert.That(block.Branch, Is.TypeOf<Rethrow>());
        Assert.That(block.Branch.Labels, Is.Empty);
        Assert.That(generator.BlockStacks[block.Label].OutgoingStack, Is.Empty);
    }

    [Test]
    public void ControlFlow_UnconditionalBranch_ForwardWithoutCarriedStackCreatesEdge()
    {
        ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
        Label targetLabel = ilGenerator.DefineLabel();
        var generator = CreateGenerator(
            VoidMethod, ilGenerator, [
                new CodeInstruction(OpCodes.Br, targetLabel),
                new CodeInstruction(OpCodes.Ret).WithLabels(targetLabel),
            ]);

        BasicBlock source = InstructionBlocks(generator).First();
        BasicBlock destination = InstructionBlocks(generator).Last();
        Assert.That(source.Branch, Is.TypeOf<UnconditionalBranch>());
        Assert.That(((UnconditionalBranch)source.Branch).Label, Is.EqualTo(destination.Label));
        Edge edge = generator.ControlFlowGraph.Edges.Single(candidate => candidate.Source == source.Label);
        Assert.That(edge.Source, Is.EqualTo(source.Label));
        Assert.That(edge.Destination, Is.EqualTo(destination.Label));
        Assert.That(edge.EdgeAssignments, Is.Empty);
    }

    [Test]
    public void ControlFlow_UnconditionalBranch_ForwardWithCarriedStackCreatesAssignment()
    {
        ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
        Label targetLabel = ilGenerator.DefineLabel();
        var generator = CreateGenerator(
            VoidMethod, ilGenerator, [
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Br, targetLabel),
                new CodeInstruction(OpCodes.Pop).WithLabels(targetLabel),
                new CodeInstruction(OpCodes.Ret),
            ]);

        BasicBlock source = InstructionBlocks(generator).First();
        BasicBlock destination = InstructionBlocks(generator).Last();
        Edge edge = generator.ControlFlowGraph.Edges.Single(candidate => candidate.Source == source.Label);
        Assert.That(generator.BlockStacks[source.Label].OutgoingStack, Has.Count.EqualTo(1));
        Assert.That(generator.BlockStacks[destination.Label].IncomingStack, Has.Count.EqualTo(1));
        Assert.That(edge.EdgeAssignments, Has.Count.EqualTo(1));
        Assert.That(edge.EdgeAssignments[0].Output,
            Is.SameAs(generator.BlockStacks[destination.Label].IncomingStack[0]));
        Assert.That(edge.EdgeAssignments[0].Input,
            Is.SameAs(generator.BlockStacks[source.Label].OutgoingStack[0]));
    }

    [Test]
    public void ControlFlow_UnconditionalBranch_BackwardWithoutCarriedStackCreatesEdge()
    {
        ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
        Label loopLabel = ilGenerator.DefineLabel();
        var generator = CreateGenerator(
            VoidMethod, ilGenerator, [
                new CodeInstruction(OpCodes.Br, loopLabel),
                new CodeInstruction(OpCodes.Br, loopLabel).WithLabels(loopLabel),
            ]);

        BasicBlock loopBlock = InstructionBlocks(generator).Last();
        Assert.That(((UnconditionalBranch)loopBlock.Branch).Label, Is.EqualTo(loopBlock.Label));
        Assert.That(generator.ControlFlowGraph.Edges.Any(edge =>
            edge.Source == loopBlock.Label && edge.Destination == loopBlock.Label), Is.True);
    }

    [Test]
    public void ControlFlow_UnconditionalBranch_BackwardWithCarriedStackEstablishedByForwardEdgeCreatesAssignment()
    {
        // The earlier forward branch is required by the CLI backward-branch constraint: it establishes that the loop
        // header can have a non-empty evaluation stack before a later back edge carries a value to the same location.
        ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
        Label loopLabel = ilGenerator.DefineLabel();
        var generator = CreateGenerator(
            VoidMethod, ilGenerator, [
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Br, loopLabel),
                new CodeInstruction(OpCodes.Pop).WithLabels(loopLabel),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Br, loopLabel),
            ]);

        BasicBlock loopBlock = InstructionBlocks(generator).Last();
        Edge backEdge = generator.ControlFlowGraph.Edges.Single(edge => edge.Source == loopBlock.Label);
        Assert.That(generator.BlockStacks[loopBlock.Label].IncomingStack, Has.Count.EqualTo(1));
        Assert.That(generator.BlockStacks[loopBlock.Label].OutgoingStack, Has.Count.EqualTo(1));
        Assert.That(backEdge.EdgeAssignments, Has.Count.EqualTo(1));
    }

    [Test]
    public void ControlFlow_ConditionalBranches_EachOpcodeCreatesTakenAndFallthroughEdges()
    {
        (OpCode OpCode, int InputCount)[] cases =
        [
            (OpCodes.Brfalse, 1), (OpCodes.Brfalse_S, 1), (OpCodes.Brtrue, 1), (OpCodes.Brtrue_S, 1),
            (OpCodes.Beq, 2), (OpCodes.Beq_S, 2), (OpCodes.Bge, 2), (OpCodes.Bge_S, 2),
            (OpCodes.Bge_Un, 2), (OpCodes.Bge_Un_S, 2), (OpCodes.Bgt, 2), (OpCodes.Bgt_S, 2),
            (OpCodes.Bgt_Un, 2), (OpCodes.Bgt_Un_S, 2), (OpCodes.Ble, 2), (OpCodes.Ble_S, 2),
            (OpCodes.Ble_Un, 2), (OpCodes.Ble_Un_S, 2), (OpCodes.Blt, 2), (OpCodes.Blt_S, 2),
            (OpCodes.Blt_Un, 2), (OpCodes.Blt_Un_S, 2), (OpCodes.Bne_Un, 2), (OpCodes.Bne_Un_S, 2),
        ];

        foreach (var testCase in cases)
        {
            ILGenerator il = PatchProcessor.CreateILGenerator();
            Label targetLabel = il.DefineLabel();
            Label endLabel = il.DefineLabel();
            var target = new CodeInstruction(OpCodes.Br, endLabel).WithLabels(targetLabel);
            var end = new CodeInstruction(OpCodes.Ret).WithLabels(endLabel);
            List<CodeInstruction> instructions =
            [
                .. Enumerable.Repeat(new CodeInstruction(OpCodes.Ldc_I4_0), testCase.InputCount),
                new CodeInstruction(testCase.OpCode, targetLabel),
                new CodeInstruction(OpCodes.Br, endLabel),
                target,
                end,
            ];

            var generator = CreateGenerator(VoidMethod, il, instructions);
            BasicBlock source = FirstInstructionBlock(generator);
            Assert.That(source.Branch, Is.TypeOf<ConditionalBranch>(), testCase.OpCode.Name);
            var branch = (ConditionalBranch)source.Branch;
            Assert.That(branch.OpCode, Is.EqualTo(testCase.OpCode), testCase.OpCode.Name);
            Assert.That(branch.Inputs, Has.Count.EqualTo(testCase.InputCount), testCase.OpCode.Name);
            Assert.That(branch.Labels, Has.Count.EqualTo(2), testCase.OpCode.Name);
            Assert.That(generator.ControlFlowGraph.OutgoingEdges(source), Has.Exactly(2).Items, testCase.OpCode.Name);
        }
    }

    [Test]
    public void ControlFlow_ConditionalBranch_ForwardWithoutCarriedStackCreatesTwoEmptyEdges()
    {
        ILGenerator il = PatchProcessor.CreateILGenerator();
        Label targetLabel = il.DefineLabel();
        Label endLabel = il.DefineLabel();
        var generator = CreateGenerator(
            VoidMethod, il, [
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Brtrue, targetLabel),
                new CodeInstruction(OpCodes.Br, endLabel),
                new CodeInstruction(OpCodes.Br, endLabel).WithLabels(targetLabel),
                new CodeInstruction(OpCodes.Ret).WithLabels(endLabel),
            ]);

        BasicBlock source = FirstInstructionBlock(generator);
        Assert.That(generator.BlockStacks[source.Label].OutgoingStack, Is.Empty);
        Assert.That(generator.ControlFlowGraph.OutgoingEdges(source), Has.Exactly(2).Items);
        Assert.That(generator.ControlFlowGraph.OutgoingEdges(source),
            Has.All.Matches<Edge>(edge => edge.EdgeAssignments.Count == 0));
    }

    [Test]
    public void ControlFlow_ConditionalBranch_ForwardWithCarriedStackCreatesTwoAssignments()
    {
        ILGenerator il = PatchProcessor.CreateILGenerator();
        Label targetLabel = il.DefineLabel();
        Label endLabel = il.DefineLabel();
        var generator = CreateGenerator(
            VoidMethod, il, [
                new CodeInstruction(OpCodes.Ldc_I4, 42),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Brtrue, targetLabel),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Br, endLabel),
                new CodeInstruction(OpCodes.Pop).WithLabels(targetLabel),
                new CodeInstruction(OpCodes.Br, endLabel),
                new CodeInstruction(OpCodes.Ret).WithLabels(endLabel),
            ]);

        BasicBlock source = FirstInstructionBlock(generator);
        Assert.That(generator.BlockStacks[source.Label].OutgoingStack, Has.Count.EqualTo(1));
        Assert.That(generator.ControlFlowGraph.OutgoingEdges(source), Has.Exactly(2).Items);
        Assert.That(generator.ControlFlowGraph.OutgoingEdges(source),
            Has.All.Matches<Edge>(edge => edge.EdgeAssignments.Count == 1));
    }

    [Test]
    public void ControlFlow_ConditionalBranch_BackwardWithoutCarriedStackCreatesEmptyBackEdge()
    {
        ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
        Label loopLabel = ilGenerator.DefineLabel();
        var generator = CreateGenerator(
            VoidMethod, ilGenerator, [
                new CodeInstruction(OpCodes.Ldc_I4_0).WithLabels(loopLabel),
                new CodeInstruction(OpCodes.Brtrue, loopLabel),
                new CodeInstruction(OpCodes.Ret),
            ]);

        BasicBlock loopBlock = FirstInstructionBlock(generator);
        Edge backEdge = generator.ControlFlowGraph.OutgoingEdges(loopBlock)
            .Single(edge => edge.Destination == loopBlock.Label);
        Assert.That(generator.BlockStacks[loopBlock.Label].IncomingStack, Is.Empty);
        Assert.That(generator.BlockStacks[loopBlock.Label].OutgoingStack, Is.Empty);
        Assert.That(backEdge.EdgeAssignments, Is.Empty);
    }

    [Test]
    public void ControlFlow_ConditionalBranch_BackwardWithCarriedStackEstablishedByForwardEdgeReusesIncomingSlot()
    {
        // As above, the forward edge establishes the non-empty stack shape before the backward conditional edge uses it.
        ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
        Label loopLabel = ilGenerator.DefineLabel();
        var generator = CreateGenerator(
            VoidMethod, ilGenerator, [
                new CodeInstruction(OpCodes.Ldc_I4, 42),
                new CodeInstruction(OpCodes.Br, loopLabel),
                new CodeInstruction(OpCodes.Ldc_I4_0).WithLabels(loopLabel),
                new CodeInstruction(OpCodes.Brtrue, loopLabel),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ret),
            ]);

        BasicBlock loopBlock = InstructionBlocks(generator).ElementAt(1);
        Edge forwardEdge = generator.ControlFlowGraph.IncomingEdges(loopBlock)
            .Single(edge => edge.Source != loopBlock.Label);
        Edge backEdge = generator.ControlFlowGraph.OutgoingEdges(loopBlock)
            .Single(edge => edge.Destination == loopBlock.Label);
        Assert.That(generator.BlockStacks[loopBlock.Label].IncomingStack, Has.Count.EqualTo(1));
        Assert.That(generator.BlockStacks[loopBlock.Label].OutgoingStack, Has.Count.EqualTo(1));
        Assert.That(forwardEdge.EdgeAssignments, Has.Count.EqualTo(1));
        Assert.That(generator.BlockStacks[loopBlock.Label].OutgoingStack[0],
            Is.SameAs(generator.BlockStacks[loopBlock.Label].IncomingStack[0]));
        Assert.That(backEdge.EdgeAssignments, Is.Empty);
    }

    [Test]
    public void ControlFlow_BrShort_CreatesUnconditionalEdge()
    {
        ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
        Label targetLabel = ilGenerator.DefineLabel();
        var generator = CreateGenerator(
            VoidMethod, ilGenerator, [
                new CodeInstruction(OpCodes.Br_S, targetLabel),
                new CodeInstruction(OpCodes.Ret).WithLabels(targetLabel),
            ]);

        BasicBlock source = FirstInstructionBlock(generator);
        Assert.That(source.Branch, Is.TypeOf<UnconditionalBranch>());
        Assert.That(((UnconditionalBranch)source.Branch).Label, Is.EqualTo(generator.BlockLabels[targetLabel]));
        Assert.That(generator.ControlFlowGraph.OutgoingEdges(source).Single().Destination,
            Is.EqualTo(generator.BlockLabels[targetLabel]));
    }

    [Test]
    public void ControlFlow_Switch_CreatesFallthroughAndOneEdgePerDistinctTargetWithCarriedStack()
    {
        ILGenerator il = PatchProcessor.CreateILGenerator();
        Label firstTargetLabel = il.DefineLabel();
        Label secondTargetLabel = il.DefineLabel();
        Label endLabel = il.DefineLabel();
        var generator = CreateGenerator(
            VoidMethod, il, [
                new CodeInstruction(OpCodes.Ldc_I4, 42),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Switch, new[] { firstTargetLabel, secondTargetLabel }),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Br, endLabel),
                new CodeInstruction(OpCodes.Pop).WithLabels(firstTargetLabel),
                new CodeInstruction(OpCodes.Br, endLabel),
                new CodeInstruction(OpCodes.Pop).WithLabels(secondTargetLabel),
                new CodeInstruction(OpCodes.Br, endLabel),
                new CodeInstruction(OpCodes.Ret).WithLabels(endLabel),
            ]);

        BasicBlock source = FirstInstructionBlock(generator);
        Assert.That(source.Branch, Is.TypeOf<ConditionalBranch>());
        var branch = (ConditionalBranch)source.Branch;
        Assert.That(branch.OpCode, Is.EqualTo(OpCodes.Switch));
        Assert.That(branch.Inputs, Has.Count.EqualTo(1));
        Assert.That(branch.Labels, Has.Count.EqualTo(3));
        Assert.That(generator.ControlFlowGraph.OutgoingEdges(source), Has.Exactly(3).Items);
        Assert.That(generator.ControlFlowGraph.OutgoingEdges(source),
            Has.All.Matches<Edge>(edge => edge.EdgeAssignments.Count == 1));
    }

    [Test]
    public void ControlFlow_Merge_TwoPredecessorsAssignDistinctValuesToOneIncomingStackSlot()
    {
        ILGenerator il = PatchProcessor.CreateILGenerator();
        Label secondPathLabel = il.DefineLabel();
        Label mergeLabel = il.DefineLabel();
        var generator = CreateGenerator(
            VoidTwoArgumentMethod, il, [
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Brtrue, secondPathLabel),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Br, mergeLabel),
                new CodeInstruction(OpCodes.Ldc_I4_2).WithLabels(secondPathLabel),
                new CodeInstruction(OpCodes.Br, mergeLabel),
                new CodeInstruction(OpCodes.Pop).WithLabels(mergeLabel),
                new CodeInstruction(OpCodes.Ret),
            ]);

        BasicBlock mergeBlock = generator.ControlFlowGraph.GetBlock(generator.BlockLabels[mergeLabel]);
        StackSlot incoming = generator.BlockStacks[mergeBlock.Label].IncomingStack.Single();
        Edge[] incomingEdges = [.. generator.ControlFlowGraph.IncomingEdges(mergeBlock)];
        Assert.That(incomingEdges, Has.Length.EqualTo(2));
        Assert.That(incomingEdges, Has.All.Matches<Edge>(edge => edge.EdgeAssignments.Count == 1));
        Assert.That(incomingEdges.Select(edge => edge.EdgeAssignments[0].Output),
            Has.All.EqualTo(incoming));
        Assert.That(incomingEdges.Select(edge => edge.EdgeAssignments[0].Input), Is.Unique);
    }

    [Test]
    public void ControlFlow_MultipleLabelsOnOneInstructionShareBlockAndDuplicateSwitchTargetEdge()
    {
        ILGenerator il = PatchProcessor.CreateILGenerator();
        Label firstAlias = il.DefineLabel();
        Label secondAlias = il.DefineLabel();
        Label endLabel = il.DefineLabel();
        var generator = CreateGenerator(
            VoidMethod, il, [
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Switch, new[] { firstAlias, secondAlias }),
                new CodeInstruction(OpCodes.Br, endLabel),
                new CodeInstruction(OpCodes.Br, endLabel).WithLabels(firstAlias, secondAlias),
                new CodeInstruction(OpCodes.Ret).WithLabels(endLabel),
            ]);

        BasicBlock source = FirstInstructionBlock(generator);
        Assert.That(generator.BlockLabels[firstAlias], Is.SameAs(generator.BlockLabels[secondAlias]));
        var branch = (ConditionalBranch)source.Branch;
        Assert.That(branch.Labels[1], Is.SameAs(branch.Labels[2]));
        Assert.That(generator.ControlFlowGraph.OutgoingEdges(source), Has.Exactly(2).Items);
        Assert.That(generator.ControlFlowGraph.OutgoingEdges(source).Count(edge =>
            edge.Destination == generator.BlockLabels[firstAlias]), Is.EqualTo(1));
    }

    [Test]
    public void ControlFlow_SwitchWithNoCases_HasOnlyFallthroughEdge()
    {
        var generator = CreateGenerator(
            VoidMethod, PatchProcessor.CreateILGenerator(), [
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Switch, Array.Empty<Label>()),
                new CodeInstruction(OpCodes.Ret),
            ]);

        BasicBlock source = FirstInstructionBlock(generator);
        var branch = (ConditionalBranch)source.Branch;
        Assert.That(branch.Labels, Has.Count.EqualTo(1));
        Assert.That(generator.ControlFlowGraph.OutgoingEdges(source), Has.Exactly(1).Items);
    }

    [Test]
    public void ControlFlow_ConditionalTargetEqualToFallthroughCreatesOneEdge()
    {
        ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
        Label nextLabel = ilGenerator.DefineLabel();
        var generator = CreateGenerator(
            VoidMethod, ilGenerator, [
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Brtrue, nextLabel),
                new CodeInstruction(OpCodes.Ret).WithLabels(nextLabel),
            ]);

        BasicBlock source = FirstInstructionBlock(generator);
        var branch = (ConditionalBranch)source.Branch;
        Assert.That(branch.Labels, Has.Count.EqualTo(2));
        Assert.That(branch.Labels[0], Is.SameAs(branch.Labels[1]));
        Assert.That(generator.ControlFlowGraph.OutgoingEdges(source), Has.Exactly(1).Items);
    }

    [Test]
    public void ControlFlow_ForwardBranchWithTwoCarriedStackSlotsCreatesOrderedAssignments()
    {
        ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
        Label targetLabel = ilGenerator.DefineLabel();
        var target = new CodeInstruction(OpCodes.Pop).WithLabels(targetLabel);
        var generator = CreateGenerator(
            VoidMethod, ilGenerator, [
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Ldc_I4_2),
                new CodeInstruction(OpCodes.Br, targetLabel),
                target,
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ret),
            ]);

        BasicBlock source = FirstInstructionBlock(generator);
        BasicBlock destination = generator.ControlFlowGraph.GetBlock(generator.BlockLabels[targetLabel]);
        Edge edge = generator.ControlFlowGraph.OutgoingEdges(source).Single();
        Assert.That(generator.BlockStacks[source.Label].OutgoingStack.Select(slot => slot.Depth),
            Is.EqualTo(new[] { 0, 1 }));
        Assert.That(generator.BlockStacks[destination.Label].IncomingStack.Select(slot => slot.Depth),
            Is.EqualTo(new[] { 0, 1 }));
        Assert.That(edge.EdgeAssignments, Has.Count.EqualTo(2));
        Assert.That(edge.EdgeAssignments.Select(assignment => assignment.Input),
            Is.EqualTo(generator.BlockStacks[source.Label].OutgoingStack));
        Assert.That(edge.EdgeAssignments.Select(assignment => assignment.Output),
            Is.EqualTo(generator.BlockStacks[destination.Label].IncomingStack));
    }

    [Test]
    public void ControlFlow_ForwardBranch_CarriesReturnValuePastUnreachableAnnotationToLocalStore()
    {
        // Patch rules emit annotations as real nops. The unreachable annotation block must not overwrite the one-slot
        // stack established for the labelled destination by the preceding forward branch.
        ILGenerator il = PatchProcessor.CreateILGenerator();
        LocalBuilder result = il.DeclareLocal(typeof(int));
        Label storeLabel = il.DefineLabel();

        var generator = CreateGenerator(
            IntMethod, il, [
                new CodeInstruction(OpCodes.Ldc_I4_7),
                new CodeInstruction(OpCodes.Br, storeLabel),
                CodeInstruction.Annotation("Unreachable rule boundary"),
                new CodeInstruction(OpCodes.Nop).WithLabels(storeLabel),
                new CodeInstruction(OpCodes.Stloc_S, result),
                new CodeInstruction(OpCodes.Ldloc_S, result),
                new CodeInstruction(OpCodes.Ret),
            ]);

        BasicBlock destination = generator.ControlFlowGraph.GetBlock(generator.BlockLabels[storeLabel]);
        Assert.That(generator.BlockStacks[destination.Label].IncomingStack, Has.Count.EqualTo(1));
        Assert.That(generator.BlockStacks[destination.Label].OutgoingStack, Is.Empty);
        Assert.That(GetILOp(generator, OpCodes.Stloc_S).Inputs,
            Is.EqualTo(generator.BlockStacks[destination.Label].IncomingStack));
    }

    [Test]
    public void ControlFlow_ForwardBranch_CarriesInlineBooleanPastUnreachableAnnotationToLocalStore()
    {
        // InlineRuleBuilder leaves an annotation between the branch returning from the inlined method and its return
        // label. Prefix skip decisions use this exact shape before storing the returned bool to a local.
        ILGenerator il = PatchProcessor.CreateILGenerator();
        LocalBuilder result = il.DeclareLocal(typeof(int));
        LocalBuilder runOriginal = il.DeclareLocal(typeof(bool));
        Label storeBooleanLabel = il.DefineLabel();
        Label skipOriginalLabel = il.DefineLabel();

        var generator = CreateGenerator(
            IntMethod, il, [
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Stloc_S, result),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Br, storeBooleanLabel),
                CodeInstruction.Annotation("Unreachable end of inlined method"),
                new CodeInstruction(OpCodes.Nop).WithLabels(storeBooleanLabel),
                new CodeInstruction(OpCodes.Stloc_S, runOriginal),
                new CodeInstruction(OpCodes.Ldloc_S, runOriginal),
                new CodeInstruction(OpCodes.Brfalse, skipOriginalLabel),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Stloc_S, result),
                new CodeInstruction(OpCodes.Nop).WithLabels(skipOriginalLabel),
                new CodeInstruction(OpCodes.Ldloc_S, result),
                new CodeInstruction(OpCodes.Ret),
            ]);

        BasicBlock booleanStore = generator.ControlFlowGraph.GetBlock(generator.BlockLabels[storeBooleanLabel]);
        Assert.That(generator.BlockStacks[booleanStore.Label].IncomingStack, Has.Count.EqualTo(1));
        Assert.That(generator.BlockStacks[booleanStore.Label].OutgoingStack, Is.Empty);
        ILOp store = booleanStore.Ops.SelectMany(Flatten).OfType<ILOp>()
            .First(operation => operation.IL.OpCode == OpCodes.Stloc_S);
        Assert.That(store.Inputs, Has.Count.EqualTo(1));
    }

    [Test]
    public void ControlFlow_Fallthrough_WithoutCarriedStackCreatesImplicitEdge()
    {
        ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
        Label targetLabel = ilGenerator.DefineLabel();
        var generator = CreateGenerator(
            VoidMethod, ilGenerator, [
                new CodeInstruction(OpCodes.Break),
                new CodeInstruction(OpCodes.Ret).WithLabels(targetLabel),
            ]);

        BasicBlock source = InstructionBlocks(generator).First();
        BasicBlock destination = InstructionBlocks(generator).Last();
        Assert.That(source.Branch, Is.TypeOf<UnconditionalBranch>());
        Assert.That(((UnconditionalBranch)source.Branch).Label, Is.EqualTo(destination.Label));
        Assert.That(generator.ControlFlowGraph.Edges.Single(edge => edge.Source == source.Label).EdgeAssignments, Is.Empty);
    }

    [Test]
    public void ControlFlow_Fallthrough_WithCarriedStackCreatesAssignment()
    {
        ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
        Label targetLabel = ilGenerator.DefineLabel();
        var generator = CreateGenerator(
            VoidMethod, ilGenerator, [
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Pop).WithLabels(targetLabel),
                new CodeInstruction(OpCodes.Ret),
            ]);

        BasicBlock source = InstructionBlocks(generator).First();
        BasicBlock destination = InstructionBlocks(generator).Last();
        Assert.That(generator.BlockStacks[source.Label].OutgoingStack, Has.Count.EqualTo(1));
        Assert.That(generator.BlockStacks[destination.Label].IncomingStack, Has.Count.EqualTo(1));
        Assert.That(generator.ControlFlowGraph.Edges.Single(edge => edge.Source == source.Label).EdgeAssignments,
            Has.Count.EqualTo(1));
    }

    [Test]
    public void Regions_SyntheticEntryAndOrdinaryBlocksBelongToGraphRoot()
    {
        var generator = CreateGenerator(VoidMethod, PatchProcessor.CreateILGenerator(), [new CodeInstruction(OpCodes.Ret)]);

        RootRegion root = generator.ControlFlowGraph.RootRegion;
        BasicBlock syntheticEntry = generator.ControlFlowGraph.GetBlock(root.EntryLabel);
        BasicBlock instructionEntry = FirstInstructionBlock(generator);
        Assert.That(syntheticEntry.Region, Is.SameAs(root));
        Assert.That(syntheticEntry.Label, Is.SameAs(root.EntryLabel));
        Assert.That(syntheticEntry.Ops, Is.Empty);
        Assert.That(syntheticEntry.Branch, Is.TypeOf<UnconditionalBranch>());
        Assert.That(((UnconditionalBranch)syntheticEntry.Branch).Label, Is.SameAs(instructionEntry.Label));
        Assert.That(instructionEntry.Region, Is.SameAs(root));
        Assert.That(generator.ControlFlowGraph.Edges.Any(edge =>
            edge.Source == syntheticEntry.Label && edge.Destination == instructionEntry.Label), Is.True);
    }

    [Test]
    public void ExceptionRegions_Catch_ReceivesImplicitExceptionOnStack()
    {
        ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
        Label endLabel = ilGenerator.DefineLabel();

        var generator = CreateGenerator(
            VoidMethod, ilGenerator, [
                new CodeInstruction(OpCodes.Nop)
                    .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
                new CodeInstruction(OpCodes.Leave, endLabel),
                new CodeInstruction(OpCodes.Pop)
                    .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginCatchBlock, typeof(InvalidOperationException))),
                new CodeInstruction(OpCodes.Leave, endLabel)
                    .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
                new CodeInstruction(OpCodes.Ret).WithLabels(endLabel),
            ]);

        ExceptionGroup group = generator.ControlFlowGraph.ExceptionGroups.Single();
        Assert.That(group.HandlerRegions, Has.Count.EqualTo(1));
        Assert.That(group.HandlerRegions[0], Is.TypeOf<CatchRegion>());
        var catchRegion = (CatchRegion)group.HandlerRegions[0];
        BasicBlock syntheticEntry = generator.ControlFlowGraph.GetBlock(catchRegion.EntryLabel);
        BasicBlock catchBlock = generator.ControlFlowGraph.GetBlock(((UnconditionalBranch)syntheticEntry.Branch).Label);
        Edge entryEdge = generator.ControlFlowGraph.OutgoingEdges(syntheticEntry).Single();
        Assert.That(catchRegion.ExceptionType, Is.EqualTo(typeof(InvalidOperationException)));
        Assert.That(generator.BlockStacks[syntheticEntry.Label].IncomingStack,
            Is.EqualTo(new[] { catchRegion.IncomingException }));
        Assert.That(generator.BlockStacks[syntheticEntry.Label].OutgoingStack,
            Is.EqualTo(new[] { catchRegion.IncomingException }));
        Assert.That(generator.BlockStacks[catchBlock.Label].IncomingStack, Has.Count.EqualTo(1));
        Assert.That(generator.BlockStacks[catchBlock.Label].IncomingStack[0], Is.Not.EqualTo(catchRegion.IncomingException));
        Assert.That(entryEdge.EdgeAssignments, Has.Count.EqualTo(1));
        Assert.That(entryEdge.EdgeAssignments[0].Input, Is.EqualTo(catchRegion.IncomingException));
        Assert.That(entryEdge.EdgeAssignments[0].Output,
            Is.EqualTo(generator.BlockStacks[catchBlock.Label].IncomingStack[0]));
        Assert.That(generator.BlockStacks[catchBlock.Label].OutgoingStack, Is.Empty);
    }

    [Test]
    public void ExceptionRegions_Finally_HasEmptySyntheticEntryAndEndsWithEndfinally()
    {
        ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
        Label endLabel = ilGenerator.DefineLabel();

        var generator = CreateGenerator(
            VoidMethod, ilGenerator, [
                new CodeInstruction(OpCodes.Nop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
                new CodeInstruction(OpCodes.Leave, endLabel),
                new CodeInstruction(OpCodes.Nop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginFinallyBlock)),
                new CodeInstruction(OpCodes.Endfinally).WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
                new CodeInstruction(OpCodes.Ret).WithLabels(endLabel),
            ]);

        ExceptionGroup group = generator.ControlFlowGraph.ExceptionGroups.Single();
        Assert.That(group.HandlerRegions.Single(), Is.TypeOf<FinallyRegion>());
        var region = (FinallyRegion)group.HandlerRegions.Single();
        BasicBlock syntheticEntry = generator.ControlFlowGraph.GetBlock(region.EntryLabel);
        BasicBlock finallyBlock = generator.ControlFlowGraph.GetBlock(((UnconditionalBranch)syntheticEntry.Branch).Label);
        Assert.That(generator.BlockStacks[syntheticEntry.Label].IncomingStack, Is.Empty);
        Assert.That(generator.BlockStacks[syntheticEntry.Label].OutgoingStack, Is.Empty);
        Assert.That(generator.ControlFlowGraph.OutgoingEdges(syntheticEntry).Single().EdgeAssignments, Is.Empty);
        Assert.That(finallyBlock.Region, Is.SameAs(region));
        Assert.That(finallyBlock.Branch, Is.TypeOf<Return>());
        Assert.That(((Return)finallyBlock.Branch).IL.OpCode, Is.EqualTo(OpCodes.Endfinally));
        Assert.That(((Return)finallyBlock.Branch).Value, Is.TypeOf<VoidOp>());
    }

    [Test]
    public void ExceptionRegions_Fault_HasEmptySyntheticEntryAndEndsWithEndfinally()
    {
        ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
        Label endLabel = ilGenerator.DefineLabel();

        var generator = CreateGenerator(
            VoidMethod, ilGenerator, [
                new CodeInstruction(OpCodes.Nop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
                new CodeInstruction(OpCodes.Leave, endLabel),
                new CodeInstruction(OpCodes.Nop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginFaultBlock)),
                new CodeInstruction(OpCodes.Endfinally).WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
                new CodeInstruction(OpCodes.Ret).WithLabels(endLabel),
            ]);

        ExceptionGroup group = generator.ControlFlowGraph.ExceptionGroups.Single();
        Assert.That(group.HandlerRegions.Single(), Is.TypeOf<FaultRegion>());
        var region = (FaultRegion)group.HandlerRegions.Single();
        BasicBlock syntheticEntry = generator.ControlFlowGraph.GetBlock(region.EntryLabel);
        BasicBlock faultBlock = generator.ControlFlowGraph.GetBlock(((UnconditionalBranch)syntheticEntry.Branch).Label);
        Assert.That(generator.BlockStacks[syntheticEntry.Label].IncomingStack, Is.Empty);
        Assert.That(generator.BlockStacks[syntheticEntry.Label].OutgoingStack, Is.Empty);
        Assert.That(generator.ControlFlowGraph.OutgoingEdges(syntheticEntry).Single().EdgeAssignments, Is.Empty);
        Assert.That(faultBlock.Region, Is.SameAs(region));
        Assert.That(faultBlock.Branch, Is.TypeOf<Return>());
        Assert.That(((Return)faultBlock.Branch).IL.OpCode, Is.EqualTo(OpCodes.Endfinally));
        Assert.That(((Return)faultBlock.Branch).Value, Is.TypeOf<VoidOp>());
    }

    [Test]
    public void ExceptionRegions_MultipleCatchHandlersBelongToOneGroupAndHaveDistinctExceptionSlots()
    {
        ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
        Label endLabel = ilGenerator.DefineLabel();

        var generator = CreateGenerator(
            VoidMethod, ilGenerator, [
                new CodeInstruction(OpCodes.Nop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
                new CodeInstruction(OpCodes.Leave, endLabel),
                new CodeInstruction(OpCodes.Pop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginCatchBlock,
                    typeof(InvalidOperationException))),
                new CodeInstruction(OpCodes.Leave, endLabel),
                new CodeInstruction(OpCodes.Pop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginCatchBlock,
                    typeof(ArgumentException))),
                new CodeInstruction(OpCodes.Leave, endLabel).WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
                new CodeInstruction(OpCodes.Ret).WithLabels(endLabel),
            ]);

        ExceptionGroup group = generator.ControlFlowGraph.ExceptionGroups.Single();
        ProtectedRegion protectedRegion = generator.ControlFlowGraph.BasicBlocks.Select(block => block.Region)
            .OfType<ProtectedRegion>().First(region => ReferenceEquals(region.Group, group));
        Assert.That(group.HandlerRegions, Has.Count.EqualTo(2));
        var catches = group.HandlerRegions.Cast<CatchRegion>().ToArray();
        Assert.That(catches.Select(region => region.ExceptionType),
            Is.EqualTo(new[] { typeof(InvalidOperationException), typeof(ArgumentException) }));
        // StackSlot is a record, but its Id is semantic identity: equal depth and type must not merge distinct values.
        Assert.That(catches[0].IncomingException, Is.Not.EqualTo(catches[1].IncomingException));
        Assert.That(catches.Select(region => region.IncomingException.Id), Is.Unique);
        Assert.That(catches, Has.All.Matches<CatchRegion>(region => ReferenceEquals(region.Parent, protectedRegion.Parent)));
    }

    [Test]
    public void ExceptionRegions_NestedGroupsPreserveParentRelationships()
    {
        ILGenerator il = PatchProcessor.CreateILGenerator();
        Label afterInnerLabel = il.DefineLabel();
        Label endLabel = il.DefineLabel();

        var generator = CreateGenerator(
            VoidMethod, il, [
                new CodeInstruction(OpCodes.Nop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
                new CodeInstruction(OpCodes.Nop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
                new CodeInstruction(OpCodes.Leave, afterInnerLabel),
                new CodeInstruction(OpCodes.Nop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginFinallyBlock)),
                new CodeInstruction(OpCodes.Endfinally).WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
                new CodeInstruction(OpCodes.Leave, endLabel).WithLabels(afterInnerLabel),
                new CodeInstruction(OpCodes.Pop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginCatchBlock, typeof(Exception))),
                new CodeInstruction(OpCodes.Leave, endLabel).WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
                new CodeInstruction(OpCodes.Ret).WithLabels(endLabel),
            ]);

        ExceptionGroup outerGroup = generator.ControlFlowGraph.ExceptionGroups
            .Single(group => group.HandlerRegions.Single() is CatchRegion);
        ExceptionGroup innerGroup = generator.ControlFlowGraph.ExceptionGroups
            .Single(group => group.HandlerRegions.Single() is FinallyRegion);
        ProtectedRegion outerProtectedRegion = generator.ControlFlowGraph.BasicBlocks.Select(block => block.Region)
            .OfType<ProtectedRegion>().First(region => ReferenceEquals(region.Group, outerGroup));
        ProtectedRegion innerProtectedRegion = generator.ControlFlowGraph.BasicBlocks.Select(block => block.Region)
            .OfType<ProtectedRegion>().First(region => ReferenceEquals(region.Group, innerGroup));
        Assert.That(outerProtectedRegion.Parent, Is.SameAs(generator.ControlFlowGraph.RootRegion));
        Assert.That(innerProtectedRegion.Parent, Is.SameAs(outerProtectedRegion));
        Assert.That(innerGroup.HandlerRegions.Single().Parent, Is.SameAs(outerProtectedRegion));
        Assert.That(outerGroup.HandlerRegions.Single().Parent, Is.SameAs(generator.ControlFlowGraph.RootRegion));
    }

    [Test]
    public void ExceptionRegions_ProtectedAndCatchRegionsCanContainMultipleBasicBlocks()
    {
        ILGenerator il = PatchProcessor.CreateILGenerator();
        Label secondTryBlockLabel = il.DefineLabel();
        Label secondCatchBlockLabel = il.DefineLabel();
        Label endLabel = il.DefineLabel();

        var generator = CreateGenerator(
            VoidMethod, il, [
                new CodeInstruction(OpCodes.Ldc_I4_0).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
                new CodeInstruction(OpCodes.Brtrue, secondTryBlockLabel),
                new CodeInstruction(OpCodes.Leave, endLabel),
                new CodeInstruction(OpCodes.Leave, endLabel).WithLabels(secondTryBlockLabel),
                new CodeInstruction(OpCodes.Pop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginCatchBlock, typeof(Exception))),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Brtrue, secondCatchBlockLabel),
                new CodeInstruction(OpCodes.Leave, endLabel),
                new CodeInstruction(OpCodes.Leave, endLabel).WithLabels(secondCatchBlockLabel)
                    .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
                new CodeInstruction(OpCodes.Ret).WithLabels(endLabel),
            ]);

        ExceptionGroup group = generator.ControlFlowGraph.ExceptionGroups.Single();
        ProtectedRegion protectedRegion = generator.ControlFlowGraph.BasicBlocks.Select(block => block.Region)
            .OfType<ProtectedRegion>().First(region => ReferenceEquals(region.Group, group));
        CatchRegion catchRegion = group.HandlerRegions.Cast<CatchRegion>().Single();
        Assert.That(generator.ControlFlowGraph.BasicBlocks.Count(block => block.Region == protectedRegion),
            Is.EqualTo(3));
        Assert.That(generator.ControlFlowGraph.BasicBlocks.Count(block => block.Region == catchRegion),
            Is.EqualTo(4));
        BasicBlock catchEntry = generator.ControlFlowGraph.GetBlock(catchRegion.EntryLabel);
        Assert.That(generator.ControlFlowGraph.IncomingEdges(catchEntry), Is.Empty);
        Assert.That(generator.ControlFlowGraph.OutgoingEdges(catchEntry), Has.Exactly(1).Items);
    }

    [Test]
    // Harmony currently corrupts exception-filter metadata before it reaches Disharmony, so filter behavior cannot be
    // tested end-to-end. Preserve the intended unit fixture, but keep it ignored until Harmony can supply usable input.
    [Ignore("Exception filters are unsupported due to a Harmony bug")]
    public void ExceptionRegions_Filter_ThrowsNotSupportedException()
    {
        ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
        Label endLabel = ilGenerator.DefineLabel();
        var optimizer = new global::Disharmony.Optimizer.Optimizer(
            VoidMethod,
            [
                new CodeInstruction(OpCodes.Nop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
                new CodeInstruction(OpCodes.Leave, endLabel),
                new CodeInstruction(OpCodes.Pop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptFilterBlock)),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Endfilter),
                new CodeInstruction(OpCodes.Pop).WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginCatchBlock)),
                new CodeInstruction(OpCodes.Leave, endLabel).WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
                new CodeInstruction(OpCodes.Ret).WithLabels(endLabel),
            ], ilGenerator, false);
        var generator = new CreateControlFlowGraph(optimizer);

        Assert.Throws<NotSupportedException>(generator.RunInternal);
    }

    [Test]
    public void ControlFlow_Leave_LongAndShortFormsCreateLeaveWithEmptyStackEdge()
    {
        foreach (OpCode opcode in new[] { OpCodes.Leave, OpCodes.Leave_S })
        {
            ILGenerator ilGenerator = PatchProcessor.CreateILGenerator();
            Label targetLabel = ilGenerator.DefineLabel();
            var target = new CodeInstruction(OpCodes.Ret).WithLabels(targetLabel);
            var generator = CreateGenerator(VoidMethod, ilGenerator, [new CodeInstruction(opcode, targetLabel), target]);

            BasicBlock source = FirstInstructionBlock(generator);
            Assert.That(source.Branch, Is.TypeOf<Leave>(), opcode.Name);
            Assert.That(((Leave)source.Branch).Label, Is.EqualTo(target.labels.Select(label => generator.BlockLabels[label]).Single()),
                opcode.Name);
            Assert.That(generator.ControlFlowGraph.OutgoingEdges(source).Single().EdgeAssignments, Is.Empty, opcode.Name);
        }
    }

    [Test]
    public void ControlFlow_Jmp_CreatesTerminalTransferWithoutSuccessor()
    {
        var generator = CreateGenerator(VoidMethod, PatchProcessor.CreateILGenerator(), [new CodeInstruction(OpCodes.Jmp, VoidMethod)]);

        BasicBlock block = FirstInstructionBlock(generator);
        Assert.That(block.Branch, Is.TypeOf<Jump>());
        var branch = (Jump)block.Branch;
        Assert.That(branch.Value, Is.TypeOf<ILOp>());
        var operation = (ILOp)branch.Value;
        Assert.That(operation.IL.OpCode, Is.EqualTo(OpCodes.Jmp));
        Assert.That(operation.IL.Operand, Is.SameAs(VoidMethod));
        Assert.That(operation.Inputs, Is.Empty);
        Assert.That(operation.Type, Is.EqualTo(typeof(void)));
        Assert.That(block.Branch.Labels, Is.Empty);
        Assert.That(generator.ControlFlowGraph.OutgoingEdges(block), Is.Empty);
        Assert.That(generator.BlockStacks[block.Label].OutgoingStack, Is.Empty);
    }

    [Test]
    public void ControlFlow_Jmp_WithParametersDoesNotConsumeEvaluationStackValues()
    {
        var generator = CreateGenerator(
            VoidTwoArgumentMethod, PatchProcessor.CreateILGenerator(), [new CodeInstruction(OpCodes.Jmp, VoidTwoArgumentMethod)]);

        var jump = (Jump)FirstInstructionBlock(generator).Branch;
        Assert.That(((ILOp)jump.Value).Inputs, Is.Empty);
        Assert.That(generator.BlockStacks[FirstInstructionBlock(generator).Label].OutgoingStack, Is.Empty);
    }

    [Test]
    public void ControlFlow_Break_IsANonTerminalVoidOperation()
    {
        var generator = CreateGenerator(
            VoidMethod, PatchProcessor.CreateILGenerator(), [new CodeInstruction(OpCodes.Break), new CodeInstruction(OpCodes.Ret)]);

        BasicBlock block = FirstInstructionBlock(generator);
        ILOp operation = GetILOp(generator, OpCodes.Break);
        Assert.That(operation.Inputs, Is.Empty);
        Assert.That(operation.Type, Is.EqualTo(typeof(void)));
        Assert.That(block.Branch, Is.TypeOf<Return>());
    }

    private static CreateControlFlowGraph CreateGenerator(MethodBase method, ILGenerator ilGenerator, List<CodeInstruction> instructions)
    {
        var optimizer = new global::Disharmony.Optimizer.Optimizer(method, instructions, ilGenerator, false);
        var generator = new CreateControlFlowGraph(optimizer);
        generator.RunInternal();
        return generator;
    }

    // Root and handler regions have synthetic empty entry blocks. Tests inspecting input IL must deliberately step past
    // the root entry rather than assuming that BasicBlocks.First() is the first translated instruction block.
    private static BasicBlock FirstInstructionBlock(CreateControlFlowGraph generator) =>
        generator.ControlFlowGraph.GetBlock(
            ((UnconditionalBranch)generator.ControlFlowGraph.GetBlock(generator.ControlFlowGraph.RootRegion.EntryLabel).Branch).Label);

    private static IEnumerable<BasicBlock> InstructionBlocks(CreateControlFlowGraph generator) =>
        generator.ControlFlowGraph.BasicBlocks.Where(block => block.Label != generator.ControlFlowGraph.RootRegion.EntryLabel);

    private static List<CodeInstruction> ThrowTerminated() =>
        [new CodeInstruction(OpCodes.Ldnull), new CodeInstruction(OpCodes.Throw)];

    private static ILOp GetILOp(CreateControlFlowGraph generator, OpCode opcode) =>
        generator.ControlFlowGraph.BasicBlocks
            .SelectMany(block => block.Ops)
            .SelectMany(Flatten)
            .OfType<ILOp>()
            .Single(operation => operation.IL.OpCode == opcode);

    private static IEnumerable<Op> Flatten(Op operation)
    {
        yield return operation;
        switch (operation)
        {
            case AssignmentOp assignment:
                foreach (var nested in Flatten(assignment.Input))
                    yield return nested;
                break;
            case ILOp il:
                foreach (var input in il.Inputs)
                foreach (var nested in Flatten(input))
                    yield return nested;
                break;
        }
    }
}
