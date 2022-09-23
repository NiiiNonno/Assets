using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Graphics;

public interface IVideoSource
{
    decimal Interval { get; }

    void Seek(long number);

    Task CopyNextFrame(Bitmap to);
}
