using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets;
public struct AsyncEnumerator<T, TEnumerator> : IEnumerator<T>, IAsyncEnumerator<T> where TEnumerator : struct
{
    TEnumerator _s;

    private AsyncEnumerator(TEnumerator s)
    {
        if (s is not IEnumerator<T> and not IAsyncEnumerator<T>) throw new ArgumentException();
        _s = s;
    }

    public T Current
    {
        get
        {
            switch (_s)
            {
            case IEnumerator<T> s:
                var r = s.Current;
                _s = (TEnumerator)s;
                return r;
            case IAsyncEnumerator<T> a:
                r = a.Current;
                _s = (TEnumerator)a;
                return r;
            }
            throw new Exception();
        }
    }
    object IEnumerator.Current => Current!;

    public void Dispose()
    {
        switch (_s)
        {
        case IEnumerator<T> s:
            s.Dispose();
            _s = (TEnumerator)s;
            return;
        case IAsyncEnumerator<T> a:
            var r = a.DisposeAsync();
            _s = (TEnumerator)a;
            if (r.IsCompleted) return;
            else r.AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
            return;
        }
        throw new Exception();
    }
    public ValueTask DisposeAsync()
    {
        switch (_s)
        {
        case IEnumerator<T> s:
            s.Dispose();
            _s = (TEnumerator)s;
            return new(Task.CompletedTask);
        case IAsyncEnumerator<T> a:
            var r = a.DisposeAsync();
            _s = (TEnumerator)a;
            return r;
        }
        throw new Exception();
    }
    public bool MoveNext()
    {
        switch (_s)
        {
        case IEnumerator<T> s:
            var r = s.MoveNext();
            _s = (TEnumerator)s;
            return r;
        case IAsyncEnumerator<T> a:
            var r_ = a.MoveNextAsync();
            _s = (TEnumerator)a;
            if (r_.IsCompleted) return r_.Result;
            else r = r_.AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
            return r;
        }
        throw new Exception();
    }
    public ValueTask<bool> MoveNextAsync()
    {
        switch (_s)
        {
        case IEnumerator<T> s:
            var r_ = s.MoveNext();
            _s = (TEnumerator)s;
            return new(r_);
        case IAsyncEnumerator<T> a:
            var r = a.MoveNextAsync();
            _s = (TEnumerator)a;
            return r;
        }
        throw new Exception();
    }
    public void Reset()
    {
        switch (_s)
        {
        case IEnumerator<T> s:
            s.Reset();
            _s = (TEnumerator)s;
            return;
        case IAsyncEnumerator<T>:
            throw new NotSupportedException();
        }
        throw new Exception();
    }

    public static implicit operator AsyncEnumerator<T, TEnumerator>(TEnumerator e) => new(e);
}
