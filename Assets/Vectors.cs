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

[SuppressMessage("Style", "IDE0060:未使用のパラメーターを削除します")]
[Serializable]
public readonly struct Vector0
{
    [SuppressMessage("Performance", "CA1822:メンバーを static に設定します")]
    public bool IsZero => true;
    public override bool Equals(object? obj) => obj is Vector0;
    public override int GetHashCode() => default;
    public override string ToString() => $"()";
    public static Vector0 operator -(in Vector0 vector) => new();
    public static Vector0 operator +(in Vector0 left, in Vector0 right) => new();
    public static Vector0 operator -(in Vector0 left, in Vector0 right) => new();
    public static Vector0 operator *(Dec left, in Vector0 right) => new();
    public static Vector0 operator *(Shift left, in Vector0 right) => new();
    public void Deconstruct() { }
    public void Write(Span<byte> to) { }
    [Serializable]
    public readonly struct Recipro
    {
        public override bool Equals(object? obj) => obj is Recipro;
        public override int GetHashCode() => 0;
        public override string ToString() => $"()";
        public static implicit operator Recipro(in Vector0 v) => new();
        public static implicit operator Vector0(in Recipro r) => new();
    }
}

[Serializable]
public readonly struct Vector1
{
    public readonly Dec c0;
    public bool IsZero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => c0 == 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector1(Dec c0)
    {
        this.c0 = c0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is Vector1 p && p.c0 == c0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(c0);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"({c0})";
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1 operator -(in Vector1 vector) => new(-vector.c0);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1 operator +(in Vector1 left, in Vector1 right) => new(left.c0 + right.c0);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1 operator -(in Vector1 left, in Vector1 right) => new(left.c0 - right.c0);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1 operator *(Dec left, in Vector1 right) => new(left * right.c0);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector1 operator *(Shift left, in Vector1 right) => new(left * right.c0);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out Dec c0)
    {
        c0 = this.c0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector1(in Dec p) => new(p);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"({1 / r0})";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Recipro(in Vector1 v) => new(1 / v.c0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector1(in Recipro r) => new(1 / r.r0);
    }
}

[Serializable]
public readonly struct Vector2
{
    public readonly Dec c0, c1;
    public bool IsZero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => c0 == 0 && c1 == 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2(Dec c0, Dec c1)
    {
        this.c0 = c0;
        this.c1 = c1;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is Vector2 p && p.c0 == c0 && p.c1 == c1;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(c0, c1);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"({c0}, {c1})";
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator -(in Vector2 vector) => new(-vector.c0, -vector.c1);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator +(in Vector2 left, in Vector2 right) => new(left.c0 + right.c0, left.c1 + right.c1);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator -(in Vector2 left, in Vector2 right) => new(left.c0 - right.c0, left.c1 - right.c1);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator *(Dec left, in Vector2 right) => new(left * right.c0, left * right.c1);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator *(Shift left, in Vector2 right) => new(left * right.c0, left * right.c1);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out Dec c0, out Dec c1)
    {
        c0 = this.c0;
        c1 = this.c1;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector2(in (Dec c0, Dec c1) p) => new(p.c0, p.c1);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"({1 / r0}, {1 / r1})";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Recipro(in Vector2 v) => new(1 / v.c0, 1 / v.c1);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector2(in Recipro r) => new(1 / r.r0, 1 / r.r1);
    }
}

[Serializable]
public readonly struct Vector3
{
    public readonly Dec c0, c1, c2;
    public bool IsZero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => c0 == 0 && c1 == 0 && c2 == 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3(Dec c0, Dec c1, Dec c2)
    {
        this.c0 = c0;
        this.c1 = c1;
        this.c2 = c2;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is Vector3 p && p.c0 == c0 && p.c1 == c1 && p.c2 == c2;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(c0, c1, c2);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"({c0}, {c1}, {c2})";
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator -(in Vector3 vector) => new(-vector.c0, -vector.c1, -vector.c2);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator +(in Vector3 left, in Vector3 right) => new(left.c0 + right.c0, left.c1 + right.c1, left.c2 + right.c2);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator -(in Vector3 left, in Vector3 right) => new(left.c0 - right.c0, left.c1 - right.c1, left.c2 - right.c2);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator *(Dec left, in Vector3 right) => new(left * right.c0, left * right.c1, left * right.c2);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator *(Shift left, in Vector3 right) => new(left * right.c0, left * right.c1, left * right.c2);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out Dec c0, out Dec c1, out Dec c2)
    {
        c0 = this.c0;
        c1 = this.c1;
        c2 = this.c2;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector3(in (Dec c0, Dec c1, Dec c2) p) => new(p.c0, p.c1, p.c2);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"({1 / r0}, {1 / r1}, {1 / r2})";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Recipro(in Vector3 v) => new(1 / v.c0, 1 / v.c1, 1 / v.c2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector3(in Recipro r) => new(1 / r.r0, 1 / r.r1, 1 / r.r2);
    }
}

[Serializable]
public readonly struct Vector4
{
    public readonly Dec c0, c1, c2, c3;
    public bool IsZero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => c0 == 0 && c1 == 0 && c2 == 0 && c3 == 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector4(Dec c0, Dec c1, Dec c2, Dec c3)
    {
        this.c0 = c0;
        this.c1 = c1;
        this.c2 = c2;
        this.c3 = c3;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is Vector4 p && p.c0 == c0 && p.c1 == c1 && p.c2 == c2 && p.c3 == c3;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(c0, c1, c2, c3);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"({c0}, {c1}, {c2}, {c3})";
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator -(in Vector4 vector) => new(-vector.c0, -vector.c1, -vector.c2, -vector.c3);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator +(in Vector4 left, in Vector4 right) => new(left.c0 + right.c0, left.c1 + right.c1, left.c2 + right.c2, left.c3 + right.c3);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator -(in Vector4 left, in Vector4 right) => new(left.c0 - right.c0, left.c1 - right.c1, left.c2 - right.c2, left.c3 - right.c3);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator *(Dec left, in Vector4 right) => new(left * right.c0, left * right.c1, left * right.c2, left * right.c3);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator *(Shift left, in Vector4 right) => new(left * right.c0, left * right.c1, left * right.c2, left * right.c3);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out Dec c0, out Dec c1, out Dec c2, out Dec c3)
    {
        c0 = this.c0;
        c1 = this.c1;
        c2 = this.c2;
        c3 = this.c3;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector4(in (Dec c0, Dec c1, Dec c2, Dec c3) p) => new(p.c0, p.c1, p.c2, p.c3);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"({1 / r0}, {1 / r1}, {1 / r2}, {1 / r3})";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Recipro(in Vector4 v) => new(1 / v.c0, 1 / v.c1, 1 / v.c2, 1 / v.c3);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector4(in Recipro r) => new(1 / r.r0, 1 / r.r1, 1 / r.r2, 1 / r.r3);
    }
}

[Serializable]
public readonly struct Vector5
{
    public readonly Dec c0, c1, c2, c3, c4;
    public bool IsZero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => c0 == 0 && c1 == 0 && c2 == 0 && c3 == 0 && c4 == 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector5(Dec c0, Dec c1, Dec c2, Dec c3, Dec c4)
    {
        this.c0 = c0;
        this.c1 = c1;
        this.c2 = c2;
        this.c3 = c3;
        this.c4 = c4;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is Vector5 p && p.c0 == c0 && p.c1 == c1 && p.c2 == c2 && p.c3 == c3 && p.c4 == c4;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(c0, c1, c2, c3, c4);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"({c0}, {c1}, {c2}, {c3}, {c4})";
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector5 operator -(in Vector5 vector) => new(-vector.c0, -vector.c1, -vector.c2, -vector.c3, -vector.c4);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector5 operator +(in Vector5 left, in Vector5 right) => new(left.c0 + right.c0, left.c1 + right.c1, left.c2 + right.c2, left.c3 + right.c3, left.c4 + right.c4);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector5 operator -(in Vector5 left, in Vector5 right) => new(left.c0 - right.c0, left.c1 - right.c1, left.c2 - right.c2, left.c3 - right.c3, left.c4 - right.c4);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector5 operator *(Dec left, in Vector5 right) => new(left * right.c0, left * right.c1, left * right.c2, left * right.c3, left * right.c4);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector5 operator *(Shift left, in Vector5 right) => new(left * right.c0, left * right.c1, left * right.c2, left * right.c3, left * right.c4);
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
    public static implicit operator Vector5(in (Dec c0, Dec c1, Dec c2, Dec c3, Dec c4) p) => new(p.c0, p.c1, p.c2, p.c3, p.c4);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"({1 / r0}, {1 / r1}, {1 / r2}, {1 / r3}, {1 / r4})";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Recipro(in Vector5 v) => new(1 / v.c0, 1 / v.c1, 1 / v.c2, 1 / v.c3, 1 / v.c4);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector5(in Recipro r) => new(1 / r.r0, 1 / r.r1, 1 / r.r2, 1 / r.r3, 1 / r.r4);
    }
}

[Serializable]
public readonly struct Vector6
{
    public readonly Dec c0, c1, c2, c3, c4, c5;
    public bool IsZero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => c0 == 0 && c1 == 0 && c2 == 0 && c3 == 0 && c4 == 0 && c5 == 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector6(Dec c0, Dec c1, Dec c2, Dec c3, Dec c4, Dec c5)
    {
        this.c0 = c0;
        this.c1 = c1;
        this.c2 = c2;
        this.c3 = c3;
        this.c4 = c4;
        this.c5 = c5;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is Vector6 p && p.c0 == c0 && p.c1 == c1 && p.c2 == c2 && p.c3 == c3 && p.c4 == c4 && p.c5 == c5;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(c0, c1, c2, c3, c4, c5);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"({c0}, {c1}, {c2}, {c3}, {c4}, {c5})";
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector6 operator -(in Vector6 vector) => new(-vector.c0, -vector.c1, -vector.c2, -vector.c3, -vector.c4, -vector.c5);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector6 operator +(in Vector6 left, in Vector6 right) => new(left.c0 + right.c0, left.c1 + right.c1, left.c2 + right.c2, left.c3 + right.c3, left.c4 + right.c4, left.c5 + right.c5);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector6 operator -(in Vector6 left, in Vector6 right) => new(left.c0 - right.c0, left.c1 - right.c1, left.c2 - right.c2, left.c3 - right.c3, left.c4 - right.c4, left.c5 - right.c5);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector6 operator *(Dec left, in Vector6 right) => new(left * right.c0, left * right.c1, left * right.c2, left * right.c3, left * right.c4, left * right.c5);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector6 operator *(Shift left, in Vector6 right) => new(left * right.c0, left * right.c1, left * right.c2, left * right.c3, left * right.c4, left * right.c5);
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
    public static implicit operator Vector6(in (Dec c0, Dec c1, Dec c2, Dec c3, Dec c4, Dec c5) p) => new(p.c0, p.c1, p.c2, p.c3, p.c4, p.c5);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"({1 / r0}, {1 / r1}, {1 / r2}, {1 / r3}, {1 / r4}, {1 / r5})";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Recipro(in Vector6 v) => new(1 / v.c0, 1 / v.c1, 1 / v.c2, 1 / v.c3, 1 / v.c4, 1 / v.c5);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector6(in Recipro r) => new(1 / r.r0, 1 / r.r1, 1 / r.r2, 1 / r.r3, 1 / r.r4, 1 / r.r5);
    }
}

[Serializable]
public readonly struct Vector7
{
    public readonly Dec c0, c1, c2, c3, c4, c5, c6;
    public bool IsZero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => c0 == 0 && c1 == 0 && c2 == 0 && c3 == 0 && c4 == 0 && c5 == 0 && c6 == 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector7(Dec c0, Dec c1, Dec c2, Dec c3, Dec c4, Dec c5, Dec c6)
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
    public override bool Equals(object? obj) => obj is Vector7 p && p.c0 == c0 && p.c1 == c1 && p.c2 == c2 && p.c3 == c3 && p.c4 == c4 && p.c5 == c5 && p.c6 == c6;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(c0, c1, c2, c3, c4, c5, c6);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"({c0}, {c1}, {c2}, {c3}, {c4}, {c5}, {c6})";
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector7 operator -(in Vector7 vector) => new(-vector.c0, -vector.c1, -vector.c2, -vector.c3, -vector.c4, -vector.c5, -vector.c6);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector7 operator +(in Vector7 left, in Vector7 right) => new(left.c0 + right.c0, left.c1 + right.c1, left.c2 + right.c2, left.c3 + right.c3, left.c4 + right.c4, left.c5 + right.c5, left.c6 + right.c6);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector7 operator -(in Vector7 left, in Vector7 right) => new(left.c0 - right.c0, left.c1 - right.c1, left.c2 - right.c2, left.c3 - right.c3, left.c4 - right.c4, left.c5 - right.c5, left.c6 - right.c6);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector7 operator *(Dec left, in Vector7 right) => new(left * right.c0, left * right.c1, left * right.c2, left * right.c3, left * right.c4, left * right.c5, left * right.c6);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector7 operator *(Shift left, in Vector7 right) => new(left * right.c0, left * right.c1, left * right.c2, left * right.c3, left * right.c4, left * right.c5, left * right.c6);
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
    public static implicit operator Vector7(in (Dec c0, Dec c1, Dec c2, Dec c3, Dec c4, Dec c5, Dec c6) p) => new(p.c0, p.c1, p.c2, p.c3, p.c4, p.c5, p.c6);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"({1 / r0}, {1 / r1}, {1 / r2}, {1 / r3}, {1 / r4}, {1 / r5}, {1 / r6})";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Recipro(in Vector7 v) => new(1 / v.c0, 1 / v.c1, 1 / v.c2, 1 / v.c3, 1 / v.c4, 1 / v.c5, 1 / v.c6);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector7(in Recipro r) => new(1 / r.r0, 1 / r.r1, 1 / r.r2, 1 / r.r3, 1 / r.r4, 1 / r.r5, 1 / r.r6);
    }
}

[Serializable]
public readonly struct Vector8
{
    public readonly Dec c0, c1, c2, c3, c4, c5, c6, c7;
    public bool IsZero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => c0 == 0 && c1 == 0 && c2 == 0 && c3 == 0 && c4 == 0 && c5 == 0 && c6 == 0 && c7 == 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector8(Dec c0, Dec c1, Dec c2, Dec c3, Dec c4, Dec c5, Dec c6, Dec c7)
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
    public override bool Equals(object? obj) => obj is Vector8 p && p.c0 == c0 && p.c1 == c1 && p.c2 == c2 && p.c3 == c3 && p.c4 == c4 && p.c5 == c5 && p.c6 == c6 && p.c7 == c7;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine(c0, c1, c2, c3, c4, c5, c6, c7);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"({c0}, {c1}, {c2}, {c3}, {c4}, {c5}, {c6}, {c7})";
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector8 operator -(in Vector8 vector) => new(-vector.c0, -vector.c1, -vector.c2, -vector.c3, -vector.c4, -vector.c5, -vector.c6, -vector.c7);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector8 operator +(in Vector8 left, in Vector8 right) => new(left.c0 + right.c0, left.c1 + right.c1, left.c2 + right.c2, left.c3 + right.c3, left.c4 + right.c4, left.c5 + right.c5, left.c6 + right.c6, left.c7 + right.c7);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector8 operator -(in Vector8 left, in Vector8 right) => new(left.c0 - right.c0, left.c1 - right.c1, left.c2 - right.c2, left.c3 - right.c3, left.c4 - right.c4, left.c5 - right.c5, left.c6 - right.c6, left.c7 - right.c7);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector8 operator *(Dec left, in Vector8 right) => new(left * right.c0, left * right.c1, left * right.c2, left * right.c3, left * right.c4, left * right.c5, left * right.c6, left * right.c7);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector8 operator *(Shift left, in Vector8 right) => new(left * right.c0, left * right.c1, left * right.c2, left * right.c3, left * right.c4, left * right.c5, left * right.c6, left * right.c7);
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
    public static implicit operator Vector8(in (Dec c0, Dec c1, Dec c2, Dec c3, Dec c4, Dec c5, Dec c6, Dec c7) p) => new(p.c0, p.c1, p.c2, p.c3, p.c4, p.c5, p.c6, p.c7);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"({1 / r0}, {1 / r1}, {1 / r2}, {1 / r3}, {1 / r4}, {1 / r5}, {1 / r6}, {1 / r7})";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Recipro(in Vector8 v) => new(1 / v.c0, 1 / v.c1, 1 / v.c2, 1 / v.c3, 1 / v.c4, 1 / v.c5, 1 / v.c6, 1 / v.c7);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector8(in Recipro r) => new(1 / r.r0, 1 / r.r1, 1 / r.r2, 1 / r.r3, 1 / r.r4, 1 / r.r5, 1 / r.r6, 1 / r.r7);
    }
}

#pragma warning restore CA2231 // 値型 Equals のオーバーライドで、演算子 equals をオーバーロードします