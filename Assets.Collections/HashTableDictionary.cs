using SysGC = System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Nonno.Assets.Collections;

public class HashTableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDictionary<TKey, TValue> where TKey : notnull
{
    public new ICollection<TKey> Keys => new Collection<TKey>(Keys);
    public new ICollection<TValue> Values => new Collection<TValue>(Values);

    public void Copy(Span<KeyValuePair<TKey, TValue>> to, ref int index)
    {
        foreach (var item in this)
        {
            to[index++] = item;
        }
    }

    public bool TryAdd(KeyValuePair<TKey, TValue> item) => base.TryAdd(item.Key, item.Value);

    public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value) => TryGetValue(key, out value) && base.Remove(key);
    public new TValue Remove(TKey key) => TryRemove(key, out var value) ? value : throw new KeyNotFoundException();
    public bool TryRemove(KeyValuePair<TKey, TValue> item) => TryGetValue(item.Key, out var value) && Equals(value, item.Value) && base.Remove(item.Key);
    public void Remove(KeyValuePair<TKey, TValue> item) => _ = TryRemove(item);

    public bool TrySetValue(TKey key, TValue value)
    {
        if (ContainsKey(key))
        {
            base[key] = value;
            return true;
        }
        else
        {
            return false;
        }
    }

    internal class Collection<T> : ICollection<T>
    {
        readonly SysGC::ICollection<T> _base;

        public int Count => _base.Count;

        public Collection(SysGC::ICollection<T> @base) => _base = @base;

        public void Clear() => _base.Clear();

        public bool Contains(T item) => _base.Contains(item);

        public void Copy(Span<T> to, ref int index)
        {
            foreach (var item in _base)
            {
                to[index++] = item;
            }
        }

        public IEnumerator<T> GetEnumerator() => _base.GetEnumerator();

        public bool TryAdd(T item)
        {
            _base.Add(item);
            return true;
        }

        public bool TryRemove(T item) => _base.Remove(item);
    }
}
