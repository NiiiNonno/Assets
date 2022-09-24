namespace Nonno.Assets;

public class DuplicatingNote : INote
{
    readonly INote _primaryNote;
    readonly Dictionary<NotePoint, NotePoint?[]> _notePoints;
    INote?[] _subordinateNotes;
    int _subordinateNotesLastIndex;
    bool _isDisposed;

    public DuplicatingNote(INote primaryNote, int defaultSubordinateNotesCapacity = 1)
    {
        if (defaultSubordinateNotesCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(defaultSubordinateNotesCapacity));

        _primaryNote = primaryNote;
        _notePoints = new();
        _subordinateNotes = new INote[defaultSubordinateNotesCapacity];
    }

    public NotePoint Point 
    {
        get
        {
            var r = _primaryNote.Point;

            var nPs = new NotePoint?[_subordinateNotes.Length];
            for (int i = 0; i < nPs.Length; i++) nPs[i] = _subordinateNotes[i] is INote note ? note.Point : null;
            _notePoints.Add(r, nPs);

            return r;
        }
        set
        {
            _primaryNote.Point = value;

            var nPs = _notePoints[value];
            for (int i = 0; i < nPs.Length; i++) if (nPs[i] != null && _subordinateNotes[i] is INote note) note.Point = nPs[i]!.Value; 
        }
    }
    public INote PrimaryNote => _primaryNote;
    public IEnumerable<INote> Notes 
    {
        get
        {
            return Enumerate();
            IEnumerable<INote> Enumerate()
            {
                yield return _primaryNote;
                foreach (var note in _subordinateNotes) if (note != null) yield return note;
            }
        }
    }

    public void Add(INote note)
    {
        if (Notes.Contains(note)) throw new ArgumentException("指定された冊は既に重ねられています。");

        if (++_subordinateNotesLastIndex < _subordinateNotes.Length)
        {
            _subordinateNotes[_subordinateNotesLastIndex] = note;
        }
        else
        {
            var sNs = _subordinateNotes;
            _subordinateNotes = new INote[sNs.Length * 2];
            sNs.CopyTo(_subordinateNotes, 0);
            _subordinateNotes[_subordinateNotesLastIndex] = note;
        }
    }
    public void Remove(INote note)
    {
        if (Equals(note, _primaryNote)) throw new ArgumentException("指定された冊は最上位であり、除去することはできません。");

        for (int i = 0; i < _subordinateNotesLastIndex; i++) if (_subordinateNotes[i] is INote n && n.Equals(note)) _subordinateNotes[i] = null;
        throw new ArgumentException("指定された冊は重ねられていませんでした。");
    }

    public INote Copy() => _primaryNote.Copy();
    public Task<INote> CopyAsync() => _primaryNote.CopyAsync();
    public void Dispose()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    public virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                foreach (var note in Notes) note.Dispose();
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
    public virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                Tasks tasks = default;
                foreach (var note in Notes) tasks += note.DisposeAsync().AsTask();
                await tasks.WhenAll();
            }

            // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
            // TODO: 大きなフィールドを null に設定します
            _isDisposed = true;
        }
    }
    public Task Insert(in NotePoint index)
    {
        Tasks tasks = default;
        foreach (var note in Notes) tasks += note.Insert(index: index);
        return tasks.WhenAll();
    }
    public Task Insert<T>(Memory<T> memory) where T : unmanaged
    {
        Tasks tasks = default;
        foreach (var note in Notes) tasks += note.Insert(memory: memory);
        return tasks.WhenAll();
    }
    public void InsertSync<T>(Span<T> span) where T : unmanaged
    {
        foreach (var note in Notes) note.InsertSync(span: span);
    }
    public bool IsValid(NotePoint index)
    {
        if (!TryIsValid(index, out var result)) throw new NoteDoesNotMatchException() { Notes = Notes };
        return result;
    }
    public Task Remove(out NotePoint index)
    {
        if (!TryRemove(out index)) throw new NoteDoesNotMatchException() { Notes = Notes };
        return Task.CompletedTask;
    }
    public Task Remove<T>(Memory<T> memory) where T : unmanaged
    {
        if (!TryRemove(memory)) throw new NoteDoesNotMatchException() { Notes = Notes };
        return Task.CompletedTask;
    }
    public void RemoveSync<T>(Span<T> span) where T : unmanaged
    {
        if (!TryRemoveSync(span)) throw new NoteDoesNotMatchException() { Notes = Notes };
    }

    public bool TryIsValid(NotePoint index, out bool result)
    {
        bool r = true;
        result = _primaryNote.IsValid(index);
        foreach (var note in _subordinateNotes) if (note != null && result != note.IsValid(index)) r = false;
        return r;
    }
    public bool TryRemove(out NotePoint index)
    {
        bool r = true;
        _primaryNote.Remove(index: out index).Wait();
        foreach (var note in _subordinateNotes)
        {
            if (note == null) continue;

            note.Remove(index: out var v).Wait();
            if (index != v) r = false;
        }
        return r;
    }
    public bool TryRemove<T>(Memory<T> memory) where T : unmanaged
    {
        bool r = true;
        _primaryNote.Remove(memory: memory);
        Span<T> v = memory.Length > ConstantValues.STACKALLOC_MAX_LENGTH ? new T[memory.Length] : stackalloc T[memory.Length];
        memory.Span.CopyTo(v);
        foreach (var note in _subordinateNotes)
        {
            if (note == null) continue;

            note.Remove(memory: memory).Wait();
            if (!memory.Span.SequenceEqual(v)) r = false;
        }
        return r;
    }
    public bool TryRemoveSync<T>(Span<T> span) where T : unmanaged
    {
        bool r = true;
        _primaryNote.RemoveSync(span: span);
        Span<T> v = span.Length > ConstantValues.STACKALLOC_MAX_LENGTH ? new T[span.Length] : stackalloc T[span.Length];
        foreach (var note in _subordinateNotes)
        {
            if (note == null) continue;

            note.RemoveSync(span: span);
            if (!v.SequenceEqual(span)) r = false;
        }
        return r;
    }
}
