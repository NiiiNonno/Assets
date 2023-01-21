using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Nonno.Assets.Collections;
using static System.Collections.Specialized.BitVector32;

namespace Nonno.Assets.Scrolls;
/// <summary>
/// 節巻子を表します。
/// </summary>
/// <remarks>
/// 節号は予約領域があります。
/// <list type="bullet">
/// <listheader>
/// <term>節号</term><description>用途</description>
/// </listheader>
/// <item>
/// <term><c>0</c></term><description>最初の節で、書いたものが消える場合があります。</description>
/// </item>
/// <item>
/// <term><see cref="long.MaxValue"/></term><description>最終の節であり、書き込むことができません。</description>
/// </item>
/// </list>
/// </remarks>
/// <typeparam name="TSection">
/// 節を表す型。
/// </typeparam>
public abstract class SectionScroll<TSection> : IScroll where TSection : ISection
{
    readonly HashSet<long> _floatings;
    readonly EmptySection _lastSection;
    long _current;

    /// <summary>
    /// 後方の節号を取得します。
    /// </summary>
    protected long PreviousSectionNumber => _current;
    /// <summary>
    /// 後方の節を取得します。
    /// </summary>
    protected TSection PreviousSection => this[PreviousSectionNumber];
    /// <summary>
    /// 前方の節号を取得します。
    /// </summary>
    protected long NextSectionNumber => PreviousSection.Next;
    /// <summary>
    /// 前方の節を取得します。
    /// </summary>
    protected ISection NextSection => NextSectionNumber == long.MaxValue ? this[NextSectionNumber] : _lastSection;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public ScrollPointer Point
    {
        get
        {
            InsertSection();

            _floatings.Add(_current);
            return new(_current);
        }
        set
        {
            PreviousSection.Mode = SectionMode.Idle;
            NextSection.Mode = SectionMode.Idle;

            _current = value.LongNumber;
            _floatings.Remove(_current);

            PreviousSection.Mode = SectionMode.Write;
            NextSection.Mode = SectionMode.Read;
        }
    }
    /// <summary>
    /// 節号を以て節を取得します。
    /// </summary>
    /// <param name="index">
    /// 節号。
    /// </param>
    /// <returns>
    /// 取得した節。
    /// </returns>
    protected abstract TSection this[long index] { get; }

    /// <summary>
    /// 後方の節号を指定して節巻子を初期化します。
    /// </summary>
    /// <param name="nextSection"></param>
    public SectionScroll(long currentNumber)
    {
        _floatings = new();
        _lastSection = new();
        _current = currentNumber;
    }

    public bool IsValid(ScrollPointer pointer) => _floatings.Contains(pointer.LongNumber);

    public bool Is(ScrollPointer on)
    {
        var c = _current;

        retry:;
        if (c == on.LongNumber) return true;

        if (this[c].IsEmpty) 
        { 
            RemoveSection();
            goto retry;
        }

        return false;
    }

    public abstract IScroll Copy();

    /// <summary>
    /// 後方に節を追加します。
    /// </summary>
    protected void InsertSection()
    {
        var prevSN = PreviousSectionNumber;
        var prevS = this[prevSN];
        var nextSN = NextSectionNumber;
        var nextS = this[nextSN];
        var newSN = FindVacantNumber();
        var newS = this[newSN];

        CreateSection(newSN);
        prevS.Next = newSN;
        newS.Next = nextSN;
        prevS.Mode = SectionMode.Idle;
        newS.Mode = SectionMode.Write;
        nextS.Mode = SectionMode.Read;
    }
    /// <summary>
    /// 前方から節を除去します。
    /// </summary>
    protected void RemoveSection()
    {
        var prevSN = PreviousSectionNumber;
        var prevS = this[prevSN];
        var oldSN = NextSectionNumber;
        var oldS = this[oldSN];
        var nextSN = oldS.Next;
        var nextS = this[nextSN];

        DeleteSection(oldSN);
        prevS.Next = nextSN;
        prevS.Mode = SectionMode.Write;
        nextS.Mode = SectionMode.Read;
    }

