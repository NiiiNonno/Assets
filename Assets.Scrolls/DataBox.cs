using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Nonno.Assets;
using Nonno.Assets.Collections;
using Nonno.Assets.Scrolls;
using MI = System.Runtime.CompilerServices.MethodImplAttribute;
using MIO = System.Runtime.CompilerServices.MethodImplOptions;
using static Nonno.Assets.Utils;

namespace Nonno.Assets.Scrolls;

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

[DataBox]
public readonly struct BytesDataBox
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

[DataBox]
public readonly struct StringBox
{
    public readonly Latin1String @string;

    public bool IsEmpty => @string is null;

    public StringBox(Latin1String @string) => this.@string = @string;
}

[DataBox]
public readonly struct EmptyBox { }

public static partial class ScrollExtensions
{
    public static readonly Dictionary<Type, IRDelegate> _delegates = new();
    
    [MI(MIO.AggressiveInlining)]
    public static void Insert(this IScroll @this, in object dataBox)
    {
        var type = dataBox.GetType();

        if (!_delegates.TryGetValue(type, out var @delegate))
        {
            if (!type.GetCustomAttributes(typeof(DataBoxAttribute), false).Any()) throw new ArgumentException("渡された物は匱に入りません。", nameof(dataBox));
            @delegate = new(type);
            _delegates.Add(type, @delegate);
        }
        @delegate.Insert(@this, dataBox);
    }
    [MI(MIO.AggressiveInlining)]
    public static void Remove(this IScroll @this, out object dataBox)
    {
        var p = @this.Point;

        @this.Remove(pointer: out var point);
        @this.Remove(type: out var type);

        if (type is null) ThrowHelper.FailToGetReflections();

        @this.Insert(pointer: point);
        @this.Insert(type: type);

        @this.Point = p;

        if (!_delegates.TryGetValue(type, out var @delegate))
        {
            if (!type.GetCustomAttributes(typeof(DataBoxAttribute), false).Any()) throw new ArgumentException("渡された物は匱に入りません。", nameof(dataBox));
            @delegate = new(type);
            _delegates.Add(type, @delegate);
        }
        @delegate.Remove(@this, out dataBox);

        if (!@this.Is(on:point)) throw new FormatException("匱に不整合があります。正しく取得されていません。");
    }
    internal static Type SkipDataBox(this IScroll @this)
    {
        @this.Remove(pointer: out var p_n);
        @this.Remove(type: out var r);

        var p_1 = @this.Point;
        p_n = @this.Point = p_n;
        var p_m = @this.Point;

        @this.Point = p_1;

        @this.Insert(pointer: p_n);
        @this.Insert(type: r);

        @this.Point = p_m;

        return r;
    }

    [IRMethod]
    public static void Insert(this IScroll @this, in BytesDataBox bytesDataBox)
    {
        @this.InsertArrayAsBox<BytesDataBox, byte>(bytesDataBox.Data);
    }
    [IRMethod]
    public static void Remove(this IScroll @this, out BytesDataBox bytesDataBox)
    {
        @this.RemoveArrayAsBox<BytesDataBox, byte>(out var array);
        bytesDataBox = new(array.ToArray());
    }

    [IRMethod]
    public static void Insert(this IScroll @this, in StringBox stringBox)
    {
        var p = @this.Point;
        @this.Insert(type: typeof(StringBox));
        @this.Insert(latin1String: stringBox.@string);
        var s = @this.Point;
        var e = @this.Point;
        @this.Point = p;
        @this.Insert(s);
        @this.Point = e;
    }
    [IRMethod]
    public static void Remove(this IScroll @this, out StringBox stringBox)
    {
        @this.Remove(pointer: out var pointer);
        @this.Remove(type: out var type);
        Utils.CheckTypeId<StringBox>(type);
        @this.Remove(latin1String: out var @string);
        @this.Point = pointer;
        stringBox = new(@string);
    }

    [IRMethod]
    public static void Insert(this IScroll @this, EmptyBox emptyBox)
    {
        var p = @this.Point;
        @this.Insert(type: typeof(EmptyBox));
        var s = @this.Point;
        var e = @this.Point;
        @this.Point = p;
        @this.Insert(s);
        @this.Point = e;
    }
    [IRMethod]
    public static void Remove(this IScroll @this, out EmptyBox emptyBox)
    {
        @this.Remove(pointer: out var pointer);
        @this.Remove(type: out var type);
        Utils.CheckTypeId<EmptyBox>(type);
        @this.Point = pointer;
    }
}
