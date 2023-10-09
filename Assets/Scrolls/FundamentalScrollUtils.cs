//#define USE_BYTE_SPAN
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using static System.BitConverter;
using BS = System.Span<byte>;
using MI = System.Runtime.CompilerServices.MethodImplAttribute;
using MIO = System.Runtime.CompilerServices.MethodImplOptions;

namespace Nonno.Assets.Scrolls;
#pragma warning disable IDE0001 // 意図せずInsert(value:)の参照先が変わらぬようつける。
public static class FundamentalScrollUtils
{
    static readonly Assembly ASSEMBLY = Assembly.GetExecutingAssembly();
    static readonly AssemblyName ASSEMBLY_NAME = ASSEMBLY.GetName();

    static FundamentalScrollUtils()
    {
        InitSectionForAssemblyOrdering();
    }

    #region Primitives

/*
 foreach(var (type, name) in new[]{ 
    ("byte", "@byte"),
    ("sbyte", "@sbyte"),
    ("ushort", "uint16"),
    ("short", "int16"),
    ("uint", "uint32"),
    ("int", "int32"),
    ("ulong", "uint64"),
    ("long", "int64"),
    ("DateTime", "dateTime"),
    ("TimeSpan", "timeSpan"),
    ("nint", "intPtr"),
    ("nuint", "uintPtr"),
    ("Half", "half"),
    ("float", "@float"),
    ("double", "@double"),
    ("decimal", "@decimal"),
    ("char", "@char"),
    ("bool", "boolean"),
})
Console.WriteLine(
$$"""
    [IRMethod] public static void Insert<TScroll>(this TScroll @this, {{type}} {{name}}) where TScroll : IScroll => @this.Insert(value: {{name}});
    [IRMethod] public static void Insert<TScroll>(this TScroll @this, {{type}}? {{name}}OrNull) where TScroll : IScroll { @this.Insert(boolean: {{name}}OrNull.HasValue); if ({{name}}OrNull.HasValue) @this.Insert({{name}}: {{name}}OrNull.Value); }
    [IRMethod] public static void Remove<TScroll>(this TScroll @this, out {{type}} {{name}}) where TScroll : IScroll => @this.Remove(value: out {{name}});
    [IRMethod] public static void Remove<TScroll>(this TScroll @this, out {{type}}? {{name}}OrNull) where TScroll : IScroll { @this.Remove(boolean: out bool f); if (f) { @this.Remove({{name}}: out {{type}} value); {{name}}OrNull = value; } else { {{name}}OrNull = null; } }
"""
);
 */

    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, byte @byte) where TScroll : IScroll => @this.Insert(value: @byte);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, byte? @byteOrNull) where TScroll : IScroll { @this.Insert(boolean: @byteOrNull.HasValue); if (@byteOrNull.HasValue) @this.Insert(@byte: @byteOrNull.Value); }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out byte @byte) where TScroll : IScroll => @this.Remove(value: out @byte);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out byte? @byteOrNull) where TScroll : IScroll { @this.Remove(boolean: out bool f); if (f) { @this.Remove(@byte: out byte value); @byteOrNull = value; } else { @byteOrNull = null; } }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, sbyte @sbyte) where TScroll : IScroll => @this.Insert(value: @sbyte);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, sbyte? @sbyteOrNull) where TScroll : IScroll { @this.Insert(boolean: @sbyteOrNull.HasValue); if (@sbyteOrNull.HasValue) @this.Insert(@sbyte: @sbyteOrNull.Value); }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out sbyte @sbyte) where TScroll : IScroll => @this.Remove(value: out @sbyte);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out sbyte? @sbyteOrNull) where TScroll : IScroll { @this.Remove(boolean: out bool f); if (f) { @this.Remove(@sbyte: out sbyte value); @sbyteOrNull = value; } else { @sbyteOrNull = null; } }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, ushort uint16) where TScroll : IScroll => @this.Insert(value: uint16);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, ushort? uint16OrNull) where TScroll : IScroll { @this.Insert(boolean: uint16OrNull.HasValue); if (uint16OrNull.HasValue) @this.Insert(uint16: uint16OrNull.Value); }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out ushort uint16) where TScroll : IScroll => @this.Remove(value: out uint16);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out ushort? uint16OrNull) where TScroll : IScroll { @this.Remove(boolean: out bool f); if (f) { @this.Remove(uint16: out ushort value); uint16OrNull = value; } else { uint16OrNull = null; } }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, short int16) where TScroll : IScroll => @this.Insert(value: int16);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, short? int16OrNull) where TScroll : IScroll { @this.Insert(boolean: int16OrNull.HasValue); if (int16OrNull.HasValue) @this.Insert(int16: int16OrNull.Value); }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out short int16) where TScroll : IScroll => @this.Remove(value: out int16);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out short? int16OrNull) where TScroll : IScroll { @this.Remove(boolean: out bool f); if (f) { @this.Remove(int16: out short value); int16OrNull = value; } else { int16OrNull = null; } }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, uint uint32) where TScroll : IScroll => @this.Insert(value: uint32);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, uint? uint32OrNull) where TScroll : IScroll { @this.Insert(boolean: uint32OrNull.HasValue); if (uint32OrNull.HasValue) @this.Insert(uint32: uint32OrNull.Value); }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out uint uint32) where TScroll : IScroll => @this.Remove(value: out uint32);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out uint? uint32OrNull) where TScroll : IScroll { @this.Remove(boolean: out bool f); if (f) { @this.Remove(uint32: out uint value); uint32OrNull = value; } else { uint32OrNull = null; } }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, int int32) where TScroll : IScroll => @this.Insert(value: int32);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, int? int32OrNull) where TScroll : IScroll { @this.Insert(boolean: int32OrNull.HasValue); if (int32OrNull.HasValue) @this.Insert(int32: int32OrNull.Value); }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out int int32) where TScroll : IScroll => @this.Remove(value: out int32);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out int? int32OrNull) where TScroll : IScroll { @this.Remove(boolean: out bool f); if (f) { @this.Remove(int32: out int value); int32OrNull = value; } else { int32OrNull = null; } }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, ulong uint64) where TScroll : IScroll => @this.Insert(value: uint64);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, ulong? uint64OrNull) where TScroll : IScroll { @this.Insert(boolean: uint64OrNull.HasValue); if (uint64OrNull.HasValue) @this.Insert(uint64: uint64OrNull.Value); }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out ulong uint64) where TScroll : IScroll => @this.Remove(value: out uint64);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out ulong? uint64OrNull) where TScroll : IScroll { @this.Remove(boolean: out bool f); if (f) { @this.Remove(uint64: out ulong value); uint64OrNull = value; } else { uint64OrNull = null; } }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, long int64) where TScroll : IScroll => @this.Insert(value: int64);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, long? int64OrNull) where TScroll : IScroll { @this.Insert(boolean: int64OrNull.HasValue); if (int64OrNull.HasValue) @this.Insert(int64: int64OrNull.Value); }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out long int64) where TScroll : IScroll => @this.Remove(value: out int64);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out long? int64OrNull) where TScroll : IScroll { @this.Remove(boolean: out bool f); if (f) { @this.Remove(int64: out long value); int64OrNull = value; } else { int64OrNull = null; } }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, Half half) where TScroll : IScroll => @this.Insert(value: half);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, Half? halfOrNull) where TScroll : IScroll { @this.Insert(boolean: halfOrNull.HasValue); if (halfOrNull.HasValue) @this.Insert(half: halfOrNull.Value); }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out Half half) where TScroll : IScroll => @this.Remove(value: out half);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out Half? halfOrNull) where TScroll : IScroll { @this.Remove(boolean: out bool f); if (f) { @this.Remove(half: out Half value); halfOrNull = value; } else { halfOrNull = null; } }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, float single) where TScroll : IScroll => @this.Insert(value: single);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, float? singleOrNull) where TScroll : IScroll { @this.Insert(boolean: singleOrNull.HasValue); if (singleOrNull.HasValue) @this.Insert(single: singleOrNull.Value); }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out float single) where TScroll : IScroll => @this.Remove(value: out single);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out float? singleOrNull) where TScroll : IScroll { @this.Remove(boolean: out bool f); if (f) { @this.Remove(single: out float value); singleOrNull = value; } else { singleOrNull = null; } }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, double @double) where TScroll : IScroll => @this.Insert(value: @double);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, double? @doubleOrNull) where TScroll : IScroll { @this.Insert(boolean: @doubleOrNull.HasValue); if (@doubleOrNull.HasValue) @this.Insert(@double: @doubleOrNull.Value); }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out double @double) where TScroll : IScroll => @this.Remove(value: out @double);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out double? @doubleOrNull) where TScroll : IScroll { @this.Remove(boolean: out bool f); if (f) { @this.Remove(@double: out double value); @doubleOrNull = value; } else { @doubleOrNull = null; } }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, decimal @decimal) where TScroll : IScroll => @this.Insert(value: @decimal);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, decimal? @decimalOrNull) where TScroll : IScroll { @this.Insert(boolean: @decimalOrNull.HasValue); if (@decimalOrNull.HasValue) @this.Insert(@decimal: @decimalOrNull.Value); }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out decimal @decimal) where TScroll : IScroll => @this.Remove(value: out @decimal);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out decimal? @decimalOrNull) where TScroll : IScroll { @this.Remove(boolean: out bool f); if (f) { @this.Remove(@decimal: out decimal value); @decimalOrNull = value; } else { @decimalOrNull = null; } }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, char @char) where TScroll : IScroll => @this.Insert(value: @char);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, char? @charOrNull) where TScroll : IScroll { @this.Insert(boolean: @charOrNull.HasValue); if (@charOrNull.HasValue) @this.Insert(@char: @charOrNull.Value); }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out char @char) where TScroll : IScroll => @this.Remove(value: out @char);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out char? @charOrNull) where TScroll : IScroll { @this.Remove(boolean: out bool f); if (f) { @this.Remove(@char: out char value); @charOrNull = value; } else { @charOrNull = null; } }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, bool boolean) where TScroll : IScroll => @this.Insert(value: boolean);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, bool? booleanOrNull) where TScroll : IScroll { @this.Insert(boolean: booleanOrNull.HasValue); if (booleanOrNull.HasValue) @this.Insert(boolean: booleanOrNull.Value); }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out bool boolean) where TScroll : IScroll => @this.Remove(value: out boolean);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out bool? booleanOrNull) where TScroll : IScroll { @this.Remove(boolean: out bool f); if (f) { @this.Remove(boolean: out bool value); booleanOrNull = value; } else { booleanOrNull = null; } }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, nint intPtr) where TScroll : IScroll => @this.Insert(value: intPtr);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, nint? intPtrOrNull) where TScroll : IScroll { @this.Insert(boolean: intPtrOrNull.HasValue); if (intPtrOrNull.HasValue) @this.Insert(intPtr: intPtrOrNull.Value); }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out nint intPtr) where TScroll : IScroll => @this.Remove(value: out intPtr);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out nint? intPtrOrNull) where TScroll : IScroll { @this.Remove(boolean: out bool f); if (f) { @this.Remove(intPtr: out nint value); intPtrOrNull = value; } else { intPtrOrNull = null; } }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, nuint uintPtr) where TScroll : IScroll => @this.Insert(value: uintPtr);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, nuint? uintPtrOrNull) where TScroll : IScroll { @this.Insert(boolean: uintPtrOrNull.HasValue); if (uintPtrOrNull.HasValue) @this.Insert(uintPtr: uintPtrOrNull.Value); }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out nuint uintPtr) where TScroll : IScroll => @this.Remove(value: out uintPtr);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out nuint? uintPtrOrNull) where TScroll : IScroll { @this.Remove(boolean: out bool f); if (f) { @this.Remove(uintPtr: out nuint value); uintPtrOrNull = value; } else { uintPtrOrNull = null; } }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, DateTime dateTime) where TScroll : IScroll => @this.Insert(value: dateTime);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, DateTime? dateTimeOrNull) where TScroll : IScroll { @this.Insert(boolean: dateTimeOrNull.HasValue); if (dateTimeOrNull.HasValue) @this.Insert(dateTime: dateTimeOrNull.Value); }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out DateTime dateTime) where TScroll : IScroll => @this.Remove(value: out dateTime);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out DateTime? dateTimeOrNull) where TScroll : IScroll { @this.Remove(boolean: out bool f); if (f) { @this.Remove(dateTime: out DateTime value); dateTimeOrNull = value; } else { dateTimeOrNull = null; } }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, TimeSpan timeSpan) where TScroll : IScroll => @this.Insert(value: timeSpan);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, TimeSpan? timeSpanOrNull) where TScroll : IScroll { @this.Insert(boolean: timeSpanOrNull.HasValue); if (timeSpanOrNull.HasValue) @this.Insert(timeSpan: timeSpanOrNull.Value); }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out TimeSpan timeSpan) where TScroll : IScroll => @this.Remove(value: out timeSpan);
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out TimeSpan? timeSpanOrNull) where TScroll : IScroll { @this.Remove(boolean: out bool f); if (f) { @this.Remove(timeSpan: out TimeSpan value); timeSpanOrNull = value; } else { timeSpanOrNull = null; } }

    #endregion
    #region BuiltIns

    static readonly Dictionary<Type, IRDelegate> _delegates = new();

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void Insert<TScroll>(this TScroll @this, string? @string) where TScroll : IScroll
    {
        if (@string is null) { @this.Insert(int32: -1); return; }
        @this.Insert(int32: @string.Length); // lengthは文字長(バイト長ではない)であることに注意。
        @this.Insert(span: @string.UnsafeAsByteSpan());
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void Remove<TScroll>(this TScroll @this, out string? @string) where TScroll : IScroll
    {
        @this.Remove(int32: out int length); // lengthは文字長(バイト長ではない)であることに注意。
        if (length < 0) { @string = null; return; }
        @string = new(default, length);
        @this.Remove(span: @string.UnsafeAsByteSpan());
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void Insert<TScroll>(this TScroll @this, ReadOnlySpan<byte> u8string) where TScroll : IScroll
    {
        BS span = u8string.Length > ConstantValues.STACKALLOC_MAX_LENGTH ? new byte[u8string.Length] : stackalloc byte[u8string.Length];
        @this.Insert(int32: u8string.Length);
        @this.Insert(span: span);
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void Remove<TScroll>(this TScroll @this, out ReadOnlySpan<byte> u8string) where TScroll : IScroll
    {
        @this.Remove(int32: out var length);
        var r = new byte[length];
        @this.Remove<byte>(span: r);
        u8string = r;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void Insert<TScroll, T>(this TScroll @this, T[]? array) where TScroll : IScroll
    {
        if (array == null)
        {
            @this.Insert(int32: -1);
        }
        else
        {
            @this.Insert(int32: array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                @this.Insert(instance: array[i]);
            }
        }
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void Remove<TScroll, T>(this TScroll @this, T[]? array) where TScroll : IScroll
    {
        @this.Remove(out int length);
        if (length < 0)
        {
            if (array is not null) throw new ArgumentException("渡された配列が適しません。搴取された値が`null`であったのに対し、渡された値は`null`ではありませんでした。", nameof(array));
        }
        else
        {
            if (array is null) throw new ArgumentNullException("渡された配列が適しません。搴取された値が`null`でではなかったのに対し、渡された値は`null`でした。", nameof(array));
            if (array.Length != length) throw new ArgumentException($"渡された配列が適しません。搴取された値の長さが`{length}`であったのに対し、渡された値の長さは`{array.Length}`でした。", nameof(array));

            for (int i = 0; i < array.Length; i++)
            {
                @this.Remove(instance: out T item);
                array[i] = item;
            }
        }
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void Remove<T>(this IScroll @this, out T[]? array)
    {
        @this.Remove(out int length);
        if (length < 0)
        {
            array = null;
        }
        else
        {
            array = new T[length];

            for (int i = 0; i < array.Length; i++)
            {
                @this.Remove(instance: out T item);
                array[i] = item;
            }
        }
    }

    [IRMethod]
    public static void Insert<TScroll>(this TScroll @this, Type? type) where TScroll : IScroll
    {
        var etor = TypeIdentifier.Of(type);
        while (etor.MoveNext())
        {
            @this.Insert(guid: etor.Current.Identifier);
        }
    }
    [IRMethod]
    public static void Remove<TScroll>(this TScroll @this, out Type? type) where TScroll : IScroll
    {
        type = TypeIdentifier.GetType(GetEnumerator());

        IEnumerator<TypeIdentifier> GetEnumerator()
        {
            while (true)
            {
                @this.Remove(guid: out var r);
                yield return new(r);
            }
        }
    }

    #endregion
    #region Object

    [MI(MIO.AggressiveInlining)]
    public static void Insert(this IScroll @this, in object? @object)
    {
        if (@object is null)
        {
            @this.Insert(type: null);
        }
        else
        {
            var type = @object.GetType();
            @this.Insert(type: type);

            if (!_delegates.TryGetValue(type, out var @delegate))
            {
                @delegate = new(type);
                _delegates.Add(type, @delegate);
            }
            @delegate.Insert(@this, @object);
        }
    }
    [MI(MIO.AggressiveInlining)]
    public static void Remove(this IScroll @this, out object? @object)
    {
        @this.Remove(type: out var type);
        if (type is null)
        {
            @object = null;
        }
        else
        {
            if (!_delegates.TryGetValue(type, out var @delegate))
            {
                @delegate = new(type);
                _delegates.Add(type, @delegate);
            }
            @delegate.Remove(@this, out @object);
        }
    }

    #endregion
    #region Others

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void Insert<TScroll>(this TScroll @this, Shift shift) where TScroll : IScroll
    {
        @this.Insert(int32: shift.exponent);
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void Remove<TScroll>(this TScroll @this, out Shift shift) where TScroll : IScroll
    {
        @this.Remove(out int exponent);
        shift = new(exponent);
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void Insert<TScroll>(this TScroll @this, Guid guid) where TScroll : IScroll
    {
        BS span = stackalloc byte[16];
        _ = guid.TryWriteBytes(span);

        @this.Insert(uint32: ToUInt32(span/*[0..]*/));
        @this.Insert(uint16: ToUInt16(span[4..]));
        @this.Insert(uint16: ToUInt16(span[6..]));
        @this.Insert(@byte: span[8]);
        @this.Insert(@byte: span[9]);
        @this.Insert(@byte: span[10]);
        @this.Insert(@byte: span[11]);
        @this.Insert(@byte: span[12]);
        @this.Insert(@byte: span[13]);
        @this.Insert(@byte: span[14]);
        @this.Insert(@byte: span[15]);
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void Remove<TScroll>(this TScroll @this, out Guid guid) where TScroll : IScroll
    {
        @this.Remove(out uint a);
        @this.Remove(out ushort b);
        @this.Remove(out ushort c);
        @this.Remove(out byte d);
        @this.Remove(out byte e);
        @this.Remove(out byte f);
        @this.Remove(out byte g);
        @this.Remove(out byte h);
        @this.Remove(out byte i);
        @this.Remove(out byte j);
        @this.Remove(out byte k);

        guid = new(a, b, c, d, e, f, g, h, i, j, k);
    }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Insert<TScroll>(this TScroll @this, Guid? guidOrNull) where TScroll : IScroll { @this.Insert(boolean: guidOrNull.HasValue); if (guidOrNull.HasValue) @this.Insert(guid: guidOrNull.Value); }
    [IRMethod, MI(MIO.AggressiveInlining)] public static void Remove<TScroll>(this TScroll @this, out Guid? guidOrNull) where TScroll : IScroll { @this.Remove(boolean: out bool f); if (f) { @this.Remove(guid: out Guid value); guidOrNull = value; } else { guidOrNull = null; } }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void Insert<TScroll>(this TScroll @this, in BigInteger integer) where TScroll : IScroll
    {
        int length = integer.GetByteCount();
        BS span = stackalloc byte[length];
        if (!integer.TryWriteBytes(span, out int bytesWritten) || length != bytesWritten) throw new Exception("不明なエラーです。");
        @this.Insert(span: span);
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void Remove<TScroll>(this TScroll @this, out BigInteger integer) where TScroll : IScroll
    {
        @this.Remove(out int length);
        BS span = stackalloc byte[length];
        @this.Remove(span: span);
        integer = new BigInteger(span);
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void Insert<TScroll, T>(this TScroll @this, in ShortIdentifier<T> shortIdentifier) where TScroll : IScroll
    {
        BS span = stackalloc byte[ShortIdentifier<T>.SIZE];
        ShortIdentifier<T>.Write(to: span, shortIdentifier);
        @this.Insert(span: span);
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void Remove<TScroll, T>(this TScroll @this, out ShortIdentifier<T> shortIdentifier) where TScroll : IScroll
    {
        BS span = stackalloc byte[ShortIdentifier<T>.SIZE];
        @this.Remove(span: span);
        shortIdentifier = ShortIdentifier<T>.Read(from: span);
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void Insert<TScroll, T>(this TScroll @this, in LongIdentifier<T> longIdentifier) where TScroll : IScroll
    {
        BS span = stackalloc byte[LongIdentifier<T>.SIZE];
        LongIdentifier<T>.Write(to: span, longIdentifier);
        @this.Insert(span: span);
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void Remove<TScroll, T>(this TScroll @this, out LongIdentifier<T> longIdentifier) where TScroll : IScroll
    {
        BS span = stackalloc byte[LongIdentifier<T>.SIZE];
        @this.Remove(span: span);
        longIdentifier = LongIdentifier<T>.Read(from: span);
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void Insert<TScroll, T>(this TScroll @this, in UniqueIdentifier<T> uniqueIdentifier) where TScroll : IScroll
    {
        BS span = stackalloc byte[UniqueIdentifier<T>.SIZE];
        UniqueIdentifier<T>.Write(to: span, uniqueIdentifier);
        @this.Insert(span: span);
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void Remove<TScroll, T>(this TScroll @this, out UniqueIdentifier<T> uniqueIdentifier) where TScroll : IScroll
    {
        BS span = stackalloc byte[UniqueIdentifier<T>.SIZE];
        @this.Remove(span: span);
        uniqueIdentifier = UniqueIdentifier<T>.Read(from: span);
    }

    #endregion
    #region Generic

    /// <summary>
    /// 未知の型を巻子に挿入します。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="instance"></param>
    /// <returns>挿入を保証する用務。</returns>
    [MI(MIO.AggressiveInlining)]
    [Mark("generic_insert_method")]
    public static void Insert<TScroll, T>(this TScroll @this, in T instance) where TScroll : IScroll => IRDelegate<TScroll, T>.Default.Insert(@this, in instance);
    /// <summary>
    /// 未知の型を巻子に挿入します。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="instance"></param>
    /// <returns>挿入を保証する用務。</returns>
    [MI(MIO.AggressiveInlining)]
    public static void Insert<TScroll, T>(this TScroll @this, T instance) where TScroll : IScroll => IRDelegate<TScroll, T>.Default.Insert(@this, instance);
    /// <summary>
    /// 未知の型を巻子から搴取します。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="instance"></param>
    /// <returns>挿入を保証する用務。</returns>
    [MI(MIO.AggressiveInlining)]
    [Mark("generic_remove_method")]
    public static void Remove<TScroll, T>(this TScroll @this, out T instance) where TScroll : IScroll => IRDelegate<TScroll, T>.Default.Remove(@this, out instance);
    /// <summary>
    /// 未知の型を巻子から搴取します。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="instance"></param>
    /// <returns>挿入を保証する用務。</returns>
    [MI(MIO.AggressiveInlining)]
    public static void Remove<TScroll, T>(this TScroll @this, T instance) where TScroll : IScroll => IRDelegate<TScroll, T>.Default.Remove(@this, instance);

    #endregion
    #region AssemblyOrdering

    static Dictionary<Type, List<MethodInfo>> _cands = null!;
    static IEnumerable<Assembly> _assemblies = null!;
    static bool _allowAutoAddition;

    /// <summary>
    /// 新たな繹典の輸入時に自動的に挿搴務容を追加するを許可否を取得、または設定します。
    /// </summary>
    public static bool AllowAutoAddition
    {
        get => _allowAutoAddition;
        set
        {
            if (value == _allowAutoAddition) return;

            _cands.Clear();

            if (value) AppDomain.CurrentDomain.AssemblyLoad += Load;
            else AppDomain.CurrentDomain.AssemblyLoad -= Load;

            _allowAutoAddition = value;
        }
    }
    public static IEnumerable<Assembly> Assemblies
    {
        get => _assemblies;
        set
        {
            foreach (var asm in value) Add(asm); 

            _assemblies = value;
        }
    }
    internal static Dictionary<Type, List<MethodInfo>> Candidates => _cands;

    static void InitSectionForAssemblyOrdering()
    {
        _assemblies = null!;
        _cands = new();

        Assemblies = AppDomain.CurrentDomain.GetAssemblies();
        AllowAutoAddition = true;
    }

    static void Load(object? sender, AssemblyLoadEventArgs e) 
    {
        Assemblies = Assemblies.Append(e.LoadedAssembly);
        Add(e.LoadedAssembly); 
    }

    static void Add(Assembly assembly)
    {
        foreach(var type in assembly.GetExportedTypes())
        {
            if (!type.IsClass || type.IsGenericType) continue;

            foreach (var mI in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (mI.GetCustomAttribute<IRMethodAttribute>() is null) continue;

                var pIs = mI.GetParameters();
                if (pIs.Length != 2) continue;
                
                var pT_a = pIs[1].ParameterType;
                if (pT_a.IsByRef) pT_a = pT_a.GetElementType();
                if (pT_a is null) ThrowHelper.FailToGetReflections();

                var pT_ad = pT_a.IsGenericType ? pT_a.GetGenericTypeDefinition() : pT_a;
                if (!_cands.TryGetValue(pT_ad, out var list))
                {
                    list = new();
                    _cands.Add(pT_ad, list);
                }
                list.Add(mI);
            }
        }
    }

    #endregion
}
#pragma warning restore IDE0001 // 意図せずInsert(value:)の参照先が変わらぬようつける。

public struct IRMethodInformations
{
    MethodInfo? _insert, _insert_ref, _remove, _remove_ref;

    public MethodInfo? Insert => _insert;
    public MethodInfo? InsertRef => _insert_ref;
    public MethodInfo? Remove => _remove;
    public MethodInfo? RemoveRef => _remove_ref;

    public bool Init(Type instanceType)
    {
        bool r = false;
        var key = instanceType.IsGenericType ? instanceType.GetGenericTypeDefinition() : instanceType;
        if (FundamentalScrollUtils.Candidates.TryGetValue(key, out var cands))
        {
            /*
             * IR<TK, TV>(Dict<TK, TV>) <- IR<str, int>(StrDict<int>)
             */
            foreach (var cand in cands)
            {
                MethodInfo mI;
                bool isByRef;

                var rT = cand.ReturnType;
                if (rT != typeof(void)) continue;

                var pIs = cand.GetParameters();
                if (pIs.Length != 2) continue;

                if (cand.IsGenericMethod)
                {
                    var gTAs = cand.GetGenericArguments();
                    var pT_s = pIs[0].ParameterType;
#pragma warning disable CS0642 // empty ステートメントが間違っている可能性があります
                    if (pT_s == typeof(IScroll)) ;
#pragma warning restore CS0642 // empty ステートメントが間違っている可能性があります
                    else if (pT_s.IsGenericTypeParameter) gTAs.Replace(pT_s, typeof(IScroll));
                    else continue;

                    var pT_a = pIs[1].ParameterType;
                    isByRef = pT_a.IsByRef;
                    if (isByRef) pT_a = pT_a.GetElementType()!;

                    var type_a = instanceType;
                    var pT_ad = pT_a.GetGenericTypeDefinition();
                    do
                    {
                        if (type_a.GetGenericTypeDefinition() == pT_ad) goto next;
                        type_a = type_a.BaseType;
                    }
                    while (type_a != null);
                    continue;

                next:;
                    if (pT_a.IsGenericType)
                    {
                        var gTAs_pa = pT_a.GetGenericArguments();
                        var gTAs_ta = type_a.GetGenericArguments();

                        for (int i = 0; i < gTAs_pa.Length; i++)
                        {
                            gTAs[gTAs.GetIndex(of: gTAs_pa[i])] = gTAs_ta[i];
                        }
                    }

                    mI = cand.MakeGenericMethod(gTAs);

                    Debug.Assert(mI.IsConstructedGenericMethod);
                }
                else
                {
                    var pT_s = pIs[0].ParameterType;
                    if (!pT_s.IsAssignableFrom(typeof(IScroll))) continue;

                    var pT_a = pIs[1].ParameterType;
                    isByRef = pT_a.IsByRef;
                    if (isByRef) pT_a = pT_a.GetElementType()!;
                    if (!pT_a.IsAssignableFrom(instanceType)) continue;

                    mI = cand;
                }

                switch (mI.Name, isByRef)
                {
                    case (nameof(Insert), false):
                        Debug.Assert(Insert is null, "既存の挿搴務容を置き換えます。");
                        _insert = mI;
                        break;
                    case (nameof(Insert), true):
                        Debug.Assert(InsertRef is null, "既存の挿搴務容を置き換えます。");
                        _insert_ref = mI;
                        break;
                    case (nameof(Remove), false):
                        Debug.Assert(Remove is null, "既存の挿搴務容を置き換えます。");
                        _remove = mI;
                        break;
                    case (nameof(Remove), true):
                        Debug.Assert(RemoveRef is null, "既存の挿搴務容を置き換えます。");
                        _remove_ref = mI;
                        break;
                }
            }
        }
        return r;
    }

    static readonly Dictionary<Type, IRMethodInformations> _infos = new();

    public static IRMethodInformations GetInfos(Type instanceType)
    {
        if (!_infos.TryGetValue(instanceType, out var r))
        {
            r = new();

            var c = instanceType;
            do
            {
                if (r.Init(c)) goto completed;
                c = c.BaseType;
            }
            while (c is not null);
            foreach (var c_ in instanceType.GetInterfaces())
            {
                if (r.Init(c_)) goto completed;
            }
            throw new KeyNotFoundException();

        completed:;
            _infos.Add(instanceType, r);
        }
        return r;
    }
    public static IRMethodInformations GetInfos(Type scrollType, Type instanceType)
    {
        var r = GetInfos(instanceType);
        Replace(ref r._insert, scrollType);
        Replace(ref r._insert_ref, scrollType);
        Replace(ref r._remove, scrollType);
        Replace(ref r._remove_ref, scrollType);
        return r;        

        static void Replace(ref MethodInfo? mI, Type scrollT)
        {
            if (mI is not null && mI.IsGenericMethod)
            {
                var gATs = mI.GetGenericArguments();
                gATs.ReplaceFirst(typeof(IScroll), scrollT);
                mI = mI.GetGenericMethodDefinition().MakeGenericMethod(gATs);
            }
        }
    }
}

public readonly unsafe struct IRDelegate
{
    readonly delegate*<IScroll, in object, void> _insert_ref;
    readonly delegate*<IScroll, out object, void> _remove_ref;

    public IRDelegate(Type type)
    {
        var infos = IRMethodInformations.GetInfos(type);

        if (type.IsValueType)
        {
            if (infos.InsertRef is null || infos.RemoveRef is null) throw new NullReferenceException();
            foreach(var mI in typeof(Helper<>).MakeGenericType(type).GetMethods())
            {
                switch (mI.Name)
                {
                    case nameof(Insert):
                    _insert_ref = (delegate*<IScroll, in object, void>)mI.MethodHandle.GetFunctionPointer();
                    break;
                    case nameof(Remove):
                    _remove_ref = (delegate*<IScroll, out object, void>)mI.MethodHandle.GetFunctionPointer();
                    break;
                }
            }
        }
        else
        {
            if (infos.InsertRef is null || infos.RemoveRef is null) throw new NullReferenceException();
            _insert_ref = (delegate*<IScroll, in object, void>)infos.InsertRef.MethodHandle.GetFunctionPointer();
            _remove_ref = (delegate*<IScroll, out object, void>)infos.RemoveRef.MethodHandle.GetFunctionPointer();
        }
    }

    public void Insert(IScroll scroll, in object value) => _insert_ref(scroll, value);
    public void Remove(IScroll scroll, out object value) => _remove_ref(scroll, out value);

    public static class Helper<T> where T : struct
    {
        public static void Insert(IScroll scroll, in object value) => IRDelegate<IScroll, T>.Default.Insert(scroll, (T)value);
        public static void Remove(IScroll scroll, out object value) { IRDelegate<IScroll, T>.Default.Remove(scroll, out var r); value = r; }
    }
}

public readonly unsafe struct IRDelegate<TScroll, T> where TScroll : IScroll
{
    readonly internal delegate* <TScroll, in T, void> _insert_ref;
    readonly internal delegate* <TScroll, T, void> _insert;
    readonly internal delegate* <TScroll, out T, void> _remove_ref;
    readonly internal delegate* <TScroll, T, void> _remove;

    public IRDelegate()
    {
        var infos = IRMethodInformations.GetInfos(typeof(TScroll), typeof(T));
        if (infos.Insert is not null) _insert = (delegate* <TScroll, T, void>)infos.Insert.MethodHandle.GetFunctionPointer();
        if (infos.InsertRef is not null) _insert_ref = (delegate* <TScroll, in T, void>)infos.InsertRef.MethodHandle.GetFunctionPointer();
        if (infos.Remove is not null) _remove = (delegate* <TScroll, T, void>)infos.Remove.MethodHandle.GetFunctionPointer();
        if (infos.RemoveRef is not null) _remove_ref = (delegate* <TScroll, out T, void>)infos.RemoveRef.MethodHandle.GetFunctionPointer();
    }

    public void Insert(TScroll scroll, in T value) => _insert_ref(scroll, value);
    public void Insert(TScroll scroll, T value) => _insert(scroll, value);
    public void Remove(TScroll scroll, out T value) => _remove_ref(scroll, out value);
    public void Remove(TScroll scroll, T value) => _remove(scroll, value);

    public static IRDelegate<TScroll, T> Default { get; } = new();
}
