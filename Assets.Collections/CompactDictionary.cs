using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using SysGC = System.Collections.Generic;
using static Nonno.Assets.Utils;

namespace Nonno.Assets.Collections;

public class CompactDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    readonly CompactList<(TKey key, TValue value)> _items;

    public KeyCollection Keys => new(this);
    ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
    public ValueCollection Values => new(this);
    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _items.Count;
    }

    public TValue this[TKey key]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            foreach (var (aEKey, value) in _items.AsSpan())
            {
                if (EqualityComparer<TKey>.Default.Equals(aEKey, key)) return value;
            }
            throw new KeyNotFoundException();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            var items = _items.AsSpan();
            for (int i = 0; i < items.Length; i++)
            {
                if (EqualityComparer<TKey>.Default.Equals(items[i].key, key))
                {
                    items[i] = new(key, value);
                    return;
                }
            }
            throw new KeyNotFoundException();
        }
    }

    public CompactDictionary()
    {
        _items = new();
    }
    public CompactDictionary(SysGC.IDictionary<TKey, TValue> dictionary)
    {
        _items = new(dictionary.Select(x => (x.Key, x.Value)));
    }

    public void Add(TKey key, TValue value) => Add(new(key, value));
    bool IDictionary<TKey, TValue>.TryAdd(TKey key, TValue value) => TryAdd(new(key, value));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add((TKey key, TValue value) item)
    {
        if (ContainsKey(item.key)) throw new ArgumentException("同じキーを持つ要素が既に存在します。", nameof(item));
        _items.Add(item);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryAdd((TKey key, TValue value) item)
    {
        if (ContainsKey(item.key))
        {
            return false;
        }
        else
        {
            _items.Add(item);
            return true;
        }
    }
    bool ICollection<(TKey key, TValue value)>.TryAdd((TKey key, TValue value) item) => TryAdd(item);

    public void Clear() => _items.Clear();

    public bool Contains((TKey key, TValue value) item) => _items.Contains(item);

    public bool ContainsKey(TKey key)
    {
        foreach (var (aEKey, _) in _items.AsSpan()) if (EqualityComparer<TKey>.Default.Equals(aEKey, key)) return true;
        return false;
    }

    public bool ContainsValue(TValue value)
    {
        foreach (var (_, aEValue) in _items.AsSpan()) if (EqualityComparer<TValue>.Default.Equals(aEValue, value)) return true;
        return false;
    }

    public void Copy(Span<(TKey key, TValue value)> to, ref int index) => _items.Copy(to, ref index);

    public IEnumerator<(TKey key, TValue value)> GetEnumerator() => _items.GetEnumerator();

    public TValue Remove(TKey key)
    {
        if (TryRemove(key, out var r)) return r;
        else throw new KeyNotFoundException();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var items = _items.AsSpan();
        for (int i = 0; i < items.Length; i++)
        {
            if (EqualityComparer<TKey>.Default.Equals(items[i].key, key))
            {
                var pair = _items.Remove(at: i);
                value = pair.value;
                return true;
            }
        }

        value = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRemove((TKey key, TValue value) item)
    {
        var items = _items.AsSpan();
        for (int i = 0; i < items.Length; i++)
        {
            if (EqualityComparer<(TKey, TValue)>.Default.Equals(items[i], item))
            {
                _ = _items.Remove(at: i);
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        foreach (var (aEKey, aEValue) in _items.AsSpan())
        {
            if (EqualityComparer<TKey>.Default.Equals(aEKey, key))
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
        var items = _items.AsSpan();
        for (int i = 0; i < items.Length; i++)
        {
            if (EqualityComparer<TKey>.Default.Equals(items[i].key, key))
            {
                items[i] = new(key, value);
                return true;
            }
        }
        return false;
    }

    public ReadOnlySpan<(TKey key, TValue value)> AsSpan() => _items.AsSpan();

    public class KeyCollection : ICollection<TKey>
    {
        readonly CompactDictionary<TKey, TValue> _dictionary;

        public int Count => _dictionary.Count;
        public bool IsReadOnly => false;

        public KeyCollection(CompactDictionary<TKey, TValue> dictionary) => _dictionary = dictionary;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TKey item) => _dictionary.ContainsKey(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Copy(Span<TKey> to, ref int index)
        {
            if (to.Length - index < _dictionary.Count) throw new ArgumentException("要素数が、コピー先の使用可能な要素数を超えています。", nameof(index));
            foreach (var (key, _) in _dictionary._items.AsSpan()) to[index++] = key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<TKey> GetEnumerator()
        {
            foreach (var (key, _) in _dictionary._items) yield return key;
        }

        void SysGC::ICollection<TKey>.Add(TKey item) => throw new NotSupportedException();
        bool SysGC::ICollection<TKey>.Remove(TKey item) => throw new NotSupportedException();
        void SysGC::ICollection<TKey>.Clear() => throw new NotSupportedException();
        bool ICollection<TKey>.TryAdd(TKey item) => false;
        bool ICollection<TKey>.TryRemove(TKey item) => false;
    }

    public class ValueCollection : ICollection<TValue>
    {
        readonly CompactDictionary<TKey, TValue> _dictionary;

        public int Count => _dictionary.Count;
        public bool IsReadOnly => false;

        public ValueCollection(CompactDictionary<TKey, TValue> dictionary) => _dictionary = dictionary;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TValue item) => _dictionary.ContainsValue(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Copy(Span<TValue> to, ref int index)
        {
            if (to.Length - index < _dictionary.Count) throw new ArgumentException("要素数が、コピー先の使用可能な要素数を超えています。", nameof(index));
            foreach (var (_, value) in _dictionary._items.AsSpan()) to[index++] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (var (_, value) in _dictionary._items) yield return value;
        }

        void SysGC::ICollection<TValue>.Add(TValue item) => throw new NotSupportedException();
        bool SysGC::ICollection<TValue>.Remove(TValue item) => throw new NotSupportedException();
        void SysGC::ICollection<TValue>.Clear() => throw new NotSupportedException();
        bool ICollection<TValue>.TryAdd(TValue item) => false;
        bool ICollection<TValue>.TryRemove(TValue item) => false;
    }
}
