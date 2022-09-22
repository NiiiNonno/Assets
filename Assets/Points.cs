#pragma warning disable CA2231 // 値型 Equals のオーバーライドで、演算子 equals をオーバーロードします
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
//using Nonno.Assets.Generators.DynamicCompiledAttributes;
#if USE_DOUBLE
using Dec = System.Double;
#else
using Dec = System.Single;
#endif

namespace Nonno.Assets;

[Serializable]
public readonly struct Point0
{
    [SuppressMessage("Performance", "CA1822:メンバーを static に設定します")]
    public bool IsZero => true;
    public override bool Equals(object? obj) => obj is Vector0;
    public override int GetHashCode() => 0;
    public override string ToString() => $"()";
    public static Point0 operator +(in Point0 a, in Vector0 b) => default;
    public static Point0 operator -(in Point0 a, in Vector0 b) => default;
    public static Vector0 operator -(in Point0 a, in Point0 b) => default;
    public void Write(Span<byte> to) { }

    [Serializable]
    public readonly struct Recipro
    {
        public override bool Equals(object? obj) => obj is Recipro;
        public override int GetHashCode() => 0;
        public override string ToString() => $"()";
        public static Recipro operator /(Recipro a, Dec b) => default;
        public static implicit operator Recipro(Point0 p) => default;
        public static explicit operator Point0(Recipro p) => default;
    }
}

