using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets;

public abstract class SectionedNote<TSection> : INote where TSection : ISection
{
    readonly int _bufferLength;
    readonly LinkedList<TSection> _sections;
    readonly Dictionary<NotePoint, LinkedListNode<TSection>> _nodes;
    LinkedListNode<TSection>? _writeSectionNode, _readSectionNode;
    bool _isDisposed;

    protected LinkedListNode<TSection>? WriteSectionNode
    {
        get => _writeSectionNode;
        //init => _writeSectionNode = value;
    }
    protected LinkedListNode<TSection>? ReadSectionNode
    {
        get => _readSectionNode;
        //init => _readSectionNode = value;
    }
    protected int WriteSectionNumber => _writeSectionNode is LinkedListNode<TSection> node ? node.Value.Number : Int32.MinValue;
    protected int ReadSectionNumber => _readSectionNode is LinkedListNode<TSection> node ? node.Value.Number : Int32.MaxValue;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public NotePoint Point
    {
        get
        {
            // 実質的にはWriteSectionに新しい節を追加挿入する処理。
            if (_writeSectionNode is LinkedListNode<TSection> writeSectionNode) writeSectionNode.Value.Mode = SectionMode.Idle;

            var number = Utils.AverageCeiling(of1: WriteSectionNumber, of2: ReadSectionNumber);
            var (index, section) = CreateSection(number);
            var node = GetNode(section);
            _nodes.Add(index, node);
            _writeSectionNode = node;

            _writeSectionNode.Value.Mode = SectionMode.Write;
            if (_readSectionNode != null) _readSectionNode.Value.Mode = SectionMode.Read;
            if (ReadSectionNumber - WriteSectionNumber < 2) Rearrange();
            return index;
        }
        set
        {
            // 前半はReadSectionのを削除連結する処理。
            switch (_writeSectionNode, _readSectionNode)
            {
            case (LinkedListNode<TSection> wSN, LinkedListNode<TSection> rSN):
                {
                    long length = rSN.Value.Length;
                    while (length > 0) Move(ref length, rSN.Value, wSN.Value, _bufferLength);
                    static void Move(ref long length, TSection rS, TSection wS, int bufferLength)
                    {
                        Span<byte> buffer = stackalloc byte[(int)Math.Min(length, bufferLength)];
                        rS.Read(buffer);
                        wS.Write(buffer);
                        length -= buffer.Length;
                    }

                    wSN.Value.Mode = SectionMode.Idle;
                    rSN.Value.Delete();
                    _sections.Remove(rSN);
                    // ちなみにこの時点(現節がこの節となったとき)に_nodesからは記述が削除されているので気にしない。

                    break;
                }
            case (LinkedListNode<TSection> wSN, null):
                {
                    wSN.Value.Mode = SectionMode.Idle;

                    break;
                }
            case (null, LinkedListNode<TSection> rSN):
                {
                    rSN.Value.Delete();
                    _sections.Remove(rSN);

                    break;
                }
            }
            // ここまでで連結、削除の処理は終了。

            if (!_nodes.Remove(value, out var node)) throw new ArgumentException($"索引が不明です。`{nameof(NotePoint)}`の用法を確認してください。", nameof(value));
            _writeSectionNode = node.Previous;
            _readSectionNode = node;
            if (_writeSectionNode is not null) _writeSectionNode.Value.Mode = SectionMode.Write;
            _readSectionNode.Value.Mode = SectionMode.Read;
        }
    }

    public SectionedNote(int bufferLength = 1024)
    {
        _bufferLength = bufferLength;
        _sections = new();
        _nodes = new();
    }

    public bool IsValid(NotePoint index) => _nodes.ContainsKey(index);

    public async Task Insert<T>(Memory<T> memory) where T : unmanaged
    {
        if (memory is Memory<byte> memory_)
        {
            if (WriteSectionNode is LinkedListNode<TSection> node) await node.Value.WriteAsync(memory_);
        }
        else
        {
            InsertSync(memory.Span);
        }
    }
    public void InsertSync<T>(Span<T> span) where T : unmanaged
    {
        var span_ = span.ToSpan<T, byte>();
        if (WriteSectionNode is LinkedListNode<TSection> node) node.Value.Write(span_);
    }
    public async Task Remove<T>(Memory<T> memory) where T : unmanaged
    {
        if (memory is Memory<byte> memory_)
        {
            if (ReadSectionNode is LinkedListNode<TSection> node) await node.Value.ReadAsync(memory_);
        }
        else
        {
            RemoveSync(memory.Span);
        }
    }
    public void RemoveSync<T>(Span<T> span) where T : unmanaged
    {
        var span_ = span.ToSpan<T, byte>();
        if (ReadSectionNode is LinkedListNode<TSection> node) node.Value.Read(span_);
    }

