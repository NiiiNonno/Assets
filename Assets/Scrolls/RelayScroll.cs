//#define USE_BYTE_SPAN

namespace Nonno.Assets.Scrolls;

public sealed class RelayScroll : IScroll
{
    IScroll? _target;

    public IScroll? Target
    {
        get => _target;
        set
        {
            if (value == this) throw new ArgumentException("中継先を自身で循環参照させることはできません。");
            _target = value;
        }
    }

    public void Detarget()
    {
        if (_target == null) throw new InvalidOperationException("既に中継巻子は解放されています。行程の混乱が起こっている可能性があります。");
        _target = null;
    }

#nullable disable
    public ScrollPointer Point { get => _target.Point; set => _target.Point = value; }
    public void Dispose() => _target.Dispose();
    //public ValueTask DisposeAsync() => _target.DisposeAsync();
    public bool IsValid(ScrollPointer pointer) => _target.IsValid(pointer);
    public bool Is(ScrollPointer on) => _target.Is(on);
    public void Insert(in ScrollPointer pointer) => _target.Insert(pointer: pointer);
    public void Insert<T>(Span<T> span) where T : unmanaged => _target.Insert(span);
    public Task InsertAsync<T>(Memory<T> memory, CancellationToken token = default) where T : unmanaged => _target.InsertAsync(memory, token);
    public void Remove(out ScrollPointer pointer) => _target.Remove(pointer: out pointer);
    public void Remove<T>(Span<T> span) where T : unmanaged => _target.Remove(span);
    public Task RemoveAsync<T>(Memory<T> memory, CancellationToken token = default) where T : unmanaged => _target.RemoveAsync(memory, token);
#nullable restore
}
