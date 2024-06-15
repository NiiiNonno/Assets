using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static Nonno.Assets.Sample;

namespace Nonno.Assets.Collections;

// http://wwwa.pikara.ne.jp/okojisan/rb-tree/index.html

public class RedBlackDictionary<TKey, TValue> where TKey : IComparable<TKey>
{
    Node? _root;

    public bool IsEmpty => _root == null;

    public bool TryAdd(TKey key, TValue value)
    {
        if (_root is null)
        {
            _root = new() { k = key, v = value };
            return true;
        }

        Node c = _root;
        while (true)
        {
            switch (key.CompareTo(c.k))
            {
            case 0:
                return false;
            case > 0:
                if (c.R is null)
                {
                    var n = _pool.Get();
                    n.k = key;
                    n.v = value;
                    n.L = n.R = null;
                    n.f = true;
                    c.R = n;
                    if (c.f) Rotate(n, ref _root);
                    return true;
                }
                c = c.R;
                break;
            case < 0: // key < c.k
                if (c.L is null)
                {
                    var n = _pool.Get();
                    n.k = key;
                    n.v = value;
                    n.L = n.R = null;
                    n.f = true;
                    c.L = n;
                    if (c.f) Rotate(n, ref _root);
                    return true;
                }
                c = c.L;
                break;
            }
        }

        static void Rotate(Node c, ref Node? root)
        {
            Debug.Assert(c != root);
            switch (c.P!.L == c, c.P!.P!.L == c.P)
            {
            case (true, true): // LL
                {
                    /*
                     *     pp_
                     *   +-+-+
                     *   p^
                     * +-+-+
                     * c   pr
                     */
                    var p = c.P;
                    var pr = p.R;
                    var pp = p.P;
                    var ppp = pp.P;
                    pp.L = pr;
                    p.R = pp;
                    c.f = false;
                    if (ppp is null)
                    {
                        root = p;
                        p.f = false;
                        return;
                    }
                    if (ppp.L == pp) ppp.L = p;
                    else ppp.R = p;
                    /*
                     *   p^
                     * +-+-+
                     * c_  pp_
                     *   +-+-+
                     *  pr
                     */
                    Rotate(p, ref root);
                    return;
                }
            case (false, true): // RL
                {
                    /*
                     *     pp_
                     *   +-+-+
                     *   p^
                     * +-+-+
                     *     c
                     *   +-+-+
                     *   l   r
                     */
                    var p = c.P;
                    var l = c.L;
                    var r = c.R;
                    var pp = p.P;
                    var ppp = pp.P;
                    c.L = p;
                    c.R = pp;
                    p.R = l;
                    pp.L = r;
                    p.f = false;
                    c.f = true;
                    if (ppp is null)
                    {
                        root = c;
                        p.f = false;
                        return;
                    }
                    if (ppp.L == pp) ppp.L = c;
                    else ppp.R = c;
                    /*
                     *      c^
                     *   +--+--+
                     *   p_    pp_
                     * +-+-+ +-+-+
                     *     l r
                     */
                    Rotate(c, ref root);
                    return;
                }
            case (true, false): // LR
                {
                    /*
                     *   pp_
                     * +-+-+
                     *     p^
                     *   +-+-+
                     *   c   
                     * +-+-+
                     * l   r
                     */
                    var p = c.P;
                    var l = c.L;
                    var r = c.R;
                    var pp = p.P;
                    var ppp = pp.P;
                    c.R = p;
                    c.L = pp;
                    p.L = r;
                    pp.R = l;
                    p.f = false;
                    c.f = true;
                    if (ppp is null)
                    {
                        root = c;
                        p.f = false;
                        return;
                    }
                    if (ppp.L == pp) ppp.L = c;
                    else ppp.R = c;
                    /*
                     *      c^
                     *   +--+--+
                     *   pp_   p_
                     * +-+-+ +-+-+
                     *     l r
                     */
                    Rotate(c, ref root);
                    return;
                }
            case (false, false): // RR
                {
                    /*
                     *   pp_
                     * +-+-+
                     *     p^
                     *   +-+-+
                     *  pl   c
                     */
                    var p = c.P;
                    var pl = p.L;
                    var pp = p.P;
                    var ppp = pp.P;
                    p.L = pp;
                    pp.R = pl;
                    c.f = false;
                    if (ppp is null)
                    {
                        root = p;
                        p.f = false;
                        return;
                    }
                    if (ppp.L == pp) ppp.L = p;
                    else ppp.R = p;
                    /*
                     *     p^
                     *   +-+-+
                     *   pp_ c_
                     * +-+-+
                     *     pl
                     */
                    Rotate(p, ref root);
                    return;
                }
            }
        }
    }

