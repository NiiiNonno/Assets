using System.Collections;

namespace Nonno.Assets.Collections
{
    public struct ArraySegmentReverseEnumerator<T> : IEnumerator<T>
    {
        readonly T[] _arr;
        readonly int _min;
        int _c;

        public ArraySegmentReverseEnumerator(ArraySegment<T> @base)
        {
            _arr = @base.Array;
            _min = @base.Offset;
            _c = _min + @base.Count;
        }

        public T Current => _arr[_c];
        object IEnumerator.Current => Current!;

        public void Dispose() { }
        public bool MoveNext() => (_c--) >= _min;
        public void Reset() => throw new NotSupportedException();
    }
}
