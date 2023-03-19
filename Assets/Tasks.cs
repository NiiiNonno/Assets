namespace Nonno.Assets;

public readonly struct Tasks
{
    readonly List<Task>? _list;
    readonly int _token;

    public bool IsValid => _list is null || _list.Count == _token;
    public int Count => _list == null ? 0 : _list.Count;

    public Tasks()
    {
        _list = null;
        _token = 0;
    }
    private Tasks(List<Task> list)
    {
        _list = list;
        _token = list.Count;
    }

    public void WaitAll()
    {
        if (!IsValid) ThrowHelper.TermIsUsed();

        if (_list == null) return;

        foreach (var task in _list)
        {
            task.Wait();
        }
    }
    public void WaitAll(CancellationToken token)
    {
        if (!IsValid) ThrowHelper.TermIsUsed();

        if (_list == null) return;

        foreach (var task in _list)
        {
            task.Wait(token);
            if (token.IsCancellationRequested) return;
        }
    }

    public Task WhenAll()
    {
        if (!IsValid) ThrowHelper.TermIsUsed();

        if (_list == null) return Task.CompletedTask;

        return Task.WhenAll(_list);
    }

    public static Tasks operator +(Tasks left, Task right)
    {
        if (!left.IsValid) ThrowHelper.TermIsUsed();

        if (right.IsCompleted) return left;

        List<Task>? list = left._list ?? new();
        list.Add(right);
        return new(list);
    }
    // 長さをトークンとして用いるためには、長さが単調増加しなければならない。
}
