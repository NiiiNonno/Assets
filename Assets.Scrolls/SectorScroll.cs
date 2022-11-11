using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Nonno.Assets.Collections;

namespace Nonno.Assets.Scrolls;
public abstract class SectorScroll<TSector> : IScroll where TSector : ISector
{
    readonly SkipList<TSector> _scts;
    readonly Dictionary<ScrollPointer, SkipList<TSector>.SkipListNode> _refs;
    SkipList<TSector>.SkipListNode _cNode;
    bool _isDisposed;

    protected SkipList<TSector> Sectors => _scts;
    protected SkipList<TSector>.SkipListNode PreviousSectorNode => _cNode.PreviousNode ?? throw new Exception("不明な錯誤です。現在の区画の以前に区画が存在しませんでした。");
    protected SkipList<TSector>.SkipListNode NextSectorNode => _cNode;
    protected long PreviousSectorNumber => PreviousSectorNode?.Value.Number ?? long.MinValue;
    protected long NextSectorNumber => NextSectorNode.Value.Number;
    public int SectorCount => _scts.Count();
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public ScrollPointer Point
    {
        get
        {
            var num = Assets.Utils.AverageCeiling(PreviousSectorNumber, NextSectorNumber);
            var sct = CreateSector(num);
            var node = _scts.InsertBefore(_cNode, sct);

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

            if (NextSectorNode.Value.IsEmpty) DeleteSector(NextSectorNode.Value);

            if (!_refs.Remove(value, out var node)) throw new ArgumentException($"索引が不明です。`{nameof(ScrollPointer)}`の用法を確認してください。", nameof(value));
            if (node.Belongs(to: _scts)) throw new ArgumentException("索引が無効です。", nameof(value));
            _cNode = node;

            DestroyPointer(value);

            PreviousSectorNode.Value.Mode = SectorMode.Write;
            NextSectorNode.Value.Mode = SectorMode.Read;
        }
    }
    /// <summary>
    /// 軸箋を指定して関連付けられた区画を取得、または設定します。
    /// </summary>
    /// <param name="pointer"></param>
    /// <returns></returns>
    protected TSector? this[ScrollPointer pointer]
    {
        get => _refs.TryGetValue(pointer, out var result) ? result.Value : default;
        set
        {
            var node = value is null ? null : _scts.Find(value);
            if (node is null) 
            { 
                _refs.Remove(pointer);
                return;
            }
            if (!_refs.TryAdd(pointer, node)) _refs[pointer] = node;
        }
    }

    public SectorScroll(TSector primarySector)
    {
        _scts = new(COMPERER);
        _refs = new();
        _cNode = _scts.Insert(primarySector);
    }
    protected SectorScroll(SectorScroll<TSector> original)
    {
        throw new NotImplementedException();
    }

    public bool IsValid(ScrollPointer pointer) => _refs.TryGetValue(pointer, out var node) && !node.Belongs(to: _scts);

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
                _ = _scts.InsertBefore(_cNode, CreateSector(pSct.Number + 1));

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
            _ = _scts.InsertBefore(_cNode, CreateSector(pSct.Number + 1));

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
                _cNode = _cNode.NextNode ?? throw new Exception("巻子の末尾に到達しました。これ以上搴取するものがありません。");
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
            _cNode = _cNode.NextNode ?? throw new Exception("巻子の末尾に到達しました。これ以上搴取するものがありません。");
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

    public abstract IScroll Copy();
    public abstract Task Insert(in ScrollPointer pointer);
    public abstract Task Remove(out ScrollPointer pointer);
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
    /// 区画の有効な軸箋を作成します。
    /// </summary>
    /// <param name="of">
    /// 軸箋を作成する区画。
    /// </param>
    /// <returns>
    /// 作成した有効な軸箋。
    /// </returns>
    protected abstract ScrollPointer MakePointer(TSector of);
    /// <summary>
    /// 軸箋を破棄して無効化します。
    /// </summary>
    /// <param name="pointer">
    /// 破棄して無効化する軸箋。
    /// </param>
    protected abstract void DestroyPointer(ScrollPointer pointer);
    public bool Is(ScrollPointer on) => throw new NotImplementedException();

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

public enum SectorMode
{
    Idle, Read, Write, Close
}
