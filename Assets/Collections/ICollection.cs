using System.Collections;
using SysGC = System.Collections.Generic;

namespace Nonno.Assets.Collections;

public interface ICollection<T> : SysGC.ICollection<T>
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
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public interface IReadOnlyCollection<T> : SysGC.IReadOnlyCollection<T>
{
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
