// 令和弐年大暑確認済。

namespace Nonno.Assets;

public enum ComparisonResult
{
    Less = -1,
    Equal = 0,
    Greater = 1,
}

public enum BasicColor : byte
{
    None = 0b0000_0000,

    Red = 0b0000_0001,
    Green = 0b0000_0010,
    Blue = 0b0000_0100,
    Thin = 0b0000_1000,

    Yellow = Red | Green,
    Cyan = Green | Blue,
    Magenta = Blue | Red,

    ThinRed = Red | Thin,
    ThinGreen = Green | Thin,
    ThinBlue = Blue | Thin,
    ThinYellow = Yellow | Thin,
    ThinCyan = Cyan | Thin,
    ThinMagenta = Magenta | Thin,
}
