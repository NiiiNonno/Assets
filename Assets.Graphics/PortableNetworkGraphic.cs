using System.Diagnostics.CodeAnalysis;
using Nonno.Assets.Collections;
using static Nonno.Assets.Utils;

namespace Nonno.Assets.Graphics;
public class PortableNetworkGraphic : PortableNetworkDataList
{
    public const ulong FILE_SIGNATURE = 0x89504E470D0A1A0A;
    public const uint MAGIC_NUMBER_FOR_CYCLIC_REDUNDANCY_CHECK = 0x04C11DB7;
    public static readonly HashTableTwoWayDictionary<ASCIIString, Type> DICTIONARY = new HashTableTwoWayDictionary<ASCIIString, Type>() 
    {
        { (ASCIIString)"IHDR", typeof(ImageHeaderChunk) },
        { (ASCIIString)"IEND", typeof(ImageTrailerChunk) },
        { (ASCIIString)"PLTE", typeof(PaletteChunk) },
        { (ASCIIString)"IDAT", typeof(ImageDataChunk) },
        { (ASCIIString)"IHDR", typeof(ImageHeaderChunk) },
    };

    public PortableNetworkGraphic() : base(FILE_SIGNATURE, MAGIC_NUMBER_FOR_CYCLIC_REDUNDANCY_CHECK, DICTIONARY)
    {
        Collections.IDictionary<string, int> dic;
    }

    public static Task Insert(INote to, PortableNetworkGraphic portableNetworkGraphic) => Insert(to, portableNetworkGraphic, BitConverter.GetBytes(FILE_SIGNATURE), MAGIC_NUMBER_FOR_CYCLIC_REDUNDANCY_CHECK, DICTIONARY);
}

[PortableNetworkDataChunk(PortableNetworkGraphic.FILE_SIGNATURE, "IHDR")]
public class ImageHeaderChunk : PortableNetworkDataChunk
{
    
}

[PortableNetworkDataChunk(PortableNetworkGraphic.FILE_SIGNATURE, "IEND")]
public class ImageTrailerChunk : PortableNetworkDataChunk
{

}

[PortableNetworkDataChunk(PortableNetworkGraphic.FILE_SIGNATURE, "PLTE")]
public class PaletteChunk
{

}

[PortableNetworkDataChunk(PortableNetworkGraphic.FILE_SIGNATURE, "IDAT")]
public class ImageDataChunk
{
    readonly byte[] _data;

    public byte[] Data => _data;

    public ImageDataChunk(int length)
    {
        _data = new byte[length];
    }

    public ShallowAccessor GetShallowAccessor(Range range, FilterType filterType, int depth) => new(this, range, filterType, depth);

    public abstract class Accessor
    {
        internal const int FILTER_TYPE_LENGTH = 1;

        readonly ImageDataChunk _iDC;
        readonly Range _range;
        int[]? _indexes;

        protected byte[] Data => _iDC._data;
        public Range Range => _range;
        public abstract int Stride { get; }
        private int[] Indexes
        {
            get
            {
                if (_indexes is null)
                {
                    _indexes = new int[Range.Height];
                    int c = FILTER_TYPE_LENGTH;
                    for (int i = 0; i < _indexes.Length; i++)
                    {
                        _indexes[i] = c;
                        c += Stride + FILTER_TYPE_LENGTH;
                    }
                }

                return _indexes;
            }
        }

        public Accessor(ImageDataChunk imageDataChunk, Range range)
        {
            _iDC = imageDataChunk;
            _range = range;
        }

        public FilterType GetFilterType(int index) => (FilterType)Data[Indexes[index] - FILTER_TYPE_LENGTH];
        public void SetFilterType(int index, FilterType filterType) => Data[Indexes[index] - FILTER_TYPE_LENGTH] = (byte)filterType;

        protected Span<byte> GetRaster(int index)
        {
            var sI = Indexes[index];
            return Data.AsSpan()[sI..(sI + Stride)];
        }
    }

    public class DeepAccessor<T> : Accessor where T : unmanaged
    {
        readonly int _stride;
        byte[]? _zeroRaster;

        public override int Stride => _stride;

        protected new Raster this[int index]
        {
            get
            {
                var fT = GetFilterType(index);
                if ((byte)fT > 2) // 真上の参照が必要。
                {
                    if (index == 0)
                    {
                        return new(_zeroRaster ??= new byte[Stride], GetRaster(index), fT);
                    }
                    else
                    {
                        return new(GetRaster(index - 1), GetRaster(index), fT);
                    }
                }
                else
                {
                    return new(GetRaster(index), fT);
                }
            }
        }
        public unsafe T this[Point point]
        {
            get => this[point.Y][point.X];
            set
            {
                var raster = this[point.Y];
                raster[point.X] = value;
            }
        }

        public unsafe DeepAccessor(ImageDataChunk imageDataChunk, Range range) : base(imageDataChunk, range)
        {
            _stride = sizeof(T) * range.Width;
        }

        public ref struct Raster
        {
            FilterType _t;
            Span<byte> _p;
            Span<byte> _c;

            public T this[int index]
            {
                get
                {
                    switch (_t)
                    {
                    case FilterType.None:
                        return _c[index];
                    case FilterType.Sub:
                        break;
                    case FilterType.Up:
                        break;
                    case FilterType.Average:
                        break;
                    case FilterType.Paeth:
                        break;
                    default:
                        throw new UndefinedEnumerationValueException(enumeraionType: typeof(FilterType));
                    }
                }
                set
                {

                }
            }

            public Raster(Span<byte> current, FilterType filterType)
            {
                if (filterType == FilterType.Up || filterType == FilterType.Average || filterType == FilterType.Paeth) throw new ArgumentException("前区間を指定しないコンストラクタで`FilterType.Up`、`FilterType.Average`または`FilterType.Paeth`を選択することはできません。", nameof(filterType));

                _t = filterType;
                _p = default;
                _c = current;
            }
            public Raster(Span<byte> previous, Span<byte> current, FilterType filterType)
            {
                _t = filterType;
                _p = previous;
                _c = current;
            }
        }
    }

    public class ShallowAccessor : Accessor
    {
        readonly int _stride;

        public override int Stride => _stride;

        public ShallowAccessor(ImageDataChunk imageDataChunk, Range range, int depth) : base(imageDataChunk, range)
        {
            _stride = (range.Width * depth + 7) / 8; // 八で割って切り上げ。
            
        }
    }

    public enum FilterType : byte
    {
        None, Sub, Up, Average, Paeth
    }
}

public static class NoteExtensions
{
    public static Task Insert(this INote @this, PortableNetworkGraphic portableNetworkGraphic) => PortableNetworkGraphic.Insert(to: @this, portableNetworkGraphic);

    public static Task Insert(this INote @this, ImageDataChunk imageDataChunk)
    {

    }
    public static Task Remove(this INote @this, out ImageDataChunk imageDataChunk)
    {
        imageDataChunk = new();
    }

    public static async Task Insert(this INote @this, ImageDataChunk imageDataChunk)
    {
        var sP = @this.Pointer;
        await @this.Insert(asciiString: (ASCIIString)"IDAT");
        await @this.Insert(memory: (Memory<byte>)imageDataChunk.Data);
        var eP = @this.Pointer;
        @this.Pointer = sP;
        @this.Insert(eP);
    }
}
