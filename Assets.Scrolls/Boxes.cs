using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Nonno.Assets.Collections;
namespace Nonno.Assets.Scrolls;

public class Boxes : IList<object>
{
    readonly ScrollPointer _end;
    readonly ArrayList<(ScrollPointer ptr, Type? type, object? box)> _boxes;
    ScrollPointer _last;

    public IScroll BaseScroll {get;}

    public int Count
    {
        get
        {
            ExtendToTheEnd();
            return _boxes.Count;
        }
    }
    public object this[int index]
    {
        get
        {
            if (index < _boxes.Count) Extend(to: index);
            var (ptr, type, box) = _boxes[index];            
            if (box is null)
            {
                ptr = BaseScroll.Point = ptr;
                BaseScroll.Remove(dataBox: out box);
                _boxes[index] = (ptr, type, box);
            }
            return box;
        }
        set
        {
            if (index < _boxes.Count) Extend(to: index);
            var (ptr, type, box) = _boxes[index];
            ptr = BaseScroll.Point = ptr;
            if (box is null)
            {
                BaseScroll.Remove(dataBox: out box);
                System.Diagnostics.Debug.WriteLine($"{box}‚ðÁ‹Ž‚µ‚Ü‚·B");
            }
            BaseScroll.Insert(dataBox: value);
            _boxes[index] = (ptr, null, box);
        }
    }

    public Boxes(IScroll baseScroll)
    {
        _boxes = new();

        BaseScroll = baseScroll;
    }

    public void Insert(int at, object dataBox)
    {
        var (ptr, type, box) = _boxes[at];
        
        var neo = BaseScroll.Point = ptr;
        BaseScroll.Insert(dataBox: dataBox);
        
        ptr = BaseScroll.Point;
        _boxes[at] = (ptr, type, box);

        _boxes.Insert(at, (neo, null, dataBox));
    }

    public object Remove(int at)
    {
        var r = this[at];
        var (ptr, _, box) = _boxes[at];
        BaseScroll.Point = ptr;
        _boxes.Remove(at);
        return r;
    }

    public Subset<T> OfType<T>() => new Subset<T>(this);

    IEnumerator IEnumerable.GetEnumerator()
    {
        for (int i = 0; true; i++)
        {
            yield return this[i];
        }
    }

    bool TryExtend(int to)
    {
        BaseScroll.Point = _last;
        while (_boxes.Count < to)
        {
            if (BaseScroll.Is(on: _end)) return false;
            var type = BaseScroll.SkipDataBox();
            _boxes.Add((BaseScroll.Point, type, null));
        }
        _last = BaseScroll.Point;
        return true;
    }
    void Extend(int to)
    {
        if (!TryExtend(to)) throw new IndexOutOfRangeException();
    }
    void ExtendToTheEnd()
    {
        BaseScroll.Point = _last;
        while (!BaseScroll.Is(on: _end))
        {
            var type = BaseScroll.SkipDataBox();
            _boxes.Add((BaseScroll.Point, type, null));
        }
    }

    public int GetIndex(object of)
    {
        for (int i = 0; i < _boxes.Count; i++)
        {
            if (_boxes[i].box == of) return i;
        }
        for (int i = 0; true; i++)
        {
            if (!TryGetValue(i, out var obj)) return -1;
            if (obj == of) return i;
        }
    }

    public bool TryGetValue(int index, [MaybeNullWhen(false)] out object value)
    {
        bool r = true;
        if (index < _boxes.Count) r = TryExtend(to: index);
        var (ptr, type, box) = _boxes[index];
        if (box is null)
        {
            ptr = BaseScroll.Point = ptr;
            BaseScroll.Remove(dataBox: out box);
            _boxes[index] = (ptr, type, box);
        }
        value = box;
        return r;
    }
    public bool TrySetValue(int index, object value)
    {
        var r = true;
        if (index < _boxes.Count) r = TryExtend(to: index);
        var (ptr, type, box) = _boxes[index];
        ptr = BaseScroll.Point = ptr;
        if (box is null)
        {
            BaseScroll.Remove(dataBox: out box);
            System.Diagnostics.Debug.WriteLine($"{box}‚ðÁ‹Ž‚µ‚Ü‚·B");
        }
        BaseScroll.Insert(dataBox: value);
        _boxes[index] = (ptr, null, box);
        return r;
    }

    public void Copy(Span<object> to, ref int index)
    {
        foreach (var obj in this) to[index++] = obj;
    }

    public bool TryAdd(object item)
    {
        var ptr = BaseScroll.Point = _last;
        BaseScroll.Insert(dataBox: item);
        _last = BaseScroll.Point;
        _boxes.Add((ptr, null, item));
        return true;
    }

    public bool TryRemove(object item)
    {
        var index = GetIndex(of: item);
        if (index < 0) return false;
        var (ptr, _, box) = _boxes[index];
        BaseScroll.Point = ptr;
        _boxes.Remove(at: index);
        return true;
    }

    public void Clear()
    {
        foreach (var c in this);
        _boxes.Clear();
    }

    public bool Contains(object item)
    {
        return 0 <= GetIndex(item);
    }

    public System.Collections.Generic.IEnumerator<object> GetEnumerator()
    {
        for (int i = 0; true; i++)
        {
            yield return this[i];
        }
    }

    public class Subset<T> : System.Collections.Generic.IEnumerable<object>
    {
        readonly Boxes _main;

        public Subset(Boxes main) => _main = main;

        public System.Collections.Generic.IEnumerator<object> GetEnumerator()
        {
            throw new NotImplementedException();
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}