using System.Diagnostics.CodeAnalysis;
using Nonno.Assets.Scrolls;

namespace Nonno.Assets;

public sealed class ASCIIString : IEquatable<ASCIIString?>
{
    const int HEAD = 0;

    readonly byte[] _codes;

    public bool IsValid
    {
        get
        {
            for (int i = 0; i < _codes.Length; i++) if ((_codes[i] & 0b1000_0000) != 0) return false;
            return true;
        }
    }
    public int Length => _codes.Length;
    public char this[int index]
    {
        get => (char)_codes[index];
        set
        {
            if ((value >> 7) != 0) throw new ArgumentException("引数がASCIIコードで示すことのできない文字です。", nameof(value));
            _codes[index] = (byte)value;
        }
    }

    public ASCIIString(string @string)
    {
        _codes = new byte[@string.Length];
        for (int i = 0; i < _codes.Length; i++) _codes[i] = unchecked((byte)@string[i]);

        //var span = @string.AsByteSpan();
        //_codes = new byte[span.Length >> 1];
        //int iCode = 0;
        //for (int iSpan = HEAD; iSpan < span.Length; iSpan += 2) _codes[iCode++] = span[iSpan];
    }
    public ASCIIString(ReadOnlySpan<byte> codes)
    {
        _codes = codes.ToArray();
    }
    public ASCIIString(IEnumerable<byte> codes)
    {
        _codes = codes.ToArray();
    }
    internal ASCIIString(byte[] codes)
    {
        _codes = codes;
    }

    public Span<byte> AsSpan() => _codes;

    public override string ToString()
    {
        string r = new(default, Length);
        var span = r.AsByteSpan();
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
