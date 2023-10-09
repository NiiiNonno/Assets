using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Nonno.Assets.Collections;

public class LimitedMemory<T> : IReadOnlyCollection<T>
{
    readonly T[] _buf;
    readonly int _mask;
    // `_c`は世代と同時に配列中のカーソル位置を示す値。    
    volatile int _c;

    public int Capacity => _buf.Length;
	int System.Collections.Generic.IReadOnlyCollection<T>.Count => Capacity;

	// 容量は指数で入力。
	public LimitedMemory(Shift capacity)
    {
        _mask = (int)~(0xFFFF_FFFF << capacity.Exponent);
        _buf = new T[_mask + 1];
        _c = 0;
    }

    public void Push(T item)
    {
        retry:;

        var c = Interlocked.Increment(ref _c);
        var c_masked = c & _mask;

        _buf[c_masked] = item;

        // 世代が合っていたら、ついでに範囲内にいれる。
        // 合っていなかったら再挑戦。
        if (c != Interlocked.CompareExchange(ref _c, c_masked, c)) goto retry;
    }

    public void Clear()
    {
        Array.Clear(_buf);
    }

    public ReadOnlySpan<T> AsSpan() => _buf;

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_buf).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _buf.GetEnumerator();
}
