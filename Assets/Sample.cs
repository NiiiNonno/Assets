using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
#if USE_DOUBLE
using Dec = System.Double;
using Math = System.Math;
using static System.Double;
#else
using Dec = System.Single;
using static System.Single;
#endif

namespace Nonno.Assets;

//型の役割を明示しない`Sample`の使用復帰が推進されています。
//[Obsolete("保持するメンバの種類を明示するため`Sample<T>`を使用してください。")]
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public readonly struct Sample : IEquatable<Sample>
{
    /// <summary>
    /// 値。
    /// <para>
    /// 価が0のとき、値は0である。
    /// また、値は非数ではない。
    /// </para>
    /// </summary>
    readonly Dec _zhi;
    /// <summary>
    /// 価。
    /// <para>
    /// 価は正である。
    /// </para>
    /// </summary>
    readonly Dec _jia;
    public bool IsWorthless
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _jia == 0;
    }
    Sample(Dec zhi, Dec jia) { _zhi = zhi; _jia = jia; }
    public void Publish(out Dec zhi, out Dec jia)
    {
        zhi = _zhi;
        jia = _jia;
    }
    public override string ToString() => $"{_zhi}({_jia})";
    public static Sample WORTHLESS { get; } = new Sample(0, 0);
    public static Sample Give(Dec zhi, Dec jia) =>
        IsNaN(zhi) ? WORTHLESS :
        jia > 0 ? new Sample(zhi, jia) :
        jia == 0 ? WORTHLESS :
        throw new ArgumentException("負または非数の強さは指定できません。", nameof(jia));
    [Obsolete]
    public static Sample Give(Dec zhi, Value jia) => IsNaN(zhi) ? WORTHLESS : new Sample(zhi, (Dec)jia);
    public static Sample Give(Dec zhi, UDec jia) => IsNaN(zhi) || jia.IsJAN ? WORTHLESS : new Sample(zhi, jia.InternalValue);
    public static Sample Give(byte zhi, byte jia) => jia == 0 ? WORTHLESS : new Sample(zhi, jia);
    public static Sample Give(sbyte zhi, byte jia) => jia == 0 ? WORTHLESS : new Sample(zhi, jia);
    public static Sample Give(int zhi, byte jia) => jia == 0 ? WORTHLESS : new Sample(zhi, jia);
    public static Sample Give(int zhi, ushort jia) => jia == 0 ? WORTHLESS : new Sample(zhi, jia);
    public static Sample Give(int zhi, uint jia) => jia == 0 ? WORTHLESS : new Sample(zhi, jia);
    public static Sample Give(int zhi, ulong jia) => jia == 0 ? WORTHLESS : new Sample(zhi, jia);
    public static Sample Give(long zhi, byte jia) => jia == 0 ? WORTHLESS : new Sample(zhi, jia);
    public static Sample Give(long zhi, ushort jia) => jia == 0 ? WORTHLESS : new Sample(zhi, jia);
    public static Sample Give(long zhi, uint jia) => jia == 0 ? WORTHLESS : new Sample(zhi, jia);
    public static Sample Give(long zhi, ulong jia) => jia == 0 ? WORTHLESS : new Sample(zhi, jia);
    public static Sample LendCredence(Dec to) => IsNaN(to) ? WORTHLESS : new Sample(to, Dec.PositiveInfinity);
    public override bool Equals(object? obj) => obj is Sample sample && Equals(sample);
    public bool Equals(Sample other) => _zhi == other._zhi;
    public override int GetHashCode() => HashCode.Combine(_zhi);

#pragma warning disable CS0618 // 型またはメンバーが旧型式です
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Obsolete]
    public static Sample operator *(in Sample a, Value b)
    {
        Dec jia = a._jia * (Dec)b;
        return jia > 0 ? new Sample(a._zhi, jia) : WORTHLESS;
    }
#pragma warning restore CS0618 // 型またはメンバーが旧型式です

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Sample operator *(in Sample a, UDec b)
    {
        Dec jia = a._jia * b.InternalValue;
        return jia > 0 ? new Sample(a._zhi, jia) : WORTHLESS;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Sample operator *(in Sample a, Dec b)
    {
        Dec jia = a._jia * b;
        return jia < 0 ? throw new ArgumentException("負の値を係数とすることはできません。", nameof(b)) : jia > 0 ? new Sample(a._zhi, jia) : WORTHLESS;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Sample operator +(in Sample a, in Sample b)
    {
        Dec jia = a._jia + b._jia;
        if (IsPositiveInfinity(jia))
        {
            if (a._jia > b._jia) return a;
            if (a._jia < b._jia) return b;
            return new Sample((a._zhi + b._zhi) * (Dec)0.5, PositiveInfinity);
        }
        Dec zhi = (a._zhi * a._jia + b._zhi * b._jia) / jia;
        return IsNaN(zhi) ? WORTHLESS : new Sample(zhi, jia);

        /*
         * 価は非負であるから、
         * jiaの価が0となるのは、aの価とbの価が共に0のときである。
         * aの価とbの価が共に0のとき、aの値とbの値は共に0であるから、
         * zhiは0かNaNであるはずである。
         * zhiがNaNでないとき、zhiは0である。
         */
    }
    public static bool operator ==(Sample left, Sample right) => left.Equals(right);
    public static bool operator !=(Sample left, Sample right) => !(left == right);
    public static implicit operator Dec(in Sample p) => p._zhi;

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    [Obsolete("`UDec`構造体への代替が推奨されています。")]
    public readonly struct Value
    {
        readonly Dec _value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Value(Dec value) => _value = value > 0 /*この時点でNaNチェックがかかる(NaN比較は全てfalse)*/? value : throw new ArgumentException($"非正または非数の価は指定できません。零の価の標本を作成するときは、{nameof(WORTHLESS)}を使用してください。", nameof(value));
        public static Value INFINITY { get; } = new Value(PositiveInfinity);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Dec(Value p) => p._value;
    }
}
