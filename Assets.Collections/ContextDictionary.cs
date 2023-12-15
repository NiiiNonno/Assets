#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using static System.Math;

namespace Nonno.Assets.Collections
{
    public class ContextDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : Context.Index
    {
        readonly Context<TKey> _context;
        (int token, TValue value)[] _arr;

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
        public ValueCollection Values => new(this);
        public int Count => Values.Count;
        public bool IsReadOnly => false;

        public ContextDictionary(Context<TKey> context)
        {
            _context = context;
            _arr = [];
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
            _arr[key] = (key.Token, value);
        }
        public bool ContainsKey(TKey key) => Keys.Contains(key);
        public void Remove(TKey key)
        {
            CheckVlidation(key);
            _arr[key] = default;
        }
        public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            value = default!;
            if (!GetValidation(key)) return false;
            value = _arr[key].value;
            _arr[key] = default;
            return true;
        }
        public bool TryRemove(TKey key)
        {
            if (!GetValidation(key)) return false;
            lock (_arr)
            {
                _arr[key] = default;
            }
            return true;
        }
        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default!;
            if (!GetValidation(key)) return false;
            value = _arr[key].value;
            return true;
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
                _arr[key] = (key.Token, value);
            }
            return true;
        }
        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
        public void Clear()
        {
            Array.Clear(_arr, 0, _arr.Length);
        }
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (!GetValidation(item.Key)) return false;
            var aE = _arr[item.Key];
            return EqualityComparer<TValue>.Default.Equals(aE.value, item.Value);
        }
        public bool Contains((TKey key, TValue value) item)
        {
            if (!GetValidation(item.key)) return false;
            var aE = _arr[item.key];
            return EqualityComparer<TValue>.Default.Equals(aE.value, item.value);
        }
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!GetValidation(item.Key)) return false;
            if (!EqualityComparer<TValue>.Default.Equals(_arr[item.Key].value, item.Value)) return false;
            _arr[item.Key] = default;
            return true;
        }
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var item in Keys)
            {
                if (_arr.Length <= item) yield break;
                if (_arr[item].token == item.Token) yield return new(item, _arr[item].value);
            }
        }

        void CheckVlidation(TKey key, bool extend = false)
        {
            if (!GetValidation(key, extend)) throw new ArgumentException("入力された鍵が無効です。");
;        }
        bool GetValidation(TKey key, bool extend = false)
        {
            CheckContext(key);

            if (key >= _arr.Length) if (extend) Extend(to: key); else return false;
            return _arr[key].token == key.Token;
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
                if (_arr[item].token == item.Token) yield return (item, _arr[item].value);
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
        SysGC::ICollection<TKey> SysGC::IDictionary<TKey, TValue>.Keys => Keys;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
        SysGC::ICollection<TValue> SysGC::IDictionary<TKey, TValue>.Values => Values;

        public readonly struct IndexCollection : ICollection<TKey>
        {
            readonly ContextDictionary<TKey, TValue> _p;
            public IndexCollection(ContextDictionary<TKey, TValue> p) { _p = p; }
            public int Count => _p.Values.Count;
            public bool IsReadOnly => true;
            public bool Contains(TKey item)
            {
                return _p._arr.Length > item && _p._arr[item].token == item.Token;
            }
            void ICollection<TKey>.Copy(Span<TKey> array, ref int arrayIndex) => throw new NotImplementedException();
            public IEnumerator<TKey> GetEnumerator()
            {
                foreach (var item in _p._context.Indexes)
                {
                    if (_p._arr[item].token == item.Token) yield return item;
                }
            }
            bool ICollection<TKey>.TryAdd(TKey item) => false;
            void SysGC::ICollection<TKey>.Clear() => throw new NotImplementedException();
            bool ICollection<TKey>.TryRemove(TKey item) => false;
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public readonly struct ValueCollection : ICollection<TValue>
        {
            readonly ContextDictionary<TKey, TValue> _p;
            public ValueCollection(ContextDictionary<TKey, TValue> p) { _p = p; }
            public int Count
            {
                get
                {
                    var c = 0;
                    foreach (var (token, _) in _p._arr)
                    {
                        if (token != 0) c++;
                    }
                    return c;
                }
            }
            public bool IsReadOnly => true;
            public bool Contains(TValue item)
            {
                foreach (var (token, value) in _p._arr)
                {
                    if (token != 0 && EqualityComparer<TValue>.Default.Equals(item, value)) return true;
                }
                return false;
            }
            void ICollection<TValue>.Copy(Span<TValue> array, ref int arrayIndex) => throw new NotImplementedException();
            public IEnumerator<TValue> GetEnumerator()
            {
                foreach (var (token, value) in _p._arr)
                {
                    if (token != 0) yield return value;
                }
            }
            bool ICollection<TValue>.TryAdd(TValue item) => false;
            void SysGC::ICollection<TValue>.Clear() => throw new NotImplementedException();
            bool ICollection<TValue>.TryRemove(TValue item) => false;
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
