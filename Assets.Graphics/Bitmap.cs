using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
#if USE_DOUBLE
using Dec = System.Double
#else
using Dec = System.Single;
#endif

namespace Nonno.Assets.Graphics;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[point.X, point.Y];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this[point.X, point.Y] = value;
    }
    public Color this[int x, int y]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _pixels[y][x];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    public unsafe void Save(Stream to)
    {
        to.Write(Head, 0, HEADER_SIZE);

        int stride = Stride;
        for (int i = _pixels.Length - 1; i >= 0; i--) // bitmapは下から上へ走査線を移動させるらしい。
        {
            fixed (Color* p = _pixels[i]) // 下の'GetRasterで簡略化もできるけど、'_pixels[i]'に働く最適化のために敢えて展開。'
            {
                to.Write(new Span<byte>(p, stride));
            }
        }
    }

    public unsafe Span<T> GetRaster<T>(int i, int length)
    {
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