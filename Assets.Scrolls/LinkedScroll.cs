using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Scrolls;

public class LinkedScroll : IScroll
{
    public const string EXTENSION = ".slk";

    public ScrollPointer Point { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public IScroll Copy()
    {
        throw new NotImplementedException();
    }

    public bool Is(ScrollPointer on)
    {
        throw new NotImplementedException();
    }

    public bool IsValid(ScrollPointer pointer)
    {
        throw new NotImplementedException();
    }

    public Task Insert(in ScrollPointer pointer)
    {
        throw new NotImplementedException();
    }
    public Task Remove(out ScrollPointer pointer)
    {
        throw new NotImplementedException();
    }

    public Task Insert<T>(Memory<T> memory) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public void InsertSync<T>(Span<T> span) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public Task Remove<T>(Memory<T> memory) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public void RemoveSync<T>(Span<T> span) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {

    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }
    protected virtual ValueTask DisposeAsync(bool disposing)
    {
        return ValueTask.CompletedTask;
    }
}
