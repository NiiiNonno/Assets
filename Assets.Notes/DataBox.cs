using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Nonno.Assets.Collections;
using static Nonno.Assets.Utils;

namespace Nonno.Assets.Notes;
public interface IDataBox
{
    bool Is<T>() => this is T;
    bool Is<T>([MaybeNullWhen(false)]out T value) 
    { 
        if (this is T t) 
        { 
            value = t; 
            return true; 
        } 
        else 
        { 
            value = default; 
            return false; 
        } 
    }
    T? As<T>() => (T)this;
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

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct LeafBox<TStructure> : IDataBox where TStructure : unmanaged
{
    public readonly TStructure structure;

    public LeafBox(TStructure value) => this.structure = value;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct ArrayBox<TStructure> : IDataBox where TStructure : unmanaged
{
    public readonly TStructure[] array;

    public bool IsEmpty => array == null;

    public ArrayBox(TStructure[] array) => this.array = array;
    public ArrayBox(long length) => array = new TStructure[length];
}

public readonly struct StringBox : IDataBox
{
    public readonly Latin1String @string;

    public bool IsEmpty => @string is null;

    public StringBox(Latin1String @string) => this.@string = @string;
}

public readonly struct EmptyBox : IDataBox { }

public readonly struct TypeIdentifier : IEquatable<TypeIdentifier>
{
    public static readonly TypeIdentifier EMPTY = default;

    readonly Guid _id;

    public Guid Identifier => _id;
    public bool IsValid => _id == Guid.Empty;

    public TypeIdentifier(Guid identifier)
    {
        _id = identifier;
    }
    public TypeIdentifier(Type type)
    {
        _id = type.GUID;
    }

    public bool IsIdentifying(Type type) => this == new TypeIdentifier(type);

    public Type GetIdentifiedType() => Assets.Utils.GetType(_id);
    public override bool Equals(object? obj) => obj is TypeIdentifier identifier && Equals(identifier);
    public bool Equals(TypeIdentifier other) => _id.Equals(other._id);
    public override int GetHashCode() => HashCode.Combine(_id);

    public static bool operator ==(TypeIdentifier left, TypeIdentifier right) => left.Equals(right);
    public static bool operator !=(TypeIdentifier left, TypeIdentifier right) => !(left == right);
}

public static partial class NoteExtensions
{
    [IRMethod]
    public static Task Insert(this INote @this, IDataBox dataBox) => @this.Insert(@object: dataBox, @as: dataBox.GetType());
    [IRMethod]
    public static Task Remove(this INote @this, out IDataBox dataBox)
    {
        var p = @this.Pointer;

        @this.Remove(pointer: out var point).Wait();
        @this.Remove(typeIdentifier: out var tId).Wait();

        @this.Insert(pointer: point).Wait();
        @this.Insert(typeIdentifier: tId).Wait();

        @this.Pointer = p;

        var r = @this.Remove(@object: out var dB_obj, @as: tId.GetIdentifiedType());
        if (dB_obj is not IDataBox dB) throw new Exception("搴取した型が`IDataBox`を実装していません。");
        dataBox = dB;

        @this.Pointer = point;
        return r;
    }
    //public static UnopenedDataBox SkipDataBox(this INote @this)
    //{
    //    var p_0 = @this.Pointer;

    //    @this.Remove(pointer: out var p_n).Wait();
    //    @this.Remove(typeIdentifier: out var tId).Wait();

    //    var p_1 = @this.Pointer;

    //    p_n = @this.Pointer = p_n;
    //    var p_m = @this.Pointer;

    //    @this.Pointer = p_1;

    //    @this.Insert(pointer: p_n).Wait();
    //    @this.Insert(typeIdentifier: tId).Wait();

    //    var r = new UnopenedDataBox(tId.GetIdentifiedType(), @this, p_0);

    //    @this.Pointer = p_m;

    //    return r;
    //}

    [IRMethod]
    public static Task Insert<T>(this INote @this, in LeafBox<T> leafBox) where T : unmanaged
    {
        Span<T> span = stackalloc T[] { leafBox.structure };

        var p = @this.Pointer;
        @this.Insert(typeIdentifier: new(typeof(LeafBox<T>))).Wait();
        @this.InsertSync(span: span);
        var s = @this.Pointer;
        var e = @this.Pointer;
        @this.Pointer = p;
        var r = @this.Insert(s);
        @this.Pointer = e;
        return r;
    }
    [IRMethod]
    public static Task Remove<T>(this INote @this, out LeafBox<T> leafBox) where T : unmanaged
    {
        Span<T> span = stackalloc T[1];

        @this.Remove(pointer: out var pointer).Wait();
        @this.Remove(typeIdentifier: out var tId).Wait();
        Utils.CheckTypeId<LeafBox<T>>(tId);
        @this.RemoveSync(span: span);
        @this.Pointer = pointer;

        leafBox = new(span[0]);
        return Task.CompletedTask;
    }

    [IRMethod]
    public static Task Insert<T>(this INote @this, in ArrayBox<T> arrayBox) where T : unmanaged
    {
        return Utils.InsertArrayAsBox<ArrayBox<T>, T>(to: @this, arrayBox.array);
    }
    [IRMethod]
    public static Task Remove<T>(this INote @this, out ArrayBox<T> arrayBox) where T : unmanaged
    {
        var r = Utils.RemoveArrayAsBox<ArrayBox<T>, T>(from: @this, out var array);
        arrayBox = new(array);
        return r;
    }

    [IRMethod]
    public static Task Insert(this INote @this, in StringBox stringBox)
    {
        var p = @this.Pointer;
        @this.Insert(typeIdentifier: new(typeof(StringBox))).Wait();
        @this.Insert(latin1String: stringBox.@string).Wait();
        var s = @this.Pointer;
        var e = @this.Pointer;
        @this.Pointer = p;
        var r = @this.Insert(s);
        @this.Pointer = e;
        return r;
    }
    [IRMethod]
    public static Task Remove(this INote @this, out StringBox stringBox)
    {
        @this.Remove(pointer: out var pointer).Wait();
        @this.Remove(typeIdentifier: out var tId).Wait();
        Utils.CheckTypeId<StringBox>(tId);
        @this.Remove(latin1String: out var @string);
        @this.Pointer = pointer;

        stringBox = new(@string);
        return Task.CompletedTask;
    }

    [IRMethod]
    public static async Task Insert(this INote @this, EmptyBox emptyBox)
    {
        var p = @this.Pointer;
        await @this.Insert(typeIdentifier: new(typeof(EmptyBox)));
        var s = @this.Pointer;
        var e = @this.Pointer;
        @this.Pointer = p;
        await @this.Insert(s);
        @this.Pointer = e;
    }
    [IRMethod]
    public static Task Remove(this INote @this, out EmptyBox emptyBox)
    {
        emptyBox = default;

        return Async();
        async Task Async()
        {
            await @this.Remove(pointer: out var pointer);
            await @this.Remove(typeIdentifier: out var tId);
            Utils.CheckTypeId<EmptyBox>(tId);
            @this.Pointer = pointer;
        }
    }

    [IRMethod]
    public static Task Insert(this INote @this, in TypeIdentifier typeIdentifier)
    {
        @this.InsertSync(stackalloc[] { typeIdentifier });
        return Task.CompletedTask;
    }
    [IRMethod]
    public static Task Remove(this INote @this, out TypeIdentifier typeIdentifier)
    {
        Span<TypeIdentifier> span = stackalloc TypeIdentifier[1];
        @this.RemoveSync(span: span);
        typeIdentifier = span[0];
        return Task.CompletedTask;
    }
}
