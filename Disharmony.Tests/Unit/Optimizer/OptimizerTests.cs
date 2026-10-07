namespace Disharmony.Tests.Unit.Optimizer;

[TestFixture]
public sealed class OptimizerTests
{
    [Test]
    public void UnreachableBasicBlock_BeginsWithStackPop_DoesNotThrow()
    {
        ILGenerator generator = PatchProcessor.CreateILGenerator();
        Label returnLabel = generator.DefineLabel();
        List<CodeInstruction> instructions =
        [
            new CodeInstruction(OpCodes.Br, returnLabel),
            new CodeInstruction(OpCodes.Pop),
            new CodeInstruction(OpCodes.Ret).WithLabels(returnLabel),
        ];
        MethodInfo method = typeof(StaticMethodTargets).GetMethod(nameof(StaticMethodTargets.Void))!;
        var optimizer = new global::Disharmony.Optimizer.Optimizer(method, instructions, generator, false);

        Assert.DoesNotThrow(() => optimizer.Optimize());
    }
}
