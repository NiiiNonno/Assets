using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Scrolls;
public class IndependentScroll : IScroll
{
    const int DEFAULT_BUFFER_SIZE = 256;

    readonly IScroll _base;
    readonly Marshal _sM;
    readonly Marshal _dM;

    public IScroll BaseScroll => _base;
    public ScrollPointer Point { get => _base.Point; set => _base.Point = value; }

    public IndependentScroll(IScroll baseScroll, Marshal serializationMarshal, Marshal deserializationMarshal)
    {
        _base = baseScroll;
        _sM = serializationMarshal;
        _dM = deserializationMarshal;
    }

    public bool IsValid(ScrollPointer pointer)
    {
        return _base.IsValid(pointer);
    }

    public bool Is(ScrollPointer on)
    {
        return _base.Is(on);
    }

    public IScroll Copy()
    {
        return _base.Copy();
    }

    public Task Insert(in ScrollPointer pointer)
    {
        return _base.Insert(pointer);
    }

    public Task Remove(out ScrollPointer pointer)
    {
        return _base.Remove(out pointer);
    }

    public Task Insert<T>(Memory<T> memory) where T : unmanaged
    {
        InsertSync(span: memory.Span);
        return Task.CompletedTask;
    }
    public unsafe void InsertSync<T>(Span<T> span) where T : unmanaged
    {
        Span<byte> result = stackalloc byte[_sM.ResultSizeOf<T>()];

        fixed (T* s = span)
        fixed (byte* r = result)
        {
            void* r_ = r;

            for (int i = 0; i < span.Length; i++)
            {
                void* s_ = &s[i];
                _sM.Conduct<T>(ref s_, ref r_);

                _base.InsertSync(span: result);
            }
        }
    }

    public Task Remove<T>(Memory<T> memory) where T : unmanaged
    {
        RemoveSync(span: memory.Span);
        return Task.CompletedTask;
    }
    public unsafe void RemoveSync<T>(Span<T> span) where T : unmanaged
    {
        Span<byte> source = stackalloc byte[_dM.SourceSizeOf<T>()];

        fixed (byte* s = source)
        fixed (T* r = span)
        {
            void* s_ = s;

            for (int i = 0; i < span.Length; i++)
            {
                _base.RemoveSync(span: source);

                void* r_ = &r[i];
                _sM.Conduct<T>(ref s_, ref r_);
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing) => _base.Dispose();

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }
    protected virtual ValueTask DisposeAsync(bool disposing) => _base.DisposeAsync();
}
