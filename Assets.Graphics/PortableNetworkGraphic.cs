using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using Nonno.Assets;
using Nonno.Assets.Collections;
using Nonno.Assets.Scrolls;
using static System.Net.Mime.MediaTypeNames;
using static Nonno.Assets.Utils;
using PNG = Nonno.Assets.Graphics.PortableNetworkGraphic;
using IHBox = Nonno.Assets.Graphics.PortableNetworkGraphic.ImageHeaderBox;
using PBox = Nonno.Assets.Graphics.PortableNetworkGraphic.PaletteBox;
using DBox = Nonno.Assets.Scrolls.BytesDataBox;
using IList = System.Collections.Generic.IList<object>;
using System.IO.Compression;
using Nonno.Assets.Graphics;
using System.Runtime.InteropServices;
using TypeIdentifierAttribute = Nonno.Assets.TypeIdentifierAttribute;
using System.Collections;

namespace Nonno.Assets.Graphics;
public abstract class PortableNetworkGraphic : IDisposable
{
    public const ulong FILE_SIGNATURE = 0x89504E470D0A1A;
    public const uint MAGIC_NUMBER_FOR_CYCLIC_REDUNDANCY_CHECK = 0x04C11DB7;
    public static readonly TableConverter<NetworkStreamScroll.TypeName, TypeIdentifier> DICTIONARY = new()
    {
        ( new((ASCIIString)"IHDR"), new TypeIdentifier(typeof(IHBox).GUID) ),
        ( new((ASCIIString)"IEND"), new TypeIdentifier(typeof(EmptyBox).GUID) ),
        ( new((ASCIIString)"PLTE"), new TypeIdentifier(typeof(PBox).GUID) ),
        ( new((ASCIIString)"IDAT"), new TypeIdentifier(typeof(DBox).GUID) ),
        ( new((ASCIIString)"tEXt"), new TypeIdentifier(typeof(StringBox).GUID) ),
        ( new((ASCIIString)"bKGD"), new TypeIdentifier(typeof(BackgroundColorBox).GUID) ),
        ( new((ASCIIString)"mINp"), new TypeIdentifier(typeof(MinimumPointBox).GUID) )
    };

    readonly IList _boxes;
    IHBox _header;
    byte[]? _patch;
    byte[] _data;
    PBox? _palette;
    MinimumPointBox _minimumPoint;
    bool _isDisposed;
    Disposables _disposables;

    public IList Boxes => _boxes;
    public Range Range
    {
        get => new((int)_header.Width, (int)_header.Height);
        set
        {
            _header.Width = (uint)value.Width;
            _header.Height = (uint)value.Height;
            RecalculateDataSize();
        }
    }
    public int DataSize
    {
        get => Range.Height * ((Range.Width * Bits + 8 - 1) / 4 + 1);
    }
    public int Bits
    {
        get
        {
            return Depth * (Palette is not null ? 1 : ColorType switch 
            {
                ColorTypes.Colored => 3,
                ColorTypes.HaveAlpha => 2,
                ColorTypes.HaveAlpha | ColorTypes.Colored => 4,
                _ => 1
            });
        }
    }
    public byte Depth
    {
        get => _header.Depth;
        set
        {
            _header.Depth = value;
        }
    }
    public ColorTypes ColorType => _header.ColorType;
    public PBox? Palette => _palette;
    public Point MinimumPoint => _minimumPoint.Point;
    public Point MaximumPoint => MinimumPoint + new Point(Range);

    protected PortableNetworkGraphic(IList heap)
    {
        _boxes = heap;
        _header = default;
        _data = Array.Empty<byte>();
        _palette = null;
    }

    public void Init(uint width, uint height, byte depth, ColorTypes colorType, CompactionMethod compactionMethod, FilterMethod filterMethod, InterlaceMethod interlaceMethod)
    {
        _header = new IHBox() 
        { 
            Width = width, 
            Height = height,
            ColorType = colorType,
            Depth = depth,
            CompactionMethod = compactionMethod,
            FilterMethod = filterMethod,
            InterlaceMethod = interlaceMethod
        };
        _palette = (colorType & ColorTypes.Palette) != 0 ? new PaletteBox() : null;

        RecalculateDataSize();
    }

