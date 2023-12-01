using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Nonno.Assets.Collections;
public struct SingleOrList<T> : IList<T>
{
    static readonly List<T> SINGLE_TOKEN = [default!];

    T? _value;
    List<T>? _listOrToken;

    [MemberNotNullWhen(false, nameof(_listOrToken))]
    public readonly bool IsEmpty => _value is null;
    [MemberNotNullWhen(true, nameof(_value))]
    public readonly bool IsSingle => ReferenceEquals(_listOrToken, SINGLE_TOKEN);
    public readonly int Count => _listOrToken is { } v ? v.Count : 0;
    public T this[int i]
    {
        readonly get => TryGetValue(i, out var r) ? r : throw new IndexOutOfRangeException();
        set { if (!TrySetValue(i, value)) throw new IndexOutOfRangeException(); }
    }

    public void Clear()
    {
        _listOrToken = null;
    }
    public readonly bool Contains(T element)
    {
        if (IsEmpty) return false;
        else if (IsSingle) return EqualityComparer<T?>.Default.Equals(_value, element);
        else return _listOrToken.Contains(element);
    }
    public readonly void Copy(Span<T> to, ref int index)
    {
        if (IsEmpty) return;
        else if (IsSingle) to[index++] = _value;
        else _listOrToken.Copy(to, ref index);
    }

    public readonly Enumerator GetEnumerator() => new(this);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    public readonly int GetIndex(T of)
    {
        if (IsEmpty) return -1;
        else if (IsSingle) return EqualityComparer<T>.Default.Equals(_value, of) ? 0 : -1;
        else return _listOrToken.IndexOf(of);
    }
    public void Insert(int index, T item)
    {
        if (IsEmpty)
        {
            if (index != 0) throw new IndexOutOfRangeException();
            _value = item;
            _listOrToken = SINGLE_TOKEN;
            return;
        }

        if (IsSingle) _listOrToken = [_value];

        _listOrToken.Insert(index, item);
    }
    public T Remove(int at)
    {
        if (IsEmpty)
        {
            throw new IndexOutOfRangeException();
        }
        else if (IsSingle)
        {
            if (at != 0) throw new IndexOutOfRangeException();
            _listOrToken = null;
            return _value;
        }
        else
        {
            var r = _listOrToken[at];
            _listOrToken.RemoveAt(at);
            return r;
        }
    }
    public void Add(T item)
    {
        if (IsEmpty)
        {
            _value = item;
            _listOrToken = SINGLE_TOKEN;
        }
        else if (IsSingle)
        {
            _listOrToken = [_value, item];
        }
        else
        {
            _listOrToken.Add(item);
        }
    }
    public bool TryAdd(T item)
    {
        Add(item);
        return true;
    }
    public readonly bool TryGetValue(int index, [MaybeNullWhen(false)] out T value)
    {
        value = _value;
        if (IsEmpty)
        {
            return false;
        }
        else if (IsSingle)
        {
            return index == 0;
        }
        else
        {
            if (index >= _listOrToken.Count) return false;

            value = _listOrToken[index];
            return true;
        }
    }
    public bool TryRemove(T item)
    {
        if (IsEmpty)
        {
            return false;
        }
        else if (IsSingle)
        {
            if (!EqualityComparer<T>.Default.Equals(item, _value)) return false;

            _listOrToken = null;
            return true;
        }
        else
        {
            return _listOrToken.Remove(item);
        }
    }
    public bool TrySetValue(int index, T value)
    {
        if (IsEmpty)
        {
            return false;
        }
        else if (IsSingle)
        {
            if (index != 0) return false;

            _value = value;
            return true;
        }
        else
        {
            if (index <  _listOrToken.Count) return false;
            
            _listOrToken[index] = value;
            return true;
        }
    }

    public struct Enumerator(SingleOrList<T> @base) : IEnumerator<T>
    {
        int i = -1;

        public readonly T Current => @base.IsSingle ? @base._value : @base._listOrToken![i];
        object IEnumerator.Current => Current!;

        public readonly void Dispose() { }
        public bool MoveNext() => ++i < @base.Count;
        public void Reset() => i = -1;
    }
}
