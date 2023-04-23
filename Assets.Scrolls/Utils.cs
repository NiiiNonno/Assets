using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Nonno.Assets.Collections;

namespace Nonno.Assets.Scrolls;
public static partial class Utils
{
    public static void InsertStructureAsBox<TDataBox, TStructure>(this IScroll @this, in TStructure structure) where TStructure : unmanaged
    {
        var p = @this.Point;
        @this.Insert(type: typeof(TDataBox));
        @this.Insert(value: in structure);
        var s = @this.Point;
        var e = @this.Point;
        @this.Point = p;
        @this.Insert(s);
        @this.Point = e;
    }
    public static void RemoveStructureAsBox<TDataBox, TStructure>(this IScroll @this, out TStructure structure) where TStructure : unmanaged
    {
        @this.Remove(pointer: out var pointer);
        @this.Remove(type: out var type);
        CheckTypeId<TDataBox>(type);
        @this.Remove(value: out structure);
        @this.Point = pointer;
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="TDataBox">
    /// <oara>
    /// 指定した型は各挿搴務容が函式で定義されている必要があります。
    /// </oara>
    /// </typeparam>
    /// <typeparam name="TStructure"></typeparam>
    /// <param name="this"></param>
    /// <param name="array"></param>
    /// <returns></returns>
    public static void InsertArrayAsBox<TDataBox, TStructure>(this IScroll @this, Span<TStructure> array) where TStructure : unmanaged
    {
        var p = @this.Point;
        @this.Insert(type: typeof(TDataBox));
        @this.Insert(span: array);
        var s = @this.Point;
        var e = @this.Point;
        @this.Point = p;
        @this.Insert(s);
        @this.Point = e;
    }
    /// <summary>
    /// </summary>
    /// <typeparam name="TDataBox">
    /// <oara>
    /// 指定した型は各挿搴務容が函式で定義されている必要があります。
    /// </oara>
    /// </typeparam>
    /// <typeparam name="TStructure"></typeparam>
    /// <param name="to"></param>
    /// <param name="array"></param>
    /// <returns></returns>
    public static void RemoveArrayAsBox<TDataBox, TStructure>(this IScroll @this, out Span<TStructure> array) where TStructure : unmanaged
    {
        @this.Remove(pointer: out var pointer);
        @this.Remove(type: out var type);
        CheckTypeId<TStructure>(type);

        var list = new ArrayList<TStructure>();
        while(!@this.Is(on: pointer))
        {
            @this.Remove<TStructure>(value: out var value);
            list.Add(value);
        }
        array = list.UnsafeAsSpan();
    }

    public static void CheckTypeId<T>(Type? type)
    {
        if (typeof(T) != type)
        //if (!typeId.IsValid || TypeIdentifierConverter.INSTANCE[typeId] != typeof(T))
            throw new Exception("函の指定型が員函の型と一致しません。");
    }
}
