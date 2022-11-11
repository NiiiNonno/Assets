using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nonno.Assets.Scrolls;

public abstract class SectionedScroll<TSection> : IScroll where TSection : ISection
{
    readonly int _bufferLength;
    readonly LinkedList<TSection> _sections;
    readonly Dictionary<ScrollPointer, LinkedListNode<TSection>> _nodes;
    readonly ManualResetEventSlim _lodgeEvent;
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
    protected int WriteSectionNumber => _writeSectionNode is { } node ? node.Value.Number : Int32.MinValue;
    protected int ReadSectionNumber => _readSectionNode is { } node ? node.Value.Number : Int32.MaxValue;
    protected ManualResetEventSlim LodgeEvent => _lodgeEvent;
    internal IEnumerable<TSection> Sections => _sections;
    public int SectionCount => _sections.Count;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public ScrollPointer Point
    {
        get
        {
            // 実質的にはWriteSectionに新しい節を追加挿入する処理。
            if (_writeSectionNode is { } writeSectionNode) writeSectionNode.Value.Mode = SectionMode.Idle;

            var number = Assets.Utils.AverageCeiling(of1: WriteSectionNumber, of2: ReadSectionNumber);
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
            case ({ } wSN, { } rSN):
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
            case ({ } wSN, null):
                {
                    wSN.Value.Mode = SectionMode.Idle;

                    break;
                }
            case (null, { } rSN):
                {
                    rSN.Value.Delete();
                    _sections.Remove(rSN);

                    break;
                }
            }
            // ここまでで連結、削除の処理は終了。

            if (!_nodes.Remove(value, out var node)) throw new ArgumentException($"索引が不明です。`{nameof(ScrollPointer)}`の用法を確認してください。", nameof(value));
            _writeSectionNode = node.Previous;
            _readSectionNode = node;
            if (_writeSectionNode is not null) _writeSectionNode.Value.Mode = SectionMode.Write;
            _readSectionNode.Value.Mode = SectionMode.Read;
        }
    }

    public SectionedScroll(int bufferLength = 1024)
    {
        _bufferLength = bufferLength;
        _sections = new();
        _nodes = new();
        _lodgeEvent = new();
    }
    protected SectionedScroll(SectionedScroll<TSection> original, CopyDelegate<TSection> copyDelegate)
    {
        var sections = new LinkedList<TSection>();
        var nodes = new Dictionary<ScrollPointer, LinkedListNode<TSection>>();
        var wSN = default(LinkedListNode<TSection>?);
        var rSN = default(LinkedListNode<TSection>?);
        foreach (var mastS in original._sections)
        {
            var node = sections.AddLast(copyDelegate(mastS));
            nodes.Add(original._nodes.First(mastN => ReferenceEquals(mastN.Value.Value, mastS)).Key, node);
            if (original._writeSectionNode != null && Equals(original._writeSectionNode.Value, mastS)) wSN = node;
            if (original._readSectionNode != null && Equals(original._readSectionNode.Value, mastS)) rSN = node;
        }

        _bufferLength = original._bufferLength;
        _sections = sections;
        _nodes = nodes;
        _lodgeEvent = new();
        _writeSectionNode = wSN;
        _readSectionNode = rSN;
        _isDisposed = original._isDisposed;
    }

    public abstract IScroll Copy();
    public abstract Task<IScroll> CopyAsync();

    public bool IsValid(ScrollPointer pointer) => _nodes.ContainsKey(pointer);

    public async Task Insert<T>(Memory<T> memory) where T : unmanaged
    {
        _lodgeEvent.Wait();

        if (memory is Memory<byte> memory_)
        {
            if (WriteSectionNode is { } node) await node.Value.WriteAsync(memory_);
        }
        else
        {
            InsertSync(memory.Span);
        }
    }
    public void InsertSync<T>(Span<T> span) where T : unmanaged
    {
        _lodgeEvent.Wait();

        var span_ = span.ToSpan<T, byte>();
        if (WriteSectionNode is { } node) node.Value.Write(span_);
    }
    public async Task Remove<T>(Memory<T> memory) where T : unmanaged
    {
        _lodgeEvent.Wait();

        if (memory is Memory<byte> memory_)
        {
            if (ReadSectionNode is { } node) await node.Value.ReadAsync(memory_);
        }
        else
        {
            RemoveSync(memory.Span);
        }
    }
    public void RemoveSync<T>(Span<T> span) where T : unmanaged
    {
        _lodgeEvent.Wait();

        var span_ = span.ToSpan<T, byte>();
        if (ReadSectionNode is { } node) node.Value.Read(span_);
    }

    public abstract Task Insert(in ScrollPointer pointer);
    public abstract Task Remove(out ScrollPointer pointer);

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

    public virtual void Lodge()
    {
        _lodgeEvent.Reset();
        if (_readSectionNode is { } rSN) rSN.Value.Mode = SectionMode.Idle;
        if (_writeSectionNode is { } wSN) wSN.Value.Mode = SectionMode.Idle;
    }
    public virtual void Dislodge()
    {
        if (_readSectionNode is { } rSN) rSN.Value.Mode = SectionMode.Read;
        if (_writeSectionNode is { } wSN) wSN.Value.Mode = SectionMode.Write;
        _lodgeEvent.Set();
    }

    ////protected virtual void Close()
    ////{
    ////    foreach (var section in _sections) section.Dispose();
    ////    _sections.Clear();
    ////    _nodes.Clear();
    ////    _writeSectionNode = _readSectionNode = null;
    ////}
    ////protected virtual Task CloseAsync()
    ////{
    ////    Tasks tasks = default;
    ////    foreach (var section in _sections) tasks += section.DisposeAsync().AsTask();
    ////    _sections.Clear();
    ////    _nodes.Clear();
    ////    _writeSectionNode = _readSectionNode = null;
    ////    return tasks.WhenAll();
    ////}

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

    protected void InsertSection(ScrollPointer index, TSection section)
    {
        var node = GetNode(section);
        if (!_nodes.TryAdd(index, node))
        {
            _sections.Remove(node);
            throw new ArgumentException("指定された索引は既に登録されています。", nameof(index));
        }
    }

    protected abstract (ScrollPointer index, TSection section) CreateSection(int number);
    public bool Is(ScrollPointer on)
    {
        throw new NotImplementedException();
    }
}

