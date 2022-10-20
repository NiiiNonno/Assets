using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Nonno.Assets.Collections;
using Nonno.Assets.Scrolls;
using static System.Net.Mime.MediaTypeNames;
using static Nonno.Assets.Utils;
using PNG = Nonno.Assets.Graphics.PortableNetworkGraphic;
using IHBox = Nonno.Assets.Graphics.PortableNetworkGraphic.ImageHeaderBox;
using PBox = Nonno.Assets.Graphics.PortableNetworkGraphic.PaletteBox;
using DBox = Nonno.Assets.Scrolls.DataBox;
using System.IO.Compression;

namespace Nonno.Assets.Graphics;
public abstract class PortableNetworkGraphic : IDisposable
{
    public const ulong FILE_SIGNATURE = 0x89504E470D0A1A;
    public const uint MAGIC_NUMBER_FOR_CYCLIC_REDUNDANCY_CHECK = 0x04C11DB7;
    public static readonly HashTableTwoWayDictionary<NetworkStreamScroll.Type, TypeIdentifier> DICTIONARY = new()
    {
        { new((ASCIIString)"IHDR"), TypeIdentifier.Get(typeof(IHBox)) },
        { new((ASCIIString)"IEND"), TypeIdentifier.Get(typeof(EmptyBox)) },
        { new((ASCIIString)"PLTE"), TypeIdentifier.Get(typeof(PBox)) },
        { new((ASCIIString)"IDAT"), TypeIdentifier.Get(typeof(DBox)) },
        { new((ASCIIString)"tEXt"), TypeIdentifier.Get(typeof(StringBox)) },
        { new((ASCIIString)"bKGD"), TypeIdentifier.Get(typeof(BackgroundColorBox)) },
        { new((ASCIIString)"mINp"), TypeIdentifier.Get(typeof(MinimumPointBox)) }
    };

    readonly IHeap<IDataBox> _heap;
    IHBox _header;
    byte[]? _patch;
    byte[] _data;
    PBox? _palette;
    MinimumPointBox _minimumPoint;
    bool _isDisposed;
    Disposables _disposables;

    public IHeap<IDataBox> Heap => _heap;
    public Range Range
    {
        get => new((int)_header.width, (int)_header.height);
        set => _header = new((uint)value.Width, (uint)value.Height, _header.depth, _header.colorType, _header.compactionMethod, _header.filterMethod, _header.interlaceMethod);
    }
    public byte Depth => _header.depth;
    public ColorTypes ColorType => _header.colorType;
    public PBox? Palette => _palette;
    public Point MinimumPoint => _minimumPoint.Point;
    public Point MaximumPoint => MinimumPoint + new Point(Range);

    protected PortableNetworkGraphic(IHeap<IDataBox> heap)
    {
        _heap = heap;
        _header = default;
        _data = default!;
        _palette = null;
    }

    public async Task Init(IHBox iHBox)
    {
        var header = iHBox;
        int bits = header.depth;
        var palette = default(PBox);
        switch (header.colorType)
        {
        case ColorTypes.Colored:
            bits *= 3;
            break;
        case ColorTypes.HaveAlpha:
            bits *= 2;
            break;
        case ColorTypes.HaveAlpha | ColorTypes.Colored:
            bits *= 4;
            break;
        }
        if ((header.colorType & ColorTypes.Palette) != 0)
        {
            palette = await _heap.Get<PBox>();
            bits = header.depth;
        }

        var capacity = header.height * ((header.width * bits + 8 - 1) / 4 + 1);
        var data = new byte[capacity];
        switch (header.compactionMethod)
        {
        case CompactionMethod.Deflate:
            using (var deflateStream = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress))
            {
                DBox dBox;
                while (true)
                {
                    dBox = await _heap.Get<DBox>();
                    if (dBox.IsEmpty) break;
                    await deflateStream.WriteAsync(dBox.Data);
                }
            }
            break;
        }