    public abstract Task Insert(in NotePoint index);
    public abstract Task Remove(out NotePoint index);

    public void Rearrange()
    {
        double v = _sections.Count + 1;
        double d = ((double)Int32.MaxValue - Int32.MinValue) / v;
        double c = Int32.MinValue;
        int i = Int32.MinValue;
        foreach (var section in _sections)
        {
            int c_ = (int)(c += d);
            if (c_ - i < 2) throw new InvalidOperationException("整列後の要素の間隔が2未満です。これ以上節を維持することができません。");
            i = c_;
            section.Number = i;
        }
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
                foreach (var section in _sections) section.Dispose();
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
                foreach (var section in _sections) await section.DisposeAsync();
            }

            // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
            // TODO: 大きなフィールドを null に設定します
            _isDisposed = true;
        }
    }

    protected LinkedListNode<TSection> GetNode(TSection section)
    {
        if (WriteSectionNode != null && ReadSectionNode != null && WriteSectionNode.Value.Number < section.Number && section.Number < ReadSectionNode.Value.Number) return _sections.AddAfter(WriteSectionNode, section);

        var c = _sections.First;
        while (c != null)
        {
            if (section.Number < c.Value.Number) return _sections.AddBefore(c, section);
            c = c.Next;
        }
        return _sections.AddLast(section);
    }

    protected void InsertSection(NotePoint index, TSection section)
    {
        var node = GetNode(section);
        if (!_nodes.TryAdd(index, node))
        {
            _sections.Remove(node);
            throw new ArgumentException("指定された索引は既に登録されています。", nameof(index));
        }
    }

    protected abstract (NotePoint index, TSection section) CreateSection(int number);
}

public class DirectoryNote : SectionedNote<FileSection>
{
    const string LOCK_FILE_EXTENSION = ".lock";
    const string ENTRY_SECTION_NAME = "entry";

    public DirectoryInfo DirectoryInfo { get; }

    public DirectoryNote(DirectoryInfo directoryInfo)
    {
        if (!directoryInfo.Exists) directoryInfo.Create();

        DirectoryInfo = directoryInfo;

        Lock();
        DirectoryInfo.Refresh();

        // 既存ファイル登録。
        foreach (var fileInfo in DirectoryInfo.EnumerateFiles())
        {
            if (fileInfo.Extension != FileSection.EXTENSION) continue;

            var index = new NotePoint(information: fileInfo.GetFileNameWithoutExtension().ToString());
            var section = new FileSection(fileInfo);
            InsertSection(index, section);
        }

        // 入節指定。
        var entrySectionIndex = new NotePoint(information: ENTRY_SECTION_NAME);
        if (IsValid(entrySectionIndex))
        {
            Point = entrySectionIndex;
        }
    }

    public override Task Insert(in NotePoint index)
    {
        return this.Insert(@string: (string?)index.Information);
    }
    public override Task Remove(out NotePoint index)
    {
        var r = this.Remove(out string? information);
        index = new(information);
        return r;
    }

    void Lock()
    {
        var path = Path.Combine(DirectoryInfo.FullName, DirectoryInfo.GetFileNameWithoutExtension().ToString() + LOCK_FILE_EXTENSION);

        if (File.Exists(path)) throw new IOException($"フォルダーは既に別のプロセス\"{File.ReadAllText(path)}\"によって{File.GetCreationTime(path):yyyy年M月d日h時m分s秒}に開かれています。");

        File.WriteAllText(path, AppDomain.CurrentDomain.FriendlyName);
    }

    void Unlock()
    {
        var path = Path.Combine(DirectoryInfo.FullName, DirectoryInfo.GetFileNameWithoutExtension().ToString() + LOCK_FILE_EXTENSION);

        File.Delete(path);
    }

    protected override (NotePoint index, FileSection section) CreateSection(int number)
    {
        var name = $"section{DateTime.Now.Ticks:X16}{number:X8}";
        return (new NotePoint(information: name), FileSection.CreateSection(new FileInfo(Path.Combine(DirectoryInfo.FullName, name + FileSection.EXTENSION)), number));
    }

    protected override void Dispose(bool disposing)
    {
        var entrySectionFileInfo = ReadSectionNode?.Value.FileInfo;
        base.Dispose(disposing);
        entrySectionFileInfo?.Refresh();
        entrySectionFileInfo?.MoveTo(Path.Combine(DirectoryInfo.FullName, ENTRY_SECTION_NAME + FileSection.EXTENSION));
        Unlock();
    }
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        var entrySectionFileInfo = ReadSectionNode?.Value.FileInfo;
        await base.DisposeAsync(disposing);
        entrySectionFileInfo?.Refresh();
        entrySectionFileInfo?.MoveTo(Path.Combine(DirectoryInfo.FullName, ENTRY_SECTION_NAME + FileSection.EXTENSION));
        Unlock();
    }
}

