using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Scrolls;
public static partial class Utils
{
    public static Task InsertStructureAsBox<TDataBox, TStructure>(this IScroll @this, in TStructure structure) where TDataBox : IDataBox where TStructure : unmanaged
    {
        var p = @this.Point;
        @this.Insert(typeIdentifier: TypeIdentifier.Get<TDataBox>()).Wait();
        var task = @this.Insert(value: in structure);

        return Async();
        async Task Async()
        {
            await task;
            var s = @this.Point;
            var e = @this.Point;
            @this.Point = p;
            await @this.Insert(s);
            @this.Point = e;
        }
    }
    public static Task RemoveStructureAsBox<TDataBox, TStructure>(this IScroll @this, out TStructure structure) where TDataBox : IDataBox where TStructure : unmanaged
    {
        @this.Remove(pointer: out var pointer).Wait();
        @this.Remove(typeIdentifier: out var tId).Wait();
        CheckTypeId<TDataBox>(tId);
        var r = @this.Remove(value: out structure);
        @this.Point = pointer;

        return r;
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
    public static async Task InsertArrayAsBox<TDataBox, TStructure>(this IScroll @this, Memory<TStructure> array) where TDataBox : IDataBox where TStructure : unmanaged
    {
        var p = @this.Point;
        await @this.Insert(typeIdentifier: TypeIdentifier.Get<TDataBox>());
        await @this.Insert(memory: array);
        var s = @this.Point;
        var e = @this.Point;
        @this.Point = p;
        await @this.Insert(s);
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
    public static Task RemoveArrayAsBox<TDataBox, TStructure>(this IScroll @this, out TStructure[] array) where TDataBox : IDataBox where TStructure : unmanaged
    {
        @this.Remove(pointer: out var pointer).Wait();
        var array_ = array = new TStructure[@this.FigureOutDistance<TStructure>(pointer)];

        return Async();
        async Task Async()
        {
            await @this.Remove(typeIdentifier: out var tId);
            await @this.Remove(memory: (Memory<TStructure>)array_);
            @this.Point = pointer;

            CheckTypeId<TDataBox>(tId);
        }
    }

    public static void CheckTypeId<T>(TypeIdentifier typeId)
    {
        if (!typeId.IsValid || typeId.IsIdentifying(typeof(T))) throw new Exception("函の指定型が員函の型と一致しません。");
    }
}
