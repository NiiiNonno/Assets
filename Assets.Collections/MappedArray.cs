// 令和弐年大暑確認済。
using System.Diagnostics.CodeAnalysis;
using System.IO.MemoryMappedFiles;
using static Nonno.Assets.Utils;

namespace Nonno.Assets.Collections;

public class MappedArray<T> where T : unmanaged
{
    static readonly int SIZE_OF_T = SizeOf<T>();

    readonly MemoryMappedFile _data;

    public int Length { get; }
    public T this[int index]
    {
        get => TryGetValue(index, out T r) ? r : throw new IndexOutOfRangeException();
        set { if (!TrySetValue(index, ref value)) throw new IndexOutOfRangeException(); }
    }

    public MappedArray(string name, int length)
    {
        _data = MemoryMappedFile.CreateNew(name, length * SIZE_OF_T);

        Length = length;
    }

    public bool TryGetValue(int index, [NotNullWhen(true)] out T result)
    {
        using var accessor = _data.CreateViewAccessor();
        if (0 <= index && index < Length)
        {
            accessor.Read(index * SIZE_OF_T, out result);
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }

    public bool TrySetValue(int index, ref T value)
    {
        using var accessor = _data.CreateViewAccessor();
        if (0 <= index && index < Length)
        {
            accessor.Write(index * SIZE_OF_T, ref value);
            return true;
        }
        else
        {
            return false;
        }
    }
}
