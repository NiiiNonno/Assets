#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using static Nonno.Assets.ThrowHelper;

namespace Nonno.Assets.Collections
{
    public readonly struct SkipNullArrayCollection<T> : ICollection<T>
    {
        readonly T?[] _ts;

        public int Count
        {
            get
            {
                var c = 0;
                foreach (var item in _ts)
                {
                    if (item is not null) c++;
                }
                return c;
            }
        }
        public bool IsReadOnly => false;
        public int Capacity
        {
            get => _ts.Length;
        }

        public SkipNullArrayCollection(int capacity)
        {
            _ts = new T?[capacity];
        }
        public SkipNullArrayCollection(T?[] array)
        {
            _ts = array;
        }

        public void Add(T? item) => ThrowIfNot(TryAdd(item));
        public bool TryAdd(T? item)
        {
            for (var i = 0; i < _ts.Length; i++)
            {
                if (_ts[i] is null)
                {
                    _ts[i] = item;
                    return true;
                }
            }
            return false;
        }

        public void Remove(T? item) => ThrowIfNot(TryRemove(item));
        public bool TryRemove(T? item)
        {
            var e = EqualityComparer<T?>.Default;
            for (var i = 0; i < _ts.Length; i++)
            {
                if (e.Equals(_ts[i], item))
                {
                    _ts[i] = default;
                    return true;
                }
            }
            return false;
        }
        public void Clear() => Array.Clear(_ts, 0, _ts.Length);
        public bool Contains(T item)
        {
            var e = EqualityComparer<T?>.Default;
            for (var i = 0; i < _ts.Length; i++)
            {
                if (e.Equals(_ts[i], item))
                    return true;
            }
            return false;
        }
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _ts)
            {
                if (item is not null) yield return item;
            }
        }

        public void Copy(Span<T> to, ref int index)
        {
            _ts.AsSpan().CopyTo(to!);
            index += _ts.Length;
        }

        IEnumerator IEnumerable.GetEnumerator() => _ts.GetEnumerator();
    }
}