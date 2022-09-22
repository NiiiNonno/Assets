// 令和弐年大暑確認済。
using System.Collections;

namespace Nonno.Assets.Collections;

/// <summary>
/// 弱参照コレクションを表します。
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TCollection"></typeparam>
public class Note<T, TCollection> : IEnumerable<T> where T : class where TCollection : System.Collections.Generic.ICollection<WeakReference<T>>
{
    readonly TCollection _collection;

    public int Count => _collection.Where(x => x.TryGetTarget(out var _)).Count();
    public bool IsReadOnly => _collection.IsReadOnly;

    public Note(Constructor<TCollection> constructor)
    {
        _collection = constructor();
    }

    public bool Add(T item)
    {
        _collection.Add(new(item));
        return true;
    }

    public void Clear() => _collection.Clear();

    public bool Contains(T item) => _collection.Where(x => x.TryGetTarget(out var target) && ReferenceEquals(item, target)).Any();

    public void CopyTo(T[] array, int arrayIndex)
    {
        foreach (var item in _collection)
        {
            if (item.TryGetTarget(out var target)) array[arrayIndex++] = target;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var item in _collection)
        {
            if (item.TryGetTarget(out var target)) yield return target;
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Remove(T item)
    {
        WeakReference<T> item_;
        foreach (var aEItem in _collection)
        {
            if (aEItem.TryGetTarget(out var target))
            {
                if (ReferenceEquals(item, target))
                {
                    item_ = aEItem;
                    goto fin;
                }
            }
        }
        throw new Exception();

        fin:
        return _collection.Remove(item_);
    }
}

public class Note<T> : Note<T, List<WeakReference<T>>> where T : class
{
    public Note() : base(() => new()) { }
}

public class StackNote<T> : Note<T, StackCollection<WeakReference<T>>> where T : class
{
    public StackNote() : base(() => new()) { }
}
