#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nonno.Assets.Collections
{
    public class CachedCollection<T> : IReadOnlyCollection<T>
    {
        T[] _arr;
        int _count;
        IEnumerable<T>? _e;

        public int Count => _count;
        public int Capacity => _arr.Length;
        public IEnumerable<T>? InnerEnumerable
        {
            get => _e;
            set
            {
                _e = value;
                Update();
            }
        }
        public T this[int i]
        {
            get
            {
                if ((uint)i >= _count) throw new ArgumentOutOfRangeException();
                return _arr[i];
            }
        }

        public CachedCollection()
        {
            _arr = Array.Empty<T>();
        }

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Update()
        {
            if (_e is null)
            {
                _count = 0;
                return;
            }

            var i = 0;
            foreach (var item in _e)
            {
                if (_arr.Length <= i)
                    Array.Resize(ref _arr, Math.Max(_e.Count(), _arr.Length * 2));

                _arr[i] = item;
                i++;
            }
            _count = i;
        }

        public struct Enumerator : IEnumerator<T>
        {
            readonly CachedCollection<T> _c;
            int _i;

            public Enumerator(CachedCollection<T> c)
            {
                _c = c;
                _i = -1;
            }

            public T Current => _c._arr[_i];
            object IEnumerator.Current => Current!;

            public void Dispose() { }
            public bool MoveNext() => ++_i < _c._count;
            public void Reset() => _i = -1;
        }
    }
}