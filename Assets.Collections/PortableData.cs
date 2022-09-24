using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Collections;

[PortableDataType(0x89504E470D0A1A0A, "")]
public class PortableData
{
    readonly ITwoWayDictionary<ASCIIString, Type> _dictionary;
    readonly byte[] _signature;
    readonly List<object> _data;

    public ITwoWayDictionary<ASCIIString, Type> Dictionary => _dictionary;
    public ReadOnlySpan<byte> Signature => _signature;
    public IEnumerable<object> Data => _data;

    public PortableData(ulong signature, ITwoWayDictionary<ASCIIString, Type> dictionary) : this(BitConverter.GetBytes(signature), dictionary) { }
    public PortableData(byte[] signature, ITwoWayDictionary<ASCIIString, Type> dictionary)
    {
        if (signature.Length != 8) throw new ArgumentException("署名は八バイトである必要があります。", nameof(signature)); 

        _dictionary = dictionary;
        _signature = signature;
        _data = new();
    }

    public void Add(object data)
    {
        _data.Add(data);
    }

    public void Remove(object data)
    {
        _ = _data.Remove(data);
    }
}

partial class NoteExtensions
{
    //public static async Task Insert(this INote @this, PortableData portableData)
    //{
    //    @this.InsertSync(portableData.Signature.ToArray().AsSpan());

    //    var dN = new DuplicatingNote(@this);

    //    foreach (var data in portableData.Data)
    //    {
    //        var sP = dN.Point;
    //        var mN = new MemoryNote();
    //        dN.Add(mN);
    //        var type = data.GetType();
    //        await dN.Insert(asciiString: portableData.Dictionary.Opposite[type]);
    //        await dN.Insert(data, type);
    //        dN.Remove(mN);
    //        var eP = dN.Point;
    //        dN.Point = sP;
    //        await dN.Insert(uint32: mN.Length);
    //        dN.Point = eP;
    //        await dN.Insert(uint32: mN.GetCyclicRedundancyCheck());
    //    }
    //}
}
