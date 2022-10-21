using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Nonno.Assets.Scrolls;
public class BufferSector : ISector, IDisposable
{
    readonly nint _ptr;
    readonly int _len;
    bool _isEmpty;
    int _strI;
    int _endI;
    bool _isDisposed;

    public bool IsEmpty => _isEmpty;
    public ScrollPointer Pointer => new(number: _ptr);
    public int Length
    {
        get
        {
            var r = _endI - _strI;
            if (r < 0) return _len - r;
            if (r == 0) return _isEmpty ? 0 : _len;
            return r;
        }
    }
    public SectorMode Mode { get; set; }
    public long Number { get; set; }

    public BufferSector(int length, long number)
    {
        _ptr = Marshal.AllocHGlobal(length);
        _len = length;

        Number = number;
    }

    public int Read(Span<byte> span)
    {
        if (span.Length == 0) return 0;
        if (_isDisposed) throw new ObjectDisposedException(nameof(BufferSector));
        var length = Length;

        var restL = _len - _strI;
        if (span.Length < length) // 足りる場合、長さはあまり気にせずに、
        {
            if (span.Length < restL) // 残りで足りる場合、
            {
                CopyTo(span, _strI, restL);
                _strI += span.Length;
            }
            else // 残りでは足りない場合、
            {
                CopyTo(span, _strI, restL);
                var pS = span[restL..];
                CopyTo(pS, 0, _len);
                _strI = pS.Length;
            }

            return span.Length;
        }
        else // 足りない場合、
        {
            if (span.Length < restL) // 折り返さない場合
            {
                CopyTo(span, _strI, length);
            }
            else // 折り返す場合
            {
                CopyTo(span, _strI, restL);
                var pS = span[restL..];
                CopyTo(pS, 0, _endI);
            }

            _strI = _endI = 0;
            _isEmpty = true;
            return length;
        }
    }
    public Task<int> ReadAsync(Memory<byte> memory)
    {
        return Task.FromResult(Read(memory.Span));
        //if (memory.Length == 0) return 0;
        //var length = Length;

        //var restL = _len - _strI;
        //if (memory.Length < length) // 足りる場合、
        //{
        //    if (memory.Length < restL) // 残りで足りる場合、
        //    {
        //        CopyTo(memory.Span, _strI, restL);
        //        _strI += memory.Length;
        //    }
        //    else // 残りでは足りない場合、
        //    {
        //        CopyTo(memory.Span, _strI, restL);
        //        var pM = memory[restL..];
        //        CopyTo(pM.Span, 0, _len);
        //        _strI = pM.Length;
        //    }

        //    return memory.Length;
        //}
        //else // 足りない場合、
        //{
        //    if (memory.Length < restL) // 折り返さない場合
        //    {
        //        CopyTo(memory.Span, _strI, length);
        //    }
        //    else // 折り返す場合
        //    {
        //        CopyTo(memory.Span, _strI, restL);
        //        var pM = memory[restL..];
        //        CopyTo(pM.Span, 0, _endI);
        //    }

        //    _strI = _endI = 0;
        //    _isEmpty = true;
        //    return length;
        //}
    }
    public unsafe int Write(ReadOnlySpan<byte> span)
    {
        if (span.Length == 0) return 0;
#if DEBUG
        if (_isDisposed) throw new ObjectDisposedException(nameof(BufferSector));
#endif
        var length = _len - Length;

        _isEmpty = false;

        var restL = _len - _endI;
        if (span.Length < length) // 足りる場合、長さはあまり気にせずに、
        {
            if (span.Length < restL) // 残りで足りる場合、
            {
                CopyFrom(span, _endI, restL);
                _endI += span.Length;
            }
            else // 残りでは足りない場合、
            {
                CopyFrom(span, _endI, restL);
                var pS = span[restL..];
                CopyFrom(pS, 0, _len);
                _endI = pS.Length;
            }

            return span.Length;
        }
        else // 足りない場合、
        {
            if (span.Length < restL) // 折り返さない場合
            {
                CopyFrom(span, _endI, length);
            }
            else // 折り返す場合
            {
                CopyFrom(span, _endI, restL);
                var pS = span[restL..];
                CopyFrom(pS, 0, _strI);
            }

            _endI = _strI;
            return length;
        }
    }
    public Task<int> WriteAsync(ReadOnlyMemory<byte> memory)
    {
        return Task.FromResult(Write(memory.Span));
    }

    public void Clear()
    {
        if (_isDisposed) throw new ObjectDisposedException(nameof(BufferSector));

#if DEBUG
        ClearMemory();
        unsafe void ClearMemory() => new Span<byte>((void*)_ptr, _len).Clear();
#endif
        _strI = _endI = 0;
        _isEmpty = true;
    }

    public void Dispose()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // TODO: マネージド状態を破棄します (マネージド オブジェクト)
            }

            Marshal.FreeHGlobal(_ptr);
            _isDisposed = true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void CopyTo(Span<byte> to, int startIndex, int length) => new Span<byte>((void*)(_ptr + startIndex), length).CopyTo(to);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void CopyFrom(ReadOnlySpan<byte> from, int startIndex, int length) => from.CopyTo(new Span<byte>((void*)(_ptr + startIndex), length));

    ~BufferSector()
    {
        Marshal.FreeHGlobal(_ptr);
    }
}

public class StreamSector : ISector
{
    readonly Stream _stream;

    public bool IsEmpty => _stream.Length == 0;

    public SectorMode Mode { get; set; }
    public long Number { get; set; }

    public StreamSector(Stream stream, long number)
    {
        _stream = stream;

        Number = number;
    }

    public int Read(Span<byte> span) => throw new NotImplementedException();
    public Task<int> ReadAsync(Memory<byte> memory) => throw new NotImplementedException();
    public int Write(ReadOnlySpan<byte> span) => throw new NotImplementedException();
    public Task<int> WriteAsync(ReadOnlyMemory<byte> memory) => throw new NotImplementedException();
}