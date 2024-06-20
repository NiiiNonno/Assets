using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Nonno.Assets.Collections;
public readonly struct Memory<T, TValue> where T : unmanaged
{
    readonly T[] _array;
    readonly long _byteOffset;
    readonly long _structCount;

    public unsafe Span<TValue> Span
    {
        get
        {
            fixed (void* p = _array[(int)_byteOffset..])
            {
                return new Span<TValue>(p, (int)_structCount);
            }
        }
    }
    public Memory<T, TValue> this[Range range] => this[(LongRange)range];
    public Memory<T, TValue> this[LongRange range]
    {
        get
        {
            var (o, l) = range.GetOffsetAndLength((int)_structCount);
            return new(_array, o * Unsafe.SizeOf<T>(), l);
        }
    }

    public Memory(T[] array, long byteOffset = 0)
    {
        var len = array.Length * Unsafe.SizeOf<T>() / Unsafe.SizeOf<TValue>();

        _array = array;
        _byteOffset = byteOffset;
        _structCount = len;
    }
    Memory(T[] array, long byteOffset, long structCount) : this(array, byteOffset)
    {
        _structCount = structCount;
    }
}

public readonly struct ReadOnlyMemory<T, TValue> where T : unmanaged
{
    readonly Memory<T, TValue> _memory;

    public ReadOnlySpan<TValue> Span => _memory.Span;

    public ReadOnlyMemory<T, TValue> this[Range range] => new(_memory[range]);
    public ReadOnlyMemory<T, TValue> this[LongRange range] => new(_memory[range]);

    ReadOnlyMemory(Memory<T, TValue> memory)
    {
        _memory = memory;
    }

    public static implicit operator ReadOnlyMemory<T, TValue>(Memory<T, TValue> memory) => new(memory);
}
