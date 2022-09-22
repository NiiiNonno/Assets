// 令和弐年大暑確認済。
using System.Diagnostics.CodeAnalysis;

namespace Nonno.Assets.Collections;

public class RollCache<TKey, TValue> : Roll<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>
{
    int System.Collections.Generic.IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => Perimeter;
    public IEnumerable<TKey> Keys => this.Select(x => x.Key);
    public IEnumerable<TValue> Values => this.Select(x => x.Value);
    public TValue this[TKey key] => TryGetValue(key, out var r) ? r : throw new KeyNotFoundException();

    public RollCache(Shift perimeter) : base(perimeter, default) { }

    public void Push(TKey key, TValue value) => Push(new(key, value));

    public void Clear() => Fill(default);

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        foreach (var aEItem in this) if (Equals(aEItem, item)) return true;
        return false;
    }
    public bool ContainsKey(TKey key)
    {
        foreach (var (aEKey, _) in this) if (Equals(aEKey, key)) return true;
        return false;
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => Copy(to: array, ref arrayIndex);

    public bool Remove(KeyValuePair<TKey, TValue> item) => Replace(item, to: default);

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        foreach (var (aEKey, aEValue) in this)
        {
            if (Equals(aEKey, key))
            {
                value = aEValue;
                return true;
            }
        }
        value = default;
        return false;
    }
}
