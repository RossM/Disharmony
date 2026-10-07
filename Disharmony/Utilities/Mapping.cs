using System.Collections;

namespace Disharmony.Utilities;

internal record MappingElement<T>(T Input, T Output);

/// <summary>
///     Represents a mapping from elements of <typeparamref name="T" /> to other elements of <typeparamref name="T" />,
///     where an element not explicitly set maps to itself.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class Mapping<T> : IEnumerable<MappingElement<T>> where T : notnull
{
    internal IEnumerable<MappingElement<T>> EnumerableImplementation => elements.Select(kvp => new MappingElement<T>(kvp.Key, kvp.Value));

    public int Count => elements.Count;
    private readonly Dictionary<T, T> elements = [];

    public Mapping() { }

    public Mapping(IEnumerable<MappingElement<T>> elements)
    {
        foreach (var element in elements)
            this[element.Input] = element.Output;
    }

    public T this[T input]
    {
        get => elements.TryGetValue(input, out T? value) ? value : input;
        set
        {
            if (Equals(input, value))
                elements.Remove(input);
            else
                elements[input] = value;
        }
    }

    public void Add(T input, T output)
    {
        if (!Equals(input, output))
            elements.Add(input, output);
    }

    public void Add(MappingElement<T> element) => Add(element.Input, element.Output);

    public bool Remove(T input) => elements.Remove(input);

    /// <summary>
    ///     Returns a mapping that produces the effect of first applying <paramref name="first" /> and then
    ///     <paramref name="second" />.
    /// </summary>
    /// <remarks>
    ///     <c>Merge(first, second)[x]</c> will give the same result as <c>second[first[x]]</c>.
    /// </remarks>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static Mapping<T> Merge(Mapping<T> first, Mapping<T> second)
    {
        Mapping<T> result = [];
        foreach (var kvp in first)
            result[kvp.Input] = second[kvp.Output];
        foreach (var kvp in second)
        {
            if (!result.elements.ContainsKey(kvp.Input))
                result[kvp.Input] = kvp.Output;
        }

        return result;
    }

    public IEnumerator<MappingElement<T>> GetEnumerator() => EnumerableImplementation.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)EnumerableImplementation).GetEnumerator();
}
