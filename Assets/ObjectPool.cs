using System.Collections.Concurrent;
using System;

namespace Nonno.Assets
{
    public class ObjectPool<T> : IObjectSource<T>
    {
        readonly ConcurrentBag<T> _objects;
        readonly IObjectSource<T> _source;
        public IObjectSource<T> InternalSource => _source;
        public int Count { get; private set; }

        public ObjectPool(IObjectSource<T> source)
        {
            _objects = new ConcurrentBag<T>();
            _source = source;
        }

        public virtual T Get()
        {
            Count++;
            return _objects.TryTake(out T? item) ? item : Create();
        }
        public virtual ValueTask<T> GetAsync()
        {
            Count++;
            return _objects.TryTake(out T? item) ? new(item) : CreateAsync();
        }

        public virtual void Release(T item)
        {
            Count--;
            _objects.Add(item);
        }
        public virtual Task ReleaseAsync(T item)
        {
            Count--;
            _objects.Add(item);
            return Task.CompletedTask;
        }

        protected T Create() => _source.Get();
        protected ValueTask<T> CreateAsync() => _source.GetAsync();

        protected void Destroy(T item) => _source.Release(item);
        protected Task DestroyAsync(T item) => _source.ReleaseAsync(item);
    }
}