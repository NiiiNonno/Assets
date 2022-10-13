using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Nonno.Assets.Collections;
using Nonno.Assets.Notes;
using static System.Net.Mime.MediaTypeNames;
using static Nonno.Assets.Utils;
using PNG = Nonno.Assets.Graphics.PortableNetworkGraphic;
using IHBox = Nonno.Assets.Notes.LeafBox<Nonno.Assets.Graphics.PortableNetworkGraphic.ImageHeader>;
using PBox = Nonno.Assets.Notes.ArrayBox<Nonno.Assets.Graphics.RGBColor24>;
using DBox = Nonno.Assets.Notes.ArrayBox<byte>;
using System.IO.Compression;

namespace Nonno.Assets.Graphics;
public class PortableNetworkGraphic : IDisposable
{
    public const ulong FILE_SIGNATURE = 0x89504E470D0A1A;
    public const uint MAGIC_NUMBER_FOR_CYCLIC_REDUNDANCY_CHECK = 0x04C11DB7;
    public static readonly HashTableTwoWayDictionary<NetworkStreamNote.Type, TypeIdentifier> DICTIONARY = new() 
    {
        { new((ASCIIString)"IHDR"), new(typeof(IHBox)) },
        { new((ASCIIString)"IEND"), new(typeof(EmptyBox)) },
        { new((ASCIIString)"PLTE"), new(typeof(PBox)) },
        { new((ASCIIString)"IDAT"), new(typeof(DBox)) },
        { new((ASCIIString)"tEXt"), new(typeof(StringBox)) },
        { new((ASCIIString)"bKGD"), new(typeof(BackgroundColorBox)) },
    };

    readonly IHeap<IDataBox> _heap;
    ImageHeader _header;
    byte[]? _patch;
    byte[] _data;
    ArrayList<RGBColor24>? _palette;
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
    public ArrayList<RGBColor24>? Palette => _palette;
    public Raster this[int index]
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public PortableNetworkGraphic(IHeap<IDataBox> heap)
    {
        _heap = heap;
        _header = default;
        _data = default!;
        _palette = null;
    }
    public PortableNetworkGraphic(uint width, uint height, byte depth, ColorTypes colorType, CompactionMethod compactionMethod = CompactionMethod.Deflate, FilterMethod filterMethod = FilterMethod.Basic5, InterlaceMethod interlaceMethod = InterlaceMethod.None)
    {
        var header = new ImageHeader(width, height, depth, colorType, compactionMethod, filterMethod, interlaceMethod);
        
        _heap = new ArrayHeap<IDataBox>(new IHBox(header));
        _data = default!;
    }

    public async Task Init()
    {
        var header = (await _heap.Get<IHBox>()).structure;
        int bits = header.depth;
        var palette = default(ArrayList<RGBColor24>);
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
            var pBox = await _heap.Get<PBox>();
            palette = new(pBox.array);
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
                while (!(dBox = await _heap.Get<DBox>()).IsEmpty)
                {
                    await deflateStream.WriteAsync(dBox.array);
                }
            }
            break;
        }

        _header = header;
        _data = data;
        _palette = palette;
    }

    /// <summary>
    /// ヘッダの変更を反映し、末尾を明らかにしてヒープを完成させます。
    /// </summary>
    public async Task Close()
    {
#if DEBUG
        while (await _heap.Contains<EmptyBox>())
        {
            _ = await _heap.Remove<EmptyBox>();
        }
        await _heap.Add(new EmptyBox());
#endif
        throw new NotImplementedException();
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

    public static async ValueTask<PNG> Create(FileInfo fileInfo)
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
    public static async ValueTask<PNG> Open(string path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
    {
        var stream = File.Open(path, mode, access, share);
        var r = await Instantiate(stream: stream);
        r._disposables += stream;
        return r;
    }
    public static async ValueTask<PNG> Open(FileInfo fileInfo, FileMode mode = FileMode.Open, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
    {
        var stream = fileInfo.Open(mode, access, share);
        var r = await Instantiate(stream: stream);
        r._disposables += stream;
        return r;
    }
    public static ValueTask<PNG> Instantiate(Stream stream) => Instantiate(new NetworkStreamNote(stream, DICTIONARY) { MagicNumberForCyclicRecursiveCheck = MAGIC_NUMBER_FOR_CYCLIC_REDUNDANCY_CHECK });
    public static async ValueTask<PNG> Instantiate(INote scroll)
    {
        CheckSignature(scroll);
        var boxList = await BoxList.Instantiate(scroll: scroll);
        var r = await Instantiate(boxes: boxList);
        r._disposables += boxList;
        return r;

        static void CheckSignature(INote scroll)
        {
            Span<byte> signature = stackalloc byte[sizeof(ulong)];
            scroll.RemoveSync(span: signature);
            if (BinaryPrimitives.ReadUInt64BigEndian(signature) != FILE_SIGNATURE) throw new Exception("ストリーム署名が異なります。");
        }
    }
    public static async ValueTask<PNG> Instantiate(IHeap<IDataBox> boxes)
    {
        var r = new PNG(boxes);
        await r.Init();
        return r;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct ImageHeader
    {
        public readonly uint width;
        public readonly uint height;
        public readonly byte depth;
        public readonly ColorTypes colorType;
        public readonly CompactionMethod compactionMethod;
        public readonly FilterMethod filterMethod;
        public readonly InterlaceMethod interlaceMethod;

        public ImageHeader(uint width, uint height, byte depth, ColorTypes colorType, CompactionMethod compactionMethod, FilterMethod filterMethod, InterlaceMethod interlaceMethod)
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

    public ref struct Raster
    {
        readonly Span<byte> _p;
        readonly Span<byte> _c;

        public FilterMethod FilterMethod => (FilterMethod)_c[0];
        

        public Raster(Span<byte> previous, Span<byte> current)
        {
            _p = previous;
            _c = current;
        }
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
}

public static class NoteExtensions
{
    [IRMethod]
    public static Task Insert(this INote @this, PNG portableNetworkGraphic)
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
    public static Task Remove(this INote @this, out PNG portableNetworkGraphic)
    {
        Span<byte> signature = stackalloc byte[sizeof(ulong)];
        @this.RemoveSync(span: signature);
        if (BinaryPrimitives.ReadUInt64BigEndian(signature) != PNG.FILE_SIGNATURE) throw new Exception("署名が異なります。");

        var heap = new ArrayHeap<IDataBox>();
        portableNetworkGraphic = new(heap);

        return Async();
        async Task Async()
        {
            while (true)
            {
                await @this.Remove(dataBox: out var box);
                heap.Add(box);
                if (box is EmptyBox) return;
            }
        }
    }

    [IRMethod]
    public static Task Insert(this INote @this, PNG.BackgroundColorBox pNG_backgroundColorBox)
    {
        return Notes.Utils.InsertArrayAsBox<PNG.BackgroundColorBox, byte>(@this, pNG_backgroundColorBox.Data);
    }
    [IRMethod]
    public static Task Remove(this INote @this, out PNG.BackgroundColorBox pNG_backgroundColorBox)
    {
        pNG_backgroundColorBox = new();

        return Async(pNG_backgroundColorBox);
        async Task Async(PNG.BackgroundColorBox pNG_bCB)
        {
            await Notes.Utils.RemoveArrayAsBox<PNG.BackgroundColorBox, byte>(@this, out var data);
            pNG_bCB._data = data;
        }
    }
}
