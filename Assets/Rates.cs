// 令和弐年大暑確認済。
using static Nonno.Assets.Shift;
#if USE_DOUBLE
using Dec = System.Double;
#else
using Dec = System.Single;
#endif

namespace Nonno.Assets;

/// <summary>
/// 百分率を表します。
/// </summary>
public readonly struct Percent : IEquatable<Percent>
{
    public const int DENOM = 100;

    readonly Dec _value;

    public int Value => (int)(_value * DENOM);

    /// <summary>
    /// 百分率を初期化します。
    /// </summary>
    /// <param name="value">
    /// 百分率の値。
    /// </param>
    public Percent(int value) : this(value * 0.01f) { }
    /// <summary>
    /// 百分率を初期化します。
    /// </summary>
    /// <param name="value">
    /// 最も近い浮動小数点数による値。
    /// </param>
    public Percent(Dec @decimal) => _value = @decimal;

    public override string ToString() => $"{(int)_value}pp100({Value})";
    public override bool Equals(object? obj) => obj is Percent percent && Equals(percent);
    public bool Equals(Percent other) => _value == other._value;
    public override int GetHashCode() => HashCode.Combine(_value);

    public static int operator *(int parameter, Percent percent) => (int)(parameter * percent._value);
    public static int operator /(int parameter, Percent percent) => (int)(parameter / percent._value);
    public static long operator *(long parameter, Percent percent) => (long)(parameter * percent._value);
    public static long operator /(long parameter, Percent percent) => (long)(parameter / percent._value);
    public static uint operator *(uint parameter, Percent percent) => (uint)(parameter * percent._value);
    public static uint operator /(uint parameter, Percent percent) => (uint)(parameter / percent._value);
    public static ulong operator *(ulong parameter, Percent percent) => (ulong)(parameter * percent._value);
    public static ulong operator /(ulong parameter, Percent percent) => (ulong)(parameter / percent._value);
    public static Dec operator *(Dec parameter, Percent percent) => parameter * percent._value;
    public static Dec operator /(Dec parameter, Percent percent) => parameter / percent._value;

    public static implicit operator float(Percent percent) => (float)percent._value;
    public static implicit operator double(Percent percent) => percent._value;
    public static explicit operator Percent(float @float) => new(@float);
    public static explicit operator Percent(double @double) => new((Dec)@double);
    public static explicit operator Percenbi(Percent percent) => (Percenbi)percent._value;
    public static explicit operator Permibi(Percent percent) => (Permibi)percent._value;

    public static bool operator ==(Percent left, Percent right) => left.Equals(right);
    public static bool operator !=(Percent left, Percent right) => !(left == right);
}

/// <summary>
/// 千廿亖分率を表します。
/// <para>
/// 整数の割合を表すのに都合がよく、処理速度が高いです。
/// </para>
/// </summary>
public readonly struct Permibi : IEquatable<Permibi>
{
    public const int BREAK = 10;
    public const int DENOM = 1 << BREAK;

    readonly int _value;

    public int Value => _value;

    /// <summary>
    /// 千廿亖分率を初期化します。
    /// </summary>
    /// <param name="value">
    /// 千廿亖分率の値。
    /// </param>
    public Permibi(int value) => _value = value;
    /// <summary>
    /// 千廿亖分率を初期化します。
    /// </summary>
    /// <param name="value">
    /// 最も近い浮動小数点数による値。
    /// </param>
    public Permibi(Dec @decimal) => _value = (int)(@decimal * 1024);

    public override string ToString() => $"{_value}pp1024({Value})";
    public override bool Equals(object? obj) => obj is Permibi permibi && Equals(permibi);
    public bool Equals(Permibi other) => _value == other._value;
    public override int GetHashCode() => HashCode.Combine(_value);

    public static int operator *(int parameter, Permibi permibi) => (parameter * permibi._value) >> BREAK;
    public static int operator /(int parameter, Permibi permibi) => (parameter << BREAK) / permibi._value;
    public static long operator *(long parameter, Permibi permibi) => (parameter * permibi._value) >> BREAK;
    public static long operator /(long parameter, Permibi permibi) => (parameter << BREAK) / permibi._value;
    public static uint operator *(uint parameter, Permibi permibi) => (parameter * (uint)permibi._value) >> BREAK;
    public static uint operator /(uint parameter, Permibi permibi) => (parameter << BREAK) / (uint)permibi._value;
    public static ulong operator *(ulong parameter, Permibi permibi) => (parameter * (ulong)permibi._value) >> BREAK;
    public static ulong operator /(ulong parameter, Permibi permibi) => (parameter << BREAK) / (ulong)permibi._value;

    public static implicit operator float(Permibi permibi) => permibi._value / (float)DENOM;
    public static implicit operator double(Permibi permibi) => permibi._value / (double)DENOM;
    public static explicit operator Permibi(float @float) => new(@float);
    public static explicit operator Permibi(double @double) => new((Dec)@double);
    public static explicit operator Percent(Permibi permibi) => (Percent)(Dec)permibi;
    public static explicit operator Percenbi(Permibi permibi) => new(value: permibi._value >> 3);

    public static bool operator ==(Permibi left, Permibi right) => left.Equals(right);
    public static bool operator !=(Permibi left, Permibi right) => !(left == right);
}

