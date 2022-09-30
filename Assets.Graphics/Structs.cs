using Vintage = System.Drawing;
#if USE_DOUBLE
using Dec = System.Double;
#else
#endif

namespace Nonno.Assets.Graphics;

public readonly struct Point : IEquatable<Point>
{
    public int X { get; init; }
    public int Y { get; init; }

    public Point() { X = Y = 0; }
    public Point(Range size) : this(size.Width, size.Height) { }
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object? obj) => obj is Point point && this == point;
    public bool Equals(Point other) => this == other;

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public override string ToString() => $"(X: {X}, Y: {Y})";

    public static bool operator ==(in Point l, in Point r) => l.X == r.X && l.Y == r.Y;
    public static bool operator !=(in Point l, in Point r) => l.X != r.X || l.Y != r.Y;
    public static Point operator *(in Point l, int r) => new(l.X * r, l.Y * r);
    public static Point operator /(in Point l, int r) => new(l.X / r, l.Y / r);
    public static Point operator +(in Point l, in Point r) => new(l.X + r.X, l.Y + r.Y);
    public static Point operator -(in Point l, in Point r) => new(l.X - r.X, l.Y - r.Y);
    public static implicit operator Point(Vintage.Point p) => new(p.X, p.Y);
    public static explicit operator Vintage.Point(Point p) => new(p.X, p.Y);
}

public readonly struct Range : IEquatable<Range>
{
    public int Width { get; init; }
    public int Height { get; init; }

    public Range() { Width = Height = 0; }
    public Range(Point point) : this(point.X, point.Y) { }
    public Range(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public override bool Equals(object? obj) => obj is Range range && this == range;
    public bool Equals(Range other) => this == other;

    public override int GetHashCode() => HashCode.Combine(Width, Height);

    public override string ToString() => $"(Width: {Width}, Height: {Height})";

    public static bool operator ==(in Range l, in Range r) => l.Width == r.Width && l.Height == r.Height;
    public static bool operator !=(in Range l, in Range r) => l.Width != r.Width || l.Height != r.Height;
    public static Range operator *(in Range l, int r) => new(l.Width * r, l.Height * r);
    public static Range operator /(in Range l, int r) => new(l.Width / r, l.Height / r);
    public static Range operator +(in Range l, in Point r) => new(l.Width + r.X, l.Height + r.Y);
    public static Range operator -(in Range l, in Point r) => new(l.Width - r.X, l.Height - r.Y);
    public static implicit operator Range(Vintage.Size p) => new(p.Width, p.Height);
    public static explicit operator Vintage.Size(Range p) => new(p.Width, p.Height);
}
