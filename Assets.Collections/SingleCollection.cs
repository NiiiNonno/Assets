using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Collections;
public struct SingleCollection<T>
{
    T? _value;

    public void Add(T item)
    {
        if (_value == null) _value = item;
        else ThrowHelper.InvalidOperation();
    }

    public void Remove(T item)
    {
        if (EqualityComparer<T>.Default.Equals(_value, item)) _value = default;
        else ThrowHelper.InvalidOperation();
    }

    public bool Contains(T item) => EqualityComparer<T>.Default.Equals(item, _value);

    public T Single() => _value ?? throw new InvalidOperationException();
    public T? SingleOrDefault() => _value;
}

public struct BoundSingleCollection<T> : IBoundCollection<T>
{
    T? _value;

    public int Count => _value != null ? 1 : 0;

    public event IBoundCollection<T>.ItemRemovedEventHandler? ItemRemoved;
    public event IBoundCollection<T>.ItemAddedEventHandler? ItemAdded;
    public event IBoundCollection<T>.ClearedEventHandler? Cleared;

    public void Add(T item)
    {
        if (_value == null) _value = item;
        else ThrowHelper.InvalidOperation();
        ItemAdded?.Invoke(null, new IBoundCollection<T>.ItemAddedEventArgs(item));
    }
    public bool TryAdd(T item)
    {
        if (_value == null) 
        {
            _value = item;
            ItemAdded?.Invoke(null, new IBoundCollection<T>.ItemAddedEventArgs(item));
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Remove(T item)
    {
        if (EqualityComparer<T>.Default.Equals(_value, item)) _value = default;
        else ThrowHelper.InvalidOperation();
        ItemRemoved?.Invoke(null, new IBoundCollection<T>.ItemRemovedEventArgs(item));
    }
    public bool TryRemove(T item)
    {
        if (EqualityComparer<T>.Default.Equals(_value, item))
        {
            _value = default;
            ItemRemoved?.Invoke(null, new IBoundCollection<T>.ItemRemovedEventArgs(item));
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool Contains(T item) => EqualityComparer<T>.Default.Equals(item, _value);

    public T Single() => _value ?? throw new InvalidOperationException();
    public T? SingleOrDefault() => _value;

    public void Copy(Span<T> to, ref int index)
    {
        if (_value != null) to[index++] = _value;
    }

    public void Clear()
    {
        _value = default;
        Cleared?.Invoke(null, new());
    }

    public IEnumerator<T> GetEnumerator()
    {
        if (_value != null) yield return _value;
    }
}
