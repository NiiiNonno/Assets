using MI = System.Runtime.CompilerServices.MethodImplAttribute;
using MIO = System.Runtime.CompilerServices.MethodImplOptions;

namespace Nonno.Assets;

public readonly struct ShortIdentifier<T> : IEquatable<ShortIdentifier<T>>
{
    public const int SIZE = 4;

    readonly uint _i0;

    internal ShortIdentifier(uint i0)
    {
        _i0 = i0;
    }

    public override string ToString() => $"{_i0:X4}:{typeof(T)}";

    public static ShortIdentifier<T> GetNew()
    {
        uint i0;
        do i0 = unchecked((uint)Utils.GetRandomValue()); while (i0 == 0);
        return new ShortIdentifier<T>(i0);
    }

    public override bool Equals(object? obj) => obj is ShortIdentifier<T> identifier && Equals(identifier);
    public bool Equals(ShortIdentifier<T> other) => _i0 == other._i0;
    public override int GetHashCode() => unchecked((int)_i0);

    public static bool operator ==(ShortIdentifier<T> left, ShortIdentifier<T> right) => left.Equals(right);
    public static bool operator !=(ShortIdentifier<T> left, ShortIdentifier<T> right) => !(left == right);
    public static implicit operator LongIdentifier<T>(ShortIdentifier<T> identifier) => new(identifier._i0, identifier._i0);
    public static implicit operator UniqueIdentifier<T>(ShortIdentifier<T> identifier) => (LongIdentifier<T>)identifier;

    [MI(MIO.AggressiveInlining)]
    public static void Write(Span<byte> to, ShortIdentifier<T> shortIdentifier)
    {
        if (!BitConverter.TryWriteBytes(to, shortIdentifier._i0)) throw new Exception("バイト列への書込みに失敗しました。");
    }
    [MI(MIO.AggressiveInlining)]
    public static ShortIdentifier<T> Read(ReadOnlySpan<byte> from)
    {
        return new ShortIdentifier<T>(BitConverter.ToUInt32(from));
    }
}

public readonly struct LongIdentifier<T> : IEquatable<LongIdentifier<T>>
{
    public const int SIZE = 8;

    readonly uint _i0, _i1;

    internal LongIdentifier(uint i0, uint i1)
    {
        _i0 = i0;
        _i1 = i1;
    }

    public override string ToString() => $"{_i0:X4}-{_i1:X4}:{typeof(T)}";

    public static LongIdentifier<T> GetNew()
    {
        uint i0, i1;
        do i0 = unchecked((uint)Utils.GetRandomValue()); while (i0 == 0);
        do i1 = unchecked((uint)Utils.GetRandomValue()); while (i1 == 0);
        return new LongIdentifier<T>(i0, i1);
    }

    public override bool Equals(object? obj) => obj is LongIdentifier<T> identifier && Equals(identifier);
    public bool Equals(LongIdentifier<T> other) => _i0 == other._i0 && _i1 == other._i1;
    public override int GetHashCode() => unchecked((int)(_i0 ^ _i1));

    public static bool operator ==(LongIdentifier<T> left, LongIdentifier<T> right) => left.Equals(right);
    public static bool operator !=(LongIdentifier<T> left, LongIdentifier<T> right) => !(left == right);
    public static explicit operator ShortIdentifier<T>(LongIdentifier<T> identifier) => new(identifier._i0 ^ ~identifier._i1);
    public static implicit operator UniqueIdentifier<T>(LongIdentifier<T> identifier) => new(identifier._i0, identifier._i0, identifier._i1, identifier._i1);

    [MI(MIO.AggressiveInlining)]
    public static void Write(Span<byte> to, LongIdentifier<T> longIdentifier)
    {
        if (!BitConverter.TryWriteBytes(to[0..4], longIdentifier._i0)) throw new Exception("バイト列への書込みに失敗しました。");
        if (!BitConverter.TryWriteBytes(to[4..8], longIdentifier._i1)) throw new Exception("バイト列への書込みに失敗しました。");
    }
    [MI(MIO.AggressiveInlining)]
    public static LongIdentifier<T> Read(ReadOnlySpan<byte> from)
    {
        return new LongIdentifier<T>(BitConverter.ToUInt32(from[0..4]), BitConverter.ToUInt32(from[4..8]));
    }
}

