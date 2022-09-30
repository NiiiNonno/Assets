namespace Nonno.Assets.Graphics;

/*
 * いつ作ったのか不明。
 * 画像処理関連ではそこそこ有用だとも思うけれど当面の間は役目がない。
 */

public interface IVisualizer
{
    VisualizeType Type { get; }
    Task Write(Stream to);

    public enum VisualizeType
    {
        Png,
        Bmp,
        Tiff,
        Jpeg,
        Gif,
    }
}
