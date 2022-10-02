using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Collections;
public class SplayTree<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : notnull,IComparable<TKey>, IEquatable<TKey>
{
    public int Count
    {
        get
        {
            throw new NotImplementedException();
        }
    }
    public TValue this[TKey key]
    {
        get
        {
            throw new NotImplementedException();
        }
        set
        {
            throw new NotImplementedException();
        }
    }

    public TValue Remove(TKey key)
    {
        throw new NotImplementedException();
    }
    public bool TryRemove(TKey key, out TValue result)
    {
        throw new NotImplementedException();
    }

    public void Add(TKey key, TValue value)
    {
        throw new NotImplementedException();
    }
    public bool TryAdd(TKey key, TValue value)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => throw new NotImplementedException();
    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
}