    public Task Insert(in ScrollPointer pointer)
    {
        _floatings.Remove(pointer.LongNumber);
        return this.Insert(int64: pointer.LongNumber);
    }
    public Task Remove(out ScrollPointer pointer)
    {
        var r = this.Remove(int64: out var p_n);
        _floatings.Add(p_n);
        pointer = new(p_n);
        return r;
    }

    public async Task Insert<T>(Memory<T> memory) where T : unmanaged
    {
        if (memory is Memory<byte> memory_)
        {
            int c = 0;
            while (true)
            {
                c += await PreviousSection.WriteAsync(memory_[c..]);
                if (c >= memory_.Length) break;

                InsertSection();
            }
        }

        InsertSync(memory.Span);
    }
    public void InsertSync<T>(Span<T> span) where T : unmanaged
    {
        var span_ = span.ToSpan<T, byte>();

        int c = 0;
        while (true)
        {
            c += PreviousSection.Write(span_[c..]);
            if (c >= span_.Length) break;

            InsertSection();
        }
    }

    public async Task Remove<T>(Memory<T> memory) where T : unmanaged
    {
        if (memory is Memory<byte> memory_)
        {
            int c = 0;
            while (true)
            {
                c += await NextSection.ReadAsync(memory_[c..]);
                if (c >= memory_.Length) break;

                RemoveSection();
            }
        }

        RemoveSync(memory.Span);
    }
    public void RemoveSync<T>(Span<T> span) where T : unmanaged
    {
        var span_ = span.ToSpan<T, byte>();

        int c = 0;
        while (true)
        {
            c += NextSection.Read(span_[c..]);
            if (c >= span_.Length) break;

            RemoveSection();
        }
    }

    /// <summary>
    /// 新たな節号から節を作成します。
    /// </summary>
    /// <param name="number">
    /// 新たな節号。予約領域の値を与えません。
    /// </param>
    protected abstract void CreateSection(long number);
    /// <summary>
    /// 指定した節号の節を削除します。
    /// </summary>
    /// <param name="number">
    /// 指定する節号。予約領域の値を与えません。
    /// </param>
    protected abstract void DeleteSection(long number);

    /// <summary>
    /// 使用されていない節号を求めます。
    /// </summary>
    /// <returns>
    /// 戻り値は予約領域の値を返すことはありません。
    /// </returns>
    protected abstract long FindVacantNumber();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        var entry = this[0];
        entry.Clear();
        entry.Next = PreviousSectionNumber;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }
    protected virtual ValueTask DisposeAsync(bool disposing)
    {
        this[0].Next = PreviousSectionNumber;

        return ValueTask.CompletedTask;
    }
}

//public abstract class Section
//{
//    long _nextSectionNumber;

//    /// <summary>
//    /// 節に実があるかを取得します。特に節の作成直後もしくは削除直前に<see cref="true"/>です。
//    /// </summary>
//    public abstract bool IsEmpty { get; }
//    /// <summary>
//    /// 節の体勢を取得、または設定します。
//    /// <para>
//    /// 体勢に合わない動作を行った場合、合った体勢に変更されるまで待機されるか、例外が投げられます。
//    /// </para>
//    /// <para>
//    /// <list type="table">
//    /// <listheader>
//    /// <term>値</term>
//    /// <description>説明</description>
//    /// </listheader>
//    /// <item>
//    /// <term><see cref="SectionMode.Idle"/></term>
//    /// <description>節はすぐに再開できる状態で待機します。但し休止状態から遷移した場合は相変わらず休止状態である場合があります。</description>
//    /// </item>
//    /// <item>
//    /// <term><see cref="SectionMode.Close"/></term>
//    /// <description>節は可能な限りの資料を解放し休止します。</description>
//    /// </item>
//    /// <item>
//    /// <term><see cref="SectionMode.Read"/></term>
//    /// <description>節から数據を読み取ることができます。</description>
//    /// </item>
//    /// <item>
//    /// <term><see cref="SectionMode.Write"/></term>
//    /// <description>節に数據を書き込むことができます。</description>
//    /// </item>
//    /// </list>
//    /// </para>
//    /// </summary>
//    public abstract SectionMode Mode { get; set; }
//    public virtual long NextSectionNumber 
//    {
//        get
//        {
//            if (_nextSectionNumber == 0) throw new InvalidOperationException("パラメータの値が設定されていません。");

