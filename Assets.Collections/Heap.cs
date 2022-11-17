using System.Diagnostics;

namespace Nonno.Assets.Collections;

public abstract class Heap<TConstraint> : IHeap<TConstraint> where TConstraint: notnull
{
    public abstract int Count { get; }

    public virtual async ValueTask<bool> Contains<T>() where T : notnull, TConstraint => await Contains(typeof(T));
    public abstract ValueTask<bool> Contains(Type type);

    public virtual async ValueTask<T?> Get<T>() where T : notnull, TConstraint => (T?)await Get(typeof(T));
    public abstract ValueTask<TConstraint?> Get(Type type);

    public virtual async ValueTask<T?> Move<T>(T? @object) where T : notnull, TConstraint => (T?)await Move(typeof(T), @object);
    public abstract ValueTask<TConstraint?> Move(Type type, TConstraint? @object);

    public virtual async Task Set<T>(T? @object) where T : notnull, TConstraint => await Set(typeof(T), @object);
    public abstract Task Set(Type type, TConstraint? @object);

    public virtual Task AddAsync(TConstraint @object) { Add(@object); return Task.CompletedTask; }
    public abstract void Add(TConstraint item);
    bool ICollection<TConstraint>.TryAdd(TConstraint item) { Add(item); return true; }

    public virtual Task RemoveAsync(TConstraint @object) { Remove(@object); return Task.CompletedTask; }
    public abstract void Remove(TConstraint item);
    bool ICollection<TConstraint>.TryRemove(TConstraint item) { if (Contains(item)) { Remove(item); return true; } else { return false; } }

    public virtual async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        while (Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TConstraint? item = await Get<TConstraint>();
            Debug.Assert(item is null);
            await RemoveAsync(item!);
        }
    }
    public virtual void Clear() => ClearAsync().Wait();

    public abstract bool Contains(TConstraint item);
    public virtual void Copy(Span<TConstraint> to, ref int index)
    {
        foreach (var item in this) to[index++] = item;
    }

    public abstract IEnumerator<TConstraint> GetEnumerator();
    public virtual async IAsyncEnumerator<TConstraint> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        foreach (var item in this) yield return item;
    }
}
