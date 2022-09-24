using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nonno.Assets;
using Nonno.Assets.Collections;

namespace Assets.Graphics;
public class PortableNetworkGraphic : PortableData
{
    public const ulong FILE_SIGNATURE = 0x89504E470D0A1A0A;

    public PortableNetworkGraphic() : base(FILE_SIGNATURE, new HashTableTwoWayDictionary<ASCIIString, Type>())
    {
    }
}

[PortableDataType(PortableNetworkGraphic.FILE_SIGNATURE, "IHDR")]
public class ImageHeaderChunk
{
    
}

[PortableDataType(PortableNetworkGraphic.FILE_SIGNATURE, "IEND")]
public class ImageTrailer
{

}
