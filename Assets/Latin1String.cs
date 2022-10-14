using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Nonno.Assets.Scrolls;
using static Nonno.Assets.Utils;

namespace Nonno.Assets;

public sealed class Latin1String
{
    const byte FIRST_UNASSIGNED_START = 0x00;
    const byte FIRST_UNASSIGNED_END = 0x1F;
    const byte SECOND_UNASSIGNED_START = 0x7F;
    const byte SECOND_UNASSIGNED_END = 0x9F;

    readonly Latin1Char[] _chars;

    internal Latin1Char[] Chars => _chars;
    public int Length => _chars.Length - 1;
    public Latin1Char this[int index] => _chars[index];

    public Latin1String(IEnumerable<Latin1Char> chars)
    {
        _chars = chars.Append(default).ToArray();
    }
    public Latin1String(ReadOnlySpan<byte> bytes)
    {
        _chars = new Latin1Char[bytes.Length + 1];
        for (int i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] is >= FIRST_UNASSIGNED_START and <= FIRST_UNASSIGNED_END) throw new ArgumentException("無効な第一ラテン文字が含まれていました。", nameof(bytes));
            if (bytes[i] is >= SECOND_UNASSIGNED_START and <= SECOND_UNASSIGNED_END) throw new ArgumentException("無効な第一ラテン文字が含まれていました。", nameof(bytes));
            _chars[i] = (Latin1Char)bytes[i];
        }
        _chars[^1] = default;
    }
    public Latin1String(params Latin1Char[] charParams)
    {
        if (charParams[^1] != default) charParams = charParams.Append(default).ToArray();

        _chars = charParams;
    }

    /// <summary>
    /// 文字列をバイト区間とします。
    /// </summary>
    /// <returns>
    /// 文字列のバイト区間。
    /// <para>
    /// ヌル終端文字を末尾に含みます。
    /// </para>
    /// </returns>
    public Span<byte> AsSpan() => ((Span<Latin1Char>)_chars).ToSpan<Latin1Char, byte>();

    public ref Latin1Char GetPinnableReference() => ref _chars[0];

    public override string ToString()
    {
        string r = new(default, Length);
        var span = r.AsByteSpan();
        int iCode = 0;
        for (int iSpan = 0; iSpan < span.Length; iSpan += 2) span[iSpan] = (byte)_chars[iCode++];
        return r;
    }
}

public enum Latin1Char : byte
{
    Space = 0x20,
    ExclamationMark,
    DoubleQuotation,
    NumberSign,
    DollarSign,
    PercentSign,
    Ampersand,
    SingleQuotation,
    LeftRoundBracket,
    RightRoundBracket,
    Asterisk,
    PlusSign,
    Comma,
    MinusSign,
    Dot,
    Slash,
    Number0, Number1, Number2, Number3, Number4, Number5, Number6, Number7, Number8, Number9,
    Colon,
    Semicolon,
    LessThanSign,
    EqualsSign,
    GreaterThanSign,
    QuestionMark,

    CommercialAt = 0x40,
    A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
    LeftSquareBracket,
    Backslash,
    RightSquareBracket,
    Circumflex,
    Underline,

    GraveAccent = 0x60,
    SmallA, SmallB, SmallC, SmallD, SmallE, SmallF, SmallG, SmallH, SmallI, SmallJ, SmallK, SmallL, SmallM, SmallN, SmallO, SmallP, SmallQ, SmallR, SmallS, SmallT, SmallU, SmallV, SmallW, SmallX, SmallY, SmallZ,
    LeftCurlyBracket,
    VerticalBar,
    RightCurlyBracket,
    Tilde,

    NoBreakSpace = 0xA0,
    InvertedExclamationMark,
    CentSign,
    PoundSign,
    CurrencySign,
    YenSign,
    BrokenBar,
    SectionSign,
    Diaeresis,
    CopyrightSymbol,
    FeminineOrdinalIndicator,
    LeftAngleQuotationMark,
    NegationSign,
    SoftHyphen,
    RegisteredTrademarkSymbol,
    Macron,
    DegreeSymbol,
    PlusMinusSign,
    SquareSign,
    CubeSign,
    AcuteAccent,
    MicroSign,
    ParagraphMark,
    Interpunct,
    Cedilla,
    Superscript,
    MasculineOrdinalIndicator,
    RightAngleQuotationMark,
    Quarter,
    Half,
    ThreeQuarters,
    InvertedQuestionMark,

