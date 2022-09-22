namespace Nonno.Assets;

public readonly struct Double32 : IEquatable<Double32>, IComparable<Double32>
{
    public const int DIGIT = 32;
    public static readonly Double32 ONE = new(0x0000_0001_0000_0000);
    public static readonly Shift DIGIT_SHIFT = new(DIGIT);

    readonly long _number;

    public long Number => _number;

    public Double32(long number)
    {
        _number = number;
    }

    public override bool Equals(object? obj) => obj is Double32 @double && Equals(@double);
    public bool Equals(Double32 other) => _number == other._number;
    public override int GetHashCode() => HashCode.Combine(_number);
    public int CompareTo(Double32 other) => _number.CompareTo(other._number);
    public override string ToString() => ((decimal)this).ToString();
    public string ToString(string? format = null)
    {
        switch (format)
        {
        case "X":
            {
                if ((_number & 0xFFFF_FFFF) == 0)
                {
                    return $"0x{_number >> DIGIT:X1}";
                }
                else
                {
                    string r = "0x" + _number.ToString("X9");
                    return r.Insert(r.Length - DIGIT / 4, ".").TrimEnd('0');
                }
            }
        case "X()":
            {
                if ((_number & 0xFFFF_FFFF) == 0)
                {
                    return $"0x{_number >> DIGIT:X1}({ToString()})";
                }
                else
                {
                    string r = "0x" + _number.ToString("X9");
                    return r.Insert(r.Length - DIGIT / 4, ".").TrimEnd('0') + ToString();
                }
            }
        default:
            {
                return ((decimal)this).ToString();
            }
        }
    }

    public static Double32 operator -(Double32 double32) => new(-double32._number);
    public static Double32 operator +(Double32 left, Double32 right) => new(left._number + right._number);
    public static Double32 operator -(Double32 left, Double32 right) => new(left._number - right._number);
    public static Double32 operator *(long left, Double32 right) => new(left * right._number);
    public static Double32 operator *(Shift left, Double32 right) => new(left * right._number);
    public static Double32 operator *(Double32 left, long right) => new(left._number * right);
    public static Double32 operator /(Double32 left, long right) => new(left._number / right);
    public static Double32 operator /(Double32 left, Shift right) => new(left._number / right);
    public static Double32 operator %(Double32 left, Double32 right) => new(left._number % right._number);
    public static Double32 operator <<(Double32 left, int right) => new(left._number << right);
    public static Double32 operator >>(Double32 left, int right) => new(left._number >> right);
    public static bool operator ==(Double32 left, Double32 right) => left.Equals(right);
    public static bool operator !=(Double32 left, Double32 right) => !(left == right);
    public static bool operator <(Double32 left, Double32 right) => left._number < right._number;
    public static bool operator >(Double32 left, Double32 right) => left._number > right._number;
    public static implicit operator Double32(byte @byte) => new((long)@byte << DIGIT);
    public static implicit operator Double32(sbyte @sbyte) => new((long)@sbyte << DIGIT);
    public static implicit operator Double32(short int16) => new((long)int16 << DIGIT);
    public static implicit operator Double32(ushort uint16) => new((long)uint16 << DIGIT);
    public static explicit operator Double32(int int32) => new(int32 << DIGIT);
    public static explicit operator Double32(uint uint32) => new(uint32 << DIGIT);
    public static explicit operator Double32(long int64) => new(int64 << DIGIT);
    public static explicit operator Double32(ulong uint64) => new((long)uint64 << DIGIT);
    public static implicit operator Double32(float single) => new((long)(DIGIT_SHIFT * single));
    public static implicit operator Double32(double @double) => new((long)(DIGIT_SHIFT * @double));
    public static implicit operator Double32(decimal @decimal) => new((long)(@decimal * ONE.Number));
    public static explicit operator byte(Double32 double32) => (byte)(double32._number >> DIGIT);
    public static explicit operator sbyte(Double32 double32) => (sbyte)(double32._number >> DIGIT);
    public static explicit operator short(Double32 double32) => (short)(double32._number >> DIGIT);
    public static explicit operator ushort(Double32 double32) => (ushort)(double32._number >> DIGIT);
    public static explicit operator int(Double32 double32) => (int)(double32._number >> DIGIT);
    public static explicit operator uint(Double32 double32) => (uint)(double32._number >> DIGIT);
    public static explicit operator long(Double32 double32) => double32._number >> DIGIT;
    public static explicit operator ulong(Double32 double32) => (ulong)(double32._number >> DIGIT);
    public static unsafe explicit operator float(Double32 double24) => (float)double24._number / DIGIT_SHIFT;
    public static unsafe explicit operator double(Double32 double24) => (double)double24._number / DIGIT_SHIFT;
    public static explicit operator decimal(Double32 double32) => (decimal)double32._number / ONE.Number;
}

