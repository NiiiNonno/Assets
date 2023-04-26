using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nonno.Assets.Collections;

namespace Nonno.Assets.Scrolls;

public class DefinedTypeScroll : IScroll
{
    readonly IConverter<TypeName, Type> _typeDefinitionConverter;

    public IScroll BaseScroll { get; }
    /// <summary>
    /// ��斖���ɍċA�؂�t������ꍇ�́A�ċA�؂̐���萔���擾�A�܂��͏��������܂��B�t�����Ȃ��ꍇ��<c>null</c>�B
    /// </summary>
    public uint? MagicNumberForCyclicRecursiveCheck { get; init; }
    /// <summary>
    /// �����ɍڂ��Ă��Ȃ��^���������ꂽ���_�ŗ�O�𓊂��邩���w�肵�܂��B������ꍇ��<c>true</c>�A�Ώ�����<see cref="TypeIdentifier.EMPTY"/>�Ƃ��ď�������ꍇ��<c>false</c>�B
    /// </summary>
    public bool ThrowIfUnknownTypeIsFound { get; set; }
    public bool LeaveOpen { get; set; }
    public ScrollPointer Point { get => BaseScroll.Point; set => BaseScroll.Point = value; }

    public DefinedTypeScroll(IScroll baseScroll, IConverter<TypeName, Type> typeDefinitionConverter, bool leaveOpen = false)
    {
        _typeDefinitionConverter = typeDefinitionConverter;

        BaseScroll = baseScroll;
        LeaveOpen = leaveOpen;
    }

    public bool IsValid(ScrollPointer pointer) => BaseScroll.IsValid(pointer);
    public bool Is(ScrollPointer on) => BaseScroll.Is(on);
    public void Insert(in ScrollPointer pointer) => BaseScroll.Insert(pointer);
    public void Remove(out ScrollPointer pointer) => BaseScroll.Remove(out pointer);
    public Task InsertAsync<T>(Memory<T> memory, CancellationToken cancellationToken = default) where T : unmanaged => BaseScroll.InsertAsync(memory, cancellationToken);
    public void Insert<T>(Span<T> span) where T : unmanaged => BaseScroll.Insert(span);
    public Task RemoveAsync<T>(Memory<T> memory, CancellationToken cancellationToken = default) where T : unmanaged => BaseScroll.RemoveAsync(memory, cancellationToken);
    public void Remove<T>(Span<T> span) where T : unmanaged => BaseScroll.Remove(span);
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!LeaveOpen) BaseScroll.Dispose();
    }
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