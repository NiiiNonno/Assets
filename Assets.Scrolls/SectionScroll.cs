using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Nonno.Assets.Collections;
using static System.Collections.Specialized.BitVector32;

namespace Nonno.Assets.Scrolls;
/// <summary>
/// 節巻子を表します。
/// </summary>
/// <remarks>
/// 節番号は予約領域があります。
/// <list type="bullet">
/// <listheader>
/// <term>節番号</term><description>用途</description>
/// </listheader>
/// <item>
/// <term><c>0</c></term><description>最初の節で、書いたものが消える場合があります。</description>
/// </item>
/// <item>
/// <term><c>0xFFFF_FFFF_FFFF_FFFF</c></term><description>最終の節であり、書き込むことができません。</description>
/// </item>
/// </list>
/// </remarks>
/// <typeparam name="TSection">
/// 節を表す型。
/// </typeparam>
public abstract class SectionScroll<TSection> : IScroll where TSection : Section
{
    readonly HashSet<ulong> _floatings;
    readonly EmptySection _endSection;
    ulong _current;

    /// <summary>
    /// 後方の節番号を取得します。
    /// </summary>
    protected ulong PreviousSectionNumber => _current;
    /// <summary>
    /// 後方の節を取得します。
    /// </summary>
    protected TSection PreviousSection => this[PreviousSectionNumber];
    /// <summary>
    /// 前方の節番号を取得します。
    /// </summary>
    protected ulong NextSectionNumber => PreviousSection.NextNumber;
    /// <summary>
    /// 前方の節を取得します。
    /// </summary>
    protected Section NextSection => NextSectionNumber == Section.END_SECTION_NUMBER ? this[NextSectionNumber] : _endSection;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public ScrollPointer Point
    {
        get
        {
            InsertSection();

            _floatings.Add(_current);
            return new(unchecked((long)_current));
        }
        set
        {
            PreviousSection.Mode = SectionMode.Idle;
            NextSection.Mode = SectionMode.Idle;

            _current = unchecked((ulong)value.LongNumber);
            _floatings.Remove(_current);

            PreviousSection.Mode = SectionMode.Write;
            NextSection.Mode = SectionMode.Read;
        }
    }
    /// <summary>
    /// 節番号を以て節を取得します。
    /// </summary>
    /// <param name="number">
    /// 節番号。
    /// </param>
    /// <returns>
    /// 取得した節。
    /// </returns>
    protected abstract TSection this[ulong number] { get; }

    /// <summary>
    /// 後方の節番号を指定して節巻子を初期化します。
    /// </summary>
    /// <param name="nextSection"></param>
    public SectionScroll(ulong currentNumber)
    {
        _floatings = new();
        _endSection = new();
        _current = currentNumber;
    }
    public SectionScroll(SectionScroll<TSection> original)
    {
        _floatings= new();
        _endSection = new();
        _current = original._current;
    }

    public bool IsValid(ScrollPointer pointer) => _floatings.Contains(unchecked((ulong)pointer.LongNumber));

    public bool Is(ScrollPointer on)
    {
        var c = _current;

        retry:;
        if (c == unchecked((ulong)on.LongNumber)) return true;

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
        var newSN = FindVacantNumber(prevSN, nextSN);
        var newS = this[newSN];

        CreateSection(newSN);
        prevS.NextNumber = newSN;
        newS.NextNumber = nextSN;
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
        var nextSN = oldS.NextNumber;
        var nextS = this[nextSN];

        DeleteSection(oldSN);
        prevS.NextNumber = nextSN;
        prevS.Mode = SectionMode.Write;
        nextS.Mode = SectionMode.Read;
    }

    public void Insert(in ScrollPointer pointer)
    {
        _floatings.Remove(unchecked((ulong)pointer.LongNumber));
        this.Insert(uint64: unchecked((ulong)pointer.LongNumber));
    }
    public void Remove(out ScrollPointer pointer)
    {
        this.Remove(uint64: out var p_n);
        _floatings.Add(p_n);
        pointer = new(unchecked((long)p_n));
    }

