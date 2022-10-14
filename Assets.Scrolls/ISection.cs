using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Nonno.Assets.Collections;
using MI = System.Runtime.CompilerServices.MethodImplAttribute;
using MIO = System.Runtime.CompilerServices.MethodImplOptions;

namespace Nonno.Assets.Scrolls;

/*
 * sectionとは巻子における節のこと。Noteで使うことが多い。インターフェース化が必ずしも要るわけではないと思うのだが、まとめやすいのでそうしている。
 */

public interface ISection : IDisposable, IAsyncDisposable
{
    SectionMode Mode { get; set; }
    long Length { get; }
    int Number { get; set; }
    void Flush();
    Task FlushAsync();
    void Delete();
    Task DeleteAsync();
    void Read(Span<byte> memory);
    Task ReadAsync(Memory<byte> memory);
    void Write(Span<byte> memory);
    Task WriteAsync(Memory<byte> memory);
}

public enum SectionMode : sbyte
{
    Read = -1,
    Idle = 0,
    Write = 1,
}

public class MemorySection : ISection
{
    readonly List<byte> _list;
    int _index;

    public IEnumerable<byte> Memory => _list;
    public SectionMode Mode { get; set; }
    long ISection.Length => Length;
    public int Length => _list.Count - _index;
    public int Number { get; set; }

    public MemorySection(int defaultCpacity = 0x10)
    {
        _list = new(defaultCpacity);
    }
    public MemorySection(MemorySection original)
    {
        _list = new(original._list.ToArray());
        _index = original._index;
    }

    public void Delete() { }
    public Task DeleteAsync() => Task.CompletedTask;

    public void Flush() { }
    public Task FlushAsync() => Task.CompletedTask;

    [MI(MIO.AggressiveInlining)]
    public void Read(Span<byte> memory)
    {
        for (int i = 0; i < memory.Length; i++) memory[i] = _list[_index++];
    }
    [MI(MIO.AggressiveInlining)]
    public Task ReadAsync(Memory<byte> memory)
    {
        Read(memory.Span);
        return Task.CompletedTask;
    }

    [MI(MIO.AggressiveInlining)]
    public void Write(Span<byte> memory)
    {
        _list.AddRange(memory.ToArray());
    }
    [MI(MIO.AggressiveInlining)]
    public Task WriteAsync(Memory<byte> memory)
    {
        Write(memory.Span);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing) { }
    public async ValueTask DisposeAsync()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        await DisposeAsync(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual ValueTask DisposeAsync(bool disposing) => ValueTask.CompletedTask;
}



//public sealed class FooterSection : ISection
//{
//    SectionMode _mode;

//    public AccessorIndex Index { get; }
//    SectionMode ISection.Mode
//    {
//        get => _mode;
//        set
//        {
//            if (value != SectionMode.Idle && value != SectionMode.Read) throw new ArgumentException("末節に書き込むことはできません。", nameof(value));
//            _mode = value;
//        }
//    }
//    int ISection.Number { get; set; }

//    public FooterSection(AccessorIndex index) => Index = index;

//    void ISection.Close() { }
//    ValueTask ISection.CloseAsync() => ValueTask.CompletedTask;
//    void ISection.Delete() { }
//    ValueTask ISection.DeleteAsync() => ValueTask.CompletedTask;
//    void IDisposable.Dispose() { }
//    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;
//    long ISection.GetLength() => 0;
//    void ISection.Read(Span<byte> memory) => throw new IOException("索引の位置を越えて削除することはできません。");
//    Task ISection.ReadAsync(Memory<byte> memory) => throw new IOException("索引の位置を越えて削除することはできません。");
//    void ISection.Write(Span<byte> memory) => throw new InvalidOperationException("対応するモードが異なります。正しく設定してください。");
//    Task ISection.WriteAsync(Memory<byte> memory) => throw new InvalidOperationException("対応するモードが異なります。正しく設定してください。");
//}

public abstract class StreamSection : ISection
{
    Header _header;
    Stream? _stream;
    SectionMode _mode;
    bool _isDisposed;

