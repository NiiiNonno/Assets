using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Nonno.Assets.Collections.RangeExtentions;

namespace Nonno.Assets.Collections;
public interface IReadOnlyVector<T> : IEnumerable<T>
{
    int Length { get; }
    long LongLength => Length;
    T this[int index] { get; }
    T this[long index] => this[(int)index];
    T this[Index index] => this[index.GetOffset(Length)];
    T this[LongIndex index] => this[index.GetOffset(LongLength)];
    ReadOnlySpan<T> this[Range range] { get; }
    ReadOnlySpan<T> this[LongRange range] => this[(Range)range];

    void SetFlag(Flag flag) { }

    ReadOnlySpan<T> AsSpan() => this[..];

    new Enumerator GetEnumerator() => new(this, 0);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<T>
    {
        long _i;
        readonly IReadOnlyVector<T> _a;

        public readonly unsafe T Current => _a[_i];

        readonly object IEnumerator.Current => Current!;

        public Enumerator(IReadOnlyVector<T> a, long start = 0)
        {
            _a = a;
            _i = start;
        }

        public void Dispose() { }
        public bool MoveNext()
        {
            _i++;
            if (_i >= _a.Length) return false;
            return true;
        }
        public void Reset() => throw new NotSupportedException();
    }

    [Flags]
    public enum Flag
    {
        None = 0,
        Hold = 1,
    }
}

public interface IVector<T> : IReadOnlyVector<T>
{
    new int Length { get; set; }
    new long LongLength { get => Length; set => Length = (int)value; }
    new T this[int index] { get; set; }
    new T this[long index] {get => this[(int)index]; set => this[(int)index] = value; }
    new T this[Index index] { get => this[index.GetOffset(Length)]; set => this[index.GetOffset(Length)] = value; }
    new T this[LongIndex index] { get => this[index.GetOffset(LongLength)]; set => this[index.GetOffset(LongLength)] = value; }
    new Span<T> this[Range range] { get; }
    new Span<T> this[LongRange range] => this[(Range)range];
    new Span<T> AsSpan() => this[..];

    int IReadOnlyVector<T>.Length => Length;
    long IReadOnlyVector<T>.LongLength => LongLength;
    T IReadOnlyVector<T>.this[int index] => this[index];
    T IReadOnlyVector<T>.this[long index] => this[index];
    T IReadOnlyVector<T>.this[Index index] => this[index];
    T IReadOnlyVector<T>.this[LongIndex index] => this[index];
    ReadOnlySpan<T> IReadOnlyVector<T>.this[Range range] => this[range];
    ReadOnlySpan<T> IReadOnlyVector<T>.this[LongRange range] => this[range];
    ReadOnlySpan<T> IReadOnlyVector<T>.AsSpan() => this.AsSpan();
}

public readonly record struct ArrayVector<T>(List<T> InnerList) : IVector<T>
{
    public T this[int index] { get => InnerList[index]; set => InnerList[index] = value; }

    public Span<T> this[Range range] => Unsafe.As<ListDummy>(InnerList).Items[range];

    public int Length 
    { 
        get => InnerList.Count;
        set
        {
            if (value == InnerList.Count)
            {
                return;
            }
            else if (value < InnerList.Count)
            {
                InnerList.RemoveRange(value, InnerList.Count - value);
                return;
            }
            else
            {
                if (value > InnerList.Capacity) InnerList.Capacity = value;
                while (InnerList.Count < value) InnerList.Add(default!);
            }
        }
    }

    public Span<T> AsSpan() => Unsafe.As<ListDummy>(InnerList).Items.AsSpan();
    public int GetIndex(T of, Range range)
    {
        var s = range.Start.GetOffset(Length);
        var e = range.End.GetOffset(Length);
        return s <= e ? InnerList.IndexOf(of, s, e - s) : InnerList.LastIndexOf(of, s, s - e);
    }
    public int GetIndex(ReadOnlySpan<T> of, Range range)
    {
        var s = range.Start.GetOffset(Length);
        var e = range.End.GetOffset(Length);

        if (s <= e)
        {
            int c = 0;
            for (int i = s; i < e; i++)
                if (EqualityComparer<T>.Default.Equals(InnerList[i], of[c]))
                    if (++c == of.Length) return c - InnerList.Count + 1;
        }
        else
        {
            int c = of.Length - 1;
            for (int i = s - 1; i >= e; i--)
                if (EqualityComparer<T>.Default.Equals(InnerList[i], of[c]))
                    if (c-- == 0) return c;
        }
        return -1;
    }

    class ListDummy
    {
        internal T[] Items = null!;
    }
}
