namespace Nonno.Assets.Collections;

public interface IBoundCollection<T> : ICollection<T>
{
    public event ItemRemovedEventHandler? ItemRemoved;
    public delegate void ItemRemovedEventHandler(object? sender, ItemRemovedEventArgs e);
    public record ItemRemovedEventArgs(T Old);
    public event ItemAddedEventHandler? ItemAdded;
    public delegate void ItemAddedEventHandler(object? sender, ItemAddedEventArgs e);
    public record ItemAddedEventArgs(T Neo);
    public event ClearedEventHandler? Cleared;
    public delegate void ClearedEventHandler(object? sender, ClearedEventArgs e);
    public record ClearedEventArgs();
}

public class BoundCollection<T> : IBoundCollection<T>
{
    readonly ICollection<T> _collection;

    public int Count => _collection.Count;
    public event IBoundCollection<T>.ItemRemovedEventHandler? ItemRemoved;
    public event IBoundCollection<T>.ItemAddedEventHandler? ItemAdded;
    public event IBoundCollection<T>.ClearedEventHandler? Cleared;

    internal BoundCollection(ICollection<T> collection)
    {
        _collection = collection;
    }
    public BoundCollection(Constructor<ICollection<T>> collectionConstructor) : this(collectionConstructor()) { }

    public bool Contains(T item) => _collection.Contains(item);

    public bool TryAdd(T item)
    {
        if (_collection.TryAdd(item))
        {
            ItemAdded?.Invoke(this, new(item));
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool TryRemove(T item)
    {
        if (_collection.TryRemove(item))
        {
            ItemRemoved?.Invoke(this, new(item));
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Clear()
    {
        _collection.Clear();

        Cleared?.Invoke(this, new());
    }

    public void Copy(Span<T> to, ref int index) => _collection.Copy(to, ref index);

    public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();
}