    public SectionMode Mode
    {
        get => _mode;
        set
        {
            if (_isDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (_mode == value) return;

            switch (_mode)
            {
            case SectionMode.Read:
                {
                    if (_stream == null) throw new Exception();

                    _header.startPosition = _stream.Position;

                    break;
                }
            case SectionMode.Write:
                {
                    if (_stream == null) throw new Exception();

                    _header.endPosition = _stream.Position;

                    break;
                }
            }
            _mode = value;

            switch (value)
            {
            case SectionMode.Read:
                {
                    _stream ??= GetStream();

                    _stream.Position = _header.startPosition;

                    break;
                }
            case SectionMode.Idle:
                break;
            case SectionMode.Write:
                {
                    _stream ??= GetStream();

                    _stream.Position = _header.endPosition;

                    break;
                }
            default:
                throw new UndefinedEnumerationValueException(nameof(value), typeof(SectionMode));
            }
        }
    }
    public long Length => _mode switch
    {
        SectionMode.Read => _header.endPosition - _stream!.Position,
        SectionMode.Idle => _header.endPosition - _header.startPosition,
        SectionMode.Write => _stream!.Position - _header.startPosition,
        _ => throw new Exception("不明なエラーです。"),
    };
    public int Number
    {
        get => _header.number;
        set => _header.number = value;
    }
    internal bool IsDisposed => _isDisposed;
    protected Stream? Stream
    {
        get => _stream;
        set
        {
            _stream = value;

            if (value != null)
            {
                value.Position = 0;
                Span<byte> span = stackalloc byte[Header.SIZE];
                if (value.Read(span) != span.Length) throw new ArgumentException("指定されたストリームにヘッダーが存在しませんでした。", nameof(value));

                _header = span.ToStruct<byte, Header>().Checked();
            }
        }
    }

    public StreamSection() { }
    protected StreamSection(StreamSection original)
    {
        _header = original._header;
        _mode = original._mode;
        _isDisposed = original._isDisposed;
    }

    [MethodImpl(MIO.AggressiveInlining)]
    public async Task ReadAsync(Memory<byte> memory)
    {
#if DEBUG
        if (_mode != SectionMode.Read) throw new InvalidOperationException("対応するモードが異なります。正しく設定してください。");
        if (_stream == null) throw new Exception("不明なエラーです。`_mode == SectionMode.Read`である時にストリームが`null`でした。著者は早急に対処してください。");
#else
        _stream = _stream!;
#endif
        try
        {
            int read = await _stream.ReadAsync(memory);
            if (read != memory.Length) throw new IOException("索引の位置を越えて削除することはできません。");
        }
        catch (ObjectDisposedException e)
        {
            if (_isDisposed) throw new ObjectDisposedException($"破棄された節に操作を行おうとしました。", e);
            else throw new Exception("不明なエラーです。破棄されていない節のストリームが`null`でした。");
        }
    }
    [MethodImpl(MIO.AggressiveInlining)]
    public void Read(Span<byte> memory)
    {
#if DEBUG
        if (_mode != SectionMode.Read) throw new InvalidOperationException("対応するモードが異なります。正しく設定してください。");
        if (_stream == null) throw new Exception("不明なエラーです。`_mode == SectionMode.Read`である時にストリームが`null`でした。著者は早急に対処してください。");
#else
        _stream = _stream!;
#endif
        try
        {
            int read = _stream.Read(memory);
            if (read != memory.Length) throw new IOException("索引の位置を越えて削除することはできません。");
        }
        catch (ObjectDisposedException e)
        {
            if (_isDisposed) throw new ObjectDisposedException($"破棄された節に操作を行おうとしました。", e);
            else throw new Exception("不明なエラーです。破棄されていない節のストリームが`null`でした。");
        }
    }

    [MethodImpl(MIO.AggressiveInlining)]
    public async Task WriteAsync(Memory<byte> memory)
    {
#if DEBUG
        if (_mode != SectionMode.Write) throw new InvalidOperationException("対応するモードが異なります。正しく設定してください。");
        if (_stream == null) throw new Exception("不明なエラーです。`_mode == SectionMode.Write`である時にストリームが`null`でした。著者は早急に対処してください。");
#else
        _stream = _stream!;
#endif
        try
        {
            await _stream.WriteAsync(memory);
        }
        catch (ObjectDisposedException e)
        {
            if (_isDisposed) throw new ObjectDisposedException($"破棄された節に操作を行おうとしました。", e);
            else throw new Exception("不明なエラーです。破棄されていない節のストリームが`null`でした。");
        }
    }
    [MethodImpl(MIO.AggressiveInlining)]
    public void Write(Span<byte> memory)
    {
#if DEBUG
        if (_mode != SectionMode.Write) throw new InvalidOperationException("対応するモードが異なります。正しく設定してください。");
        if (_stream == null) throw new Exception("不明なエラーです。`_mode == SectionMode.Write`である時にストリームが`null`でした。著者は早急に対処してください。");
#else
        _stream = _stream!;
#endif
        try
        {
            _stream.Write(memory);
        }
        catch (ObjectDisposedException e)
        {
            if (_isDisposed) throw new ObjectDisposedException($"破棄された節に操作を行おうとしました。", e);
            else throw new Exception("不明なエラーです。破棄されていない節のストリームが`null`でした。");
        }
    }

    public void Close()
    {
        if (Mode == SectionMode.Read || Mode == SectionMode.Write) throw new InvalidOperationException("節が読込み又は書込みの最中であるときに節を閉じることはできません。");
        if (_isDisposed) throw new ObjectDisposedException(GetType().FullName);
        if (_stream == null) return;

        _stream.Position = 0;
        _stream.Write(_header.ToSpan<Header, byte>());
        _stream.Dispose();
        _stream = null;
    }
    public ValueTask CloseAsync()
    {
        if (Mode == SectionMode.Read || Mode == SectionMode.Write) throw new InvalidOperationException("節が読込み又は書込みの最中であるときに節を閉じることはできません。");
        if (_isDisposed) throw new ObjectDisposedException(GetType().FullName);
        if (_stream == null) return ValueTask.CompletedTask;

        _stream.Position = 0;
        _stream.Write(_header.ToSpan<Header, byte>());
        var stream = _stream;
        _stream = null;
        return stream.DisposeAsync();
    }

    public void Flush()
    {
        if (_stream == null) return;
        if (_isDisposed) throw new ObjectDisposedException(GetType().FullName);

        var mode = Mode;
        Mode = SectionMode.Idle;
        _stream.Position = 0;
        _stream.Write(_header.ToSpan<Header, byte>());
        _stream.Flush();
        Mode = mode;
    }
    public async Task FlushAsync()
    {
        if (_stream == null) return;
        if (_isDisposed) throw new ObjectDisposedException(GetType().FullName);

        var mode = Mode;
        Mode = SectionMode.Idle;
        _stream.Position = 0;
        _stream.Write(_header.ToSpan<Header, byte>());
        await _stream.FlushAsync();
        Mode = mode;
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
                Mode = SectionMode.Idle;
                Close();
            }

            // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
            // TODO: 大きなフィールドを null に設定します
            _isDisposed = true;
        }
    }
    public async ValueTask DisposeAsync()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        await DisposeAsync(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                Mode = SectionMode.Idle;
                await CloseAsync();
            }

            // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
            // TODO: 大きなフィールドを null に設定します
            _isDisposed = true;
        }
    }

    public abstract void Delete();
    public abstract Task DeleteAsync();

    /// <summary>
    /// 節と数據の接続を撤退します。
    /// </summary>
    public void Withdraw()
    {
        if (Mode == SectionMode.Idle) Stream?.Close();
    }

    protected abstract Stream GetStream();

    [StructLayout(LayoutKind.Explicit, Size = SIZE)]
    public struct Header
    {
        public const int SIZE = 0x20;
        public const char TOKEN = '蓮';

        [FieldOffset(0x00)]
        public long startPosition;
        [FieldOffset(0x08)]
        public long endPosition;
        [FieldOffset(0x10)]
        public int number;
        [FieldOffset(0x14)]
        public char token = TOKEN;

        public bool IsValid => token == TOKEN;

        public Header(long startPosition, long endPosition, int number)
        {
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.number = number;
        }
        public Header(int number)
        {
            startPosition = SIZE;
            endPosition = SIZE;
            this.number = number;
        }

        public Header Checked()
        {
            if (token != TOKEN) throw new AuthenticationException(token, TOKEN);
            return this;
        }
    }
}

public class FileSection : StreamSection
{
    public const string EXTENSION = ".sect";