//            return _nextSectionNumber;
//        }
//        set
//        {
//            if (value == 0) throw new ArgumentException("`0`は始節を表す番号であり、次の節号に設定できません。");

//            _nextSectionNumber = value;
//        } 
//    }

//    protected Section()
//    {

//    }

//    public abstract int Read(Span<byte> span);
//    public abstract Task<int> ReadAsync(Memory<byte> memory);
//    public abstract int Write(ReadOnlySpan<byte> span);
//    public abstract Task<int> WriteAsync(ReadOnlyMemory<byte> memory);

//    public abstract void Clear();
//}

public interface ISection
{
    /// <summary>
    /// 節に実があるかを取得します。特に節の作成直後もしくは削除直前に<see cref="true"/>です。
    /// </summary>
    bool IsEmpty { get; }
    /// <summary>
    /// 節の体勢を取得、または設定します。
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
    /// <term><see cref="SectionMode.Idle"/></term>
    /// <description>節はすぐに再開できる状態で待機します。但し休止状態から遷移した場合は相変わらず休止状態である場合があります。</description>
    /// </item>
    /// <item>
    /// <term><see cref="SectionMode.Close"/></term>
    /// <description>節は可能な限りの資料を解放し休止します。</description>
    /// </item>
    /// <item>
    /// <term><see cref="SectionMode.Read"/></term>
    /// <description>節から数據を読み取ることができます。</description>
    /// </item>
    /// <item>
    /// <term><see cref="SectionMode.Write"/></term>
    /// <description>節に数據を書き込むことができます。</description>
    /// </item>
    /// </list>
    /// </para>
    /// </summary>
    SectionMode Mode { get; set; }
    /// <summary>
    /// 次の節号を取得または設定します。
    /// <para>
    /// 初期値は<see cref="long.MaxValue"/>であり、これは次が末端であることを表します。
    /// </para>
    /// </summary>
    long Next { get; set; }
    ///// <summary>
    ///// 節の末尾に別の節を繋げます。
    ///// </summary>
    ///// <param name="sector">
    ///// 繋げる節。
    ///// </param>
    //void Lead(ISection sector);
    ///// <summary>
    ///// 節を永久に削除します。節のためにある資料はすべて削除されます。
    ///// <para>
    ///// 実体が<see cref="IDisposable.Dispose"/>を実装する場合は、<see cref="Delete"/>の呼び出しの後にそれが呼ばれますが、<see cref="IDisposable.Dispose"/>が実体の所持する参照の解放であって場合によって復元可能であるのに対し、<see cref="Delete"/>は節のための資料をすべて削除します。
    ///// </para>
    ///// </summary>
    //void Delete();
    int Read(Span<byte> span);
    Task<int> ReadAsync(Memory<byte> memory);
    int Write(ReadOnlySpan<byte> span);
    Task<int> WriteAsync(ReadOnlyMemory<byte> memory);
    void Clear();
}

public class EmptySection : ISection
{
    SectionMode _mode;
    long? _next;

    public bool IsEmpty => true;
    public SectionMode Mode
    {
        get => _mode;
        set
        {
            if (value is SectionMode.Read) throw new ArgumentException("空節を読取態勢にすることはできません。");
            _mode = value;
        }
    }
    public long Next
    {
        get => _next ?? throw new InvalidOperationException();
        set => _next = value;
    }

    public EmptySection(long? next = null) => _next = next;

    public int Read(Span<byte> span) => 0;
    public Task<int> ReadAsync(Memory<byte> memory) => Task.FromResult(0);

