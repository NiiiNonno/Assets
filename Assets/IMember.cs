using static Nonno.Assets.Shift;
#if USE_DOUBLE
using Dec = System.Double;
using Math = System.Math;
using static System.Double;
#else
using Dec = System.Single;
using static System.Single;
#endif

namespace Nonno.Assets;

/*
 * いるかどうか不明。
 * 何かに使える気はするけど。
 */

public interface INullable
{
    bool IsNull { get; }
}

public interface IAddable<in T, out TResult>
{
    TResult Add(T value);
    TResult Subtract(T value);

    public static TResult operator +(IAddable<T, TResult> @this, T value) => @this.Add(value);
    public static TResult operator -(IAddable<T, TResult> @this, T value) => @this.Subtract(value);
}

public interface IMultiplicable<in T, out TResult>
{
    TResult Multiply(T value);

    public static TResult operator *(IMultiplicable<T, TResult> @this, T value) => @this.Multiply(value);
}

public interface IDividable<in T, out TResult>
{
    TResult Divide(T value);

    public static TResult operator /(IDividable<T, TResult> @this, T value) => @this.Divide(value);
}

public interface IInvertable<in T, out TResult>
{
    TResult Invert();
    TResult Invert(T value);

    public static TResult operator *(IInvertable<T, TResult> @this, T value) => @this.Invert(value);
    //public static implicit operator TResult(IInvertable<T, TResult> @this) => @this.Invert();
}

public interface ISignFlipable<out TResult>
{
    TResult FlipSign();

    public static TResult operator -(ISignFlipable<TResult> @this) => @this.FlipSign();
}

public interface IEvaluable
{
    Dec Value { get; }
}

public interface IEvaluatingAddable<in TAddable, in T, out TResult> where TAddable : IAddable<T, TResult>
{

}

/// <summary>
/// 基本的な同種の値同士の演算が可能であることを示します。
/// </summary>
/// <remarks>
/// この機能の実装によって以下の演算が可能になります。
/// <code>
/// IMember<T> a, b;
/// Dec c;
/// Shift s;
/// 
/// _ = a + b;//<see cref="Add(T)"/>
/// _ = a - b;//<see cref="Sub(T)"/>
/// _ = a * c;//<see cref="Mul(Dec)"/>
/// _ = a / c;//<see cref="Div(Dec)"/>
/// _ = -a;//<see cref="Flp"/>
/// _ = a * s;//<see cref="Shp(Shift)"/>
/// _ = a / s;//<see cref="Shn(Shift)"/>
/// </code>
/// </remarks>
/// <typeparam name="T"></typeparam>
public interface IMember<T> : IEquatable<T> where T : struct
{
    bool EqualsDefault { get; }
    bool IsNil { get; }
    T Add(T other);
    T Sub(T other);
    T Mul(Dec dec);
    T Div(Dec dec);
    T Flp();
    T Shp(Shift shift);
    T Shn(Shift shift);
    T Read(ReadOnlySpan<byte> from);
    T Write(Span<byte> to);

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// この機能の実装によって以下の演算が可能になります。
    /// <code>
    /// IMember<T> a;
    /// IMember<T>.Recipro<TRecipro> a_r;
    /// Dec c;
    /// Shift s;
    /// 
    /// _ = a / a_r;//<see cref="Frc(T)"/>
    /// _ = c / a_r;//<see cref="Div(Dec)"/>
    /// _ = a_r / c;//<see cref="Dir(Dec)"/>
    /// _ = -a_r;//<see cref="Flp"/>
    /// _ = a_r * s;//<see cref="Shp(Shift)"/>
    /// _ = a_r / s;//<see cref="Shn(Shift)"/>
    /// _ = 1 / a;//<see cref="Rev(T)"/>
    /// _ = 1 / a_r;//<see cref="Rev(TRecipro)"/>
    /// </code>
    /// </remarks>
    /// <typeparam name="TRecipro"></typeparam>
    public interface IRecipro<TRecipro> : IEquatable<TRecipro> where TRecipro : struct
    {
        bool EqualsDefault { get; }
        bool IsNil { get; }
        TRecipro Mul(TRecipro other);
        /// <summary>
        /// 以下の演算を行います。
        /// <code>
        /// other / this;
        /// </code>
        /// </summary>
        Dec Frc(T other);
        /// <summary>
        /// 以下の演算を行います。
        /// <code>
        /// dec / this;
        /// </code>
        /// </summary>
        T Div(Dec dec);
        TRecipro Dir(Dec dec);
        TRecipro Flp();
        TRecipro Shp(Shift shift);
        TRecipro Shn(Shift shift);
        TRecipro Rev(T other);
        T Rev(TRecipro other);
        T Read(ReadOnlySpan<byte> from);
        T Write(Span<byte> to);
    }
}

