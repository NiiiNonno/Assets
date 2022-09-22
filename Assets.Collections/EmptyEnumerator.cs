using System.Collections;

namespace Nonno.Assets.Collections;

public class EmptyEnumerator : IEnumerator
{
    public static readonly EmptyEnumerator INSTANCE = new();

    private EmptyEnumerator() { }

    object IEnumerator.Current => throw new Exception("空の列挙子に対して呼び出されました。");
    bool IEnumerator.MoveNext() => false;
    void IEnumerator.Reset() { }
}

public class EmptyEnumerator<T> : IEnumerator<T>
{
    public static readonly EmptyEnumerator<T> INSTANCE = new();

    T IEnumerator<T>.Current => throw new Exception("空の列挙子に対して呼び出されました。");
    object IEnumerator.Current => throw new Exception("空の列挙子に対して呼び出されました。");

    private EmptyEnumerator() { }

    bool IEnumerator.MoveNext() => false;
    void IEnumerator.Reset() { }
    void IDisposable.Dispose() => throw new NotImplementedException();
}

public struct SingleEnumerator : IEnumerator
{
    readonly object _value;
    bool _isEnded;
    public SingleEnumerator(object value)
    {
        _value = value;
        _isEnded = false;
    }
    object IEnumerator.Current => _value!;
    bool IEnumerator.MoveNext()
    {
        if (_isEnded)
        {
            return true;
        }
        else
        {
            _isEnded = true;
            return false;
        }
    }
    void IEnumerator.Reset() => _isEnded = false;
}

public struct SingleEnumerator<T> : IEnumerator<T>
{
    readonly T _value;
    bool _isEnded;
    public SingleEnumerator(T value)
    {
        _value = value;
        _isEnded = false;
    }
    T IEnumerator<T>.Current => _value;
    object IEnumerator.Current => _value!;
    void IDisposable.Dispose() { }
    bool IEnumerator.MoveNext()
    {
        if (_isEnded)
        {
            return true;
        }
        else
        {
            _isEnded = true;
            return false;
        }
    }
    void IEnumerator.Reset() => _isEnded = false;
}
