// 令和弐年大暑確認済。

namespace Nonno.Assets;

/*
 * Noteに置き換えれないか検討してみるべし。
 */

public interface IOriginal<T>
{
    /// <summary>
    /// 原本を範囲に複製します。原本の長さが範囲の長さに満たなかった場合は、範囲の原本の長さを超えた部分は変更されません。
    /// </summary>
    /// <param name="to">
    /// 複製先の範囲。
    /// </param>
    void Copy(Span<T> to) => Copy(to, 0);
    /// <summary>
    /// 原本を範囲に複製します。原本の長さが範囲の長さに満たなかった場合は、範囲の原本の長さを超えた部分は変更されません。
    /// </summary>
    /// <param name="to">
    /// 複製先の範囲。
    /// </param>
    /// <param name="index">
    /// 原本の複製開始位置。
    /// </param>
    void Copy(Span<T> to, int index);

    public static IOriginal<T> Empty { get; } = new EmptyOriginal();

    private readonly struct EmptyOriginal : IOriginal<T> { void IOriginal<T>.Copy(Span<T> buffer, int offset) { } }
}
