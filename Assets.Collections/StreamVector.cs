using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using static Nonno.Assets.Collections.Context;

namespace Nonno.Assets.Collections;
public class StreamVector<T> : IVector<T> where T : unmanaged
{
    /// <summary>
    /// 基渠。
    /// </summary>
    Stream _str;
    long _len_bs;
    long _len_ts;
    /// <summary>
    /// 主緩衝列。
    /// </summary>
    byte[] _mbuf;
    /// <summary>
    /// 副緩衝列。一時的に過去の主緩衝列を保持する必要の時に用いる。
    /// </summary>
    byte[] _sbuf;
    /// <summary>
    /// 主緩衝列の先頭番。
    /// </summary>
    long _i_mbuf_s;
    /// <summary>
    /// 副緩衝列の先頭番。
    /// </summary>
    long _i_sbuf_s;
    long _validLen_mbuf;
    long _validLen_sbuf;
    /// <summary>
    /// 保序列否。是や則常に<see cref="GetAsync(Range)"/>が得演るように長区に亘っても数拠を保持する。
    /// </summary>
    bool _h;
    /// <summary>
    /// 最小緩衝列長度。但指構し列長度。
    /// </summary>
    int _mbl;
    bool _isChanged;

    public int Length 
    { 
        get => checked((int)LongLength); 
        set => LongLength = value; 
    }
    public long LongLength
    {
        get => _len_ts;
        set
        {
            const int LEN = 1024;
            _len_ts = value;
            _len_bs = value * Unsafe.SizeOf<T>();
            var r = _len_bs - _str.Seek(0, SeekOrigin.End);
            var s = (stackalloc byte[LEN]);
            while (true)
            {
                if (r < LEN)
                    _str.Write(s[..(int)r]);
                else
                    _str.Write(s);
                if ((r -= LEN) < 0)
                    return;
            }
        }
    }
    public Stream BaseStream
    {
        get => _str;
        set
        {
            _str = value;
            LongLength = LongLength;
        }
    }
    public int MinimumBufferLength
    {
        get => _mbl;
        set
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
            
            _mbl = value;
            var v = value * Unsafe.SizeOf<T>();
            ExtendForward(v).Wait(false);
        }
    }
    public bool Hold
    {
        get => _h;
        set => _h = value;
    }
    protected long IndexOfMainBufferStart => _i_mbuf_s;
    protected long IndexOfMainBufferEnd => _i_mbuf_s + _validLen_mbuf;
    protected long IndexOfSubBufferStart => _i_sbuf_s;
    protected long IndexOfSubBufferEnd => _i_sbuf_s + _validLen_sbuf;
    protected Memory<byte> MainBuffer => _mbuf.AsMemory(0, (int)_validLen_mbuf);
    protected Memory<byte> SubBuffer => _sbuf.AsMemory(0, (int)_validLen_sbuf);
    public T this[int index] 
    { 
        get => this[(long)index]; 
        set => this[(long)index] = value;
    }
    public T this[long index]
    {
        get => GetAsync(index).Wait();
        set => SetAsync(index, value).Wait(false);
    }
    public Span<T> this[Range range] => this[(LongRange)range];
    public Span<T> this[LongRange range] => GetAsync(range).Wait().Span;
    ReadOnlySpan<T> IReadOnlyVector<T>.this[Range range] => GetReadOnlyAsync(range).Wait().Span;
    ReadOnlySpan<T> IReadOnlyVector<T>.this[LongRange range] => GetReadOnlyAsync(range).Wait().Span;

    public StreamVector(Stream baseStream)
    {
        _str = baseStream;
        _mbuf = _sbuf = null!;
        _i_mbuf_s = _i_sbuf_s = -1;

        MinimumBufferLength = 1;
    }

    public async ValueTask<T> GetAsync(long index, CancellationToken cancellationToken = default)
    {
        var size_t = Unsafe.SizeOf<T>();
        var i_v_s = index * size_t;
        var i_v_e = i_v_s + size_t;
    retry:;
        var i_b_s = IndexOfMainBufferStart;
        var i_b_e = IndexOfMainBufferEnd;

        if (Hold)
        {
            if (i_v_s < i_b_s) await ExtendBackward(checked((int)(i_b_e - i_v_s)), cancellationToken);
            if (i_b_e < i_v_e) await ExtendForward(checked((int)(i_b_e - i_v_s)), cancellationToken);
            return DirectlyGet();
        }
        else
        {
            if (i_v_e <= i_b_s)
            {
                // 副緩衝配列が対応できる時。
                if (IndexOfSubBufferStart <= i_v_s && i_v_e <= IndexOfSubBufferEnd) { SwitchBuffer(); goto retry; }

                await FlushAsync(cancellationToken);
                await LoadAsync(Math.Min(i_v_s, i_b_s - _mbuf.LongLength), false, cancellationToken); // 上一区画内則此、否則由謄番。

                return DirectlyGet();
            }
            if (i_b_e <= i_v_s)
            {
                // 副緩衝配列が対応できる時。
                if (IndexOfSubBufferStart <= i_v_s && i_v_e <= IndexOfSubBufferEnd) { SwitchBuffer(); goto retry; }

                await FlushAsync(cancellationToken);
                await LoadAsync(i_v_e < i_b_e + _mbuf.LongLength ? i_b_e : i_v_s, false, cancellationToken); // 下一区画内則此、否則由謄番。

                return DirectlyGet();
            }

            switch (i_b_s <= i_v_s, i_v_e <= i_b_e)
            {
            case (true, true):
                return DirectlyGet();
            case (true, false):
                await FlushAsync(cancellationToken);
                SwitchBuffer();
                await LoadAsync(i_b_s - _mbuf.Length, false, cancellationToken);
                return ConcatMainHeadSubTail(i_b_s - i_v_s);
            case (false, true):
                await FlushAsync(cancellationToken);
                SwitchBuffer();
                await LoadAsync(i_b_e, false, cancellationToken);
                return ConcatMainTailSubHead(i_b_e - i_v_s);
            default:
                await FlushAsync(cancellationToken);
                await LoadAsync(i_v_s, true, cancellationToken);
                return DirectlyGet();
            }
        }

        T ConcatMainTailSubHead(long offset)
        {
            if (_validLen_mbuf != _mbuf.Length) throw new IndexOutOfRangeException("主緩衝配列が完全には読み込まれていません。");
            var o = (int)offset;
            var r = (stackalloc byte[size_t]);
            _mbuf[^o..].CopyTo(r);
            _sbuf.CopyTo(r[o..]);
            return r.AsStruct<byte, T>();
        }

        T ConcatMainHeadSubTail(long offset)
        {
            if (_validLen_sbuf != _sbuf.Length) throw new IndexOutOfRangeException("主緩衝配列が完全には読み込まれていません。");
            var o = (int)offset;
            var r = (stackalloc byte[size_t]);
            _sbuf[^o..].CopyTo(r);
            _mbuf.CopyTo(r[o..]);
            return r.AsStruct<byte, T>();
        }

        T DirectlyGet() => MainBuffer.Span[(int)(i_v_s - IndexOfMainBufferStart)..].AsStruct<byte, T>();
    }

    public async Task SetAsync(long index, T value, CancellationToken cancellationToken = default)
    {
        var size_t = Unsafe.SizeOf<T>();
        var i_v_s = index * size_t;
        var i_v_e = i_v_s + size_t;
    retry:;
        var i_b_s = IndexOfMainBufferStart;
        var i_b_e = IndexOfMainBufferEnd;

        if (Hold)
        {
            if (i_v_s < i_b_s) await ExtendBackward(checked((int)(i_b_e - i_v_s)), cancellationToken);
            if (i_b_e < i_v_e) await ExtendForward(checked((int)(i_b_e - i_v_s)), cancellationToken);
            DirectlySet(value);
            return;
        }
        else
        {
            if (i_v_e <= i_b_s)
            {
                // 副緩衝配列が対応できる時。
                if (IndexOfSubBufferStart <= i_v_s && i_v_e <= IndexOfSubBufferEnd) { SwitchBuffer(); goto retry; }

                await FlushAsync(cancellationToken);
                await LoadAsync(Math.Min(i_v_s, i_b_s - _mbuf.LongLength), false, cancellationToken); // 上一区画内則此、否則由謄番。

                DirectlySet(value);
                return;
            }
            if (i_b_e <= i_v_s)
            {
                // 副緩衝配列が対応できる時。
                if (IndexOfSubBufferStart <= i_v_s && i_v_e <= IndexOfSubBufferEnd) { SwitchBuffer(); goto retry; }

                await FlushAsync(cancellationToken);
                await LoadAsync(i_v_e < i_b_e + _mbuf.LongLength ? i_b_e : i_v_s, false, cancellationToken); // 下一区画内則此、否則由謄番。

                DirectlySet(value);
                return;
            }

            switch (i_b_s <= i_v_s, i_v_e <= i_b_e)
            {
            case (true, true):
                DirectlySet(value);
                return;
            case (true, false):
                await FlushAsync(cancellationToken);
                SwitchBuffer();
                await LoadAsync(i_b_s - _mbuf.Length, false, cancellationToken);
                DivideMainHeadSubTail(i_b_s - i_v_s, value);
                return;
            case (false, true):
                await FlushAsync(cancellationToken);
                SwitchBuffer();
                await LoadAsync(i_b_e, false, cancellationToken);
                DivideMainTailSubHead(i_b_e - i_v_s, value);
                return;
            default:
                await FlushAsync(cancellationToken);
                await LoadAsync(i_v_s, true, cancellationToken);
                DirectlySet(value);
                return;
            };
        }

        void DivideMainTailSubHead(long offset, T value)
        {
            if (_validLen_mbuf != _mbuf.Length) throw new IndexOutOfRangeException("主緩衝配列が完全には読み込まれていません。");
            var o = (int)offset;
            var s = Assets.Utils.AsSpan<T, byte>(ref value);
            var r = (stackalloc byte[size_t]);
            s.CopyTo(_mbuf[^o..]);
            s[o..].CopyTo(_sbuf);
        }

        void DivideMainHeadSubTail(long offset, T value)
        {
            if (_validLen_mbuf != _mbuf.Length) throw new IndexOutOfRangeException("主緩衝配列が完全には読み込まれていません。");
            var o = (int)offset;
            var s = Assets.Utils.AsSpan<T, byte>(ref value);
            var r = (stackalloc byte[size_t]);
            s.CopyTo(_sbuf[^o..]);
            s[o..].CopyTo(_mbuf);
        }

        void DirectlySet(T value) => Assets.Utils.AsSpan<T, byte>(ref value).CopyTo(_mbuf.AsSpan((int)(i_v_s - _i_mbuf_s), size_t));
    }

    public async ValueTask<Memory<byte, T>> GetAsync(LongRange range, CancellationToken cancellationToken = default)
    {
        var r = await GetAsyncCore(range, cancellationToken);
        _isChanged = true;
        return r;
    }

    public async ValueTask<ReadOnlyMemory<byte, T>> GetReadOnlyAsync(LongRange range, CancellationToken cancellationToken = default)
    {
        return await GetAsyncCore(range, cancellationToken);
    }

    private async ValueTask<Memory<byte, T>> GetAsyncCore(LongRange range, CancellationToken cancellationToken = default)
    {
        var size_t = Unsafe.SizeOf<T>();
        var (start, length) = range.GetOffsetAndLength(Length);
        var i_v_s = start * size_t;
        var l_v = length * size_t;
        var i_v_e = i_v_s + l_v;
        var i_b_s = _i_mbuf_s;
        var i_b_e = _i_mbuf_s + _mbuf.LongLength;

        if (i_v_e <= i_b_s || i_b_e <= i_v_s)
        {
            if (l_v > _mbuf.Length)
            {
                ArrayPool<byte>.Shared.Return(_mbuf);
                _mbuf = ArrayPool<byte>.Shared.Rent(checked((int)l_v));
            }
            await LoadAsync(i_v_s, false, cancellationToken);
            return GetDirectly();
        }

        if (i_v_s < i_b_s) await ExtendBackward(checked((int)(i_b_e - i_v_s)), cancellationToken);
        if (i_b_e < i_v_e) await ExtendForward(checked((int)(i_b_e - i_v_s)), cancellationToken);
        return GetDirectly();

        Memory<byte, T> GetDirectly() 
        {
            return new Memory<byte, T>(_mbuf, -i_b_s)[range];
        }
    }

    /// <summary>
    /// 替現主現副緩衝配列。
    /// </summary>
    protected void SwitchBuffer()
    {
        Debug.Assert(!_isChanged);

        var minBufLen = MinimumBufferLength * Unsafe.SizeOf<T>();
        var b_m = _mbuf;
        var s_m = _i_mbuf_s;
        var vl_m = _validLen_mbuf;
        // 若副緩衝配列小於最小緩衝配列長、更副緩衝配列。
        if (_sbuf.Length < minBufLen)
        {
            _mbuf = ArrayPool<byte>.Shared.Rent(minBufLen);
            _i_mbuf_s = -1;
            _validLen_mbuf = 0;
            if (_sbuf.Length != 0) ArrayPool<byte>.Shared.Return(_sbuf);
        }
        else
        {
            _mbuf = _sbuf;
            _i_mbuf_s = _i_sbuf_s;
            _validLen_mbuf = _validLen_sbuf;
        }
        _sbuf = b_m;
        _i_sbuf_s = s_m;
        _validLen_sbuf = vl_m;
    }

    public int GetIndex(T of) => GetIndex(of, 0);
    public int GetIndex(T of, int start = 0) => GetIndexAsync(of, start).Wait(false);
    public async Task<int> GetIndexAsync(T of, int start = 0, CancellationToken cancellationToken = default)
    {
        var size_t = Unsafe.SizeOf<T>();

        for (int i = start; i * size_t < _len_ts; i++)
        {
            if (EqualityComparer<T>.Default.Equals(await GetAsync(i, cancellationToken), of)) return i;
        }
        return -1;
    }

    public int GetIndex(ReadOnlySpan<T> of) => GetIndex(of, 0);
    public int GetIndex(ReadOnlySpan<T> of, int start = 0)
    {
        var size_t = Unsafe.SizeOf<T>();
        var c = 0;

        for (int i = start; i * size_t < _len_ts; i++)
        {
            if (EqualityComparer<T>.Default.Equals(this[i], of[c]))
            {
                if (++c == of.Length) return i - of.Length + 1;
            }
            else
            {
                c = 0;
            }
        }
        return -1;
    }
    public async Task<int> GetIndexAsync(ReadOnlyMemory<T> of, int start = 0, CancellationToken cancellationToken = default)
    {
        var size_t = Unsafe.SizeOf<T>();
        var c = 0;

        for (int i = start; i * size_t < _len_ts; i++)
        {
            if (EqualityComparer<T>.Default.Equals(await GetAsync(i, cancellationToken), of.Span[c]))
            {
                if (++c == of.Length) return i - of.Length + 1;
            }
            else
            {
                c = 0;
            }
        }
        return -1;
    }

    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        if (!_isChanged) return;

        _isChanged = false;

        _str.Position = IndexOfMainBufferStart;
        await _str.WriteAsync(MainBuffer, cancellationToken);
        await _str.FlushAsync(cancellationToken);
    }

    public async Task LoadAsync(long index, bool forceUpdate = true, CancellationToken cancellationToken = default)
    {
        Debug.Assert(!_isChanged);
        
        if (!forceUpdate && index == _i_mbuf_s) return;
        _str.Position = _i_mbuf_s = index;
        _validLen_mbuf = Math.Min(await _str.ReadAsync(_mbuf, cancellationToken), _len_bs - _i_mbuf_s);
    }
    /// <summary>
    /// 緩衝配列を前方に延伸します。
    /// </summary>
    /// <param name="minLen"></param>
    /// <returns>延伸した長度</returns>
    private async Task ExtendForward(int minLen = 0, CancellationToken cancellationToken = default)
    {
        var len = Math.Max(minLen, _mbuf.Length * 2);
        var b_m = _mbuf;
        _mbuf = ArrayPool<byte>.Shared.Rent(len);
        var r = _mbuf.Length - b_m.Length;
        Array.Copy(b_m, _mbuf, b_m.Length);
        _validLen_mbuf = Math.Min(await _str.ReadAsync(_mbuf.AsMemory(b_m.Length, r), cancellationToken), _len_bs - _i_mbuf_s);
        if (b_m.Length != 0) ArrayPool<byte>.Shared.Return(b_m);
    }
    /// <summary>
    /// 緩衝配列を後方に延伸します。
    /// </summary>
    /// <param name="minLen"></param>
    /// <returns>延伸した長度</returns>
    private async Task ExtendBackward(int minLen = 0, CancellationToken cancellationToken = default)
    {
        var len = Math.Max(minLen, _mbuf.Length * 2);
        var b_m = _mbuf;
        _mbuf = ArrayPool<byte>.Shared.Rent(len);
        var r = _mbuf.Length - b_m.Length;
        _i_mbuf_s -= r;
        Array.Copy(b_m, 0, _mbuf, _mbuf.Length - b_m.Length, b_m.Length);
        _validLen_mbuf = Math.Min(await _str.ReadAsync(_mbuf.AsMemory(0, r), cancellationToken), _len_bs - _i_mbuf_s);
        if (b_m.Length != 0) ArrayPool<byte>.Shared.Return(b_m);
    }

    public Enumerator GetEnumerator(long start = 0) => new(this, start);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public SkippedEnumerable Skip(long count) => new(new(this, count));

    public struct SkippedEnumerable(Enumerator etor) : IEnumerable<T>
    {
        public Enumerator GetEnumerator() => etor;
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => etor;
        IEnumerator IEnumerable.GetEnumerator() => etor;
    }

    public struct Enumerator : IEnumerator<T>
    {
        long _i;
        readonly StreamVector<T> _a;

        public readonly unsafe T Current => _a[_i];

        readonly object IEnumerator.Current => Current;

        public Enumerator(StreamVector<T> a, long start = 0)
        {
            _a = a;
            _i = start;
        }

        public void Dispose() { }
        public bool MoveNext()
        {
            _i++;
            if (_i >= _a.Length) return false;
            return true;
        }
        public void Reset() => throw new NotSupportedException();
    }
}
