using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets;
public readonly struct Disposables : IDisposable
{
    readonly List<IDisposable>? _list;
    readonly int _token;

    public bool IsValid => _list is null || _list.Count == _token;

    public Disposables()
    {
        _list = null;
        _token = 0;
    }
    private Disposables(List<IDisposable> list)
    {
        _list = list;
        _token = list.Count;
    }

    // PNG.Instantiate(INote note)での操作において、追加された順に破棄されることが保証されている必要がある。
    /// <summary>
    /// 要破棄物を追加します。
    /// <para>
    /// 追加された要破棄物は末端に追加され、破棄の際に元の要破棄物より後に破棄されます。
    /// </para>
    /// </summary>
    /// <param name="left">
    /// 元の要破棄物。
    /// <para>
    /// 演算によって元の要破棄物は使用できなくなります。
    /// </para>
    /// </param>
    /// <param name="right">
    /// 追加する要破棄物。
    /// </param>
    /// <returns>
    /// 得られた要破棄物。
    /// </returns>
    public static Disposables operator +(Disposables left, IDisposable right)
    {
        if (left._list is null) 
        { 
            return new(new List<IDisposable>() { right });
        }
        else
        {
            left._list.Add(right);
            return left;
        }
    }
    // 長さをトークンとして用いるためには、長さが単調増加しなければならない。
    //public static Disposables operator -(Disposables left, IDisposable right) => throw new Exception();

    public void Dispose()
    {
        if (_list is not null) 
        {
            if (_list.Count != _token) throw new InvalidOperationException("演算に使用され無効となった廃棄物を破棄することができません。");
            foreach (var item in _list) item.Dispose(); 
        }
    }
}

public readonly struct AsyncDisposables : IAsyncDisposable
{
    readonly List<IAsyncDisposable>? _list;
    readonly int _token;

    public bool IsValid => _list is null || _list.Count == _token;

    public AsyncDisposables()
    {
        _list = null;
        _token = 0;
    }
    private AsyncDisposables(List<IAsyncDisposable> list)
    {
        _list = list;
        _token = list.Count;
    }

    public static AsyncDisposables operator +(AsyncDisposables left, IAsyncDisposable right)
    {
        if (left._list is null)
        {
            return new(new List<IAsyncDisposable>() { right });
        }
        else
        {
            left._list.Add(right);
            return left;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_list is not null)
        {
            if (_list.Count != _token) throw new InvalidOperationException("演算に使用され無効となった廃棄物を破棄することができません。");
            foreach (var item in _list) await item.DisposeAsync();
        }
    }
}
