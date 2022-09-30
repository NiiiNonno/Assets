using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Collections;
public interface ITwoWayDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    public IDictionary<TValue, TKey> Opposite { get; }
}

public class TwoWayDictionary<TKey, TValue> : BoundDictionary<TKey, TValue>, ITwoWayDictionary<TKey, TValue> where TKey : notnull
{
    public IDictionary<TValue, TKey> Opposite { get; }

    public TwoWayDictionary(Constructor<IDictionary<TKey, TValue>> dictionaryConstructor, Constructor<IDictionary<TValue, TKey>> oppositeConstructor) : this(dictionaryConstructor(), oppositeConstructor()) { }
    protected TwoWayDictionary(IDictionary<TKey, TValue> dictionary, IDictionary<TValue, TKey> opposite) : base(dictionary)
    {
        Opposite = opposite;

        ItemAdded += (sender, e) => Opposite.Add(e.Value, e.Key);
        ItemRemoved += (sender, e) => Opposite.Remove(e.Value);
        Cleared += (sender, e) => Opposite.Clear();
        ValueReplaced += (sender, e) => { Opposite.Remove(e.Old); Opposite.Add(e.Neo, e.Key); };
    }
}

public class HashTableTwoWayDictionary<TKey, TValue> : TwoWayDictionary<TKey, TValue> where TKey : notnull where TValue : notnull
{
    public HashTableTwoWayDictionary() : base(new HashTableDictionary<TKey, TValue>(), new HashTableTwoWayDictionary<TValue, TKey>()) { }
}
