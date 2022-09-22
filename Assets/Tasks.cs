namespace Nonno.Assets;

public readonly struct Tasks
{
    readonly List<Task>? _list;

    public int Count => _list == null ? 0 : _list.Count;

    private Tasks(List<Task>? list) => _list = list;

    public void WaitAll()
    {
        if (_list == null) return;

        foreach (var task in _list)
        {
            task.Wait();
        }
    }
    public void WaitAll(CancellationToken token)
    {
        if (_list == null) return;

        foreach (var task in _list)
        {
            task.Wait(token);
            if (token.IsCancellationRequested) return;
        }
    }

    public Task WhenAll()
    {
        if (_list == null) return Task.CompletedTask;

        return Task.WhenAll(_list);
    }

    public static Tasks operator +(Tasks left, Task right)
    {
        if (right.IsCompleted) return left;

        List<Task>? list = left._list ?? new();
        list.Add(right);
        return new(list);
    }
}
