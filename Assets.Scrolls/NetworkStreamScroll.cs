using System;
using System.IO;
using Nonno.Assets.Collections;

namespace Nonno.Assets.Scrolls;

public class NetworkStreamScroll : StreamScroll
{
    readonly IConverter<TypeName, TypeIdentifier> _tD;

    /// <summary>
    /// 区画末尾に再帰証を付加する場合は、再帰証の成解定数を取得、または初期化します。付加しない場合は<c>null</c>。
    /// </summary>
    public uint? MagicNumberForCyclicRecursiveCheck { get; init; }
    /// <summary>
    /// 辞書に載っていない型が発見された時点で例外を投げるかを指定します。投げる場合は<c>true</c>、対処せず<see cref="TypeIdentifier.EMPTY"/>として処理する場合は<c>false</c>。
    /// </summary>
    public bool ThrowIfUnknownTypeIsFound { get; set; }

    public NetworkStreamScroll(Stream mainStream, IConverter<TypeName, TypeIdentifier> typeDictionary) : base(mainStream: mainStream)
    {
        _tD = typeDictionary;
    }

    // public override Task InsertAsync<T>(Memory<T> memory, CancellationToken token)
    // {
    //     if (memory.Span.Is(out Span<UniqueIdentifier<Type>> result))
    //     {
    //         Tasks tasks = default;
    //         foreach (var tI in result)
    //         {
    //             var type = _tD[tI];
    //             tasks += this.Insert(uInt64: type.Value);
    //         }
    //         return tasks.WhenAll();
    //     }

    //     return base.InsertAsync(memory: memory);
    // }

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