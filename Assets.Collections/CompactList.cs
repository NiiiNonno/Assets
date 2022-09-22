using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Nonno.Assets.Collections;

public class CompactList<T> : IList<T>
{
    T[] _items;

    public int Length => _items.Length;
    public int Count => _items.Length;
    public bool IsReadOnly => false;
    public T this[int index]
    {
        get
        {
            try
            {
                return _items[index];
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
        set
        {
            try
            {
                _items[index] = value;
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }

    public CompactList()
    {
        _items = Array.Empty<T>();
    }
    public CompactList(IEnumerable<T> collection)
    {
        _items = collection.ToArray();
    }
    public CompactList(params T[] temporalArray)
    {
        _items = temporalArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        var neo = new T[_items.Length + 1];
        Array.Copy(_items, neo, _items.Length);
        neo[_items.Length] = item;
        _items = neo;
    }
    bool ICollection<T>.TryAdd(T item) { Add(item); return true; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRange(IEnumerable<T> items)
    {
        var neo = new T[_items.Length + items.Count()];
        Array.Copy(_items, neo, _items.Length);
        int c = _items.Length;
        foreach (var item in items) neo[c++] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Remove(int at)
    {
        if (_items.Length <= at) throw new ArgumentOutOfRangeException(nameof(at));
        T r = _items[at];
        var neo = new T[_items.Length - 1];
        Array.Copy(_items, neo, at);
        Array.Copy(_items, at + 1, neo, at, neo.Length - at);
        return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => _items = Array.Empty<T>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan() => _items.AsSpan();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan(Index startIndex) => _items.AsSpan(startIndex);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan(int start) => _items.AsSpan(start);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan(Range range) => _items.AsSpan(range);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan(int start, int length) => _items.AsSpan(start, length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_items).GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetIndex(T of)
    {
        int r = -1;
        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i]!.Equals(of)) r = i;
        }
        return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(int index, T item)
    {
        var neo = new T[_items.Length + 1];
        Array.Copy(_items, neo, index);
        neo[index] = item;
        Array.Copy(_items, index, neo, index + 1, _items.Length - index);
        _items = neo;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T item)
    {
        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i]!.Equals(item)) return true;
        }
        return false;
    }

    public void Copy(Span<T> to, ref int index)
    {
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
        foreach (var item in _items)
        {
            try
            {
                to[index++] = item;
            }
            catch (IndexOutOfRangeException e)
            {
                throw new ArgumentException($"コピー元の{typeof(List<T>)}の要素数が使用可能領域を超えています。", e);
            }
        }
    }

    public bool TryRemove(T item)
    {
        int i = ((System.Collections.Generic.IList<T>)this).IndexOf(item);
        if (i > 0)
        {
            ((System.Collections.Generic.IList<T>)this).RemoveAt(i);
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool TryGetValue(int index, [MaybeNullWhen(false)] out T value)
    {
        if (0 <= index && index < _items.Length)
        {
            value = _items[index]!;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public bool TrySetValue(int index, T value)
    {
        if (0 <= index && index < _items.Length)
        {
            _items[index] = value;
            return true;
        }
        else
        {
            return false;
        }
    }
}
