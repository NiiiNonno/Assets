using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Nonno.Assets.Collections;
public class HarmonizedCollections<T> : ICollection<T>, IHarmonized<SysGC::ICollection<T>>
{
    readonly ArrayList<SysGC::ICollection<T>> _cs = [];
    readonly int _pN = 0;
    ArrayList<T>? _p;

    public int Count => _cs.Count;
    SysGC::ICollection<T>? Primary => _cs.TryGetValue(_pN, out var r ) ? r : null;

    public void Clear()
    {
        foreach (var c in _cs)
        {
            c.Clear();
        }
    }
    public bool Contains(T item) => _cs.TryGetValue(_pN, out var p)&& p.Contains(item);
    public void Copy(Span<T> to, ref int index)
    {
        foreach (var c in _cs)
        {
            if (c is ICollection<T> c_)
            {
                c_.Copy(to, ref index);
                return;
            }
        }
        if (_cs.TryGetValue(_pN, out var p))
        {
            foreach (var item in p)
            {
                to[index++] = item;
            }
        }
    }
    public IEnumerator<T> GetEnumerator() => _cs.TryGetValue(_pN, out var p) ? p.GetEnumerator() : EmptyEnumerator<T>.INSTANCE;
    public void Add(T item)
    {
        if (_cs.Count == 0) _cs.Add(_p = []);

        foreach(var c in _cs)
        {
            c.Add(item);
        }
    }
    public bool TryAdd(T item)
    {
        Add(item);
        return true;
    }
    public void Remove(T item)
    {
        if (!TryRemove(item)) throw new Exception("要素の削除に失敗しました。");
    }
    public bool TryRemove(T item)
    {
        var cs = _cs.AsSpan();

        if (cs.Length == 0) return false;

        int i = 0;
        var r = cs[i].Remove(item);
        for (; i < cs.Length; i++)
        {
            var r2 = cs[i].Remove(item);
            if (r ^ r2) throw new Exception("要素の削除に成功したものとしなかったものとで不整合が生じます。");
        }
        return r;
    }

    public void Include(SysGC::ICollection<T> collection)
    {
        collection.Clear();

        if (_cs.TryGetValue(_pN, out var p))
        {
            foreach (var item in p)
            {
                collection.Add(item);
            }
        }

        _cs.Add(collection);
    }
    public void Exclude(SysGC::ICollection<T> collection)
    {
        _cs.Remove(collection);

        if (_cs.Count == 0) _cs.Add(_p = [..collection.ToArray()]);
    }
}
