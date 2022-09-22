using System.Diagnostics.CodeAnalysis;

namespace Nonno.Assets.Collections;

/// <summary>
/// 循環バッファ。
/// </summary>
/// <typeparam name="T">要素の型</typeparam>
public class CircularList<T> : IEnumerable<T>, IBoundList<T>
{
    T[] _data;
    int _top, _bottom;
    int _mask;

    public event IBoundList<T>.ItemRemovedEventHandler? ItemRemoved;
    public event IBoundList<T>.ItemAddedEventHandler? ItemAdded;
    public event IBoundList<T>.ItemReplacedEventHandler? ItemReplaced;
    public event IBoundCollection<T>.ClearedEventHandler? Cleared;

    public CircularList() : this(256) { }
    /// <summary>
    /// 初期最大容量を指定して初期化。
    /// </summary>
    /// <param name="capacity">初期最大容量</param>
    public CircularList(int capacity)
    {
        capacity = Pow2((uint)capacity);
        this._data = new T[capacity];
        this._top = this._bottom = 0;
        this._mask = capacity - 1;
    }

    /// <summary>
    /// 格納されている要素数。
    /// </summary>
    public int Count
    {
        get
        {
            int count = this._bottom - this._top;
            if (count < 0) count += this._data.Length;
            return count;
        }
    }
    /// <summary>
    /// i 番目の要素を読み書き。
    /// </summary>
    /// <param name="i">読み書き位置</param>
    /// <returns>読み出した要素</returns>
    public T this[int i]
    {
        get => _data[(i + _top) & _mask];
        set => _data[(i + _top) & _mask] = value;
    }

    /// <summary>
    /// 配列を確保しなおす。
    /// </summary>
    /// <remarks>
    /// 配列長は2倍ずつ拡張していきます。
    /// </remarks>
    void Extend()
    {
        var data = new T[this._data.Length * 2];
        int i = 0;
        foreach (T elem in this)
        {
            data[i] = elem;
            ++i;
        }
        this._top = 0;
        this._bottom = this.Count;
        this._data = data;
        this._mask = data.Length - 1;
    }

    /// <summary>
    /// i 番目の位置に新しい要素を追加。
    /// </summary>
    /// <param name="i">追加位置</param>
    /// <param name="elem">追加する要素</param>
    public void Insert(int i, T elem)
    {
        if (this.Count >= this._data.Length - 1)
            this.Extend();

        if (i < this.Count / 2)
        {
            for (int n = 0; n <= i; ++n)
            {
                this[n - 1] = this[n];
            }
            this._top = (this._top - 1) & this._mask;
            this[i] = elem;
        }
        else
        {
            for (int n = this.Count; n > i; --n)
            {
                this[n] = this[n - 1];
            }
            this[i] = elem;
            this._bottom = (this._bottom + 1) & this._mask;
        }
    }
    /// <summary>
    /// 先頭に新しい要素を追加。
    /// </summary>
    /// <param name="elem">追加する要素</param>
    public void InsertFirst(T elem)
    {
        if (this.Count >= this._data.Length - 1)
            this.Extend();

        this._top = (this._top - 1) & this._mask;
        this._data[this._top] = elem;
    }
    /// <summary>
    /// 末尾に新しい要素を追加。
    /// </summary>
    /// <param name="elem">追加する要素</param>
    public void InsertLast(T elem)
    {
        if (this.Count >= this._data.Length - 1)
            this.Extend();

        this._data[this._bottom] = elem;
        this._bottom = (this._bottom + 1) & this._mask;
    }

    /// <summary>
    /// i 番目の要素を削除。
    /// </summary>
    /// <param name="i">削除位置</param>
    public T Remove(int i)
    {
        var r = this[i];
        for (int n = i; n < this.Count - 1; ++n)
        {
            this[n] = this[n + 1];
        }
        this._bottom = (this._bottom - 1) & this._mask;
        return r;
    }
    /// <summary>
    /// 先頭の要素を削除。
    /// </summary>
    public void RemoveFirst()
    {
        _top = (_top + 1) & _mask;
    }
    /// <summary>
    /// 末尾の要素を削除。
    /// </summary>
    public void RemoveLast()
    {
        this._bottom = (this._bottom - 1) & this._mask;
    }

    public bool TryAdd(T item)
    {
        InsertFirst(item);
        return true;
    }

    public bool TryRemove(T item)
    {
        throw new NotImplementedException();
    }

    public void Clear() => _top = _bottom;

    public bool Contains(T item)
    {
        if (item == null)
        {
            foreach (var aEItem in this)
            {
                if (aEItem == null) return true;
            }
            return false;
        }
        else
        {
            foreach (var aEItem in this)
            {
                if (aEItem != null && aEItem.Equals(item)) return true;
            }
            return false;
        }
    }

    public int GetIndex(T of)
    {
        throw new NotImplementedException();
    }

    public bool TryGetValue(int index, [MaybeNullWhen(false)] out T value)
    {
        value = _data[(index + _top) & _mask];
        return value != null;
    }

    public bool TrySetValue(int index, T value)
    {
        _data[(index + _top) & _mask] = value;
        return true;
    }

    static int Pow2(uint n)
    {
        --n;
        int p = 0;
        for (; n != 0; n >>= 1) p = (p << 1) + 1;
        return p + 1;
    }

    public void Copy(Span<T> to, ref int index)
    {
        if (this._top <= this._bottom)
        {
            for (int i = this._top; i < this._bottom; ++i)
                to[index++] = this._data[i];
        }
        else
        {
            for (int i = this._top; i < this._data.Length; ++i)
                to[index++] = this._data[i];
            for (int i = 0; i < this._bottom; ++i)
                to[index++] = this._data[i];
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        if (this._top <= this._bottom)
        {
            for (int i = this._top; i < this._bottom; ++i)
                yield return this._data[i];
        }
        else
        {
            for (int i = this._top; i < this._data.Length; ++i)
                yield return this._data[i];
            for (int i = 0; i < this._bottom; ++i)
                yield return this._data[i];
        }
    }
}
