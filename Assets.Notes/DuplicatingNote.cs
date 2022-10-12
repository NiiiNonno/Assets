using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Nonno.Assets.Collections;
using static Nonno.Assets.Sample;
using SysCG = System.Collections.Generic;

namespace Nonno.Assets.Notes;

// 一巻子も無かった場合は、`Insert`系は無視、`Remove(NotePoint)`はほぼ空で返し、`Remove<T>`系は何も書かず返す。
public class DuplicatingNote : INote
{
    readonly ArrayList<Relay> _relays;
    bool _isDisposed;

    public DuplicatingNote(int defaultSubordinateNotesCapacity = 1)
    {
        if (defaultSubordinateNotesCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(defaultSubordinateNotesCapacity));

        _relays = new();
    }
    public DuplicatingNote(DuplicatingNote original)
    {
        var relays = new ArrayList<Relay>(original._relays.Capacity);
        for (int i = 0; i < relays.Count; i++) relays[i] = new(original[i].Copy());

        _relays = relays;
        _isDisposed = original._isDisposed;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public NotePointer Pointer 
    {
        get
        {
            var points = new (Relay, NotePointer)[Count];
            for (int i = 0; i < points.Length; i++) points[i] = (_relays[i], this[i].Pointer);
            return new(information: points);
        }
        set
        {
            int count = 0;

            if (value.Information is not (Relay, NotePointer)[] points) throw new ArgumentException("指示子の出所が異なります。", nameof(value));
            foreach (var (relay, point) in points)
            {
                if (relay.Note is INote note) 
                {
                    count++;
                    note.Pointer = point; 
                }
            }

            if (count != Count) throw new Exception("対処可能な中継の数が複巻子中の巻子の数より少なく、即ち複巻子中に指示子に記載のない巻子が存在しているため、指示子を設定することができません。");
        }
    }
    public int Count => _relays.Count;
    public SysCG::IEnumerable<INote> Notes 
    {
        get
        {
            return Enumerate();
            SysCG::IEnumerable<INote> Enumerate()
            {
                foreach (var relay in _relays) yield return relay.Note ?? throw new Exception("不明な錯誤です。重ねられている巻子の中継が無効でした。");
            }
        }
    }
    public INote this[int number] => _relays[number].Note ?? throw new IndexOutOfRangeException();

    public void Put(INote note)
    {
        if (Notes.Contains(note)) throw new ArgumentException("指定された巻子は既に重ねられています。");

        _relays.Add(item: new(note));
    }
    public void Take(INote note)
    {
        for (int i = 0; i < _relays.Count; i++)
        {
            if (Equals(_relays[i].Note, note))
            {
                var relay = _relays[i];
                _relays.Remove(at: i);
                relay.Note = null;

                return;
            }
        }

        throw new ArgumentException("指定された巻子は重ねられていませんでした。");
    }

    public INote Copy() => new DuplicatingNote(this);
    public Task<INote> CopyAsync() => Task.FromResult(Copy());
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
    public Task Insert(in NotePointer index)
    {
        if (index.Information is not (Relay, NotePointer)[] points) throw new ArgumentException("指示子の出所が異なります。", nameof(index));

        Tasks tasks = default;
        foreach (var (relay, point) in points)
        {
            var note = relay.Note ?? throw new Exception("不明な錯誤です。重ねられている巻子の中継が無効でした。");
            tasks += note.Insert(point);
        }

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
    public bool IsValid(NotePointer pointer) => IsValid(pointer, true);
    public bool IsValid(NotePointer pointer, bool throwWhenNoteDoesNotMatch = true)
    {
        if (pointer.Information is not (Relay, NotePointer)[] info) return false;

        var count = 0;
        var r = true;
        foreach (var (relay, point) in info)
        {
            if (relay.Note is INote note)
            {
                count++;
                if (!note.IsValid(point)) r = false;
            }
        }

        if (count != Count) return false;
        else return r;
    }
    public Task Remove(out NotePointer pointer)
    {
        var info = new (Relay, NotePointer)[_relays.Count];
        
        for (int i = 0; i < info.Length; i++)
        {
            this[i].Remove(out NotePointer p).Wait();
            info[i] = (_relays[i], p);
        }

        pointer = new(information: info);
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

    public bool TryRemove<T>(Memory<T> memory) where T : unmanaged
    {
        if (_relays.Count == 0) return true;
        if (_relays.Count == 1)
        {
            _relays[0].Note!.Remove(memory: memory).Wait();
            return true;
        }

        bool r = true;

        Span<T> v = memory.Length > ConstantValues.STACKALLOC_MAX_LENGTH ? new T[memory.Length] : stackalloc T[memory.Length];
        this[0].RemoveSync(span: v);

        for (int i = 1; i < _relays.Count; i++)
        {
            this[i].Remove(memory: memory).Wait();
            if (!memory.Span.SequenceEqual(v)) r = false;
        }

        return r;
    }
    public bool TryRemoveSync<T>(Span<T> span) where T : unmanaged
    {
        if (_relays.Count == 0) return true;

        bool r = true;

        Span<T> v = span.Length > ConstantValues.STACKALLOC_MAX_LENGTH ? new T[span.Length] : stackalloc T[span.Length];
        this[0].RemoveSync(span: v);

        for (int i = 1; i < _relays.Count; i++)
        {
            this[i].RemoveSync(span: span);
            if (!span.SequenceEqual(v)) r = false;
        }

        return r;
    }

    public long FigureOutDistance<T>(NotePointer to)
    {
        long? r = null;
        foreach (var note in Notes)
        {
            var v = note.FigureOutDistance<T>(to);
            if (r.HasValue && r.Value != v) return -1;
            r = v;
        }
        return r ?? 0;
    }

    class Relay
    {
        public INote? Note { get; set; }

        public Relay(INote note) => Note = note;
    }
}
