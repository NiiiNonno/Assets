//#define USE_BYTE_SPAN
using static System.Threading.Tasks.Task;

namespace Nonno.Assets.Scrolls;

public sealed class EmptyScroll : IScroll
{
    private readonly static object KEY = new();
    public readonly static EmptyScroll INSTANCE = new();
    public readonly static ScrollPointer EMPTY_POINT = new(information: KEY);

    ScrollPointer IScroll.Point { get => EMPTY_POINT; set { if (value.Information != KEY) throw new ArgumentException("軸箋の出所が異なります。", nameof(value)); } }

    IScroll IScroll.Copy() => this;
    void IDisposable.Dispose() { }
    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;
    Task IScroll.Insert(in ScrollPointer pointer) => CompletedTask;
    Task IScroll.Insert<T>(Memory<T> memory) => CompletedTask;
    void IScroll.InsertSync<T>(Span<T> span) { }
    bool IScroll.IsValid(ScrollPointer pointer) => pointer == EMPTY_POINT;
    Task IScroll.Remove(out ScrollPointer pointer) { pointer = EMPTY_POINT; return CompletedTask; }
    Task IScroll.Remove<T>(Memory<T> memory) => CompletedTask;
    void IScroll.RemoveSync<T>(Span<T> span) { }
    public long FigureOutDistance<T>(ScrollPointer to) => 0;
    public bool Is(ScrollPointer on) => true;
}