        _header = header;
        _data = data;
        _palette = palette;
        _minimumPoint = await _heap.Get<MinimumPointBox>();
    }

    /// <summary>
    /// ヘッダの変更を反映し、末尾を明らかにしてヒープを完成させます。
    /// </summary>
    public async Task Close()
    {
        await _heap.Set(_header);
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
                Close().Wait();
                _disposables.Dispose();
            }

            _isDisposed = true;
        }
    }

    #region Statics

    public static async Task<PNG> Create(FileInfo fileInfo)
    {
        var stream = fileInfo.Create();
        WriteSignature(stream);
        stream.Position = 0;
        var r = await Instantiate(stream: stream);
        r._disposables += stream;
        return r;

        static void WriteSignature(Stream stream)
        {
            Span<byte> signature = stackalloc byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64BigEndian(signature, FILE_SIGNATURE);
            stream.Write(signature);
        }
    }
    public static async Task<PNG> Open(string path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
    {
        var stream = File.Open(path, mode, access, share);
        var r = await Instantiate(stream: stream);
        r._disposables += stream;
        return r;
    }
    public static async Task<PNG> Open(FileInfo fileInfo, FileMode mode = FileMode.Open, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
    {
        var stream = fileInfo.Open(mode, access, share);
        var r = await Instantiate(stream: stream);
        r._disposables += stream;
        return r;
    }
    public static Task<PNG> Instantiate(Stream stream) => Instantiate(new NetworkStreamScroll(stream, DICTIONARY) { MagicNumberForCyclicRecursiveCheck = MAGIC_NUMBER_FOR_CYCLIC_REDUNDANCY_CHECK });
    public static async Task<PNG> Instantiate(IScroll scroll)
    {
        CheckSignature(scroll);
        var boxList = await BoxHeap.Instantiate(scroll: scroll, trailerBoxTypeId: TypeIdentifier.Get<EmptyBox>());
        var r = await Instantiate(boxes: boxList);
        r._disposables += boxList;
        return r;

        static void CheckSignature(IScroll scroll)
        {
            Span<byte> signature = stackalloc byte[sizeof(ulong)];
            scroll.RemoveSync(span: signature);
            if (BinaryPrimitives.ReadUInt64BigEndian(signature) != FILE_SIGNATURE) throw new Exception("ストリーム署名が異なります。");
        }
    }
    public static async Task<PNG> Instantiate(IHeap<IDataBox> boxes)
    {
        var iHBox = await boxes.Get<IHBox>();
        PNG r;
        switch (iHBox)
        {
        case { colorType: ColorTypes.None, depth: 1 }: 
            r = new PortableNetworkGraphic_Monochrome1(boxes);
            break;
        default:
            throw new Exception("ヘッダ情報から適切な型を復元できませんでした。");
        }
        await r.Init(iHBox);
        return r;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct ImageHeaderBox : IDataBox
    {
        public readonly uint width;
        public readonly uint height;
        public readonly byte depth;
        public readonly ColorTypes colorType;
        public readonly CompactionMethod compactionMethod;
        public readonly FilterMethod filterMethod;
        public readonly InterlaceMethod interlaceMethod;

        public ImageHeaderBox(uint width, uint height, byte depth, ColorTypes colorType, CompactionMethod compactionMethod, FilterMethod filterMethod, InterlaceMethod interlaceMethod)
        {
            if (width is <= 0 or > int.MaxValue) throw new ArgumentException("値が有効な範囲外です。", nameof(width));
            if (height is <= 0 or > int.MaxValue) throw new ArgumentException("値が有効な範囲外です。", nameof(height));
            switch (colorType)
            {
            case ColorTypes.None:
                if (depth is not 1 and not 2 and not 4 and not 8 and not 16) throw new ArgumentException("値が有効な範囲外です。", nameof(depth));
                break;
            case ColorTypes.Colored | ColorTypes.Palette:
                if (depth is not 1 and not 2 and not 4 and not 8) throw new ArgumentException("値が有効な範囲外です。", nameof(depth));
                break;
            case ColorTypes.Colored:
            case ColorTypes.HaveAlpha:
            case ColorTypes.HaveAlpha | ColorTypes.Colored:
                if (depth is not 8 and not 16) throw new ArgumentException("値が有効な範囲外です。", nameof(depth));
                break;
            default:
                throw new UndefinedEnumerationValueException(paramName: nameof(colorType));
            }
            switch (compactionMethod)
            {
            case CompactionMethod.Deflate:
                break;
            default:
                throw new UndefinedEnumerationValueException(paramName: nameof(colorType));
            }
            switch (filterMethod)
            {
            case FilterMethod.Basic5:
                break;
            default:
                throw new UndefinedEnumerationValueException(paramName: nameof(colorType));
            }
            switch (interlaceMethod)
            {
            case InterlaceMethod.None:
            case InterlaceMethod.Adam7:
                break;
            default:
                throw new UndefinedEnumerationValueException(paramName: nameof(colorType));
            }

            this.width = width;
            this.height = height;
            this.depth = depth;
            this.colorType = colorType;
            this.compactionMethod = compactionMethod;
            this.filterMethod = filterMethod;
            this.interlaceMethod = interlaceMethod;
        }
    }

    public class PaletteBox : ArrayList<RGBColor24>, IDataBox
    {
        public PaletteBox() : base() { }
        public PaletteBox(params RGBColor24[] colorParams) : base(colorParams) { }
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

    public sealed class BackgroundColorBox : IDataBox
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

    #endregion
}

class PortableNetworkGraphic_Monochrome1 : PNG, IImage<Monochrome1>
{
    public Monochrome1 this[Point point] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    Monochrome1 IReadOnlyImage<Monochrome1>.this[Point point] => throw new NotImplementedException();

    public PortableNetworkGraphic_Monochrome1(IHeap<IDataBox> heap) : base(heap)
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

    }
    public void SetRaster(Span<Monochrome1> to, int index, int start, int end)
    {

    }
}

public readonly record struct MinimumPointBox(Point Point) : IDataBox;

public static class ScrollExtensions
{
    [IRMethod]
    public static Task Insert(this IScroll @this, PNG portableNetworkGraphic)
    {
        Span<byte> signature = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64BigEndian(signature, PNG.FILE_SIGNATURE);
        @this.InsertSync(signature);

        return Async();
        async Task Async()
        {
            foreach (var item in portableNetworkGraphic.Heap)
            {
                await @this.Insert(item);
            }
        }
    }
    [IRMethod]
    public static Task Remove(this IScroll @this, out PNG portableNetworkGraphic)
    {
        portableNetworkGraphic = PNG.Instantiate(scroll: @this).Result;
        return Task.CompletedTask;
    }

    [IRMethod]
    public static Task Insert(this IScroll @this, in IHBox pNG_imageHeaderBox) => @this.InsertStructureAsBox<IHBox, IHBox>(in pNG_imageHeaderBox);
    [IRMethod]
    public static Task Remove(this IScroll @this, out IHBox pNG_imageHeaderBox) => @this.RemoveStructureAsBox<IHBox, IHBox>(out pNG_imageHeaderBox);

    [IRMethod]
    public static Task Insert(this IScroll @this, in PBox pNG_paletteBox) => @this.InsertArrayAsBox<PBox, RGBColor24>(pNG_paletteBox.AsMemory());
    [IRMethod]
    public static Task Remove(this IScroll @this, out PBox pNG_paletteBox) 
    { 
        var r = @this.RemoveArrayAsBox<PBox, RGBColor24>(out var array);
        pNG_paletteBox = new(array);
        return r;
    }

    [IRMethod]
    public static Task Insert(this IScroll @this, PNG.BackgroundColorBox pNG_backgroundColorBox)
    {
        return Scrolls.Utils.InsertArrayAsBox<PNG.BackgroundColorBox, byte>(@this, pNG_backgroundColorBox.Data);
    }
    [IRMethod]
    public static Task Remove(this IScroll @this, out PNG.BackgroundColorBox pNG_backgroundColorBox)
    {
        pNG_backgroundColorBox = new();

        return Async(pNG_backgroundColorBox);
        async Task Async(PNG.BackgroundColorBox pNG_bCB)
        {
            await Scrolls.Utils.RemoveArrayAsBox<PNG.BackgroundColorBox, byte>(@this, out var data);
            pNG_bCB._data = data;
        }
    }

    [IRMethod]
    public static Task Insert(this IScroll @this, in MinimumPointBox minimumPointBox) => @this.InsertStructureAsBox<MinimumPointBox, MinimumPointBox>(in minimumPointBox);
    [IRMethod]
    public static Task Remove(this IScroll @this, out MinimumPointBox minimumPointBox) => @this.RemoveStructureAsBox<MinimumPointBox, MinimumPointBox>(out minimumPointBox);
}
