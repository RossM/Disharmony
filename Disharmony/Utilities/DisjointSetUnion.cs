using System.Collections;

namespace Disharmony.Utilities;

/// <summary>
///     Implements a collection of disjoint sets which supports looking up which set an element
///     is a member of, and merging sets given a member from each set.
/// </summary>
/// <remarks>
///     <para>
///         Each set is identified by one representative member of the set. The representative is not stable
///         and may change after a merge operation.
///     </para>
/// </remarks>
/// <typeparam name="T"></typeparam>
internal class DisjointSetUnion<T> : IEnumerable<IGrouping<T, T>> where T : notnull
{
    private IEnumerable<IGrouping<T, T>> Groups => parents.Keys.ToArray().GroupBy(GetRoot);

    private readonly Dictionary<T, T> parents = [];

    public T this[T value] => GetRoot(value);

    public bool Add(T value)
    {
        if (parents.ContainsKey(value))
            return false;

        parents.Add(value, value);
        return true;
    }

    private T GetRoot(T value)
    {
        T? parent = parents[value];
        if (Equals(value, parent))
            return parent;
        return parents[value] = GetRoot(parent);
    }

    public void Merge(T left, T right)
    {
        var rootLeft = GetRoot(left);
        var rootRight = GetRoot(right);
        if (!EqualityComparer<T>.Default.Equals(rootLeft, rootRight))
            parents[rootLeft] = rootRight;
    }

    public IEnumerator<IGrouping<T, T>> GetEnumerator() => Groups.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Groups).GetEnumerator();
}
