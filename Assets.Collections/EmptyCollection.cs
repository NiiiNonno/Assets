// 令和弐年大暑確認済。
using System.Diagnostics.CodeAnalysis;

namespace Nonno.Assets.Collections;

public class EmptyCollection<T> : IReadOnlyList<T>, IReadOnlySet<T>
{
    public static readonly EmptyCollection<T> INSTANCE = new();

    int System.Collections.Generic.IReadOnlyCollection<T>.Count => 0;
    T System.Collections.Generic.IReadOnlyList<T>.this[int index] => throw new IndexOutOfRangeException();

    bool IReadOnlySet<T>.Contains(T item) => false;
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => EmptyEnumerator<T>.INSTANCE;
    bool IReadOnlySet<T>.IsProperSubsetOf(IEnumerable<T> other)
    {
        foreach (var _ in other) return true;
        return false;
    }
    bool IReadOnlySet<T>.IsProperSupersetOf(IEnumerable<T> other) => false;
    bool IReadOnlySet<T>.IsSubsetOf(IEnumerable<T> other) => true;
    bool IReadOnlySet<T>.IsSupersetOf(IEnumerable<T> other)
    {
        foreach (var _ in other) return false;
        return true;
    }
    bool IReadOnlySet<T>.Overlaps(IEnumerable<T> other) => false;
    bool IReadOnlySet<T>.SetEquals(IEnumerable<T> other) => false;
    bool IReadOnlyList<T>.TryGetValue(int index, [MaybeNullWhen(false)] out T value)
    {
        value = default;
        return false;
    }
}

public class EmptyKeyValueCollection<TKey, TValue> : IReadOnlyList<KeyValuePair<TKey, TValue>>, IReadOnlySet<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>
{
    public static readonly EmptyKeyValueCollection<TKey, TValue> INSTANCE = new();

    int System.Collections.Generic.IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => 0;
    IEnumerable<TKey> System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>.Keys => EmptyCollection<TKey>.INSTANCE;
    IEnumerable<TValue> System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>.Values => EmptyCollection<TValue>.INSTANCE;
    KeyValuePair<TKey, TValue> System.Collections.Generic.IReadOnlyList<KeyValuePair<TKey, TValue>>.this[int index] => throw new IndexOutOfRangeException();

    bool IReadOnlySet<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => false;
    bool System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key) => false;
    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => EmptyEnumerator<KeyValuePair<TKey, TValue>>.INSTANCE;
    bool IReadOnlySet<KeyValuePair<TKey, TValue>>.IsProperSubsetOf(IEnumerable<KeyValuePair<TKey, TValue>> other)
    {
        foreach (var _ in other) return true;
        return false;
    }
    bool IReadOnlySet<KeyValuePair<TKey, TValue>>.IsProperSupersetOf(IEnumerable<KeyValuePair<TKey, TValue>> other) => false;
    bool IReadOnlySet<KeyValuePair<TKey, TValue>>.IsSubsetOf(IEnumerable<KeyValuePair<TKey, TValue>> other) => true;
    bool IReadOnlySet<KeyValuePair<TKey, TValue>>.IsSupersetOf(IEnumerable<KeyValuePair<TKey, TValue>> other)
    {
        foreach (var _ in other) return false;
        return true;
    }
    bool IReadOnlySet<KeyValuePair<TKey, TValue>>.Overlaps(IEnumerable<KeyValuePair<TKey, TValue>> other) => false;
    bool IReadOnlySet<KeyValuePair<TKey, TValue>>.SetEquals(IEnumerable<KeyValuePair<TKey, TValue>> other) => false;
    bool System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
    {
        value = default!;
        return false;
    }
    bool IReadOnlyList<KeyValuePair<TKey, TValue>>.TryGetValue(int index, [MaybeNullWhen(false)] out KeyValuePair<TKey, TValue> value)
    {
        value = default;
        return false;
    }
}