    public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        Node? c = _root;
        while (true)
        {
            if (c is null)
            {
                value = default;
                return false;
            }

            switch (key.CompareTo(c.k))
            {
            case 0:
                value = c.v;
                switch (c.L, c.R)
                {
                case (null, null):
                    if (c == _root) _root = null;
                    else if (c.P!.L == c) c.P.L = null;
                    else c.P.R = null;
                    return true;
                case (_, null):
                    if (c == _root) _root = c.L;
                    else if (c.P!.L == c) c.P.L = c.L;
                    else c.P.R = c.L;
                    return true;
                case (null, _):
                    if (c == _root) _root = c.R;
                    else if (c.P!.L == c) c.P.L = c.R;
                    else c.P.R = c.R;
                    return true;
                case (var l, var r):
                    var n = c.L;
                    while (n.R is { }) n = n.R; // 最大値を求めている。
                    var np = n.P!;
                    np.R = n.L;
                    if (c.P!.L == c) c.P.L = n;
                    else c.P.R = n;
                    n.L = l;
                    n.R = r;
                    if (!c.f) Rotate(n, ref _root);
                    return true;
                }
            case > 0:
                c = c.R;
                break;
            case < 0: // key < c.k
                c = c.L;
                break;
            }
        }

        static void Rotate(Node c, ref Node? root)
        {
            if (c.P is null) return;

            var p = c.P;
            if (p.L == c)
            {
                var o = p.R;
                Debug.Assert(o is not null);
                if (o.f)
                {
                    /*
                     *   p_
                     * +-+-+
                     * c   o^
                     *   +-+-+
                     *  ol
                     */
                    var ol = o.L;
                    var pp = p.P;
                    o.L = p;
                    p.R = ol;
                    o.f = false;
                    p.f = true;
                    if (pp == null) ;
                    else if (pp.L == p) pp.L = o;
                    else pp.R = o;
                    /*
                     *     o_
                     *   +-+-+
                     *   p^
                     * +-+-+
                     * c   ol
                     */

                    p = o;
                    o = p.R;
                    Debug.Assert(o is not null);
                }
                switch (o.L != null && o.L.f, o.R != null && o.R.f)
                {
                case (true, _):
                    {
                        /*
                         *    p?
                         *  +-+-+
                         *  c   o_
                         *    +-+-+
                         *    s^
                         *  +-+-+
                         * sl   sr
                         */
                        var s = o.L!;
                        var sr = s.R;
                        var sl = s.L;
                        var pf = p.f;
                        var pp = p.P;
                        s.L = p;
                        s.R = o;
                        p.R = sl;
                        o.L = sr;
                        p.f = false;
                        s.f = pf;
                        if (pp is null) return;
                        if (pp.L == p) pp.L = s;
                        else pp.R = s;
                        return;
                        /*
                         *       s?
                         *   +---+---+
                         *   p_      o_
                         * +-+-+   +-+-+
                         * c   sl sr
                         */
                    }
                case (_, true):
                    {
                        /*
                         *   p?
                         * +-+-+
                         * c   o_
                         *   +-+-+
                         *  ol   s^
                         */
                        var s = o.R!;
                        var ol = o.L;
                        var pf = p.f;
                        var pp = p.P;
                        o.L = p;
                        p.R = ol;
                        s.f = false;
                        o.f = pf;
                        if (pp is null) return;
                        if (pp.L == p) pp.L = s;
                        else pp.R = s;
                        return;
                        /*
                         *     o?
                         *   +-+-+
                         *   p_  s_
                         * +-+-+
                         * c   ol
                         */
                    }
                default:
                    {
                        /*
                         *   p?
                         * +-+-+
                         * c   o_
                         */
                        var pf = p.f;
                        p.f = false;
                        o.f = true;
                        /*
                         *   p_
                         * +-+-+
                         * c   o^
                         */
                        if (!pf) Rotate(p, ref root);
                        return;
                    }
                }
            }
            else
            {
                // 逆にしただけ。コメント未修正。
                var o = p.L;
                Debug.Assert(o is not null);
                if (o.f)
                {
                    /*
                     *   p_
                     * +-+-+
                     * c   o^
                     *   +-+-+
                     *  ol   or
                     */
                    var ol = o.R;
                    var or = o.L;
                    p.R = o;
                    p.L = or;
                    o.R = c;
                    o.L = ol;
                    /*
                     *     p_
                     *   +-+-+
                     *   o^   or
                     * +-+-+
                     * c   ol
                     */

                    p = o;
                    o = p.L;
                    Debug.Assert(o is not null);
                }
                switch (o.R != null && o.R.f, o.L != null && o.L.f)
                {
                case (true, _):
                    {
                        /*
                         *    p?
                         *  +-+-+
                         *  c   o_
                         *    +-+-+
                         *    s^
                         *  +-+-+
                         * sl   sr
                         */
                        var s = o.R!;
                        var sr = s.L;
                        var sl = s.R;
                        var pf = p.f;
                        var pp = p.P;
                        s.R = p;
                        s.L = o;
                        p.L = sl;
                        o.R = sr;
                        p.f = false;
                        s.f = pf;
                        if (pp is null) return;
                        if (pp.R == p) pp.R = s;
                        else pp.L = s;
                        return;
                        /*
                         *       s?
                         *   +---+---+
                         *   p_      o_
                         * +-+-+   +-+-+
                         * c   sl sr
                         */
                    }
                case (_, true):
                    {
                        /*
                         *   p?
                         * +-+-+
                         * c   o_
                         *   +-+-+
                         *  ol   s^
                         */
                        var s = o.L!;
                        var ol = o.R;
                        var pf = p.f;
                        var pp = p.P;
                        o.R = p;
                        p.L = ol;
                        s.f = false;
                        o.f = pf;
                        if (pp is null) return;
                        if (pp.R == p) pp.R = s;
                        else pp.L = s;
                        return;
                        /*
                         *     o?
                         *   +-+-+
                         *   p_  s_
                         * +-+-+
                         * c   ol
                         */
                    }
                default:
                    {
                        /*
                         *   p?
                         * +-+-+
                         * c   o_
                         */
                        var pf = p.f;
                        p.f = false;
                        o.f = true;
                        /*
                         *   p_
                         * +-+-+
                         * c   o^
                         */
                        if (!pf) Rotate(p, ref root);
                        return;
                    }
                }
            }
        }
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)]out TValue value)
    {
        var c = _root;
        while (c is not null)
        {
            switch (c.k.CompareTo(key))
            {
            case 0:
                value = c.v;
                return true;
            case > 0:
                c = c.R;
                break;
            case < 0:
                c = c.L;
                break;
            }
        }
        value = default;
        return false;
    }

    public bool TrySetValue(TKey key, TValue value)
    {
        var c = _root;
        while (c is not null)
        {
            switch (c.k.CompareTo(key))
            {
            case 0:
                c.v = value;
                return true;
            case > 0:
                c = c.R;
                break;
            case < 0:
                c = c.L;
                break;
            }
        }
        return false;
    }

    public TValue GetTheClosest(TKey key)
    {
        var c = _root;
        while (c is not null)
        {
            switch (c.k.CompareTo(key))
            {
            case 0:
                return c.v;
            case > 0:
                if (c.R is null) return c.v;
                c = c.R;
                break;
            case < 0:
                if (c.L is null) return c.v;
                c = c.L;
                break;
            }
        }
        throw new Exception("寶に一切の要素がありませんでした。");
    }

    protected class Node
    {
        public TKey k = default!;
        public TValue v = default!;
        private Node? l, r, p;
        /// <summary>
        /// 黒なら偽、紅なら真。
        /// </summary>
        public bool f;

        public Node? P => p;
        public Node? L
        {
            get => l;
            set
            {
                l = value;
                if (value != null) value.p = this;
            }
        }
        public Node? R
        {
            get => r;
            set
            {
                r = value;
                if (value != null) value.p = this;
            }
        }
    }

    static readonly ObjectPool<Node> _pool = new(new ObjectSource<Node>(() => new Node()));
}

public class RedBlackSet<T> where T : IComparable<T>
{
    readonly RedBlackDictionary<T, Void> _d = new();
    protected readonly struct Void();
}
