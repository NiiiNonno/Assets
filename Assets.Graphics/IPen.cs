using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V = System.Numerics.Vector2;

namespace Nonno.Assets.Graphics;
public interface IPen
{
    Point<V> Start { get; set; }
    Point<V> End { get; set; }
    V Velocity { get; set; }

    void DrawLine(Point<V> end) => DrawLine(End, end);
    void DrawLine(Point<V> start, Point<V> end);

    void DrawArc(Point<V> end)
    {
        DrawArc(End, end, default);
    }
    void DrawArc(Point<V> end, Point<V> center) => DrawArc(End, end, center);
    void DrawArc(Point<V> start, Point<V> end, Point<V> center);

    void DrawQuad(Point<V> cp, Point<V> end) => DrawQuad(End, cp, end);
    void DrawQuad(Point<V> start, Point<V> cp, Point<V> end);

    void DrawCurve(Point<V> cp1, Point<V> cp2, Point<V> end) => DrawCurve(End, cp1, cp2, end);
    void DrawCurve(Point<V> start, Point<V> cp1, Point<V> cp2, Point<V> end);

    void DrawCurve(ReadOnlySpan<Point<V>> cps, Point<V> end) => DrawCurve(End, cps, end);
    void DrawCurve(Point<V> start, ReadOnlySpan<Point<V>> cps, Point<V> end);
}

public interface IColorPen : IPen
{
    int ColorNumber { get; set; }
}
