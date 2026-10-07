namespace Disharmony.Tests;

public class FieldLookupBaseTargets
{
    public int Value;
}

public sealed class FieldLookupDerivedTargets : FieldLookupBaseTargets
{
    public new int Value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Target() { }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void CallInner(FieldLookupDerivedTargets inner) => inner.Target();
}
