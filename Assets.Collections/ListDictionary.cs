// 令和弐年大暑確認済。
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using SysGC = System.Collections.Generic;

namespace Nonno.Assets.Collections;

public class ListDictionary<TKey, TValue> : SysGC.IDictionary<TKey, TValue>, SysGC.IReadOnlyDictionary<TKey, TValue>, IDictionary<TKey, TValue>
{
    readonly ArrayList<(TKey key, TValue value)> _items;

    public KeyCollection Keys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(this);
    }
    ICollection<TKey> IDictionary<TKey, TValue>.Keys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Keys;
    }
    public ValueCollection Values
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(this);
    }
    ICollection<TValue> IDictionary<TKey, TValue>.Values
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Values;
    }
    IEnumerable<TKey> SysGC.IReadOnlyDictionary<TKey, TValue>.Keys
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Keys;
    }
    IEnumerable<TValue> SysGC.IReadOnlyDictionary<TKey, TValue>.Values
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Values;
    }
    internal ArrayList<(TKey key, TValue value)> BaseList => _items;
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _items.Count;
    }
    public bool IsReadOnly => false;
    public TValue this[TKey key]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            foreach (var (aEKey, value) in _items)
            {
                if (Equals(aEKey, key)) return value;
            }
            throw new KeyNotFoundException();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            var items = _items;
            for (int i = 0; i < items.Count; i++)
            {
                if (Equals(items[i].key, key))
                {
                    items[i] = new(key, value);
                    return;
                }
            }
            throw new KeyNotFoundException();
        }
    }

    public ListDictionary()
    {
        _items = new();
    }
    public ListDictionary(int capacity = 0)
    {
        _items = new(capacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd(TKey key, TValue value)
    {
        Add(key, value);
        return true;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd((TKey key, TValue value) item)
    {
        Add(item);
        return true;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(TKey key, TValue value) => Add(new(key, value));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add((TKey key, TValue value) item)
    {
        if (ContainsKey(item.key)) throw new ArgumentException("同じキーを持つ要素が既に存在します。", nameof(item));
        _items.Add(item);
    }
    void SysGC::ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    bool SysGC.ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => TryRemove((item.Key, item.Value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => _items.Clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains((TKey key, TValue value) item) => _items.Contains(item);
    bool SysGC.ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => Contains((item.Key, item.Value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(TKey key)
    {
        foreach (var (aEKey, _) in _items) if (Equals(aEKey, key)) return true;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsValue(TValue value)
    {
        foreach (var (_, aEValue) in _items) if (Equals(aEValue, value)) return true;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Copy(Span<(TKey key, TValue value)> to, ref int index)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            to[index++] = _items[i];
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo((TKey key, TValue value)[] array, ref int arrayIndex) => _items.Copy(array, ref arrayIndex);
    void SysGC.ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            var (k ,v) = _items[i];
            array[arrayIndex++] = new(k,v);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<(TKey key, TValue value)> GetEnumerator() => ((IEnumerable<T>)_items).GetEnumerator();
    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        foreach (var (key, value) in this) yield return new(key, value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var items = _items;
        for (int i = 0; i < items.Count; i++)
        {
            if (Equals(items[i].key, key))
            {
                value = _items[i].value;
                _items.Remove(at: i);
                return true;
            }
        }
        value = default;
        return false;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue Remove(TKey key)
    {
        var items = _items;
        for (int i = 0; i < items.Count; i++)
        {
            if (Equals(items[i].key, key))
            {
                var value = _items[i].value;
                _items.Remove(at: i);
                return value;
            }
        }
        throw new KeyNotFoundException("キーは存在しません。");
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool SysGC.IDictionary<TKey, TValue>.Remove(TKey key)
    {
        var items = _items;
        for (int i = 0; i < items.Count; i++)
        {
            if (Equals(items[i].key, key))
            {
                _items.Remove(at: i);
                return true;
            }
        }
        return false;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRemove((TKey key, TValue value) item)
    {
        var items = _items;
        for (int i = 0; i < items.Count; i++)
        {
            if (Equals(items[i], item))
            {
                _items.Remove(at: i);
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        foreach (var (aEKey, aEValue) in _items)
        {
            if (Equals(aEKey, key))
            {
                value = aEValue;
                return true;
            }
        }
        value = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TrySetValue(TKey key, TValue value)
    {
        var items = _items;
        for (int i = 0; i < items.Count; i++)
        {
            if (Equals(items[i].key, key))
            {
                items[i] = new(key, value);
                return true;
            }
        }
        return false;
    }

    public ref TValue GetReference(TKey key)
    {
        var span = _items.AsSpan();
        for (int i = 0; i < span.Length; i++)
        {
            if (Equals(span[i].key, key)) return ref _items.GetReference(i).value;
        }
        throw new KeyNotFoundException();
    }

    public bool TryReplace(TKey key, TValue neo, [MaybeNullWhen(false)] out TValue old)
    {
        var items = _items;
        for (int i = 0; i < items.Count; i++)
        {
            if (Equals(items[i].key, key))
            {
                old = items[i].value;
                items[i] = new(key, neo);
                return true;
            }
        }
        old = default;
        return false;
    }
    public TValue Replace(TKey key, TValue value) => TryReplace(key, value, out var r) ? r : throw new Exception("項目の置換に失敗しました。");

    public class KeyCollection : ICollection<TKey>
    {
        readonly ListDictionary<TKey, TValue> _dictionary;

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dictionary.Count;
        }
        public bool IsReadOnly => false;

        public KeyCollection(ListDictionary<TKey, TValue> dictionary) => _dictionary = dictionary;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TKey item) => _dictionary.ContainsKey(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Copy(Span<TKey> to, ref int index)
        {
            if (to.Length - index < _dictionary.Count) throw new ArgumentException("要素数が、コピー先の使用可能な要素数を超えています。", nameof(index));
            foreach (var (key, _) in _dictionary._items) to[index++] = key;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(TKey[] array, int arrayIndex)
        {
            if (array.Length - arrayIndex < _dictionary.Count) throw new ArgumentException("要素数が、コピー先の使用可能な要素数を超えています。", nameof(arrayIndex));
            foreach (var (key, _) in _dictionary._items) array[arrayIndex++] = key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<TKey> GetEnumerator()
        {
            foreach (var (key, _) in _dictionary._items) yield return key;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        bool ICollection<TKey>.TryAdd(TKey item) => false;
        void SysGC.ICollection<TKey>.Add(TKey item) => throw new NotSupportedException();

        void SysGC.ICollection<TKey>.Clear() => throw new NotSupportedException();

        bool ICollection<TKey>.TryRemove(TKey item) => false;
        bool SysGC.ICollection<TKey>.Remove(TKey item) => throw new NotSupportedException();
    }

    public class ValueCollection : ICollection<TValue>
    {
        readonly ListDictionary<TKey, TValue> _dictionary;

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dictionary.Count;
        }
        public bool IsReadOnly => false;

        public ValueCollection(ListDictionary<TKey, TValue> dictionary) => _dictionary = dictionary;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TValue item) => _dictionary.ContainsValue(item);

        public void Copy(Span<TValue> to, ref int index)
        {
            if (to.Length - index < _dictionary.Count) throw new ArgumentException("要素数が、コピー先の使用可能な要素数を超えています。", nameof(index));
            foreach (var (_, value) in _dictionary._items) to[index++] = value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(TValue[] array, int arrayIndex)
        {
            if (array.Length - arrayIndex < _dictionary.Count) throw new ArgumentException("要素数が、コピー先の使用可能な要素数を超えています。", nameof(arrayIndex));
            foreach (var (_, value) in _dictionary._items) array[arrayIndex++] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (var (_, value) in _dictionary._items) yield return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        bool ICollection<TValue>.TryAdd(TValue item) => false;
        void SysGC.ICollection<TValue>.Add(TValue item) => throw new NotSupportedException();

        void SysGC.ICollection<TValue>.Clear() => throw new NotSupportedException();

        bool ICollection<TValue>.TryRemove(TValue item) => false;
        bool SysGC.ICollection<TValue>.Remove(TValue item) => throw new NotSupportedException();
    }
}