    readonly FileInfo _fileInfo;

    public FileInfo FileInfo => _fileInfo;

    /// <summary>
    /// 既存のファイルから節を作成します。
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <exception cref="ArgumentException"></exception>
    public FileSection(FileInfo fileInfo)
    {
        if (!fileInfo.Exists || fileInfo.Extension != EXTENSION) throw new ArgumentException("存在しないか拡張子の不適切なファイルが指定されました。", nameof(fileInfo));

        _fileInfo = fileInfo;

        Stream = fileInfo.Open(FileMode.Open, FileAccess.ReadWrite);
    }
    private FileSection(FileInfo fileInfo, FileStream stream)
    {
        if (!fileInfo.Exists || fileInfo.Extension != EXTENSION) throw new ArgumentException("存在しないか拡張子の不適切なファイルが指定されました。", nameof(fileInfo));
        if (fileInfo.FullName != stream.Name) throw new ArgumentException("異なるファイルストリームが渡されました。", nameof(stream));

        _fileInfo = fileInfo;

        Stream = stream;
    }

    public override void Delete()
    {
        var fileInfo = _fileInfo;
        Dispose();
        fileInfo.Delete();
    }
    public override async Task DeleteAsync()
    {
        var fileInfo = _fileInfo;
        await DisposeAsync();
        fileInfo.Delete();
    }

    protected override FileStream GetStream() => FileInfo.Open(FileMode.Open, FileAccess.ReadWrite);

    public static FileSection CreateSection(FileInfo fileInfo, int number)
    {
        if (fileInfo.Exists || fileInfo.Extension != EXTENSION) throw new ArgumentException("既に存在するか拡張子の不適切なファイルが指定されました。", nameof(fileInfo));

        var stream = fileInfo.Create();
        stream.Write(new Header(number).ToSpan<Header, byte>());
        return new FileSection(fileInfo, stream);
    }
}

public class ZipArchiveSection : StreamSection
{
    readonly ZipArchiveEntry _entry;

    public ZipArchiveEntry Entry => _entry;

    public ZipArchiveSection(ZipArchiveEntry entry)
    {
        _entry = entry;

        Stream = entry.Open();
    }
    private ZipArchiveSection(ZipArchiveEntry entry, Stream stream)
    {
        _entry = entry;

        Stream = stream;
    }

    public override void Delete()
    {
        var entry = _entry;
        Dispose();
        entry.Delete();
    }
    public override async Task DeleteAsync()
    {
        var entry = _entry;
        await DisposeAsync();
        entry.Delete();
    }

    protected override Stream GetStream() => _entry.Open();

    public static ZipArchiveSection CreateSection(ZipArchiveEntry entry, int number)
    {
        var stream = entry.Open();
        stream.Position = 0;
        stream.Write(new Header(number).ToSpan<Header, byte>());
        return new ZipArchiveSection(entry, stream);
    }
}
