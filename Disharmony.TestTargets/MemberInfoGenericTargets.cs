namespace Disharmony.Tests;

public sealed class MemberInfoGenericTargets<T>
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static T StaticIdentity(T value) => value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public T Identity(T value) => value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public T CallIdentity(T value) => Identity(value);
}
