using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Nonno.Assets.Collections;

public class TableConverter<T1, T2> : IConverter<T1, T2> where T1 : notnull where T2 : notnull
{
    readonly HashTableDictionary<T1, T2> _d1 = new();
    readonly HashTableDictionary<T2, T1> _d2 = new();
    TableConverter<T2, T1>? _rev;

    public ICollection<T1> Keys => _d1.Keys;
    public ICollection<T2> Values => _d2.Keys;
    public T2 this[T1 t1] { get => _d1[t1]; set => _d1[t1] = value; }
    public T1 this[T2 t2] { get => _d2[t2]; set => _d2[t2] = value; }

    private TableConverter(HashTableDictionary<T1, T2> dictionary1, HashTableDictionary<T2, T1> dictionary2, TableConverter<T2, T1>? reversal)
    {
        _d1 = dictionary1;
        _d2 = dictionary2;
        _rev = reversal;
    }
    public TableConverter()
    {
        _d1 = new();
        _d2 = new();
    }
    public TableConverter(int capacity)
    {
        _d1 = new(capacity);
        _d2 = new(capacity);
    }
    public TableConverter(IEnumerable<KeyValuePair<T1, T2>> pairs)
    {
        _d1 = new(pairs);
        _d2 = new(pairs.Select(x => new KeyValuePair<T2, T1>(x.Value, x.Key)));
    }

    public T2 GetForward(T1 key) => _d1[key];
    public T1 GetBackward(T2 key) => _d2[key];
    public bool TryGetForward(T1 key, [MaybeNullWhen(false)] out T2 value) => _d1.TryGetValue(key, out value);
    public bool TryGetBackward(T2 key, [MaybeNullWhen(false)] out T1 value) => _d2.TryGetValue(key, out value);

    public void SetForward(T1 key, T2 value) => _d1[key] = value;
    public void SetBackward(T2 key, T1 value) => _d2[key] = value;
    public bool TrySetForward(T1 key, T2 value)
    {
        if (!_d1.ContainsKey(key)) return false;
        _d1[key] = value;
        return true;
    }
    public bool TrySetBackward(T2 key, T1 value)
    {
        if (!_d2.ContainsKey(key)) return false;
        _d2[key] = value;
        return true;
    }

    public T2 MoveForward(T1 key, T2 value)
    {
        var r = _d1[key];
        _d1[key] = value;
        return r;
    }
    public T1 MoveBackward(T2 key, T1 value)
    {
        var r = _d2[key];
        _d2[key] = value;
        return r;
    }
    public bool TryMoveForward(T1 key, T2 neo, [MaybeNullWhen(false)] out T2 old)
    {
        if (!_d1.TryGetValue(key, out old)) return false;
        _d1[key] = neo;
        return true;
    }
    public bool TryMoveBackward(T2 key, T1 neo, [MaybeNullWhen(false)] out T1 old)
    {
        if (!_d2.TryGetValue(key, out old)) return false;
        _d2[key] = neo;
        return true;
    }

    public bool ContainsForward(T1 item) => _d1.ContainsKey(item);
    public bool ContainsBackward(T2 item) => _d2.ContainsKey(item);

    public void Add(KeyValuePair<T1, T2> item) => TryAdd(item);
    public void Remove(KeyValuePair<T1, T2> item) => TryRemove(item);
    public bool TryAdd(KeyValuePair<T1, T2> item) => TryAdd(item.Key, item.Value);
    public bool TryRemove(KeyValuePair<T1, T2> item)
    {
        if (_d1.TryGetValue(item.Key, out var v1) && item.Value.Equals(v1) && _d2.TryGetValue(item.Value, out var v2) && item.Key.Equals(v2))
        {
            if (!_d1.TryRemove(item.Key, out _)) throw new Exception("変換の削除に失敗しました。");
            if (!_d2.TryRemove(item.Value, out _)) throw new Exception("変換の削除に失敗しました。");
            return true;
        }
        return false;
    }
    public void Add(T1 t1, T2 t2) => _ = TryAdd(t1, t2);
    public bool TryAdd(T1 t1, T2 t2)
    {
        if (_d1.TryAdd(t1, t2))
        {
            if (_d2.TryAdd(t2, t1))
            {
                return true;
            }
            else
            {
                if (!_d1.TryRemove(t1, out _)) throw new Exception("変換の追加に失敗しました。");
            }
        }
        return false;
    }
    public T2 Remove(T1 t1) => TryRemove(t1, out var r) ? r : throw new Exception("変換の削除に失敗しました。");
    public bool TryRemove(T1 t1, out T2 t2)
    {
        return _d1.Remove(t1, out t2!);
    }

    public IConverter<T2, T1> Reverse() => _rev ??= new(_d2, _d1, this);

    public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator() => _d1.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
