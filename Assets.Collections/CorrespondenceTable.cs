#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using static System.Math;

namespace Nonno.Assets.Collections
{
    [Obsolete]
    public class CorrespondenceTable<TKey, TValue> where TKey : Context.Index
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

        public Context<TKey>.IndexCollection Keys => _context.Indexes;
        public ConfinedArray<TValue?> Values
        {
            get
            {
                if (Keys.Count >= _arr.Length) Extend(to: Keys.Count);
                return new(_arr, Keys.Count);
            }
        }
        public int Count => Values.Count;
        public bool IsReadOnly => false;

        public CorrespondenceTable(Context<TKey> context)
        {
            _context = context;
            _arr = Array.Empty<TValue>();
        }

        public TValue? this[TKey key]
        {
            get
            {
                CheckContext(key);
                if (key >= _arr.Length) Extend(key);
                return _arr[key];
            }
            set
            {
                CheckContext(key);
                if (key >= _arr.Length) Extend(key);
                _arr[key] = value;
            }
        }

        void Extend(int to)
        {
            Capacity = Max(to + 1, _arr.Length * 2);
        }

        public bool ContainsKey(TKey key) => Keys.Contains(key);
        public void Clear() => Array.Clear(_arr, 0, _arr.Length);

        void CheckContext(TKey key)
        {
            if (key.Context != _context) throw new ArgumentException();
        }
    }
}