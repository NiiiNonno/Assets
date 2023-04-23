//#define USE_BYTE_SPAN
using static System.Threading.Tasks.Task;

namespace Nonno.Assets.Scrolls;

public sealed class EmptyScroll : IScroll
{
    private readonly static object KEY = new();
    public readonly static EmptyScroll INSTANCE = new();
    public readonly static ScrollPointer EMPTY_POINT = new(information: KEY);

    ScrollPointer IScroll.Point { get => EMPTY_POINT; set { if (value.Information != KEY) throw new ArgumentException("軸箋の出所が異なります。", nameof(value)); } }

    void IDisposable.Dispose() { }
    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;
    void IScroll.Insert(in ScrollPointer pointer) {}
    Task IScroll.InsertAsync<T>(Memory<T> memory, CancellationToken token) => CompletedTask;
    void IScroll.Insert<T>(Span<T> span) { }
    bool IScroll.IsValid(ScrollPointer pointer) => pointer == EMPTY_POINT;
    void IScroll.Remove(out ScrollPointer pointer) => pointer = EMPTY_POINT;
    Task IScroll.RemoveAsync<T>(Memory<T> memory, CancellationToken token) => CompletedTask;
    void IScroll.Remove<T>(Span<T> span) { }
    public bool Is(ScrollPointer on) => true;
}
