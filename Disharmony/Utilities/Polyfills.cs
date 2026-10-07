namespace Disharmony.Utilities;

internal static class Polyfills
{
    extension<TKey, TValue>(KeyValuePair<TKey, TValue> kvp)
    {
        public void Deconstruct(out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
}