    public int Write(ReadOnlySpan<byte> span) => span.Length;
    public Task<int> WriteAsync(ReadOnlyMemory<byte> memory) => Task.FromResult(memory.Length);

    public void Clear() { }
}

public enum SectionMode
{
    /// <summary>
    /// 節はすぐに再開できる状態で待機します。但し休止状態から遷移した場合は相変わらず休止状態である場合があります。
    /// </summary>
    Idle,
    /// <summary>
    /// 節から数據を読み取ることができます。
    /// </summary>
    Read,
    /// <summary>
    /// 節に数據を書き込むことができます。
    /// </summary>
    Write,
    /// <summary>
    /// 節は可能な限りの資料を解放し休止します。
    /// </summary>
    Close
}

//public abstract class SectionScroll<Self> : IScroll where Self : ISection
//{
//    readonly SkipList<Self> _scts;
//    readonly Dictionary<ScrollPointer, ISection> _refs;
//    ISection _cSct;
//    bool _isDisposed;

//    protected SkipList<Self> Sections => _scts;
//    protected SkipList<Self>.SkipListNode? PreviouSectionNode => _cSct.PreviousNode;// ?? throw new Exception("不明な錯誤です。現在の節の以前に節が存在しませんでした。");
//    protected SkipList<Self>.SkipListNode NextSectionNode => _cSct;
//    protected ISection PreviousSection => _cSct;
//    protected Self NextSection => _cSct.Next;
//    public int SectionCount => _scts.Count();
//    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
//    public ScrollPointer Point
//    {
//        get
//        {
//            var nSct = CreateSector();
            

//            if (NextSection.Number - PreviousSection.Number < 1) Rearrange();
//            var num = Assets.Utils.AverageCeiling(PreviousSection.Number, NextSection.Number);
//            var sct = CreateSector(num);
//            var node = _scts.InsertBefore(_cSct, sct);

//            _cSct.Value.Mode = SectionMode.Idle;
//            node.Value.Mode = SectionMode.Write;

//            var ptr = ProducePointer(of: sct);
//            _refs.Add(ptr, node);
//            return ptr;
//        }
//        set
//        {
//            PreviousSection.Mode = SectionMode.Idle;
//            NextSection.Mode = SectionMode.Idle;

//            if (NextSection.IsEmpty) DeleteSector(NextSectionNode.Value);

//            if (!_refs.Remove(value, out var node)) throw new ArgumentException($"索引が不明です。`{nameof(ScrollPointer)}`の用法を確認してください。", nameof(value));
//            if (node.Belongs(to: _scts)) throw new ArgumentException("索引が無効です。", nameof(value));
//            _cSct = node;

//            DestroyPointer(value);

//            PreviousSection.Mode = SectionMode.Write;
//            NextSection.Mode = SectionMode.Read;
//        }
//    }
//    /// <summary>
//    /// 軸箋を指定して関連付けられた節を取得、または設定します。
//    /// </summary>
//    /// <param name="pointer"></param>
//    /// <returns></returns>
//    protected Self? this[ScrollPointer pointer]
//    {
//        get => _refs.TryGetValue(pointer, out var result) ? result.Value : default;
//        set
//        {
//            var node = value is null ? null : _scts.Find(value);
//            if (node is null) 
//            { 
//                _refs.Remove(pointer);
//                return;
//            }
//            if (!_refs.TryAdd(pointer, node)) _refs[pointer] = node;
//        }
//    }

//    public SectionScroll(Self primarySection)
//    {
//        _scts = new(COMPERER);
//        _refs = new();
//        _cSct = _scts.Insert(primarySection);
//        _eS = new() { Number = long.MinValue };
//    }
//    protected SectionScroll(SectionScroll<Self> original)
//    {
//        throw new NotImplementedException();
//    }

//    public bool IsValid(ScrollPointer pointer) => _refs.TryGetValue(pointer, out var node) && !node.Belongs(to: _scts);
//    public bool Is(ScrollPointer on) => throw new NotImplementedException();

