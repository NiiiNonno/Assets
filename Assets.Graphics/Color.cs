using System.Runtime.InteropServices;

namespace Nonno.Assets.Graphics;

public readonly struct Color
{
    const int MASK = 0xFF;

    readonly int _value;

    public int Alpha => _value >> 24 & MASK;
    public int Red => _value >> 16 & MASK;
    public int Green => _value >> 8 & MASK;
    public int Blue => _value & MASK;

    public Color(int red, int green, int blue, int alpha = 0)
    {
        _value = blue | green << 8 | red << 16 | alpha << 24;
    }
    public Color(int value)
    {
        _value = value;
    }

    public static implicit operator int(Color color) => color._value;
}

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 24)]
public readonly record struct RGBColor24
{
    readonly byte red;
    readonly byte green;
    readonly byte blue;

    public RGBColor24(byte red, byte green, byte blue)
    {
        this.red = red;
        this.green = green;
        this.blue = blue;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 48)]
public readonly record struct RGBColor48
{
    readonly ushort red;
    readonly ushort green;
    readonly ushort blue;

    public RGBColor48(ushort red, ushort green, ushort blue)
    {
        this.red = red;
        this.green = green;
        this.blue = blue;
    }
}
