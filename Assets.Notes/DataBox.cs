using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Notes;
public abstract class DataBox
{
    static DataBox()
    {

    }
}

public readonly struct TypeIdentifier
{
    readonly Guid _id;

    public Guid Identifier => _id;

    public TypeIdentifier(Guid identifier)
    {
        _id = identifier;
    }
    public TypeIdentifier(Type type)
    {
        _id = type.GUID;
    }

    public Type GetIdentifiedType() => Utils.GetType(_id);
}

public static partial class NoteExtensions
{
    [IRMethod]
    public static Task Insert(this INote @this, DataBox dataBox) => @this.Insert(@object: dataBox, @as: dataBox.GetType());
    [IRMethod]
    public static Task Remove(this INote @this, out DataBox dataBox)
    {
        var p = @this.Point;

        @this.Remove(index: out var point).Wait();
        @this.Remove(typeIdentifier: out var tId).Wait();

        @this.Insert(index: point).Wait();
        @this.Insert(typeIdentifier: tId).Wait();

        @this.Point = p;

        var r = @this.Remove(@object: out var dB_obj, @as: tId.GetIdentifiedType());
        if (dB_obj is not DataBox dB) throw new Exception("搴取した実体が`DataBox`の派生型ではありませんでした。");
        dataBox = dB;

        @this.Point = point;
        return r;
    }
    public static ReinstateDelegate<DataBox> Skip(this INote @this)
    {
        var p = @this.Point;

        @this.Remove(index: out var point).Wait();
        @this.Remove(typeIdentifier: out var tId).Wait();

        @this.Insert(index: point).Wait();
        @this.Insert(typeIdentifier: tId).Wait();

        @this.Point = point;

        return async Task<DataBox>() =>
        {
            @this.Point = p;
            await @this.Remove(@object: out var dB_obj, @as: tId.GetIdentifiedType());
            if (dB_obj is not DataBox dB) throw new Exception("搴取した実体が`DataBox`の派生型ではありませんでした。");
            return dB;
        };
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
