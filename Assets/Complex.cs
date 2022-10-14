// 令和弐年大暑確認済。
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Nonno.Assets.Scrolls;
#if USE_DOUBLE
using Dec = System.Double;
using Math = System.Math;
#else
using Dec = System.Single;
using Math = System.MathF;
#endif

namespace Nonno.Assets;

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public readonly struct Complex
{
    public readonly Dec r, i;
    public Dec Abs
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Math.Sqrt(r * r + i * i);
    }
    public Dec Arg
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Math.Atan2(i, r) * ConstantValues.PI_RECIPRO;
    }
    public Complex Exp
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            Dec v = Math.Exp(r);
            return new Complex(v * Math.Cos(i), v * Math.Sin(i));
        }
    }
    public Complex Log
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(Math.Abs(i), Arg);
    }
    public bool IsReal
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => i == 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Complex(Dec r, Dec i)
    {
        this.r = r;
        this.i = i;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsLess(in Complex than) => r * r + i * i < than.r * than.r + than.i * than.i;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMore(in Complex than) => r * r + i * i > than.r * than.r + than.i * than.i;
    public override string ToString() => (r, i) switch
    {
        (Dec.PositiveInfinity, Dec.PositiveInfinity) => "Infinity (orthant: 1)",
        (Dec.NegativeInfinity, Dec.PositiveInfinity) => "Infinity (orthant: 2)",
        (Dec.NegativeInfinity, Dec.NegativeInfinity) => "Infinity (orthant: 3)",
        (Dec.PositiveInfinity, Dec.NegativeInfinity) => "Infinity (orthant: 4)",
        (Dec.PositiveInfinity, _) => "RePositiveInfinity",
        (_, Dec.PositiveInfinity) => "ImPositiveInfinity",
        (Dec.NegativeInfinity, _) => "ReNegativeInfinity",
        (_, Dec.NegativeInfinity) => "ImNegativeInfinity",
        (_, Dec.NaN) or (Dec.NaN, _) => "NaN",
        (0, 0) => nameof(Zero),
        (0, _) => $"{i}i",
        (not 0, 0) => $"{r}",
        (not 0, > 0) => $"{r} + {i}i",
        (not 0, < 0) => $"{r} - {-i}i",
    };
    public string ToString(string? format) => (r, i) switch
    {
        (Dec.PositiveInfinity, Dec.PositiveInfinity) => "Infinity (orthant: 1)",
        (Dec.NegativeInfinity, Dec.PositiveInfinity) => "Infinity (orthant: 2)",
        (Dec.NegativeInfinity, Dec.NegativeInfinity) => "Infinity (orthant: 3)",
        (Dec.PositiveInfinity, Dec.NegativeInfinity) => "Infinity (orthant: 4)",
        (Dec.PositiveInfinity, _) => "RePositiveInfinity",
        (_, Dec.PositiveInfinity) => "ImPositiveInfinity",
        (Dec.NegativeInfinity, _) => "ReNegativeInfinity",
        (_, Dec.NegativeInfinity) => "ImNegativeInfinity",
        (_, Dec.NaN) or (Dec.NaN, _) => nameof(NaN),
        (0, 0) => nameof(Zero),
        (0, _) => $"{i.ToString(format)}i",
        (not 0, 0) => $"{r.ToString(format)}",
        (not 0, > 0) => $"{r.ToString(format)} + {i.ToString(format)}i",
        (not 0, < 0) => $"{r.ToString(format)} - {(-i).ToString(format)}i",
    };
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Deconstruct(in Complex p, out Dec r, out Dec i)
    {
        r = p.r;
        i = p.i;
    }
    public static Complex Zero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(0, 0);
    }
    public static Complex NaN
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(Dec.NaN, Dec.NaN);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Indistinguishable(in Complex a, in Complex b) => a.r == b.r && a.i == b.i;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex Pow(Dec a, in Complex b) => (b * Math.Log(a)).Exp;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex Pow(in Complex a, Dec b) => (b * a.Log).Exp;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex Pow(in Complex a, in Complex b) => (b * a.Log).Exp;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex operator +(in Complex a, in Complex b) => new(a.r + b.r, a.i + b.i);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex operator -(in Complex a, in Complex b) => new(a.r - b.r, a.i - b.i);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex operator *(in Complex a, in Complex b) => new(a.r * b.r - a.i * b.i, a.r * b.i + a.i * b.r);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex operator *(Dec a, in Complex b) => new(a * b.r, a * b.i);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex operator *(in Complex a, Dec b) => new(a.r * b, a.i * b);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex operator /(in Complex a, Dec b)
    {
        Dec v0 = 1 / b;
        Dec v1 = a.r * v0;
        return new Complex(v1, v0 * v1 * a.i);
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Recipro
    {
        public readonly Dec r_r, i_r;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Recipro(Dec r_r, Dec i_r)
        {
            this.r_r = r_r;
            this.i_r = i_r;
        }
        public override string ToString() => $"{1 / r_r} + {1 / i_r}i";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Recipro(Complex p)
        {
            Dec v = p.r * p.r + p.i * p.i;
            return new Recipro((p.r + p.i) / v, (p.r - p.i) / v);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Complex(Recipro p)
        {
            Dec v = p.r_r * p.r_r + p.i_r * p.i_r;
            return new Complex((p.r_r + p.i_r) / v, (p.r_r - p.i_r) / v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Complex operator /(in Complex a, in Recipro b) => new(a.r * b.r_r - a.i * b.i_r, a.r * b.i_r + a.i * b.r_r);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Complex operator /(Dec a, in Recipro b) => new(a * b.r_r, a * b.i_r);
    }
}

partial class ScrollExtensions
{
    public static Task Insert(this IScroll @this, in Complex complex)
    {
        @this.Insert(complex.r).Wait();
        return @this.Insert(complex.i);
    }
    public static Task Remove(this IScroll @this, out Complex complex)
    {
        @this.Remove(out Dec r).Wait();
        var r_ = @this.Remove(out Dec i);
        complex = new(r, i);
        return r_;
    }

    public static Task Insert(this IScroll @this, in Complex.Recipro recipro)
    {
        @this.Insert(recipro.r_r).Wait();
        return @this.Insert(recipro.i_r);
    }
    public static Task Remove(this IScroll @this, out Complex.Recipro recipro)
    {
        @this.Remove(out Dec r_r).Wait();
        var r = @this.Remove(out Dec i_r);
        recipro = new(r_r, i_r);
        return r;
    }
}