using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UInt16 = System.UInt16;
using static Nonno.Assets.ConstantValues;

namespace Nonno.Assets;
public enum BunanCode : byte
{
    無, 日, 月, 木, 火, 土, 金, 水,
    〇, 一, 二, 三, 四, 五, 六, 七,
    八, 九, 十, 廿, 入, 下, 右, 出,
    生, 人, 爪, 心, 口, 女, 尸, 難,

    N = 無,
    A = 日,
    B = 月,
    D = 木,
    F = 火,
    G = 土,
    C = 金,
    E = 水,
    V0 = 〇,
    V1 = 一,
    V2 = 二,
    V3 = 三,
    V4 = 四,
    V5 = 五,
    V6 = 六,
    V7 = 七,
    V8 = 八,
    V9 = 九,
    V10 = 十,
    V20 = 廿,
    Ins = 入,//inside 
    Dwn = 下,//down
    Rit = 右,//right
    Out = 出,//outside
    H = 生,
    I = 人,
    L = 爪,
    P = 心,
    R = 口,
    V = 女,
    S = 尸,
    X = 難,
}

public class BunanString : IEnumerable<BunanCode>
{
    readonly BunanStrip8[] _strips;
    readonly int _len;
    readonly int _rem;

    public int Length => _len;
    public BunanCode this[int index] => index < _len ? _strips[index >> 3][index & 0b111] : throw new IndexOutOfRangeException();

    public BunanString(params BunanStrip8[] strip8Params)
    {
        _strips = strip8Params;
    }
    public BunanString(ReadOnlySpan<BunanCode> codes)
    {
        _len = codes.Length;

        if ((_rem = codes.Length & 0b111) == 0)
        {
            _strips = new BunanStrip8[codes.Length >> 3];

            int c = 0;
            for (int i = 0; i < _strips.Length; i++)
            {
                _strips[i] = new(codes[c++], codes[c++], codes[c++], codes[c++], codes[c++], codes[c++], codes[c++], codes[c++]);
            }
        }
        else
        {
            _strips = new BunanStrip8[(codes.Length >> 3) + 1];

            int c = 0;
            int l = _strips.Length - 1;
            for (int i = 0; i < l; i++)
            {
                _strips[i] = new(codes[c++], codes[c++], codes[c++], codes[c++], codes[c++], codes[c++], codes[c++], codes[c++]);
            }
            switch (_rem)
            {
            case 0:
                _strips[l] = new(codes[c]);
                break;
            case 1:
                _strips[l] = new(codes[c++], codes[c]);
                break;
            case 2:
                _strips[l] = new(codes[c++], codes[c++], codes[c]);
                break;
            case 3:
                _strips[l] = new(codes[c++], codes[c++], codes[c++], codes[c]);
                break;
            case 4:
                _strips[l] = new(codes[c++], codes[c++], codes[c++], codes[c++], codes[c]);
                break;
            case 5:
                _strips[l] = new(codes[c++], codes[c++], codes[c++], codes[c++], codes[c++], codes[c]);
                break;
            case 6:
                _strips[l] = new(codes[c++], codes[c++], codes[c++], codes[c++], codes[c++], codes[c++], codes[c]);
                break;
            case 7:
                _strips[l] = new(codes[c++], codes[c++], codes[c++], codes[c++], codes[c++], codes[c++], codes[c++], codes[c]);
                break;
            }
        }
    }
    public BunanString(params BunanCode[] codes) : this((ReadOnlySpan<BunanCode>)codes) { }

    public IEnumerator<BunanCode> GetEnumerator()
    {
        var l = _strips.Length - 1;
        for (int i = 0; i < l; i++)
        {
            var v = _strips[i];
            yield return v.Code0;
            yield return v.Code1;
            yield return v.Code2;
            yield return v.Code3;
            yield return v.Code4;
            yield return v.Code5;
            yield return v.Code6;
            yield return v.Code7;
        }

        var last = _strips[l];
        for (int i = 0; i < _rem; i++) yield return last[i];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 2)]
