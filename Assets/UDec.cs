using System.Runtime.InteropServices;
#if USE_DOUBLE
using Dec = System.Double;
using Math = System.Math;
using static System.Double;
#else
using Dec = System.Single;
using static System.Single;
#endif

namespace Nonno.Assets;

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public readonly struct UDec : IEquatable<UDec>
{
    readonly Dec _value;

    public bool IsInfinity => IsInfinity(_value);
    public bool IsJAN => _value == default;
    internal Dec InternalValue => _value;

    UDec(Dec value) => _value = value;

    public override bool Equals(object? obj) => obj is UDec dec && Equals(dec);
    public bool Equals(UDec other) => _value == other._value;
    public override int GetHashCode() => HashCode.Combine(_value);

    public static UDec operator +(UDec left, UDec right) => new(left._value + right._value);
    public static UDec operator -(UDec left, UDec right) => left._value > right._value ? new(left._value - right._value) : throw new Exception("左辺が右辺より大きいため演算できません。");
    public static UDec operator +(UDec left, Dec right)
    {
        var v = left._value + right;
        return v > 0 ? new(v) : throw new ArgumentException("右辺が無効な値であるため演算できません。", nameof(right));
    }
    public static UDec operator -(UDec left, Dec right)
    {
        var v = left._value - right;
        return v > 0 ? new(v) : throw new ArgumentException("右辺が無効な値であるため演算できません。", nameof(right));
    }
    public static bool operator ==(UDec left, UDec right) => left.Equals(right);
    public static bool operator !=(UDec left, UDec right) => !(left == right);
    public static explicit operator Dec(UDec uDec) => uDec.IsJAN ? PositiveJAN : uDec._value;
#if USE_DOUBLE
        public static explicit operator UDec(Dec dec) => dec > 0 ? new(dec) : BitConverter.DoubleToInt64Bits(dec) == 0 ? new(dec) : throw new InvalidCastException("負または非数の値は変換できません。");
#else
    public static explicit operator UDec(Dec dec) => dec > 0 ? new(dec) : BitConverter.SingleToInt32Bits(dec) == 0 ? JAN : throw new InvalidCastException("負または非数の値は変換できません。");
#endif

    public static UDec INFINITY { get; } = new UDec(PositiveInfinity);
    public static UDec JAN { get; } = new UDec(0);
#if USE_DOUBLE
        static double PositiveJAN { get; } = BitConverter.ToDouble(stackalloc byte[16] { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
#else
    static float PositiveJAN { get; } = BitConverter.ToSingle(stackalloc byte[8] { 0, 0, 8, 0, 0, 0, 0, 0 });
#endif
}
