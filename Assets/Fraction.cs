#if USE_DOUBLE
using Dec = System.Double;
#else
using Dec = System.Single;
#endif

namespace Nonno.Assets;

public readonly struct Fraction : IEquatable<Fraction>
{
    public static readonly Fraction ZERO = new(0, 1);

    public readonly int numerator;
    public readonly int denominator;

    public Dec Value => numerator / (Dec)denominator;
    public bool IsReduced => 0 <= denominator;
    public bool IsZero => numerator == 0 && denominator != 0;

    public Fraction(int numerator, int denominator, bool isReduced = false)
    {
        if (isReduced || denominator < 0)
        {
            this.numerator = numerator;
            this.denominator = denominator;
        }
        else
        {
            this.numerator = -numerator;
            this.denominator = -denominator;
        }
    }
    Fraction(int numerator, int denominator)
    {
        this.numerator = numerator;
        this.denominator = denominator;
    }

    public override bool Equals(object? obj) => obj is Fraction fraction && Equals(fraction);
    public bool Equals(Fraction other) => numerator == other.numerator && denominator == other.denominator;
    public override int GetHashCode() => HashCode.Combine(numerator, denominator);
    public override string ToString() => $"{(denominator < 0 ? -numerator : numerator)}/{(denominator < 0 ? -denominator : denominator)}={Value}";

    public static Fraction operator *(int left, Fraction right) => new(left * right.numerator, right.denominator);
    public static Fraction operator /(int left, Fraction right) => left * Inverse(right);
    public static int operator *(Fraction left, int right) => right * left.numerator / left.denominator;
    public static long operator *(Fraction left, long right) => right * left.numerator / left.denominator;
    public static bool operator ==(Fraction left, Fraction right)
    {
        if (left.denominator == right.denominator) return left.numerator == right.numerator;

        left = Reduce(left);
        right = Reduce(right);
        return left.denominator == right.denominator && left.numerator == right.numerator;
    }
    public static bool operator !=(Fraction left, Fraction right)
    {
        if (left.denominator == right.denominator) return left.numerator != right.numerator;

        left = Reduce(left);
        right = Reduce(right);
        return left.denominator != right.denominator || left.numerator != right.numerator;
    }

    public static explicit operator float(Fraction fraction) => (float)fraction.numerator / fraction.denominator;
    public static explicit operator double(Fraction fraction) => (double)fraction.numerator / fraction.denominator;
    public static explicit operator decimal(Fraction fraction) => (decimal)fraction.numerator / fraction.denominator;

    public static Fraction Reduce(Fraction fraction)
    {
        if (fraction.IsReduced)
        {
            return fraction;
        }
        else if (fraction.numerator == 0)
        {
            return ZERO;
        }
        else
        {
            // 符号に注意。入力の分母は常に非正で出力の分母は常に非負である。
            var v = -GetGreatestCommonDivisor(fraction.numerator < 0 ? -fraction.numerator : fraction.numerator, -fraction.denominator);
            return new Fraction(fraction.numerator / v, fraction.denominator / v);
        }
    }

    public static int GetGreatestCommonDivisor(int integar1, int integar2)
    {
#if DEBUG
            if (integar1 <= 0) throw new ArgumentException("正の値が必要です。", nameof(integar1));
            if (integar2 <= 0) throw new ArgumentException("正の値が必要です。", nameof(integar2));
#endif

        while (integar1 > 0)
        {
            int integar1_ = integar2 % integar1;
            integar2 = integar1;
            integar1 = integar1_;
        }
        return integar2;
    }

    public static Fraction Inverse(Fraction fraction)
    {
        if (fraction.IsReduced)
        {
            if (fraction.numerator < 0) return new Fraction(-fraction.denominator, -fraction.numerator);
            else return new Fraction(fraction.denominator, fraction.numerator);
        }
        else
        {
            if (fraction.numerator < 0) return new Fraction(fraction.denominator, fraction.numerator);
            else return new Fraction(-fraction.denominator, -fraction.numerator);
        }
    }

    public static Fraction GetApproximation(float value, int deepness = sizeof(float), float errorRange = 0)
    {
#if DEBUG
            if (errorRange < 0) throw new ArgumentException("誤差幅は非負の値である必要があります。", nameof(errorRange));
#endif

#if DEBUG
            int[] span = new int[deepness];
#else
        Span<int> span = stackalloc int[deepness];
#endif

        int i = 0;
        for (; i < span.Length; i++)
        {
            var floor = (int)value;
            span[i] = floor;
            var fP = value - floor;
            if (-errorRange <= fP && fP <= errorRange) { i++; break; }
            value = 1 / fP;
        }

        i--;

        int numerator = span[i], denominator = 1;
        for (int j = i - 1; j >= 0; j--)
        {
            int numerator_ = numerator * span[j] + denominator;
            denominator = numerator;
            numerator = numerator_;
        }

        return new Fraction(numerator, denominator);
    }
    public static Fraction GetApproximation(double value, int deepness = sizeof(double), double errorRange = 0)
    {
#if DEBUG
            if (errorRange < 0) throw new ArgumentException("誤差幅は非負の値である必要があります。", nameof(errorRange));
#endif

#if DEBUG
            int[] span = new int[deepness];
#else
        Span<int> span = stackalloc int[deepness];
#endif

        int i = 0;
        for (; i < span.Length; i++)
        {
            var floor = (int)value;
            span[i] = floor;
            var fP = value - floor;
            if (-errorRange <= fP && fP <= errorRange) { i++; break; }
            value = 1 / fP;
        }

        i--;

        int numerator = span[i], denominator = 1;
        for (int j = i - 1; j >= 0; j--)
        {
            int numerator_ = numerator * span[j] + denominator;
            denominator = numerator;
            numerator = numerator_;
        }

        return new Fraction(numerator, denominator);
    }
}
