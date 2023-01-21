using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Nonno.Assets.Collections;

namespace Nonno.Assets.Scrolls;
public static partial class Utils
{
    public static IScroll GetScroll(string fileName) => GetScroll(new Uri(Path.GetFullPath(fileName)));
    public static IScroll GetScroll(Uri uri)
    {
        switch (uri.Scheme)
        {
            case "http":
            {


                goto default;
            }
            case "https":
            {
                goto default;
            }
            case "file":
            {
                var path = uri.AbsolutePath;
                switch (path[(path.LastIndexOf('.') + 1)..])
                {
                    default:
                    break;
                }
                goto default;
            }
            case "ftp":
            {
                goto default;
            }
            case "nfs":
            {
                goto default;
            }
            case "files-scl":
            {
                goto default;
            }
            case "cell":
            {
                goto default;
            }
            case "data":
            {
                goto default;
            }
            default:
            throw new NotSupportedException("対応していないスキーマです。");
        }
    }

    public static Task InsertStructureAsBox<TDataBox, TStructure>(this IScroll @this, in TStructure structure) where TDataBox : IDataBox where TStructure : unmanaged
    {
        var p = @this.Point;
        @this.Insert(uniqueIdentifier: TypeIdentifierConverter.INSTANCE[typeof(TDataBox)]).Wait();
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
        @this.Remove(uniqueIdentifier: out UniqueIdentifier<Type> tId).Wait();
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
        await @this.Insert(uniqueIdentifier: TypeIdentifierConverter.INSTANCE[typeof(TDataBox)]);
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
            await @this.Remove(uniqueIdentifier: out UniqueIdentifier<Type> tId);
            await @this.Remove(memory: (Memory<TStructure>)array_);
            @this.Point = pointer;

            CheckTypeId<TDataBox>(tId);
        }
    }

    public static void CheckTypeId<T>(UniqueIdentifier<Type> typeId)
    {
        if (!typeId.IsValid || TypeIdentifierConverter.INSTANCE[typeId] != typeof(T)) throw new Exception("函の指定型が員函の型と一致しません。");
    }
}
