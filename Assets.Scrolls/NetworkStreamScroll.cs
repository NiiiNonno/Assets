using System;
using System.IO;
using Nonno.Assets.Collections;

namespace Nonno.Assets.Scrolls;

public class NetworkStreamScroll : StreamScroll
{
    readonly IConverter<TypeName, TypeIdentifier> _tD;

    /// <summary>
    /// ��斖���ɍċA�؂�t������ꍇ�́A�ċA�؂̐���萔���擾�A�܂��͏��������܂��B�t�����Ȃ��ꍇ��<c>null</c>�B
    /// </summary>
    public uint? MagicNumberForCyclicRecursiveCheck { get; init; }
    /// <summary>
    /// �����ɍڂ��Ă��Ȃ��^���������ꂽ���_�ŗ�O�𓊂��邩���w�肵�܂��B������ꍇ��<c>true</c>�A�Ώ�����<see cref="TypeIdentifier.EMPTY"/>�Ƃ��ď�������ꍇ��<c>false</c>�B
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