public readonly struct Double24 : IEquatable<Double24>, IComparable<Double24>
{
    public const int DIGIT = 24;
    public static readonly Double24 ONE = new(0x0000_0000_0100_0000);
    public static readonly Shift DIGIT_SHIFT = new(DIGIT);

    readonly long _number;

    public long Number => _number;

    public Double24(long number)
    {
        _number = number;
    }

    public override bool Equals(object? obj) => obj is Double24 @double && Equals(@double);
    public bool Equals(Double24 other) => _number == other._number;
    public override int GetHashCode() => HashCode.Combine(_number);
    public int CompareTo(Double24 other) => _number.CompareTo(other._number);
    public override string ToString() => ((decimal)this).ToString();
    public string ToString(string? format = null)
    {
        switch (format)
        {
        case "X":
            {
                if ((_number & 0x00FF_FFFF) == 0)
                {
                    return $"0x{_number >> DIGIT:X1}";
                }
                else
                {
                    string r = "0x" + _number.ToString("X7");
                    return r.Insert(r.Length - DIGIT / 4, ".").TrimEnd('0');
                }
            }
        case "X()":
            {
                if ((_number & 0x00FF_FFFF) == 0)
                {
                    return $"0x{_number >> DIGIT:X1}({ToString()})";
                }
                else
                {
                    string r = "0x" + _number.ToString("X7");
                    return r.Insert(r.Length - DIGIT / 4, ".").TrimEnd('0') + ToString();
                }
            }
        default:
            {
                return ((decimal)this).ToString();
            }
        }
    }

    public static Double24 operator -(Double24 double24) => new(-double24._number);
    public static Double24 operator +(Double24 left, Double24 right) => new(left._number + right._number);
    public static Double24 operator -(Double24 left, Double24 right) => new(left._number - right._number);
    public static Double24 operator *(long left, Double24 right) => new(left * right._number);
    public static Double24 operator *(Shift left, Double24 right) => new(left * right._number);
    public static Double24 operator *(Double24 left, long right) => new(left._number * right);
    public static Double24 operator /(Double24 left, long right) => new(left._number / right);
    public static Double24 operator /(Double24 left, Shift right) => new(left._number / right);
    public static Double24 operator %(Double24 left, Double24 right) => new(left._number % right._number);
    public static Double24 operator <<(Double24 left, int right) => new(left._number << right);
    public static Double24 operator >>(Double24 left, int right) => new(left._number >> right);
    public static bool operator ==(Double24 left, Double24 right) => left.Equals(right);
    public static bool operator !=(Double24 left, Double24 right) => !(left == right);
    public static bool operator <(Double24 left, Double24 right) => left._number < right._number;
    public static bool operator >(Double24 left, Double24 right) => left._number > right._number;
    public static implicit operator Double24(byte @byte) => new((long)@byte << DIGIT);
    public static implicit operator Double24(sbyte @sbyte) => new((long)@sbyte << DIGIT);
    public static implicit operator Double24(short int16) => new((long)int16 << DIGIT);
    public static implicit operator Double24(ushort uint16) => new((long)uint16 << DIGIT);
    public static explicit operator Double24(int int32) => new(int32 << DIGIT);
    public static explicit operator Double24(uint uint32) => new(uint32 << DIGIT);
    public static explicit operator Double24(long int64) => new(int64 << DIGIT);
    public static explicit operator Double24(ulong uint64) => new((long)uint64 << DIGIT);
    public static implicit operator Double24(float single) => new((long)(DIGIT_SHIFT * single));
    public static implicit operator Double24(double @double) => new((long)(DIGIT_SHIFT * @double));
    public static implicit operator Double24(decimal @decimal) => new((long)(@decimal * ONE.Number));
    public static explicit operator byte(Double24 double24) => (byte)(double24._number >> DIGIT);
    public static explicit operator sbyte(Double24 double24) => (sbyte)(double24._number >> DIGIT);
    public static explicit operator short(Double24 double24) => (short)(double24._number >> DIGIT);
    public static explicit operator ushort(Double24 double24) => (ushort)(double24._number >> DIGIT);
    public static explicit operator int(Double24 double24) => (int)(double24._number >> DIGIT);
    public static explicit operator uint(Double24 double24) => (uint)(double24._number >> DIGIT);
    public static explicit operator long(Double24 double24) => double24._number >> DIGIT;
    public static explicit operator ulong(Double24 double24) => (ulong)(double24._number >> DIGIT);
    public static unsafe explicit operator float(Double24 double24)
    {
        if (double24._number == 0) return 0;

        bool sign = double24._number < 0;
        ulong significant = (ulong)(sign ? -double24._number : double24._number);
        // ONEのとき127。正規化するとONEは右に1シフトするから127-1=126。
        uint exponent = 126;

        // 仮数部が多すぎて捨てなければならない状況。
        if ((significant & 0xFFFF_FFFF_FF00_0000) != 0)
        {
            while (true)
            {
                significant >>= 1;
                exponent++;
                if ((significant & 0xFFFF_FFFF_FF00_0000) == 0) break;
            }
        }
        // 仮数部が少ないので正規化が必要、または既に正規化済みである状況。
        else
        {
            while (true)
            {
                if ((significant & 0x0080_0000) != 0) break;
                significant <<= 1;
                exponent--;
            }
        }

        uint significant_ = unchecked((uint)significant & 0x007F_FFFF);
        significant_ |= exponent << 23;
        if (sign) significant_ |= 0x8000_0000;
        return *(float*)&significant_;
    }
    public static unsafe explicit operator double(Double24 double24)
    {
        if (double24._number == 0) return 0;

        bool sign = double24._number < 0;
        ulong significant = (ulong)(sign ? -double24._number : double24._number);
        // ONEのとき1023。正規化するとONEは左に28シフトするから1023+28=1051。
        ulong exponent = 1051;

        // 仮数部が多すぎて捨てなければならない状況。
        if ((significant & 0xFFE0_0000_0000_0000) != 0)
        {
            while (true)
            {
                significant >>= 1;
                if ((significant & 0xFFE0_0000_0000_0000) == 0) break;
                exponent++;
            }
        }
        // 仮数部が少ないので正規化が必要、または既に正規化済みである状況。
        else
        {
            while (true)
            {
                if ((significant & 0x0010_0000_0000_0000) != 0) break;
                significant <<= 1;
                exponent--;
            }
        }

        significant &= 0x000F_FFFF_FFFF_FFFF;
        significant |= exponent << 52;
        if (sign) significant |= 0x8000_0000_0000_0000;
        return *(double*)&significant;
    }
    public static explicit operator decimal(Double24 double24) => (decimal)double24._number / ONE.Number;
}
