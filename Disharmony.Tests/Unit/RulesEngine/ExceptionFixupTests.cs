namespace Disharmony.Tests.Unit.RulesEngine;

[TestFixture]
public sealed class ExceptionFixupTests
{
    private static readonly MethodInfo VoidMethod =
        typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.Void))!;

    private static readonly MethodInfo IntResultMethod =
        typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.IntResult))!;

    [Test]
    public void NoExceptionRegion_LeavesInstructionsUnchanged()
    {
        List<CodeInstruction> instructions =
        [
            new CodeInstruction(OpCodes.Ldc_I4_1),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Ret),
        ];

        List<CodeInstruction> result = Fix(VoidMethod, instructions);

        Assert.That(result, Is.EqualTo(instructions));
    }

    [Test]
    public void EmptyStackAtExceptionRegion_DoesNotInsertStoresOrLoads()
    {
        ConstructorInfo constructor = typeof(ConstructorTargets).GetConstructor(Type.EmptyTypes)!;
        List<CodeInstruction> instructions =
        [
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
            new CodeInstruction(OpCodes.Ret),
        ];

        List<CodeInstruction> result = Fix(constructor, instructions);

        Assert.That(result, Is.EqualTo(instructions));
    }

    [Test]
    public void IntValueLiveAcrossExceptionRegion_IsStoredAndRestored()
    {
        List<CodeInstruction> instructions =
        [
            new CodeInstruction(OpCodes.Ldc_I4, 42),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
            new CodeInstruction(OpCodes.Ret),
        ];

        List<CodeInstruction> result = Fix(IntResultMethod, instructions);

        var savedValue = (LocalBuilder)result[1].operand;
        Assert.That(result.Select(instruction => (instruction.opcode, instruction.operand)), Is.EqualTo(new (OpCode, object?)[]
        {
            (OpCodes.Ldc_I4, 42),
            (OpCodes.Stloc_S, savedValue),
            (OpCodes.Nop, null),
            (OpCodes.Nop, null),
            (OpCodes.Ldloc_S, savedValue),
            (OpCodes.Ret, null),
        }));
        Assert.That(savedValue.LocalType, Is.EqualTo(typeof(int)));
        Assert.That(result[2], Is.SameAs(instructions[1]));
        Assert.That(result[3], Is.SameAs(instructions[2]));
    }

    [Test]
    public void MultipleValuesLiveAcrossExceptionRegion_PreserveTypesAndStackOrder()
    {
        List<CodeInstruction> instructions =
        [
            new CodeInstruction(OpCodes.Ldc_I4_1),
            new CodeInstruction(OpCodes.Ldc_I8, 2L),
            new CodeInstruction(OpCodes.Ldstr, "three"),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Ret),
        ];

        List<CodeInstruction> result = Fix(VoidMethod, instructions);

        var objectValue = (LocalBuilder)result[3].operand;
        var longValue = (LocalBuilder)result[4].operand;
        var intValue = (LocalBuilder)result[5].operand;
        Assert.That(result.Select(instruction => (instruction.opcode, instruction.operand)), Is.EqualTo(new (OpCode, object?)[]
        {
            (OpCodes.Ldc_I4_1, null),
            (OpCodes.Ldc_I8, 2L),
            (OpCodes.Ldstr, "three"),
            (OpCodes.Stloc_S, objectValue),
            (OpCodes.Stloc_S, longValue),
            (OpCodes.Stloc_S, intValue),
            (OpCodes.Nop, null),
            (OpCodes.Nop, null),
            (OpCodes.Ldloc_S, intValue),
            (OpCodes.Ldloc_S, longValue),
            (OpCodes.Ldloc_S, objectValue),
            (OpCodes.Pop, null),
            (OpCodes.Pop, null),
            (OpCodes.Pop, null),
            (OpCodes.Ret, null),
        }));
        Assert.That(objectValue.LocalType, Is.EqualTo(typeof(object)));
        Assert.That(longValue.LocalType, Is.EqualTo(typeof(long)));
        Assert.That(intValue.LocalType, Is.EqualTo(typeof(int)));
    }

    [Test]
    public void ReferenceValueLiveAcrossExceptionRegion_UsesObjectLocal()
    {
        List<CodeInstruction> instructions =
        [
            new CodeInstruction(OpCodes.Ldstr, "value"),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Ret),
        ];

        List<CodeInstruction> result = Fix(VoidMethod, instructions);

        Assert.That(((LocalBuilder)result[1].operand).LocalType, Is.EqualTo(typeof(object)));
        Assert.That(result[4].operand, Is.SameAs(result[1].operand));
    }

    [Test]
    public void ManagedReferenceLiveAcrossExceptionRegion_PreservesByRefType()
    {
        MethodInfo method = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.IntArgument))!;
        List<CodeInstruction> instructions =
        [
            new CodeInstruction(OpCodes.Ldarga_S, (byte)0),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Ret),
        ];

        List<CodeInstruction> result = Fix(method, instructions);

        Assert.That(((LocalBuilder)result[1].operand).LocalType, Is.EqualTo(typeof(int).MakeByRefType()));
        Assert.That(result[4].operand, Is.SameAs(result[1].operand));
    }

    [Test]
    public void StructArgumentLiveAcrossExceptionRegion_PreservesValueType()
    {
        MethodInfo method = typeof(StaticMethodTargets)
            .GetMethod(nameof(StaticMethodTargets.StructArgument))!;
        List<CodeInstruction> instructions =
        [
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Ret),
        ];

        List<CodeInstruction> result = Fix(method, instructions);

        Assert.That(((LocalBuilder)result[1].operand).LocalType, Is.EqualTo(typeof(BindingStruct)));
        Assert.That(result[4].operand, Is.SameAs(result[1].operand));
    }

    [Test]
    public void InstanceArgumentLiveAcrossExceptionRegion_UsesObjectLocal()
    {
        MethodInfo method = typeof(ClassMethodTargets)
            .GetMethod(nameof(ClassMethodTargets.Void))!;
        List<CodeInstruction> instructions =
        [
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Ret),
        ];

        List<CodeInstruction> result = Fix(method, instructions);

        Assert.That(((LocalBuilder)result[1].operand).LocalType, Is.EqualTo(typeof(object)));
        Assert.That(result[4].operand, Is.SameAs(result[1].operand));
    }

    [Test]
    public void InstanceMethodParameterLiveAcrossExceptionRegion_UsesParameterType()
    {
        MethodInfo method = typeof(ClassMethodTargets)
            .GetMethod(nameof(ClassMethodTargets.IntIdentity))!;
        List<CodeInstruction> instructions =
        [
            new CodeInstruction(OpCodes.Ldarg_1),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
            new CodeInstruction(OpCodes.Ret),
        ];

        List<CodeInstruction> result = Fix(method, instructions);

        Assert.That(((LocalBuilder)result[1].operand).LocalType, Is.EqualTo(typeof(int)));
        Assert.That(result[4].operand, Is.SameAs(result[1].operand));
    }

    [Test]
    public void LocalBuilderLoadLiveAcrossExceptionRegion_UsesDeclaredLocalType()
    {
        ILGenerator generator = PatchProcessor.CreateILGenerator();
        LocalBuilder sourceLocal = generator.DeclareLocal(typeof(BindingStruct));
        List<CodeInstruction> instructions =
        [
            new CodeInstruction(OpCodes.Ldloc_S, sourceLocal),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Ret),
        ];

        ExceptionFixup.Fix(VoidMethod, ref instructions, generator);

        var savedValue = (LocalBuilder)instructions[1].operand;
        Assert.That(savedValue, Is.Not.SameAs(sourceLocal));
        Assert.That(savedValue.LocalType, Is.EqualTo(typeof(BindingStruct)));
        Assert.That(instructions[4].operand, Is.SameAs(savedValue));
    }

    [Test]
    public void IndexedLocalLoadLiveAcrossExceptionRegion_UsesMethodBodyLocalType()
    {
        MethodInfo method = typeof(ControlFlowGraphTargets)
            .GetMethod(nameof(ControlFlowGraphTargets.MethodWithLocal))!;
        List<CodeInstruction> instructions =
        [
            new CodeInstruction(OpCodes.Ldloc_0),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
            new CodeInstruction(OpCodes.Ret),
        ];

        List<CodeInstruction> result = Fix(method, instructions);

        Assert.That(((LocalBuilder)result[1].operand).LocalType, Is.EqualTo(typeof(int)));
        Assert.That(result[4].operand, Is.SameAs(result[1].operand));
    }

    [Test]
    public void UnknownLocalIndexLiveAcrossExceptionRegion_UsesIntPtrFallback()
    {
        List<CodeInstruction> instructions =
        [
            new CodeInstruction(OpCodes.Ldloc_S, (byte)10),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Ret),
        ];

        List<CodeInstruction> result = Fix(VoidMethod, instructions);

        Assert.That(((LocalBuilder)result[1].operand).LocalType, Is.EqualTo(typeof(IntPtr)));
        Assert.That(result[4].operand, Is.SameAs(result[1].operand));
    }

    [Test]
    public void DuplicatedValueLiveAcrossExceptionRegion_UsesDistinctLocalsForBothStackSlots()
    {
        List<CodeInstruction> instructions =
        [
            new CodeInstruction(OpCodes.Ldc_I4_1),
            new CodeInstruction(OpCodes.Dup),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Ret),
        ];

        List<CodeInstruction> result = Fix(VoidMethod, instructions);

        var topValue = (LocalBuilder)result[2].operand;
        var bottomValue = (LocalBuilder)result[3].operand;
        Assert.That(topValue, Is.Not.SameAs(bottomValue));
        Assert.That(topValue.LocalType, Is.EqualTo(typeof(int)));
        Assert.That(bottomValue.LocalType, Is.EqualTo(typeof(int)));
        Assert.That(result[6].operand, Is.SameAs(bottomValue));
        Assert.That(result[7].operand, Is.SameAs(topValue));
    }

    [Test]
    public void ForwardBranchCarriedValue_IsSavedAtExceptionRegionAfterTarget()
    {
        ILGenerator generator = PatchProcessor.CreateILGenerator();
        Label targetLabel = generator.DefineLabel();
        List<CodeInstruction> instructions =
        [
            new CodeInstruction(OpCodes.Ldc_I4_1),
            new CodeInstruction(OpCodes.Br, targetLabel),
            new CodeInstruction(OpCodes.Nop).WithLabels(targetLabel),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Ret),
        ];

        ExceptionFixup.Fix(VoidMethod, ref instructions, generator);

        Assert.That(instructions.Select(instruction => instruction.opcode), Is.EqualTo(new[]
        {
            OpCodes.Ldc_I4_1,
            OpCodes.Br,
            OpCodes.Nop,
            OpCodes.Stloc_S,
            OpCodes.Nop,
            OpCodes.Nop,
            OpCodes.Ldloc_S,
            OpCodes.Pop,
            OpCodes.Ret,
        }));
    }

    [Test]
    public void ForwardBranchToNonFirstAlias_CarriedValueIsSavedAtExceptionRegionAfterTarget()
    {
        ILGenerator generator = PatchProcessor.CreateILGenerator();
        Label firstAlias = generator.DefineLabel();
        Label branchTarget = generator.DefineLabel();
        List<CodeInstruction> instructions =
        [
            new CodeInstruction(OpCodes.Ldc_I4_1),
            new CodeInstruction(OpCodes.Br, branchTarget),
            new CodeInstruction(OpCodes.Nop).WithLabels(firstAlias, branchTarget),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Ret),
        ];

        ExceptionFixup.Fix(VoidMethod, ref instructions, generator);

        Assert.That(instructions.Select(instruction => instruction.opcode), Is.EqualTo(new[]
        {
            OpCodes.Ldc_I4_1,
            OpCodes.Br,
            OpCodes.Nop,
            OpCodes.Stloc_S,
            OpCodes.Nop,
            OpCodes.Nop,
            OpCodes.Ldloc_S,
            OpCodes.Pop,
            OpCodes.Ret,
        }));
    }

    [Test]
    public void SwitchCarriedValue_IsSavedAtExceptionRegionAfterTarget()
    {
        ILGenerator generator = PatchProcessor.CreateILGenerator();
        Label targetLabel = generator.DefineLabel();
        Label endLabel = generator.DefineLabel();
        List<CodeInstruction> instructions =
        [
            new CodeInstruction(OpCodes.Ldc_I4, 42),
            new CodeInstruction(OpCodes.Ldc_I4_0),
            new CodeInstruction(OpCodes.Switch, new[] { targetLabel }),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Br, endLabel),
            new CodeInstruction(OpCodes.Nop).WithLabels(targetLabel),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Ret).WithLabels(endLabel),
        ];

        ExceptionFixup.Fix(VoidMethod, ref instructions, generator);

        Assert.That(instructions.Select(instruction => instruction.opcode), Is.EqualTo(new[]
        {
            OpCodes.Ldc_I4,
            OpCodes.Ldc_I4_0,
            OpCodes.Switch,
            OpCodes.Pop,
            OpCodes.Br,
            OpCodes.Nop,
            OpCodes.Stloc_S,
            OpCodes.Nop,
            OpCodes.Nop,
            OpCodes.Ldloc_S,
            OpCodes.Pop,
            OpCodes.Ret,
        }));
    }

    [Test]
    public void NestedExceptionRegions_SaveAndRestoreEachRegionsStackIndependently()
    {
        List<CodeInstruction> instructions =
        [
            new CodeInstruction(OpCodes.Ldc_I4_1),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            new CodeInstruction(OpCodes.Ldstr, "inner"),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Ret),
        ];

        List<CodeInstruction> result = Fix(VoidMethod, instructions);

        var outerValue = (LocalBuilder)result[1].operand;
        var innerValue = (LocalBuilder)result[4].operand;
        Assert.That(result.Select(instruction => (instruction.opcode, instruction.operand)), Is.EqualTo(new (OpCode, object?)[]
        {
            (OpCodes.Ldc_I4_1, null),
            (OpCodes.Stloc_S, outerValue),
            (OpCodes.Nop, null),
            (OpCodes.Ldstr, "inner"),
            (OpCodes.Stloc_S, innerValue),
            (OpCodes.Nop, null),
            (OpCodes.Nop, null),
            (OpCodes.Ldloc_S, innerValue),
            (OpCodes.Pop, null),
            (OpCodes.Nop, null),
            (OpCodes.Ldloc_S, outerValue),
            (OpCodes.Pop, null),
            (OpCodes.Ret, null),
        }));
        Assert.That(outerValue.LocalType, Is.EqualTo(typeof(int)));
        Assert.That(innerValue.LocalType, Is.EqualTo(typeof(object)));
    }

    [Test]
    public void SequentialExceptionRegions_UseSeparateSavedLocals()
    {
        List<CodeInstruction> instructions =
        [
            new CodeInstruction(OpCodes.Ldc_I4_1),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Ldstr, "second"),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),
            new CodeInstruction(OpCodes.Nop)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Ret),
        ];

        List<CodeInstruction> result = Fix(VoidMethod, instructions);

        LocalBuilder firstValue = result
            .Where(instruction => instruction.opcode == OpCodes.Stloc_S)
            .Select(instruction => (LocalBuilder)instruction.operand)
            .Single(local => local.LocalType == typeof(int));
        LocalBuilder secondValue = result
            .Where(instruction => instruction.opcode == OpCodes.Stloc_S)
            .Select(instruction => (LocalBuilder)instruction.operand)
            .Single(local => local.LocalType == typeof(object));
        Assert.That(firstValue, Is.Not.SameAs(secondValue));
        Assert.That(result.Count(instruction => instruction.opcode == OpCodes.Ldloc_S), Is.EqualTo(2));
        Assert.That(result.Where(instruction => instruction.opcode == OpCodes.Ldloc_S)
            .Select(instruction => instruction.operand), Is.EqualTo(new object[] { firstValue, secondValue }));
    }

    private static List<CodeInstruction> Fix(MethodBase method, List<CodeInstruction> instructions)
    {
        List<CodeInstruction> result = [.. instructions];
        ExceptionFixup.Fix(method, ref result, PatchProcessor.CreateILGenerator());
        return result;
    }
}
