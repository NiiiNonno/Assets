namespace Nonno.Assets.Collections;

public abstract class WeakReferenceCollection<T> : ICollection<T> where T : class
{
    protected IList<WeakReference<T>> _list;

    public int Count
    {
        get
        {
            int r = 0;
            foreach (var reference in _list) if (reference.TryGetTarget(out var _)) r++;
            return r;
        }
    }

    public WeakReferenceCollection()
    {
        InitList();

        if (_list == null) throw new Exception("実装が不適切です。`InitList`呼び出し後の`_list`が`null`です。");
    }

    public void Clear() => _list.Clear();

    public bool Contains(T item)
    {
        foreach (var reference in _list)
        {
            if (reference.TryGetTarget(out var target) && target.Equals(item)) return true;
        }
        return false;
    }

    public void Copy(Span<T> to, ref int index)
    {
        foreach (var reference in _list)
        {
            if (reference.TryGetTarget(out var target)) to[index++] = target;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var reference in _list)
        {
            if (reference.TryGetTarget(out var target)) yield return target;
        }
    }

    public void Add(T item) => _ = TryAdd(item);
    public bool TryAdd(T item)
    {
        for (int i = 0; i < _list.Count; i++)
        {
            if (!_list[i].TryGetTarget(out var _))
            {
                _list[i].SetTarget(item);
                return true;
            }
        }

        _list.Add(new(item));
        return true;
    }

    public void Remove(T item) { }
    public bool TryRemove(T item) => true;

    protected abstract void InitList();
}

public sealed class WeakReferenceArrayCollection<T> : WeakReferenceCollection<T> where T : class
{
    protected override void InitList() => _list = new ArrayList<WeakReference<T>>();
}

public sealed class WeakReferenceCompactCollection<T> : WeakReferenceCollection<T> where T : class
{
    protected override void InitList() => _list = new CompactList<WeakReference<T>>();
}