    GraveA = 0xC0, AcuteA, CircumflexA, TildeA, DiaeresisA, RingA,
    Ash,
    CedillaC,
    GraveE, AcuteE, CircumflexE, DiaeresisE,
    GraveI, AcuteI, CircumflexI, DiaeresisI,
    Eth,
    TildeN,
    GraveO, AcuteO, CircumflexO, TildeO, DiaeresisO,
    MultiplicationSign,
    SlashedO,
    GraveU, AcuteU, CircumflexU, DiaeresisU,
    AcuteY,
    Thorn,
    Eszett,

    SmallGraveA = 0xE0, SmallAcuteA, SmallCircumflexA, SmallTildeA, SmallDiaeresisA, SmallRingA,
    SmallAsh,
    SmallCedillaC,
    SmallGraveE, SmallAcuteE, SmallCircumflexE, SmallDiaeresisE,
    SmallGraveI, SmallAcuteI, SmallCircumflexI, SmallDiaeresisI,
    SmallEth,
    SmallTildeN,
    SmallGraveO, SmallAcuteO, SmallCircumflexO, SmallTildeO, SmallDiaeresisO,
    DivisionSign,
    SmallSlashedO,
    SmallGraveU, SmallAcuteU, SmallCircumflexU, SmallDiaeresisU,
    SmallAcuteY,
    SmallThorn,
    SmallDiaeresisY,

    SP = Space,
    NBSP = NoBreakSpace,
    SHY = SoftHyphen,
    Not = NegationSign,
    And = Ampersand,
    Or = VerticalBar,
    Sharp = NumberSign,
    LeftAngleBracket = LessThanSign,
    RightAngleBracket = GreaterThanSign,
    N0 = Number0, N1 = Number1, N2 = Number2, N3 = Number3, N4 = Number4, N5 = Number5, N6 = Number6, N7 = Number7, N8 = Number8, N9 = Number9,
    Zero = Number0, One = Number1, Two = Number2, Three = Number3, Four = Number4, Five = Number5, Six = Number6, Seven = Number7, Eight = Number8, Nine = Number9,
    À = GraveA,
    Á = AcuteA,
    Â = CircumflexA,
    Ã = TildeA,
    Ä = DiaeresisA,
    Å = RingA,
    Æ = Ash,
    Ç = CedillaC,
    È = GraveE,
    É = AcuteE,
    Ê = CircumflexE,
    Ë = DiaeresisE,
    Ì = GraveI,
    Í = AcuteI,
    Î = CircumflexI,
    Ï = DiaeresisI,
    Ð = Eth,
    Ñ = TildeN,
    Ò = GraveO,
    Ó = AcuteO,
    Ô = CircumflexO,
    Õ = TildeO,
    Ö = DiaeresisO,
    Ø = SlashedO,
    Ù = GraveU,
    Ú = AcuteU,
    Û = CircumflexO,
    Ü = DiaeresisU,
    Ý = AcuteY,
    Þ = Thorn,
    ß = Eszett,
    Small = Space,
    //à = SmallGraveA,
    //á = SmallAcuteA,
    //â = SmallCircumflexA,
    //ã = SmallTildeA,
    //ä = SmallDiaeresisA, 
    //å = SmallRingA,
    //æ = SmallAsh,
    //ç = SmallCedillaC,
    //è = SmallGraveE,
    //é = SmallAcuteE, 
    //ê = SmallCircumflexE,
    //ë = SmallDiaeresisE, 
    //ì = SmallGraveI,
    //í = SmallAcuteI,
    //î = SmallCircumflexI,
    //ï = SmallDiaeresisI,
    //ð = SmallEth,
    //ñ = SmallTildeN,
    //ò = SmallGraveO,
    //ó = SmallAcuteO,
    //ô = SmallCircumflexO,
    //õ = SmallTildeO,
    //ö = SmallDiaeresisO,
    //ø = SmallSlashedO,
    //ù = SmallGraveU,
    //ú = SmallAcuteU, 
    //û = SmallCircumflexU, 
    //ü = SmallDiaeresisU,
    //ý = SmallAcuteY,
    //þ = SmallThorn, 
    //ÿ = SmallDiaeresisY,
}

partial class ScrollExtensions
{
    [IRMethod]
    public static Task Insert(this IScroll @this, Latin1String latin1String) => @this.Insert<Latin1Char>(memory: latin1String.Chars);
    [IRMethod]
    public static Task Remove(this IScroll @this, out Latin1String latin1String)
    {
        List<Latin1Char> list = new();
        Span<Latin1Char> span = stackalloc Latin1Char[1];
        while (true)
        {
            @this.RemoveSync(span: span);
            if (span[0] == default)
            {
                list.Add(default);
                latin1String = new(list);
                return Task.CompletedTask;
            }
            list.Add(span[0]);
        }
    }
}
