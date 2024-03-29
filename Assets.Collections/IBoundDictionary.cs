﻿// 令和弐年大暑確認済。
using System.Diagnostics.CodeAnalysis;

namespace Nonno.Assets.Collections;

public interface IBoundDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IBoundCollection<(TKey key, TValue value)>
{
    new event ItemAddedEventHandler? ItemAdded;
    new delegate void ItemAddedEventHandler(object? sender, ItemAddedEventArgs e);
    new record ItemAddedEventArgs(TKey Key, TValue Value);
    event IBoundCollection<(TKey key, TValue value)>.ItemAddedEventHandler? IBoundCollection<(TKey key, TValue value)>.ItemAdded
    {
        add
        {
            if (value != null) ItemAdded += value.Handle;
        }
        remove
        {
            if (value != null) ItemAdded -= value.Handle;
        }
    }
    new event ItemRemovedEventHandler? ItemRemoved;
    new delegate void ItemRemovedEventHandler(object? sender, ItemRemovedEventArgs e);
    new record ItemRemovedEventArgs(TKey Key, TValue Value);
    event IBoundCollection<(TKey key, TValue value)>.ItemRemovedEventHandler? IBoundCollection<(TKey key, TValue value)>.ItemRemoved
    {
        add
        {
            if (value != null) ItemRemoved += value.Handle;
        }
        remove
        {
            if (value != null) ItemRemoved -= value.Handle;
        }
    }
    event ValueReplacedEventHandler? ValueReplaced;
    delegate void ValueReplacedEventHandler(object? sender, ValueReplacedEventArgs e);
    record ValueReplacedEventArgs(TKey Key, TValue Old, TValue Neo);
}

public class BoundDictionary<TKey, TValue> : IBoundDictionary<TKey, TValue>
{
    readonly IDictionary<TKey, TValue> _dictionary;

    public int Count => ((ICollection<(TKey, TValue)>)_dictionary).Count;
    public ICollection<TKey> Keys => _dictionary.Keys;
    public ICollection<TValue> Values => _dictionary.Values;
    public event IBoundDictionary<TKey, TValue>.ItemAddedEventHandler? ItemAdded;
    public event IBoundDictionary<TKey, TValue>.ItemRemovedEventHandler? ItemRemoved;
    public event IBoundDictionary<TKey, TValue>.ValueReplacedEventHandler? ValueReplaced;
    public event IBoundCollection<(TKey key, TValue value)>.ClearedEventHandler? Cleared;

    protected internal BoundDictionary(IDictionary<TKey, TValue> dictionary)
    {
        _dictionary = dictionary;
    }
    public BoundDictionary(Constructor<IDictionary<TKey, TValue>> dictionaryConstructor) : this(dictionaryConstructor()) { }

    public bool Contains((TKey key, TValue value) item) => _dictionary.Contains(item);

    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    public void Add(TKey key, TValue value) => _ = TryAdd(key, value);
    public bool TryAdd(TKey key, TValue value)
    {
        if (_dictionary.TryAdd(key, value))
        {
            ItemAdded?.Invoke(this, new(key, value));
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool TryAdd((TKey key, TValue value) item)
    {
        if (_dictionary.TryAdd(item))
        {
            ItemAdded?.Invoke(this, new(item.key, item.value));
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (_dictionary.TryRemove(key, out value))
        {
            ItemRemoved?.Invoke(this, new(key, value));
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool TryRemove((TKey key, TValue value) item)
    {
        if (_dictionary.TryRemove(item))
        {
            ItemRemoved?.Invoke(this, new(item.key, item.value));
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Clear()
    {
        ((ICollection<(TKey, TValue)>)_dictionary).Clear();

        Cleared?.Invoke(this, new());
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _dictionary.TryGetValue(key, out value);

    public bool TrySetValue(TKey key, TValue value)
    {
        var old = _dictionary[key];
        if (_dictionary.TrySetValue(key, value))
        {
            ValueReplaced?.Invoke(this, new(key, old, value));
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Copy(Span<(TKey key, TValue value)> to, ref int index) => _dictionary.Copy(to, ref index);

    public IEnumerator<(TKey key, TValue value)> GetEnumerator() => ((ICollection<(TKey, TValue)>)_dictionary).GetEnumerator();
}

internal static partial class TrickyExtends
{
    public static void Handle<TKey, TValue>(this IBoundCollection<(TKey key, TValue value)>.ItemAddedEventHandler @this, object? sender, IBoundDictionary<TKey, TValue>.ItemAddedEventArgs e) => @this(sender, new(new(e.Key, e.Value)));
    public static void Handle<TKey, TValue>(this IBoundCollection<(TKey key, TValue value)>.ItemRemovedEventHandler @this, object? sender, IBoundDictionary<TKey, TValue>.ItemRemovedEventArgs e) => @this(sender, new(new(e.Key, e.Value)));
}
