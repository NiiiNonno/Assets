using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Nonno.Assets.Collections;
public readonly struct PersistentList<T>
{
    readonly T?[]? _arr;
    readonly int _count;

    public int Count => _count;
    public int Capacity => _arr is null ? 0 : _arr.Length;
    public T this[int i]
    {
        get
        {
            Debug.Assert(i < _count);
            Debug.Assert(_arr is not null);
            var r = _arr[i];
            Debug.Assert(r is not null);
            return r;
        }
    }

    public PersistentList(int capacity)
    {
        _arr = new T[capacity];
        _count = 0;
    }
    private PersistentList(T?[] arr, int count)
    {
        _arr = arr;
        _count = count;
    }

    public PersistentList<T> Add(T item)
    {
        if (_arr is null)
        {
            return new(new T[] { item }, 1);
        }
        if (_arr.Length == _count || _arr[_count] is not null)
        {
            var neo = new T[_count << 1];
            Array.Copy(_arr, neo, _count);
            neo[_count] = item;
            return new(neo, _count + 1);
        }
        else
        {
            _arr[_count] = item;
            return new(_arr, _count + 1);
        }
    }
}
