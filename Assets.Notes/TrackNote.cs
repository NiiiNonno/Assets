using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nonno.Assets;
using Nonno.Assets.Collections;
using static System.Collections.Specialized.BitVector32;

namespace Nonno.Assets.Notes;
public class TrackNote : INote
{
    readonly int _sectorLength;
    readonly List<Sector> _sectors;
    Sector _writeSector, _readSector;
    bool _isDisposed;

    public NotePointer Pointer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public TrackNote(int sectorLength)
    {
        var primarySector = new Sector(sectorLength);

        _sectorLength = sectorLength;
        _sectors = new() { primarySector };
        _writeSector = _readSector = primarySector;
    }
    protected TrackNote(TrackNote original)
    {
        var sectors = new List<Sector>(original._sectors.Capacity);
        for (int i = 0; i < original._sectors.Count; i++) sectors[i] = new Sector(original._sectors[i]);

        _sectorLength = original._sectorLength;
        _sectors = sectors;
        _writeSector = original._writeSector;
        _readSector = original._readSector;
        _isDisposed = original._isDisposed;
    }

    public INote Copy()
    {
        return new TrackNote(this);
    }

    public bool IsValid(NotePointer pointer)
    {

        int number = (int)(pointer.Number & 0xFFFFFFFF);//??longの上下位で、、、
        if (number >= _sectors.Count) return false;
        var sector = _sectors[number];
        return sector.IsUsed && ReferenceEquals(sector, pointer.Information);
    }
    public Task Insert(in NotePointer pointer)
    {
        throw new NotImplementedException();
    }
    public Task Insert<T>(Memory<T> memory) where T : unmanaged => throw new NotImplementedException();
    public void InsertSync<T>(Span<T> span) where T : unmanaged => throw new NotImplementedException();

    public Task Remove(out NotePointer index) => throw new NotImplementedException();
    public Task Remove<T>(Memory<T> memory) where T : unmanaged => throw new NotImplementedException();
    public void RemoveSync<T>(Span<T> span) where T : unmanaged => throw new NotImplementedException();

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
                await Task.CompletedTask;
            }

            // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
            // TODO: 大きなフィールドを null に設定します
            _isDisposed = true;
        }
    }

    public class Sector
    {
        readonly byte[] _data;
        int _generation;

        public int PreviousIndex { get; set; }
        public int NextIndex { get; set; }
        public bool IsUsed => (_generation & 1) == 0;
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int Generation => _generation;
        public byte[] Data => _data;

        public Sector(int length)
        {
            _data = new byte[length];
        }
        public Sector(Sector original)
        {
            var data = new byte[original._data.Length];
            for (int i = 0; i < data.Length; i++) data[i] = original._data[i];

            _data = data;

            PreviousIndex = original.PreviousIndex;
            NextIndex = original.NextIndex;
            StartIndex = original.StartIndex;
            EndIndex = original.EndIndex;
        }

        public void Use()
        {
            if (IsUsed) throw new InvalidOperationException();

            _generation++;
        }

        public void Disuse()
        {
            if (!IsUsed) throw new InvalidOperationException();

            _generation++;
        }
    }
}
