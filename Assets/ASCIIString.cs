using System.Diagnostics.CodeAnalysis;
using Nonno.Assets.Scrolls;

namespace Nonno.Assets;

/// <summary>
/// アスキー字符号列を表します。
/// </summary>
public sealed class ASCIIString : IEquatable<ASCIIString?>
{
    const int HEAD = 0;

    readonly byte[] _codes;

    /// <summary>
    /// 有効否を取得ます。
    /// </summary>
    public bool IsValid
    {
        get
        {
            for (int i = 0; i < _codes.Length; i++) if ((_codes[i] & 0b1000_0000) != 0) return false;
            return true;
        }
    }
    /// <summary>
    /// 字符号列長を取得します。
    /// </summary>
    public int Length => _codes.Length;
    /// <summary>
    /// 字符号列上指定番の字符を取得します。
    /// </summary>
    /// <param name="index">
    /// 指定番。
    /// </param>
    /// <returns>
    /// 取得した字符。
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public char this[int index]
    {
        get => (char)_codes[index];
        set
        {
            if ((value >> 7) != 0) ThrowHelper.ArgumentIsNotASCII(value);
            _codes[index] = (byte)value;
        }
    }

    /// <summary>
    /// 十綜字符号列に構アスキー字符号列。
    /// </summary>
    /// <param name="string">
    /// 十綜字符号列。
    /// </param>
    public ASCIIString(string @string)
    {
        _codes = new byte[@string.Length];
        for (int i = 0; i < _codes.Length; i++) _codes[i] = unchecked((byte)@string[i]);

        //var span = @string.AsByteSpan();
        //_codes = new byte[span.Length >> 1];
        //int iCode = 0;
        //for (int iSpan = HEAD; iSpan < span.Length; iSpan += 2) _codes[iCode++] = span[iSpan];
    }
    /// <summary>
    /// 至十数刪に構アスキー字符号列。
    /// </summary>
    /// <param name="codes">
    /// 至十数刪。
    /// </param>
    public ASCIIString(ReadOnlySpan<byte> codes)
    {
        _codes = codes.ToArray();
    }
    /// <summary>
    /// 至十数列に構アスキー字符号列。
    /// </summary>
    /// <param name="codes">
    /// 至十数列。
    /// </param>
    public ASCIIString(IEnumerable<byte> codes)
    {
        _codes = codes.ToArray();
    }
    /// <summary>
    /// 至十数配列に構アスキー字符号列。
    /// </summary>
    /// <param name="codes">
    /// 構至十数配列。
    /// </param>
    internal ASCIIString(byte[] codes)
    {
        _codes = codes;
    }

    /// <summary>
    /// アスキー字符号列見做至十数刪。
    /// </summary>
    /// <returns>
    /// 所見做矣至十数刪。
    /// </returns>
    public Span<byte> AsSpan() => _codes;

    /// <summary>
    /// アスキー字符号列為十綜字符号列。
    /// </summary>
    /// <returns>
    /// 所為矣十綜字符号列。
    /// </returns>
    public override string ToString()
    {
        string r = new(default, Length);
        var span = r.UnsafeAsByteSpan();
        int iCode = 0;
        for (int iSpan = HEAD; iSpan < span.Length; iSpan += 2) span[iSpan] = _codes[iCode++];
        return r;
    }

    public override bool Equals(object? obj) => Equals(obj as ASCIIString);
    public bool Equals(ASCIIString? other) => other != null && _codes.SequenceEqual(other._codes);
    public override int GetHashCode() => HashCode.Combine(_codes);

    [return: NotNullIfNotNull("aSCIIString")]
    public static implicit operator string?(ASCIIString? aSCIIString) => aSCIIString?.ToString();
    [return: NotNullIfNotNull("string")]
    public static explicit operator ASCIIString?(string? @string) => @string is null ? null : new(@string);

    public static bool operator ==(ASCIIString? left, ASCIIString? right) => EqualityComparer<ASCIIString>.Default.Equals(left, right);
    public static bool operator ==(ASCIIString? left, string? right)
    {
        if (left is null) return right is null; else if (right is null) return false;

        for (int i = 0; i < right.Length; i++)
        {
            if (left._codes[i] != right[i]) return false;
        }
        return true;
    }
    public static bool operator !=(ASCIIString? left, ASCIIString? right) => !(left == right);
    public static bool operator !=(ASCIIString? left, string? right) => !(left == right);
}

public static partial class ScrollExtensions
{
    [IRMethod]
    public static Task Insert(this IScroll @this, ASCIIString? asciiString)
    {
        if (asciiString is null)
        {
            return @this.Insert(int32: -1);
        }
        else
        {
            @this.Insert(int32: asciiString.Length).Wait();
            @this.InsertSync(span: asciiString.AsSpan());
            return Task.CompletedTask;
        }
    }
    [IRMethod]
    public static Task Remove(this IScroll @this, out ASCIIString? asciiString)
    {
        @this.Remove(out int length);
        if (length < 0)
        {
            asciiString = null;
            return Task.CompletedTask;
        }
        else
        {
            byte[] codes = new byte[length];
            @this.RemoveSync(span: codes.AsSpan());
            asciiString = new(codes);
            return Task.CompletedTask;
        }
    }
}
