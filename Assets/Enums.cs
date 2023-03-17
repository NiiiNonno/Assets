// 令和弐年大暑確認済。

using System.Collections.Immutable;

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
    Black = 0b0000_1000,

    Yellow = Red | Green,
    Cyan = Green | Blue,
    Magenta = Blue | Red,
    White = Red | Green | Blue,

    Thin = Black,
    ThinRed = Red | Thin,
    ThinGreen = Green | Thin,
    ThinBlue = Blue | Thin,
    ThinYellow = Yellow | Thin,
    ThinCyan = Cyan | Thin,
    ThinMagenta = Magenta | Thin,

    Gray = White | Black,

    /*
     * none
     * red
     * green
     * yellow
     * blue
     * magenta
     * cyan
     * white
     * black thin
     * ...
     */
}

public readonly record struct ColoredCharacter(char Character, BasicColor ForegroundColor = BasicColor.None, BasicColor BackgroundColor = BasicColor.None)
{
    public static implicit operator ColoredCharacter(char value) => new(value);
    public static explicit operator char(ColoredCharacter value) => value.Character;
}
