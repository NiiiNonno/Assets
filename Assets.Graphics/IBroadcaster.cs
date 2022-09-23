using System.IO;
using System.Threading.Tasks;

namespace Nonno.Assets.Graphics;

public interface IBroadcaster
{
    decimal FrameRate { get; set; }
    double Quality { get; set; }

    Task GetNextFrame(Stream to, string type);
}