public class CompactedNote : SectionedNote<ZipArchiveSection>
{
    const string INDEX_FILE_NAME = "\0";
    const int INDEX_ENTRY_INDEX_POSITION = 0;
    const int INDEX_COUNT_POSITION = INDEX_ENTRY_INDEX_POSITION + sizeof(long);
    const int INDEX_FILE_LENGTH = INDEX_COUNT_POSITION + sizeof(long);

    readonly ZipArchive _archive;
    long _count;

    public ZipArchive Archive => _archive;
    public CompressionLevel PriorCompressionLevel { get; set; }

    public CompactedNote(FileInfo fileInfo)
    {
        fileInfo.Refresh();
        _archive = fileInfo.Exists
            ? (new(fileInfo.Open(FileMode.Open, FileAccess.ReadWrite), ZipArchiveMode.Update, false, Encoding.UTF8))
            : (new(fileInfo.Open(FileMode.CreateNew, FileAccess.ReadWrite), ZipArchiveMode.Update, false, Encoding.UTF8));

        ZipArchiveEntry? indexEntry = null;
        foreach (var entry in _archive.Entries)
        {
            if (entry.Name == INDEX_FILE_NAME)
            {
                indexEntry = entry;
                continue;
            }
            InsertSection(new NotePoint(GetNumber(entry.Name)), new ZipArchiveSection(entry));
        }

        // 初期位置読み出しとカウンター取得。
        if (indexEntry is not null)
        {
            using (var stream = indexEntry.Open())
            {
                Span<byte> span = stackalloc byte[INDEX_FILE_LENGTH];
                if (stream.Read(span) != span.Length) throw new IOException("内部インデックスファイルの形式が不明です。ファイルが破損している可能性があります。");

                var number = BitConverter.ToInt64(span[INDEX_ENTRY_INDEX_POSITION..]);
                if (number >= 0) // number < 0となるのは最後のDispose時にReadSectionNode == null(つまりファイル終点)だった時。
                {
                    var index = new NotePoint(number: number);
                    if (IsValid(index)) Point = index;
                }

                _count = BitConverter.ToInt64(span[INDEX_COUNT_POSITION..]);
            }
        }
    }

    public override Task Insert(in NotePoint index)
    {
        return this.Insert(int64: index.Number);
    }
    public override Task Remove(out NotePoint index)
    {
        var r = this.Remove(out long number);
        index = new(number: number);
        return r;
    }

    protected override (NotePoint index, ZipArchiveSection section) CreateSection(int number)
    {
        var count = _count++;
        var entry = _archive.CreateEntry(GetName(count), PriorCompressionLevel);
        return (new NotePoint(number: count), ZipArchiveSection.CreateSection(entry, number));
    }

    protected override void Dispose(bool disposing)
    {
        UpdateIndexFile();
        base.Dispose(disposing);
        _archive.Dispose();
    }
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        UpdateIndexFile();
        await base.DisposeAsync(disposing);
        _archive.Dispose();
    }

    void UpdateIndexFile()
    {
        // 初期位置書き込みとカウンター設定。
        using (var stream = (_archive.GetEntry(INDEX_FILE_NAME) ?? _archive.CreateEntry(INDEX_FILE_NAME, CompressionLevel.NoCompression)).Open())
        {
            Span<byte> span = stackalloc byte[INDEX_FILE_LENGTH];
            _ = BitConverter.TryWriteBytes(span[INDEX_ENTRY_INDEX_POSITION..], ReadSectionNode is not null 
                ? GetNumber(ReadSectionNode.Value.Entry.Name)
                : -1);
            _ = BitConverter.TryWriteBytes(span[INDEX_COUNT_POSITION..], _count);
            stream.Position = 0;
            stream.Write(span);
        }
    }

    static long GetNumber(string name) 
    {
        Span<byte> span = stackalloc byte[name.Length];
        for (int i = 0; i < name.Length; i++)
        {
            span[i] = checked((byte)(name[i] - 0x0100));
        }
        return BitConverter.ToInt64(span);
        //return span.ToStruct<byte, long>();
        //return Int64.Parse(name);
        //return name.AsSpan().ToStruct<char, long>(); 
    }
    static string GetName(long number)
    {
        Span<byte> span = stackalloc byte[sizeof(long)];
        _ = BitConverter.TryWriteBytes(span, number);
        //var span = number.ToSpan<long, byte>();
        Span<char> r = stackalloc char[span.Length];
        for (int i = 0; i < span.Length; i++)
        {
            r[i] = (char)(span[i] + 0x0100);
        }
        return new string(r);
        //return number.ToString();
        //return new(number.ToSpan<long, char>());
    }
}
