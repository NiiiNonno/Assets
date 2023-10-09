namespace Nonno.Assets;

/// <summary>
/// 二の冪乗の数を表します。
/// <para>
/// この値型は特に整数の乗除剰余演算に対して高速で、また浮動小数点数の除算に対しても一般的な速さで演算できます。
/// </para>
/// </summary>
[Serializable]
public readonly struct Shift
{
    readonly int e;

    public int Exponent => e;

    public Shift(int exponent)
    {
        if (exponent < 0) throw new ArgumentException("負の指数は指定できません。", nameof(exponent));
        this.e = exponent;
    }

    public static Shift GetSufficientValue(int value) => GetSufficientValue((uint)value);
    public static Shift GetSufficientValue(uint value)
    {
        int i = 0;
        value--;
        while (value != 0)
        {
            value >>= 1;
            i++;
        }
        return new(i);
    }

    public static Shift GetNecessaryValue(int value) => GetNecessaryValue((uint)value);
    public static Shift GetNecessaryValue(uint value)
    {
        int i = -1;
        while (value != 0)
        {
            value >>= 1;
            i++;
        }
        return new(i);
    }

    public static readonly Shift S1 = new(0);
    public static readonly Shift S2 = new(1);
    public static readonly Shift S4 = new(2);
    public static readonly Shift S8 = new(3);
    public static readonly Shift S16 = new(4);
    public static readonly Shift S32 = new(5);
    public static readonly Shift S64 = new(6);
    public static readonly Shift S128 = new(7);
    public static readonly Shift S256 = new(8);
    public static readonly Shift S512 = new(9);
    public static readonly Shift S1024 = new(10);
    public static readonly Shift S2048 = new(11);
    public static readonly Shift S4096 = new(12);
    public static readonly Shift S8192 = new(13);
    public static readonly Shift S16384 = new(14);
    public static readonly Shift S32768 = new(15);
    public static readonly Shift S65535 = new(16);

    public static int operator *(int a, Shift b) => a << b.e;
    public static int operator *(Shift left, int right) => right << left.e;
    public static uint operator *(Shift left, uint right) => right << left.e;
    public static long operator *(Shift left, long right) => right << left.e;
    public static ulong operator *(Shift left, ulong right) => right << left.e;
    //public static unsafe float operator *(Shift left, float right) => (float)((Float32)right << left.e);
    //public static unsafe double operator *(Shift left, double right) => (double)((Float64)right << left.e);
    public static Shift operator *(Shift left, Shift right) => new(left.e + right.e);
    public static int operator /(int left, Shift right) => left >> right.e;
    public static uint operator /(uint left, Shift right) => left >> right.e;
    public static long operator /(long left, Shift right) => left >> right.e;
    public static ulong operator /(ulong left, Shift right) => left >> right.e;
    //public static unsafe float operator /(float left, Shift right) => (float)((Float32)left >> right.e);
    //public static unsafe double operator /(double left, Shift right) => (double)((Float64)left >> right.e);
    public static Shift operator /(Shift left, Shift right) => new(left.e - right.e);
    public static int operator %(int left, Shift right) => left & ~(~0 << right.e);
    public static uint operator %(uint left, Shift right) => left & ~(~0U << right.e);
    public static long operator %(long left, Shift right) => left & ~(~0L << right.e);
    public static ulong operator %(ulong left, Shift right) => left & ~(~0UL << right.e);
    public static implicit operator int(Shift p) => 1 << p.e;
}