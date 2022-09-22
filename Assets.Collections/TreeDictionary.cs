using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Integar = System.Int32;

namespace Nonno.Assets.Collections;

/// <summary>
/// 二分探索木による整数によって一意にオブジェクトを決定する辞書またはリストを表します。
/// </summary>
/// <typeparam name="T">
/// 整数によって決定されるオブジェクトの型。
/// </typeparam>
public class TreeDictionary<T> : IList<T>, IDictionary<Integar, T>
{
    /// <summary>
    /// 木辞書が空であるかどうかを取得します。
    /// </summary>
    public bool IsEmpty
    {
        [MemberNotNullWhen(false, nameof(Root))]
        get => Root == null;
    }
    /// <summary>
    /// 根の節の欄。
    /// </summary>
    protected Node? Root { get; private set; }

    /// <summary>
    /// キーとなる整数のコレクションを取得します。
    /// </summary>
    public KeyCollection Keys => new(this);
    ICollection<Integar> IDictionary<int, T>.Keys => Keys;
    /// <summary>
    /// 整数によって決定される全てのオブジェクトのコレクションを取得します。
    /// </summary>
    public ValueCollection Values => new(this);
    ICollection<T> IDictionary<int, T>.Values => Values;
    /// <summary>
    /// 格納されている要素数を取得します。
    /// </summary>
    public int Count => Root == null ? 0 : Root.ChildrenCount + 1;
    /// <summary>
    /// 整数によって一意に決定されるオブジェクトを取得または設定します。
    /// </summary>
    /// <param name="key">
    /// オブジェクトを決定する整数。
    /// </param>
    /// <returns>
    /// 決定されたオブジェクト。
    /// </returns>
    public T this[Integar key]
    {
        get => TryGetValue(key, out var r) ? r! : throw new KeyNotFoundException();
        set => TrySetValue(key, value);
    }

    /// <summary>
    /// 木辞書を初期化します。
    /// </summary>
    public TreeDictionary() { }
    /// <summary>
    /// 木辞書を根の節を指定して初期化します。
    /// </summary>
    /// <param name="origin">
    /// 節の位置。
    /// </param>
    /// <param name="root">
    /// 根の節に設定される値。
    /// </param>
    public TreeDictionary(Integar origin, T root)
    {
        Root = new(origin, root);
    }

