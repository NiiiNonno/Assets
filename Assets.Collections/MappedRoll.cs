using System.Collections;
using System.Runtime.CompilerServices;

namespace Nonno.Assets.Collections;

public class MappedRoll<T> : IEnumerable<T> where T : unmanaged
{
    readonly MappedArray<T> _data;
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
    public MappedRoll(string name, Shift perimeter)
    {
        _data = new(name, perimeter);
        _mask = _data.Length - 1;
        _head = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(T item)
    {
        _head = (_head - 1) & _mask;
        _data[_head] = item;
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
