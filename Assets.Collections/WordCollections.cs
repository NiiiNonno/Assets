// 令和弐年大暑確認済。
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using BasicUtils = Nonno.Assets.Utils;
using static Nonno.Assets.Collections.Utils;

namespace Nonno.Assets.Collections;

/// <summary>
/// 空白で区切られた文字列による、読み取り専用のリストを表します。
/// <para>
/// 文字が空白であるかの判定は<see cref="BasicUtils.IsSeparator(char)"/>に準拠します。
/// </para>
/// </summary>
public readonly struct WordList : IReadOnlyList<string>, IEquatable<WordList>
{
    readonly string? _string;

    /// <inheritdoc/>
    public int Count => AsSpan().Count;
    public int Size => _string == null ? 0 : _string.Length;
    /// <summary>
    /// リストの指定位置にある要素を読み取り専用範囲で返します。
    /// </summary>
    /// <param name="number">
    /// 指定する位置。
    /// </param>
    /// <returns>リストの指定位置にある要素。</returns>
    /// <exception cref="ArgumentOutOfRangeException">指定位置がリストの長さを超えた場合に投げられます。</exception>
    public string this[int number] => AsSpan()[number];
    public WordList this[Range numberRange] => new(AsSpan()[numberRange]);

    public WordList() => _string = string.Empty;
    /// <summary>
    /// 文字列からリストを初期化します。
    /// <para>
    /// 初期化にかかる負荷は非常に軽微なものです。
    /// </para>
    /// </summary>
    /// <param name="string"></param>
    public WordList(string @string) => _string = @string;
    public WordList(WordSpan span) => _string = span.ToString();
    public WordList(params string[] args) : this((IEnumerable<string>)args) { }
    public WordList(IEnumerable<string> args)
    {
        var builder = new StringBuilder();
        foreach (var arg in args)
        {
            arg.WriteEscaped(to: builder);
            _ = builder.Append(' ');
        }

        _string = builder.Remove(builder.Length - 1, 1).ToString();
    }

    /// <inheritdoc/>
    public IEnumerator<string> GetEnumerator()
    {
        int end = 0;

        while (true)
        {
            if (AsSpan().TryGetRange(0, end, out var result))
            {
                yield return AsSpan().GetWordAsString(result);
                end = result.End;
            }
            else
            {
                yield break;
            }
        }
    }
    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Contains(int number) => AsSpan().Contains(number);
    /// <summary>
    /// 指定する要素がリストに含まれるかを示す値を返します。
    /// </summary>
    /// <param name="of">
    /// 指定する要素。
    /// </param>
    /// <returns>
    /// 指定した要素がリストに含まれるならば`true`、それ以外は`false`。
    /// </returns>
    public bool Contains(ReadOnlySpan<char> of) => AsSpan().Contains(of);
    public bool Equals(WordList other) => AsSpan().Equals(other.AsSpan());
    public override bool Equals(object? obj) => obj is WordList wordList && AsSpan().Equals(wordList.AsSpan());
    public override int GetHashCode() => AsSpan().GetHashCode();
    public int GetIndex(int number, int index = 0) => AsSpan().GetIndex(number, index);
    public int GetNumber(ReadOnlySpan<char> of) => AsSpan().GetNumber(of);
    public int GetNumberFromIndex(int index) => AsSpan().GetNumberFromIndex(index);
    public WordRange GetRange(int number, int index = 0) => AsSpan().GetRange(number, index);
    public WordRange GetRange(ReadOnlySpan<char> of) => AsSpan().GetRange(of);
    public WordRange GetRangeFromIndex(int index) => AsSpan().GetRangeFromIndex(index);
    public ReadOnlySpan<char> GetWordAsSpan(WordRange range) => AsSpan().GetWordAsSpan(range);
    public string GetWordAsString(WordRange range) => AsSpan().GetWordAsString(range);
    public override string ToString() => _string ?? String.Empty;
    public bool TryGetIndex(int number, int index, [NotNullWhen(true)] out int result) => AsSpan().TryGetIndex(number, index, out result);
    public bool TryGetNumber(ReadOnlySpan<char> of, [NotNullWhen(true)] out int result) => AsSpan().TryGetNumber(of, out result);
    public bool TryGetRange(int number, int index, [NotNullWhen(true)] out WordRange result) => AsSpan().TryGetRange(number, index, out result);
    public bool TryGetRange(ReadOnlySpan<char> of, [NotNullWhen(true)] out WordRange result) => AsSpan().TryGetRange(of, out result);

    public WordSpan AsSpan() => new(_string);

    bool IReadOnlyList<string>.TryGetValue(int index, [MaybeNullWhen(false)] out string value)
    {
        value = AsSpan()[index];
        return true;
    }

    public static bool operator ==(WordList left, WordList right) => left.Equals(right);
    public static bool operator !=(WordList left, WordList right) => !left.Equals(right);
    public static implicit operator WordSpan(WordList p) => new(p._string);
}

