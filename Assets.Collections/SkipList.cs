using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Collections;
public class SkipList<T> : IEnumerable<T>
{
    readonly IComparer<T> _comparer;
    int _depth;
    SkipListNode? _first;

    public SkipListNode? First => _first;
    public int Depth
    {
        get => _depth;
        set
        {
            if (_depth != value)
            {
                var c = _first;
                while (c is not null)
                {
                    

                    c = c.NextNode;
                }

                _depth = value;
            }
        }
    }

    public SkipList(IComparer<T> comparer, int depth = 4)
    {
        _comparer = comparer;
        _depth = depth;
    }

    public SkipListNode? Remove(T value)
    {
        var r = Find(value);
        if (r is not null) Remove(r);
        return r;
    }

    public SkipListNode Insert(T value)
    {
        if (_first is null) return _first = new(value, null, null) { Height = _depth };

        var c = _first;
        for (int i = Depth - 1; i >= 0; i--)
        {
            while (c._ns[i] is SkipListNode next)
            {
                switch (_comparer.Compare(value, next.Value))
                {
                case > 0:
                    c = next;
                    break;
                case 0:
                    throw new ArgumentException("指定された値は既に存在します。");
                case < 0:
                    goto down;
                }
            }
            down:;
        }

        return InsertAfter(c, value);
    }

    public SkipListNode? Find(T value)
    {
        if (_first is null) return null;

            var c = _first;
        for (int i = Depth - 1; i >= 0; i--)
        {
            while (c._ns[i] is SkipListNode next)
            {
                switch (_comparer.Compare(value, next.Value))
                {
                case > 0:
                    c = next;
                    break;
                case 0:
                    return next;
                case < 0:
                    goto down;
                }
            }
            down:;
        }

        return null;
    }

    public SkipListNode InsertAfter(SkipListNode node, T value, int height = 1)
    {
        var next = new SkipListNode(value, node, node.NextNode);
        next.Height = height;
        return next;
    }
    public SkipListNode InsertBefore(SkipListNode node, T value, int height = 1)
    {
        var previous = new SkipListNode(value, node, node.PreviousNode);
        previous.Height = height;
        return previous;
    }

    public void Remove(SkipListNode node)
    {
        for (int i = 0; i < node.Height; i++)
        {
            if (node._ps[i] is SkipListNode p) p._ns[i] = node._ns[i];
            if (node._ns[i] is SkipListNode n) n._ps[i] = node._ps[i];
            node._ps[i] = node._ns[i] = null;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        var c = _first;
        while (c is not null)
        {
            yield return c.Value;
            c = c.NextNode;
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public class SkipListNode
    {
        internal SkipListNode?[] _ps;
        internal SkipListNode?[] _ns;
        T _v;

        public ReadOnlyMemory<SkipListNode?> PreviousNodes => _ps;
        public ReadOnlyMemory<SkipListNode?> NextNodes => _ns;
        public SkipListNode? PreviousNode => _ps[0];
        public SkipListNode? NextNode => _ns[0];
        public int Height
        {
            get
            {
#if DEBUG
                if (_ps.Length != _ns.Length) throw new Exception();
#endif
                return _ps.Length;
            }
            set
            {
                var height = Height;
                if (height > value)
                {
                    var ps = _ps;
                    var ns = _ns;
                    _ps = new SkipListNode[value];
                    _ns = new SkipListNode[value];

                    int i = 0;
                    for (; i < value; i++) // 前の参照を引き継ぐ。
                    {
                        _ps[i] = ps[i];
                        _ns[i] = ns[i];
                    }
                    for (; i < height; i++) // 前の高い節の高位を後ろの高い節に繋ぐ。
                    {
                        ps[i]!._ns[i] = ns[i];
                    }
                }
                else if (height < value)
                {
                    var ps = _ps;
                    var ns = _ns;
                    _ps = new SkipListNode[value];
                    _ns = new SkipListNode[value];

                    int i = 0;
                    for (; i < height; i++) // 前の参照を引き継ぐ。
                    {
                        _ps[i] = ps[i];
                        _ns[i] = ns[i];
                    }
                    for (; i < value; i++) // 前と後の節の間に割り込む。
                    {
                        if (FindPreviousNode(i + 1) is SkipListNode p)
                        {
                            _ps[i] = p;
                            p._ns[i] = this;
                        }
                        if (FindNextNode(i + 1) is SkipListNode n)
                        {
                            _ns[i] = n;
                            n._ps[i] = this;
                        }
                    }
                }
            }
        }
        public T Value => _v;

        internal SkipListNode(T value, SkipListNode? previousNode, SkipListNode? nextNode)
        {
            _ps = new[] { previousNode };
            _ns = new[] { nextNode };
            _v = value;
        }
        private SkipListNode(T value, int height, SkipListNode?[]? previousNodes, SkipListNode?[]? nextNodes)
        {
            _ps = previousNodes ?? new SkipListNode[height];
            _ns = nextNodes ?? new SkipListNode[height];
            _v = value;
        }

        private SkipListNode? FindPreviousNode(int height)
        {
            var c = this;
            while (c.Height < height)
            {
                c = c._ps[^1];
                if (c is null) return null;
            }
            return c;
        }
        private SkipListNode? FindNextNode(int height)
        {
            var c = this;
            while (c.Height < height)
            {
                c = c._ns[^1];
                if (c is null) return null;
            }
            return c;
        }

        public bool Belongs(SkipList<T> to)
        {
            var c = to.First;
            while (c is not null)
            {
                if (Equals(c)) return true;

                c = c.NextNode;
            }

            return false;
        }

        public ref T GetValue() => ref _v;
    }
}

public class SkipDictionary<TKey, TValue> : SkipList<KeyValuePair<TKey, TValue>> where TKey : IComparable<TKey>
{
    public SkipDictionary() : base(Comparer.INSTANCE)
    {

    }

    class Comparer : IComparer<KeyValuePair<TKey, TValue>>
    {
        public static Comparer INSTANCE = new();

        public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) => x.Key.CompareTo(y.Key);
    }
}
