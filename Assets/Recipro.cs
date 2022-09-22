#if USE_DOUBLE
using Dec = System.Double;
#else
using Dec = System.Single;
#endif

namespace Nonno.Assets;

public readonly struct Recipro : IEquatable<Recipro>
{
    readonly Dec _value;

    public Recipro(Dec value) => _value = value;

    public override bool Equals(object? obj) => obj is Recipro recipro && Equals(recipro);
    public bool Equals(Recipro other) => _value == other._value;
    public override int GetHashCode() => _value.GetHashCode();
    public override string ToString() => "1/" + _value;

    public static explicit operator Recipro(Dec scalar) => new(1 / scalar);
    public static explicit operator Dec(Recipro recipro) => 1 / recipro._value;

    public static bool operator ==(Recipro left, Recipro right) => left.Equals(right);
    public static bool operator !=(Recipro left, Recipro right) => !(left == right);
    public static bool operator <=(Recipro left, Recipro right)
    {
        if (Dec.IsNegative(left._value))
        {
            return !Dec.IsNegative(right._value) || left._value >= right._value;
        }
        else
        {
            return !Dec.IsNegative(right._value) && left._value >= right._value;
        }
    }
    public static bool operator >=(Recipro left, Recipro right)
    {
        if (Dec.IsNegative(left._value))
        {
            return Dec.IsNegative(right._value) && left._value <= right._value;
        }
        else
        {
            return Dec.IsNegative(right._value) || left._value <= right._value;
        }
    }
    public static bool operator <(Recipro left, Recipro right)
    {
        if (Dec.IsNegative(left._value))
        {
            return !Dec.IsNegative(right._value) || left._value > right._value;
        }
        else
        {
            return !Dec.IsNegative(right._value) && left._value > right._value;
        }
    }
    public static bool operator >(Recipro left, Recipro right)
    {
        if (Dec.IsNegative(left._value))
        {
            return Dec.IsNegative(right._value) && left._value < right._value;
        }
        else
        {
            return Dec.IsNegative(right._value) || left._value < right._value;
        }
    }

    public static Recipro operator *(Recipro left, Recipro right) => new(left._value * right._value);
    public static Dec operator *(Recipro left, Dec right) => right / left._value;
    public static Recipro operator *(Dec left, Recipro right) => new(left / right._value);

    public static Dec operator /(Recipro left, Recipro right) => right._value / left._value;
    public static Recipro operator /(Recipro left, Dec right) => new(left._value * right);
    public static Dec operator /(Dec left, Recipro right) => left * right._value;
}
