using System.Collections;
using System.Runtime.CompilerServices;

namespace Nonno.Assets.Collections;

public class CompactSet<T> : ISet<T>, IReadOnlySet<T>
{
    readonly CompactList<T> _items;

    public int Count => _items.Count;
    public bool IsReadOnly => false;

    public CompactSet()
    {
        _items = new();
    }
    public CompactSet(params T[] temporalArray)
    {
        _items = new(temporalArray);
    }
    public CompactSet(IReadOnlySet<T> set) : this()
    {
        _items.AddRange(set);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Add(T item)
    {
        if (!Contains(item))
        {
            _items.Add(item);
            return true;
        }
        else
        {
            return false;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void System.Collections.Generic.ICollection<T>.Add(T item) => _ = Add(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRange(System.Collections.Generic.IEnumerable<T> items) => _items.AddRange(items.Where(item => !Contains(item)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T item) => ((System.Collections.Generic.ICollection<T>)_items).Remove(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => _items.Clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T item) => _items.Contains(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(T[] array, int arrayIndex) => _items.AsSpan().CopyTo(array.AsSpan(arrayIndex));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T> AsSpan() => _items.AsSpan();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ExceptWith(System.Collections.Generic.IEnumerable<T> other)
    {
        foreach (var item in other) _ = Remove(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IntersectWith(System.Collections.Generic.IEnumerable<T> other)
    {
        foreach (var item in _items) if (!other.Contains(item)) _ = Remove(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsProperSubsetOf(System.Collections.Generic.IEnumerable<T> other) => IsSubsetOf(other) && other.Count() != Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsProperSupersetOf(System.Collections.Generic.IEnumerable<T> other) => IsSupersetOf(other) && other.Count() != Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSubsetOf(System.Collections.Generic.IEnumerable<T> other)
    {
        foreach (var item in other) if (!Contains(item)) return false;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSupersetOf(System.Collections.Generic.IEnumerable<T> other)
    {
        foreach (var item in _items) if (!other.Contains(item)) return false;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Overlaps(System.Collections.Generic.IEnumerable<T> other)
    {
        foreach (var item in other) if (Contains(item)) return true;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool SetEquals(System.Collections.Generic.IEnumerable<T> other) => IsSubsetOf(other) && IsSupersetOf(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SymmetricExceptWith(System.Collections.Generic.IEnumerable<T> other)
    {
        var toAdds = other.Except(this);
        ExceptWith(other);
        AddRange(toAdds);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnionWith(System.Collections.Generic.IEnumerable<T> other) => AddRange(other.Except(this));
}
