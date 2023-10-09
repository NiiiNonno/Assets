using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nonno.Assets.Scrolls;
using IS = System.Runtime.InteropServices;

namespace Nonno.Assets.Scrolls;
public class StreamScroll : SectionScroll<Section>
{
    readonly Dictionary<ulong, BufferSection> _useds;
    readonly Stack<BufferSection> _buffers;
    readonly Stream _mS;
    readonly HashSet<ulong> _numbers;

    /// <summary>
    /// 作成されるバッファ長を取得、または設定します。
    /// <para>
    /// 設定したバッファ長は以降にバッファが生成される場合に適用され、既存のバッファは影響を受けません。バッファは繰り返し使用されるため、以前に作成された、現在の<see cref="BufferSize"/>と長さが異なるバッファが再び使用される可能性があることを留意してください。
    /// </para>
    /// </summary>
    public int BufferSize { get; set; }

    protected override Section this[ulong number] => _useds[number];

    public StreamScroll(Stream mainStream) : base(Section.ENTRY_SECTION_NUMBER)
    {
        _useds = new();
        _buffers = new();
        _mS = mainStream;
        _numbers = new();

        BufferSize = 1024;
    }
    protected StreamScroll(StreamScroll original) : base(original)
    {
        throw new NotImplementedException();
    }

    public void Flush()
    {
        throw new NotImplementedException();
    }

    public override IScroll Copy()
    {
        return new StreamScroll(this);
    }

    protected override void CreateSection(ulong number)
    {
        if (_buffers.TryPop(out var r)) r.Clear();
        else { r = new(BufferSize); r.Init(); }
        
        _useds.Add(number, r);
    }

    protected override void DeleteSection(ulong number)
    {
        switch (this[number])
        {
        case BufferSection bS:
            {
                _buffers.Push(bS);

                return;
            }
        case StreamSection:
            {
                return;
            }
        }
    }

    protected override ulong FindVacantNumber(ulong? previousSectionNumber, ulong? nextSectionNumber)
    {
        retry:;
        var r = unchecked((ulong)Random.Shared.NextInt64());
        if (_useds.ContainsKey(r)) goto retry; 

        return r;
    }

    protected override void Dispose(bool disposing)
    {
        foreach (var buf in _useds.Values)
        {
            buf.Dispose();
        }

        base.Dispose(disposing);
    }
    protected override ValueTask DisposeAsync(bool disposing)
    {
        foreach (var buf in _useds.Values)
        {
            buf.Dispose();
        }

        return base.DisposeAsync(disposing);
    }
}
