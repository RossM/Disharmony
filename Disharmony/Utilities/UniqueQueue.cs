using System.Collections;

namespace Disharmony.Utilities;

/// <summary>
///     FIFO worklist which contains each value at most once.
/// </summary>
internal class UniqueQueue<T> : IEnumerable<T>
{
    public int Count => queue.Count;
    private readonly Queue<T> queue = [];
    private readonly HashSet<T> hashSet = [];

    public bool Enqueue(T item)
    {
        if (!hashSet.Add(item))
            return false;
        queue.Enqueue(item);
        return true;
    }

    public T Dequeue()
    {
        T item = queue.Dequeue();
        hashSet.Remove(item);
        return item;
    }

    public void Clear()
    {
        queue.Clear();
        hashSet.Clear();
    }

    public IEnumerator<T> GetEnumerator() => queue.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)queue).GetEnumerator();
}
