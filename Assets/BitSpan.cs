using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Nonno.Assets;
public readonly ref struct BitSpan
{
    readonly Span<byte> _ref;
    readonly int _len;
    readonly int _ofs;

    public uint this[Range range]
    {
        get
        {
            var i_s = range.Start.GetOffset(_len);
            var i_e = range.End.GetOffset(_len);
            return GetValue(i_s, i_e, Endian.HostByteOrder);
        }
    }

    public unsafe uint GetValue(int start, int end, Endian endian)
    {
        var i_s8 = start >> 3;
        var i_sm = i_s8 << 3;
        var o_s = start - i_sm;
        var i_e8 = ((end - 1) >> 3) + 1;
        var i_em = i_e8 << 3;
        var o_e = i_em - end;
        var l_buf = i_e8 - i_s8;

        switch (l_buf)
        {
        case 0:
            return 0;
        case 1:
        case 2:
        case 3:
        case 4:
            uint b = default;
            fixed (byte* p = _ref) endian.Localize(p + i_s8, &b);
            uint m = ~(0xFFFF_FFFFu << (end - i_s8));
            return (b & m) >> o_s;
        default:
            throw new NotImplementedException();
        }
    }

    public static BitSpan Slice(Span<byte> bytes)
    {
        throw new NotImplementedException();
    }
}
