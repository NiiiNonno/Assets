#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nonno.Assets.Collections
{
    public class Context
    {
        protected readonly List<WeakReference> _refs = new();
        private int _nextToken = 1;

        public override bool Equals(object? obj) => Equals(obj as Context);
        public bool Equals(Context? other) => other is not null && ReferenceEquals(_refs, other._refs);
        public override int GetHashCode() => HashCode.Combine(_refs);

        public static bool operator ==(Context? left, Context? right) => EqualityComparer<Context?>.Default.Equals(left, right);
        public static bool operator !=(Context? left, Context? right) => !(left == right);

        public class Index : IEquatable<Index?>
        {
            readonly int _value;
            readonly int _token;
            readonly Context _context;

            public Context Context => _context;
            [Obsolete]
            public int Value => _value;
            /// <summary>
            /// 各々実体に空ではない別々の値が振られます。
            /// </summary>
            public int Token => _token;

            public Index(Context context)
            {
                int i;
                lock (context._refs)
                {
                    for (i = 0; i < context._refs.Count; i++)
                    {
                        if (!context._refs[i].IsAlive)
                        {
                            context._refs[i].Target = this;
                            _value = i;
                            goto fin;
                        }
                    }
                }
                context._refs.Add(new(this));
                _value = i;

                fin:;
                _token = Interlocked.Increment(ref context._nextToken);
                if (_token == 0) throw new Exception();
                _context = context;
            }
            public Index(Context context, int value)
            {
                lock (context._refs)
                {
                    if (context._refs[value].IsAlive) throw new ArgumentException();

                    context._refs[value].Target = this;
                }

                _value = value;
                _context = context;
            }

            public override bool Equals(object? obj) => ReferenceEquals(this, obj);
            public bool Equals(Index? other) => ReferenceEquals(this, other);
            public override int GetHashCode() => HashCode.Combine(this);

            /// <summary>
            /// 索を使用しないことを明示的に宣言します。これによって索を使用する一部のクラスのメモリ使用量が削減される可能性がありますが、宣言された索を使用した場合の動作は未定義です。
            /// </summary>
            protected void ExplicitlyDisuse()
            {
                lock (_context._refs)
                {
                    var r = _context._refs[_value];
                    if (r.IsAlive && this.Equals(r.Target)) r.Target = null;
                }
            }

            public static bool operator ==(Index? left, Index? right) => EqualityComparer<Index?>.Default.Equals(left, right);
            public static bool operator !=(Index? left, Index? right) => !(left == right);

            public static implicit operator int(Index index) => index._value;
        }
    }

    public class Context<TIndex> : Context where TIndex : Context.Index
    {
        public IndexCollection Indexes => new(this);

        public readonly struct IndexCollection : SysGC::ICollection<TIndex>
        {
            readonly Context<TIndex> _context;

            public int Count
            {
                get
                {
                    var c = 0;
                    foreach (var @ref in _context._refs)
                    {
                        if (@ref.IsAlive) c++;
                    }
                    return c;
                }
            }
            public bool IsReadOnly => true;

            public IndexCollection(Context<TIndex> context)
            {
                _context = context;
            }

            public bool Contains(TIndex item)
            {
                var wR = _context._refs[item];
                return item.Equals(wR.Target);
            }
            public void CopyTo(TIndex[] array, int arrayIndex) => throw new NotImplementedException();
            public Enumerator GetEnumerator() => new(this);

            void SysGC::ICollection<TIndex>.Add(TIndex item) => throw new NotSupportedException();
            void SysGC::ICollection<TIndex>.Clear() => throw new NotSupportedException();
            bool SysGC::ICollection<TIndex>.Remove(TIndex item) => throw new NotSupportedException();
            IEnumerator<TIndex> IEnumerable<TIndex>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public struct Enumerator : IEnumerator<TIndex>
            {
                readonly IndexCollection _indexes;
                int _i;
                TIndex _c;

                public Enumerator(IndexCollection indexes)
                {
                    _indexes = indexes;
                    _i = -1;
                    _c = null!;
                }

                public readonly TIndex Current => _c;
                readonly object IEnumerator.Current => _c;

                public readonly void Dispose() { }
                public bool MoveNext()
                {
                    var refs = _indexes._context._refs;
                    re:;
                    if (refs.Count <= ++_i) return false;
                    if (refs[_i].Target is not TIndex c) goto re;
                    _c = c;
                    return true;
                }
                public void Reset()
                {
                    _i = -1;
                }
            }
        }
    }
}