using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Nonno.Assets.Notes;

public interface ISector
{
    /// <summary>
    /// 区画に実があるかを取得します。特に区画の作成直後もしくは削除直前に<see cref="true"/>です。
    /// </summary>
    bool IsEmpty { get; }
    /// <summary>
    /// 区画の体勢を取得、または設定します。
    /// <para>
    /// 体勢に合わない動作を行った場合、合った体勢に変更されるまで待機されるか、例外が投げられます。
    /// </para>
    /// <para>
    /// <list type="table">
    /// <listheader>
    /// <term>値</term>
    /// <description>説明</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="SectorMode.Idle"/></term>
    /// <description>区画はすぐに再開できる状態で待機します。但し休止状態から遷移した場合は相変わらず休止状態である場合があります。</description>
    /// </item>
    /// <item>
    /// <term><see cref="SectorMode.Close"/></term>
    /// <description>区画は可能な限りの資料を解放し休止します。</description>
    /// </item>
    /// <item>
    /// <term><see cref="SectorMode.Read"/></term>
    /// <description>区画から数據を読み取ることができます。</description>
    /// </item>
    /// <item>
    /// <term><see cref="SectorMode.Write"/></term>
    /// <description>区画に数據を書き込むことができます。</description>
    /// </item>
    /// </list>
    /// </para>
    /// </summary>
    SectorMode Mode { get; set; }
    /// <summary>
    /// 区画の順序付けられた番号を取得、または設定します。
    /// <para>
    /// </para>
    /// </summary>
    long Number { get; set; }
    ///// <summary>
    ///// 区画の末尾に別の区画を繋げます。
    ///// </summary>
    ///// <param name="sector">
    ///// 繋げる区画。
    ///// </param>
    //void Lead(ISector sector);
    ///// <summary>
    ///// 区画を永久に削除します。区画のためにある資料はすべて削除されます。
    ///// <para>
    ///// 実体が<see cref="IDisposable.Dispose"/>を実装する場合は、<see cref="Delete"/>の呼び出しの後にそれが呼ばれますが、<see cref="IDisposable.Dispose"/>が実体の所持する参照の解放であって場合によって復元可能であるのに対し、<see cref="Delete"/>は区画のための資料をすべて削除します。
    ///// </para>
    ///// </summary>
    //void Delete();
    int Read(Span<byte> span);
    Task<int> ReadAsync(Memory<byte> memory);
    int Write(ReadOnlySpan<byte> span);
    Task<int> WriteAsync(ReadOnlyMemory<byte> memory);
}

public class BufferSector : ISector, IDisposable
{
    readonly nint _ptr;
    readonly int _len;
    bool _isEmpty;
    int _hOs;
    int _eOs;
    bool _isDisposed;

    public bool IsEmpty => _isEmpty;
    public NotePointer Pointer => new NotePointer(number: _ptr);
    public int Length
    {
        get
        {
            var r = _eOs - _hOs;
            if (r < 0) return _len - r;
            if (r == 0) return _isEmpty ? 0 : _len;
            return r;
        }
    }
    public SectorMode Mode { get; set; }
    public long Number { get; set; }

    public BufferSector(int length)
    {
        _ptr = Marshal.AllocHGlobal(length);
        _len = length;
    }

    public unsafe int Read(Span<byte> span)
    {
        if (span.Length == 0) return 0;
        var length = Length;

        var restL = _len - _hOs;
        if (span.Length < length) // 足りる場合、
        {
            if (span.Length < restL) // 残りで足りる場合、
            {
                new Span<byte>((void*)(_ptr + _hOs), restL).CopyTo(span);
                _hOs += span.Length;
            }
            else // 残りでは足りない場合、
            {
                new Span<byte>((void*)(_ptr + _hOs), restL).CopyTo(span);
                var pS = span[restL..];
                new Span<byte>((void*)_ptr, _len).CopyTo(pS);
                _hOs = pS.Length;
            }

            return span.Length;
        }
        else // 足りない場合、
        {
            if (span.Length < restL) // 折り返さない場合
            {
                new Span<byte>((void*)(_ptr + _hOs), length).CopyTo(span);
            }
            else // 折り返す場合
            {
                new Span<byte>((void*)(_ptr + _hOs), restL).CopyTo(span);
                var pS = span[restL..];
                new Span<byte>((void*)_ptr, _eOs).CopyTo(pS);
            }
            
            _hOs = _eOs = 0;
            _isEmpty = true;
            return length;
        }
    }
    public Task<int> ReadAsync(Memory<byte> memory)
    {
        return Task<int>.FromResult(Read(memory.Span));
    }
    public int Write(ReadOnlySpan<byte> span)
    {
        if (span.Length == 0) return 0;
        var length = _len - Length;
        
        _isEmpty = false;

        var restL = _len - _e0s;
        if (span.Length < length) // 足りる場合、
        {
            if (span.Length < restL) // 残りで足りる場合、
            {
                span.CopyTo(new Span<byte>((void*)(_ptr + _eOs), restL));
                _eOs += span.Length;
            }
            else // 残りでは足りない場合、
            {
                span.CopyTo(new Span<byte>((void*)(_ptr + _eOs), restL));
                var pS = span[restL..];
                pS.CopyTo(new Span<byte>((void*)_ptr, _len));
                _eOs = pS.Length;
            }

            return span.Length;
        }
        else // 足りない場合、
        {
            if (span.Length < restL) // 折り返さない場合
            {
                span.CopyTo(new Span<byte>((void*)(_ptr + _eOs), length));
            }
            else // 折り返す場合
            {
                span.CopyTo(new Span<byte>((void*)(_ptr + _eOs), restL));
                var pS = span[restL..];
                pS.CopyTo(new Span<byte>((void*)_ptr, _hOs));
            }
            
            _eOs = _hOs;
            return length;
        }
    }
    public Task<int> WriteAsync(ReadOnlyMemory<byte> memory)
    {
        return Task<int>.ResultFrom(Write(memory.Span));
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

    ~BufferSector()
    {
        Marshal.FreeHGlobal(_ptr);
    }
}
