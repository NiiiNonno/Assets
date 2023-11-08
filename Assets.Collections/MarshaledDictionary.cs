using System;
using System.Collections.Generic;
using System.Text;

namespace Nonno.Assets.Collections;
public class MarshaledDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue> where TKey : IComparable<TKey>
{
    readonly ArrayList<IReadOnlyDictionary<TKey, TValue>> _list;

    public IEnumerable<TKey> Keys => throw new NotImplementedException();

    public IEnumerable<TValue> Values => throw new NotImplementedException();

    public int Count => throw new NotImplementedException();

    public bool ContainsKey(TKey key) => throw new NotImplementedException();
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => throw new NotImplementedException();
    public bool TryGetValue(TKey key, out TValue value) => throw new NotImplementedException();

    public void AddDictionary(IReadOnlyDictionary<TKey, TValue> dictionary)
    {
        _list.Add(dictionary);
    }
    public void RemoveDictionary(IReadOnlyDictionary<TKey, TValue> dictionary)
    {
        _list.Remove(dictionary);
    }
}
