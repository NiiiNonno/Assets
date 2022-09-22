// 令和弐年大暑確認済。
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Nonno.Assets.Collections;

/// <summary>
/// スタック優位の連結コレクションを表します。
/// </summary>
/// <typeparam name="T">
/// コレクションの型。
/// </typeparam>
public class StackCollection<T> : ICollection<T>
{
    protected Node? _root;

    /// <summary>
    /// 保持する要素の数。
    /// </summary>
    public int Count => _root != null ? _root.Count : 0;
    bool System.Collections.Generic.ICollection<T>.IsReadOnly => false;

    public bool Contains(T item)
    {
        var c = _root;
        while (c != null)
        {
            if (Equals(c.value, item)) return true;

            c = c.next;
        }
        return false;
    }

    public void Add(T item)
    {
        if (_root == null)
        {
            _root = new(item);
        }
        else
        {
            var root = _root;
            _root = new(item);
            _root.next = root;
        }
    }
    bool ICollection<T>.TryAdd(T item)
    {
        Add(item);
        return true;
    }

    public void Remove(T item)
    {
        if (_root == null) Throw();

        if (Equals(_root.value, item))
        {
            _root = _root.next;
        }
        else
        {
            var c = _root;
            while (c != null)
            {
                if (c.next == null) Throw();

                if (Equals(c.next.value, item))
                {
                    c.next = c.next.next;
                    return;
                }

                c = c.next;
            }
        }

        Throw();

        [DoesNotReturn] static void Throw() => throw new Exception("消すべき要素がありません。");
    }
    /// <summary>
    /// 指定の要素を削除します。
    /// <para>
    /// 最後に追加した要素を削除しようとする方が高速です。
    /// </para>
    /// </summary>
    /// <param name="item">
    /// 指定する要素。
    /// </param>
    /// <returns>
    /// 削除に成功したかを表す真偽値。
    /// <para>
    /// 削除に成功した場合は`true`、そうで無い場合は`false`。
    /// </para>
    /// </returns>
    public bool TryRemove(T item)
    {
        if (_root == null) return false;

        if (Equals(_root.value, item))
        {
            _root = _root.next;
        }
        else
        {
            var c = _root;
            while (c != null)
            {
                if (c.next == null) return false;

                if (Equals(c.next.value, item))
                {
                    c.next = c.next.next;
                    return true;
                }

                c = c.next;
            }
        }

        return false;
    }

    /// <summary>
    /// 全要素を削除します。
    /// </summary>
    public void Clear() => _root = null;

    public IEnumerator<T> GetEnumerator() => new Enumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Copy(Span<T> to, ref int index)
    {
        var c = _root;
        for (; index < to.Length; index++)
        {
            if (c == null) return;
            to[index] = c.value;
            c = c.next;
        }
    }

    protected sealed class Node
    {
        public readonly T value;
        public Node? next;

        public int Count => next != null ? next.Count + 1 : 0;
        public Node Trailing => next != null ? next.Trailing : this;

        public Node(T value) => this.value = value;
    }

    /// <summary>
    /// <see cref="StackCollection{T}"/>の列挙子を表します。
    /// </summary>
    public sealed class Enumerator : IEnumerator<T>
    {
        Node? _current;

        public T Current => _current!.value;
        object IEnumerator.Current => _current!.value!;

        private Enumerator(Node? node) => _current = node;
        /// <summary>
        /// 列挙子を初期化します。
        /// </summary>
        /// <param name="target">
        /// 列挙子が指し示す対象のオブジェクト。
        /// </param>
        public Enumerator(StackCollection<T> target) : this(target._root) { }

        public bool MoveNext() => _current != null && (_current = _current.next) != null;

        void IDisposable.Dispose() { }

        void IEnumerator.Reset() => throw new NotSupportedException();
    }
}
