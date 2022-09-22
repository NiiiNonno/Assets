using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using SysGC = System.Collections.Generic;

namespace Nonno.Assets.Collections;

public class WeakReferenceDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue> where TKey : class where TValue : class
{
    readonly ConditionalWeakTable<TKey, TValue> _table;

    public WeakReferenceDictionary()
    {
        _table = new();
    }

    public KeyCollection Keys => new(this);
    ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
    IEnumerable<TKey> SysGC.IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
    public ValueCollection Values => new(this);
    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
    IEnumerable<TValue> SysGC.IReadOnlyDictionary<TKey, TValue>.Values => Values;
    public int Count => _table.Count();

    public void Clear() => _table.Clear();
    public bool Contains(KeyValuePair<TKey, TValue> item) => _table.Contains(item);
    public bool ContainsKey(TKey key) => _table.TryGetValue(key, out var _);
    public bool ContainsValue(TValue value)
    {
        foreach (var (_, aEValue) in _table)
        {
            if (ReferenceEquals(aEValue, value)) return true;
        }
        return false;
    }

    public void Copy(Span<KeyValuePair<TKey, TValue>> to, ref int index)
    {
        foreach (var item in _table)
        {
            to[index++] = item;
        }
    }
    public void CopyKey(Span<TKey> to, ref int index)
    {
        foreach (var (key, _) in _table)
        {
            to[index++] = key;
        }
    }
    public void CopyValue(Span<TValue> to, ref int index)
    {
        foreach (var (_, value) in _table)
        {
            to[index++] = value;
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue>>)_table).GetEnumerator();
    public IEnumerator<TKey> GetKeyEnumerator()
    {
        foreach (var (key, _) in _table)
        {
            yield return key;
        }
    }
    public IEnumerator<TValue> GetValueEnumerator()
    {
        foreach (var (_, value) in _table)
        {
            yield return value;
        }
    }

    public void Add(TKey key, TValue value)
    {
        if (TryAdd(key, value)) return;
        throw new Exception("項目の追加に失敗しました。");
    }
    public bool TryAdd(TKey key, TValue value)
    {
        if (!_table.TryGetValue(key, out var _))
        {
            _table.Add(key, value);
            return true;
        }
        return false;
    }
    public bool TryAdd(KeyValuePair<TKey, TValue> item)
    {
        if (!_table.Contains(item))
        {
            _table.Add(item.Key, item.Value);
            return true;
        }
        return false;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _table.TryGetValue(key, out value);

    public TValue Remove(TKey key)
    {
        if (TryRemove(key, out var r)) return r;
        throw new Exception("項目の削除に失敗しました。");
    }
    public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value) => _table.TryGetValue(key, out value) && _table.Remove(key);
    public bool TryRemove(KeyValuePair<TKey, TValue> item) => _table.TryGetValue(item.Key, out TValue? value) && ReferenceEquals(item.Value, value) && _table.Remove(item.Key);

    public bool TrySetValue(TKey key, TValue value)
    {
        _table.AddOrUpdate(key, value);
        return true;
    }

    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

    public class KeyCollection : ICollection<TKey>
    {
        readonly WeakReferenceDictionary<TKey, TValue> _dictionary;

        public int Count => _dictionary.Count;

        public KeyCollection(WeakReferenceDictionary<TKey, TValue> dictionary) => _dictionary = dictionary;

        public bool Contains(TKey item) => _dictionary.ContainsKey(item);
        public void Copy(Span<TKey> to, ref int index) => _dictionary.CopyKey(to, ref index);
        public IEnumerator<TKey> GetEnumerator() => _dictionary.GetKeyEnumerator();

        void SysGC::ICollection<TKey>.Clear() => throw new NotSupportedException();
        bool ICollection<TKey>.TryAdd(TKey item) => throw new NotSupportedException();
        bool ICollection<TKey>.TryRemove(TKey item) => throw new NotSupportedException();
    }

    public class ValueCollection : ICollection<TValue>
    {
        readonly WeakReferenceDictionary<TKey, TValue> _dictionary;

        public int Count => _dictionary.Count;

        public ValueCollection(WeakReferenceDictionary<TKey, TValue> dictionary) => _dictionary = dictionary;

        public bool Contains(TValue item) => _dictionary.ContainsValue(item);
        public void Copy(Span<TValue> to, ref int index) => _dictionary.CopyValue(to, ref index);
        public IEnumerator<TValue> GetEnumerator() => _dictionary.GetValueEnumerator();

        void SysGC::ICollection<TValue>.Clear() => throw new NotSupportedException();
        bool ICollection<TValue>.TryAdd(TValue item) => throw new NotSupportedException();
        bool ICollection<TValue>.TryRemove(TValue item) => throw new NotSupportedException();
    }
}
