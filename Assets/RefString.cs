using String = System.ReadOnlySpan<char>;

namespace Nonno.Assets;
#pragma warning disable CS0659 // 型は Object.Equals(object o) をオーバーライドしますが、Object.GetHashCode() をオーバーライドしません
#pragma warning disable CS0661 // 型は演算子 == または演算子 != を定義しますが、Object.GetHashCode() をオーバーライドしません
public readonly ref struct RefString
{
    readonly String _string;

    public bool IsNull => _string == default(String);
    public int Length => _string.Length;

    internal RefString(String @string) => _string = @string;

    public override bool Equals(object? obj) => obj is string @string && this == @string;
    public override string ToString() => new(_string);
    public String AsSpan() => _string;

    public static bool operator ==(RefString left, RefString right) => left._string == right._string || left._string.SequenceEqual(right._string);
    public static bool operator !=(RefString left, RefString right) => !(left == right);
    public static bool operator ==(RefString left, string? right) => right is null ? left._string == default(String) : left._string.SequenceEqual(right);
    public static bool operator !=(RefString left, string? right) => !(left == right);
    public static bool operator ==(string? left, RefString right) => left is null ? right._string == default(String) : right._string.SequenceEqual(left);
    public static bool operator !=(string? left, RefString right) => !(left == right);
    public static bool operator ==(RefString left, String right) => left._string == right;
    public static bool operator !=(RefString left, String right) => left._string == right;
    public static bool operator ==(String left, RefString right) => left == right._string;
    public static bool operator !=(String left, RefString right) => left == right._string;
    public static implicit operator RefString(string? @string) => new(@string is null ? default(String) : @string.AsSpan());
    public static explicit operator string?(RefString refString) => refString.IsNull ? null : new(refString._string);
}
#pragma warning restore CS0659 // 型は Object.Equals(object o) をオーバーライドしますが、Object.GetHashCode() をオーバーライドしません
#pragma warning restore CS0661 // 型は演算子 == または演算子 != を定義しますが、Object.GetHashCode() をオーバーライドしません

partial class Utils
{
    public static RefString AsString(this String @this) => new(@this);
}
