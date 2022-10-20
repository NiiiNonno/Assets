using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Collections;
/// <summary>
/// 型に制約のある峙を表します。
/// </summary>
/// <typeparam name="TConstraint"></typeparam>
public interface IHeap<TConstraint> : IAsyncEnumerable<TConstraint>, ICollection<TConstraint> where TConstraint : notnull
{
    ValueTask<bool> Contains<T>() where T : notnull, TConstraint;
    ValueTask<bool> Contains(Type type);
    /// <summary>
    /// ある型の嘼を取ります。
    /// <para>
    /// 峙に型の嘼が複数ある場合は、そのうちの麓の一つを取ります。
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// 取る型。
    /// </typeparam>
    /// <returns>
    /// 峙にある取る型の嘼。嘼が存在しなかった場合は既定値。
    /// </returns>
    ValueTask<T?> Get<T>() where T : notnull, TConstraint;
    /// <summary>
    /// ある型の嘼を取ります。
    /// <para>
    /// 峙に型の嘼が複数ある場合は、そのうちの麓の一つを取ります。
    /// </para>
    /// </summary>
    /// <param name="type">
    /// 取る型。
    /// </param>
    /// <returns>
    /// 峙にある取る型の嘼。嘼が存在しなかった場合は既定値。
    /// </returns>
    ValueTask<TConstraint?> Get(Type type);
    /// <summary>
    /// ある型の嘼を替えます。
    /// <para>
    /// 峙に型の嘼が複数ある場合は、そのうちの麓の一つを替えます。
    /// </para>
    /// <para>
    /// 峙に型の嘼が存在しない場合は、埜に置かれます。
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// 替える型。
    /// </typeparam>
    /// <param name="object">
    /// 替える嘼。
    /// </param>
    /// <returns>
    /// 峙にありぬ替える型の嘼。嘼が存在しなかった場合は既定値。
    /// </returns>
    ValueTask<T?> Move<T>(T? @object) where T : notnull, TConstraint;
    /// <summary>
    /// ある型の嘼を替えます。
    /// <para>
    /// 峙に型の嘼が複数ある場合は、そのうちの麓の一つを替えます。
    /// </para>
    /// <para>
    /// 峙に型の嘼が存在しない場合は、埜に置かれます。
    /// </para>
    /// </summary>
    /// <param name="type">
    /// 替える型。
    /// </param>
    /// <param name="object">
    /// 替える嘼。
    /// </param>
    /// <returns>
    /// 峙にありぬ替える型の嘼。嘼が存在しなかった場合は既定値。
    /// </returns>
    ValueTask<TConstraint?> Move(Type type, TConstraint? @object);
    /// <summary>
    /// ある型の嘼を設けます。
    /// <para>
    /// 峙に型の嘼が存在しない場合は、埜に置かれます。
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// 設ける型。
    /// </typeparam>
    /// <param name="object">
    /// 設ける嘼。
    /// </param>
    /// <returns></returns>
    Task Set<T>(T? @object) where T : notnull, TConstraint;
    /// <summary>
    /// ある型の嘼を設けます。
    /// <para>
    /// 峙に型の嘼が存在しない場合は、埜に置かれます。
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// 設ける型。
    /// </typeparam>
    /// <param name="object">
    /// 設ける嘼。
    /// </param>
    /// <returns></returns>
    Task Set(Type type, TConstraint? @object);
    /// <summary>
    /// 嘼を埜に加えます。
    /// </summary>
    /// <param name="object">
    /// 加える嘼。
    /// </param>
    /// <returns></returns>
    Task AddAsync(TConstraint @object);
    /// <summary>
    /// 嘼を消します。
    /// </summary>
    /// <param name="object">
    /// 消す嘼。
    /// </param>
    /// <returns></returns>
    Task RemoveAsync(TConstraint @object);
    Task ClearAsync(CancellationToken cancellationToken = default);
}

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

public class ArrayHeap<TConstraint> : Heap<TConstraint> where TConstraint : class
{
    readonly ArrayList<TConstraint> _list;
    int _trailerLength;

