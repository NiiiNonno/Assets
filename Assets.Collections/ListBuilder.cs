using System.Collections;
using System.Text;

namespace Nonno.Assets.Collections;

public readonly struct ListBuilder<T> : IEnumerable<T>
{
    readonly Node? _first;
    readonly Node? _last;

    public bool IsEmpty => _first == null;

    public ListBuilder(T item)
    {
        _first = _last = new Node(item);
    }
    ListBuilder(Node node1, Node node2)
    {
        _last = node2;
        _first = node1;
    }

    public IEnumerator<T> GetEnumerator()
    {
        var c = _first;
        while (c != null)
        {
            yield return c.Value;
            if (ReferenceEquals(c, _last)) yield break;
            c = c.Next;
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    class Node
    {
        public Node? Next { get; set; }
        public T Value { get; set; }

        public Node(T value)
        {
            Value = value;
        }
    }

    public static implicit operator ListBuilder<T>(T p) => new(p);
    public static ListBuilder<T> operator +(ListBuilder<T> left, ListBuilder<T> right)
    {
        switch (left.IsEmpty, right.IsEmpty)
        {
        case (true, true):
            return default;
        case (true, false):
            return right;
        case (false, true):
            return left;
        case (false, false):
            left._last!.Next = right._first;
            return new ListBuilder<T>(left._first!, right._last!);
        }
    }

    public override string ToString()
    {
        var r = new StringBuilder();
        foreach (var item in this) _ = r.Append(item);
        return r.ToString();
    }
}
