namespace Nonno.Assets.Collections;

public class KeyCollection<TKey, TValue> : ICollection<TKey>
{
    readonly IDictionary<TKey, TValue?> _dictionary;

    public KeyCollection(IDictionary<TKey, TValue?> dictionary)
    {
        _dictionary = dictionary;
    }

    public int Count => _dictionary.Count;

    public void Clear() => _dictionary.Clear();

    public bool Contains(TKey item) => _dictionary.ContainsKey(item);

    public void Copy(Span<TKey> to, ref int index) => _dictionary.Keys.Copy(to, ref index);

    public IEnumerator<TKey> GetEnumerator() => _dictionary.Keys.GetEnumerator();

    public void Add(TKey key) => _dictionary.Add(key, default);
    public bool TryAdd(TKey item) => _dictionary.TryAdd(item, default);

    public void Remove(TKey key) => _ = _dictionary.Remove(key);
    public bool TryRemove(TKey item) => _dictionary.TryRemove(item, out _);
}