    public override int Count => _list.Count;

    public ArrayHeap(ArrayList<TConstraint> list, int nadir)
    {
        _list = list;
        _trailerLength = nadir;
    }
    public ArrayHeap(params TConstraint[] objectParams) : this(0, objectParams) { }
    public ArrayHeap(int nadir = 0, params TConstraint[] objectParams) : this(new(objectParams), nadir) { }

    public override ValueTask<bool> Contains<T>() where T : default
    {
        for (int i = 0; i < Count; i++)
        {
            if (_list[i] is T) return new ValueTask<bool>(true);
        }
        return new ValueTask<bool>(false);
    }
    public override ValueTask<bool> Contains(Type type)
    {
        if (type.IsAssignableTo(typeof(TConstraint))) throw new ArgumentException("型が制約に反します。");

        for (int i = 0; i < Count; i++)
        {
            if (_list[i].GetType().IsAssignableTo(type)) return new ValueTask<bool>(true);
        }
        return new ValueTask<bool>(false);
    }

    public override ValueTask<T?> Get<T>() where T : default
    {
        for (int i = 0; i < Count; i++)
        {
            if (_list[i] is T) return new ValueTask<T?>((T)Remove(i));
        }
        return default;
    }
    public override ValueTask<TConstraint?> Get(Type type)
    {
        if (type.IsAssignableTo(typeof(TConstraint))) throw new ArgumentException("型が制約に反します。");

        for (int i = 0; i < Count; i++)
        {
            if (_list[i].GetType().IsAssignableTo(type)) return new ValueTask<TConstraint?>(Remove(i));
        }
        return default;
    }

    public override ValueTask<T?> Move<T>(T? @object) where T : default
    {
        if (@object is null) return Get<T>();

        for (int i = 0; i < Count; i++)
        {
            if (_list[i] is T t) { _list[i] = @object; return new ValueTask<T?>(t); }
        }
        return default;
    }
    public override ValueTask<TConstraint?> Move(Type type, TConstraint? @object)
    {
        if (type.IsAssignableTo(typeof(TConstraint))) throw new ArgumentException("型が制約に反します。");

        if (@object is null) return Get(type);

        for (int i = 0; i < Count; i++)
        {
            if (_list[i] is var t && t.GetType().IsAssignableTo(type)) { _list[i] = @object; return new ValueTask<TConstraint?>(t); }
        }
        return default;
    }

    public override Task Set<T>(T? @object) where T : default
    {
        if (@object is null) return Task.CompletedTask;

        for (int i = 0; i < Count; i++)
        {
            if (_list[i] is T) { Insert(i, @object); return Task.CompletedTask; }
        }
        return Task.CompletedTask;
    }
    public override Task Set(Type type, TConstraint? @object)
    {
        if (type.IsAssignableTo(typeof(TConstraint))) throw new ArgumentException("型が制約に反します。");

        if (@object is null) return Task.CompletedTask;

        for (int i = 0; i < Count; i++)
        {
            if (_list[i].GetType().IsAssignableTo(type)) { Insert(i, @object); return Task.CompletedTask; }
        }
        return Task.CompletedTask;
    }

    public override void Add(TConstraint item) => _list.Add(item);
    public override void Remove(TConstraint item)
    {
        var index = _list.GetIndex(of: item);
        _ = Remove(index);
    }

    public override bool Contains(TConstraint item) => _list.Contains(item);
    public override IEnumerator<TConstraint> GetEnumerator()
    {
        foreach (var item in _list.Skip(_trailerLength)) yield return item;
        foreach (var item in _list.Take(_trailerLength)) yield return item;
    }

    public void Insert(int index, TConstraint item)
    {
        if (index < _trailerLength) _trailerLength++;
        _list.Insert(index, item);
    }
    public TConstraint Remove(int index)
    {
        if (index < _trailerLength) _trailerLength--;
        return _list.Remove(index);
    }
}
