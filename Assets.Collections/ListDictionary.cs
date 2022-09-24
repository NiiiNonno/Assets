// 令和弐年大暑確認済。
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using SysGC = System.Collections.Generic;

namespace Nonno.Assets.Collections;

public class ListDictionary<TKey, TValue> : SysGC.IDictionary<TKey, TValue>, SysGC.IReadOnlyDictionary<TKey, TValue>, IDictionary<TKey, TValue>
{
    readonly List<KeyValuePair<TKey, TValue>> _items;

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
    internal List<KeyValuePair<TKey, TValue>> BaseList => _items;
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
                if (Equals(items[i].Key, key))
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
    public bool TryAdd(KeyValuePair<TKey, TValue> item)
    {
        Add(item);
        return true;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(TKey key, TValue value) => Add(new(key, value));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(KeyValuePair<TKey, TValue> item)
    {
        if (ContainsKey(item.Key)) throw new ArgumentException("同じキーを持つ要素が既に存在します。", nameof(item));
        _items.Add(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => _items.Clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(KeyValuePair<TKey, TValue> item) => _items.Contains(item);

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
    public void Copy(Span<KeyValuePair<TKey, TValue>> to, ref int index)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            to[index++] = _items[i];
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _items.GetEnumerator();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var items = _items;
        for (int i = 0; i < items.Count; i++)
        {
            if (Equals(items[i].Key, key))
            {
                value = _items[i].Value;
                _items.RemoveAt(i);
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
            if (Equals(items[i].Key, key))
            {
                var value = _items[i].Value;
                _items.RemoveAt(i);
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
            if (Equals(items[i].Key, key))
            {
                _items.RemoveAt(i);
                return true;
            }
        }
        return false;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRemove(KeyValuePair<TKey, TValue> item)
    {
        var items = _items;
        for (int i = 0; i < items.Count; i++)
        {
            if (Equals(items[i], item))
            {
                _items.RemoveAt(i);
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
            if (Equals(items[i].Key, key))
            {
                items[i] = new(key, value);
                return true;
            }
        }
        return false;
    }

    public bool TryReplace(TKey key, TValue neo, [MaybeNullWhen(false)] out TValue old)
    {
        var items = _items;
        for (int i = 0; i < items.Count; i++)
        {
            if (Equals(items[i].Key, key))
            {
                old = items[i].Value;
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
