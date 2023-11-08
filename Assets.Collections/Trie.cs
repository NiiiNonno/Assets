using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Nonno.Assets.Collections;
public readonly struct Trie<TKey, TValue, TItem> where TKey : IEnumerator<TItem> where TValue : notnull, ITable<TItem, TValue>
{
    readonly TValue _root;

    public TValue Root => _root;

    public Trie(TValue root)
    {
        _root = root;
    }

    public TValue this[TKey key] => TryGetValue(key, out var val) ? val : throw new KeyNotFoundException();

    public bool TryGetValue(TKey key, out TValue value)
    {
        value = _root;
        while (key.MoveNext()) if (value[key.Current] is { } v) value = v; else return false;
        return true;
    }
    public bool TrySetValue(TKey key, TValue value)
    {
        if (!key.MoveNext()) return false;

        TItem kC;
        TValue? c = _root;
        do
        {
            kC = key.Current;
            c = c[kC];
            if (c is null) return false;
        }
        while (key.MoveNext());

        c[kC] = value;
        return true;
    }
}