public readonly struct Member : IMember<Member>
{
    readonly Dec _value;

    bool IMember<Member>.EqualsDefault => _value.IsDefault();

    bool IMember<Member>.IsNil => IsNaN(_value);

    public Member(Dec value) => _value = value;

    public override bool Equals(object? obj) => obj is Member member && _value == member._value;
    public override int GetHashCode() => HashCode.Combine(_value);
    public override string ToString() => $"{_value} [{nameof(Member)}]";

    Member IMember<Member>.Add(Member other) => this + other;
    Member IMember<Member>.Sub(Member other) => this - other;
    Member IMember<Member>.Mul(Dec dec) => dec * this;
    Member IMember<Member>.Div(Dec dec) => this / dec;
    Member IMember<Member>.Flp() => -this;
    bool IEquatable<Member>.Equals(Member other) => this == other;
    Member IMember<Member>.Shp(Shift shift) => shift * this;
    Member IMember<Member>.Shn(Shift shift) => this / shift;

    public static Member operator +(Member left, Member right) => new(left._value + right._value);
    public static Member operator -(Member left, Member right) => new(left._value - right._value);
    public static Member operator *(Dec left, Member right) => new(left * right._value);
    public static Member operator /(Member left, Dec right) => new(left._value / right);
    public static Member operator -(Member p) => new(p._value);
    public static bool operator ==(Member left, Member right) => left._value == right._value;
    public static bool operator !=(Member left, Member right) => left._value != right._value;
    public static Member operator *(Shift left, Member right) => new(left * right._value);
    public static Member operator /(Member left, Shift right) => new(left._value / right);
    public static implicit operator Dec(Member p) => p._value;
    public static explicit operator Member(Dec p) => new(p);

    public Member Read(ReadOnlySpan<byte> from) => new(BitConverter.ToSingle(from));
    public Member Write(Span<byte> to) { _ = BitConverter.TryWriteBytes(to, _value); return default; }

    public readonly struct Recipro : IMember<Member>.IRecipro<Recipro>
    {
        readonly Dec _value;

        bool IMember<Member>.IRecipro<Recipro>.EqualsDefault => _value.IsDefault();

        bool IMember<Member>.IRecipro<Recipro>.IsNil => IsNaN(_value);

        public Recipro(Dec value) => _value = value;

        public override bool Equals(object? obj) => obj is Recipro recipro && _value == recipro._value;
        public override int GetHashCode() => HashCode.Combine(_value);
        public override string ToString() => $"1 / {_value} [{nameof(Member)}]";

        Recipro IMember<Member>.IRecipro<Recipro>.Mul(Recipro other) => this * other;
        Dec IMember<Member>.IRecipro<Recipro>.Frc(Member other) => other / this;
        Member IMember<Member>.IRecipro<Recipro>.Div(Dec dec) => dec / this;
        Recipro IMember<Member>.IRecipro<Recipro>.Dir(Dec dec) => this / dec;
        Recipro IMember<Member>.IRecipro<Recipro>.Flp() => -this;
        bool IEquatable<Recipro>.Equals(Recipro other) => this == other;
        Recipro IMember<Member>.IRecipro<Recipro>.Shp(Shift shift) => shift * this;
        Recipro IMember<Member>.IRecipro<Recipro>.Shn(Shift shift) => this / shift;
        Recipro IMember<Member>.IRecipro<Recipro>.Rev(Member other) => other;
        Member IMember<Member>.IRecipro<Recipro>.Rev(Recipro other) => other;

        public static Recipro operator *(Recipro left, Recipro right) => new(left._value * right._value);
        public static Dec operator /(Member left, Recipro right) => left._value * right._value;
        public static Member operator /(Dec left, Recipro right) => new(left * right._value);
        public static Recipro operator /(Recipro left, Dec right) => new(left._value * right);
        public static Recipro operator -(Recipro p) => new(-p._value);
        public static bool operator ==(Recipro left, Recipro right) => left._value == right._value;
        public static bool operator !=(Recipro left, Recipro right) => left._value != right._value;
        public static Recipro operator *(Shift left, Recipro right) => new(right._value / left);
        public static Recipro operator /(Recipro left, Shift right) => new(right * left._value);
        public static implicit operator Recipro(Member p) => new(1 / p._value);
        public static implicit operator Member(Recipro p) => new(1 / p._value);

        public Member Read(ReadOnlySpan<byte> from) => new(BitConverter.ToSingle(from));
        public Member Write(Span<byte> to) { _ = BitConverter.TryWriteBytes(to, _value); return default; }
    }
}

public readonly struct Sample<T> : IEquatable<Sample<T>> where T : struct, IMember<T>
{
    public static readonly Sample<T> NIL = new(default, 0);

    readonly T _member;
    readonly Dec _value;

    public bool IsNil => _value == 0;

    public Sample(T member, UDec value)
    {
        if (member.IsNil)
        {
            _member = default;
            _value = 0;
        }
        else
        {
            _member = member;
            _value = value.InternalValue;
        }
    }
    Sample(T member, Dec value)
    {
        _member = member;
        _value = value;
    }

    public override bool Equals(object? obj) => obj is Sample<T> sample && Equals(sample);
    public bool Equals(Sample<T> other) => EqualityComparer<T>.Default.Equals(_member, other._member) && _value == other._value;
    public override int GetHashCode() => HashCode.Combine(_member, _value);
    public override string ToString() => $"{_member}({_value})";

    public static Sample<T> operator *(UDec left, Sample<T> right) => left.IsJAN ? NIL : (new(right._member, right._value * left.InternalValue));
    public static Sample<T> operator +(Sample<T> left, Sample<T> right)
    {
        if (left.IsNil) return right;
        if (right.IsNil) return left;
        var value = left._value + right._value;
        if (IsPositiveInfinity(value))
        {
            if (left._value < right._value) return right;
            if (right._value < left._value) return left;
            return new(left._member.Add(right._member).Shn(S2), value);
        }
        var member = left._member.Mul(left._value).Add(right._member.Mul(right._value)).Div(value);
        return member.IsNil ? NIL : new(member, value);
    }
    public static bool operator ==(Sample<T> left, Sample<T> right) => left.Equals(right);
    public static bool operator !=(Sample<T> left, Sample<T> right) => !(left == right);
    public static explicit operator T(Sample<T> sample) => sample._member;

    public static Sample<T> LendCredence(T member) => member.IsNil ? NIL : (new(member, PositiveInfinity));
}
