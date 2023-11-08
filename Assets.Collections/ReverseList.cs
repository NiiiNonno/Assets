//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Text;

//namespace Nonno.Assets.Collections;
//public readonly struct ReverseList<TList, T> : IList<T> where TList : IList<T>
//{
//    readonly TList _list;

//    public ReverseList(TList list) => _list = list;

//    public int Count => _list.Count;

//    public void Clear() => _list.Clear();
//    public bool Contains(T item) => _list.Contains(item);
//    public void Copy(Span<T> to, ref int index)
//    {
//        _list.Copy(to, ref index);
//        to.Reverse();
//    }
//    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
//    public int GetIndex(T of) => _list.GetIndex(of);
//    public void Insert(int index, T item) => _list.Insert(index, item);
//    public T Remove(int at) => _list.Remove(at);
//    public bool TryAdd(T item) => _list.TryAdd(item);
//    public bool TryGetValue(int index, [MaybeNullWhen(false)] out T value) => _list.TryGetValue(index, out value);
//    public bool TryRemove(T item) => _list.TryRemove(item);
//    public bool TrySetValue(int index, T value) => _list.TrySetValue(index, value);
//}
