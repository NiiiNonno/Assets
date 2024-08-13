using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Graphics;

public readonly record struct Point<V>(V Vector)
{
    public static Point<V> operator +(Point<V> left, V right)
    {
        if (left.Vector is Vector2 l && right is Vector2 r && l + r is V v) return new(v);
        throw new Exception();
    }
    public static Point<V> operator -(Point<V> left, V right)
    {
        if (left.Vector is Vector2 l && right is Vector2 r && l - r is V v) return new(v);
        throw new Exception();
    }
    public static V operator -(Point<V> left, Point<V> right)
    {
        if (left.Vector is Vector2 l && right.Vector is Vector2 r && l - r is V v) return v;
        throw new Exception();
    }
}
