#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using static System.Math;
using SysGC = System.Collections.Generic;

namespace Nonno.Assets.Collections
{
    [Obsolete]
    public class CorrespondenceDictionary<TKey, TValue> : SysGC::IDictionary<TKey, TValue> where TKey : Context.Index where TValue : notnull
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
            if (key.Value >= _arr.Length) Extend(to: key.Value);
            lock (_arr)
            {
                _arr[key.Value] = value;
            }
        }
        public bool ContainsKey(TKey key) => Keys.Contains(key);
        public void Remove(TKey key)
        {
            CheckContext(key);
            if (key.Value >= _arr.Length) return;
            _arr[key.Value] = default;
        }
        public bool TryRemove(TKey key)
        {
            CheckContext(key);
            if (key.Value >= _arr.Length) return false;
            if (!EqualityComparer<TValue>.Default.Equals(_arr[key.Value])) return false;
            _arr[key.Value] = default;
            return true;
        }
        public bool TryGetValue(TKey key, out TValue value)
        {
            CheckContext(key);
            value = default!;
            if (key.Value >= _arr.Length) return false;
            value = _arr[key.Value]!;
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
            if (key.Value >= _arr.Length) return false;
            lock (_arr)
            {
                _arr[key.Value] = value;
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
            if (key.Value >= _arr.Length) Extend(key.Value);
            lock (_arr)
            {
                _arr[key.Value] = value;
            }
        }
        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
        public void Clear() => Values.Clear();
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            CheckContext(item.Key);
            if (item.Key.Value >= _arr.Length) return false;
            var aE = _arr[item.Key.Value];
            if (aE is null) return false;
            return EqualityComparer<TValue>.Default.Equals(aE, item.Value);
        }
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            CheckContext(item.Key);
            if (item.Key.Value >= _arr.Length) return false;
            if (_arr[item.Key.Value] is not { } v || !EqualityComparer<TValue>.Default.Equals(v, item.Value)) return false;
            _arr[item.Key.Value] = default;
            return true;
        }
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var item in Keys)
            {
                if (_arr.Length <= item.Value) yield break;
                if (_arr[item.Value] is { } v) yield return new(item, v);
            }
        }

        void CheckContext(TKey key)
        {
            if (key.Context != _context) throw new ArgumentException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        bool SysGC::IDictionary<TKey, TValue>.Remove(TKey key) => TryRemove(key);
        SysGC::ICollection<TKey> SysGC::IDictionary<TKey, TValue>.Keys => Keys;
        SysGC::ICollection<TValue> SysGC::IDictionary<TKey, TValue>.Values => Values;

        public readonly struct IndexCollection : SysGC::ICollection<TKey>
        {
            readonly CorrespondenceDictionary<TKey, TValue> _p;
            public IndexCollection(CorrespondenceDictionary<TKey, TValue> p) { _p = p; }
            public int Count => _p.Values.Count;
            public bool IsReadOnly => true;
            public bool Contains(TKey item)
            {
                return _p._arr.Length > item.Value && _p._arr[item.Value] is not null;
            }
            public void CopyTo(TKey[] array, int arrayIndex) => throw new NotImplementedException();
            public IEnumerator<TKey> GetEnumerator()
            {
                foreach (var item in _p._context.Indexes)
                {
                    if (_p._arr[item.Value] is not null) yield return item;
                }
            }
            void SysGC::ICollection<TKey>.Add(TKey item) => throw new NotImplementedException();
            void SysGC::ICollection<TKey>.Clear() => throw new NotImplementedException();
            bool SysGC::ICollection<TKey>.Remove(TKey item) => throw new NotImplementedException();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
