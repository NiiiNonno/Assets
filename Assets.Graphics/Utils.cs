using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Nonno.Assets.Scrolls;

namespace Nonno.Assets.Graphics;

public static partial class Utils
{
    //public static Bitmap ToBitmap(this ICsio @this, int startIndex, int length, int scaleFactor)
    //{
    //    var span = @this.GetCodeSpan(startIndex - 1, length + 2);
    //    var r = new Bitmap() { Range = new Range(scaleFactor * 16, scaleFactor * length) };

    //    throw new NotImplementedException();

    //    for (int i = 2; i < span.Length; i++)
    //    {
    //        var a = span[i - 2];
    //        var b = span[i - 1];
    //        var c = span[i];

    //        r.GetRaster<Color>(i, r.Stride);
    //    }

    //    return r;
    //}
}
