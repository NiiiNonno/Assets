using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using IS = System.Runtime.InteropServices;

namespace Nonno.Assets.Scrolls;
public class BufferSection : Section, IDisposable
{
    readonly nint _ptr;
    readonly int _len;
    bool _isEmpty;
    int _strI;
    int _endI;
    bool _isDisposed;

    public override bool IsEmpty => _isEmpty;
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
    public override SectionMode Mode { get; set; }

    public BufferSection(int length)
    {
        _ptr = IS::Marshal.AllocHGlobal(length);
        _len = length;
    }
    public BufferSection(nint ptr, int length)
    {
        _ptr = ptr;
        _len = length;
    }

    public override int Read(Span<byte> span)
    {
        if (span.Length == 0) return 0;
        if (_isDisposed) throw new ObjectDisposedException(nameof(BufferSection));
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
    public override Task<int> ReadAsync(Memory<byte> memory)
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
    public unsafe override int Write(ReadOnlySpan<byte> span)
    {
        if (span.Length == 0) return 0;
#if DEBUG
        if (_isDisposed) throw new ObjectDisposedException(nameof(BufferSection));
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
    public override Task<int> WriteAsync(ReadOnlyMemory<byte> memory)
    {
        return Task.FromResult(Write(memory.Span));
    }

    public override void Clear()
    {
        if (_isDisposed) throw new ObjectDisposedException(nameof(BufferSection));

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
            }

            IS::Marshal.FreeHGlobal(_ptr);
            _isDisposed = true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void CopyTo(Span<byte> to, int startIndex, int length) => new Span<byte>((void*)(_ptr + startIndex), length).CopyTo(to);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void CopyFrom(ReadOnlySpan<byte> from, int startIndex, int length) => from.CopyTo(new Span<byte>((void*)(_ptr + startIndex), length));

    ~BufferSection()
    {
        IS::Marshal.FreeHGlobal(_ptr);
    }
}