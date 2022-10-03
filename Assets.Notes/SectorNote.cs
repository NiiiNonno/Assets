using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nonno.Assets.Collections;

namespace Nonno.Assets.Notes;
public abstract class SectorNote<TSector> : INote where TSector : ISector
{
    readonly SkipList<TSector> _scts;
    readonly Dictionary<NotePointer, SkipList<TSector>.SkipListNode> _refs;
    SkipList<TSector>.SkipListNode _cNode;
    NotePointer _cPtr;
    bool _isDisposed;

    protected SkipList<TSector> Sectors => _scts;
    protected SkipList<TSector>.SkipListNode PreviousSectorNode => _cNode.PreviousNode ?? throw new Exception("不明な錯誤です。現在の区画の以前に区画が存在しませんでした。");
    protected SkipList<TSector>.SkipListNode NextSectorNode => _cNode;
    protected long PreviousSectorNumber => PreviousSectorNode?.Value.Number ?? long.MinValue;
    protected long NextSectorNumber => NextSectorNode.Value.Number;
    public int SectorCount => _scts.Count();
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public NotePointer Pointer
    {
        get
        {
            var num = Utils.AverageCeiling(PreviousSectorNumber, NextSectorNumber);
            var sct = CreateSector(num);
            var node = _cNode.InsertBefore(sct);

            _cNode.Value.Mode = SectorMode.Idle;
            node.Value.Mode = SectorMode.Write;

            if (NextSectorNumber - PreviousSectorNumber < 2) Rearrange();

            var ptr = MakePointer(of: sct);
            _refs.Add(ptr, node);
            return ptr;
        }
        set
        {
            PreviousSectorNode.Value.Mode = SectorMode.Idle;
            NextSectorNode.Value.Mode = SectorMode.Idle;

            DestroyPointer(_cPtr);
            if (NextSectorNode.Value.IsEmpty) DeleteSector(NextSectorNode.Value);

            if (!_refs.Remove(value, out var node)) throw new ArgumentException($"索引が不明です。`{nameof(NotePointer)}`の用法を確認してください。", nameof(value));
            if (node.IsRemoved) throw new ArgumentException("索引が無効です。", nameof(value));
            _cNode = node;

            PreviousSectorNode.Value.Mode = SectorMode.Write;
            NextSectorNode.Value.Mode = SectorMode.Read;
        }
    }

    public SectorNote(TSector primarySector)
    {
        _scts = new(COMPERER);
        _refs = new();
        _cNode = _scts.Insert(primarySector);
    }

    public bool IsValid(NotePointer pointer) => _refs.TryGetValue(pointer, out var node) && !node.IsRemoved;

    public virtual async Task Insert<T>(Memory<T> memory) where T : unmanaged
    {
        if (memory is Memory<byte> byteM) 
        {
            int c = 0;
            while (true)
            {
                var pSct = PreviousSectorNode.Value;
                c += await pSct.WriteAsync(byteM[c..]);
                if (c >= byteM.Length) break;
                _ = _cNode.InsertBefore(CreateSector(pSct.Number + 1));

                if (NextSectorNumber - PreviousSectorNumber < 2) Rearrange();
            }
        }

        InsertSync(memory.Span);
    }
    public virtual void InsertSync<T>(Span<T> span) where T : unmanaged
    {
        var byteS = span.ToSpan<T, byte>();

        int c = 0;
        while (true)
        {
            var pSct = PreviousSectorNode.Value;
            c += pSct.Write(byteS[c..]);
            if (c >= byteS.Length) break;
            _ = _cNode.InsertBefore(CreateSector(pSct.Number + 1));

            if (NextSectorNumber - PreviousSectorNumber < 2) Rearrange();
        }
    }

    public virtual async Task Remove<T>(Memory<T> memory) where T : unmanaged
    {
        if (memory is Memory<byte> byteM)
        {
            int c = 0;
            while (true)
            {
                c += await _cNode.Value.ReadAsync(byteM[c..]);
                if (c >= byteM.Length) break;
                DeleteSector(_cNode.Value);
                _cNode = _cNode.NextNode ?? throw new Exception("冊の末尾に到達しました。これ以上搴取するものがありません。");
            }
        }

        RemoveSync(memory.Span);
    }
    public virtual void RemoveSync<T>(Span<T> span) where T : unmanaged
    {
        var byteS = span.ToSpan<T, byte>();

        int c = 0;
        while (true)
        {
            c += _cNode.Value.Read(byteS[c..]);
            if (c >= byteS.Length) break;
            DeleteSector(_cNode.Value);
            _cNode = _cNode.NextNode ?? throw new Exception("冊の末尾に到達しました。これ以上搴取するものがありません。");
        }
    }

    public void Rearrange()
    {
        double v = SectorCount + 1;
        double d = ((double)Int64.MaxValue - Int64.MinValue) / v;
        double c = Int64.MinValue;
        long i = Int64.MinValue;
        foreach (var sct in _scts)
        {
            int c_ = (int)(c += d);
            if (c_ - i < 2) throw new InvalidOperationException("整列後の要素の間隔が2未満です。これ以上節を維持することができません。");
            i = c_;
            sct.Number = i;
        }
    }

    public abstract INote Copy();
    public abstract Task Insert(in NotePointer pointer);
    public abstract Task Remove(out NotePointer pointer);
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
                foreach (var sct in Sectors)
                {
                    if (sct is IDisposable disposable) disposable.Dispose();
                }
            }
            _isDisposed = true;
        }
    }
    public async ValueTask DisposeAsync()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        await DisposeAsync(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                foreach (var sct in Sectors)
                {
                    if (sct is IAsyncDisposable asyncDisposable) await asyncDisposable.DisposeAsync();
                    else if (sct is IDisposable disposable) disposable.Dispose();
                }
            }
            _isDisposed = true;
        }
    }

    protected abstract TSector CreateSector(long number);
    protected abstract void DeleteSector(TSector sector);

    /// <summary>
    /// 区画の有効な冊第を作成します。
    /// </summary>
    /// <param name="of">
    /// 冊第を作成する区画。
    /// </param>
    /// <returns>
    /// 作成した有効な冊第。
    /// </returns>
    protected abstract NotePointer MakePointer(TSector of);
    /// <summary>
    /// 冊第を破棄して無効化します。
    /// </summary>
    /// <param name="pointer">
    /// 破棄して無効化する冊第。
    /// </param>
    protected abstract void DestroyPointer(NotePointer pointer);

    readonly static Comperer COMPERER = new();

    class Comperer : IComparer<TSector>
    {
        public int Compare(TSector? x, TSector? y)
        {
            if (x is null || y is null) throw new InvalidOperationException();
            var d = x.Number - y.Number;
            if (d < 0) return -1;
            if (d > 0) return 1;
            return 0;
        }
    }
}

public enum SectorMode
{
    Idle, Read, Write, Close
}
