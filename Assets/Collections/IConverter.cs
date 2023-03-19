using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Collections;
public interface IConverter<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>
{
    T2 this[T1 t1] { get => GetForward(t1); set => SetForward(t1, value); }
    T1 this[T2 t2] { get => GetBackward(t2); set => SetBackward(t2, value); }

    ICollection<T1> Keys { get; }
    ICollection<T2> Values { get; }

    T2 GetForward(T1 key);
    T1 GetBackward(T2 key);
    bool TryGetForward(T1 key, [MaybeNullWhen(false)] out T2 value);
    bool TryGetBackward(T2 key, [MaybeNullWhen(false)] out T1 value);
    void SetForward(T1 key, T2 value);
    void SetBackward(T2 key, T1 value);
    bool TrySetForward(T1 key, T2 value);
    bool TrySetBackward(T2 key, T1 value);
    T2 MoveForward(T1 key, T2 value);
    T1 MoveBackward(T2 key, T1 value);
    bool TryMoveForward(T1 key, T2 neo, [MaybeNullWhen(false)] out T2 old);
    bool TryMoveBackward(T2 key, T1 neo, [MaybeNullWhen(false)] out T1 old);
    bool ContainsForward(T1 item);
    bool ContainsBackward(T2 item);
    void Add(KeyValuePair<T1, T2> item) => TryAdd(item);
    void Remove(KeyValuePair<T1, T2> item) => TryRemove(item);
    bool TryAdd(KeyValuePair<T1, T2> item);
    bool TryRemove(KeyValuePair<T1, T2> item);
    void Add(T1 t1, T2 t2) => _ = TryAdd(t1, t2);
    bool TryAdd(T1 t1, T2 t2);
    T2 Remove(T1 t1) => TryRemove(t1, out var r) ? r : throw new Exception("変換の削除に失敗しました。");
    bool TryRemove(T1 t1,[MaybeNullWhen(false)] out T2 t2);
    IConverter<T2, T1> Reverse();

    protected class ReverseConverter : IConverter<T2, T1>
    {
        public readonly IConverter<T1, T2> _converter;

        public ReverseConverter(IConverter<T1, T2> converter)
        {
            _converter = converter;
        }

        public ICollection<T2> Keys => _converter.Values;
        public ICollection<T1> Values => _converter.Keys;

        public T2 GetBackward(T1 key) => _converter.GetForward(key);
        public T1 GetForward(T2 key) => _converter.GetBackward(key);
        public bool TryGetForward(T2 key, [MaybeNullWhen(false)] out T1 value) => _converter.TryGetBackward(key, out value);
        public bool TryGetBackward(T1 key, [MaybeNullWhen(false)] out T2 value) => _converter.TryGetForward(key, out value);
        public void SetBackward(T1 key, T2 value) => _converter.SetForward(key, value);
        public void SetForward(T2 key, T1 value) => _converter.SetBackward(key, value);
        public bool TrySetForward(T2 key, T1 value) => _converter.TrySetBackward(key, value);
        public bool TrySetBackward(T1 key, T2 value) => _converter.TrySetForward(key, value);
        public T1 MoveForward(T2 key, T1 value) => _converter.MoveBackward(key, value);
        public T2 MoveBackward(T1 key, T2 value) => _converter.MoveForward(key, value);
        public bool TryMoveForward(T2 key, T1 neo, [MaybeNullWhen(false)] out T1 old) => _converter.TryMoveBackward(key, neo, out old);
        public bool TryMoveBackward(T1 key, T2 neo, [MaybeNullWhen(false)] out T2 old) => _converter.TryMoveForward(key, neo, out old);
        public bool ContainsBackward(T1 item) => _converter.ContainsForward(item);
        public bool ContainsForward(T2 item) => _converter.ContainsBackward(item);
        public bool TryAdd(KeyValuePair<T2, T1> item) => _converter.TryAdd(item: new(item.Value, item.Key));
        public bool TryAdd(T2 t1, T1 t2) => _converter.TryAdd(t2, t1);
        public bool TryRemove(KeyValuePair<T2, T1> item) => _converter.TryRemove(item: new(item.Value, item.Key));
        public bool TryRemove(T2 t1, [MaybeNullWhen(false)] out T1 t2)
        {
            t2 = _converter.GetBackward(t1);
            return _converter.TryRemove(item: new(t2, t1));
        }
        public IConverter<T1, T2> Reverse() => _converter;
        public IEnumerator<KeyValuePair<T2, T1>> GetEnumerator()
        {
            foreach (var (key, value) in _converter) yield return new(value, key);
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
