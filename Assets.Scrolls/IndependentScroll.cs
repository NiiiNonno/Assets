using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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

    public void Insert(in ScrollPointer pointer)
    {
        _base.Insert(pointer);
    }

    public void Remove(out ScrollPointer pointer)
    {
        _base.Remove(out pointer);
    }

    public Task InsertAsync<T>(Memory<T> memory, CancellationToken token = default)where T : unmanaged
    {
        Insert(span: memory.Span);
        return Task.CompletedTask;
    }
    public unsafe void Insert<T>(Span<T> span) where T : unmanaged
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

                _base.Insert(span: result);
            }
        }
    }

    public Task RemoveAsync<T>(Memory<T> memory, CancellationToken token = default) where T : unmanaged
    {
        Remove(span: memory.Span);
        return Task.CompletedTask;
    }
    public unsafe void Remove<T>(Span<T> span) where T : unmanaged
    {
        Span<byte> source = stackalloc byte[_dM.SourceSizeOf<T>()];

        fixed (byte* s = source)
        fixed (T* r = span)
        {
            void* s_ = s;

            for (int i = 0; i < span.Length; i++)
            {
                _base.Remove(span: source);

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
}
