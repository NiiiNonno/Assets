#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using static System.Math;

namespace Nonno.Assets.Collections
{
    public class CorrespondenceDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : Context.Index where TValue : notnull
    {
        readonly Context<TKey> _context;
        TValue?[] _arr;

        public int Capacity
        {
            get => _arr.Length;
            set
            {
                lock (_arr)
                {
                    Array.Resize(ref _arr, Max(_arr.Length, value));
                }
            }
        }

        public IndexCollection Keys => new(this);
        public SkipNullArrayCollection<TValue> Values => new(_arr);
        public int Count => Values.Count;
        public bool IsReadOnly => false;

        public CorrespondenceDictionary(Context<TKey> context)
        {
            _context = context;
            _arr = Array.Empty<TValue>();
        }

        public TValue this[TKey key]
        {
            get => TryGetValue(key, out var r) ? r : throw new KeyNotFoundException();
            set {if(!TrySetValue(key, value)) throw new KeyNotFoundException(); }
        }

        void Extend(int to)
        {
            Capacity = Max(to + 1, _arr.Length * 2);
        }

        public void Add(TKey key, TValue value)
        {
            CheckContext(key);
            if (key >= _arr.Length) Extend(to: key);
            lock (_arr)
            {
                _arr[key] = value;
            }
        }
        public bool ContainsKey(TKey key) => Keys.Contains(key);
        public void Remove(TKey key)
        {
            CheckContext(key);
            if (key >= _arr.Length) return;
            _arr[key] = default;
        }
        public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            CheckContext(key);
            value = default!;
            if (key >= _arr.Length) return false;
            if (_arr[key] is null) return false;
            value = _arr[key]!;
            _arr[key] = default;
            return true;
        }
        public bool TryRemove(TKey key)
        {
            CheckContext(key);
            if (key >= _arr.Length) return false;
            if (_arr[key] is null) return false;
            _arr[key] = default;
            return true;
        }
        public bool TryGetValue(TKey key, out TValue value)
        {
            CheckContext(key);
            value = default!;
            if (key >= _arr.Length) return false;
            value = _arr[key]!;
            return value is not null;
        }
        /// <summary>
        /// 値を設定します。
        /// <para>
        /// 場合によって、未存の鍵に対しても正常に値が設定されることがあり、これは<c>true</c>を返します。
        /// </para>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TrySetValue(TKey key, TValue value)
        {
            CheckContext(key);
            if (key >= _arr.Length) return false;
            lock (_arr)
            {
                _arr[key] = value;
            }
            return true;
        }
        /// <summary>
        /// 値の設定、または値が存在しない場合は値を追加します。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(TKey key, TValue value)
        {
            CheckContext(key);
            if (key >= _arr.Length) Extend(key);
            lock (_arr)
            {
                _arr[key] = value;
            }
        }
        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
        public void Clear() => Values.Clear();
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            CheckContext(item.Key);
            if (item.Key >= _arr.Length) return false;
            var aE = _arr[item.Key];
            if (aE is null) return false;
            return EqualityComparer<TValue>.Default.Equals(aE, item.Value);
        }
        public bool Contains((TKey key, TValue value) item)
        {
            CheckContext(item.key);
            if (item.key >= _arr.Length) return false;
            var aE = _arr[item.key];
            if (aE is null) return false;
            return EqualityComparer<TValue>.Default.Equals(aE, item.value);
        }
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            CheckContext(item.Key);
            if (item.Key >= _arr.Length) return false;
            if (_arr[item.Key] is not { } v || !EqualityComparer<TValue>.Default.Equals(v, item.Value)) return false;
            _arr[item.Key] = default;
            return true;
        }
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var item in Keys)
            {
                if (_arr.Length <= item) yield break;
                if (_arr[item] is { } v) yield return new(item, v);
            }
        }

        void CheckContext(TKey key)
        {
            if (key.Context != _context) throw new ArgumentException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        bool SysGC::IDictionary<TKey, TValue>.Remove(TKey key) => TryRemove(key);
        bool IDictionary<TKey, TValue>.TryAdd(TKey key, TValue value) 
        { 
            Add(key, value);
            return true;
        }
        IEnumerator<(TKey key, TValue value)> IEnumerable<(TKey key, TValue value)>.GetEnumerator()
        {
            foreach (var item in Keys)
            {
                if (_arr.Length <= item) yield break;
                if (_arr[item] is { } v) yield return (item, v);
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
        SysGC::ICollection<TKey> SysGC::IDictionary<TKey, TValue>.Keys => Keys;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
        SysGC::ICollection<TValue> SysGC::IDictionary<TKey, TValue>.Values => Values;

        public readonly struct IndexCollection : ICollection<TKey>
        {
            readonly CorrespondenceDictionary<TKey, TValue> _p;
            public IndexCollection(CorrespondenceDictionary<TKey, TValue> p) { _p = p; }
            public int Count => _p.Values.Count;
            public bool IsReadOnly => true;
            public bool Contains(TKey item)
            {
                return _p._arr.Length > item && _p._arr[item] is not null;
            }
            void ICollection<TKey>.Copy(Span<TKey> array, ref int arrayIndex) => throw new NotImplementedException();
            public IEnumerator<TKey> GetEnumerator()
            {
                foreach (var item in _p._context.Indexes)
                {
                    if (_p._arr[item] is not null) yield return item;
                }
            }
            bool ICollection<TKey>.TryAdd(TKey item) => false;
            void SysGC::ICollection<TKey>.Clear() => throw new NotImplementedException();
            bool ICollection<TKey>.TryRemove(TKey item) => false;
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
