using System.Data.Common;
using System.Data.SqlTypes;
using System.Reflection;
using static Nonno.Assets.Sample;
using System.IO;
using SPath = System.IO.Path;
using System.Runtime.Loader;

namespace Nonno.Assets.Collections;

/// <summary>
/// 設定を表します。
/// </summary>
public class Configuration : IDisposable
{
    internal const string UNACCOUNTED_BASE_PATH_WORD = "?:\\";

    readonly WordList _dictionary;
    readonly ListDictionary<string, string> _changes;
    bool _isDisposed;

    public string Path { get; }
    public string DirectoryPath => SPath.GetPathRoot(Path)!;

    /// <summary>
    /// 設定の値を取得、または設定します。
    /// <para>
    /// 設定の値はドメインが同一である限り不変で、設定した値は次の起動時に初めて反映されます。
    /// </para>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Value this[string name]
    {
        get
        {
            foreach (var word in _dictionary)
            {
                var length = name.Length;
                if (word.Length >= length + 1 && word.AsSpan()[..length].SequenceEqual(name) && word[length] == '=') return new(word[(length + 1)..]) { BasePath = Path };
            }

            return Value.EMPTY;
        }
        set
        {
            if (value.BasePath is not null)
            {
                if (value.BasePath == UNACCOUNTED_BASE_PATH_WORD) value = SPath.GetRelativePath(DirectoryPath, value);
                else if (value.BasePath != Path) value = SPath.GetRelativePath(DirectoryPath, SPath.GetFullPath(value, value.BasePath));
            }

            if (value._value.Contains('\"')) value = !value._value.Contains('\'') ? $"`{value}`" : throw new NotImplementedException("現在、ダブルクオーテーションとクオーテーションを共に含む文字列を設定の鍵として追加することはできません。");
            else if (value._value.Contains('\'')) value = $"\"{value}\"";

            if (!_changes.TrySetValue(name, value)) _changes.Add(name, value);
        }
    }

    public Configuration(string path)
    {
        if (File.Exists(path)) _dictionary = new(File.ReadAllText(path));
        else _dictionary = new();

        _changes = new();

        Path = path;
    }