public readonly struct UniqueIdentifier<T> : IEquatable<UniqueIdentifier<T>>
{
    public const int SIZE = 16;

    readonly uint _i0, _i1, _i2, _i3;

    internal UniqueIdentifier(uint i0, uint i1, uint i2, uint i3)
    {
        _i0 = i0;
        _i1 = i1;
        _i2 = i2;
        _i3 = i3;
    }

    public override string ToString() => $"{_i0:X4}-{_i1:X4}-{_i2:X4}-{_i3:X4}:{typeof(T)}";

    public static UniqueIdentifier<T> GetNew()
    {
        uint i0, i1, i2, i3;
        do i0 = unchecked((uint)Utils.GetRandomValue()); while (i0 == 0);
        do i1 = unchecked((uint)Utils.GetRandomValue()); while (i1 == 0);
        do i2 = unchecked((uint)Utils.GetRandomValue()); while (i2 == 0);
        do i3 = unchecked((uint)Utils.GetRandomValue()); while (i3 == 0);
        return new UniqueIdentifier<T>(i0, i1, i2, i3);
    }

    public override bool Equals(object? obj) => obj is UniqueIdentifier<T> identifier && Equals(identifier);
    public bool Equals(UniqueIdentifier<T> other) => _i0 == other._i0 && _i1 == other._i1 && _i2 == other._i2 && _i3 == other._i3;
    public override int GetHashCode() => unchecked((int)(_i0 ^ _i1 ^ _i2 ^ _i3));

    public static bool operator ==(UniqueIdentifier<T> left, UniqueIdentifier<T> right) => left.Equals(right);
    public static bool operator !=(UniqueIdentifier<T> left, UniqueIdentifier<T> right) => !(left == right);
    public static explicit operator ShortIdentifier<T>(UniqueIdentifier<T> identifier) => (ShortIdentifier<T>)(LongIdentifier<T>)identifier;
    public static explicit operator LongIdentifier<T>(UniqueIdentifier<T> identifier) => new(identifier._i0 ^ ~identifier._i1, identifier._i2 ^ ~identifier._i3);

    [MI(MIO.AggressiveInlining)]
    public static void Write(Span<byte> to, UniqueIdentifier<T> uniqueIdentifier)
    {
        if (!BitConverter.TryWriteBytes(to[0..4], uniqueIdentifier._i0)) throw new Exception("バイト列への書込みに失敗しました。");
        if (!BitConverter.TryWriteBytes(to[4..8], uniqueIdentifier._i1)) throw new Exception("バイト列への書込みに失敗しました。");
        if (!BitConverter.TryWriteBytes(to[8..12], uniqueIdentifier._i2)) throw new Exception("バイト列への書込みに失敗しました。");
        if (!BitConverter.TryWriteBytes(to[12..16], uniqueIdentifier._i3)) throw new Exception("バイト列への書込みに失敗しました。");
    }
    [MI(MIO.AggressiveInlining)]
    public static UniqueIdentifier<T> Read(ReadOnlySpan<byte> from)
    {
        return new UniqueIdentifier<T>(BitConverter.ToUInt32(from[0..4]), BitConverter.ToUInt32(from[4..8]), BitConverter.ToUInt32(from[8..12]), BitConverter.ToUInt32(from[12..16]));
    }
}

public readonly struct TypeIdentifier : IEquatable<TypeIdentifier>
{
    public static readonly TypeIdentifier EMPTY = default;

    readonly Guid _id;

    public Guid Identifier => _id;
    public bool IsValid => _id == Guid.Empty;

    private TypeIdentifier(Guid identifier)
    {
        _id = identifier;
    }

    public bool IsIdentifying(Type type) => !type.IsGenericType && Identifier == type.GUID;

    public Type GetIdentifiedType() => Utils.GetType(_id);

    public override bool Equals(object? obj) => obj is TypeIdentifier identifier && Equals(identifier);
    public bool Equals(TypeIdentifier other) => _id.Equals(other._id);
    public override int GetHashCode() => HashCode.Combine(_id);

    public static TypeIdentifier Get(Type from, bool throwIfInvalidTypeAssigned = true)
    {
        if (from.IsGenericType) if (throwIfInvalidTypeAssigned) throw new ArgumentException("指定された型が無効です。型織別子は泛型には使用できません。"); else return default;

        return new(from.GUID);
    }
    public static TypeIdentifier Get<T>(bool throwIfInvalidTypeAssigned = true) => Get(typeof(T), throwIfInvalidTypeAssigned);

    public static bool operator ==(TypeIdentifier left, TypeIdentifier right) => left.Equals(right);
    public static bool operator !=(TypeIdentifier left, TypeIdentifier right) => !(left == right);
}

public interface IRecognizable<T>
{
    bool Recognize(ShortIdentifier<T> record) => record == GetShortIdentifier();
    bool Recognize(LongIdentifier<T> record) => record == GetLongIdentifier();
    bool Recognize(UniqueIdentifier<T> record) => record == GetUniqueIdentifier();
    ShortIdentifier<T> GetShortIdentifier();
    LongIdentifier<T> GetLongIdentifier();
    UniqueIdentifier<T> GetUniqueIdentifier();
}
