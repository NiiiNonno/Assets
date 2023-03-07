using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nonno.Assets.Collections;
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

    public override void Insert(in ScrollPointer pointer)
    {
        throw new NotImplementedException();
    }
    public override void Remove(out ScrollPointer pointer)
    {
        throw new NotImplementedException();
    }

    protected override void CreateSection(ulong number)
    {
        if (_buffers.TryPop(out var r)) r.Clear(); 
        else r = new(BufferSize);
        
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

public class NetworkStreamScroll : StreamScroll
{
    readonly IConverter<TypeName, UniqueIdentifier<Type>> _tD;

    /// <summary>
    /// 区画末尾に再帰証を付加する場合は、再帰証の成解定数を取得、または初期化します。付加しない場合は<c>null</c>。
    /// </summary>
    public uint? MagicNumberForCyclicRecursiveCheck { get; init; }
    /// <summary>
    /// 辞書に載っていない型が発見された時点で例外を投げるかを指定します。投げる場合は<c>true</c>、対処せず<see cref="TypeIdentifier.EMPTY"/>として処理する場合は<c>false</c>。
    /// </summary>
    public bool ThrowIfUnknownTypeIsFound { get; set; }

    public NetworkStreamScroll(Stream mainStream, IConverter<TypeName, UniqueIdentifier<Type>> typeDictionary) : base(mainStream: mainStream)
    {
        _tD = typeDictionary;
    }

    public override Task InsertAsync<T>(Memory<T> memory)
    {
        if (memory.Span.Is(out Span<UniqueIdentifier<Type>> result))
        {
            Tasks tasks = default;
            foreach (var tI in result)
            {
                var type = _tD[tI];
                tasks += this.Insert(uInt64: type.Value);
            }
            return tasks.WhenAll();
        }

        return base.InsertAsync(memory: memory);
    }

    public readonly struct TypeName : IEquatable<TypeName>
    {
        readonly ulong _value;

        public ulong Value => _value;

        public TypeName(ulong value)
        {
            _value = value;
        }
        public TypeName(ASCIIString @string)
        {
            _value = BitConverter.ToUInt64(@string.AsSpan());
        }

        public static explicit operator TypeName(ASCIIString @string) => new(@string);
        public static implicit operator ASCIIString(TypeName type) => new((Span<byte>)BitConverter.GetBytes(type._value));

        public static bool operator ==(TypeName left, TypeName right) => left.Equals(right);
        public static bool operator !=(TypeName left, TypeName right) => !(left == right);

        public override bool Equals(object? obj) => obj is TypeName code && Equals(code);
        public bool Equals(TypeName other) => _value == other._value;
        public override int GetHashCode() => HashCode.Combine(_value);
    }
}

public enum StreamIndexFormat
{
    AbsoluteUInt32,
    AbsoluteUInt64,
    AbsoluteUInt32ExtendableJustBehind,
    AbsoluteUInt32ExtendableBehind4,
    AbsoluteUInt32ExtendableBehind8,
    RelativeInt32,
    RelativeInt64,
    SortedRelativeUInt32,
    SortedRelativeUInt64,
}