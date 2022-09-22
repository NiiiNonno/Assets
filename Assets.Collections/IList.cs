using System.Diagnostics.CodeAnalysis;
using SysGC = System.Collections.Generic;

namespace Nonno.Assets.Collections;

public interface IList<T> : ICollection<T>, SysGC.IList<T>
{
    T SysGC::IList<T>.this[int index]
    {
        get => TryGetValue(index, out T? r) ? r : throw new ArgumentException("指定された索引の位置が範囲外であるためか、値を取得できません。", nameof(index));
        set { if (!TrySetValue(index, value)) throw new ArgumentException("指定された索引の位置が範囲外であるためか、値を設定できません。"); }
    }
    int GetIndex(T of);
    int SysGC::IList<T>.IndexOf(T item) => GetIndex(of: item);
    T Remove(int @at);
    void SysGC::IList<T>.RemoveAt(int index) => _ = Remove(at: index);
    bool TryGetValue(int index, [MaybeNullWhen(false)] out T value);
    bool TrySetValue(int index, T value);
}

public interface IReadOnlyList<T> : IReadOnlyCollection<T>, SysGC::IReadOnlyList<T>
{
    T SysGC::IReadOnlyList<T>.this[int index] => TryGetValue(index, out T? r) ? r : throw new ArgumentException("指定された索引の位置が範囲外であるためか、値を取得できません。", nameof(index));
    bool TryGetValue(int index, [MaybeNullWhen(false)] out T value);
}