/// <summary>
/// 百廿八分率を表します。
/// <para>
/// 整数の割合を表すのに都合がよいですが、誤差を避けるため通常は<see cref="Permibi"/>を使用します。
/// </para>
/// </summary>
public readonly struct Percenbi : IEquatable<Percenbi>
{
    public const int BREAK = 7;
    public const int DENOM = 1 << BREAK;

    readonly int _value;

    public int Value => _value;

    /// <summary>
    /// 百廿八分率を初期化します。
    /// </summary>
    /// <param name="value">
    /// 百廿八分率の値。
    /// </param>
    public Percenbi(int value) => _value = value;
    /// <summary>
    /// 百廿八分率を初期化します。
    /// </summary>
    /// <param name="value">
    /// 最も近い浮動小数点数による値。
    /// </param>
    public Percenbi(Dec @decimal) => _value = (int)(S128 * @decimal);

    public override string ToString() => $"{_value}pp128({Value})";
    public override bool Equals(object? obj) => obj is Percenbi percenbi && Equals(percenbi);
    public bool Equals(Percenbi other) => _value == other._value;
    public override int GetHashCode() => HashCode.Combine(_value);

    public static int operator *(int parameter, Percenbi percenbi) => (parameter * percenbi._value) >> BREAK;
    public static int operator /(int parameter, Percenbi percenbi) => (parameter << BREAK) / percenbi._value;
    public static long operator *(long parameter, Percenbi percenbi) => (parameter * percenbi._value) >> BREAK;
    public static long operator /(long parameter, Percenbi percenbi) => (parameter << BREAK) / percenbi._value;
    public static uint operator *(uint parameter, Percenbi percenbi) => (parameter * (uint)percenbi._value) >> BREAK;
    public static uint operator /(uint parameter, Percenbi percenbi) => (parameter << BREAK) / (uint)percenbi._value;
    public static ulong operator *(ulong parameter, Percenbi percenbi) => (parameter * (ulong)percenbi._value) >> BREAK;
    public static ulong operator /(ulong parameter, Percenbi percenbi) => (parameter << BREAK) / (ulong)percenbi._value;

    public static implicit operator float(Percenbi percenbi) => percenbi._value / (float)DENOM;
    public static implicit operator double(Percenbi percenbi) => percenbi._value / (double)DENOM;
    public static explicit operator Percenbi(float @float) => new(@float);
    public static explicit operator Percenbi(double @double) => new((Dec)@double);
    public static explicit operator Percent(Percenbi percenbi) => (Percent)(Dec)percenbi;
    public static explicit operator Permibi(Percenbi percenbi) => new(percenbi._value << 3);

    public static bool operator ==(Percenbi left, Percenbi right) => left.Equals(right);
    public static bool operator !=(Percenbi left, Percenbi right) => !(left == right);
}

public readonly struct Percharbi : IEquatable<Percharbi>
{
    public const int BREAK = 16;
    public const int DENOM = 1 << BREAK;

    readonly long _value;

    public int Value => (int)_value;

    public Percharbi(int value) => _value = value;
    public Percharbi(Dec @decimal) => _value = (long)(S65535 * @decimal);

    public override string ToString() => $"{_value}pp65535({(double)this})";
    public override bool Equals(object? obj) => obj is Percharbi percharbi && Equals(percharbi);
    public bool Equals(Percharbi other) => _value == other._value;
    public override int GetHashCode() => HashCode.Combine(_value);

    public static long operator *(Percharbi percharbi, long parameter) => (percharbi._value * parameter) >> BREAK;

    public static implicit operator float(Percharbi percenbi) => percenbi._value / (float)DENOM;
    public static implicit operator double(Percharbi percenbi) => percenbi._value / (double)DENOM;
    public static explicit operator Percharbi(float @float) => new(@float);
    public static explicit operator Percharbi(double @double) => new((Dec)@double);

    public static bool operator ==(Percharbi left, Percharbi right) => left.Equals(right);
    public static bool operator !=(Percharbi left, Percharbi right) => !(left == right);
}
