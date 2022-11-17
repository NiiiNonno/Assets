﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Nonno.Assets;
using Nonno.Assets.Collections;
using Nonno.Assets.Scrolls;
using static Nonno.Assets.Utils;
[assembly: TypeIdentifier(typeof(BytesDataBox), "465C4674-A32E-47B4-B347-1A49F7B17634")]
[assembly: TypeIdentifier(typeof(StringBox), "F229F39C-7BB9-4958-9EE6-E26846F69E1B")]
[assembly: TypeIdentifier(typeof(EmptyBox), "3b425a78-b46d-4129-9e46-b81b833fe2a4")]

namespace Nonno.Assets.Scrolls;
public interface IDataBox
{
}

//public class UnopenedDataBox : IDataBox
//{
//    readonly INote _note;
//    readonly NotePointer _pointer;

//    public Type Type { get; }

//    public UnopenedDataBox(Type type, INote note, NotePointer pointer)
//    {
//        _note = note;
//        _pointer = pointer;

//        Type = type;
//    }

//    public bool Is<T>() => Type.IsAssignableTo(typeof(T));

//    public T? As<T>()
//    {
//        if (Is<T>()) return (T)Open();
//        else return default;
//    }
//    public async ValueTask<T?> AsAsync<T>()
//    {
//        if (Is<T>()) return (T)await OpenAsync();
//        else return default;
//    }

//    public IDataBox Open()
//    {
//        var p = _note.Pointer;
//        _note.Pointer = _pointer;
//        _note.Remove(dataBox: out var dataBox).Wait();
//        _note.Pointer = p;
//        return dataBox;
//    }
//    public async ValueTask<IDataBox> OpenAsync()
//    {
//        var p = _note.Pointer;
//        _note.Pointer = _pointer;
//        await _note.Remove(dataBox: out var dataBox);
//        _note.Pointer = p;
//        return dataBox;
//    }
//}

public readonly struct BytesDataBox : IDataBox
{
    [MemberNotNullWhen(false, nameof(Data))]
    public bool IsEmpty => Data == null;
    public byte[] Data { get; }

    public BytesDataBox(int length) => Data = new byte[length];
    public BytesDataBox(byte[] data) => Data = data;

    public static BytesDataBox Copy(byte[] from)
    {
        var arr = new byte[from.Length];
        Array.Copy(from, arr, 0);
        return new(arr);
    }
}

public readonly struct StringBox : IDataBox
{
    public readonly Latin1String @string;

    public bool IsEmpty => @string is null;

    public StringBox(Latin1String @string) => this.@string = @string;
}

public readonly struct EmptyBox : IDataBox { }

public static partial class ScrollExtensions
{
    [IRMethod]
    public static Task Insert(this IScroll @this, IDataBox dataBox) => FundamentalScrollUtils.Insert(to: @this, @object: dataBox, @as: dataBox.GetType());
    [IRMethod]
    public static Task Remove(this IScroll @this, out IDataBox dataBox)
    {
        var p = @this.Point;

        @this.Remove(pointer: out var point).Wait();
        @this.Remove(uniqueIdentifier: out UniqueIdentifier<Type> tId).Wait();

        @this.Insert(pointer: point).Wait();
        @this.Insert(uniqueIdentifier: tId).Wait();

        @this.Point = p;

        var r = FundamentalScrollUtils.Remove(to: @this, @object: out var dB_obj, @as: TypeIdentifierConverter.INSTANCE[tId]);
        if (dB_obj is not IDataBox dB) throw new Exception("搴取した型が`IDataBox`を実装していません。");
        dataBox = dB;

        @this.Point = point;
        return r;
    }
    internal static void SkipDataBox(this IScroll @this)
    {
        @this.Remove(pointer: out var p_n).Wait();

        var p_1 = @this.Point;

        p_n = @this.Point = p_n;
        var p_m = @this.Point;

        @this.Point = p_1;

        @this.Insert(pointer: p_n).Wait();

        @this.Point = p_m;
    }
    internal static void RemoveDataBox(this IScroll @this)
    {
        @this.Remove(pointer: out var p_n).Wait();
        var length = @this.FigureOutDistance<byte>(p_n);
        if (length < 0) throw new Exception("現在`RemoveDataBox`の有効性には疑問が呈されており、バイト列に変換できない内容にどう対処するべきかは検討中です。");
        Span<byte> span = stackalloc byte[length > ConstantValues.STACKALLOC_MAX_LENGTH ? ConstantValues.STACKALLOC_MAX_LENGTH : (int)length];
        while (length > 0)
        {
            var span_ = length > span.Length ? span : span[..(int)length];
            @this.RemoveSync(span_);
            length -= span_.Length;
        }
    }

    [IRMethod]
    public static Task Insert(this IScroll @this, in BytesDataBox bytesDataBox)
    {
        return @this.InsertArrayAsBox<BytesDataBox, byte>(bytesDataBox.Data);
    }
    [IRMethod]
    public static Task Remove(this IScroll @this, out BytesDataBox bytesDataBox)
    {
        var r = @this.RemoveArrayAsBox<BytesDataBox, byte>(out var array);
        bytesDataBox = new(array);
        return r;
    }

    [IRMethod]
    public static Task Insert(this IScroll @this, in StringBox stringBox)
    {
        var p = @this.Point;
        @this.Insert(uniqueIdentifier: TypeIdentifierConverter.INSTANCE[typeof(StringBox)]).Wait();
        @this.Insert(latin1String: stringBox.@string).Wait();
        var s = @this.Point;
        var e = @this.Point;
        @this.Point = p;
        var r = @this.Insert(s);
        @this.Point = e;
        return r;
    }
    [IRMethod]
    public static Task Remove(this IScroll @this, out StringBox stringBox)
    {
        @this.Remove(pointer: out var pointer).Wait();
        @this.Remove(uniqueIdentifier: out UniqueIdentifier<Type> tId).Wait();
        Utils.CheckTypeId<StringBox>(tId);
        @this.Remove(latin1String: out var @string);
        @this.Point = pointer;

        stringBox = new(@string);
        return Task.CompletedTask;
    }

    [IRMethod]
    public static async Task Insert(this IScroll @this, EmptyBox emptyBox)
    {
        var p = @this.Point;
        await @this.Insert(uniqueIdentifier: TypeIdentifierConverter.INSTANCE[typeof(EmptyBox)]);
        var s = @this.Point;
        var e = @this.Point;
        @this.Point = p;
        await @this.Insert(s);
        @this.Point = e;
    }
    [IRMethod]
    public static Task Remove(this IScroll @this, out EmptyBox emptyBox)
    {
        emptyBox = default;

        return Async();
        async Task Async()
        {
            await @this.Remove(pointer: out var pointer);
            await @this.Remove(uniqueIdentifier: out UniqueIdentifier<Type> tId);
            Utils.CheckTypeId<EmptyBox>(tId);
            @this.Point = pointer;
        }
    }
}
