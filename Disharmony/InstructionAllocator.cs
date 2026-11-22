namespace Disharmony;

internal class InstructionAllocator(ILGenerator generator)
{
    public ILGenerator Generator { get; } = generator;

    public Label DefineLabel() => Generator.DefineLabel();

    public virtual LocalBuilder DeclareLocal(Type type, bool pinned = false) =>
        Generator.DeclareLocal(type, pinned);
}
