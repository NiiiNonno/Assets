using System;
using System.Collections.Generic;
using System.Text;

namespace Nonno.Assets.Collections;
public readonly struct Table<TKey, TValue>
{
    readonly IDictionary<TKey, TValue> _dict;

    public Table(IDictionary<TKey, TValue> dict) => _dict = dict;

    public TValue? this[TKey key]
    {
        get => _dict.TryGetValue(key, out var value) ? value : default;
        set
        {
            if (value is null)
            {
                _dict.Remove(key);
            }
            else if (!_dict.TryAdd(key, value))
            {
                _dict[key] = value;
            }
        }
    }

    public void Clear() => ((ICollection<(TKey, TValue)>)_dict).Clear();
}
