using System.Diagnostics.CodeAnalysis;

namespace Nonno.Assets.Collections;

public class HashTableDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
{
    readonly KeyCollection _keys;
    readonly ValueCollection _values;
    ListDictionary<TKey, TValue>[] _table;
    int _mask;
    Shift _range;
    int _generation;
    int _count;

    public ICollection<TKey> Keys => _keys;
    public ICollection<TValue> Values => _values;

    public int Range => _table.Length;
    public int Count
    {
        get => _count;
        protected set
        {
            _count = value;
            if (_count > 2 * _range) Rerange();
        }
    }

    public TValue this[TKey key]
    {
        get
        {
            if (!TryGetValue(key, out var r)) throw new Exception();
            return r;
        }
        set
        {
            if (!TrySetValue(key, value)) throw new Exception();
        }
    }

    public HashTableDictionary(Shift range, int defaultDeepness = 1, IEnumerable<ListDictionary<TKey, TValue>>? listDictionariesForReuse = null)
    {
        var etor = listDictionariesForReuse?.GetEnumerator();
        var table = new ListDictionary<TKey, TValue>[range];
        for (int i = 0; i < table.Length; i++)
        {
            if (etor is not null && etor.MoveNext()) { etor.Current.Clear(); table[i] = etor.Current; }
            else table[i] = new(capacity: defaultDeepness);
        }

        _range = range;
        _table = table;
        _mask = range - 1;
        _keys = new(this);
        _values = new(this);
    }
    public HashTableDictionary() : this(Shift.S1) { }

    public void Clear()
    {
        _generation++;
        foreach (var list in _table) list.Clear();
        _count = 0;
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
        var g = _generation;
        foreach (var list in _table)
            foreach (var pair in list)
            {
                if (g != _generation) throw new InvalidOperationException("列挙中に辞書の内容を変更することはできません。");
                yield return pair;
            }
    }

    public bool TryAdd((TKey key, TValue value) item)
    {
        _generation++;
        var r =  _table[GetIndex(item.key)].TryAdd(item);
        if (r) Count++;
        return r;
    }
    public bool TryRemove((TKey key, TValue value) item)
    {
        _generation++;
        var r =_table[GetIndex(item.key)].TryRemove(item);
        if (r) Count--;
        return r;
    }

    public bool TryAdd(TKey key, TValue value)
    {
        _generation++;
        var r = _table[GetIndex(key)].TryAdd(key, value);
        if (r) Count++;
        return r;
    }
    public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        _generation++;
        var r = _table[GetIndex(key)].TryRemove(key, out value);
        if (r) Count--;
        return r;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _table[GetIndex(key)].TryGetValue(key, out value);
    }
    public bool TrySetValue(TKey key, TValue value)
    {
        return _table[GetIndex(key)].TrySetValue(key, value);
    }

    public ref TValue GetReference(TKey key) => ref _table[GetIndex(key)].GetReference(key);

    protected int GetIndex(TKey key) => key.GetHashCode() & _mask;

    public void Rerange() => Rerange(Shift.GetSufficientValue(Count));
    public void Rerange(Shift range, int defaultDeepness = 1)
    {
        var etor = ((IEnumerable<ListDictionary<TKey, TValue>>)_table).GetEnumerator();
        var table = new ListDictionary<TKey, TValue>[range];
        for (int i = 0; i < table.Length; i++)
        {
            if (etor is not null && etor.MoveNext()) { etor.Current.Clear(); table[i] = etor.Current; }
            else table[i] = new(capacity: defaultDeepness);
        }

        _range = range;
        _table = table;
        _mask = range - 1;
    }

    public class KeyCollection : ICollection<TKey>
    {
        HashTableDictionary<TKey, TValue> _base;

        public int Count => _base.Count;

        public KeyCollection(HashTableDictionary<TKey, TValue> @base) => _base = @base;

        void SysGC.ICollection<TKey>.Clear() => throw new InvalidOperationException();
        public bool Contains(TKey item) => _base.ContainsKey(item);

        public void Copy(Span<TKey> to, ref int index)
        {
            foreach (var item in this) to[index++] = item;
        }

        public IEnumerator<TKey> GetEnumerator()
        {
            var g = _base._generation;
            foreach (var (key, _) in _base)
            {
                if (g != _base._generation) throw new InvalidOperationException("列挙中に辞書の内容を変更することはできません。");
                yield return key;
            }
        }

        bool ICollection<TKey>.TryAdd(TKey item) => false;
        bool ICollection<TKey>.TryRemove(TKey item) => false;
    }

    public class ValueCollection : ICollection<TValue>
    {
        HashTableDictionary<TKey, TValue> _base;

        public int Count => _base.Count;

        public ValueCollection(HashTableDictionary<TKey, TValue> @base) => _base = @base;

        void SysGC.ICollection<TValue>.Clear() => throw new InvalidOperationException();
        public bool Contains(TValue item) => _base.ContainsValue(item);

        public void Copy(Span<TValue> to, ref int index)
        {
            foreach (var item in this) to[index++] = item;
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            var g = _base._generation;
            foreach (var (_, value) in _base)
            {
                if (g != _base._generation) throw new InvalidOperationException("列挙中に辞書の内容を変更することはできません。");
                yield return value;
            }
        }

        bool ICollection<TValue>.TryAdd(TValue item) => false;
        bool ICollection<TValue>.TryRemove(TValue item) => false;
    }
}
