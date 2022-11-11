using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Collections;
public interface IConverter<T1, T2>
{
    T2 this[T1 t1] { get => GetForward(t1); set => SetForward(t1, value); }
    T1 this[T2 t2] { get => GetBackward(t2); set => SetBackward(t2, value); }

    T2 GetForward(T1 key);
    T1 GetBackward(T2 key);
    void SetForward(T1 key, T2 value);
    void SetBackward(T2 key, T1 value);
    void Add(KeyValuePair<T1, T2> item) => TryAdd(item);
    void Remove(KeyValuePair<T1, T2> item) => TryRemove(item);
    bool TryAdd(KeyValuePair<T1, T2> item);
    bool TryRemove(KeyValuePair<T1, T2> item);
    void Add(T1 t1, T2 t2) => _ = TryAdd(t1, t2);
    bool TryAdd(T1 t1, T2 t2);
    T2 Remove(T1 t1) => TryRemove(t1, out var r) ? r : throw new Exception("変換の削除に失敗しました。");
    bool TryRemove(T1 t1,[MaybeNullWhen(false)] out T2 t2);
    IConverter<T2, T1> Reverse();
}

public class TableConverter<T1, T2> : IConverter<T1, T2> where T1 : notnull where T2 : notnull
{
    readonly Dictionary<T1, T2> _d1 = new();
    readonly Dictionary<T2, T1> _d2 = new();
    TableConverter<T2, T1>? _rev;

    public T2 this[T1 t1] { get => _d1[t1]; set => _d1[t1] = value; }
    public T1 this[T2 t2] { get => _d2[t2]; set => _d2[t2] = value; }

    private TableConverter(Dictionary<T1, T2> dictionary1, Dictionary<T2, T1> dictionary2, TableConverter<T2, T1>? reversal)
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
    public void SetForward(T1 key, T2 value) => _d1[key] = value;
    public void SetBackward(T2 key, T1 value) => _d2[key] = value;

    public void Add(KeyValuePair<T1, T2> item) => TryAdd(item);
    public void Remove(KeyValuePair<T1, T2> item) => TryRemove(item);
    public bool TryAdd(KeyValuePair<T1, T2> item) => TryAdd(item.Key, item.Value);
    public bool TryRemove(KeyValuePair<T1, T2> item)
    {
        if (_d1.TryGetValue(item.Key, out var v1) && item.Value.Equals(v1) && _d2.TryGetValue(item.Value, out var v2) && item.Key.Equals(v2))
        {
            if (!_d1.Remove(item.Key)) throw new Exception("変換の削除に失敗しました。");
            if (!_d2.Remove(item.Value)) throw new Exception("変換の削除に失敗しました。");
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
                if (!_d1.Remove(t1)) throw new Exception("変換の追加に失敗しました。");
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
}
