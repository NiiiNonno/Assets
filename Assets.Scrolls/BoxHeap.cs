using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Nonno.Assets.Collections;
using SysCG = System.Collections.Generic;

namespace Nonno.Assets.Scrolls;
public sealed class BoxHeap : Heap<IDataBox>, IDisposable, IAsyncDisposable
{
    readonly IScroll _scroll;
    readonly LinkedList<DataBoxInfo> _infos;
    bool _isDisposed;

    public override int Count => _infos.Count;
    public Index FieldIndex { get; set; }

    private BoxHeap(IScroll scroll, LinkedList<DataBoxInfo> infos)
    {
        _scroll = scroll;
        _infos = infos;

        FieldIndex = ^1;
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
                // 変更は`DisposeAsync`から。
                var c = _infos.First;
                while (c is not null)
                {
                    // 途中に未だ搴られていない嘼があったら、その前に挿す。
                    if (c.Value.Point is ScrollPointer p)
                    {
                        _scroll.Point = p;
                        do
                        {
                            _scroll.Insert(_infos.First!.Value.GetDataBoxSync(_scroll)).Wait();
                            _infos.RemoveFirst();
                        }
                        while (!c.Equals(_infos.First));
                    }

                    c = c.Next;
                }

                // 最後に、残り全てを後ろに挿す。
                foreach (var info in _infos)
                {
                    _scroll.Insert(info.GetDataBoxSync(_scroll)).Wait();
                }
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
                // 変更時、`Dispose`内も変更。
                var c = _infos.First;
                while (c is not null)
                {
                    // 途中に未だ搴られていない嘼があったら、その前に挿す。
                    if (c.Value.Point is ScrollPointer p)
                    {
                        _scroll.Point = p;
                        do
                        {
                            await _scroll.Insert(await _infos.First!.Value.GetDataBox(_scroll));
                            _infos.RemoveFirst();
                        }
                        while (!c.Equals(_infos.First));
                    }

                    c = c.Next;
                }

                // 最後に、残り全てを後ろに挿す。
                foreach (var info in _infos)
                {
                    await _scroll.Insert(await info.GetDataBox(_scroll));
                }
            }