    /// <summary>
    /// 設定されているオブジェクトを取得するよう試みます。
    /// </summary>
    /// <param name="key">
    /// 取得するオブジェクトを決定する整数。
    /// </param>
    /// <param name="value">
    /// 決定されたオブジェクト。取得できなかった場合や取得したオブジェクトが無参照であった場合は`null`。
    /// </param>
    /// <returns>
    /// 正しく取得できた場合は`true`。そうで無い場合は`false`。
    /// </returns>
    public bool TryGetValue(Integar key, [MaybeNullWhen(false)] out T value)
    {
        if (TryGetNode(key, out var node))
        {
            value = node.Value;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public T? GetAppearingValue(Integar ballparkKey) => GetAppearingNode(ballparkKey) is Node node ? node.Value : default;

    /// <summary>
    /// 既存の決定されるオブジェクトを変更するよう試みます。
    /// </summary>
    /// <param name="key">
    /// 設定するオブジェクトを決定する整数。
    /// </param>
    /// <param name="value">
    /// 設定するオブジェクト。
    /// </param>
    /// <returns>
    /// 正しく設定できた場合は`true`。そうで無い場合は`false`。
    /// </returns>
    public bool TrySetValue(Integar key, T value)
    {
        if (TryGetNode(key, out var node))
        {
            node.Value = value;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 節を取得するよう試みます。
    /// </summary>
    /// <param name="key">
    /// 取得する節の数値。
    /// </param>
    /// <param name="result">
    /// 取得した節。取得できなかった場合は`null`。
    /// </param>
    /// <returns>
    /// 正しく取得できた場合は`true`。そうで無い場合は`false`。
    /// </returns>
    internal bool TryGetNode(Integar key, [NotNullWhen(true)] out Node? result)
    {
        result = Root;
        while (result != null)
        {
            if (result.Key > key) result = result.Left;
            else if (result.Key < key) result = result.Right;
            else break;
        }
        return result != null;
    }
    /// <summary>
    /// 節を取得するよう試みます。
    /// </summary>
    /// <param name="value">
    /// 取得する節が保有するオブジェクト。
    /// </param>
    /// <param name="result">
    /// 取得した節。取得できなかった場合は`null`。
    /// </param>
    /// <returns>
    /// 正しく取得できた場合は`true`。そうで無い場合は`false`。
    /// </returns>
    internal bool TryGetNode(T value, [NotNullWhen(true)] out Node? result)
    {
        if (Root == null)
        {
            result = null;
            return false;
        }
        result = Root.GetNode(value);
        return result != null;
    }

    internal Node? GetAppearingNode(Integar ballparkKey)
    {
        var r = Root;
        if (r == null) return null;

#if DEBUG
            int i = 0;
#endif
        while (true)
        {
            if (r.Key > ballparkKey)
            {
                if (r.Left == null) return r;
                r = r.Left;
            }
            else if (r.Key < ballparkKey)
            {
                if (r.Right == null) return r;
                r = r.Right;
            }
            else
            {
                return r;
            }
#if DEBUG
                if (i++ > 100000) throw new Exception("繰り返し回数が許容範囲を超過しました。");
#endif
        }
    }

    /// <summary>
    /// 指定した節の親に当たる節を取得するよう試みます。
    /// </summary>
    /// <param name="key">
    /// 取得する節の子の数値。
    /// </param>
    /// <param name="result">
    /// 取得した節。取得できなかった場合または指定された節が根の節だった場合は`null`。
    /// </param>
    /// <returns>
    /// 正しく取得できた場合は`true`。そうで無い場合は`false`。
    /// </returns>
    internal bool TryGetParentNode(Integar key, out Node? result)
    {
        if (Root == null)
        {
            result = null;
            return false;
        }

        if (Root.Key == key)
        {
            result = null;
            return true;
        }

        var current = Root;
#if DEBUG
            int i = 0;
#endif
        while (true)
        {
            if (key < current.Key)
            {
                if (current.Right is Node right)
                {
                    if (right.Key == key)
                    {
                        result = current;
                        return true;
                    }
                    current = right;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            else if (key > current.Key)
            {
                if (current.Left is Node left)
                {
                    if (left.Key == key)
                    {
                        result = current;
                        return true;
                    }
                    current = left;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
#if DEBUG
                if (i++ > 100000) throw new Exception("繰り返し回数が許容範囲を超過しました。");
#endif
        }
    }

    /// <summary>
    /// オブジェクトからそれを決定する整数を取得します。
    /// </summary>
    /// <param name="of">
    /// 取得する整数によって決定されているオブジェクト。
    /// </param>
    /// <returns>
    /// 正しく取得できた場合はオブジェクトを決定する整数。そうで無い場合は`null`。
    /// </returns>
    public int? GetIndex(T of)
    {
        _ = TryGetNode(of, out var node);
        return node?.Key;
    }
    /// <summary>
    /// オブジェクトからそれを決定する整数を取得するよう試みます。
    /// </summary>
    /// <param name="of">
    /// 取得する整数によって決定されているオブジェクト。
    /// </param>
    /// <param name="index">
    /// オブジェクトを決定する整数。取得できなかった場合は未定義の値。
    /// </param>
    /// <returns>
    /// 正しく取得できた場合は`true`。そうで無い場合は`false`。
    /// </returns>
    public bool TryGetIndex(T of, out Integar index)
    {
        if (TryGetNode(of, out var node))
        {
            index = node.Key;
            return true;
        }
        index = -1;
        return false;
    }

    /// <summary>
    /// 整数からそれがオブジェクトを決定するかどうかを取得します。
    /// </summary>
    /// <param name="key">
    /// 確認する整数。
    /// </param>
    /// <returns>
    /// 整数がオブジェクトを決定する場合は`true`。そうで無い場合は`false`。
    /// </returns>
    public bool Contains(Integar key) => TryGetNode(key, out _);
    bool System.Collections.Generic.IDictionary<Integar, T>.ContainsKey(Integar key) => Contains(key);
    /// <summary>
    /// オブジェクトからそれが決定される対象であるかどうかを取得します。
    /// </summary>
    /// <param name="value">
    /// 確認するオブジェクト。
    /// </param>
    /// <returns>
    /// オブジェクトが決定される対象である場合は`true`。そうで無い場合は`false`。
    /// </returns>
    public bool Contains(T value) => TryGetNode(value, out _);
    bool System.Collections.Generic.ICollection<KeyValuePair<Integar, T>>.Contains(KeyValuePair<Integar, T> item) => TryGetNode(item.Key, out var node) && Equals(node.Value, item.Value);

    /// <summary>
    /// 新たな整数と、それによって決定されるオブジェクトを追加します。
    /// </summary>
    /// <param name="key">
    /// オブジェクトを決定する新たな整数。
    /// </param>
    /// <param name="value">
    /// 整数によって決定されるオブジェクト。
    /// </param>
    /// <returns>
    /// 正しく追加できた場合は`true`。そうで無い場合は`false`。
    /// </returns>
    public bool Add(Integar key, T value)
    {
        if (Root == null)
        {
            Root = new(key, value);
            return true;
        }

        var current = Root;
        while (true)
        {
            if (key < current.Key)
            {
                if (current.Left is Node left)
                {
                    current = left;
                }
                else
                {
                    current.Left = new(key, value);
                    return true;
                }
            }
            else if (key > current.Key)
            {
                if (current.Right is Node right)
                {
                    current = right;
                }
                else
                {
                    current.Right = new(key, value);
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
    }
    void System.Collections.Generic.IList<T>.Insert(int index, T item) => _ = Add(index, item);
    void System.Collections.Generic.ICollection<KeyValuePair<Integar, T>>.Add(KeyValuePair<Integar, T> item) => _ = Add(item.Key, item.Value);
    void System.Collections.Generic.IDictionary<Integar, T>.Add(int key, T value) => _ = Add(key, value);

    public bool Add(T value, Integar ballparkKey = 0)
    {
        throw new NotImplementedException();
    }
    void System.Collections.Generic.ICollection<T>.Add(T item) => Add(item);

    public bool Remove(Integar key)
    {
        if (Root == null)
        {
            return false;
        }

        if (Root.Key == key)
        {
            Root = null;
            return true;
        }

        var current = Root;
#if DEBUG
            int i = 0;
#endif
        while (true)
        {
            if (key < current.Key)
            {
                if (current.Right is Node right)
                {
                    if (right.Key == key)
                    {
                        current.Right = Node.Connect(right.Left, right.Right);
                        return true;
                    }
                    current = right;
                }
                else
                {
                    return false;
                }
            }
            else if (key > current.Key)
            {
                if (current.Left is Node left)
                {
                    if (left.Key == key)
                    {
                        current.Left = Node.Connect(left.Left, left.Right);
                        return true;
                    }
                    current = left;
                }
                else
                {
                    return false;
                }
            }
#if DEBUG
                if (i++ > 100000) throw new Exception("繰り返し回数が許容範囲を超過しました。");
#endif
        }
    }

    bool System.Collections.Generic.ICollection<KeyValuePair<Integar, T>>.Remove(KeyValuePair<Integar, T> item)
    {
        if (Root == null) return false;

        if (TryGetParentNode(item.Key, out var node))
        {
            if (node == null)
            {
                if (Root.Key == item.Key && Equals(Root.Value, item.Value))
                {
                    Clear();
                    return true;
                }
            }
            else
            {
                if (node.Left is Node left && left.Key == item.Key)
                {
                    if (Equals(left.Value, item.Value)) node.Left = Node.Connect(left.Left, left.Right);
                    return true;
                }
                else if (node.Right is Node right)
                {
                    if (Equals(right.Value, right.Value)) node.Right = Node.Connect(right.Left, right.Right);
                    return true;
                }
            }
        }
        return false;
    }

    public bool Remove(T value, Integar ballparkKey = 0)
    {
        throw new NotImplementedException();
    }

    public void Clear() => Root = null;

    public void Copy(Span<T> to, ref int index)
    {
        if (Root != null) Root.Copy(to, ref index);
    }
    public void Copy(Span<Integar> to, ref int index)
    {
        if (Root != null) Root.Copy(to, ref index);
    }
    public void Copy(Span<KeyValuePair<Integar, T>> to, ref int index)
    {
        if (Root != null) Root.Copy(to, ref index);
    }
    void System.Collections.Generic.ICollection<KeyValuePair<Integar, T>>.CopyTo(KeyValuePair<Integar, T>[] array, int arrayIndex) => Copy(to: array, ref arrayIndex);

    public IEnumerator<T> GetEnumerator() => Root == null ? EmptyEnumerator<T>.INSTANCE : Root.GetValueEnumerator();
    IEnumerator<KeyValuePair<Integar, T>> IEnumerable<KeyValuePair<Integar, T>>.GetEnumerator() => Root is IEnumerable<Node> nodes ? nodes.Select(x => new KeyValuePair<Integar, T>(x.Key, x.Value)).GetEnumerator() : EmptyEnumerator<KeyValuePair<Integar, T>>.INSTANCE;
    IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException("`IEnumerable<KeyValuePair<Integar, T>>`と`IEnumerable<T>`の区別があいまいなため、この呼び出しはサポートされていません。");

    public bool TryAdd(int key, T value) => throw new NotImplementedException();
    public bool TryRemove(int key, [MaybeNullWhen(false)] out T value) => throw new NotImplementedException();
    public bool TryAdd(KeyValuePair<int, T> item) => throw new NotImplementedException();
    public bool TryRemove(KeyValuePair<int, T> item) => throw new NotImplementedException();
    int IList<T>.GetIndex(T of) => throw new NotImplementedException();
    T IList<T>.Remove(int at) => throw new NotImplementedException();
    public bool TryAdd(T item) => throw new NotImplementedException();
    public bool TryRemove(T item) => throw new NotImplementedException();

    protected internal class Node : IEnumerable<T>, IEnumerable<Node>
    {
        readonly Integar _key;
        T _value;
        Node? _left, _right;

        public int ChildrenCount => (_left, _right) switch
        {
            (null, null) => 0,
            (null, var right) => right.ChildrenCount + 1,
            (var left, null) => left.ChildrenCount + 1,
            (var left, var right) => left.ChildrenCount + right.ChildrenCount + 2,
        };
        public T Value { get => _value; set => _value = value; }
        public Integar Key => _key;
        public Node? Left { get => _left; set => _left = value; }
        public Node? Right { get => _right; set => _right = value; }
        public Node Leftmost
        {
            get
            {
                var r = this;
                while (r._left != null) r = r._left;
                return r;
            }
        }
        public Node Rightmost
        {
            get
            {
                var r = this;
                while (r._right != null) r = r._right;
                return r;
            }
        }

        public Node(Integar key, T value)
        {
            _key = key;
            _value = value;
            _left = _right = null;
        }

        public Node? GetNode(T value)
        {
            if (value == null) return GetNodeValueIsNull();
            if (value is IEquatable<T> equatable) return GetNode(equatable);
            return GetNodeUnoptimizable(value);
        }
        public Node? GetNodeUnoptimizable(T value)
        {
            if (_value == null) return this;
            if (_left != null && _left.GetNodeUnoptimizable(value) is Node r1) return r1;
            if (_right != null && _right.GetNodeUnoptimizable(value) is Node r2) return r2;
            return null;
        }
        public Node? GetNode(IEquatable<T> value)
        {
            if (_value == null) return this;
            if (_left != null && _left.GetNode(value) is Node r1) return r1;
            if (_right != null && _right.GetNode(value) is Node r2) return r2;
            return null;
        }
        public Node? GetNodeValueIsNull()
        {
            if (_value == null) return this;
            if (_left != null && _left.GetNodeValueIsNull() is Node r1) return r1;
            if (_right != null && _right.GetNodeValueIsNull() is Node r2) return r2;
            return null;
        }

        public void Copy(Span<T> to, ref int index)
        {
            if (_left != null) _left.Copy(to, ref index);
            to[index++] = _value;
            if (_right != null) _right.Copy(to, ref index);
        }
        public void Copy(Span<Integar> to, ref int index)
        {
            if (_left != null) _left.Copy(to, ref index);
            to[index++] = _key;
            if (_right != null) _right.Copy(to, ref index);
        }
        public void Copy(Span<KeyValuePair<Integar, T>> to, ref int index)
        {
            if (_left != null) _left.Copy(to, ref index);
            to[index++] = new(_key, _value);
            if (_right != null) _right.Copy(to, ref index);
        }

        public IEnumerator<Node> GetEnumerator()
        {
            if (_left != null) foreach (var item in _left) yield return item;
            yield return this;
            if (_right != null) foreach (var item in _right) yield return item;
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetValueEnumerator()
        {
            if (_left != null) foreach (var item in (IEnumerable<T>)_left) yield return item;
            yield return _value;
            if (_right != null) foreach (var item in (IEnumerable<T>)_right) yield return item;
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetValueEnumerator();

        public IEnumerator<Integar> GetKeyEnumerator()
        {
            if (_left != null) foreach (var item in (IEnumerable<Integar>)_left) yield return item;
            yield return _key;
            if (_right != null) foreach (var item in (IEnumerable<Integar>)_right) yield return item;
        }

        static bool _selector;

        public static ComparisonResult Compare(Node node1, Node node2) => node1._key == node2._key ? ComparisonResult.Equal : node1._key > node2._key ? ComparisonResult.Greater : ComparisonResult.Less;

        public static Node? Connect(Node? left, Node? right)
        {
            switch (left, right)
            {
            case (null, null):
                return null;
            case (null, _):
                return right;
            case (_, null):
                return left;
            case (_, _):
                switch (_selector = !_selector)
                {
                case true:
                    left!.Left = Connect(left!.Left, left!.Right);
                    left!.Right = right;
                    return left;
                case false:
                    right!.Right = Connect(right!.Left, right!.Right);
                    right!.Left = left;
                    return right;
                }
            }
        }
    }

    public class KeyCollection : ICollection<Integar>
    {
        readonly TreeDictionary<T> _target;

        public KeyCollection(TreeDictionary<T> target) => _target = target;

        public int Count => _target.Count;
        bool System.Collections.Generic.ICollection<Integar>.IsReadOnly => false;

        public void Clear() => _target.Clear();

        public bool Contains(Integar item) => _target.Contains(item);

        public void Copy(Span<Integar> to, ref int index)
        {
            if (_target.Root is Node root) root.Copy(to, ref index);
        }
        void System.Collections.Generic.ICollection<Integar>.CopyTo(Integar[] array, int arrayIndex) => Copy(array, ref arrayIndex);

        public IEnumerator<Integar> GetEnumerator() => _target.Root is IEnumerable<Node> nodes ? nodes.Select(x => x.Key).GetEnumerator() : EmptyEnumerator<Integar>.INSTANCE;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        bool ICollection<Integar>.TryAdd(Integar item) => throw new NotSupportedException();
        bool ICollection<Integar>.TryRemove(Integar item) => throw new NotSupportedException();
    }

    public class ValueCollection : ICollection<T>
    {
        readonly TreeDictionary<T> _target;

        public ValueCollection(TreeDictionary<T> target) => _target = target;

        public int Count => _target.Count;
        bool System.Collections.Generic.ICollection<T>.IsReadOnly => false;

        public void Clear() => _target.Clear();

        public bool Contains(T item) => _target.Contains(item);

        public void Copy(Span<T> to, ref int index)
        {
            if (_target.Root is Node root) root.Copy(to, ref index);
        }

        public IEnumerator<T> GetEnumerator() => _target.Root is IEnumerable<T> nodes ? nodes.GetEnumerator() : EmptyEnumerator<T>.INSTANCE;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        bool ICollection<T>.TryAdd(T item) => throw new NotSupportedException();
        bool ICollection<T>.TryRemove(T item) => throw new NotSupportedException();
    }
}
