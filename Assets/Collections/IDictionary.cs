﻿using System.Diagnostics.CodeAnalysis;
using SysGC = System.Collections.Generic;

namespace Nonno.Assets.Collections;

public interface IDictionary<TKey, TValue> : SysGC.IDictionary<TKey, TValue>, ICollection<(TKey key, TValue value)>
{
    TValue SysGC.IDictionary<TKey, TValue>.this[TKey key]
    {
        get => TryGetValue(key, out var value) ? value : throw new Exception("要素の取得に失敗しました。");
        set { if (!TrySetValue(key, value)) throw new Exception("要素の設定に失敗しました。"); }
    }
    bool SysGC::ICollection<KeyValuePair<TKey,TValue>>.IsReadOnly => false;
    new ICollection<TKey> Keys { get; }
    SysGC::ICollection<TKey> SysGC.IDictionary<TKey, TValue>.Keys => Keys;
    new ICollection<TValue> Values { get; }
    SysGC::ICollection<TValue> SysGC.IDictionary<TKey, TValue>.Values => Values;
    void SysGC.IDictionary<TKey, TValue>.Add(TKey key, TValue value)
    {
        if (!TryAdd(key, value)) throw new Exception("要素の追加に失敗しました。");
    }
    //new void Add(TKey key, TValue value) => _ = TryAdd(key, value);
    bool TryAdd(TKey key, TValue value);
    new TValue Remove(TKey key) => TryRemove(key, out var r) ? r : throw new Exception("要素の削除に失敗しました。");
    bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value);
    bool TrySetValue(TKey key, TValue value);
    #region 不要メンバ
    void SysGC.ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add((item.Key, item.Value));
    bool SysGC::ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => ((ISet<(TKey,TValue)>)this).Contains((item.Key, item.Value));
    void SysGC.ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        foreach (var item in (IEnumerable<KeyValuePair<TKey, TValue>>)this)
        {
            array[arrayIndex++] = item;
        }
    }
    void ICollection<(TKey key, TValue value)>.Copy(System.Span<(TKey key, TValue value)> to, ref int index)
    {
        foreach (var item in (IEnumerable<(TKey, TValue)>)this)
        {
            to[index++] = item;
        }
    }
    void ICollection<(TKey key, TValue value)>.Remove((TKey key, TValue value) item)
    {
        if (!TryRemove(item)) throw new Exception("要素の削除に失敗しました。");
    }
    bool ICollection<(TKey key, TValue value)>.TryAdd((TKey key, TValue value) item)
    {
        return TryAdd(item.key, item.value);
    }
    bool ICollection<(TKey key, TValue value)>.TryRemove((TKey key, TValue value) item)
    {
        if (!TryGetValue(item.key, out var r)) return false;
        if (!EqualityComparer<TValue>.Default.Equals(r, item.value)) return false;
        if (!TryRemove(item.key, out r)) return false;
        if (!EqualityComparer<TValue>.Default.Equals(r, item.value)) throw new Exception("削除された要素が指定のものと異なります。");
        return true;
    }
    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        foreach (var (key, value) in (IEnumerable<(TKey, TValue)>)this)
        {
            yield return new(key, value);
        }
    }
    bool SysGC.IDictionary<TKey, TValue>.Remove(TKey key) => TryRemove(key, out _);
    bool SysGC.ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => TryRemove((item.Key, item.Value));
    #endregion
}

public interface IReadOnlyDictionary<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>>, SysGC.IReadOnlyDictionary<TKey, TValue>
{
    TValue SysGC.IReadOnlyDictionary<TKey, TValue>.this[TKey key] => TryGetValue(key, out var value) ? value : throw new Exception("要素の取得に失敗しました。");
}
