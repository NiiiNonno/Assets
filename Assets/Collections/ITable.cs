using System;
using System.Collections.Generic;
using System.Text;

namespace Nonno.Assets.Collections;
public interface ITable<TKey, TValue>
{
    TValue? this[TKey key]
    {
        get => TryGetValue(key, out var r) ? r : throw new KeyNotFoundException();
        set { if (!TrySetValue(key, value)) throw new KeyNotFoundException(); }
    }
    
    ISet<TKey> Keys { get; }

    bool ContainsKey(TKey key) => Keys.Contains(key)
    bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value);
    bool TrySetValue(TKey key, TValue value);
}
