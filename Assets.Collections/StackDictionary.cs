// 令和弐年大暑確認済。
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Nonno.Assets.Collections;

public class StackDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
{
    protected Node? _root;

    public KeyCollection Keys => new(this);
    ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
    public ValueCollection Values => new(this);
    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
    public int Count => _root != null ? _root.Count : 0;
    bool System.Collections.Generic.ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        var c = _root;
        while (c != null)
        {
            if (c.key.Equals(item.Key) && Equals(c.value, item.Value)) return true;

            c = c.next;
        }
        return false;
    }
    public bool ContainsKey(TKey key)
    {
        var c = _root;
        while (c != null)
        {
            if (c.key.Equals(key)) return true;

            c = c.next;
        }
        return false;
    }
    public bool ContainsValue(TValue value)
    {
        var c = _root;
        while (c != null)
        {
            if (Equals(c.value, value)) return true;

            c = c.next;
        }
        return false;
    }

    public void Add(TKey key, TValue value)
    {
        if (_root == null)
        {
            _root = new(key, value);
        }
        else
        {
            var root = _root;
            _root = new(key, value);
            _root.next = root;
        }
    }
    bool IDictionary<TKey, TValue>.TryAdd(TKey key, TValue value)
    {
        Add(key, value);
        return true;
    }
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    bool ICollection<KeyValuePair<TKey, TValue>>.TryAdd(KeyValuePair<TKey, TValue> item)
    {
        Add(item);
        return true;
    }

    public TValue Remove(TKey key) => TryRemove(key, out var r) ? r : throw new Exception("要素の削除に失敗しました。");

    public bool TryRemove(TKey key)
    {
        if (_root == null) return false;

        if (_root.key.Equals(key))
        {
            _root = _root.next;
        }
        else
        {
            var c = _root;
            while (c != null)
            {
                if (c.next == null) return false;

                if (c.next.key.Equals(key))
                {
                    c.next = c.next.next;
                    return true;
                }

                c = c.next;
            }
        }

        return false;
    }
    public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        value = default;

        if (_root == null) return false;

        if (_root.key.Equals(key))
        {
            _root = _root.next;
        }
        else
        {
            var c = _root;
            while (c != null)
            {
                if (c.next == null) return false;

                if (c.next.key.Equals(key))
                {
                    value = c.next.value;
                    c.next = c.next.next;
                    return true;
                }

                c = c.next;
            }
        }

        return false;
    }

    public void Remove(KeyValuePair<TKey, TValue> item)
    {
        if (!TryRemove(item)) throw new Exception("消すべき要素がありません。");
    }
    public bool TryRemove(KeyValuePair<TKey, TValue> item)
    {
        if (_root == null) return false;

        if (_root.key.Equals(item.Key) && Equals(_root.value, item.Value))
        {
            _root = _root.next;
        }
        else
        {
            var c = _root;
            while (c != null)
            {
                if (c.next == null) return false;

                if (c.next.key.Equals(item.Key) && Equals(_root.value, item.Value))
                {
                    c.next = c.next.next;
                    return true;
                }

                c = c.next;
            }
        }

        return false;
    }

    public int Erase(TValue value)
    {
        if (_root == null) return 0;

        int count = 0;

        if (Equals(_root.value, value))
        {
            _root = _root.next;
        }
        else
        {
            var c = _root;
            while (c != null)
            {
                if (c.next == null) return count;

                if (Equals(c.next.value, value))
                {
                    c.next = c.next.next;
                    count++;
                }

                c = c.next;
            }
        }

        return count;
    }
    public int Erase(TValue value, Action<TKey, TValue> action)
    {
        if (_root == null) return 0;

        int count = 0;

        if (Equals(_root.value, value))
        {
            _root = _root.next;
        }
        else
        {
            var c = _root;
            while (c != null)
            {
                if (c.next == null) return count;

                if (Equals(c.next.value, value))
                {
                    action(c.next.key, c.next.value);
                    c.next = c.next.next;
                    count++;
                }

                c = c.next;
            }
        }

        return count;
    }

    public void Clear() => _root = null;

    public void Copy(Span<KeyValuePair<TKey, TValue>> to, ref int index)
    {
        var c = _root;
        while (c != null)
        {
            to[index++] = c.ToPair();

            c = c.next;
        }
    }
    public void Copy(Span<TKey> to, ref int index)
    {
        var c = _root;
        while (c != null)
        {
            to[index++] = c.key;

            c = c.next;
        }
    }
    public void Copy(Span<TValue> to, ref int index)
    {
        var c = _root;
        while (c != null)
        {
            to[index++] = c.value;

            c = c.next;
        }
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var c = _root;
        while (c != null)
        {
            if (c.key.Equals(key))
            {
                value = c.value;
                return true;
            }

            c = c.next;
        }
        value = default;
        return false;
    }

    public bool TrySetValue(TKey key, TValue value)
    {
        var c = _root;
        while (c != null)
        {
            if (c.key.Equals(key))
            {
                c.value = value;
                return true;
            }

            c = c.next;
        }
        return false;
    }

    public TKey GetFirstKey(TValue value)
    {
        var c = _root;
        while (c != null)
        {
            if (Equals(c.value, value))
            {
                return c.key;
            }

            c = c.next;
        }
        throw new ArgumentException("値が見つかりません。", nameof(value));
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        var c = _root;
        while (c != null)
        {
            yield return c.ToPair();

            c = c.next;
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected sealed class Node
    {
        public readonly TKey key;
        public TValue value;
        public Node? next;

        public int Count => next != null ? next.Count + 1 : 0;
        public Node Trailing => next != null ? next.Trailing : this;

        public Node(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }

        public KeyValuePair<TKey, TValue> ToPair() => new(key, value);
    }

    public class KeyCollection : ICollection<TKey>
    {
        readonly StackDictionary<TKey, TValue> _target;

        public int Count => _target.Count;

        public KeyCollection(StackDictionary<TKey, TValue> target) => _target = target;

        public bool Contains(TKey item) => _target.ContainsKey(item);

        public void Copy(Span<TKey> to, ref int index) => _target.Copy(to, ref index);

        public IEnumerator<TKey> GetEnumerator()
        {
            var c = _target._root;
            while (c != null)
            {
                yield return c.key;

                c = c.next;
            }
        }

        bool ICollection<TKey>.TryAdd(TKey item) => false;
        bool ICollection<TKey>.TryRemove(TKey item) => false;
        void System.Collections.Generic.ICollection<TKey>.Clear() => throw new NotSupportedException();
    }

    public class ValueCollection : ICollection<TValue>
    {
        readonly StackDictionary<TKey, TValue> _target;

        public int Count => _target.Count;

        public ValueCollection(StackDictionary<TKey, TValue> target) => _target = target;

        public bool Contains(TValue item) => _target.ContainsValue(item);

        public void Copy(TValue[] to, ref int index) => _target.Copy(to, ref index);
        void System.Collections.Generic.ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex) => _target.Copy(to: array, ref arrayIndex);

        public void Copy(Span<TValue> to, ref int index) => _target.Copy(to, ref index);

        public IEnumerator<TValue> GetEnumerator()
        {
            var c = _target._root;
            while (c != null)
            {
                yield return c.value;

                c = c.next;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        bool ICollection<TValue>.TryAdd(TValue item) => false;
        bool ICollection<TValue>.TryRemove(TValue item) => false;
        void System.Collections.Generic.ICollection<TValue>.Clear() => throw new NotSupportedException();
    }
}