public class MemoryNote : SectionedScroll<MemorySection>
{
    public int Length
    {
        get
        {
            int r = 0;
            foreach (var section in Sections) r += section.Length;
            return r;
        }
    }
    public IEnumerable<byte> Memory
    {
        get
        {
            return Enumerate();

            IEnumerable<byte> Enumerate()
            {
                foreach (var section in Sections) foreach (var item in section.Memory) yield return item;
            }
        }
    }

    public MemoryNote() : base() { }
    protected MemoryNote(MemoryNote original) : base(original, original => new MemorySection(original)) { }

    public override IScroll Copy()
    {
        return new MemoryNote(this);
    }
    public override Task<IScroll> CopyAsync()
    {
        return Task.FromResult<IScroll>(new MemoryNote(this));
    }

    public override Task Insert(in ScrollPointer index)
    {
        return this.Insert(int32: (int)index.LongNumber);
    }
    public override Task Remove(out ScrollPointer index)
    {
        var r = this.Remove(int32: out int number);
        index = new(number: number);
        return r;
    }
    protected override (ScrollPointer index, MemorySection section) CreateSection(int number)
    {
        return (new ScrollPointer(number), new MemorySection() { Number = number });
    }
}

public class DirectoryNote : SectionedScroll<FileSection>
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

            var index = new ScrollPointer(information: fileInfo.GetFileNameWithoutExtension().ToString());
            var section = new FileSection(fileInfo);
            InsertSection(index, section);
        }

        // 入節指定。
        var entrySectionIndex = new ScrollPointer(information: ENTRY_SECTION_NAME);
        if (IsValid(entrySectionIndex))
        {
            Point = entrySectionIndex;
        }
    }
    //private void Init()
    //{
    //    Lock();
    //    DirectoryInfo.Refresh();

    //    // 既存ファイル登録。
    //    foreach (var fileInfo in DirectoryInfo.EnumerateFiles())
    //    {
    //        if (fileInfo.Extension != FileSection.EXTENSION) continue;

    //        var index = new NotePoint(information: fileInfo.GetFileNameWithoutExtension().ToString());
    //        var section = new FileSection(fileInfo);
    //        InsertSection(index, section);
    //    }

    //    // 入節指定。
    //    var entrySectionIndex = new NotePoint(information: ENTRY_SECTION_NAME);
    //    if (IsValid(entrySectionIndex))
    //    {
    //        Point = entrySectionIndex;
    //    }
    //}
    /// <summary>
    /// 複製します。
    /// </summary>
    /// <param name="original">複製元。</param>
    /// <param name="directoryInfo">複製先のディレクトリ情報。</param>
    protected DirectoryNote(DirectoryNote original, DirectoryInfo directoryInfo) : base(original, mastSection => new(new(Path.Combine(directoryInfo.FullName, mastSection.FileInfo.Name))))
    {
        DirectoryInfo = directoryInfo;
    }

    public override Task Insert(in ScrollPointer index)
    {
        return this.Insert(@string: (string?)index.Information);
    }
    public override Task Remove(out ScrollPointer index)
    {
        var r = this.Remove(out string? information);
        index = new(information: information ?? throw new Exception("不明な錯誤です。軸箋の名前が`null`でした。"));
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

    public override IScroll Copy()
    {
        var newDI = new DirectoryInfo(Path.Combine(DirectoryInfo.Parent?.FullName ?? String.Empty, $"{DirectoryInfo.Name}({DateTime.Now})"));
        DirectoryInfo.Copy(newDI);

        try
        {
            Lodge();
            return new DirectoryNote(this, newDI);
        }
        finally
        {
            Dislodge();
        }

        //Close();
        //var newDI = new DirectoryInfo(Path.Combine(DirectoryInfo.Parent?.FullName ?? String.Empty, $"{DirectoryInfo.Name}({DateTime.Now})"));
        //DirectoryInfo.Copy(to: newDI, allowOverride: false);
        //Init();
        //return new DirectoryNote(newDI);
    }
    public override Task<IScroll> CopyAsync()
    {
        var newDI = new DirectoryInfo(Path.Combine(DirectoryInfo.Parent?.FullName ?? String.Empty, $"{DirectoryInfo.Name}({DateTime.Now})"));
        DirectoryInfo.Copy(newDI);

        try
        {
            Lodge();
            return Task.FromResult<IScroll>(new DirectoryNote(this, newDI));
        }
        finally
        {
            Dislodge();
        }

        //await CloseAsync();
        //var newDI = new DirectoryInfo(Path.Combine(DirectoryInfo.Parent?.FullName ?? String.Empty, $"{DirectoryInfo.Name}({DateTime.Now})"));
        //DirectoryInfo.Copy(to: newDI, allowOverride: false);
        //Init();
        //return new DirectoryNote(newDI);
    }

    protected override (ScrollPointer index, FileSection section) CreateSection(int number)
    {
        var name = $"section{DateTime.Now.Ticks:X16}{number:X8}";
        return (new ScrollPointer(information: name), FileSection.CreateSection(new FileInfo(Path.Combine(DirectoryInfo.FullName, name + FileSection.EXTENSION)), number));
    }

    /// <summary>
    /// ディレクトリ内容を全てディスクに保存し、節の接続を撤退させます。
    /// <para>
    /// これによってディレクトリは、次に搴取を行うまで安全に複製することができます。
    /// </para>
    /// </summary>
    public override void Lodge()
    {
        base.Lodge();
        foreach (var section in Sections) section.Withdraw();
    }

    //protected override void Close()
    //{
    //    var entrySectionFileInfo = ReadSectionNode?.Value.FileInfo;
    //    base.Close();
    //    entrySectionFileInfo?.Refresh();
    //    entrySectionFileInfo?.MoveTo(Path.Combine(DirectoryInfo.FullName, ENTRY_SECTION_NAME + FileSection.EXTENSION));
    //    Unlock();
    //}
    //protected override async Task CloseAsync()
    //{
    //    var entrySectionFileInfo = ReadSectionNode?.Value.FileInfo;
    //    await base.CloseAsync();
    //    entrySectionFileInfo?.Refresh();
    //    entrySectionFileInfo?.MoveTo(Path.Combine(DirectoryInfo.FullName, ENTRY_SECTION_NAME + FileSection.EXTENSION));
    //    Unlock();
    //}

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

public class CompactedNote : SectionedScroll<ZipArchiveSection>
{
    const string INDEX_FILE_NAME = "\0";
    const int INDEX_ENTRY_INDEX_POSITION = 0;
    const int INDEX_COUNT_POSITION = INDEX_ENTRY_INDEX_POSITION + sizeof(long);
    const int INDEX_FILE_LENGTH = INDEX_COUNT_POSITION + sizeof(long);

    /*readonly*/ZipArchive _archive;
    long _count;

    public ZipArchive Archive => _archive;
    public FileInfo FileInfo { get; }
    public CompressionLevel PriorCompressionLevel { get; set; }

    public CompactedNote(FileInfo fileInfo)
    {
        _archive = null!;

        FileInfo = fileInfo;

        FileInfo.Refresh();
        _archive = FileInfo.Exists
            ? (new(FileInfo.Open(FileMode.Open, FileAccess.ReadWrite), ZipArchiveMode.Update, false, Encoding.UTF8))
            : (new(FileInfo.Open(FileMode.CreateNew, FileAccess.ReadWrite), ZipArchiveMode.Update, false, Encoding.UTF8));

        ZipArchiveEntry? indexEntry = null;
        foreach (var entry in _archive.Entries)
        {
            if (entry.Name == INDEX_FILE_NAME)
            {
                indexEntry = entry;
                continue;
            }
            InsertSection(new ScrollPointer(GetNumber(entry.Name)), new ZipArchiveSection(entry));
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
                    var pointer = new ScrollPointer(longNumber: number);
                    if (IsValid(pointer)) Point = pointer;
                }

                _count = BitConverter.ToInt64(span[INDEX_COUNT_POSITION..]);
            }
        }
    }
    protected CompactedNote(CompactedNote original, FileInfo fileInfo, ZipArchive archive) : base(original, mastS => new ZipArchiveSection(archive.GetEntry(mastS.Entry.Name) ?? throw new Exception("アーカイブから口を見つけられませんでした。")))
    {
        _archive = archive;
        _count = original._count;

        FileInfo = fileInfo;
        PriorCompressionLevel = original.PriorCompressionLevel;
    }
    //private void Init()
    //{
    //    FileInfo.Refresh();
    //    _archive = FileInfo.Exists
    //        ? (new(FileInfo.Open(FileMode.Open, FileAccess.ReadWrite), ZipArchiveMode.Update, false, Encoding.UTF8))
    //        : (new(FileInfo.Open(FileMode.CreateNew, FileAccess.ReadWrite), ZipArchiveMode.Update, false, Encoding.UTF8));

    //    ZipArchiveEntry? indexEntry = null;
    //    foreach (var entry in _archive.Entries)
    //    {
    //        if (entry.Name == INDEX_FILE_NAME)
    //        {
    //            indexEntry = entry;
    //            continue;
    //        }
    //        InsertSection(new NotePoint(GetNumber(entry.Name)), new ZipArchiveSection(entry));
    //    }

    //    // 初期位置読み出しとカウンター取得。
    //    if (indexEntry is not null)
    //    {
    //        using (var stream = indexEntry.Open())
    //        {
    //            Span<byte> span = stackalloc byte[INDEX_FILE_LENGTH];
    //            if (stream.Read(span) != span.Length) throw new IOException("内部インデックスファイルの形式が不明です。ファイルが破損している可能性があります。");

    //            var number = BitConverter.ToInt64(span[INDEX_ENTRY_INDEX_POSITION..]);
    //            if (number >= 0) // number < 0となるのは最後のDispose時にReadSectionNode == null(つまりファイル終点)だった時。
    //            {
    //                var index = new NotePoint(number: number);
    //                if (IsValid(index)) Point = index;
    //            }

    //            _count = BitConverter.ToInt64(span[INDEX_COUNT_POSITION..]);
    //        }
    //    }
    //}

    public override Task Insert(in ScrollPointer index)
    {
        return this.Insert(int64: index.LongNumber);
    }
    public override Task Remove(out ScrollPointer index)
    {
        var r = this.Remove(out long number);
        index = new(longNumber: number);
        return r;
    }

    public override IScroll Copy()
    {
        try
        {
            Lodge();
            var newFI = FileInfo.CopyTo(Path.Combine(FileInfo.Directory?.FullName ?? String.Empty, $"{FileInfo.Name}({DateTime.Now})"));
            var archive = new ZipArchive(newFI.Open(FileMode.Open, FileAccess.ReadWrite));
            return new CompactedNote(this, newFI, archive);
        }
        finally
        {
            Dislodge();
        }

        //Close();
        //var newFI = new FileInfo(Path.Combine(FileInfo.Directory?.FullName ?? String.Empty, $"{FileInfo.Name}({DateTime.Now})"));
        //FileInfo.CopyTo(newFI.FullName);
        //Init();
        //return new CompactedNote(newFI);
    }
    public override Task<IScroll> CopyAsync()
    {
        try
        {
            Lodge();
            var newFI = FileInfo.CopyTo(Path.Combine(FileInfo.Directory?.FullName ?? String.Empty, $"{FileInfo.Name}({DateTime.Now})"));
            var archive = new ZipArchive(newFI.Open(FileMode.Open, FileAccess.ReadWrite));
            return Task.FromResult<IScroll>(new CompactedNote(this, newFI, archive));
        }
        finally
        {
            Dislodge();
        }

        //await CloseAsync();
        //var newFI = new FileInfo(Path.Combine(FileInfo.Directory?.FullName ?? String.Empty, $"{FileInfo.Name}({DateTime.Now})"));
        //FileInfo.CopyTo(newFI.FullName);
        //Init();
        //return new CompactedNote(newFI);
    }

    protected override (ScrollPointer index, ZipArchiveSection section) CreateSection(int number)
    {
        var count = _count++;
        var entry = _archive.CreateEntry(GetName(count), PriorCompressionLevel);
        return (new ScrollPointer(longNumber: count), ZipArchiveSection.CreateSection(entry, number));
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

    public override void Lodge() 
    {
        base.Lodge();
        foreach (var section in Sections) section.Withdraw();
        _archive.Dispose();
    }
    public override void Dislodge()
    {
        _archive = new(FileInfo.Open(FileMode.Open, FileAccess.ReadWrite));
        base.Dislodge();
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
