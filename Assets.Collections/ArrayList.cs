using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Nonno.Assets.Scrolls;

namespace Nonno.Assets.Collections;

public class ArrayList<T> : IList<T>// where T : notnull
{
    T[] _array;

    public int Count { get; private set; }
    public int Capacity
    {
        get => _array.Length;
        set
        {
            if (value < Count) throw new ArgumentException("容量は要素数よりも大きい必要があります。", nameof(value));
            var neo = new T[value];
            Array.Copy(_array, neo, Count);
            _array = neo;
        }
    }
    public T this[int index]
    {
        get => _array[index];
        set => _array[index] = value;
    }
    public ArraySegment<T> this[Range range]
    {
        get
        {
            var (o, l) = range.GetOffsetAndLength(Count);
            return new(_array, o, l);
        }
    }

    public ArrayList()
    {
        _array = Array.Empty<T>();
    }
    public ArrayList(int capacity)
    {
        _array = new T[capacity];
    }
    public ArrayList(params T[] itemParams)
    {
        _array = itemParams;
        Count = itemParams.Length;
    }

    public void Copy(Span<T> to, ref int index)
    {
        foreach (var item in this)
        {
            to[index++] = item;
        }
    }

    public int GetIndex(T of)
    {
        for (int i = 0; i < Count; i++)
        {
            if (Equals(_array[i], of)) return i;
        }
        return -1;
    }

    public void Add(T item)
    {
        if (_array.Length == Count) Extend();
        _array[Count] = item;
        Count++;
    }
    bool ICollection<T>.TryAdd(T item)
    {
        if (_array.Length == Count) Extend();
        _array[Count] = item;
        Count++;
        return true;
    }
    public void Add(IEnumerable<T> range)
    {
        foreach (var item in range)
        {
            if (_array.Length == Count) Extend();
            _array[Count++] = item;
        }
    }

    public void Remove(T item)
    {
        for (int i = 0; i < Count; i++)
        {
            if (Equals(_array[i], item))
            {
                Remove(at: i);
                return;
            }
        }
    }
    public bool TryRemove(T item)
    {
        for (int i = 0; i < Count; i++)
        {
            if (Equals(_array[i], item))
            {
                Remove(at: i);
                return true;
            }
        }
        return false;
    }
    public T Remove(int at)
    {
        var r = _array[at];
        var startI = at + 1;
        Array.Copy(_array, startI, _array, at, Count - startI);
        Count--;
        return r;
    }
    public void Remove(Range range)
    {
        var startI = range.End.GetOffset(Count);
        var at = range.End.GetOffset(Count);
        Array.Copy(_array, startI, _array, at, Count - startI);
        Count -= startI - at;
    }

    public void Insert(int index, T item)
    {
        if (_array.Length == Count) Extend();
        Array.Copy(_array, index, _array, index + 1, Count - index);
        Count++;
        _array[index] = item;
    }

    public void Clear()
    {
        Count = 0;
    }

    public bool Contains(T item)
    {
        for (int i = 0; i < Count; i++)
        {
            if (Equals(_array[i], item))
            {
                return true;
            }
        }
        return false;
    }

    public bool TryGetValue(int index, [MaybeNullWhen(false)] out T value)
    {
        if (index < 0 && Count <= index)
        {
            value = default; return false;
        }
        else
        {
            value = _array[index];
            return true;
        }
    }

    public bool TrySetValue(int index, T value)
    {
        if (index < 0 && Count <= index)
        {
            return false;
        }
        else
        {
            _array[index] = value;
            return true;
        }
    }

    public ref T GetReference(int index) => ref _array[index];

    public Memory<T> AsMemory() => _array.AsMemory(0, Count);
    public ReadOnlySpan<T> AsSpan() => _array.AsSpan(0, Count);
    public Span<T> UnsafeAsSpan() => _array.AsSpan(0, Count);

    public ArraySegment<T>.Enumerator GetEnumerator() => this[..].GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    void Extend()
    {
        var neo = new T[_array.Length == 0 ? 1 : _array.Length * 2];
        Array.Copy(_array, neo, _array.Length);
        _array = neo;
    }

    public readonly struct Reverse : IList<T>
    {
        readonly ArrayList<T> _base;

        public Reverse(ArrayList<T> @base) => _base = @base;

        public int Count => (_base).Count;
        private int Max => _base.Count - 1;

        public void Clear() => (_base).Clear();
        public bool Contains(T item) => (_base).Contains(item);
        public void Copy(Span<T> to, ref int index) { (_base).Copy(to, ref index); to.Reverse(); }
        public IEnumerator<T> GetEnumerator() => (_base[..]).GetReverseEnumerator();
        public int GetIndex(T of) => Max - (_base).GetIndex(of);
        public void Insert(int index, T item) => (_base).Insert(Max - index, item);
        public T Remove(int at) => (_base).Remove(Max - at);
        public bool TryAdd(T item) => ((ICollection<T>)_base).TryAdd(item);
        public bool TryGetValue(int index, [MaybeNullWhen(false)] out T value) => (_base).TryGetValue(Max - index, out value);
        public bool TryRemove(T item) => (_base).TryRemove(item);
        public bool TrySetValue(int index, T value) => (_base).TrySetValue(Max - index, value);
    }
}

public static partial class NoteExtensions
{
    [IRMethod]
    public static void Insert<TScroll, T>(this TScroll @this, ArrayList<T> arrayList) where TScroll : IScroll where T : notnull
    {
        @this.Insert(arrayList.Count);
        foreach (var item in arrayList)
        {
            @this.Insert(item);
        }
    }
    [IRMethod]
    public static void Remove<TScroll, T>(this TScroll @this, out ArrayList<T> arrayList) where TScroll : IScroll where T : notnull
    {
        @this.Remove(out int count);
        arrayList = new(count + 1);
        for (int i = 0; i < count; i++)
        {
            @this.Remove(out T value);
            arrayList.Add(value);
        }
    }
}
