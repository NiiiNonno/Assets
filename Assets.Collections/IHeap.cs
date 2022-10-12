using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Collections;
public interface IHeap<TConstraint> : IAsyncEnumerable<TConstraint>, IEnumerable<TConstraint> where TConstraint : notnull
{
    ValueTask<bool> Contains<T>() where T : notnull, TConstraint;
    ValueTask<bool> Contains(Type type);
    ValueTask<T?> Get<T>() where T : notnull, TConstraint;
    ValueTask<TConstraint?> Get(Type type);
    ValueTask<T?> Remove<T>() where T : notnull, TConstraint;
    ValueTask<TConstraint?> Remove(Type type);
    Task Add(TConstraint @object);
    Task Remove(TConstraint @object);
}

public class ArrayHeap<TConstraint> : ArrayList<TConstraint>, IHeap<TConstraint> where TConstraint : notnull
{
    public ArrayHeap() : base() { }
    public ArrayHeap(int capacity) : base(capacity) { }
    public ArrayHeap(params TConstraint[] objectParams) : base(objectParams) { }

    public async ValueTask<bool> Contains<T>() where T : notnull, TConstraint 
    { 
        await foreach (var item in this) 
        { 
            if (item is T) return true; 
        } 
        return false; 
    }
    public async ValueTask<bool> Contains(Type type)
    {
        await foreach (var item in this)
        {
            if (item.GetType().IsAssignableTo(type)) return true;
        }
        return false;
    }
    public async ValueTask<T?> Get<T>() where T : notnull, TConstraint
    {
        await foreach (var item in this)
        {
            if (item is T t) return t;
        }
        return default;
    }
    public async ValueTask<TConstraint?> Get(Type type)
    {
        await foreach (var item in this)
        {
            if (item.GetType().IsAssignableTo(type)) return item;
        }
        return default;
    }
    public async ValueTask<T?> Remove<T>() where T : notnull, TConstraint
    {
        await Task.CompletedTask;
        for (int i = 0; i < Count; i++)
        {
            if (this[i] is T t)
            {
                _ = Remove(at: i);
                return t;
            }
        }
        return default;
    }
    public async ValueTask<TConstraint?> Remove(Type type)
    {
        await Task.CompletedTask;
        for (int i = 0; i < Count; i++)
        {
            if (this[i].GetType().IsAssignableTo(type))
            {
                _ = Remove(at: i);
                return this[i];
            }
        }
        return default;
    }
    Task IHeap<TConstraint>.Add(TConstraint @object) { Add(@object); return Task.CompletedTask; }
    Task IHeap<TConstraint>.Remove(TConstraint @object) { Remove(@object); return Task.CompletedTask; }
    
    async IAsyncEnumerator<TConstraint> IAsyncEnumerable<TConstraint>.GetAsyncEnumerator(CancellationToken cancellationToken)
    {
        await Task.CompletedTask; // 警告抑制。
        foreach (var item in this)
        {
            yield return item;
        }
    }
}