public readonly struct WordDictionary : IReadOnlyDictionary<string, string>
{
    public const char SEPARATOR = ':';
    public const char SPACE = ' ';

    readonly string? _string;

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => AsSpan().Count;
    }
    public bool IsValid => _string != null && new WordList(_string).All(x => x.Contains(SEPARATOR));
    public IEnumerable<string> Keys
    {
        get
        {
            if (_string == null) yield break;
            int end = 0;

            while (true)
            {
                if (AsSpan().TryGetRange(0, end, out var range))
                {
                    var index = _string.IndexOf(SEPARATOR, range.Start);
                    if (0 <= index && index <= range.End)
                    {
                        yield return AsSpan().GetWordAsString(range.CloneWithEndIs(index));
                    }
                    end = range.End;
                }
                else
                {
                    yield break;
                }
            }
        }
    }
    public IEnumerable<string> Values
    {
        get
        {
            if (_string == null) yield break;
            int end = 0;

            while (true)
            {
                if (AsSpan().TryGetRange(0, end, out var range))
                {
                    var index = _string.IndexOf(SEPARATOR, range.Start);
                    if (0 <= index && index <= range.End)
                    {
                        yield return AsSpan().GetWordAsString(range.CloneWithStartIs(index + 1));
                    }
                    end = range.End;
                }
                else
                {
                    yield break;
                }
            }
        }
    }
    public string this[string key] => TryGetValue(key, out string? r) ? r : throw new KeyNotFoundException();

    public WordDictionary() => _string = string.Empty;
    public WordDictionary(string @string)
    {
        _string = @string;
    }
    public WordDictionary(params string[] args) : this((IEnumerable<string>)args) { }
    public WordDictionary(IEnumerable<string> args)
    {
        var builder = new StringBuilder();
        foreach (var arg in args)
        {
            arg.WriteEscaped(to: builder);
            _ = builder.Append(SPACE);
        }
        _string = builder.ToString();
    }
    public WordDictionary(params KeyValuePair<string, string>[] args) : this((IEnumerable<KeyValuePair<string, string>>)args) { }
    public WordDictionary(IEnumerable<KeyValuePair<string, string>> dictionary)
    {
        var builder = new StringBuilder();
        foreach (var (key, value) in dictionary)
        {
            key.WriteEscaped(to: builder);
            _ = builder.Append(SEPARATOR);
            value.WriteEscaped(to: builder);
            _ = builder.Append(SPACE);
        }
        _string = builder.ToString();
    }

    public bool Contains(KeyValuePair<string, string> item)
    {
        if (_string == null) return false;
        int end = 0;

        while (true)
        {
            if (AsSpan().TryGetRange(0, end, out var range))
            {
                var index = _string.IndexOf(SEPARATOR, range.Start);
                if (0 <= index && index <= range.End)
                {
                    if (range.CloneWithEndIs(index).SpanEquals(of: _string, to: item.Key) && range.CloneWithStartIs(index + 1).SpanEquals(of: _string, to: item.Value)) return true;
                }
                end = range.End;
            }
            else
            {
                return false;
            }
        }
    }
    public bool ContainsKey(string key)
    {
        if (_string == null) return false;
        int end = 0;

        while (true)
        {
            if (AsSpan().TryGetRange(0, end, out var range))
            {
                var index = _string.IndexOf(SEPARATOR, range.Start);
                if (0 <= index && index <= range.End)
                {
                    if (range.CloneWithEndIs(index).SpanEquals(of: _string, to: key)) return true;
                }
                end = range.End;
            }
            else
            {
                return false;
            }
        }
    }
    public bool ContainsValue(string value)
    {
        if (_string == null) return false;
        int end = 0;

        while (true)
        {
            if (AsSpan().TryGetRange(0, end, out var range))
            {
                var index = _string.IndexOf(SEPARATOR, range.Start);
                if (0 <= index && index <= range.End)
                {
                    if (range.CloneWithStartIs(index + 1).SpanEquals(of: _string, to: value)) return true;
                }
                end = range.End;
            }
            else
            {
                return false;
            }
        }
    }

    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
    {
        if (_string == null) return;
        int end = 0;

        while (true)
        {
            if (AsSpan().TryGetRange(0, end, out var range))
            {
                var index = _string.IndexOf(SEPARATOR, range.Start);
                if (0 <= index && index <= range.End)
                {
                    array[arrayIndex++] = new(AsSpan().GetWordAsString(range.CloneWithEndIs(index)), AsSpan().GetWordAsString(range.CloneWithStartIs(index + 1)));
                }
                end = range.End;
            }
            else
            {
                return;
            }
        }
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        if (_string == null) yield break;
        int end = 0;

        while (true)
        {
            if (AsSpan().TryGetRange(0, end, out var range))
            {
                var index = _string.IndexOf(SEPARATOR, range.Start);
                if (0 <= index && index <= range.End)
                {
                    yield return new(AsSpan().GetWordAsString(range.CloneWithEndIs(index)), AsSpan().GetWordAsString(range.CloneWithStartIs(index + 1)));
                }
                end = range.End;
            }
            else
            {
                yield break;
            }
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
    {
        if (_string == null)
        {
            value = null;
            return false;
        }
        int end = 0;

        while (true)
        {
            if (AsSpan().TryGetRange(0, end, out var range))
            {
                var index = _string.IndexOf(SEPARATOR, range.Start);
                if (0 <= index && index <= range.End)
                {
                    if (range.CloneWithEndIs(index).SpanEquals(of: _string, to: key))
                    {
                        value = AsSpan().GetWordAsString(range.CloneWithStartIs(index + 1));
                        return true;
                    }
                }
                end = range.End;
            }
            else
            {
                value = default;
                return false;
            }
        }
    }
    public bool TryGetValue(ReadOnlySpan<char> key, [MaybeNullWhen(false)] out ReadOnlySpan<char> value)
    {
        if (_string == null)
        {
            value = default;
            return false;
        }
        int end = 0;

        while (true)
        {
            if (AsSpan().TryGetRange(0, end, out var range))
            {
                var index = _string.IndexOf(SEPARATOR, range.Start);
                if (0 <= index && index <= range.End)
                {
                    if (range.CloneWithEndIs(index).SpanEquals(of: _string, to: key))
                    {
                        value = AsSpan().GetWordAsSpan(range.CloneWithStartIs(index + 1));
                        return true;
                    }
                }
                end = range.End;
            }
            else
            {
                value = default;
                return false;
            }
        }
    }

    /// <summary>
    /// 語典を表す文字列を返します。
    /// </summary>
    /// <returns>
    /// 語典を表す文字列。
    /// </returns>
    public override string ToString() => _string ?? String.Empty;

    public WordSpan AsSpan() => new(_string ?? String.Empty);
}

/// <summary>
/// 空白で区切られた文字列による、読み取り専用のリストを表します。
/// <para>
/// 文字が空白であるかの判定は<see cref="BasicUtils.IsSeparator(char)"/>に準拠します。
/// </para>
/// </summary>
public readonly ref struct WordSpan
{
    readonly ReadOnlySpan<char> _string;

    /// <inheritdoc/>
    public int Count
    {
        get
        {
            var count = 0;
            // antichar flag
            var aF = true;
            // escaping flag
            byte eF = 0;

            for (int i = 0; i < _string.Length; i++)
            {
                // エスケープ外
                if (eF == 0)
                {
                    // エスケープ文字を更新し、
                    eF = GetEscapingCode(of: _string[i]);

                    // もしエスケープ記号でなく語間ならば、
                    if (eF == 0 && BasicUtils.IsSeparator(_string[i]))
                    {
                        // 前字フラグを立てる。
                        aF = true;
                    }
                    // もし語頭ならば、
                    else if (aF)
                    {
                        // 語を数え、
                        count++;

                        // 前字フラグを下す。
                        aF = false;
                    }
                    // もし語中ならば、
                    else
                    {

                    }
                }
                // エスケープ内
                else
                {
                    // もし記号が前のエスケープ記号と同じならば、
                    if (GetEscapingCode(of: _string[i]) == eF)
                    {
                        // エスケープを解く。
                        eF = 0;
                    }
                }
            }

            return count;
        }
    }
    public int Size => _string.Length;
    /// <summary>
    /// リストの指定位置にある要素を読み取り専用範囲で返します。
    /// </summary>
    /// <param name="number">
    /// 指定する位置。
    /// </param>
    /// <returns>リストの指定位置にある要素。</returns>
    /// <exception cref="ArgumentOutOfRangeException">指定位置がリストの長さを超えた場合に投げられます。</exception>
    public string this[int number] => GetWordAsString(GetRange(number));
    public WordSpan this[Range numberRange]
    {
        get
        {
            var count = Count;
            var rangeStart = numberRange.Start.GetOffset(count);
            var rangeEnd = numberRange.End.GetOffset(count);
            var start = GetIndex(rangeStart);
            var end = GetRange(rangeEnd - rangeStart, start).End;
            return new(_string[start..end]);
        }
    }

    public WordSpan() => _string = string.Empty;
    /// <summary>
    /// 文字列からリストを初期化します。
    /// <para>
    /// 初期化にかかる負荷は非常に軽微なものです。
    /// </para>
    /// </summary>
    /// <param name="string"></param>
    public WordSpan(ReadOnlySpan<char> @string) => _string = @string;

    public ReadOnlySpan<char> GetWordAsSpan(WordRange range) => range.HasEscaped ? Unescape(_string[(Range)range]) : _string[(Range)range];
    public string GetWordAsString(WordRange range) => range.HasEscaped ? Unescape(_string[(Range)range]) : new(_string[(Range)range]);

    // 文字列との等価比較から結果を得る。

    /// <summary>
    /// 指定する要素がリストに含まれるかを示す値を返します。
    /// </summary>
    /// <param name="of">
    /// 指定する要素。
    /// </param>
    /// <returns>
    /// 指定した要素がリストに含まれるならば`true`、それ以外は`false`。
    /// </returns>
    public bool Contains(ReadOnlySpan<char> of)
    {
        var headI = 0;
        // antichar flag
        var aF = true;
        // escaping flag
        byte eF = 0;
        // have escaped flag
        var heF = false;

        for (int i = 0; i < _string.Length; i++)
        {
            // エスケープ外
            if (eF == 0)
            {
                // エスケープ文字を更新し、
                eF = GetEscapingCode(of: _string[i]);
                // それが零であることか判断しておき、
                var eF_ = eF == 0;
                // エスケープフラグを更新する。
                heF |= !eF_;

                // もし語間ならば、
                if (eF_ && BasicUtils.IsSeparator(_string[i]))
                {
                    // 現在の語が指定の語ならば返す。返せば二度目は回ってこないので空白の連続を考える必要がない。
                    if (new WordRange(headI, Size, heF).SpanEquals(of: _string, to: of)) return true;

                    // 前字フラグを立てる。
                    aF = true;

                    // エスケープフラグを下す。
                    heF = false;
                }
                // もし語頭ならば、
                else if (aF)
                {
                    // ヘッダを更新する。
                    headI = i;

                    // 前字フラグを下す。
                    aF = false;
                }
                // もし語中ならば、
                else
                {

                }
            }
            // エスケープ内
            else
            {
                // もし記号が前のエスケープ記号と同じならば、
                if (GetEscapingCode(of: _string[i]) == eF)
                {
                    // エスケープを解く。
                    eF = 0;
                }
            }
        }

        // 最後の語でも前が語間でなければ(←語間で返すから)返す。
        if (!aF && new WordRange(headI, Size, heF).SpanEquals(of: _string, to: of)) return true;

        return false;
    }

    public int GetNumber(ReadOnlySpan<char> of) => TryGetNumber(of, out var result) ? result : throw new KeyNotFoundException();
    /// <summary>
    /// 指定する要素がリストの何番目かを示す索引を返します。
    /// </summary>
    /// <param name="of">
    /// 指定する要素。
    /// </param>
    /// <returns>
    /// 指定した要素がリストに含まれないときは`null`、それ以外は指定した要素がリストの何番目かを示す索引。
    /// </returns>
    public bool TryGetNumber(ReadOnlySpan<char> of, [NotNullWhen(true)] out int result)
    {
        var count = 0;
        var headI = 0;
        // antichar flag
        var aF = true;
        // escaping flag
        byte eF = 0;
        // have escaped flag
        var heF = false;

        for (int i = 0; i < _string.Length; i++)
        {
            // エスケープ外
            if (eF == 0)
            {
                // エスケープ文字を更新し、
                eF = GetEscapingCode(of: _string[i]);
                // それが零であることか判断しておき、
                var eF_ = eF == 0;
                // エスケープフラグを更新する。
                heF |= !eF_;

                // もし語間ならば、
                if (eF_ && BasicUtils.IsSeparator(_string[i]))
                {
                    // 現在の語が指定の語ならば返す。返せば二度目は回ってこないので空白の連続を考える必要がない。
                    if (new WordRange(headI, i, heF).SpanEquals(of: _string, to: of))
                    {
                        result = count;
                        return true;
                    }

                    // 前字フラグを立てる。
                    aF = true;

                    // エスケープフラグを下す。
                    heF = false;
                }
                // もし語頭ならば、
                else if (aF)
                {
                    // 語を数え、
                    count++;

                    // ヘッダを更新する。
                    headI = i;

                    // 前字フラグを下す。
                    aF = false;
                }
                // もし語中ならば、
                else
                {

                }
            }
            // エスケープ内
            else
            {
                // もし記号が前のエスケープ記号と同じならば、
                if (GetEscapingCode(of: this._string[i]) == eF)
                {
                    // エスケープを解く。
                    eF = 0;
                }
            }
        }

        // 最後の語でも前が語間でなければ(←語間で返すから)返す。
        if (!aF && new WordRange(headI, Size, heF).SpanEquals(of: _string, to: of))
        {
            result = count;
            return true;
        }

        result = -1;
        return false;
    }

    /// <summary>
    /// 指定する要素の索引を返します。
    /// </summary>
    /// <param name="of">
    /// 指定する要素。
    /// </param>
    /// <returns>
    /// 指定した要素がリストの何番目かを示す索引。
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// 指定された要素がリストに存在しません。
    /// </exception>
    public WordRange GetRange(ReadOnlySpan<char> of) => TryGetRange(of, out var result) ? result : throw new KeyNotFoundException();
    /// <summary>
    /// 指定する要素の索引を返します。
    /// </summary>
    /// <param name="of">
    /// 指定する要素。
    /// </param>
    /// <returns>
    /// 指定した要素がリストに含まれないときは`null`、それ以外は指定した要素がリストの何番目かを示す索引。
    /// </returns>
    public bool TryGetRange(ReadOnlySpan<char> of, [NotNullWhen(true)] out WordRange result)
    {
        var headI = 0;
        // antichar flag
        var aF = true;
        // escaping flag
        byte eF = 0;
        // have escaped flag
        var heF = false;

        for (int i = 0; i < _string.Length; i++)
        {
            // エスケープ外
            if (eF == 0)
            {
                // エスケープ文字を更新し、
                eF = GetEscapingCode(of: _string[i]);
                // それが零であることか判断しておき、
                var eF_ = eF == 0;
                // エスケープフラグを更新する。
                heF |= !eF_;

                // もし語間ならば、
                if (eF_ && BasicUtils.IsSeparator(_string[i]))
                {
                    // 現在の語を代入してみる。
                    result = new(headI, i, heF);

                    // これが指定の語ならば返す。返せば二度目は回ってこないので空白の連続を考える必要がない。
                    if (result.SpanEquals(_string, of)) return true;

                    // 前字フラグを立てる。
                    aF = true;

                    // エスケープフラグを下す。
                    heF = false;
                }
                // もし語頭ならば、
                else if (aF)
                {
                    // ヘッダを更新する。
                    headI = i;

                    // 前字フラグを下す。
                    aF = false;
                }
                // もし語中ならば、
                else
                {

                }
            }
            // エスケープ内
            else
            {
                // もし記号が前のエスケープ記号と同じならば、
                if (GetEscapingCode(of: this._string[i]) == eF)
                {
                    // エスケープを解く。
                    eF = 0;
                }
            }
        }

        // 最後の語でも前が語間でなければ(←語間で返すから)返す。
        if (!aF)
        {
            // 現在の語を代入してみる。
            result = new(headI, Size, heF);

            // これが指定の語ならば返す。
            if (result.SpanEquals(_string, of)) return true;
        }

        result = default;
        return false;
    }

    // 番目から結果を得る。

    public bool Contains(int number)
    {
        var count = 0;
        // antichar flag
        var aF = true;
        // escaping flag
        byte eF = 0;

        for (int i = 0; i < _string.Length; i++)
        {
            // エスケープ外
            if (eF == 0)
            {
                // エスケープ文字を更新し、
                eF = GetEscapingCode(of: _string[i]);

                // もしエスケープ記号でなく語間ならば、
                if (eF == 0 && BasicUtils.IsSeparator(_string[i]))
                {
                    // 前字フラグを立てる。
                    aF = true;
                }
                // もし語頭ならば、
                else if (aF)
                {
                    // 語を数え、
                    if (count++ == number) return true;

                    // 前字フラグを下す。
                    aF = false;
                }
                // もし語中ならば、
                else
                {

                }
            }
            // エスケープ内
            else
            {
                // もし記号が前のエスケープ記号と同じならば、
                if (GetEscapingCode(of: _string[i]) == eF)
                {
                    // エスケープを解く。
                    eF = 0;
                }
            }
        }

        return false;
    }

    public WordRange GetRange(int number, int index = 0) => GetRangeFromIndex(GetIndex(number, index));
    public bool TryGetRange(int number, int index, [NotNullWhen(true)] out WordRange result)
    {
        if (TryGetIndex(number, index, out var result_))
        {
            result = GetRangeFromIndex(result_);
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }

    public int GetIndex(int number, int index = 0) => TryGetIndex(number, index, out var result) ? result : throw new IndexOutOfRangeException();
    public bool TryGetIndex(int number, int index, [NotNullWhen(true)] out int result)
    {
        var count = 0;
        // antichar flag
        var aF = true;
        // escaping flag
        byte eF = 0;

        for (int i = index; i < _string.Length; i++)
        {
            // エスケープ外
            if (eF == 0)
            {
                // エスケープ文字を更新し、
                eF = GetEscapingCode(of: _string[i]);

                // もし語間ならば、
                if (eF == 0 && BasicUtils.IsSeparator(_string[i]))
                {
                    // 前字フラグを立てる。
                    aF = true;
                }
                // もし語頭ならば、
                else if (aF)
                {
                    if (count++ == number)
                    {
                        result = i;
                        return true;
                    }

                    // 前字フラグを下す。
                    aF = false;
                }
                // もし語中ならば、
                else
                {

                }
            }
            // エスケープ内
            else
            {
                // もし記号が前のエスケープ記号と同じならば、
                if (GetEscapingCode(of: this._string[i]) == eF)
                {
                    // エスケープを解く。
                    eF = 0;
                }
            }
        }

        result = -1;
        return false;
    }

    // インデックスから結果を得る。

    public int GetNumberFromIndex(int index)
    {
        var count = 0;
        // antichar flag
        var aF = true;
        // escaping flag
        byte eF = 0;

        for (int i = 0; i < _string.Length; i++)
        {
            // エスケープ外
            if (eF == 0)
            {
                // エスケープ文字を更新し、
                eF = GetEscapingCode(of: _string[i]);

                // もしエスケープ記号でなく語間ならば、
                if (eF == 0 && BasicUtils.IsSeparator(_string[i]))
                {
                    // 前字フラグを立てる。
                    aF = true;
                }
                // もし語頭ならば、
                else if (aF)
                {
                    // 指定位置を過ぎた場合は数を返す。
                    if (i <= index) return count;

                    // 語を数え、
                    count++;

                    // 前字フラグを下す。
                    aF = false;
                }
                // もし語中ならば、
                else
                {

                }
            }
            // エスケープ内
            else
            {
                // もし記号が前のエスケープ記号と同じならば、
                if (GetEscapingCode(of: _string[i]) == eF)
                {
                    // エスケープを解く。
                    eF = 0;
                }
            }
        }

        throw new ArgumentOutOfRangeException(nameof(index));
    }

    public WordRange GetRangeFromIndex(int index)
    {
        // escaping flag
        byte eF = 0;
        // has escaped flag
        var hEF = false;

        for (int i = index; i < _string.Length; i++)
        {
            // エスケープ外
            if (eF == 0)
            {
                // エスケープ文字を更新し、
                eF = GetEscapingCode(of: _string[i]);
                var eF_ = eF == 0;
                hEF |= !eF_;

                // もしエスケープ記号でなく語間ならば、
                if (eF_ && BasicUtils.IsSeparator(_string[i]))
                {
                    return new(index, i, hEF);
                }
            }
            // エスケープ内
            else
            {
                // もし記号が前のエスケープ記号と同じならば、
                if (GetEscapingCode(of: _string[i]) == eF)
                {
                    // エスケープを解く。
                    eF = 0;
                }
            }
        }

        // 最後の要素でも前が語間でなければ(←語間で返すから)返す。
        return new(index, Size, hEF);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is WordList other && Equals(other);
    /// <inheritdoc/>
    public bool Equals(WordSpan other)
    {
        int tI = 0;
        int oI = 0;
        byte tEF = 0;
        byte oEF = 0;
        bool cF;

        while (tI < this._string.Length && oI < other._string.Length)
        {
            cF = true;

            // エスケープ外
            if (tEF == 0)
            {
                // エスケープ文字を更新し、
                tEF = GetEscapingCode(of: this._string[tI]);

                if (tEF != 0 || BasicUtils.IsSeparator(this._string[tI]))
                {
                    tI++;
                    cF = false;
                }
            }
            // エスケープ内
            else
            {
                if (GetEscapingCode(of: this._string[tI]) == tEF)
                {
                    tI++;
                    cF = false;
                    tEF = 0;
                }
            }

            // エスケープ外
            if (oEF == 0)
            {
                // エスケープ文字を更新し、
                oEF = GetEscapingCode(of: other._string[oI]);

                if (oEF != 0 || BasicUtils.IsSeparator(other._string[oI]))
                {
                    oI++;
                    cF = false;
                }
            }
            // エスケープ内
            else
            {
                if (GetEscapingCode(of: other._string[oI]) == oEF)
                {
                    oI++;
                    cF = false;
                    oEF = 0;
                }
            }

            if (cF)
            {
                if (this._string[tI++] != other._string[oI++]) return false;
            }
        }

        return tI == this._string.Length && oI == other._string.Length;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int r = 0;
        var lastIsSeparator = true;
        var oflag = true;

        for (int i = 0; i < _string.Length; i++)
        {
            if (oflag && BasicUtils.IsSeparator(_string[i]))
            {
                lastIsSeparator = true;
            }
            else if (BasicUtils.IsParenthesis(_string[i]))
            {
                oflag = !oflag;
            }
            else
            {
                if (lastIsSeparator) r ^= _string[i].GetHashCode();
                lastIsSeparator = false;
            }
        }

        return r;
    }

    /// <summary>
    /// 原文を取得します。
    /// </summary>
    /// <returns>
    /// 原文。
    /// </returns>
    public override string ToString() => new(_string);

    // A'B"C'D E'F"G'H'I"J'K
    // "A'B"'"'"C'D E'F"'"'"G'H'I"'"'"J'K"
    public static string Escape(ReadOnlySpan<char> unescapedWord, bool escapeSeparator = false)
    {
        var builder = new StringBuilder();
        var rest = unescapedWord;

        for (int i = 0; i < rest.Length;)
        {
            switch (GetEscapingCode(of: rest[i]))
            {
            case 0:
                if (escapeSeparator && BasicUtils.IsSeparator(rest[i]))
                {
                    _ = builder.Append(rest[..i]);
                    _ = builder.Append('\'');
                    _ = builder.Append(rest[i]);
                    _ = builder.Append('\'');
                    if (rest.Length == i + 1) goto fin;
                    rest = rest[(i + 1)..];
                    i = 0;
                    continue;
                }
                break;
            case 1:
                _ = builder.Append(rest[..i]);
                _ = builder.Append("'\"'");
                if (rest.Length == i + 1) goto fin;
                rest = rest[(i + 1)..];
                i = 0;
                continue;
            case 2:
                _ = builder.Append(rest[..i]);
                _ = builder.Append("\"'\"");
                if (rest.Length == i + 1) goto fin;
                rest = rest[(i + 1)..];
                i = 0;
                continue;
            }

            i++;
        }

        _ = builder.Append(rest);

        fin: return builder.ToString();
    }
    public static string Unescape(ReadOnlySpan<char> escapedWord)
    {
        var builder = new StringBuilder();
        var headI = 0;
        byte eF = 0;

        for (int i = 0; i < escapedWord.Length; i++)
        {
            // エスケープ外
            if (eF == 0)
            {
                eF = GetEscapingCode(of: escapedWord[i]);

                if (eF != 0)
                {
                    _ = builder.Append(escapedWord[headI..i]);
                    headI = i + 1;
                }
            }
            // エスケープ内
            else
            {
                if (GetEscapingCode(of: escapedWord[i]) == eF)
                {
                    _ = builder.Append(escapedWord[headI..i]);
                    headI = i + 1;

                    eF = 0;
                }
            }
        }

        _ = builder.Append(escapedWord[headI..]);

        return builder.ToString();
    }

    [Obsolete("`WordRange`構造体に同じ機能のメソッドが追加されました。")]
    static bool WordEquals(ReadOnlySpan<char> escapedWord, ReadOnlySpan<char> comparedWord, bool haveEscapedFlag = true)
    {
        if (haveEscapedFlag)
        {
            int cWI = 0;
            byte eF = 0;

            for (int eWI = 0; eWI < escapedWord.Length; eWI++)
            {
                // エスケープ外
                if (eF == 0)
                {
                    // エスケープ文字を更新し、
                    eF = GetEscapingCode(of: escapedWord[eWI]);

                    // もしエスケープ記号ならば進める。
                    if (eF != 0) continue;

                    if (cWI < comparedWord.Length)
                    {
                        // 文字が異なる場合は偽を返す。
                        if (escapedWord[eWI] != comparedWord[cWI]) return false;
                    }
                    // 比較される語の長さを超える場合は偽を返す。
                    else
                    {
                        return false;
                    }

                    cWI++;
                }
                // エスケープ内
                else
                {
                    // もし記号が前のエスケープ記号と同じならば、
                    if (GetEscapingCode(of: escapedWord[eWI]) == eF)
                    {
                        // エスケープを解く。
                        eF = 0;
                    }
                }
            }

            // 最後の長さまで一致していたら真、でなければ偽。
            return cWI == comparedWord.Length;
        }
        else
        {
            return escapedWord.SequenceEqual(comparedWord);
        }
    }

    public static bool operator ==(WordSpan left, WordSpan right) => left.Equals(right);
    public static bool operator !=(WordSpan left, WordSpan right) => !left.Equals(right);
}

public readonly struct WordRange
{
    public bool IsValid => 0 <= Length && 0 <= Start;
    public int Length => End - Start;
    public bool HasEscaped { get; init; }
    public int Start { get; init; }
    public int End { get; init; }

    public WordRange(int start, int end, bool hasEscaped = true)
    {
        Start = start;
        End = end;
        HasEscaped = hasEscaped;
    }

    public WordRange GetLeadingRange(int offset) => new(Start + offset, End, HasEscaped);
    public WordRange GetTrailingRange(int offset) => new(Start, End - offset, HasEscaped);
    public WordRange GetRange(int i, int j) => new(Start + i, End - j, HasEscaped);

    public WordRange CloneWithStartIs(int start) => new(start, End, HasEscaped);
    public WordRange CloneWithEndIs(int end) => new(Start, end, HasEscaped);

    /// <summary>
    /// 文字区間の部分がエスケープを考慮した場合に別の文字区間と等価であるかを判定します。
    /// </summary>
    /// <remarks>
    /// 例えば<see cref="WordRange"/>型の[[0..10](maybe escaped)]に対して<c>A"B"C"D`"EFG</c>の区間と<c>AB""CD"`"E</c>は等価です。
    /// </remarks>
    /// <param name="of">部分を比較する文字区間。</param>
    /// <param name="to">比較される文字区間。</param>
    /// <returns>等価である場合は`true`、そうで無い場合は`false`。</returns>
    public bool SpanEquals(ReadOnlySpan<char> of, ReadOnlySpan<char> to)
    {
        if (HasEscaped)
        {
            int cWI = 0;
            byte eF = 0;

            for (int eWI = Start; eWI < End; eWI++)
            {
                // エスケープ外
                if (eF == 0)
                {
                    // エスケープ文字を更新し、
                    eF = GetEscapingCode(of: of[eWI]);

                    // もしエスケープ記号ならば進める。
                    if (eF != 0) continue;

                    if (cWI < to.Length)
                    {
                        // 文字が異なる場合は偽を返す。
                        if (of[eWI] != to[cWI]) return false;
                    }
                    // 比較される語の長さを超える場合は偽を返す。
                    else
                    {
                        return false;
                    }

                    cWI++;
                }
                // エスケープ内
                else
                {
                    // もし記号が前のエスケープ記号と同じならば、
                    if (GetEscapingCode(of: of[eWI]) == eF)
                    {
                        // エスケープを解く。
                        eF = 0;
                    }
                }
            }

            // 最後の長さまで一致していたら真、でなければ偽。
            return cWI == to.Length;
        }
        else
        {
            return of[Start..End].SequenceEqual(to);
        }
    }

    public override string ToString() => $"{Start}..{End}({(HasEscaped ? "maybe escaped" : "is simple")})";

    public static explicit operator Range(WordRange wordRange) => new(wordRange.Start, wordRange.End);
}

partial class Utils
{
    public static string ToEscaped(this string @this)
    {
        var builder = new StringBuilder();
        @this.WriteEscaped(to: builder);
        return builder.ToString();
    }
    public static void WriteEscaped(this string @this, StringBuilder to)
    {
        if (!@this.Any(x => BasicUtils.IsSeparator(x)))
        {
            _ = to.Append(@this);
        }
        else if (!@this.Any(x => x is '\"' or '\''))
        {
            _ = to.Append('\"').Append(@this).Append('\"');
        }
        else
        {
            foreach (var @char in @this)
            {
                if (BasicUtils.IsSeparator(@char) || @char == '\'')
                {
                    _ = to.Append('\"');
                    _ = to.Append(@char);
                    _ = to.Append('\"');
                }
                else if (@char == '\"')
                {
                    _ = to.Append('\'');
                    _ = to.Append(@char);
                    _ = to.Append('\'');
                }
                else
                {
                    _ = to.Append(@char);
                }
            }
        }
    }

    public static byte GetEscapingCode(char of) => of switch { '\"' => 1, '\'' => 2, _ => 0 };
}
