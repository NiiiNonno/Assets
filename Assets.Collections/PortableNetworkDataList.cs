using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Collections;

public abstract class PortableNetworkDataList
{
    public static uint MAGIC_NUMBER = 0x04C11DB7;

    //readonly ITwoWayDictionary<ASCIIString, Type> _dictionary;
    readonly uint _mN4CRC;
    readonly byte[] _signature;
    readonly List<object> _data;

    public IEnumerable<object> Data => _data;

    public PortableNetworkDataList(ulong signature, uint magicNumberForCyclicRedundancyCheck, ITwoWayDictionary<ASCIIString, Type> dictionary) : this(BitConverter.GetBytes(signature), magicNumberForCyclicRedundancyCheck, dictionary) { }
    public PortableNetworkDataList(byte[] signature, uint magicNumberForCyclicRedundancyCheck, ITwoWayDictionary<ASCIIString, Type> dictionary)
    {
        if (signature.Length != 8) throw new ArgumentException("署名は八バイトである必要があります。", nameof(signature)); 

        _mN4CRC = magicNumberForCyclicRedundancyCheck;
        _signature = signature;
        _data = new();
    }

    public void Add<T>(T data, uint length)
    {
        //AddChunk(new PortableNetworkDataChunk<T>(length, , data));
    }
    public virtual void AddChunk(object chunk)
    {
    }

    public void Remove<T>(T data, uint length)
    {
        //RemoveChunk(new PortableNetworkDataChunk<T>(length,, data));
    }
    public virtual void RemoveChunk(object chunk)
    {
    }

    public virtual void Clear()
    {
        _data.Clear();
    }

    static readonly HashTableTwoWayDictionary<ASCIIString, Type> _dictionary;
    public static ITwoWayDictionary<ASCIIString, Type> Dictionary { get; }

    static PortableNetworkDataList()
    {
        _dictionary = new HashTableTwoWayDictionary<ASCIIString, Type>();
    }

    protected static Task Insert(INote to, PortableNetworkDataList dataList, Span<byte> signature, uint magicNumber, ITwoWayDictionary<ASCIIString, Type> dictionary)
    {
        to.InsertSync(span: signature);

        return Async(); async Task Async()
        {
            var dN = new DuplicatingNote();
            dN.Put(to);

            foreach (var data in dataList.Data)
            {
                var type = data.GetType();
                var mN = new MemoryNote();
                var aType = dictionary.Opposite[type];
                var buf = new byte[4];

                dN.Put(mN);
                await dN.Insert(@object: data, @as: type);
                dN.Take(mN);

                var etor = mN.Memory.GetEnumerator();

                // length確認。
                int c0 = 0;
                foreach (var item in mN.Memory)
                {
                    buf[c0++] = item;
                    if (c0 == buf.Length) break;
                }
                if (mN.Length - 8 != BitConverter.ToUInt32(buf)) throw new Exception("数據の保存によって書かれた数據長と実際のそれが異なります。");

                // type確認。
                var memory = mN.Memory.Skip(buf.Length);
                int c1 = 0;
                foreach (var item in memory)
                {
                    buf[c1++] = item;
                    if (c1 == buf.Length) break;
                }
                if (!aType.AsSpan().SequenceEqual(buf)) throw new Exception("数據の保存によって書かれた数據区型名と実際のそれが異なります。");

                await dN.Insert(uInt32: Assets.Utils.GetCyclicRedundancyCheck(magicNumber, mN.Memory));
            }

            dN.Take(to);
        }
    }
    protected static Task Remove(INote from, PortableNetworkDataList dataList, Span<byte> signature, uint magicNumber, ITwoWayDictionary<ASCIIString, Type> dictionary)
    {
        from.RemoveSync(span: signature);

        return Async(); async Task Async()
        {
#warning CRCチェックない
            while (true)
            {
                var sI = from.Point;
                await from.Remove(uInt32: out var length);
                await from.Remove(uInt32: out var typeName);
                var type = dictionary[new ASCIIString((Span<byte>)BitConverter.GetBytes(typeName))];
                await from.Insert(uInt32: length);
                await from.Insert(uInt32: typeName);
                from.Point = sI;
                await from.Remove(@object: out var @object, @as: type);
                await from.Remove(uInt32: out var _); // remove crc
                if (@object is PortableNetworkDataTrailerChunk) break;
                dataList.AddChunk(@object);
            }
        }
    }
}

#warning チャンクであるかをどう判断するべきか。
public abstract record PortableNetworkDataChunk
{
    readonly uint _length;
    readonly uint _tNI;

    public uint Length => Length;
    public uint TypeNameIntegar => _tNI;
    public ASCIIString TypeName => new((Span<byte>)BitConverter.GetBytes(_tNI));

    public PortableNetworkDataChunk(uint length, ASCIIString typeName) : this(length, BitConverter.ToUInt32(typeName.AsSpan())) { }
    public PortableNetworkDataChunk(uint length, uint typeNameIntegar)
    {
        _length = length;
        _tNI = typeNameIntegar;
    }
}

public sealed record PortableNetworkDataChunk<T> : PortableNetworkDataChunk, IEquatable<PortableNetworkDataChunk<T>?>
{
    readonly T _data;

    public T Data => _data;

    public PortableNetworkDataChunk(uint length, ASCIIString typeName, T data) : base(length, typeName)
    {
        _data = data;
    }
    public PortableNetworkDataChunk(uint length, uint typeNameIntegar, T data) : base(length, typeNameIntegar)
    {
        _data = data;
    }
}

public record PortableNetworkDataTrailerChunk : PortableNetworkDataChunk
{
    public PortableNetworkDataTrailerChunk(ASCIIString typeName, uint length = 0) : base(length, typeName) { }
    public PortableNetworkDataTrailerChunk(uint length, uint typeNameIntegar) : base(length, typeNameIntegar) { }
}

partial class NoteExtensions
{
    [IRMethod]
    public static async Task Insert<T>(this INote @this, PortableNetworkDataChunk<T> portableNetworkDataChunk)
    {
        await @this.Insert(uInt32: portableNetworkDataChunk.Length);
        await @this.Insert(uInt32: portableNetworkDataChunk.TypeNameIntegar);
        await @this.Insert(instance: portableNetworkDataChunk.Data);
    }
    [IRMethod]
    public static Task Remove<T>(this INote @this, out PortableNetworkDataChunk<T> portableNetworkDataChunk)
    {
        @this.Remove(uInt32: out uint length).Wait();
        @this.Remove(uInt32: out var typeNameIntegar).Wait();
        var r = @this.Remove(instance: out T instance);
        portableNetworkDataChunk = new(length, typeNameIntegar, instance);
        return r;
    }

    [IRMethod]
    public static async Task Insert<T>(this INote @this, PortableNetworkDataTrailerChunk portableNetworkDataTrailerChunk)
    {
        await @this.Insert(uInt32: portableNetworkDataTrailerChunk.Length);
        await @this.Insert(uInt32: portableNetworkDataTrailerChunk.TypeNameIntegar);
    }
    [IRMethod]
    public static Task Remove<T>(this INote @this, out PortableNetworkDataTrailerChunk portableNetworkDataTrailerChunk)
    {
        @this.Remove(uInt32: out var length).Wait();
        @this.Remove(uInt32: out var typeNameIntegar).Wait();
        portableNetworkDataTrailerChunk = new(length, typeNameIntegar);
        return Task.CompletedTask;
    }
}
