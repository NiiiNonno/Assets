using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Nonno.Assets.Collections;
using static Nonno.Assets.Sample;
using SysCG = System.Collections.Generic;

namespace Nonno.Assets.Scrolls;

// 一巻子も無かった場合は、`Insert`系は無視、`Remove(NotePoint)`はほぼ空で返し、`Remove<T>`系は何も書かず返す。
public class DuplicatingScroll : IScroll
{
    readonly ArrayList<Relay> _relays;
    bool _isDisposed;

    public DuplicatingScroll(int defaultSubordinateNotesCapacity = 1)
    {
        if (defaultSubordinateNotesCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(defaultSubordinateNotesCapacity));

        _relays = new();
    }
    public DuplicatingScroll(DuplicatingScroll original)
    {
        var relays = new ArrayList<Relay>(original._relays.Capacity);
        for (int i = 0; i < relays.Count; i++) relays[i] = new(original[i].Copy());

        _relays = relays;
        _isDisposed = original._isDisposed;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public ScrollPointer Point 
    {
        get
        {
            var points = new (Relay, ScrollPointer)[Count];
            for (int i = 0; i < points.Length; i++) points[i] = (_relays[i], this[i].Point);
            return new(information: points);
        }
        set
        {
            int count = 0;

            if (value.Information is not (Relay, ScrollPointer)[] points) throw new ArgumentException("軸箋の出所が異なります。", nameof(value));
            foreach (var (relay, point) in points)
            {
                if (relay.Note is IScroll note) 
                {
                    count++;
                    note.Point = point; 
                }
            }

            if (count != Count) throw new Exception("対処可能な中継の数が複巻子中の巻子の数より少なく、即ち複巻子中に軸箋に記載のない巻子が存在しているため、軸箋を設定することができません。");
        }
    }
    public int Count => _relays.Count;
    public SysCG::IEnumerable<IScroll> Scrolls 
    {
        get
        {
            return Enumerate();
            SysCG::IEnumerable<IScroll> Enumerate()
            {
                foreach (var relay in _relays) yield return relay.Note ?? throw new Exception("不明な錯誤です。重ねられている巻子の中継が無効でした。");
            }
        }
    }
    public IScroll this[int number] => _relays[number].Note ?? throw new IndexOutOfRangeException();

    public void Put(IScroll scroll)
    {
        if (Scrolls.Contains(scroll)) throw new ArgumentException("指定された巻子は既に重ねられています。");

        _relays.Add(item: new(scroll));
    }
    public void Take(IScroll scroll)
    {
        for (int i = 0; i < _relays.Count; i++)
        {
            if (Equals(_relays[i].Note, scroll))
            {
                var relay = _relays[i];
                _relays.Remove(at: i);
                relay.Note = null;

                return;
            }
        }

        throw new ArgumentException("指定された巻子は重ねられていませんでした。");
    }

    public IScroll Copy() => new DuplicatingScroll(this);
    public Task<IScroll> CopyAsync() => Task.FromResult(Copy());
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
                foreach (var scroll in Scrolls) scroll.Dispose();
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
                foreach (var note in Scrolls) tasks += note.DisposeAsync().AsTask();
                await tasks.WhenAll();
            }

            // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
            // TODO: 大きなフィールドを null に設定します
            _isDisposed = true;
        }
    }
    public Task Insert(in ScrollPointer index)
    {
        if (index.Information is not (Relay, ScrollPointer)[] points) throw new ArgumentException("軸箋の出所が異なります。", nameof(index));

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
        foreach (var note in Scrolls) tasks += note.Insert(memory: memory);
        return tasks.WhenAll();
    }
    public void InsertSync<T>(Span<T> span) where T : unmanaged
    {
        foreach (var note in Scrolls) note.InsertSync(span: span);
    }
    public bool IsValid(ScrollPointer pointer) => IsValid(pointer, true);
    public bool IsValid(ScrollPointer pointer, bool throwWhenNoteDoesNotMatch = true)
    {
        if (pointer.Information is not (Relay, ScrollPointer)[] info) return false;

        var count = 0;
        var r = true;
        foreach (var (relay, point) in info)
        {
            if (relay.Note is IScroll note)
            {
                count++;
                if (!note.IsValid(point)) r = false;
            }
        }

        if (count != Count) return false;
        else return r;
    }
    public Task Remove(out ScrollPointer pointer)
    {
        var info = new (Relay, ScrollPointer)[_relays.Count];
        
        for (int i = 0; i < info.Length; i++)
        {
            this[i].Remove(out ScrollPointer p).Wait();
            info[i] = (_relays[i], p);
        }

        pointer = new(information: info);
        return Task.CompletedTask;
    }
    public Task Remove<T>(Memory<T> memory) where T : unmanaged
    {
        if (!TryRemove(memory)) throw new ScrollDoesNotMatchException() { Notes = Scrolls };
        return Task.CompletedTask;
    }
    public void RemoveSync<T>(Span<T> span) where T : unmanaged
    {
        if (!TryRemoveSync(span)) throw new ScrollDoesNotMatchException() { Notes = Scrolls };
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

    public long FigureOutDistance<T>(ScrollPointer to)
    {
        long? r = null;
        foreach (var note in Scrolls)
        {
            var v = note.FigureOutDistance<T>(to);
            if (r.HasValue && r.Value != v) return -1;
            r = v;
        }
        return r ?? 0;
    }
    public bool Is(ScrollPointer on)
    {
        bool? r = null;
        foreach (var scroll in Scrolls)
        {
            var v = scroll.Is(on);
            if (r.HasValue && r.Value != v) throw new ScrollDoesNotMatchException();
            r = v;
        }
        return r ?? true;
    }

    class Relay
    {
        public IScroll? Note { get; set; }

        public Relay(IScroll note) => Note = note;
    }
}