//    public virtual async Task Insert<T>(Memory<T> memory) where T : unmanaged
//    {
//        if (memory is Memory<byte> memory_) 
//        {
//            int c = 0;
//            while (true)
//            {
//                var pSct = PreviousSection;
//                c += await pSct.WriteAsync(memory_[c..]);
//                if (c >= memory_.Length) break;

//                _ = _scts.InsertBefore(_cSct, CreateSector(pSct.Number + 1));

//                if (NextSection.Number - PreviousSection.Number < 2) Rearrange();
//            }
//        }

//        InsertSync(memory.Span);
//    }
//    public virtual void InsertSync<T>(Span<T> span) where T : unmanaged
//    {
//        var span_ = span.ToSpan<T, byte>();

//        int c = 0;
//        while (true)
//        {
//            var pSct = PreviousSection;
//            c += pSct.Write(span_[c..]);
//            if (c >= span_.Length) break;
//            _ = _scts.InsertBefore(_cSct, CreateSector(pSct.Number + 1));

//            if (NextSection.Number - PreviousSection.Number < 2) Rearrange();
//        }
//    }

//    public virtual async Task Remove<T>(Memory<T> memory) where T : unmanaged
//    {
//        if (memory is Memory<byte> memory_)
//        {
//            int c = 0;
//            while (true)
//            {
//                c += await _cSct.Value.ReadAsync(memory_[c..]);
//                if (c >= memory_.Length) break;
//                DeleteSector(_cSct.Value);
//                _cSct = _cSct.NextNode ?? throw new Exception("巻子の末尾に到達しました。これ以上搴取するものがありません。");
//            }
//        }

//        RemoveSync(memory.Span);
//    }
//    public virtual void RemoveSync<T>(Span<T> span) where T : unmanaged
//    {
//        var span_ = span.ToSpan<T, byte>();

//        int c = 0;
//        while (true)
//        {
//            c += _cSct.Value.Read(span_[c..]);
//            if (c >= span_.Length) break;
//            DeleteSector(_cSct.Value);
//            _cSct = _cSct.NextNode ?? throw new Exception("巻子の末尾に到達しました。これ以上搴取するものがありません。");
//        }
//    }

//    public abstract IScroll Copy();

//    public abstract Task Insert(in ScrollPointer pointer);
//    public abstract Task Remove(out ScrollPointer pointer);

//    public void Dispose()
//    {
//        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
//        Dispose(disposing: true);
//        GC.SuppressFinalize(this);
//    }
//    protected virtual void Dispose(bool disposing)
//    {
//        if (!_isDisposed)
//        {
//            if (disposing)
//            {
//                foreach (var sct in Sections)
//                {
//                    if (sct is IDisposable disposable) disposable.Dispose();
//                }
//            }
//            _isDisposed = true;
//        }
//    }
//    public async ValueTask DisposeAsync()
//    {
//        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
//        await DisposeAsync(disposing: true);
//        GC.SuppressFinalize(this);
//    }
//    protected virtual async ValueTask DisposeAsync(bool disposing)
//    {
//        if (!_isDisposed)
//        {
//            if (disposing)
//            {
//                foreach (var sct in Sections)
//                {
//                    if (sct is IAsyncDisposable asyncDisposable) await asyncDisposable.DisposeAsync();
//                    else if (sct is IDisposable disposable) disposable.Dispose();
//                }
//            }
//            _isDisposed = true;
//        }
//    }

//    protected abstract Self CreateSector();
//    protected abstract void DeleteSector(Self sector);

//    /// <summary>
//    /// 節の有効な軸箋を作成します。
//    /// </summary>
//    /// <param name="of">
//    /// 軸箋を作成する節。
//    /// </param>
//    /// <returns>
//    /// 作成した有効な軸箋。
//    /// </returns>
//    protected abstract ScrollPointer ProducePointer(Self of);
//    /// <summary>
//    /// 軸箋を破棄して無効化します。
//    /// </summary>
//    /// <param name="pointer">
//    /// 破棄して無効化する軸箋。
//    /// </param>
//    protected abstract void DestroyPointer(ScrollPointer pointer);
//}