public readonly struct BunanStrip3
{
    public const int MASK = 0b11111;

    public readonly UInt16 v;

    public BunanCode Code0 => (BunanCode)(v & MASK);
    public BunanCode Code1 => (BunanCode)((v >> 5) & MASK);
    public BunanCode Code2 => (BunanCode)((v >> 10) & MASK);
    public BunanCode this[int index] => index switch
    {
        0 => Code0, 1 => Code1, 2 => Code2,
        _ => throw new IndexOutOfRangeException()
    };
    public bool Flag => (v >> 15) != 0;

    internal BunanStrip3(UInt16 value)
    {
        v = value;
    }
    public BunanStrip3(BunanCode code0, BunanCode code1, BunanCode code2)
    {
        v = (UInt16)code2;
        v <<= 5;
        v |= (UInt16)code1;
        v <<= 5;
        v |= (UInt16)code0;
    }
    public BunanStrip3(BunanCode code0, BunanCode code1, BunanCode code2, bool flag)
    {
        v = flag ? (UInt16)0b100000 : (UInt16)0;
        v = (UInt16)code2;
        v <<= 5;
        v |= (UInt16)code1;
        v <<= 5;
        v |= (UInt16)code0;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 5)]
public readonly struct BunanStrip8
{
    /*
     *         |- - 7 - -|- - 6 - -|- - 5 - -|- - 4 - -|- - 3 - -|- - 2 - -|- - 1 - -|- - 0 - -| 
     * high << |- - - -4- - - -|- - - -3- - - -|- - - -2- - - -|- - - -1- - - -|- - - -0- - - -| >> low
     */

    public readonly byte v0, v1, v2, v3, v4;

    public BunanCode Code0 => (BunanCode)(v0 & 0b11111);
    public BunanCode Code1 => (BunanCode)(v0 >> 5 | ((v1 & 0b11) << 3));
    public BunanCode Code2 => (BunanCode)(v1 >> 2 & 0b11111);
    public BunanCode Code3 => (BunanCode)(v1 >> 7 | ((v2 & 0b1111) << 1));
    public BunanCode Code4 => (BunanCode)(v2 >> 4 | ((v3 & 0b1) << 4));
    public BunanCode Code5 => (BunanCode)(v3 >> 1 & 0b11111);
    public BunanCode Code6 => (BunanCode)(v3 >> 6 | ((v4 & 0b111) << 2));
    public BunanCode Code7 => (BunanCode)(v4 >> 3);
    public BunanCode this[int index] => index switch
    {
        0 => Code0, 1 => Code1, 2 => Code2, 3 => Code3, 4 => Code4, 5 => Code5, 6 => Code6, 7 => Code7,
        _ => throw new IndexOutOfRangeException(),
    };

    public BunanStrip8(byte value0, byte value1, byte value2, byte value3, byte value4)
    {
        v0 = value0;
        v1 = value1;
        v2 = value2;
        v3 = value3;
        v4 = value4;
    }
    public BunanStrip8(BunanCode code0 = default, BunanCode code1 = default, BunanCode code2 = default, BunanCode code3 = default, BunanCode code4 = default, BunanCode code5 = default, BunanCode code6 = default, BunanCode code7 = default) : this((int)code0, (int)code1, (int)code2, (int)code3, (int)code4, (int)code5, (int)code6, (int)code7) { }
    public BunanStrip8(int code0 = default, int code1 = default, int code2 = default, int code3 = default, int code4 = default, int code5 = default, int code6 = default, int code7 = default)
    {
        unchecked
        {
            v0 = (byte)(code0 | code1 << 5);
            v1 = (byte)(code1 >> 3 | code2 << 2 | code3 << 7);
            v2 = (byte)(code3 >> 1 | code4 << 4);
            v3 = (byte)(code4 >> 4 | code5 << 1 | code6 << 6);
            v4 = (byte)(code6 >> 2 | code7 << 3);
        }
    }
}

partial class Utils
{
    public static BunanString ToBunanString(this uint value, bool hexadecimally = false)
    {
        if (hexadecimally)
        {
            Span<BunanCode> span = stackalloc BunanCode[NUMBER_STRING_MAX_LENGTH_DECIMAL_INT32];
            int i = 0;

            while (value != 0)
            {
                var v = value >> 4;
                var rem = value & 0b1111;
                span[i++] = (BunanCode)(rem + 8);
                value = v;
            }

            return new(span[..i]);
        }
        else
        {
            Span<BunanCode> span = stackalloc BunanCode[NUMBER_STRING_MAX_LENGTH_DECIMAL_INT32];
            int i = 0;
            
            while (value != 0)
            {
                var v = value / 10;
                var rem = value - v * 10;
                span[i++] = (BunanCode)(rem + 8);
                value = v;
            }

            return new(span[..i]);
        }
    }
}
