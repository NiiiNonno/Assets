/*
 * なぜかこれが原因で動作しなかったから消しただけ。残すつもり。
 */

//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Nonno.Assets;

//public readonly struct IdentifiedString : IEquatable<IdentifiedString>
//{
//    readonly string? _string;
//    readonly ShortIdentifier<IdentifiedString> _identifier;

//    public string? String => _string;
//    public ShortIdentifier<IdentifiedString> Identifier => _identifier;
//    [MemberNotNullWhen(false, nameof(String))]
//    public bool IsNull => _string is null;

//    public IdentifiedString(string @string, ShortIdentifier<IdentifiedString> identifier)
//    {
//        _string = @string;
//        _identifier = identifier;
//    }

//    public override string ToString() => $"{_string}#{_identifier:X4}";
//    public override bool Equals(object? obj) => obj is IdentifiedString @string && Equals(@string);
//    public bool Equals(IdentifiedString other) => _string == other._string && _identifier == other._identifier;
//    public override int GetHashCode() => HashCode.Combine(_string, _identifier);

//    public static IdentifiedString GetString(string @string, Func<ShortIdentifier<IdentifiedString>, bool> condition)
//    {
//#if DEBUG
//                int i = 0;
//#endif
//        ShortIdentifier<IdentifiedString> rand;
//        do
//        {
//            rand = ShortIdentifier<IdentifiedString>.GetNew();

//#if DEBUG
//                    if (i++ > 100000) throw new Exception("繰り返し回数が許容範囲を超過しました。");
//#endif
//        }
//        while (condition(rand));

//        return new(@string, rand);
//    }

//    public static implicit operator IdentifiedString(string @string) => new(@string, ShortIdentifier<IdentifiedString>.GetNew());
//    public static explicit operator string?(IdentifiedString identifiedString) => identifiedString.String;

//    public static bool operator ==(IdentifiedString left, IdentifiedString right) => left.Equals(right);
//    public static bool operator !=(IdentifiedString left, IdentifiedString right) => !(left == right);
//}
