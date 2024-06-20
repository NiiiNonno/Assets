using System;
using System.Collections.Generic;
using System.Text;

namespace Nonno.Assets.Collections;
public readonly record struct LongIndex(long Value)
{
    public long GetOffset(long length)
    {
        if (Value >= 0) return Value;
        else return length + Value;
    }

    public static implicit operator LongIndex(Index index) => new(index.Value);
    public static explicit operator Index(LongIndex index) => new((int)index.Value);
}

public readonly record struct LongRange(LongIndex Start, LongIndex End)
{
    public (long offset, long length) GetOffsetAndLength(long length)
    {
        var r =  (Start.Value, End.Value) switch
        {
            ( >= 0, >= 0) => (Start.Value, End.Value - Start.Value),
            ( >= 0, _) => (Start.Value, length + End.Value - Start.Value),
            (_, >= 0) => (length + Start.Value, End.Value - Start.Value - length),
            _ => (length + Start.Value, End.Value - Start.Value),
        };
        if (r.Item2 < 0) throw new IndexOutOfRangeException("指定の長さの場合範囲長度が負になります。");
        return r;
    }

    public static implicit operator LongRange(Range range) => new(range.Start, range.End);
    public static explicit operator Range(LongRange range) => new((Index)range.Start, (Index)range.End);
}