    public void Load()
    {
        if (_boxes[0] is not IHBox header) throw new FormatException();
        else _header = header;

        if ((header.ColorType & ColorTypes.Palette) != 0)
        if (_boxes[1] is not PaletteBox palette) throw new FormatException();
        else _palette = palette;

        RecalculateDataSize();
        switch (header.CompactionMethod)
        {
        case CompactionMethod.Deflate:
            using (var deflateStream = new DeflateStream(new MemoryStream(_data), CompressionMode.Decompress))
            {
                foreach (var dBox in _boxes.OfType<DBox>())
                {
                    if (dBox.IsEmpty) break;
                    deflateStream.Write(dBox.Data);
                }
            }
            break;
        }
    }

    public void Save()
    {
        _boxes[0] = _header;
        if (_palette is not null) _boxes[1] = _palette;
    }

    void RecalculateDataSize()
    {
        _data = new byte[DataSize];
    }

    /// <summary>
    /// ヘッダの変更を反映し、末尾を明らかにしてヒープを完成させます。
    /// </summary>
    public void Close()
    {
        _boxes.Insert(1, _header);
    }

    public void Dispose()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                Close();
                _disposables.Dispose();
            }

            _isDisposed = true;
        }
    }

    #region Statics

    // public static PNG Create(FileInfo fileInfo)
    // {
    //     var stream = fileInfo.Create();
    //     WriteSignature(stream);
    //     stream.Position = 0;
    //     var r = await Instantiate(stream: stream);
    //     r._disposables += stream;
    //     return r;

    //     static void WriteSignature(Stream stream)
    //     {
    //         Span<byte> signature = stackalloc byte[sizeof(ulong)];
    //         BinaryPrimitives.WriteUInt64BigEndian(signature, FILE_SIGNATURE);
    //         stream.Write(signature);
    //     }
    // }
    public static PNG Open(string path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
    {
        var stream = File.Open(path, mode, access, share);
        var r = Load(stream: stream);
        r._disposables += stream;
        return r;
    }
    public static PNG Open(FileInfo fileInfo, FileMode mode = FileMode.Open, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
    {
        var stream = fileInfo.Open(mode, access, share);
        var r = Load(stream: stream);
        r._disposables += stream;
        return r;
    }
    public static PNG Load(Stream stream) 
    {
        var scroll =new NetworkStreamScroll(stream, DICTIONARY) { MagicNumberForCyclicRecursiveCheck = MAGIC_NUMBER_FOR_CYCLIC_REDUNDANCY_CHECK };
        scroll.Remove(portableNetworkGraphic: out var r);
        return r;
    }

    public static PNG Instantiate(Boxes boxes)
    {
        var iHBox = boxes.OfType<IHBox>().Single();
        PNG r;
        switch (iHBox)
        {
        case { ColorType: ColorTypes.None, Depth: 1 }: 
            r = new PortableNetworkGraphic_Monochrome1(boxes);
            break;
        default:
            throw new Exception("ヘッダ情報から適切な型を復元できませんでした。");
        }
        r.Load();
        return r;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [DataBox]
    public struct ImageHeaderBox
    {
        public uint _width;
        public uint _height;
        public byte _depth;
        public ColorTypes _colorType;
        public CompactionMethod _compactionMethod;
        public FilterMethod _filterMethod;
        public InterlaceMethod _interlaceMethod;

        public required uint Width
        {
            get => _width;
            set
            {
                if (value is <= 0 or > int.MaxValue) throw new ArgumentException("値が有効な範囲外です。", nameof(value));
                _width = value;
            }
        }
        public required uint Height
        {
            get => _height;
            set
            {
                if (value is <= 0 or > int.MaxValue) throw new ArgumentException("値が有効な範囲外です。", nameof(value));
                _height = value;
            }
        }
        public required byte Depth
        {
            get => _depth;
            set
            {
                switch (_colorType)
                {
                    case ColorTypes.None:
                        if (value is not 1 and not 2 and not 4 and not 8 and not 16) throw new ArgumentException("値が有効な範囲外です。", nameof(value));
                        break;
                    case ColorTypes.Colored | ColorTypes.Palette:
                        if (value is not 1 and not 2 and not 4 and not 8) throw new ArgumentException("値が有効な範囲外です。", nameof(value));
                        break;
                    case ColorTypes.Colored:
                    case ColorTypes.HaveAlpha:
                    case ColorTypes.HaveAlpha | ColorTypes.Colored:
                        if (value is not 8 and not 16) throw new ArgumentException("値が有効な範囲外です。", nameof(value));
                        break;
                }
                _depth = value;
            }
        }
        public required ColorTypes ColorType
        {
            get => _colorType;
            set
            {
                switch (_colorType)
                {
                    case ColorTypes.None:
                    case ColorTypes.Colored | ColorTypes.Palette:
                    case ColorTypes.Colored:
                    case ColorTypes.HaveAlpha:
                    case ColorTypes.HaveAlpha | ColorTypes.Colored:
                        break;
                    default:
                        throw new UndefinedEnumerationValueException(paramName: nameof(value));
                }
                _colorType = value;
            }
        }
        public required CompactionMethod CompactionMethod
        {
            get => _compactionMethod;
            set
            {
                switch (value)
                {
                    case CompactionMethod.Deflate:
                        break;
                    default:
                        throw new UndefinedEnumerationValueException(paramName: nameof(value));
                }
                _compactionMethod = value;
            }
        }
        public required FilterMethod FilterMethod
        {
            get => _filterMethod;
            set
            {
                switch(value)
                {
                    case FilterMethod.Basic5:
                        break;
                    default:
                        throw new UndefinedEnumerationValueException(paramName: nameof(value));
                }
                _filterMethod = value;
            }
        }
        public required InterlaceMethod InterlaceMethod
        {
            get => _interlaceMethod;
            set
            {
                switch (value)
                {
                    case InterlaceMethod.None:
                    case InterlaceMethod.Adam7:
                        break;
                    default:
                        throw new UndefinedEnumerationValueException(paramName: nameof(value));
                }
                _interlaceMethod = value;
            }
        }
    }

    [DataBox]
    public class PaletteBox : ArrayList<RGBColor24>
    {
        public PaletteBox() : base() { }
        public PaletteBox(params RGBColor24[] colorParams) : base(colorParams) { }
    }

    [DataBox]
    public sealed class BackgroundColorBox
    {
        internal byte[] _data;

        public byte[] Data => _data;
        public byte PaletteNumber => _data[0];
        public ushort GrayLevel => BitConverter.ToUInt16(_data, 0);
        public RGBColor48 Color => new(BitConverter.ToUInt16(_data, 0), BitConverter.ToUInt16(_data, 2), BitConverter.ToUInt16(_data, 4));

        public BackgroundColorBox(byte[] data)
        {
            _data = data;
        }
        internal BackgroundColorBox()
        {
            _data = null!;
        }
    }

    [Flags]
    public enum ColorTypes : byte
    {
        None = 0, Palette = 1, Colored = 2, HaveAlpha = 4
    }

    public enum CompactionMethod : byte
    {
        Deflate,
    }

    public enum FilterMethod : byte
    {
        Basic5,
    }

    public enum InterlaceMethod : byte
    {
        None,
        Adam7,
    }

    #endregion
}

class PortableNetworkGraphic_Monochrome1 : PNG, IImage<Monochrome1>
{
    public Monochrome1 this[Point point] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    Monochrome1 IReadOnlyImage<Monochrome1>.this[Point point] => throw new NotImplementedException();

    public PortableNetworkGraphic_Monochrome1(Boxes boxes) : base(boxes)
    {

    }

    public void Get(Span<Monochrome1> to, Point startPoint, Point endPoint)
    {
        for (int y = startPoint.Y; y < endPoint.Y; y++)
        {

        }
    }
    public Task GetAsync(Memory<Monochrome1> to, Point startPoint, Point endPoint)
    {
        Get(to.Span, startPoint, endPoint);
        return Task.CompletedTask;
    }
    public void Set(ReadOnlySpan<Monochrome1> from, Point startPoint, Point endPoint)
    {
        if (endPoint.Y - startPoint.Y == 1)
        {

        }
        else
        {

        }
    }
    public Task SetAsync(ReadOnlyMemory<Monochrome1> from, Point startPoint, Point endPoint)
    {
        Set(from.Span, startPoint, endPoint);
        return Task.CompletedTask;
    }

    public void GetRaster(Span<Monochrome1> to, int index, int start, int end)
    {
        throw new NotImplementedException();
        // TODO: ラスターの取得。
    }
    public void SetRaster(Span<Monochrome1> to, int index, int start, int end)
    {
        throw new NotImplementedException();
        // TODO: ラスターの設定。
    }
}

[DataBox]
public readonly record struct MinimumPointBox(Point Point);

public static partial class ScrollExtensions
{
    [IRMethod]
    public static void Insert(this IScroll @this, PNG portableNetworkGraphic)
    {
        Span<byte> signature = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64BigEndian(signature, PNG.FILE_SIGNATURE);
        @this.Insert(signature);
        foreach (var item in portableNetworkGraphic.Boxes)
        {
            @this.Insert(dataBox: item);
        }
    }
    [IRMethod]
    public static void Remove(this IScroll @this, out PNG portableNetworkGraphic)
    {
        CheckSignature(@this);
        var boxes = new Boxes(@this);
        portableNetworkGraphic = PNG.Instantiate(boxes);

        static void CheckSignature(IScroll scroll)
        {
            Span<byte> signature = stackalloc byte[sizeof(ulong)];
            scroll.Remove(span: signature);
            if (BinaryPrimitives.ReadUInt64BigEndian(signature) != PNG.FILE_SIGNATURE) throw new Exception("ストリーム署名が異なります。");
        }
    }

    [IRMethod]
    public static void Insert(this IScroll @this, ref IHBox pNG_imageHeaderBox) => @this.InsertStructureAsBox<IHBox, IHBox>(ref pNG_imageHeaderBox);
    [IRMethod]
    public static void Remove(this IScroll @this, out IHBox pNG_imageHeaderBox) => @this.RemoveStructureAsBox<IHBox, IHBox>(out pNG_imageHeaderBox);

    [IRMethod]
    public static void Insert(this IScroll @this, in PBox pNG_paletteBox) => @this.InsertArrayAsBox<PBox, RGBColor24>(pNG_paletteBox.UnsafeAsSpan());
    [IRMethod]
    public static void Remove(this IScroll @this, out PBox pNG_paletteBox) 
    { 
        @this.RemoveArrayAsBox<PBox, RGBColor24>(out var array);
        pNG_paletteBox = new(array.ToArray());
    }

    [IRMethod]
    public static void Insert(this IScroll @this, PNG.BackgroundColorBox pNG_backgroundColorBox)
    {
        Scrolls.Utils.InsertArrayAsBox<PNG.BackgroundColorBox, byte>(@this, pNG_backgroundColorBox.Data);
    }
    [IRMethod]
    public static void Remove(this IScroll @this, out PNG.BackgroundColorBox pNG_backgroundColorBox)
    {
        pNG_backgroundColorBox = new();
        Scrolls.Utils.RemoveArrayAsBox<PNG.BackgroundColorBox, byte>(@this, out var data);
        pNG_backgroundColorBox._data = data.ToArray();
    }

    [IRMethod]
    public static void Insert(this IScroll @this, ref MinimumPointBox minimumPointBox) => @this.InsertStructureAsBox<MinimumPointBox, MinimumPointBox>(ref minimumPointBox);
    [IRMethod]
    public static void Remove(this IScroll @this, out MinimumPointBox minimumPointBox) => @this.RemoveStructureAsBox<MinimumPointBox, MinimumPointBox>(out minimumPointBox);
}
