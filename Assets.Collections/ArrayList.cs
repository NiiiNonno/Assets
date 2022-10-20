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

    public Memory<T> AsMemory() => _array.AsMemory(0, Count);
    public ReadOnlySpan<T> AsSpan() => _array.AsSpan(0, Count);

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return _array[i];
        }
    }

    void Extend()
    {
        var neo = new T[_array.Length == 0 ? 1 : _array.Length * 2];
        Array.Copy(_array, neo, _array.Length);
        _array = neo;
    }
}

public static partial class NoteExtensions
{
    [IRMethod]
    public static async Task Insert<T>(this IScroll @this, ArrayList<T> arrayList) where T : notnull
    {
        await @this.Insert(arrayList.Count);
        foreach (var item in arrayList)
        {
            await @this.Insert(item);
        }
    }
    [IRMethod]
    public static Task Remove<T>(this IScroll @this, out ArrayList<T> arrayList) where T : notnull
    {
        @this.Remove(out int count).Wait();
        arrayList = new(count + 1);
        return GetTask(arrayList);

        async Task GetTask(ArrayList<T> obj)
        {
            for (int i = 0; i < count; i++)
            {
                await @this.Remove(out T value);
                obj.Add(value);
            }
        }
    }
}
