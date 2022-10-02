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
    public NotePointer Pointer { get => _note.Pointer; set => _note.Pointer = value; }

    public ObservedNote(INote note)
    {
        _note = note;
    }

    public bool IsValid(NotePointer pointer) => _note.IsValid(pointer);

    public INote Copy() => _note.Copy();

    public Task Insert(in NotePointer pointer)
    {
        return _note.Insert(pointer);
    }
    public Task Remove(out NotePointer pointer)
    {
        return _note.Remove(out pointer);
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