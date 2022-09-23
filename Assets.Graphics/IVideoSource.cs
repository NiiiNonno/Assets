namespace Nonno.Assets.Graphics;

public interface IVideoSource
{
    decimal Interval { get; }

    void Seek(long number);

    Task CopyNextFrame(Bitmap to);
}
