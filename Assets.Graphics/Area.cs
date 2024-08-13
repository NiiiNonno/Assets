using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets;
public readonly ref struct Area<T> where T : unmanaged
{
    readonly ref T _r; // reference
    readonly int _l; // length
    readonly int _i; // interval
    readonly int _c;

    public unsafe Area(T* reference, int length, int interval, int count)
    {
        _r = ref Unsafe.AsRef<T>(reference);
        _l = length;
        _i = interval;
        _c = count;
    }
    public Area(ref T reference, int length, int interval, int count)
    {
        _r = ref reference;
        _l = length;
        _i = interval;
        _c = count;
    }

    public unsafe Area<T> this[Range r0, Range r1]
    {
        get
        {
            var (o0, l0) = r0.GetOffsetAndLength(_l);
            var (o1, l1) = r1.GetOffsetAndLength(_c);
            fixed (T* r = &_r)
            {
                return new Area<T>(r + o0 + o1 * _i, l0, _i, l1);
            }
        }
    }
    public unsafe T this[Index i0, Index i1]
    {
        get
        {
            fixed (T* r = &_r)
            {
                return *(r + i0.GetOffset(_l) + i1.GetOffset(_c) * _i);
            }
        }
        set
        {
            fixed (T* r = &_r)
            {
                *(r + i0.GetOffset(_l) + i1.GetOffset(_c) * _i) = value;
            }
        }
    }
    public unsafe T this[int i0, int i1]
    {
        get
        {
            if (unchecked((uint)i0) >= unchecked((uint)_l)) throw new IndexOutOfRangeException();
            if (unchecked((uint)i1) >= unchecked((uint)_c)) throw new IndexOutOfRangeException();
            fixed (T* r = &_r)
            {
                return *(r + i0 + i1 * _i);
            }
        }
        set
        {
            if (unchecked((uint)i0) >= unchecked((uint)_l)) throw new IndexOutOfRangeException();
            if (unchecked((uint)i1) >= unchecked((uint)_c)) throw new IndexOutOfRangeException();
            fixed (T* r = &_r)
            {
                *(r + i0 + i1 * _i) = value;
            }
        }
    }
    public unsafe Span<T> this[Index i]
    {
        get
        {
            fixed (T* r = &_r)
            {
                return new(r + i.GetOffset(_c) * _i, _l);
            }
        }
    }
    public unsafe Span<T> this[int i]
    {
        get
        {
            if (unchecked((uint)i) >= unchecked((uint)_c)) throw new IndexOutOfRangeException();
            fixed (T* r = &_r)
            {
                return new(r + i * _i, _l);
            }
        }
    }
}

public static class AreaExtentions
{
    public unsafe static Area<T> AsSpan<T>(this T[,] @this) where T : unmanaged
    {
        fixed (T* p = @this)
        {
            return new Area<T>(p, @this.GetLength(0), @this.GetLength(0), @this.GetLength(1));
        }
    }
}

