// 令和弐年大暑確認済。
using System.Collections;
using System.Runtime.CompilerServices;

namespace Nonno.Assets.Collections;

[Serializable]
public class Roll<T> : IEnumerable<T>
{
    readonly T[] _data;
    readonly int _mask;
    int _head;

    public int Perimeter => _data.Length;
    public T this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _data[(_head + i) & _mask];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _data[(_head + i) & _mask] = value;
    }
    public T this[Index i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[i.GetOffset(_data.Length)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Roll(Shift perimeter, T defaultValue)
    {
        _data = new T[perimeter];
        _mask = _data.Length - 1;
        _head = 0;

        Fill(defaultValue);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Obsolete("この初期化は`null`を許容するため、危険です。")]
    public Roll(Shift perimeter)
    {
        _data = new T[perimeter];
        _mask = _data.Length - 1;
        _head = 0;
    }

    /// <summary>
    /// 値を納めます。
    /// <para>
    /// 納めた値は列挙子または索引の最初によって示される位置に格納され、列挙子または索引の最後によって示された値は破棄されます。
    /// </para>
    /// </summary>
    /// <param name="item">
    /// 納める値。
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(T item)
    {
        _head = (_head - 1) & _mask;
        _data[_head] = item;
    }

    public void Fill(T value) => Array.Fill(_data, value);

    public void Copy(T[] to, ref int index)
    {
        int length = _data.Length - _head;
        Array.Copy(_data, _head, to, index, length);

        Array.Copy(_data, 0, to, index + length, _head);
        index += _data.Length;
    }

    public bool Replace(T value, T to)
    {
        for (int i = 0; i < _data.Length; i++)
        {
            if (Equals(_data[i], value))
            {
                _data[i] = to;
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<T> GetEnumerator()
    {
        for (int i = _head; i < _data.Length; i++) yield return _data[i];
        for (int i = 0; i < _head; i++) yield return _data[i];
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
