using System.Collections;
using SysGC = System.Collections.Generic;

namespace Nonno.Assets.Collections;

public interface ICollection<T> : SysGC.ICollection<T>, ISet<T>
{
    bool SysGC::ICollection<T>.IsReadOnly => false;
    void SysGC::ICollection<T>.CopyTo(T[] array, int arrayIndex) => Copy(to: array, ref arrayIndex);
    void Copy(Span<T> to, ref int index);
    void SysGC::ICollection<T>.Add(T item)
    {
        if (!TryAdd(item)) throw new Exception("要素の追加に失敗しました。");
    }
    bool TryAdd(T item);
    new void Remove(T item)
    {
        if (!TryRemove(item)) throw new Exception("要素の削除に失敗しました。");
    }
    bool TryRemove(T item);
    bool SysGC::ICollection<T>.Remove(T item) => TryRemove(item);
}

public interface IAsyncCollection<T> : IAsyncEnumerable<T>
{
    int Count { get; }
    Task AddAsync(T item);
    Task RemoveAsync(T item);
    Task ClearAsync();
    ValueTask<bool> TryAddAsync(T item);
    ValueTask<bool> TryRemoveAsync(T item);
    ValueTask<bool> ContainsAsync(T item);
}

public interface IReadOnlyCollection<T> : SysGC.IReadOnlyCollection<T>
{
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
