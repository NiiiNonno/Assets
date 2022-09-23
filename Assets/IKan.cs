using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vector = System.UInt32;

namespace Nonno.Assets;

public readonly struct KanLine
{
    readonly Vector _vector;

    //public bool this[int index] => index switch
    //{
    //    0 => (_vector & 0b1000_0000_0000_0000) != 0,
    //    1 => (_vector & 0b0100_0000_0000_0000) != 0,
    //    2 => (_vector & 0b0010_0000_0000_0000) != 0,
    //    3 => (_vector & 0b0001_0000_0000_0000) != 0,
    //    4 => (_vector & 0b0000_1000_0000_0000) != 0,
    //    5 => (_vector & 0b0000_0100_0000_0000) != 0,
    //    6 => (_vector & 0b0000_0010_0000_0000) != 0,
    //    7 => (_vector & 0b0000_0001_0000_0000) != 0,
    //    8 => (_vector & 0b0000_0000_1000_0000) != 0,
    //    9 => (_vector & 0b0000_0000_0100_0000) != 0,
    //    10 => (_vector & 0b0000_0000_0010_0000) != 0,
    //    11 => (_vector & 0b0000_0000_0001_0000) != 0,
    //    12 => (_vector & 0b0000_0000_0000_1000) != 0,
    //    13 => (_vector & 0b0000_0000_0000_0100) != 0,
    //    14 => (_vector & 0b0000_0000_0000_0010) != 0,
    //    15 => (_vector & 0b0000_0000_0000_0001) != 0,
    //    _ => throw new ArgumentOutOfRangeException(nameof(index))
    //};

    public Vector Vector => _vector;

    private KanLine(Vector vector) { _vector = vector; }
    public KanLine(bool l0, bool r0, bool l1, bool r1, bool l2, bool r2, bool l3, bool r3, bool l4, bool r4, bool l5, bool r5, bool l6, bool r6, bool l7, bool r7, bool l8, bool r8, bool l9, bool r9, bool l10, bool r10, bool l11, bool r11, bool l12, bool r12, bool l13, bool r13, bool l14, bool r14, bool l15, bool r15) : this((
            /* 
            for (int i = 0; i < 32; i++)
            {
                if ((i & 1) == 0) Write($"bool l{i >> 1}, ");
                else Write($"bool r{i >> 1}, ");
            }

            for (int i = 0; i < 32; i++)
            {
                char[] chars = new char[32];
                for (int j = 0; j < chars.Length; j++) chars[j] = j == i ? '1' : '0';
                Console.WriteLine($"({((i&1)==0?'l':'r')}{i>>1} ? 0b{new string(chars)}u : 0u) |");
            }
            */
            (l0 ? 0b10000000000000000000000000000000u : 0u) |
            (r0 ? 0b01000000000000000000000000000000u : 0u) |
            (l1 ? 0b00100000000000000000000000000000u : 0u) |
            (r1 ? 0b00010000000000000000000000000000u : 0u) |
            (l2 ? 0b00001000000000000000000000000000u : 0u) |
            (r2 ? 0b00000100000000000000000000000000u : 0u) |
            (l3 ? 0b00000010000000000000000000000000u : 0u) |
            (r3 ? 0b00000001000000000000000000000000u : 0u) |
            (l4 ? 0b00000000100000000000000000000000u : 0u) |
            (r4 ? 0b00000000010000000000000000000000u : 0u) |
            (l5 ? 0b00000000001000000000000000000000u : 0u) |
            (r5 ? 0b00000000000100000000000000000000u : 0u) |
            (l6 ? 0b00000000000010000000000000000000u : 0u) |
            (r6 ? 0b00000000000001000000000000000000u : 0u) |
            (l7 ? 0b00000000000000100000000000000000u : 0u) |
            (r7 ? 0b00000000000000010000000000000000u : 0u) |
            (l8 ? 0b00000000000000001000000000000000u : 0u) |
            (r8 ? 0b00000000000000000100000000000000u : 0u) |
            (l9 ? 0b00000000000000000010000000000000u : 0u) |
            (r9 ? 0b00000000000000000001000000000000u : 0u) |
            (l10 ? 0b00000000000000000000100000000000u : 0u) |
            (r10 ? 0b00000000000000000000010000000000u : 0u) |
            (l11 ? 0b00000000000000000000001000000000u : 0u) |
            (r11 ? 0b00000000000000000000000100000000u : 0u) |
            (l12 ? 0b00000000000000000000000010000000u : 0u) |
            (r12 ? 0b00000000000000000000000001000000u : 0u) |
            (l13 ? 0b00000000000000000000000000100000u : 0u) |
            (r13 ? 0b00000000000000000000000000010000u : 0u) |
            (l14 ? 0b00000000000000000000000000001000u : 0u) |
            (r14 ? 0b00000000000000000000000000000100u : 0u) |
            (l15 ? 0b00000000000000000000000000000010u : 0u) |
            (r15 ? 0b00000000000000000000000000000001u : 0u)))
    { }
    public KanLine(bool[] array) : this(
        array[0], array[1], array[2], array[3], array[4], array[5], array[6], array[7], array[8], array[9], array[10], array[11], array[12], array[13], array[14], array[15], array[16], array[17], array[18], array[19], array[20], array[21], array[22], array[23], array[24], array[25], array[26], array[27], array[28], array[29], array[30], array[31])
    { }
    public KanLine(BitArray array) : this(
        /*
        for (int i = 0; i < 32; i++)
        {
            Console.Write($"array[{i}], ");
        }
         */
        array[0], array[1], array[2], array[3], array[4], array[5], array[6], array[7], array[8], array[9], array[10], array[11], array[12], array[13], array[14], array[15], array[16], array[17], array[18], array[19], array[20], array[21], array[22], array[23], array[24], array[25], array[26], array[27], array[28], array[29], array[30], array[31])
    { }

    public static explicit operator uint(KanLine line) => line._vector;
    public static implicit operator KanLine(uint vector) => new(vector);
}

public class Kan
{
    readonly KanLine[] _lines;

    private Kan(KanLine[] lines)
    {
        _lines = lines;
    }
}

public class Character
{
    readonly KanLine[] _lines;

    private Character(KanLine[] lines)
    {
        _lines = lines;
    }

    public static Character GetCharacter(KanLine[] lines)
    {
        var copied = new KanLine[lines.Length];
        lines.CopyTo(copied, 0);
        return new Character(copied);
    }
    public static Character GetCharacter(char code)
    {
        throw new NotImplementedException();
    }
}