    public async Task InsertAsync<T>(Memory<T> memory, CancellationToken token = default) where T : unmanaged
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

        Insert(memory.Span);
    }
    public void Insert<T>(Span<T> span) where T : unmanaged
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

    public async Task RemoveAsync<T>(Memory<T> memory, CancellationToken token = default) where T : unmanaged
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

        Remove(memory.Span);
    }
    public void Remove<T>(Span<T> span) where T : unmanaged
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
    /// 新たな節番号から節を作成します。
    /// </summary>
    /// <param name="number">
    /// 新たな節番号。予約領域の値を与えません。
    /// </param>
    protected abstract void CreateSection(ulong number);
    /// <summary>
    /// 指定した節番号の節を削除します。
    /// </summary>
    /// <param name="number">
    /// 指定する節番号。予約領域の値を与えません。
    /// </param>
    protected abstract void DeleteSection(ulong number);

    /// <summary>
    /// 使用されていない節番号を求めます。
    /// </summary>
    /// <param name="previousSectionNumber">
    /// 求める節番号が存在すべき範囲を、その節より前の節の番号から制限します。
    /// </param>
    /// <param name="nextSectionNumber">
    /// 求める節番号が存在すべき範囲を、その節より後の節の番号から制限します。
    /// </param>
    /// <returns>
    /// 戻り値は予約領域の値を返すことはありません。
    /// </returns>
    protected abstract ulong FindVacantNumber(ulong? previousSectionNumber = null, ulong? nextSectionNumber = null);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        var entry = this[Section.ENTRY_SECTION_NUMBER];
        entry.Clear();
        entry.NextNumber = PreviousSectionNumber;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }
    protected virtual ValueTask DisposeAsync(bool disposing)
    {
        var entry = this[Section.ENTRY_SECTION_NUMBER];
        entry.Clear();
        entry.NextNumber = PreviousSectionNumber;

        return ValueTask.CompletedTask;
    }
}

public abstract class Section
{
    public const ulong ENTRY_SECTION_NUMBER = 0;
    public const ulong END_SECTION_NUMBER = 0xFFFF_FFFF_FFFF_FFFF;

    ulong _nextNumber;

    /// <summary>
    /// 節に実があるかを取得します。特に節の作成直後もしくは削除直前に<see cref="true"/>です。
    /// </summary>
    public abstract bool IsEmpty { get; }
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
    public abstract SectionMode Mode { get; set; }
    public ulong NextNumber
    {
        get
        {
            if (_nextNumber == ENTRY_SECTION_NUMBER) throw new InvalidOperationException("パラメータの値が設定されていません。");

            return _nextNumber;
        }
        set
        {
            if (value == ENTRY_SECTION_NUMBER) throw new ArgumentException("`0`は始節を表す番号であり、次の節番号に設定できません。");

            _nextNumber = value;
        }
    }

    protected Section()
    {
        _nextNumber = END_SECTION_NUMBER;
    }

    public abstract int Read(Span<byte> span);
    public abstract Task<int> ReadAsync(Memory<byte> memory);
    public abstract int Write(ReadOnlySpan<byte> span);
    public abstract Task<int> WriteAsync(ReadOnlyMemory<byte> memory);

    public abstract void Clear();
}

public sealed class EmptySection : Section
{
    SectionMode _mode;

    public override bool IsEmpty => true;
    public override SectionMode Mode
    {
        get => _mode;
        set
        {
            if (value is SectionMode.Read) throw new ArgumentException("空節を読取態勢にすることはできません。");
            _mode = value;
        }
    }

    public EmptySection() { }
    public EmptySection(ulong nextNumber) => NextNumber = nextNumber;

    public override int Read(Span<byte> span) => 0;
    public override Task<int> ReadAsync(Memory<byte> memory) => Task.FromResult(0);

    public override int Write(ReadOnlySpan<byte> span) => span.Length;
    public override Task<int> WriteAsync(ReadOnlyMemory<byte> memory) => Task.FromResult(memory.Length);

    public override void Clear() { }
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
