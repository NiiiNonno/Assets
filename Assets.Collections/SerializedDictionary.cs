using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static Nonno.Assets.Collections.Context;

namespace Nonno.Assets.Collections;
public readonly struct SerializedDictionary<TKey, TValue>(IVector<char> @base, char keyValueSeperator, char valueKeySeparator) : IDictionary<TKey, TValue> where TKey : unmanaged where TValue : unmanaged
{
    readonly IVector<char> _a = @base;
    readonly char _kVS = keyValueSeperator;
    readonly char _vKS = valueKeySeparator;
    readonly Parser<TKey> _keyParser;
    readonly Parser<TValue> _valueParser;

    public KeyCollection Keys => new(this);
    ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
    public ValueCollection Values => new(this);
    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

    public int Count => Keys.Count;

    public void Clear() => _a.Length = 0;
    public bool Contains((TKey key, TValue value) item)
    {

    }
    public bool ContainsKey(TKey key)
    {

    }
    public IEnumerator<(TKey key, TValue value)> GetEnumerator()
    {
        var c = 0;
        while (true)
        {
            _a.SetFlag(IReadOnlyVector<char>.Flag.Hold);
            var n = _a.GetIndex(of: _kVS, c..);
            var k = _keyParser(_a[c..n]);
            c = _a.GetIndex(of: _vKS, n..);
            var v = _valueParser(_a[n..c]);
            _a.SetFlag(IReadOnlyVector<char>.Flag.None);
            yield return (k, v);
        }
    }
    public bool TryAdd(TKey key, TValue value)
    {

    }
    public bool TryGetValue(TKey key, out TValue value)
    {

    }
    public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {

    }
    public bool TrySetValue(TKey key, TValue value)
    {

    }

    public readonly struct KeyCollection(SerializedDictionary<TKey, TValue> dictionary) : ICollection<TKey>
    {
        readonly SerializedDictionary<TKey, TValue> _d = dictionary;

        public int Count => this.Count();

        public void Clear() => _d.Clear();
        public bool Contains(TKey item) => _d.ContainsKey(item);
        public void Copy(Span<TKey> to, ref int index)
        {
            foreach (var item in this)
            {
                to[index++] = item;
            }
        }
        public IEnumerator<TKey> GetEnumerator()
        {
            var c = 0;
            while (true)
            {
                _d._a.SetFlag(IReadOnlyVector<char>.Flag.Hold);
                var n = _d._a.GetIndex(of: _d._kVS, c..);
                var k = _d._keyParser(_d._a[c..n]);
                yield return k;
                _d._a.SetFlag(IReadOnlyVector<char>.Flag.None);
                c = _d._a.GetIndex(of: _d._vKS, n..);
            }
        }
        public bool TryAdd(TKey item) => throw new NotSupportedException();
        public bool TryRemove(TKey item) => _d.Remove(item, out _);
    }

    public readonly struct ValueCollection(SerializedDictionary<TKey, TValue> dictionary) : ICollection<TValue>
    {
        readonly SerializedDictionary<TKey, TValue> _d = dictionary;

        public int Count => _d.Count;

        public void Clear() => _d.Clear();
        public bool Contains(TValue item)
        {
            foreach (var v in this)
            {
                if (EqualityComparer<TValue>.Default.Equals(v, item)) return true;
            }
            return false;
        }
        public void Copy(Span<TValue> to, ref int index)
        {
            foreach (var item in this)
            {
                to[index++] = item;
            }
        }
        public IEnumerator<TValue> GetEnumerator()
        {
            var c = 0;
            while (true)
            {
                var n = _d._a.GetIndex(of: _d._kVS, c..);
                _d._a.SetFlag(IReadOnlyVector<char>.Flag.Hold);
                c = _d._a.GetIndex(of: _d._vKS, n..);
                var v = _d._valueParser(_d._a[n..c]);
                yield return v;
                _d._a.SetFlag(IReadOnlyVector<char>.Flag.None);
            }
        }
        public bool TryAdd(TValue item) => throw new NotSupportedException();
        public bool TryRemove(TValue item) => throw new NotSupportedException();
    }
}
