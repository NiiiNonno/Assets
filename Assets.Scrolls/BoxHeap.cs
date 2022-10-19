using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Nonno.Assets.Collections;
using SysCG = System.Collections.Generic;
using System.Threading;
using System.Reflection.Metadata;
using System.Reflection;
using System.Linq;
using System.Data;
using System.Diagnostics;

namespace Nonno.Assets.Scrolls;
public sealed class BoxHeap : IHeap<IDataBox>, IDisposable, IAsyncDisposable
{
    readonly IScroll _scroll;
    readonly LinkedList<IDataBox> _onMemories;
    readonly LinkedList<(Type type, ScrollPointer pointer)> _pointers;
    bool _isDisposed;

    private BoxHeap(IScroll scroll, LinkedList<(Type type, ScrollPointer pointer)> pointers)
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
                _scroll.Point = c.Value.pointer;
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
                var p = _scroll.Point;
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
                var p = _scroll.Point;
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
                _scroll.Point = c.Value.pointer;
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
                _scroll.Point = c.Value.pointer;
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
                _scroll.Point = c.Value.pointer;
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
                _scroll.Point = c.Value.pointer;
                await _scroll.Remove(dataBox: out var box);
                _ = _onMemories.AddLast(box);
                _pointers.Remove(c);
                return box;
            }
            c = c.Next;
        }

        return default;
    }
    public async ValueTask<T?> Move<T>(T? dataBox) where T : notnull, IDataBox
    {
        var c_ = _onMemories.First;
        while (c_ != null)
        {
            if (c_.Value is T t)
            {
                if (dataBox is null) _onMemories.Remove(c_);
                else c_.Value = dataBox;
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
                if (dataBox is null)
                {
                    _scroll.Point = c.Value.pointer;
                    await _scroll.Remove(dataBox: out var box);
                    _pointers.Remove(c);
                    return (T)box;
                }
                else
                {
                    var p = _scroll.Point = c.Value.pointer;
                    await _scroll.Remove(dataBox: out var box);
                    await _scroll.Insert(dataBox: dataBox);
                    c.Value = (c.Value.type, p);
                    return (T)box;
                }
            }
            c = c.Next;
        }

        if (dataBox is not null) _onMemories.AddLast(dataBox);
        return default;
    }
    public async ValueTask<IDataBox?> Move(Type type, IDataBox? dataBox)
    {
        if (dataBox is not null && !dataBox.GetType().IsAssignableTo(type)) throw new ArgumentException("函が指定された型ではありません。", nameof(dataBox));

        var c_ = _onMemories.First;
        while (c_ != null)
        {
            if (c_.Value.GetType().IsAssignableTo(type))
            {
                if (dataBox is null) _onMemories.Remove(c_);
                else c_.Value = dataBox;
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
                if (dataBox is null)
                {
                    _scroll.Point = c.Value.pointer;
                    await _scroll.Remove(dataBox: out var box);
                    _pointers.Remove(c);
                    return box;
                }
                else
                {
                    var p = _scroll.Point = c.Value.pointer;
                    await _scroll.Remove(dataBox: out var box);
                    await _scroll.Insert(dataBox: dataBox);
                    c.Value = (c.Value.type, p);
                    return box;
                }
            }
            c = c.Next;
        }

        if (dataBox is not null) _onMemories.AddLast(dataBox);
        return default;
    }
    public async Task Set<T>(T? dataBox) where T : notnull, IDataBox
    {
        var c_ = _onMemories.First;
        while (c_ != null)
        {
            if (c_.Value is T)
            {
                if (dataBox is null) _onMemories.Remove(c_);
                else c_.Value = dataBox;
                return;
            }
            c_ = c_.Next;
        }

        var c = _pointers.First;
        while (c != null)
        {
            if (c.Value.type.IsAssignableTo(typeof(T)))
            {
                // 除外されている可能性があるから搴取して確かめなければならない。
                if (dataBox is null)
                {
                    _scroll.Point = c.Value.pointer;
                    Debug.WriteLine("行落不奨処理。函積に対し設定務容を働くことは既存の函の不遇な破棄をもたらす可能性があります。");
                    _scroll.RemoveDataBox();//上記が問題視するのはここ。
                    _pointers.Remove(c);
                }
                else
                {
                    var p = _scroll.Point = c.Value.pointer;
                    Debug.WriteLine("行落不奨処理。函積に対し設定務容を働くことは既存の函の不遇な破棄をもたらす可能性があります。");
                    _scroll.RemoveDataBox();//上記が問題視するのはここ。
                    await _scroll.Insert(dataBox: dataBox);
                    c.Value = (c.Value.type, p);
                }
            }
            c = c.Next;
        }

        if (dataBox is not null) _onMemories.AddLast(dataBox);
    }
    public async Task Set(Type type, IDataBox? dataBox)
    {
        if (dataBox is not null && !dataBox.GetType().IsAssignableTo(type)) throw new ArgumentException("函が指定された型ではありません。", nameof(dataBox));

        var c_ = _onMemories.First;
        while (c_ != null)
        {
            if (c_.Value.GetType().IsAssignableTo(type))
            {
                if (dataBox is null) _onMemories.Remove(c_);
                else c_.Value = dataBox;
                return;
            }
            c_ = c_.Next;
        }

        var c = _pointers.First;
        while (c != null)
        {
            if (c.Value.type.IsAssignableTo(type))
            {
                // 除外されている可能性があるから搴取して確かめなければならない。
                if (dataBox is null)
                {
                    _scroll.Point = c.Value.pointer;
                    Debug.WriteLine("行落不奨処理。函積に対し設定務容を働くことは既存の函の不遇な破棄をもたらす可能性があります。");
                    _scroll.RemoveDataBox();//上記が問題視するのはここ。
                    _pointers.Remove(c);
                    return;
                }
                else
                {
                    var p = _scroll.Point = c.Value.pointer;
                    Debug.WriteLine("行落不奨処理。函積に対し設定務容を働くことは既存の函の不遇な破棄をもたらす可能性があります。");
                    _scroll.RemoveDataBox();//上記が問題視するのはここ。
                    await _scroll.Insert(dataBox: dataBox);
                    c.Value = (c.Value.type, p);
                    return;
                }
            }
            c = c.Next;
        }

        if (dataBox is not null) _onMemories.AddLast(dataBox);
    }

    public async SysCG::IAsyncEnumerator<IDataBox> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        foreach (var item in _onMemories)
        {
            yield return item;
        }

        foreach (var pair in _pointers)
        {
            _scroll.Point = pair.pointer;
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

    public static async ValueTask<BoxHeap> Instantiate(IScroll scroll, ScrollPointer? endPoint = null)
    {
        var ps = new LinkedList<(Type type, ScrollPointer pointer)>();
        while (true)
        {
            var p_0 = scroll.Point;
            await scroll.Remove(pointer: out var p_next);
            await scroll.Remove(typeIdentifier: out var id);
            var p_1 = scroll.Point;

            p_next = scroll.Point = p_next;
            var p_e = scroll.Point;

            scroll.Point = p_1;
            await scroll.Insert(pointer: p_next);
            await scroll.Insert(typeIdentifier: id);
            scroll.Point = p_e;

            _ = ps.AddLast((id.GetIdentifiedType(), p_0));
            if (endPoint.HasValue && scroll.FigureOutDistance<byte>(to: endPoint.Value) == 0) break;
        }

        return new(scroll, ps);
    }
}