#if NET5_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Collections;
public readonly struct ListArray<T>
{
    readonly T[] _arr;
    readonly int[] _counts;
    readonly int _expo;

    public int Length => _counts.Length;

    public ListArray(int length, int capacity_expo)
    {
        _arr = new T[length << capacity_expo];
        _counts = new int[length];
        _expo = capacity_expo;
    }

    public SubList this[int index]
    {
        get
        {
            return new(_arr.AsSpan()[(index << _expo)..((index + 1) << _expo)], ref _counts[index]);
        }
    }

    public readonly ref struct SubList
    {
        readonly Span<T> _span;
        readonly ref int _count;

        public SubList(Span<T> span, ref int count)
        {
            _span = span;
            _count = count;
        }

        public bool TryAdd(T item)
        {
            if (_count >= _span.Length) return false;
            _span[_count] = item;
            _count++;
            return true;
        }
        public bool TryRemove(T item)
        {
            for (int i = 0; i < _count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(_span[i], item))
                {
                    _span[(i + 1)..].CopyTo(_span[i..]);
                    _count--;
                    return true;
                }
            }
            return false;
        }

        public Span<T> AsSpan() => _span[.._count];
    }
}
#endif