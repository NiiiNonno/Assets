namespace Nonno.Assets.Graphics;

[Serializable]
[Flags]
public enum Direction
{
    // 横向き。x軸。第一変数自由。
    Horizontal = 0b0001,
    // 縦向き。y軸。第二変数自由。
    Vertical = 0b0010,

}

[Serializable]
[Flags]
public enum Position
{
    /// <summary>
    /// 中央。
    /// </summary>
    Center = 0b0000,
    /// <summary>
    /// 左。
    /// </summary>
    Left = 0b0010,
    /// <summary>
    /// 右。
    /// </summary>
    Right = 0b0001,
    /// <summary>
    /// 横軸を指定しない。
    /// </summary>
    HorizontalFree = Left | Center | Right,
    /// <summary>
    /// 上。
    /// </summary>
    Top = 0b1000,
    /// <summary>
    /// 下。
    /// </summary>
    Bottom = 0b0100,
    /// <summary>
    /// 縦軸を指定しない。
    /// </summary>
    VerticalFree = Top | Center | Bottom,
}
