using MI = System.Runtime.CompilerServices.MethodImplAttribute;
using MIO = System.Runtime.CompilerServices.MethodImplOptions;

namespace Nonno.Assets.Graphics;

public class Bitmap<T> where T : unmanaged
{
    internal const int FILE_HEADER_SIZE = 14;
    internal const int INFORMATION_HEADER_SIZE = 40;
    protected internal const int HEADER_SIZE = FILE_HEADER_SIZE + INFORMATION_HEADER_SIZE;

    Range _range;
    T[][] _pixels;

    public uint Width => (uint)_range.Width;
    public uint Height => (uint)_range.Height;
    public unsafe int Stride => _range.Width * sizeof(T);
    public unsafe uint DataSize => Width * Height * (uint)sizeof(T);
    public uint FileSize => DataSize + HEADER_SIZE;
    public unsafe Range Range
    {
        get => _range;
        set
        {
            if (_range.Width * sizeof(T) % 4 != 0) throw new ArgumentException("仕様上、`Stride`が四の倍数とならなければなりません。", nameof(Range));

            switch (_range.Height != value.Height, _range.Width != value.Width)
            {
            // 高さが異なる場合、中の配列の一部を流用。
            case (true, false):
                {
                    var old = _pixels;
                    _pixels = new T[value.Height][];
                    old.CopyTo(_pixels, 0);
                    // 転写しただけでは高さが高くなった時に中身のない要素ができてしまうから埋める。
                    for (int i = old.Length; i < _pixels.Length; i++) _pixels[i] = new T[value.Width];

                    _range = value;

                    RecalculateHead();
                    break;
                }
            // 幅が異なる場合、中の配列を変更し、一部を転写。
            case (false, true):
                {
                    for (int i = 0; i < _pixels.Length; i++)
                    {
                        var old = _pixels[i];
                        _pixels[i] = new T[value.Width];
                        old.CopyTo(_pixels, 0);
                    }

                    _range = value;

                    RecalculateHead();
                    break;
                }
            case (true, true):
                {
                    var old = _pixels;
                    _pixels = new T[value.Height][];
                    for (int i = 0; i < _pixels.Length; i++)
                    {
                        _pixels[i] = new T[value.Width];
                        old[i].CopyTo(_pixels[i], 0);
                    }

                    _range = value;

                    RecalculateHead();
                    break;
                }
            }
        }
    }
    public uint Resolution { get; set; }
    protected byte[] Head { get; }
    public T this[Point point]
    {
        get => this[point.X, point.Y];
        set => this[point.X, point.Y] = value;
    }
    public T this[int x, int y]
    {
        get => _pixels[y][x];
        set => _pixels[y][x] = value;
    }

    protected unsafe Bitmap(uint colorPaletteLength = 0)
    {
        var head = new byte[HEADER_SIZE];
        {
            Span<byte> span = head;
            span[0] = 0x42; span[1] = 0x4d;
            ((uint)HEADER_SIZE).Copy(span[10..14], true);
            (INFORMATION_HEADER_SIZE + colorPaletteLength).Copy(span[14..18], true);
            ((ushort)1).Copy(span[26..28], true);
            ((ushort)(sizeof(T) * 8u)).Copy(span[28..30], true);
            (colorPaletteLength * 4u).Copy(span[46..50], true);
        }

        _pixels = Array.Empty<T[]>();

        Head = head;

        RecalculateHead();
    }

    /// <summary>
    /// 流れにビットマップ形式で保存します。
    /// </summary>
    /// <param name="to">
    /// 保存する先の流れ。
    /// </param>
    public virtual unsafe void Save(Stream to)
    {
        to.Write(Head, 0, HEADER_SIZE);

        int stride = Stride;
        for (int i = _pixels.Length - 1; i >= 0; i--) // bitmapは下から上へ走査線を移動させるらしい。
            to.Write(GetRaster<byte>(i, stride));
    }

