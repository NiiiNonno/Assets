using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TPoint = Nonno.Assets.Graphics.Point<System.Numerics.Vector2>;

namespace Nonno.Assets.Graphics;
public class BitmapLinePen : IColorPen
{
    readonly ColorBitmap _bitmap;
    public TPoint Start { get; set; }
    public TPoint End { get; set; }
    public Vector2 Velocity { get; set; }
    public int ColorNumber { get; set; }

    public BitmapLinePen(ColorBitmap bitmap)
    {
        _bitmap = bitmap;
    }

    public void DrawLine(TPoint start, TPoint end)
    {
        DrawLineCore(start, end, GetColor(0));
    }
    void DrawLineCore(TPoint start, TPoint end, RGBAColor32 color)
    {
        var v = end - start;
        var l = v.Length();
        var n = v / l;

        var c = start;
        for (float i = 0; i < l; i++)
        {
            int x = (int)c.Vector.X;
            int y = (int)c.Vector.Y;
            if (x < 0 || _bitmap.Width <= x) continue;
            if (y < 0 || _bitmap.Height <= y) continue;

            if (color.Alpha != 0)
            {
                var r1 = Byte.MaxValue - color.Alpha;
                var r0 = color.Alpha;

                var ae = _bitmap[x, y];
                _bitmap[x, y] = new(
                    (ae.Red * r0 + color.Red * r1) >>> 8,
                    (ae.Green * r0 + color.Green * r1) >>> 8,
                    (ae.Blue * r0 + color.Blue * r1) >>> 8,
                    ae.Alpha);
            }
            else
            {
                _bitmap[x, y] = color;
            }
            c += n;
        }

        End = end;
    }

    public void DrawArc(TPoint start, TPoint end, TPoint center)
    {
        DrawLineCore(start, end, GetColor(150));
    }

    public void DrawQuad(TPoint start, TPoint cp, TPoint end)
    {
        DrawLineCore(start, cp, GetColor(100));
        DrawLineCore(cp, end, GetColor(100));
    }

    public void DrawCurve(TPoint start, TPoint cp1, TPoint cp2, TPoint end)
    {
        DrawLineCore(start, cp1, GetColor(50));
        DrawLineCore(cp1, cp2, GetColor(50));
        DrawLineCore(cp2, end, GetColor(50));
    }

    public void DrawCurve(TPoint start, ReadOnlySpan<TPoint> cps, TPoint end)
    {
        DrawLineCore(start, cps[0], GetColor(50));
        for (int i = 1; i < cps.Length; i++)
            DrawLineCore(cps[i - 1], cps[i], GetColor(50));
        DrawLineCore(cps[^1], end, GetColor(50));
    }

    public RGBAColor32 GetColor(int alpha) => ColorNumber switch
    {
        1 => new(250, 100, 100, alpha),
        2 => new(200, 150, 100, alpha),
        3 => new(150, 200, 100, alpha),
        4 => new(100, 250, 100, alpha),
        5 => new(100, 200, 150, alpha),
        6 => new(100, 150, 200, alpha),
        7 => new(100, 100, 250, alpha),
        8 => new(150, 100, 200, alpha),
        9 => new(200, 100, 150, alpha),
        _ => new(240, 240, 240, alpha),
    };
}
