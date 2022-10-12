using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Nonno.Assets.Collections;
using SysCG = System.Collections.Generic;
using IScroll = Nonno.Assets.INote;
using ScrollPointer = Nonno.Assets.NotePointer;
using System.Threading;
using System.Reflection.Metadata;
using System.Reflection;
using System.Linq;

namespace Nonno.Assets.Notes;
public sealed class BoxList : IHeap<IDataBox>, IDisposable, IAsyncDisposable
{
    readonly IScroll _scroll;
    readonly LinkedList<IDataBox> _onMemories;
    readonly LinkedList<(Type type, ScrollPointer pointer)> _pointers;
    bool _isDisposed;

    private BoxList(IScroll scroll, LinkedList<(Type type, ScrollPointer pointer)> pointers)
    {
        _scroll = scroll;
        _onMemories = new();
        _pointers = pointers;
    }

    public void Dispose()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                Save<object>().Wait();
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
    async ValueTask DisposeAsync(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                await Save<object>();
            }

            _isDisposed = true;
        }
    }

    public Task Add(IDataBox @object)
    {
        _ = _onMemories.AddLast(@object);
        return Task.CompletedTask;
    }
    public async Task Remove(IDataBox @object)
    {
        if (_onMemories.TryRemove(@object)) return;

        await Load(@object.GetType());
        if (_onMemories.TryRemove(@object)) return;
        throw new SysCG::KeyNotFoundException();
    }

    public async Task Load(Type type)
    {
        var c = _pointers.First;
        while (c != null)
        {
            if (c.Value.type.IsAssignableTo(type))
            {
                _scroll.Pointer = c.Value.pointer;
                await _scroll.Remove(dataBox: out var box);
                _ = _onMemories.AddLast(box);
                _pointers.Remove(c);
            }
            c = c.Next;
        }
    }
    public Task Load<T>() => Load(typeof(T));
    public async Task Save(Type type)
    {
        var c = _onMemories.First;
        while (c != null)
        {
            if (c.Value.GetType().IsAssignableTo(type))
            {
                var p = _scroll.Pointer;
                await _scroll.Insert(c.Value);
                _ = _pointers.AddLast((c.Value.GetType(), p));
                _onMemories.Remove(c);
            }
            c = c.Next;
        }
    }
    public async Task Save<T>()
    {
        var c = _onMemories.First;
        while (c != null)
        {
            if (c.Value is T)
            {
                var p = _scroll.Pointer;
                await _scroll.Insert(c.Value);
                _ = _pointers.AddLast((c.Value.GetType(), p));
                _onMemories.Remove(c);
            }
            c = c.Next;
        }
    }

    public async ValueTask<bool> Contains<T>() where T : notnull, IDataBox
    {
        foreach (var item in _onMemories)
        {
            if (item is T) return true;
        }

        var c = _pointers.First;
        while (c != null)
        {
            if (c.Value.type.IsAssignableTo(typeof(T)))
            {
                _scroll.Pointer = c.Value.pointer;
                await _scroll.Remove(dataBox: out var box);
                _ = _onMemories.AddLast(box);
                _pointers.Remove(c);
                return true;
            }
            c = c.Next;
        }

        return false;
    }
    public async ValueTask<bool> Contains(Type type)
    {
        foreach (var item in _onMemories)
        {
            if (item.GetType().IsAssignableTo(type)) return true;
        }

        var c = _pointers.First;
        while (c != null)
        {
            if (c.Value.type.IsAssignableTo(type))
            {
                _scroll.Pointer = c.Value.pointer;
                await _scroll.Remove(dataBox: out var box);
                _ = _onMemories.AddLast(box);
                _pointers.Remove(c);
                return true;
            }
            c = c.Next;
        }

        return false;
    }
    public async ValueTask<T?> Get<T>() where T : notnull, IDataBox
    {
        foreach (var item in _onMemories)
        {
            if (item is T t) return t;
        }

        var c = _pointers.First;
        while (c != null)
        {
            if (c.Value.type.IsAssignableTo(typeof(T)))
            {
                _scroll.Pointer = c.Value.pointer;
                await _scroll.Remove(dataBox: out var box);
                _ = _onMemories.AddLast(box);
                _pointers.Remove(c);
                return (T)box;
            }
            c = c.Next;
        }

        return default;
    }
    public async ValueTask<IDataBox?> Get(Type type)
    {
        foreach (var item in _onMemories)
        {
            if (item.GetType().IsAssignableTo(type)) return item;
        }

        var c = _pointers.First;
        while (c != null)
        {
            if (c.Value.type.IsAssignableTo(type))
            {
                _scroll.Pointer = c.Value.pointer;
                await _scroll.Remove(dataBox: out var box);
                _ = _onMemories.AddLast(box);
                _pointers.Remove(c);
                return box;
            }
            c = c.Next;
        }

        return default;
    }
    public async ValueTask<T?> Remove<T>() where T : notnull, IDataBox
    {
        var c_ = _onMemories.First;
        while (c_ != null)
        {
            if (c_.Value is T t)
            {
                _onMemories.Remove(c_);
                return t;
            }
            c_ = c_.Next;
        }

        var c = _pointers.First;
        while (c != null)
        {
            if (c.Value.type.IsAssignableTo(typeof(T)))
            {
                // 除外されている可能性があるから搴取して確かめなければならない。
                _scroll.Pointer = c.Value.pointer;
                await _scroll.Remove(dataBox: out var box);
                _pointers.Remove(c);
                return (T)box;
            }
            c = c.Next;
        }

        return default;
    }
    public async ValueTask<IDataBox?> Remove(Type type)
    {
        var c_ = _onMemories.First;
        while (c_ != null)
        {
            if (c_.Value.GetType().IsAssignableTo(type))
            {
                _onMemories.Remove(c_);
                return c_.Value;
            }
            c_ = c_.Next;
        }

        var c = _pointers.First;
        while (c != null)
        {
            if (c.Value.type.IsAssignableTo(type))
            {
                // 除外されている可能性があるから搴取して確かめなければならない。
                _scroll.Pointer = c.Value.pointer;
                await _scroll.Remove(dataBox: out var box);
                _pointers.Remove(c);
                return box;
            }
            c = c.Next;
        }

        return default;
    }

    public async SysCG::IAsyncEnumerator<IDataBox> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        foreach (var item in _onMemories)
        {
            yield return item;
        }

        foreach (var pair in _pointers)
        {
            _scroll.Pointer = pair.pointer;
            await _scroll.Remove(dataBox: out var box);
            _onMemories.AddLast(box);
            yield return box;

            if (cancellationToken.IsCancellationRequested)
            {
                var c = _pointers.Find(pair);
                while (c != null)
                {
                    _pointers.Remove(c);
                    c = c.Previous;
                }
                throw new OperationCanceledException(token: cancellationToken);
            }
        }
        _pointers.Clear();
    }
    public SysCG::IEnumerator<IDataBox> GetEnumerator()
    {
        Load<object>().Wait();
        return _onMemories.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static async ValueTask<BoxList> Instantiate(IScroll scroll, ScrollPointer? endPoint = null)
    {
        var ps = new LinkedList<(Type type, ScrollPointer pointer)>();
        while (true)
        {
            var p_0 = scroll.Pointer;
            await scroll.Remove(pointer: out var p_next);
            await scroll.Remove(typeIdentifier: out var id);
            var p_1 = scroll.Pointer;

            p_next = scroll.Pointer = p_next;
            var p_e = scroll.Pointer;

            scroll.Pointer = p_1;
            await scroll.Insert(pointer: p_next);
            await scroll.Insert(typeIdentifier: id);
            scroll.Pointer = p_e;

            _ = ps.AddLast((id.GetIdentifiedType(), p_0));
            if (endPoint.HasValue && scroll.FigureOutDistance<byte>(to: endPoint.Value) == 0) break;
        }

        return new(scroll, ps);
    }
}