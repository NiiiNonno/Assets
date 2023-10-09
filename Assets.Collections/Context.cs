#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using SysGC = System.Collections.Generic;

namespace Nonno.Assets.Collections
{
    public class Context
    {
        protected readonly List<WeakReference> _refs = new();

        public override bool Equals(object? obj) => Equals(obj as Context);
        public bool Equals(Context? other) => other is not null && ReferenceEquals(_refs, other._refs);
        public override int GetHashCode() => HashCode.Combine(_refs);

        public static bool operator ==(Context? left, Context? right) => EqualityComparer<Context?>.Default.Equals(left, right);
        public static bool operator !=(Context? left, Context? right) => !(left == right);

        public class Index : IEquatable<Index?>
        {
            public Context Context { get; }
            public int Value { get; }

            public Index(Context context)
            {
                int i;
                for (i = 0; i < context._refs.Count; i++)
                {
                    if (!context._refs[i].IsAlive)
                    {
                        context._refs[i].Target = this;
                        Value = i;
                        goto fin;
                    }
                }
                context._refs.Add(new(this));
                Value = i;

                fin:;
                Context = context;
            }
            public Index(Context context, int value)
            {
                if (context._refs[value].IsAlive) throw new ArgumentException();

                context._refs[value].Target = this;

                Value = value;
                Context = context;
            }

            public override bool Equals(object? obj) => Equals(obj as Index);
            public bool Equals(Index? other) => other is not null && EqualityComparer<Context>.Default.Equals(Context, other.Context) && Value == other.Value;
            public override int GetHashCode() => HashCode.Combine(Context, Value);

            public static bool operator ==(Index? left, Index? right) => EqualityComparer<Index?>.Default.Equals(left, right);
            public static bool operator !=(Index? left, Index? right) => !(left == right);
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
                foreach (var r in _context._refs)
                {
                    if (r.Target is { } t && t.Equals(item)) return true;
                }
                return false;
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