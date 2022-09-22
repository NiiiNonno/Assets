using System.Collections;

namespace Nonno.Assets;

/*
 * 複数形。
 * IEnumerableとしても使ってもいいけれど、特殊処理をするのがセオリー。
 */

[Obsolete("インターフェースを実装するオブジェクトの乱作による混乱を避けるため。")]
public interface IPlural<out T> : IEnumerable<T>
{
    T TypicalObject { get; }
    int Number { get; }
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new PluralEnumerator<T>(this);
    IEnumerator IEnumerable.GetEnumerator() => new PluralEnumerator<T>(this);
}

public struct PluralEnumerator<T> : IEnumerator<T>
{
    readonly T _value;
    int _number;

    public T Current => _value;
    object IEnumerator.Current => _value!;

    [Obsolete("`IPlural<T>`の使用が非推奨であるため。")]
    public PluralEnumerator(IPlural<T> ts)
    {
        _value = ts.TypicalObject;
        _number = ts.Number;
    }
    public PluralEnumerator(Plural<T> ts)
    {
        _value = ts.TypicalObject;
        _number = ts.Count;
    }

    public bool MoveNext() => --_number >= 0;

    void IDisposable.Dispose() { }

    void IEnumerator.Reset() => throw new NotSupportedException();
}

#pragma warning disable CS0618 // 型またはメンバーが旧型式です
/// <summary>
/// 指定された型の複数形を表します。
/// </summary>
/// <typeparam name="T">
/// 指定する型。
/// </typeparam>
public readonly struct Plural<T> : IPlural<T>, IEquatable<Plural<T>>
{
    public T TypicalObject { get; }
    public int Count { get; }
    [Obsolete("`Count`と重複しています。")]
    public int Number => Count;

    public Plural(T typicalObject, int count)
    {
        TypicalObject = typicalObject;
        Count = count;

        if (count <= 0) throw new ArgumentException("一個未満の複数形は作れません。", nameof(count));
    }

    public override bool Equals(object? obj) => obj is Plural<T> plural && Equals(plural);
    public bool Equals(Plural<T> other) => EqualityComparer<T>.Default.Equals(TypicalObject, other.TypicalObject) && Count == other.Count;

    public override int GetHashCode() => HashCode.Combine(TypicalObject, Count);

    public static bool TryAdd<Ts1, Ts2>(Ts1 left, Ts2 right, out Plural<T> result) where Ts1 : IPlural<T> where Ts2 : IPlural<T>
    {
        if (Equals(left.TypicalObject, right.TypicalObject))
        {
            result = new(left.TypicalObject, left.Number + right.Number);
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }

    public static implicit operator Plural<T>(T singular) => new(singular, 1);

    public static bool operator ==(Plural<T> left, Plural<T> right) => left.Equals(right);
    public static bool operator !=(Plural<T> left, Plural<T> right) => !(left == right);
}
#pragma warning restore CS0618 // 型またはメンバーが旧型式です

public readonly struct PluralEnumeration<T> : IEnumerable<T>
{
    readonly IEnumerable<Plural<T>> _enumerable;

    public PluralEnumeration(IEnumerable<Plural<T>> enumerable) => _enumerable = enumerable;

    public IEnumerator<Plural<T>> GetEnumerator() => _enumerable.GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        foreach (var items in _enumerable) for (int i = 0; i < items.Count; i++) yield return items.TypicalObject;
    }
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_enumerable).GetEnumerator();
}
