using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nonno.Assets.Collections;
using Nonno.Assets.Scrolls;

namespace Nonno.Assets.Scrolls;
public class StreamScroll : SectorScroll<ISector>
{
    readonly Stack<BufferSector> _buffers;
    readonly Stream _mS;

    /// <summary>
    /// 作成されるバッファ長を取得、または設定します。
    /// <para>
    /// 設定したバッファ長は以降にバッファが生成される場合に適用され、既存のバッファは影響を受けません。バッファは繰り返し使用されるため、以前に作成された、現在の<see cref="BufferSize"/>と長さが異なるバッファが再び使用される可能性があることを留意してください。
    /// </para>
    /// </summary>
    public int BufferSize { get; set; }

    public StreamScroll(Stream mainStream) : base(new StreamSector(mainStream, long.MinValue))
    {
        _buffers = new();
        _mS = mainStream;

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

    public override Task Insert(in ScrollPointer pointer)
    {
        throw new NotImplementedException();
    }
    public override Task Remove(out ScrollPointer pointer)
    {
        throw new NotImplementedException();
    }

    protected override ISector CreateSector(long number)
    {
        if (_buffers.TryPop(out var r)) r.Clear(); 
        else r = new(BufferSize, number);
        
        return r;
    }
    protected override void DeleteSector(ISector sector)
    {
        switch (sector)
        {
        case BufferSector bS:
            {
                _buffers.Push(bS);

                return;
            }
        case StreamSector:
            {
                return;
            }
        }
    }

    protected override ScrollPointer MakePointer(ISector of)
    {
        throw new NotImplementedException();
    }
    protected override void DestroyPointer(ScrollPointer pointer)
    {
        throw new NotImplementedException();
    }
}

public class NetworkStreamScroll : StreamScroll
{
    readonly ITwoWayDictionary<Type, TypeIdentifier> _tD;

    /// <summary>
    /// 区画末尾に再帰証を付加する場合は、再帰証の成解定数を取得、または初期化します。付加しない場合は<c>null</c>。
    /// </summary>
    public uint? MagicNumberForCyclicRecursiveCheck { get; init; }
    /// <summary>
    /// 辞書に載っていない型が発見された時点で例外を投げるかを指定します。投げる場合は<c>true</c>、対処せず<see cref="TypeIdentifier.EMPTY"/>として処理する場合は<c>false</c>。
    /// </summary>
    public bool ThrowIfUnknownTypeIsFound { get; set; }

    public NetworkStreamScroll(Stream mainStream, ITwoWayDictionary<Type, TypeIdentifier> typeDictionary) : base(mainStream: mainStream)
    {
        _tD = typeDictionary;
    }

    public override Task Insert<T>(Memory<T> memory)
    {
        if (memory.Span.Is(out Span<TypeIdentifier> result))
        {
            Tasks tasks = default;
            foreach (var tI in result)
            {
                var type = _tD.Opposite[tI];
                tasks += this.Insert(uInt64: type.Value);
            }
            return tasks.WhenAll();
        }

        return base.Insert(memory: memory);
    }

    public readonly struct Type : IEquatable<Type>
    {
        readonly ulong _value;

        public ulong Value => _value;

        public Type(ulong value)
        {
            _value = value;
        }
        public Type(ASCIIString @string)
        {
            _value = BitConverter.ToUInt64(@string.AsSpan());
        }

        public static explicit operator Type(ASCIIString @string) => new(@string);
        public static implicit operator ASCIIString(Type type) => new((Span<byte>)BitConverter.GetBytes(type._value));

        public static bool operator ==(Type left, Type right) => left.Equals(right);
        public static bool operator !=(Type left, Type right) => !(left == right);

        public override bool Equals(object? obj) => obj is Type code && Equals(code);
        public bool Equals(Type other) => _value == other._value;
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