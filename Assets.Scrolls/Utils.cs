using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Scrolls;
public static class Utils
{
    public static async Task InsertArrayAsBox<TDataBox, TStructure>(IScroll to, TStructure[] array) where TDataBox : IDataBox where TStructure : unmanaged
    {
        var p = to.Point;
        await to.Insert(typeIdentifier: new(typeof(TDataBox)));
        await to.Insert(memory: (Memory<TStructure>)array);
        var s = to.Point;
        var e = to.Point;
        to.Point = p;
        await to.Insert(s);
        to.Point = e;
    }
    public static Task RemoveArrayAsBox<TDataBox, TStructure>(IScroll from, out TStructure[] array) where TDataBox : IDataBox where TStructure : unmanaged
    {
        from.Remove(pointer: out var pointer).Wait();
        var array_ = array = new TStructure[from.FigureOutDistance<TStructure>(pointer)];

        return Async();
        async Task Async()
        {
            await from.Remove(typeIdentifier: out var tId);
            await from.Remove(memory: (Memory<TStructure>)array_);
            from.Point = pointer;

            CheckTypeId<TDataBox>(tId);
        }
    }

    public static void CheckTypeId<T>(TypeIdentifier typeId)
    {
        if (!typeId.IsValid || typeId.IsIdentifying(typeof(T))) throw new Exception("函の指定型が員函の型と一致しません。");
    }
}
