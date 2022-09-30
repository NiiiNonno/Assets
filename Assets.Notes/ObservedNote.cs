using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets;

public readonly struct ObservedNote : INote
{
    readonly INote _note;

    public INote Note => _note;
    public NotePoint Point { get => _note.Point; set => _note.Point = value; }

    public ObservedNote(INote note)
    {
        _note = note;
    }

    public bool IsValid(NotePoint index) => _note.IsValid(index);

    public INote Copy() => _note.Copy();

    public Task Insert(in NotePoint index)
    {
        return _note.Insert(index);
    }
    public Task Remove(out NotePoint index)
    {
        return _note.Remove(out index);
    }

    public Task Insert<T>(Memory<T> memory) where T : unmanaged
    {
        return _note.Insert(memory);
    }

    public void InsertSync<T>(Span<T> span) where T : unmanaged
    {
        _note.InsertSync(span);
    }

    public Task Remove<T>(Memory<T> memory) where T : unmanaged
    {
        return _note.Remove(memory);
    }

    public void RemoveSync<T>(Span<T> span) where T : unmanaged
    {
        _note.RemoveSync(span);
    }

    public void Dispose()
    {
        _note.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return _note.DisposeAsync();
    }
}