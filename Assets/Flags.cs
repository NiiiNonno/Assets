using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets;

[Flags]
public enum PairFlags : sbyte
{
    None = 0,
    Iang = 1,
    Iem = ~1,

    Heavy = Iang, Light = Iem,
    Big = Iang, Small = Iem,
    Large = Iang,//Small,
    High = Iang, Low = Iem,
    Long = Iang, Short = Iem,
    In = Iang, OutOf = Iem,
    Inside = Iang, Outside = Iem,
    Inner = Iang, Outer = Iem,
    Internal = Iang, External = Iem,
    Increase = Iang, Decrease = Iem,
    Upside = Iang, Downside = Iem,
    Top = Iang, Bottom = Iem,
    Daytime = Iang, Night = Iem,
    Rare = Iang, Frequent = Iem,
    Centrifugal = Iang, Centripetal = Iem,
    Hot = Iang, Cold = Iem,
    Sun = Iang, Moon = Iem,
    Bright = Iang, Dark = Iem,
    Dry = Iang, Wet = Iem,
    Fine = Iang, Cloudy = Iem,
    Proton = Iang, Electron = Iem,
    Birth = Iang, Death = Iem,
    Time = Iang, Space = Iem,
    Male = Iang, Female = Iem,
    Man = Iang, Woman = Iem,
    Husband = Iang, Wife = Iem,
    Father = Iang, Mother = Iem,
    Body = Iang, Spirit = Iem,
    Plus = Iang, Minus = Iem,
    Right = Iang, Left = Iem,

    All = Iang | Iem,
}

[Flags]
public enum SamFlags : byte
{
    None = 0b000,
    Thien = 0b100,
    Nhien = 0b010,
    Dhiey = 0b001,
    All = 0b111,
}

[Flags]
public enum NguoFlags : byte
{
    None = 0b0000,

    Xua = 0b00001,
    Cue = 0b00010,
    Muk = 0b00100,
    Kim = 0b01000,
    Duo = 0b10000,

    Fire = Xua,
    Red = Xua,
    South = Xua,
    Summer = Xua,
    Tuesday = Xua,
    Mars = Xua,
    Bird = Xua,
    Touch = Xua,
    Tongue = Xua,
    Anger = Xua,
    Heart = Xua,
    SmallIntestine = Xua,
    Bitter = Xua,
    Seeing = Xua,
    Wheat = Xua,
    Sheep = Xua,

    Water = Cue,
    Black = Cue,
    North = Cue,
    Winter = Cue,
    Wednesday = Cue,
    Mercury = Cue,
    Shell = Cue,
    Voice = Cue,
    Ear = Cue,
    Grudge = Cue,
    Kidney = Cue,
    Bladder = Cue,
    Solty = Cue,
    Soy = Cue,
    Pig = Cue,

    Wood = Muk,
    Blue = Muk,
    East = Muk,
    Spring = Muk,
    Thursday = Muk,
    Jupiter = Muk,
    Fish = Muk,
    Color = Muk,
    Eye = Muk,
    Pleasure = Muk,
    Liver = Muk,
    Gallbladder = Muk,
    Cannabis = Muk,
    Dog = Muk,

    Metal = Kim,
    White = Kim,
    West = Kim,
    Autumn = Kim,
    Friday = Kim,
    Venus = Kim,
    Animal = Kim,
    Scent = Kim,
    Nose = Kim,
    Delight = Kim,
    Lung = Kim,
    LargeIntestine = Kim,
    Spicy = Kim,
    Millet = Kim,
    Horse = Kim,

    Soil = Duo,
    Center = Duo,
    Saturday = Duo,
    Saturn = Duo,
    Human = Duo,
    Taste = Duo,
    Mouse = Duo,
    Spleen = Duo,
    Stomach = Duo,
    Sweet = Duo,
    Rice = Duo,
    Cow = Duo,

    All = 0b11111,
}
