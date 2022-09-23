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