            _isDisposed = true;
        }
    }

    public async override ValueTask<bool> Contains<T>()
    {
        foreach (var info in _infos)
        {
            if (info.DataBoxIsOnMemory && await info.GetDataBox(_scroll) is T) return true; 
        }
        foreach (var info in _infos)
        {
            if (!info.DataBoxIsOnMemory && info.Type.IsAssignableTo(typeof(T))) return true;
        }
        return false;
    }
    public override ValueTask<bool> Contains(Type type)
    {
        foreach (var info in _infos)
        {
            if (info.Type.IsAssignableTo(type)) return new ValueTask<bool>(true);
        }
        return new ValueTask<bool>(false);
    }

    public async override ValueTask<IDataBox?> Get(Type type)
    {
        var c = _infos.First;
        while (c is not null)
        {
            if (c.Value.Type.IsAssignableTo(type)) 
            {
                var t = c.Value.GetDataBox(_scroll);
                _infos.Remove(c);
                return await t;
            }

            c = c.Next;
        }

        return default;
    }

    public override async ValueTask<IDataBox?> Move(Type type, IDataBox? @object)
    {
        if (@object is null) return await Get(type);

        if (!@object.GetType().IsAssignableTo(type)) throw new ArgumentException("型が制約に反します。");

        var c = _infos.First;
        while (c is not null)
        {
            if (c.Value.Type.IsAssignableTo(type))
            {
                var t = c.Value.GetDataBox(_scroll);
                c.Value = new(@object);
                return await t;
            }

            c = c.Next;
        }

        return default;
    }

    public override Task Set(Type type, IDataBox? @object)
    {
        if (@object is null) return Task.CompletedTask;

        if (!@object.GetType().IsAssignableTo(type)) throw new ArgumentException("型が制約に反します。");

        var c = _infos.First;
        while (c is not null)
        {
            if (c.Value.Type.IsAssignableTo(type))
            {
                _infos.AddBefore(c, new DataBoxInfo(@object));
                return Task.CompletedTask;
            }

            c = c.Next;
        }

        return Task.CompletedTask;
    }

    public override bool Contains(IDataBox item)
    {
        foreach (var info in _infos)
        {
            if (info.DataBoxIsOnMemory && info.GetDataBoxSync(_scroll).Equals(item)) return true;
        }
        foreach (var info in _infos)
        {
            if (!info.DataBoxIsOnMemory && info.GetDataBoxSync(_scroll).Equals(item)) return true;
        }

        return false;
    }

    public override void Add(IDataBox item)
    {
        var v = FieldIndex.Value;
        if (v < 0)
        {
            v = ~v;
            var c = _infos.Last;
            for (int i = 0; i < v; i++) c = c!.Previous;
            _ = _infos.AddBefore(c!, new DataBoxInfo(item));
        }
        else
        {
            var c = _infos.First;
            for (int i = 0; i < v; i++) c = c!.Next;
            _ = _infos.AddAfter(c!, new DataBoxInfo(item));
        }
    }
    public override async Task RemoveAsync(IDataBox @object)
    {
        var type = @object.GetType();

        var c = _infos.First;
        while (c is not null)
        {
            if (c.Value.Type == type)
            {
                if (@object.Equals(await c.Value.GetDataBox(_scroll))) _infos.Remove(c);
            }

            c = c.Next;
        }
    }
    public override void Remove(IDataBox item) => RemoveAsync(item).Wait();

    public override SysCG::IEnumerator<IDataBox> GetEnumerator()
    {
        foreach (var info in _infos)
        {
            yield return info.GetDataBoxSync(_scroll);
        }
    }
    public override async SysCG::IAsyncEnumerator<IDataBox> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        foreach (var info in _infos)
        {
            yield return await info.GetDataBox(_scroll);
        }
    }

    public static async ValueTask<BoxHeap> Instantiate(IScroll scroll, TypeIdentifier trailerBoxTypeId)
    {
        var ps = new LinkedList<DataBoxInfo>();
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

            _ = ps.AddLast(new DataBoxInfo(id.GetIdentifiedType(), p_0));
            if (id == trailerBoxTypeId) break;
        }

        return new(scroll, ps);
    }
    public static async ValueTask<BoxHeap> Instantiate(IScroll scroll, ScrollPointer pointer)
    {
        var ps = new LinkedList<DataBoxInfo>();
        while (!scroll.Is(on: pointer))
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

            _ = ps.AddLast(new DataBoxInfo(id.GetIdentifiedType(), p_0));
        }

        return new(scroll, ps);
    }
    public static async ValueTask<BoxHeap> Instantiate(IScroll scroll, int count = 0)
    {
        var ps = new LinkedList<DataBoxInfo>();
        for (int i = 0; i < count; i++)
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

            _ = ps.AddLast(new DataBoxInfo(id.GetIdentifiedType(), p_0));
        }

        return new(scroll, ps);
    }

    public sealed class DataBoxInfo
    {
        readonly Type _type;
        IDataBox? _dataBox;
        ScrollPointer? _point;

        public Type Type => _type;
        public ScrollPointer? Point => _point;
        public bool DataBoxIsOnMemory => _dataBox is not null;

        public DataBoxInfo(IDataBox dataBox)
        {
            _type = dataBox.GetType();
            _dataBox = dataBox;
        }
        public DataBoxInfo(Type type, ScrollPointer point)
        {
            _type = type;
            _point = point;
        }

        public async ValueTask<IDataBox> GetDataBox(IScroll scroll)
        {
            if (_dataBox is null)
            {
                Debug.Assert(!_point.HasValue);
                scroll.Point = _point!.Value;
                await scroll.Remove(dataBox: out var dataBox);
                _dataBox = dataBox;
                _point = null;
            }
            return _dataBox;
        }
        public IDataBox GetDataBoxSync(IScroll scroll)
        {
            if (_dataBox is null)
            {
                Debug.Assert(!_point.HasValue);
                scroll.Point = _point!.Value;
                scroll.Remove(dataBox: out var dataBox).Wait();
                _dataBox = dataBox;
                _point = null;
            }
            return _dataBox;
        }
    }
}
