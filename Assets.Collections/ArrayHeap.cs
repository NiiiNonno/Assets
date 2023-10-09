using System.Reflection;
namespace Nonno.Assets.Collections;

public class ArrayHeap<TConstraint> : Heap<TConstraint> where TConstraint : class
{
    readonly ArrayList<TConstraint> _list;
    int _trailerLength;

    public override int Count => _list.Count;

    public ArrayHeap(ArrayList<TConstraint> list, int nadir)
    {
        _list = list;
        _trailerLength = nadir;
    }
    public ArrayHeap(params TConstraint[] objectParams) : this(0, objectParams) { }
    public ArrayHeap(int nadir = 0, params TConstraint[] objectParams) : this(new(objectParams), nadir) { }

    public override ValueTask<bool> Contains<T>() where T : default
    {
        for (int i = 0; i < Count; i++)
        {
            if (_list[i] is T) return new ValueTask<bool>(true);
        }
        return new ValueTask<bool>(false);
    }
    public override ValueTask<bool> Contains(Type type)
    {
        if (type.IsAssignableTo(typeof(TConstraint))) throw new ArgumentException("型が制約に反します。");

        for (int i = 0; i < Count; i++)
        {
            if (_list[i].GetType().IsAssignableTo(type)) return new ValueTask<bool>(true);
        }
        return new ValueTask<bool>(false);
    }

    public override ValueTask<T?> Get<T>() where T : default
    {
        for (int i = 0; i < Count; i++)
        {
            if (_list[i] is T) return new ValueTask<T?>((T)Remove(i));
        }
        return default;
    }
    public override ValueTask<TConstraint?> Get(Type type)
    {
        if (type.IsAssignableTo(typeof(TConstraint))) throw new ArgumentException("型が制約に反します。");

        for (int i = 0; i < Count; i++)
        {
            if (_list[i].GetType().IsAssignableTo(type)) return new ValueTask<TConstraint?>(Remove(i));
        }
        return default;
    }

    public override ValueTask<T?> Move<T>(T? @object) where T : default
    {
        if (@object is null) return Get<T>();

        for (int i = 0; i < Count; i++)
        {
            if (_list[i] is T t) { _list[i] = @object; return new ValueTask<T?>(t); }
        }
        return default;
    }
    public override ValueTask<TConstraint?> Move(Type type, TConstraint? @object)
    {
        if (type.IsAssignableTo(typeof(TConstraint))) throw new ArgumentException("型が制約に反します。");

        if (@object is null) return Get(type);

        for (int i = 0; i < Count; i++)
        {
            if (_list[i] is var t && t.GetType().IsAssignableTo(type)) { _list[i] = @object; return new ValueTask<TConstraint?>(t); }
        }
        return default;
    }

    public override Task Set<T>(T? @object) where T : default
    {
        if (@object is null) return Task.CompletedTask;

        for (int i = 0; i < Count; i++)
        {
            if (_list[i] is T) { Insert(i, @object); return Task.CompletedTask; }
        }
        return Task.CompletedTask;
    }
    public override Task Set(Type type, TConstraint? @object)
    {
        if (type.IsAssignableTo(typeof(TConstraint))) throw new ArgumentException("型が制約に反します。");

        if (@object is null) return Task.CompletedTask;

        for (int i = 0; i < Count; i++)
        {
            if (_list[i].GetType().IsAssignableTo(type)) { Insert(i, @object); return Task.CompletedTask; }
        }
        return Task.CompletedTask;
    }

    public override void Add(TConstraint item) => _list.Add(item);
    public override void Remove(TConstraint item)
    {
        var index = _list.GetIndex(of: item);
        _ = Remove(index);
    }

    public override bool Contains(TConstraint item) => _list.Contains(item);
    public override IEnumerator<TConstraint> GetEnumerator()
    {
        foreach (var item in _list.Skip(_trailerLength)) yield return item;
        foreach (var item in _list.Take(_trailerLength)) yield return item;
    }

    public void Insert(int index, TConstraint item)
    {
        if (index < _trailerLength) _trailerLength++;
        _list.Insert(index, item);
    }
    public TConstraint Remove(int index)
    {
        if (index < _trailerLength) _trailerLength--;
        return _list.Remove(index);
    }
}
