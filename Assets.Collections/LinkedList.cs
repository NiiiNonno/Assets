using SysGC = System.Collections.Generic;

namespace Nonno.Assets.Collections;

public class LinkedList<T> : SysGC::LinkedList<T>, ICollection<T>
{
    public void Copy(Span<T> to, ref int index)
    {
        foreach (var item in this) to[index++] = item;
    }

    public bool TryRemove(T item) => Remove(item);

    bool ICollection<T>.TryAdd(T item) => throw new NotImplementedException();
}