    /// <summary>
    /// 緯を指定した型の区間として取得します。
    /// </summary>
    /// <typeparam name="TItem">
    /// 区間の要素の型。
    /// </typeparam>
    /// <param name="i">
    /// 緯の位置。
    /// </param>
    /// <param name="length">
    /// 区間の長さ。
    /// </param>
    /// <returns>
    /// 指定した長さの緯。
    /// </returns>
    /// <exception cref="ArgumentException">
    /// 緯の長さを超えた区間を取得しようとしました。
    /// </exception>
    [MI(MIO.AggressiveInlining)]
    public unsafe Span<TItem> GetRaster<TItem>(int i, int length) where TItem : unmanaged
    {
        if (sizeof(TItem) * length > Stride) throw new ArgumentException("緯の長さを超えた区間を取得しようとしました。", nameof(length));

        fixed (T* p = _pixels[i])
        {
            return new Span<TItem>(p, length);
        }
    }

    /// <summary>
    /// 内部数據を直接取得します。
    /// </summary>
    /// <returns>
    /// 内部数據。
    /// <para>
    /// 安易に変更を加えないでください。
    /// </para>
    /// </returns>
    internal T[][] AccessData() => _pixels;

    protected void RecalculateHead()
    {
        Span<byte> span = Head;
        // 0..2:ファイルタイプ。コンストラクタ内で代入済み。
        FileSize.Copy(span[2..6], true);
        // 6..10:予約領域。デフォルト。
        // 10..14:ファイルヘッダの先頭アドレスからビットマップデータの先頭アドレスまでのオフセット。コンストラクタ内で代入済み。
        // 14..18:ヘッダサイズ。BITMAPINFOHEADERを採用し、40で固定。コンストラクタ内で代入済み。
        Width.Copy(span[18..22], true);
        Height.Copy(span[22..26], true);
        // 26..28:プレーン数。常に1。コンストラクタ内で代入済み。
        // 28..30:iピクセル当たりのビット数。コンストラクタ内で代入済み。
        // 30..34:圧縮形式。無圧縮を採用し、0で固定。
        DataSize.Copy(span[34..38], true);
        Resolution.Copy(span[38..42], true);
        Resolution.Copy(span[42..46], true);
        // 46..50:使用する色数。コンストラクタ内で代入済み。
        // 50..54:使用する重要色数。デフォルト。
    }
}

public class ColorBitmap : Bitmap<Color>
{
    public ColorBitmap() : base(colorPaletteLength: 0) { }
}

public class PaletteBitmap : Bitmap<byte>
{
    public BitmapType Type { get; }
    public Color[] Palette { get; }

    public override void Save(Stream to)
    {
        to.Write(Head, 0, HEADER_SIZE);

        switch (Type)
        {
        case BitmapType.OS2:
            {
                foreach (var color in Palette)
                {
                    to.WriteByte((byte)color.Blue);
                    to.WriteByte((byte)color.Green);
                    to.WriteByte((byte)color.Red);
                }

                break;
            }
        case BitmapType.Windows:
            {
                foreach (var color in Palette)
                {
                    to.WriteByte((byte)color.Blue);
                    to.WriteByte((byte)color.Green);
                    to.WriteByte((byte)color.Red);
                    to.WriteByte((byte)color.Alpha);
                }

                break;
            }
        }

        int stride = Stride;
        for (int i = (int)Height - 1; i >= 0; i--) // bitmapは下から上へ走査線を移動させるらしい。
            to.Write(GetRaster<byte>(i, stride));
    }

    public PaletteBitmap(uint colorCount, BitmapType type) : base(colorPaletteLength: colorCount * (type switch 
    { 
        BitmapType.OS2 => 3u, 
        BitmapType.Windows => 4u,
        _ => throw new ArgumentException("未知のビットマップ形式です。", nameof(type))
    })) 
    {
        Palette = new Color[colorCount];
        Type = type;
    }
}

public enum BitmapType
{
    OS2,
    Windows,
}

