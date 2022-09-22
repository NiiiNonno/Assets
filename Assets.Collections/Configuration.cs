namespace Nonno.Assets.Collections;

public class Configuration : IDisposable
{
    readonly WordList _dictionary;
    readonly ListDictionary<string, string> _changes;
    bool _isDisposed;

    public string DirectoryPath { get; }

    /// <summary>
    /// 設定の値を取得、または設定します。
    /// <para>
    /// 設定の値はドメインが同一である限り不変で、設定した値は次の起動時に初めて反映されます。
    /// </para>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string this[string name]
    {
        get
        {
            foreach (var word in _dictionary)
            {
                var length = name.Length;
                if (word.Length >= length + 1 && word.AsSpan()[..length].SequenceEqual(name) && word[length] == '=') return word[(length + 1)..];
            }

            return String.Empty;
        }
        set
        {
            if (value.Contains('\"')) value = !value.Contains('\'') ? $"`{value}`" : throw new NotImplementedException("現在、ダブルクオーテーションとクオーテーションを共に含む文字列を設定の鍵として追加することはできません。");
            else if (value.Contains('\'')) value = $"\"{value}\"";

            if (!_changes.TrySetValue(name, value)) _changes.Add(name, value);
        }
    }

    public Configuration(string directoryPath)
    {
        string path = Path.Combine(directoryPath, "configuration.ini");
        if (!File.Exists(path)) using (var _ = File.Create(path)) { _dictionary = new(); }
        else _dictionary = new(File.ReadAllText(path));

        _changes = new();

        DirectoryPath = directoryPath;
    }

    // 基本的にドメインで唯一、しかも最初から最後まで存在し続けるクラスとして定義されているから、ファイナライズのコストをあまり気にする必要がない。
    ~Configuration()
    {
        Dispose(false);
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
                // TODO: マネージド状態を破棄します (マネージド オブジェクト)
            }

            using (var writer = new StreamWriter(Path.Combine(DirectoryPath, "configuration.ini"), append: false))
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

    public static Configuration Current { get; }

    static Configuration()
    {
        Current = new(Environment.CurrentDirectory);
    }
}
