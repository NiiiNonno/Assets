namespace Nonno.Assets.Graphics;

public abstract class Image<T> : IImage<T>
{
    public abstract T this[Point point] { get; set; }
    T IReadOnlyImage<T>.this[Point point] => this[point];

    public abstract Point MinimumPoint { get; }
    public abstract Point MaximumPoint { get; }

    public virtual void Get(Span<T> to, Point startPoint, Point endPoint)
    {
        int c = 0;
        for (int y = startPoint.Y; y < endPoint.Y; y++)
        {
            for (int x = startPoint.X; x < endPoint.X; x++)
            {
                to[c++] = this[new(x, y)];
            }
        }
    }
    public virtual Task GetAsync(Memory<T> to, Point startPoint, Point endPoint) 
    { 
        Get(to.Span, startPoint, endPoint); 
        return Task.CompletedTask; 
    }
    public virtual void Set(ReadOnlySpan<T> from, Point startPoint, Point endPoint)
    {
        int c = 0;
        for (int y = startPoint.Y; y < endPoint.Y; y++)
        {
            for (int x = startPoint.X; x < endPoint.X; x++)
            {
                this[new(x, y)] = from[c++];
            }
        }
    }
    public virtual Task SetAsync(ReadOnlyMemory<T> from, Point startPoint, Point endPoint)
    {
        Set(from.Span, startPoint, endPoint);
        return Task.CompletedTask;
    }
}
