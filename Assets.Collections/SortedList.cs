using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Nonno.Assets.Collections;
public class SortedList<T> : ICollection<T> where T : IComparable<T>
{
    readonly ArrayList<T> _list;

    public int Count => _list.Count;

    public SortedList()
    {
        _list = new();
    }

    public void Clear() => _list.Clear();

    public int GetIndex(T of) => TryGetIndex(of, out var r) ? r : -1;
    public bool TryGetIndex(T of, out int index)
    {
        int min = 0;
        int max = _list.Count;

        while (true)
        {
            var mid = ((max - min) >> 1) + min;
            switch (of.CompareTo(_list[mid]))
            {
            case 0:
                index = mid;
                return true;
            case < 0:
                min = mid + 1;
                if (min < max) continue;
                index = min;
                return false;
            case > 0:
                max = mid;
                if (min < max) continue;
                index = min;
                return false;
            }
        }
    }

    public bool Contains(T item) => TryGetIndex(of: item, out _);

    public bool TryAdd(T item)
    {
        if (TryGetIndex(item, out var i)) return false;
        _list.Insert(i, item);
        return true;
    }

    public bool TryRemove(T item)
    {
        if (!TryGetIndex(item, out var i)) return false;
        var t = _list.Remove(at: i);
        Debug.Assert(EqualityComparer<T>.Default.Equals(t, item));
        return true;
    }

    public void Copy(Span<T> to, ref int index) => _list.Copy(to, ref index);

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
}
