using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Disharmony.Utilities;

/// <summary>
///     Associates each key with an insertion-ordered collection of values.
/// </summary>
internal sealed class MultiDictionary<TKey, TElement> : ILookup<TKey, TElement> where TKey : notnull
{
    private class Grouping(TKey key, IEnumerable<TElement> values) : IGrouping<TKey, TElement>
    {
        public TKey Key { get; } = key;
        public IEnumerator<TElement> GetEnumerator() => values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)values).GetEnumerator();
    }

    private IEnumerable<IGrouping<TKey, TElement>> EnumerableImplementation => valuesByKey.Select(kvp => new Grouping(kvp.Key, kvp.Value));

    public int Count => valuesByKey.Count;

    private readonly Dictionary<TKey, List<TElement>> valuesByKey = [];

    // Creating and destroying empty lists causes GC pressure, so save any lists that become empty to reuse
    private readonly List<TElement>[] emptyLists = new List<TElement>[4];
    private int emptyListCount = 0;

    IEnumerable<TElement> ILookup<TKey, TElement>.this[TKey key] => this[key];
    public IReadOnlyList<TElement> this[TKey key] => valuesByKey.TryGetValue(key, out var values) ? values : Array.Empty<TElement>();

    public bool Contains(TKey key) => valuesByKey.ContainsKey(key);

    public void Add(TKey key, TElement value)
    {
        if (!valuesByKey.TryGetValue(key, out List<TElement>? values))
        {
            values = emptyListCount > 0 ? emptyLists[--emptyListCount] : [];
            valuesByKey.Add(key, values);
        }

        values.Add(value);
    }

    public bool Remove(TKey key, TElement value)
    {
        if (!valuesByKey.TryGetValue(key, out var values))
            return false;
        bool result = values.Remove(value);
        FreeIfEmpty(key, values);
        return result;
    }

    public int RemoveAll(TKey key, Predicate<TElement> predicate)
    {
        if (!valuesByKey.TryGetValue(key, out var values))
            return 0;
        int result = values.RemoveAll(predicate);
        FreeIfEmpty(key, values);
        return result;
    }

    public int RemoveAll(Func<TKey, TElement, bool> predicate)
    {
        int removed = 0;
        foreach (var (key, values) in valuesByKey.ToList())
        {
            removed += values.RemoveAll(value => predicate(key, value));
            FreeIfEmpty(key, values);
        }

        return removed;
    }

    private void FreeIfEmpty(TKey key, List<TElement> values)
    {
        if (values.Count != 0)
            return;
        valuesByKey.Remove(key);
        if (emptyListCount < emptyLists.Length)
            emptyLists[emptyListCount++] = values;
    }

    public bool TryGetValues(
        TKey key,
        [NotNullWhen(true)] out IReadOnlyList<TElement>? values)
    {
        if (valuesByKey.TryGetValue(key, out List<TElement>? mutableValues))
        {
            values = mutableValues;
            return true;
        }

        values = null;
        return false;
    }

    public void Clear()
    {
        valuesByKey.Clear();
        // Entries beyond emptyListCount may still have references to non-empty lists, so clear them
        Array.Clear(emptyLists, emptyListCount, emptyLists.Length - emptyListCount);
    }

    public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator() => EnumerableImplementation.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)EnumerableImplementation).GetEnumerator();
}