    public void Dispose()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
            }

            using (var writer = new StreamWriter(new FileStream(Path, FileMode.Create)))
            {
                var span = _dictionary.AsSpan();
                int end = 0;

                while (true)
                {
                    // WordSpan列挙の定型文。
                    if (span.TryGetRange(0, end, out var result))
                    {
                        // 書かれている形そのままで取得。
                        var wordSpan = span.GetWordAsSpan(result);
                        // 実際の値を取得。
                        var word = WordSpan.GetWord(wordSpan);
                        // 実際の値におけるセパレータの位置を取得。
                        var index = word.IndexOf("=");
                        // 名前を取得。
                        var name = word[..index];
                        // 名前が変更されているならば、値は後々の検索の邪魔になるので削除して、
                        if (_changes.TryRemove(name, out var value))
                        {
                            // 値が空の場合、
                            if (value.Length == 0)
                            {
                                // 無視して書き込まず飛ばす。
                                continue;
                            }
                            // 値がある場合、
                            else
                            {
                                // 形そのままで名前を書き込む。
                                writer.Write(wordSpan[..wordSpan.IndexOf('=')]);
                                writer.Write("=");
                                writer.WriteLine(value);
                            }
                        }
                        else
                        {
                            writer.WriteLine(wordSpan);
                        }

                        // WordSpan列挙の定型文。
                        end = result.End;
                    }
                    else
                    {
                        break;
                    }
                }

                // `_changes`に残っている値は追加する値の場合。
                foreach (var (name, value) in _changes)
                {
                    if (name.Contains('\"'))
                    {
                        if (name.Contains('\''))
                        {
                            throw new NotImplementedException("現在、ダブルクオーテーションとクオーテーションを共に含む文字列を設定の鍵として追加することはできません。");
                        }
                        else
                        {
                            writer.Write('\'');
                            writer.Write(name);
                            writer.Write('\'');
                        }
                    }
                    else if (name.Contains('\''))
                    {
                        writer.Write('\"');
                        writer.Write(name);
                        writer.Write('\"');
                    }
                    else
                    {
                        writer.Write(name);
                    }
                    writer.Write('=');
                    writer.WriteLine(value);
                }
            }

            _isDisposed = true;
        }
    }

    public static Configuration Default { get; }

    static Configuration()
    {
        var asm = Assembly.GetEntryAssembly();
        if (asm is null) Default = new($"{Environment.CurrentDirectory}\\.cfg");
        else Default = new(asm.Location + ".cfg");

        AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly())!.Unloading += _ => Default.Dispose();
    }

    /// <summary>
    /// 設定の値を表します。
    /// </summary>
    public readonly struct Value
    {
        public static readonly Value EMPTY = new();

        internal readonly string _value;

        public bool IsEmpty => _value == null || _value.Length == 0;
        /// <summary>
        /// 値が相対パスであるとき、その基準パスを置きます。
        /// </summary>
        public string? BasePath { get; init; } = default;

        internal Value(string value) => _value = value;

        public static implicit operator string(Value value) => value._value ?? string.Empty;
        public static explicit operator bool?(Value value)
        {
            if (value.IsEmpty) return null;
            if (value._value[0] is 'T' or 't') return true;
            if (value._value[0] is 'F' or 'f') return false;
            return default;
        }
        public static explicit operator byte?(Value value) => value.IsEmpty ? null : byte.TryParse(value._value, out var result) ? result : null;
        public static explicit operator sbyte?(Value value) => value.IsEmpty ? null : sbyte.TryParse(value._value, out var result) ? result : null;
        public static explicit operator short?(Value value) => value.IsEmpty ? null : short.TryParse(value._value, out var result) ? result : null;
        public static explicit operator ushort?(Value value) => value.IsEmpty ? null : ushort.TryParse(value._value, out var result) ? result : null;
        public static explicit operator int?(Value value) => value.IsEmpty ? null : int.TryParse(value._value, out var result) ? result : null;
        public static explicit operator uint?(Value value) => value.IsEmpty ? null : uint.TryParse(value._value, out var result) ? result : null;
        public static explicit operator long?(Value value) => value.IsEmpty ? null : long.TryParse(value._value, out var result) ? result : null;
        public static explicit operator ulong?(Value value) => value.IsEmpty ? null : ulong.TryParse(value._value, out var result) ? result : null;
        public static explicit operator float?(Value value) => value.IsEmpty ? null : float.TryParse(value._value, out var result) ? result : null;
        public static explicit operator double?(Value value) => value.IsEmpty ? null : double.TryParse(value._value, out var result) ? result : null;
        public static explicit operator decimal?(Value value) => value.IsEmpty ? null : decimal.TryParse(value._value, out var result) ? result : null;
        public static explicit operator DateTime?(Value value) => value.IsEmpty ? null : DateTime.TryParse(value._value, out var result) ? result : null;
        public static explicit operator TimeSpan?(Value value) => value.IsEmpty ? null : TimeSpan.TryParse(value._value, out var result) ? result : null;
        public static explicit operator Guid?(Value value) => value.IsEmpty ? null : new(value._value);
        public static explicit operator Uri?(Value value) => value.IsEmpty ? null : new(value._value);
        public static explicit operator Type?(Value value) => value.IsEmpty ? null : Type.GetType(value._value);
        public static explicit operator FileInfo?(Value value)
        {
            if (value.BasePath == null) return default;
            if (value.BasePath == UNACCOUNTED_BASE_PATH_WORD) return new FileInfo(value._value);
            return new FileInfo(SPath.GetFullPath(value._value, value.BasePath));
        }
        public static explicit operator DirectoryInfo?(Value value)
        {
            if (value.BasePath == null) return default;
            if (value.BasePath == UNACCOUNTED_BASE_PATH_WORD) return new DirectoryInfo(value._value);
            return new DirectoryInfo(SPath.GetFullPath(value._value, value.BasePath));
        }
        public static implicit operator Value(string? value) => value is not null ? new(value) : default;
        public static implicit operator Value(bool value) => new(value ? "TRUE" : "FALSE");
        public static implicit operator Value(byte value) => new(value.ToString());
        public static implicit operator Value(sbyte value) => new(value.ToString());
        public static implicit operator Value(short value) => new(value.ToString());
        public static implicit operator Value(ushort value) => new(value.ToString());
        public static implicit operator Value(int value) => new(value.ToString());
        public static implicit operator Value(uint value) => new(value.ToString());
        public static implicit operator Value(long value) => new(value.ToString());
        public static implicit operator Value(ulong value) => new(value.ToString());
        public static implicit operator Value(float value) => new(value.ToString());
        public static implicit operator Value(double value) => new(value.ToString());
        public static implicit operator Value(decimal value) => new(value.ToString());
        public static implicit operator Value(DateTime value) => new(value.ToString());
        public static implicit operator Value(TimeSpan value) => new(value.ToString());
        public static implicit operator Value(Guid value) => new(value.ToString());
        public static implicit operator Value(bool? value) => value is { } v ? new(v ? "TRUE" : "FALSE") : default;
        public static implicit operator Value(byte? value) => value is { } v ? new(v.ToString()) : default;
        public static implicit operator Value(sbyte? value) => value is { } v ? new(v.ToString()) : default;
        public static implicit operator Value(short? value) => value is { } v ? new(v.ToString()) : default;
        public static implicit operator Value(ushort? value) => value is { } v ? new(v.ToString()) : default;
        public static implicit operator Value(int? value) => value is { } v ? new(v.ToString()) : default;
        public static implicit operator Value(uint? value) => value is { } v ? new(v.ToString()) : default;
        public static implicit operator Value(long? value) => value is { } v ? new(v.ToString()) : default;
        public static implicit operator Value(ulong? value) => value is { } v ? new(v.ToString()) : default;
        public static implicit operator Value(float? value) => value is { } v ? new(v.ToString()) : default;
        public static implicit operator Value(double? value) => value is { } v ? new(v.ToString()) : default;
        public static implicit operator Value(decimal? value) => value is { } v ? new(v.ToString()) : default;
        public static implicit operator Value(DateTime? value) => value is { } v ? new(v.ToString()) : default;
        public static implicit operator Value(TimeSpan? value) => value is { } v ? new(v.ToString()) : default;
        public static implicit operator Value(Guid? value) => value is { } v ? new(v.ToString()) : default;
        public static implicit operator Value(Type? value) => new(value is null ? string.Empty : value.ToString());
        public static implicit operator Value(Uri? value) => new(value is null ? string.Empty : value.ToString());
        public static implicit operator Value(FileInfo? value)
        {
            if (value is null) return default;
            return new(value.FullName) { BasePath = UNACCOUNTED_BASE_PATH_WORD };
        }
        public static implicit operator Value(DirectoryInfo? value)
        {
            if (value is null) return default;
            return new(value.FullName) { BasePath = UNACCOUNTED_BASE_PATH_WORD };
        }
    }
}
