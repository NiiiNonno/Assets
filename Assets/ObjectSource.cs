#nullable enable
using System;
using System.Threading.Tasks;

namespace Nonno.Assets
{
    public interface IObjectSource<T>
    {
        int Count { get; }
        T Get();
        ValueTask<T> GetAsync() => new(Get());
        void Release(T obj);
        Task ReleaseAsync(T obj) { Release(obj); return Task.CompletedTask; }

        static IObjectSource<T>? Default { get; set; }
    }

    public class ObjectSource<T> : IObjectSource<T>
    {
        public int Count { get; private set; }
        public Func<T> Constructor { get; }

        public ObjectSource(Func<T> constructor)
        {
            Constructor = constructor;
        }

        public T Get()
        {
            Count++;
            return Constructor();
        }

        public void Release(T obj)
        {
            Count--;
        }
    }
}
