using System.Diagnostics.CodeAnalysis;

namespace Nonno.Assets.Collections;

public interface IBoundList<T> : IBoundCollection<T>, IList<T>
{
    new event ItemRemovedEventHandler? ItemRemoved;
    new delegate void ItemRemovedEventHandler(object? sender, ItemRemovedEventArgs e);
    new record ItemRemovedEventArgs(int Index, T Old);
    event IBoundCollection<T>.ItemRemovedEventHandler? IBoundCollection<T>.ItemRemoved
    {
        add
        {
            if (value != null) ItemRemoved += value.Handle;
        }
        remove
        {
            if (value != null) ItemRemoved -= value.Handle;
        }
    }
    new event ItemAddedEventHandler? ItemAdded;
    new delegate void ItemAddedEventHandler(object? sender, ItemAddedEventArgs e);
    new record ItemAddedEventArgs(int Index, T Neo);
    event IBoundCollection<T>.ItemAddedEventHandler? IBoundCollection<T>.ItemAdded
    {
        add
        {
            if (value != null) ItemAdded += value.Handle;
        }
        remove
        {
            if (value != null) ItemAdded -= value.Handle;
        }
    }
    event ItemReplacedEventHandler? ItemReplaced;
    delegate void ItemReplacedEventHandler(object? sender, ItemReplacedEventArgs e);
    record ItemReplacedEventArgs(int Index, T Old, T Neo);
}

public class BoundList<T> : IBoundList<T>
{
    readonly IList<T> _list;

    public int Count => _list.Count;
    public event IBoundList<T>.ItemRemovedEventHandler? ItemRemoved;
    public event IBoundList<T>.ItemAddedEventHandler? ItemAdded;
    public event IBoundList<T>.ItemReplacedEventHandler? ItemReplaced;
    public event IBoundCollection<T>.ClearedEventHandler? Cleared;
    public T this[int index]
    {
        get => _list[index];
        set
        {
            var old = _list[index];
            _list[index] = value;
            ItemReplaced?.Invoke(this, new(index, old, value));
        }
    }

    internal BoundList(IList<T> list)
    {
        _list = list;
    }
    public BoundList(Constructor<IList<T>> listConstructor) : this(listConstructor()) { }

    public int GetIndex(T of) => _list.GetIndex(of);

    public bool Contains(T item) => _list.Contains(item);

    public void Insert(int index, T item)
    {
        ItemAdded?.Invoke(this, new(index, item));

        _list.Insert(index, item);
    }

    public T Remove(int at)
    {
        var r = _list.Remove(at);

        ItemRemoved?.Invoke(this, new(at, r));

        return r;
    }

    public bool TryAdd(T item)
    {
        if (_list.TryAdd(item))
        {
            ItemAdded?.Invoke(this, new(Count, item));
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool TryRemove(T item)
    {
        int index = GetIndex(of: item);
        if (_list.TryRemove(item))
        {
            ItemRemoved?.Invoke(this, new(index, item));
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Clear()
    {
        Cleared?.Invoke(this, new());

        _list.Clear();
    }

    public void Copy(Span<T> to, ref int index) => _list.Copy(to, ref index);

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    public bool TryGetValue(int index, [MaybeNullWhen(false)] out T value) => _list.TryGetValue(index, out value);

    public bool TrySetValue(int index, T value)
    {
        if (_list.TryGetValue(index, out var old) && _list.TrySetValue(index, value))
        {
            ItemReplaced?.Invoke(this, new(index, old, value));
            return true;
        }
        return false;
    }
}

internal static partial class TrickyExtends
{
    public static void Handle<T>(this IBoundCollection<T>.ItemAddedEventHandler @this, object? sender, IBoundList<T>.ItemAddedEventArgs e) => @this(sender, new(e.Neo));
    public static void Handle<T>(this IBoundCollection<T>.ItemRemovedEventHandler @this, object? sender, IBoundList<T>.ItemRemovedEventArgs e) => @this(sender, new(e.Old));
}
