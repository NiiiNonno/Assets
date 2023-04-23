using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static Nonno.Assets.Utils;

namespace Nonno.Assets.Scrolls;

public class DirectoryScroll : SectionScroll<FileSection>
{
    public const string EXTENSION_CHILD = ".sct";

    readonly Dictionary<ulong, FileSection> _loadeds;
    readonly Random _rand;

    public DirectoryInfo DirectoryInfo { get; }
    public long MaxLength { get; }
    protected override FileSection this[ulong index]
    {
        get
        {
            if (!_loadeds.TryGetValue(index, out var section))
            {
                section = new(GetFileInfo(index), MaxLength);
                _loadeds[index] = section;
            }

            return section;
        }
    }

    protected DirectoryScroll(DirectoryInfo directoryInfo, long maxLength = long.MaxValue) : base(0)
    {
        _loadeds = new Dictionary<ulong, FileSection>();
        _rand = new Random();

        DirectoryInfo = directoryInfo;
        MaxLength = maxLength;
    }

    public override IScroll Copy() => Copy(new DirectoryInfo(DirectoryInfo.FullName + DateTime.Now));
    public IScroll Copy(DirectoryInfo to, bool allowOverride = false)
    {
        foreach (var item in _loadeds.Values)
        {
            item.Mode = SectionMode.Close;
        }
        DirectoryInfo.Copy(to: to, allowOverride);
        PreviousSection.Mode = SectionMode.Write;
        NextSection.Mode = SectionMode.Read;
        return new DirectoryScroll(to, MaxLength);
    }

    protected override void CreateSection(ulong number)
    {
        var fI = GetFileInfo(number);
        Debug.Assert(fI.Exists);
        var sect = new FileSection(fI, MaxLength);
        _loadeds.Add(number, sect);
    }
    protected override void DeleteSection(ulong number)
    {
        var sect = this[number];
        sect.Dispose();
        sect.FileInfo.Delete();
    }

    protected override void Dispose(bool disposing)
    {
        Debug.Assert(_loadeds.Count != 0);

        foreach (var item in _loadeds.Values)
        {
            item.Dispose();
        }

        base.Dispose(disposing);
    }
    protected override ValueTask DisposeAsync(bool disposing)
    {
        Debug.Assert(_loadeds.Count != 0);

        foreach (var item in _loadeds.Values)
        {
            item.Dispose();
        }

        return base.DisposeAsync(disposing);
    }

    public FileInfo GetFileInfo(ulong number) => new(Path.Combine(DirectoryInfo.FullName, GetFileName(number)));

    protected override ulong FindVacantNumber(ulong? previousSectionNumber = null, ulong? nextSectionNumber = null)
    {
        while (true)
        {
            var r = (ulong)_rand.NextInt64();
            if (r == 0) continue;
            var fI = GetFileInfo(r);
            if (!fI.Exists) return r;
        }
    }

    public static string GetFileName(ulong number) => $"{number:X16}{EXTENSION_CHILD}";
}

public abstract class StreamSection : Section, IDisposable
{
    public const int HEADER_LENGTH = 16;
    SectionMode _mode;
    Stream? _stream;
    long _start;

    public override bool IsEmpty => Length == _start;
    public override SectionMode Mode
    {
        get => _mode;
        set
        {
            if (_mode == value) return;

            switch (value)
            {
                case SectionMode.Read:
                _stream ??= GetStream();
                _stream.Position = _start;
                break;

                case SectionMode.Write:
                _stream ??= GetStream();
                _stream.Seek(0, SeekOrigin.End);
                break;

                case SectionMode.Idle:
                break;

                case SectionMode.Close:
                Update();
                _stream?.Close();
                _stream = null;
                break;
            }

            _mode = value;
        }
    }
    public long MaxLength { get; }
    protected Stream? Stream => _stream;
    protected abstract long Length { get; }
    private long Start
    {
        get => _start;
        set
        {
            if (value == 0) _start = HEADER_LENGTH;
            else if (value < HEADER_LENGTH) throw new ArgumentException("想定外の値が指定されました。");
            else _start= value;
        }
    }

    public StreamSection(long maxLength = long.MaxValue)
    {
        var stream = GetStream();
        Span<byte> buf = stackalloc byte[sizeof(long)];
        stream.Seek(0, SeekOrigin.Begin);

        _stream = stream;

        _ = stream.Read(buf);
        NextNumber = BinaryConverter.ToUInt64(buf);
        _ = stream.Read(buf);
        Start = BinaryConverter.ToInt64(buf);
        MaxLength = maxLength;
    }

    protected abstract Stream GetStream();

    public override int Read(Span<byte> span)
    {
        var r = _stream!.Read(span);
        _start += r;
        return r;
    }
    public override async Task<int> ReadAsync(Memory<byte> memory)
    {
        var r = await _stream!.ReadAsync(memory);
        _start += r;
        return r;
    }

    public override int Write(ReadOnlySpan<byte> span)
    {
        var rest = (int)(MaxLength - _stream!.Length);
        if (rest < span.Length)
        {
            _stream.Write(span[..rest]);
            return rest;
        }
        else
        {
            _stream.Write(span);
            return span.Length;
        }
    }
    public override async Task<int> WriteAsync(ReadOnlyMemory<byte> memory)
    {
        var rest = (int)(MaxLength - _stream!.Length);
        if (rest < memory.Length)
        {
            await _stream.WriteAsync(memory[..rest]);
            return rest;
        }
        else
        {
            await _stream.WriteAsync(memory);
            return memory.Length;
        }
    }

    public override void Clear()
    {
        _stream ??= GetStream();
        _stream.SetLength(0);

        Start = 0;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected void Dispose(bool disposing)
    {
        if (disposing) 
        {
            Update();

            _stream?.Dispose(); 
        }
    }

    void Update()
    {
        Span<byte> span = stackalloc byte[HEADER_LENGTH];
        BinaryConverter.ToBytes(NextNumber, span);
        BinaryConverter.ToBytes(Start, span[64..]);

        _stream ??= GetStream();
        _stream.Seek(0, SeekOrigin.Begin);
        _stream.Write(span);
    }
}

public class FileSection : StreamSection
{
    readonly FileInfo _fileInfo;

    public FileInfo FileInfo => _fileInfo;
    protected override long Length
    {
        get
        {
            if (Stream is null)
            {
                _fileInfo.Refresh();
                return _fileInfo.Length;
            }
            else
            {
                return Stream.Length;
            }
        }
    }

    public FileSection(FileInfo fileInfo, long maxLength = long.MaxValue) : base(maxLength)
    {
        _fileInfo = fileInfo;
    }

    protected override Stream GetStream() => _fileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
}
