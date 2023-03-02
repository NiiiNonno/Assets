using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Collections;

public class FixedHashTableDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
{
    readonly ListDictionary<TKey, TValue>[] _table;
    readonly int _mask;
    readonly KeyCollection _keys;
    readonly ValueCollection _values;

    public ICollection<TKey> Keys => _keys;
    public ICollection<TValue> Values => _values;

    public int Range => _table.Length;
    public int Count
    {
        get
        {
            var r = 0;
            foreach (var list in _table) r += list.Count;
            return r;
        }
    }

    public FixedHashTableDictionary(Shift range, int defaultDeepness = 1, IEnumerable<ListDictionary<TKey, TValue>>? listDictionariesForReuse = null)
    {
        var etor = listDictionariesForReuse?.GetEnumerator();
        var table = new ListDictionary<TKey, TValue>[range];
        for (int i = 0; i < table.Length; i++)
        {
            if (etor is not null && etor.MoveNext()) { etor.Current.Clear();  table[i] = etor.Current; }
            else table[i] = new(capacity: defaultDeepness);
        }

        _table = table;
        _mask = range - 1;
        _keys = new(this);
        _values = new(this);
    }

    public void Clear()
    {
        foreach (var list in _table) list.Clear();
    }

    public bool Contains((TKey key, TValue value) item) => _table[GetIndex(item.key)].Contains(item);
    public bool ContainsKey(TKey key) => _table[GetIndex(key)].ContainsKey(key);
    public bool ContainsValue(TValue value) => _table.Any(x => x.ContainsValue(value));

    public void Copy(Span<(TKey key, TValue value)> to, ref int index)
    {
        foreach (var pair in this)
        {
            to[index++] = pair;
        }
    }

    public IEnumerator<(TKey key, TValue value)> GetEnumerator()
    {
        foreach (var list in _table)
            foreach (var pair in list)
                yield return pair;
    }

    public bool TryAdd((TKey key, TValue value) item) => _table[GetIndex(item.key)].TryAdd(item);
    public bool TryRemove((TKey key, TValue value) item) => _table[GetIndex(item.key)].TryRemove(item);
    public bool TryAdd(TKey key, TValue value) => _table[GetIndex(key)].TryAdd(key, value);
    public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value) => _table[GetIndex(key)].TryRemove(key, out value);
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _table[GetIndex(key)].TryGetValue(key, out value);
    public bool TrySetValue(TKey key, TValue value) => _table[GetIndex(key)].TrySetValue(key, value);

    protected int GetIndex(TKey key) => key.GetHashCode() & _mask;

    public static void Rerange(ref FixedHashTableDictionary<TKey, TValue> dictionary, Shift range, int defaultDeepness)
    {
        if (dictionary.Range == range) return;
        dictionary = new(range, defaultDeepness: defaultDeepness, listDictionariesForReuse: dictionary._table);
    }

    public class KeyCollection : ICollection<TKey>
    {
        FixedHashTableDictionary<TKey, TValue> _base;

        public int Count => _base.Count;

        public KeyCollection(FixedHashTableDictionary<TKey, TValue> @base) => _base = @base;

        public void Clear() => throw new InvalidOperationException();
        public bool Contains(TKey item) => _base.ContainsKey(item);

        public void Copy(Span<TKey> to, ref int index)
        {
            foreach (var item in this) to[index++] = item;
        }

        public IEnumerator<TKey> GetEnumerator()
        {
            foreach (var (key, _) in _base) yield return key;
        }

        bool ICollection<TKey>.TryAdd(TKey item) => false;
        bool ICollection<TKey>.TryRemove(TKey item) => false;
    }

    public class ValueCollection : ICollection<TValue>
    {
        FixedHashTableDictionary<TKey, TValue> _base;

        public int Count => _base.Count;

        public ValueCollection(FixedHashTableDictionary<TKey, TValue> @base) => _base = @base;

        public void Clear() => throw new InvalidOperationException();
        public bool Contains(TValue item) => _base.ContainsValue(item);

        public void Copy(Span<TValue> to, ref int index)
        {
            foreach (var item in this) to[index++] = item;
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (var (_, value) in _base) yield return value;
        }

        bool ICollection<TValue>.TryAdd(TValue item) => false;
        bool ICollection<TValue>.TryRemove(TValue item) => false;
    }
}