[Serializable]
public readonly struct Point1
{
    public readonly Dec c0;
    public bool IsZero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => c0 == 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point1(Dec c0)
    {
        this.c0 = c0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is Point1 p && p.c0 == c0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(c0);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"({c0})";
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point1 operator +(in Point1 left, in Vector1 right) => new(left.c0 + right.c0);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point1 operator -(in Point1 left, in Vector1 right) => new(left.c0 - right.c0);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1 operator -(in Point1 left, in Point1 right) => new(left.c0 - right.c0);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out Dec c0)
    {
        c0 = this.c0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Point1(in Dec c0) => new(c0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Span<byte> to)
    {
        _ = BitConverter.TryWriteBytes(to[(0 * sizeof(Dec))..], c0);
    }

    [Serializable]
    public readonly struct Recipro
    {
        public readonly Dec r0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Recipro(Dec r0)
        {
            this.r0 = r0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is Recipro p && p.r0 == r0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCode.Combine(r0);
        public override string ToString() => $"({1 / r0})";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Recipro(Point1 p) => new(1 / p.c0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point1(Recipro p) => new(1 / p.r0);
    }
}

[Serializable]
public readonly struct Point2
{
    public readonly Dec c0, c1;
    public bool IsZero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => c0 == 0 && c1 == 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point2(Dec c0, Dec c1)
    {
        this.c0 = c0;
        this.c1 = c1;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is Point2 p && p.c0 == c0 && p.c1 == c1;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(c0, c1);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"({c0}, {c1})";
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2 operator +(in Point2 left, in Vector2 right) => new(left.c0 + right.c0, left.c1 + right.c1);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2 operator -(in Point2 left, in Vector2 right) => new(left.c0 - right.c0, left.c1 - right.c1);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator -(in Point2 left, in Point2 right) => new(left.c0 - right.c0, left.c1 - right.c1);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out Dec c0, out Dec c1)
    {
        c0 = this.c0;
        c1 = this.c1;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Point2(in (Dec c0, Dec c1) p) => new(p.c0, p.c1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Span<byte> to)
    {
        _ = BitConverter.TryWriteBytes(to[(0 * sizeof(Dec))..], c0);
        _ = BitConverter.TryWriteBytes(to[(1 * sizeof(Dec))..], c1);
    }

    [Serializable]
    public readonly struct Recipro
    {
        public readonly Dec r0, r1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Recipro(Dec r0, Dec r1)
        {
            this.r0 = r0;
            this.r1 = r1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is Recipro p && p.r0 == r0 && p.r1 == r1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCode.Combine(r0, r1);
        public override string ToString() => $"({1 / r0}, {1 / r1})";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Recipro(Point2 p) => new(1 / p.c0, 1 / p.c1);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point2(Recipro p) => new(1 / p.r0, 1 / p.r1);
    }
}

[Serializable]
public readonly struct Point3
{
    public readonly Dec c0, c1, c2;
    public bool IsZero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => c0 == 0 && c1 == 0 && c2 == 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point3(Dec c0, Dec c1, Dec c2)
    {
        this.c0 = c0;
        this.c1 = c1;
        this.c2 = c2;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is Point3 p && p.c0 == c0 && p.c1 == c1 && p.c2 == c2;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(c0, c1, c2);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"({c0}, {c1}, {c2})";
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3 operator +(in Point3 left, in Vector3 right) => new(left.c0 + right.c0, left.c1 + right.c1, left.c2 + right.c2);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3 operator -(in Point3 left, in Vector3 right) => new(left.c0 - right.c0, left.c1 - right.c1, left.c2 - right.c2);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator -(in Point3 left, in Point3 right) => new(left.c0 - right.c0, left.c1 - right.c1, left.c2 - right.c2);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out Dec c0, out Dec c1, out Dec c2)
    {
        c0 = this.c0;
        c1 = this.c1;
        c2 = this.c2;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Point3(in (Dec c0, Dec c1, Dec c2) p) => new(p.c0, p.c1, p.c2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Span<byte> to)
    {
        _ = BitConverter.TryWriteBytes(to[(0 * sizeof(Dec))..], c0);
        _ = BitConverter.TryWriteBytes(to[(1 * sizeof(Dec))..], c1);
        _ = BitConverter.TryWriteBytes(to[(2 * sizeof(Dec))..], c2);
    }

    [Serializable]
    public readonly struct Recipro
    {
        public readonly Dec r0, r1, r2;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Recipro(Dec r0, Dec r1, Dec r2)
        {
            this.r0 = r0;
            this.r1 = r1;
            this.r2 = r2;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is Recipro p && p.r0 == r0 && p.r1 == r1 && p.r2 == r2;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCode.Combine(r0, r1, r2);
        public override string ToString() => $"({1 / r0}, {1 / r1}, {1 / r2})";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Recipro(Point3 p) => new(1 / p.c0, 1 / p.c1, 1 / p.c2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point3(Recipro p) => new(1 / p.r0, 1 / p.r1, 1 / p.r2);
    }
}

[Serializable]
public readonly struct Point4
{
    public readonly Dec c0, c1, c2, c3;
    public bool IsZero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => c0 == 0 && c1 == 0 && c2 == 0 && c3 == 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point4(Dec c0, Dec c1, Dec c2, Dec c3)
    {
        this.c0 = c0;
        this.c1 = c1;
        this.c2 = c2;
        this.c3 = c3;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is Point4 p && p.c0 == c0 && p.c1 == c1 && p.c2 == c2 && p.c3 == c3;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(c0, c1, c2, c3);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"({c0}, {c1}, {c2}, {c3})";
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point4 operator +(in Point4 left, in Vector4 right) => new(left.c0 + right.c0, left.c1 + right.c1, left.c2 + right.c2, left.c3 + right.c3);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point4 operator -(in Point4 left, in Vector4 right) => new(left.c0 - right.c0, left.c1 - right.c1, left.c2 - right.c2, left.c3 - right.c3);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator -(in Point4 left, in Point4 right) => new(left.c0 - right.c0, left.c1 - right.c1, left.c2 - right.c2, left.c3 - right.c3);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out Dec c0, out Dec c1, out Dec c2, out Dec c3)
    {
        c0 = this.c0;
        c1 = this.c1;
        c2 = this.c2;
        c3 = this.c3;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Point4(in (Dec c0, Dec c1, Dec c2, Dec c3) p) => new(p.c0, p.c1, p.c2, p.c3);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Span<byte> to)
    {
        _ = BitConverter.TryWriteBytes(to[(0 * sizeof(Dec))..], c0);
        _ = BitConverter.TryWriteBytes(to[(1 * sizeof(Dec))..], c1);
        _ = BitConverter.TryWriteBytes(to[(2 * sizeof(Dec))..], c2);
        _ = BitConverter.TryWriteBytes(to[(3 * sizeof(Dec))..], c3);
    }

    [Serializable]
    public readonly struct Recipro
    {
        public readonly Dec r0, r1, r2, r3;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Recipro(Dec r0, Dec r1, Dec r2, Dec r3)
        {
            this.r0 = r0;
            this.r1 = r1;
            this.r2 = r2;
            this.r3 = r3;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is Recipro p && p.r0 == r0 && p.r1 == r1 && p.r2 == r2 && p.r3 == r3;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCode.Combine(r0, r1, r2, r3);
        public override string ToString() => $"({1 / r0}, {1 / r1}, {1 / r2}, {1 / r3})";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Recipro(Point4 p) => new(1 / p.c0, 1 / p.c1, 1 / p.c2, 1 / p.c3);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point4(Recipro p) => new(1 / p.r0, 1 / p.r1, 1 / p.r2, 1 / p.r3);
    }
}

[Serializable]
public readonly struct Point5
{
    public readonly Dec c0, c1, c2, c3, c4;
    public bool IsZero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => c0 == 0 && c1 == 0 && c2 == 0 && c3 == 0 && c4 == 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point5(Dec c0, Dec c1, Dec c2, Dec c3, Dec c4)
    {
        this.c0 = c0;
        this.c1 = c1;
        this.c2 = c2;
        this.c3 = c3;
        this.c4 = c4;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is Point5 p && p.c0 == c0 && p.c1 == c1 && p.c2 == c2 && p.c3 == c3 && p.c4 == c4;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(c0, c1, c2, c3, c4);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"({c0}, {c1}, {c2}, {c3}, {c4})";
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point5 operator +(in Point5 left, in Vector5 right) => new(left.c0 + right.c0, left.c1 + right.c1, left.c2 + right.c2, left.c3 + right.c3, left.c4 + right.c4);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point5 operator -(in Point5 left, in Vector5 right) => new(left.c0 - right.c0, left.c1 - right.c1, left.c2 - right.c2, left.c3 - right.c3, left.c4 - right.c4);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector5 operator -(in Point5 left, in Point5 right) => new(left.c0 - right.c0, left.c1 - right.c1, left.c2 - right.c2, left.c3 - right.c3, left.c4 - right.c4);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out Dec c0, out Dec c1, out Dec c2, out Dec c3, out Dec c4)
    {
        c0 = this.c0;
        c1 = this.c1;
        c2 = this.c2;
        c3 = this.c3;
        c4 = this.c4;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Point5(in (Dec c0, Dec c1, Dec c2, Dec c3, Dec c4) p) => new(p.c0, p.c1, p.c2, p.c3, p.c4);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Span<byte> to)
    {
        _ = BitConverter.TryWriteBytes(to[(0 * sizeof(Dec))..], c0);
        _ = BitConverter.TryWriteBytes(to[(1 * sizeof(Dec))..], c1);
        _ = BitConverter.TryWriteBytes(to[(2 * sizeof(Dec))..], c2);
        _ = BitConverter.TryWriteBytes(to[(3 * sizeof(Dec))..], c3);
        _ = BitConverter.TryWriteBytes(to[(4 * sizeof(Dec))..], c4);
    }

    [Serializable]
    public readonly struct Recipro
    {
        public readonly Dec r0, r1, r2, r3, r4;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Recipro(Dec r0, Dec r1, Dec r2, Dec r3, Dec r4)
        {
            this.r0 = r0;
            this.r1 = r1;
            this.r2 = r2;
            this.r3 = r3;
            this.r4 = r4;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is Recipro p && p.r0 == r0 && p.r1 == r1 && p.r2 == r2 && p.r3 == r3 && p.r4 == r4;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCode.Combine(r0, r1, r2, r3, r4);
        public override string ToString() => $"({1 / r0}, {1 / r1}, {1 / r2}, {1 / r3}, {1 / r4})";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Recipro(Point5 p) => new(1 / p.c0, 1 / p.c1, 1 / p.c2, 1 / p.c3, 1 / p.c4);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point5(Recipro p) => new(1 / p.r0, 1 / p.r1, 1 / p.r2, 1 / p.r3, 1 / p.r4);
    }
}

[Serializable]
public readonly struct Point6
{
    public readonly Dec c0, c1, c2, c3, c4, c5;
    public bool IsZero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => c0 == 0 && c1 == 0 && c2 == 0 && c3 == 0 && c4 == 0 && c5 == 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point6(Dec c0, Dec c1, Dec c2, Dec c3, Dec c4, Dec c5)
    {
        this.c0 = c0;
        this.c1 = c1;
        this.c2 = c2;
        this.c3 = c3;
        this.c4 = c4;
        this.c5 = c5;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is Point6 p && p.c0 == c0 && p.c1 == c1 && p.c2 == c2 && p.c3 == c3 && p.c4 == c4 && p.c5 == c5;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(c0, c1, c2, c3, c4, c5);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"({c0}, {c1}, {c2}, {c3}, {c4}, {c5})";
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point6 operator +(in Point6 left, in Vector6 right) => new(left.c0 + right.c0, left.c1 + right.c1, left.c2 + right.c2, left.c3 + right.c3, left.c4 + right.c4, left.c5 + right.c5);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point6 operator -(in Point6 left, in Vector6 right) => new(left.c0 - right.c0, left.c1 - right.c1, left.c2 - right.c2, left.c3 - right.c3, left.c4 - right.c4, left.c5 - right.c5);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector6 operator -(in Point6 left, in Point6 right) => new(left.c0 - right.c0, left.c1 - right.c1, left.c2 - right.c2, left.c3 - right.c3, left.c4 - right.c4, left.c5 - right.c5);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out Dec c0, out Dec c1, out Dec c2, out Dec c3, out Dec c4, out Dec c5)
    {
        c0 = this.c0;
        c1 = this.c1;
        c2 = this.c2;
        c3 = this.c3;
        c4 = this.c4;
        c5 = this.c5;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Point6(in (Dec c0, Dec c1, Dec c2, Dec c3, Dec c4, Dec c5) p) => new(p.c0, p.c1, p.c2, p.c3, p.c4, p.c5);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Span<byte> to)
    {
        _ = BitConverter.TryWriteBytes(to[(0 * sizeof(Dec))..], c0);
        _ = BitConverter.TryWriteBytes(to[(1 * sizeof(Dec))..], c1);
        _ = BitConverter.TryWriteBytes(to[(2 * sizeof(Dec))..], c2);
        _ = BitConverter.TryWriteBytes(to[(3 * sizeof(Dec))..], c3);
        _ = BitConverter.TryWriteBytes(to[(4 * sizeof(Dec))..], c4);
        _ = BitConverter.TryWriteBytes(to[(5 * sizeof(Dec))..], c5);
    }

    [Serializable]
    public readonly struct Recipro
    {
        public readonly Dec r0, r1, r2, r3, r4, r5;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Recipro(Dec r0, Dec r1, Dec r2, Dec r3, Dec r4, Dec r5)
        {
            this.r0 = r0;
            this.r1 = r1;
            this.r2 = r2;
            this.r3 = r3;
            this.r4 = r4;
            this.r5 = r5;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is Recipro p && p.r0 == r0 && p.r1 == r1 && p.r2 == r2 && p.r3 == r3 && p.r4 == r4 && p.r5 == r5;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCode.Combine(r0, r1, r2, r3, r4, r5);
        public override string ToString() => $"({1 / r0}, {1 / r1}, {1 / r2}, {1 / r3}, {1 / r4}, {1 / r5})";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Recipro(Point6 p) => new(1 / p.c0, 1 / p.c1, 1 / p.c2, 1 / p.c3, 1 / p.c4, 1 / p.c5);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point6(Recipro p) => new(1 / p.r0, 1 / p.r1, 1 / p.r2, 1 / p.r3, 1 / p.r4, 1 / p.r5);
    }
}

[Serializable]
public readonly struct Point7
{
    public readonly Dec c0, c1, c2, c3, c4, c5, c6;
    public bool IsZero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => c0 == 0 && c1 == 0 && c2 == 0 && c3 == 0 && c4 == 0 && c5 == 0 && c6 == 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point7(Dec c0, Dec c1, Dec c2, Dec c3, Dec c4, Dec c5, Dec c6)
    {
        this.c0 = c0;
        this.c1 = c1;
        this.c2 = c2;
        this.c3 = c3;
        this.c4 = c4;
        this.c5 = c5;
        this.c6 = c6;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is Point7 p && p.c0 == c0 && p.c1 == c1 && p.c2 == c2 && p.c3 == c3 && p.c4 == c4 && p.c5 == c5 && p.c6 == c6;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(c0, c1, c2, c3, c4, c5, c6);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"({c0}, {c1}, {c2}, {c3}, {c4}, {c5}, {c6})";
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point7 operator +(in Point7 left, in Vector7 right) => new(left.c0 + right.c0, left.c1 + right.c1, left.c2 + right.c2, left.c3 + right.c3, left.c4 + right.c4, left.c5 + right.c5, left.c6 + right.c6);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point7 operator -(in Point7 left, in Vector7 right) => new(left.c0 - right.c0, left.c1 - right.c1, left.c2 - right.c2, left.c3 - right.c3, left.c4 - right.c4, left.c5 - right.c5, left.c6 - right.c6);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector7 operator -(in Point7 left, in Point7 right) => new(left.c0 - right.c0, left.c1 - right.c1, left.c2 - right.c2, left.c3 - right.c3, left.c4 - right.c4, left.c5 - right.c5, left.c6 - right.c6);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out Dec c0, out Dec c1, out Dec c2, out Dec c3, out Dec c4, out Dec c5, out Dec c6)
    {
        c0 = this.c0;
        c1 = this.c1;
        c2 = this.c2;
        c3 = this.c3;
        c4 = this.c4;
        c5 = this.c5;
        c6 = this.c6;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Point7(in (Dec c0, Dec c1, Dec c2, Dec c3, Dec c4, Dec c5, Dec c6) p) => new(p.c0, p.c1, p.c2, p.c3, p.c4, p.c5, p.c6);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Span<byte> to)
    {
        _ = BitConverter.TryWriteBytes(to[(0 * sizeof(Dec))..], c0);
        _ = BitConverter.TryWriteBytes(to[(1 * sizeof(Dec))..], c1);
        _ = BitConverter.TryWriteBytes(to[(2 * sizeof(Dec))..], c2);
        _ = BitConverter.TryWriteBytes(to[(3 * sizeof(Dec))..], c3);
        _ = BitConverter.TryWriteBytes(to[(4 * sizeof(Dec))..], c4);
        _ = BitConverter.TryWriteBytes(to[(5 * sizeof(Dec))..], c5);
        _ = BitConverter.TryWriteBytes(to[(6 * sizeof(Dec))..], c6);
    }

    [Serializable]
    public readonly struct Recipro
    {
        public readonly Dec r0, r1, r2, r3, r4, r5, r6;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Recipro(Dec r0, Dec r1, Dec r2, Dec r3, Dec r4, Dec r5, Dec r6)
        {
            this.r0 = r0;
            this.r1 = r1;
            this.r2 = r2;
            this.r3 = r3;
            this.r4 = r4;
            this.r5 = r5;
            this.r6 = r6;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is Recipro p && p.r0 == r0 && p.r1 == r1 && p.r2 == r2 && p.r3 == r3 && p.r4 == r4 && p.r5 == r5 && p.r6 == r6;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCode.Combine(r0, r1, r2, r3, r4, r5, r6);
        public override string ToString() => $"({1 / r0}, {1 / r1}, {1 / r2}, {1 / r3}, {1 / r4}, {1 / r5}, {1 / r6})";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Recipro(Point7 p) => new(1 / p.c0, 1 / p.c1, 1 / p.c2, 1 / p.c3, 1 / p.c4, 1 / p.c5, 1 / p.c6);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point7(Recipro p) => new(1 / p.r0, 1 / p.r1, 1 / p.r2, 1 / p.r3, 1 / p.r4, 1 / p.r5, 1 / p.r6);
    }
}

[Serializable]
public readonly struct Point8
{
    public readonly Dec c0, c1, c2, c3, c4, c5, c6, c7;
    public bool IsZero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => c0 == 0 && c1 == 0 && c2 == 0 && c3 == 0 && c4 == 0 && c5 == 0 && c6 == 0 && c7 == 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point8(Dec c0, Dec c1, Dec c2, Dec c3, Dec c4, Dec c5, Dec c6, Dec c7)
    {
        this.c0 = c0;
        this.c1 = c1;
        this.c2 = c2;
        this.c3 = c3;
        this.c4 = c4;
        this.c5 = c5;
        this.c6 = c6;
        this.c7 = c7;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is Point8 p && p.c0 == c0 && p.c1 == c1 && p.c2 == c2 && p.c3 == c3 && p.c4 == c4 && p.c5 == c5 && p.c6 == c6 && p.c7 == c7;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(c0, c1, c2, c3, c4, c5, c6, c7);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"({c0}, {c1}, {c2}, {c3}, {c4}, {c5}, {c6}, {c7})";
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point8 operator +(in Point8 left, in Vector8 right) => new(left.c0 + right.c0, left.c1 + right.c1, left.c2 + right.c2, left.c3 + right.c3, left.c4 + right.c4, left.c5 + right.c5, left.c6 + right.c6, left.c7 + right.c7);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point8 operator -(in Point8 left, in Vector8 right) => new(left.c0 - right.c0, left.c1 - right.c1, left.c2 - right.c2, left.c3 - right.c3, left.c4 - right.c4, left.c5 - right.c5, left.c6 - right.c6, left.c7 - right.c7);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector8 operator -(in Point8 left, in Point8 right) => new(left.c0 - right.c0, left.c1 - right.c1, left.c2 - right.c2, left.c3 - right.c3, left.c4 - right.c4, left.c5 - right.c5, left.c6 - right.c6, left.c7 - right.c7);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out Dec c0, out Dec c1, out Dec c2, out Dec c3, out Dec c4, out Dec c5, out Dec c6, out Dec c7)
    {
        c0 = this.c0;
        c1 = this.c1;
        c2 = this.c2;
        c3 = this.c3;
        c4 = this.c4;
        c5 = this.c5;
        c6 = this.c6;
        c7 = this.c7;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Point8(in (Dec c0, Dec c1, Dec c2, Dec c3, Dec c4, Dec c5, Dec c6, Dec c7) p) => new(p.c0, p.c1, p.c2, p.c3, p.c4, p.c5, p.c6, p.c7);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Span<byte> to)
    {
        _ = BitConverter.TryWriteBytes(to[(0 * sizeof(Dec))..], c0);
        _ = BitConverter.TryWriteBytes(to[(1 * sizeof(Dec))..], c1);
        _ = BitConverter.TryWriteBytes(to[(2 * sizeof(Dec))..], c2);
        _ = BitConverter.TryWriteBytes(to[(3 * sizeof(Dec))..], c3);
        _ = BitConverter.TryWriteBytes(to[(4 * sizeof(Dec))..], c4);
        _ = BitConverter.TryWriteBytes(to[(5 * sizeof(Dec))..], c5);
        _ = BitConverter.TryWriteBytes(to[(6 * sizeof(Dec))..], c6);
        _ = BitConverter.TryWriteBytes(to[(7 * sizeof(Dec))..], c7);
    }

    [Serializable]
    public readonly struct Recipro
    {
        public readonly Dec r0, r1, r2, r3, r4, r5, r6, r7;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Recipro(Dec r0, Dec r1, Dec r2, Dec r3, Dec r4, Dec r5, Dec r6, Dec r7)
        {
            this.r0 = r0;
            this.r1 = r1;
            this.r2 = r2;
            this.r3 = r3;
            this.r4 = r4;
            this.r5 = r5;
            this.r6 = r6;
            this.r7 = r7;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is Recipro p && p.r0 == r0 && p.r1 == r1 && p.r2 == r2 && p.r3 == r3 && p.r4 == r4 && p.r5 == r5 && p.r6 == r6 && p.r7 == r7;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCode.Combine(r0, r1, r2, r3, r4, r5, r6, r7);
        public override string ToString() => $"({1 / r0}, {1 / r1}, {1 / r2}, {1 / r3}, {1 / r4}, {1 / r5}, {1 / r6}, {1 / r7})";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Recipro(Point8 p) => new(1 / p.c0, 1 / p.c1, 1 / p.c2, 1 / p.c3, 1 / p.c4, 1 / p.c5, 1 / p.c6, 1 / p.c7);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point8(Recipro p) => new(1 / p.r0, 1 / p.r1, 1 / p.r2, 1 / p.r3, 1 / p.r4, 1 / p.r5, 1 / p.r6, 1 / p.r7);
    }
}

#pragma warning restore CA2231 // 値型 Equals のオーバーライドで、演算子 equals をオーバーロードします