[Obsolete("現在は`ColorBitmap`の使用が推奨されています。")]
public class Bitmap
{
    const int FILE_HEADER_SIZE = 14;
    const int INFORMATION_HEADER_SIZE = 40;
    const int HEADER_SIZE = FILE_HEADER_SIZE + INFORMATION_HEADER_SIZE;
    Range _range;
    Color[][] _pixels;
    public uint Width => (uint)_range.Width;
    public int Stride => _range.Width * 32;
    public uint Height => (uint)_range.Height;
    public uint DataSize => Width * Height * 32;
    public uint FileSize => DataSize + HEADER_SIZE;
    public Range Range
    {
        get => _range;
        set
        {
            if (_range != value)
            {
                _pixels = new Color[value.Height][];
                for (int i = 0; i < _pixels.Length; i++) _pixels[i] = new Color[value.Width];
                _range = value;
                RecalculateHead();
            }
        }
    }
    public uint Resolution { get; set; }
    protected byte[] Head { get; }
    public Color this[Point point]
    {
        get => this[point.X, point.Y];
        set => this[point.X, point.Y] = value;
    }
    /// <summary>
    /// 指定した位置の色を取得します。
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Color this[int x, int y]
    {
        get => _pixels[y][x];
        set => _pixels[y][x] = value;
    }

    public Bitmap()
    {
        var head = new byte[HEADER_SIZE];
        {
            Span<byte> span = head;
            span[0] = 0x42; span[1] = 0x4d;
            ((uint)HEADER_SIZE).Copy(span[10..14], true);
            ((uint)INFORMATION_HEADER_SIZE).Copy(span[14..18], true);
            ((ushort)1).Copy(span[22..24], true);
            ((ushort)32).Copy(span[24..26], true);
        }
        _pixels = Array.Empty<Color[]>();
        Head = head;
        RecalculateHead();
    }

    /// <summary>
    /// 流れにビットマップ形式で保存します。
    /// </summary>
    /// <param name="to"></param>
    public unsafe void Save(Stream to)
    {
        to.Write(Head, 0, HEADER_SIZE);

        int stride = Stride;
        for (int i = _pixels.Length - 1; i >= 0; i--) // bitmapは下から上へ走査線を移動させるらしい。
            to.Write(GetRaster<byte>(i, stride));
    }

    /// <summary>
    /// 緯を指定した型の区間として取得します。
    /// </summary>
    /// <typeparam name="T">
    /// 区間の要素の型。
    /// </typeparam>
    /// <param name="i">
    /// 緯の位置。
    /// </param>
    /// <param name="length">
    /// 区間の長さ。
    /// </param>
    /// <returns>
    /// 指定した長さの緯。
    /// </returns>
    /// <exception cref="ArgumentException">
    /// 緯の長さを超えた区間を取得しようとしました。
    /// </exception>
    [MI(MIO.AggressiveInlining)]
    public unsafe Span<T> GetRaster<T>(int i, int length) where T : unmanaged
    {
        if (sizeof(T) * length > Stride) throw new ArgumentException("緯の長さを超えた区間を取得しようとしました。", nameof(length));

        fixed (Color* p = _pixels[i])
        {
            return new Span<T>(p, length);
        }
    }
    public Color[][] AccessData() => _pixels;
    void RecalculateHead()
    {
        Span<byte> span = Head;
        // 0..2:ファイルタイプ。コンストラクタ内で代入済み。
        FileSize.Copy(span[2..6], true);
        // 6..10:予約領域。デフォルト。
        // 10..14:ファイルヘッダの先頭アドレスからビットマップデータの先頭アドレスまでのオフセット。コンストラクタ内で代入済み。
        // 14..18:ヘッダサイズ。BITMAPINFOHEADERを採用し、40で固定。コンストラクタ内で代入済み。
        Width.Copy(span[18..22], true);
        Height.Copy(span[22..26], true);
        // 26..28:プレーン数。常に1。コンストラクタ内で代入済み。
        // 28..30:iピクセル当たりのビット数。コンストラクタ内で代入済み。
        // 30..34:圧縮形式。無圧縮を採用し、0で固定。
        DataSize.Copy(span[34..38], true);
        Resolution.Copy(span[38..42], true);
        Resolution.Copy(span[42..46], true);
        // 46..50:使用する色数。デフォルト。
        // 50..54:使用する重要色数。デフォルト。
    }
}