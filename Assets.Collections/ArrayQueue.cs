using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Nonno.Assets.Collections;
public class ArrayQueue<T> : ICollection<T>
{
    T[] _a;
    int _s, _e;
    int _m;
    Shift _l;

    public ArrayQueue(Shift capscity)
    {
        _a = [];

        Capacity = capscity;
    }

    public Shift Capacity
    {
        get => _l;
        set
        {
            if (_l == value) return;
            
            var neo = new T[value];
            var i = 0;
            Copy(to: neo, ref i);
            _a = neo;
            _s = 0;
            _e = i;
            _l = value;
            _m = _l - 1;
        }
    }
    public int Count => unchecked((_e - _s) & _m);

    public void Clear()
    {
        _s = _e = 0;
    }
    public bool Contains(T item) => TryGetIndex(of: item, out _);
    public void Copy(Span<T> to, ref int index)
    {
        var a = _a;
        var s = _s;
        var e = _e;
        if (_s > _e)
        {
            for (int i = s; i < a.Length; i++)
            {
                to[index++] = a[i];
            }
            for (int i = 0; i < e; i++)
            {
                to[index++] = a[i];
            }
        }
        else
        {
            for (int i = s; i < e; i++)
            {
                to[index++] = a[i];
            }
        }
    }
    public Enumerator GetEnumerator() => new(this);

    public bool Remove(T item)
    {
        if (!TryGetIndex(of: item, out var i)) return false;

        var a = _a.AsSpan();
        if (_e == 0)
        {
            var e = _e;
            _e = _m;
            if (i == _e) return true;
            a[(i + 1)..e].CopyTo(a[i..]);
            return true;
        }
        if (i < _e)
        {
            var e = _e;
            _e--;
            if (i == _e) return true;
            a[(i + 1)..e].CopyTo(a[i..]);
            return true;
        }
        else
        {
            var s = _s;
            _s = (_s + 1) & _m;
            if (i == s) return true;
            a[s..i].CopyTo(a[_s..]);
            return true;
        }
    }
    bool TryGetIndex(T of, out int index)
    {
        var a = _a;
        var s = _s;
        var e = _e;
        if (_s > _e)
        {
            for (index = s; index < a.Length; index++)
            {
                if (EqualityComparer<T>.Default.Equals(a[index], of)) return true;
            }
            for (index = 0; index < e; index++)
            {
                if (EqualityComparer<T>.Default.Equals(a[index], of)) return true;
            }
        }
        else
        {
            for (index = s; index < e; index++)
            {
                if (EqualityComparer<T>.Default.Equals(a[index], of)) return true;
            }
        }
        return false;
    }

    public void Enqueue(T item)
    {
        _e = (_e + 1) & _m;
        if (_s == _e) Capacity = new(Capacity.Exponent + 1);
    }
    public T Dequeue()
    {
        var r = _a[_s];
        _s = (_s + 1) & _m;
        return r;
    }

    bool ICollection<T>.TryAdd(T item)
    {
        Enqueue(item);
        return true;
    }
    bool ICollection<T>.TryRemove(T item) => Remove(item);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    public struct Enumerator(ArrayQueue<T> @base) : IEnumerator<T>
    {
        int _i = @base._s;

        public T Current => @base._a[_i];
        object IEnumerator.Current => @base._a[_i]!;

        public void Dispose() { }
        public bool MoveNext()
        {
            _i = (_i + 1) & @base._m;
            return @base._e != _i;
        }
        public void Reset() => _i = @base._s;
    }
}
