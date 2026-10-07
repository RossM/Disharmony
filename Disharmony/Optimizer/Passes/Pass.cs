namespace Disharmony.Optimizer.Passes;

internal abstract class Pass(Optimizer optimizer)
{
    public ControlFlowGraph ControlFlowGraph => Optimizer.cfg;
    public Optimizer Optimizer { get; } = optimizer;

    public void Run()
    {
        RunInternal();
    }

    protected internal abstract void RunInternal();
}
