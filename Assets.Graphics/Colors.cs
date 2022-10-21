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

[ColorStruct(
    LayorType.Black, double.NaN, LayorValueType.Bit, 1)]
public readonly record struct Monochrome1
{
    public readonly bool bit;

    public Monochrome1(bool bit) => this.bit = bit;
}

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 24)]
[ColorStruct(
    LayorType.Red, ConstantValues.R_FREQ, LayorValueType.Byte, 8,
    LayorType.Green, ConstantValues.G_FREQ, LayorValueType.Byte, 8,
    LayorType.Blue, ConstantValues.B_FREQ, LayorValueType.Byte, 8)]
public readonly record struct RGBColor24
{
    public readonly byte red;
    public readonly byte green;
    public readonly byte blue;

    public RGBColor24(byte red, byte green, byte blue)
    {
        this.red = red;
        this.green = green;
        this.blue = blue;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 48)]
[ColorStruct(
    LayorType.Red, ConstantValues.R_FREQ, LayorValueType.Int16, 16,
    LayorType.Green, ConstantValues.G_FREQ, LayorValueType.Int16, 16,
    LayorType.Blue, ConstantValues.B_FREQ, LayorValueType.Int16, 16)]
public readonly record struct RGBColor48
{
    public readonly ushort red;
    public readonly ushort green;
    public readonly ushort blue;

    public RGBColor48(ushort red, ushort green, ushort blue)
    {
        this.red = red;
        this.green = green;
        this.blue = blue;
    }
}
