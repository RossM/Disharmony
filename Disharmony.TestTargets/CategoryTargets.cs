namespace Disharmony.Tests;

public static class CategoryTargets
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string Multiple() => "original";

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string Uncategorized() => "original";

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string Other() => "original";

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string Empty() => "original";

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string Duplicate() => "original";

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string Mixed() => "original";
}
