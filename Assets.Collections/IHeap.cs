using System;
using System.Collections.Generic;
using System.Data;
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
    ValueTask<T?> Move<T>(T? @object) where T : notnull, TConstraint;
    ValueTask<TConstraint?> Move(Type type, TConstraint? @object);
    Task Set<T>(T? @object) where T : notnull, TConstraint;
    Task Set(Type type, TConstraint? @object);
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
    public async ValueTask<T?> Move<T>(T? @object) where T : notnull, TConstraint
    {
        await Task.CompletedTask;
        for (int i = 0; i < Count; i++)
        {
            if (this[i] is T t)
            {
                if (@object is null) _ = Remove(at: i);
                else this[i] = @object;
                return t;
            }
        }
        if (@object is not null) Add(@object);
        return default;
    }
    public async ValueTask<TConstraint?> Move(Type type, TConstraint? @object)
    {
        if (@object is not null && !@object.GetType().IsAssignableTo(type)) throw new ArgumentException("函が指定された型ではありません。", nameof(@object));

        await Task.CompletedTask;
        for (int i = 0; i < Count; i++)
        {
            if (this[i].GetType().IsAssignableTo(type))
            {
                if (@object is null) _ = Remove(at: i);
                else this[i] = @object;
                return this[i];
            }
        }
        if (@object is not null) Add(@object);
        return default;
    }
    public async Task Set<T>(T? @object) where T : notnull, TConstraint
    {
        await Task.CompletedTask;
        for (int i = 0; i < Count; i++)
        {
            if (this[i] is T)
            {
                if (@object is null) _ = Remove(at: i);
                else this[i] = @object;
                return;
            }
        }
        if (@object is not null) Add(@object);
    }
    public async Task Set(Type type, TConstraint? @object)
    {
        if (@object is not null && !@object.GetType().IsAssignableTo(type)) throw new ArgumentException("函が指定された型ではありません。", nameof(@object));

        await Task.CompletedTask;
        for (int i = 0; i < Count; i++)
        {
            if (this[i].GetType().IsAssignableTo(type))
            {
                if (@object is null) _ = Remove(at: i);
                else this[i] = @object;
                return;
            }
        }
        if (@object is not null) Add(@object);
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
