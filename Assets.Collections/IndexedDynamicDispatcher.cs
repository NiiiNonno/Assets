#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if FORUNITY
namespace Nonno.Assets.Collections
{
    public class IndexedDynamicDispatcher
    {
        readonly CorrespondenceTable<TypeIndex, Action<object?>> _table = new(TypeIndex.Context);
        readonly List<KeyValuePair<Type, Action<object?>>> _dels = new();

        public void Overload<T>(Action<T?> action) where T : class
        {
            _dels.Add(new(typeof(T), obj => action((T?)obj)));
            _table.Clear();
        }

        public void Unload<T>()
        {
            var i = _dels.FindIndex(x => x.Key == typeof(T));
            _dels.RemoveAt(i);
            _table.Clear();
        }

        public void Dispatch(Typed obj)
        {
            var v = _table[obj.Index];
            v ??= Resolve(obj.Index);
            v.Invoke(obj.Value);
        }

        Action<object?> Resolve(TypeIndex index)
        {
            var t = index.Type;

            foreach (var (key, value) in _dels)
            {
                if (key.IsAssignableFrom(t))
                {
                    _table[index] = value;
                    return value;
                }
            }

            throw new Exception("îzëóÇ…é∏îsÇµÇ‹ÇµÇΩÅB");
        }
    }
}
#else
namespace Nonno.Assets.Collections
{
    public class IndexedDynamicDispatcher
    {
        readonly CorrespondenceTable<TypeIndex, Action<object?>> _table = new(TypeIndex.Context);
        readonly List<KeyValuePair<Type, Action<object?>>> _dels = new();

        public void Overload<T>(Action<T?> action) where T : class
        {
            var v = Unsafe.As<Action<object?>>(action);
            _dels.Add(new(typeof(T), v));
            _table.Clear();
        }

        public void Unload<T>()
        {
            var i = _dels.FindIndex(x => x.Key == typeof(T));
            _dels.RemoveAt(i);
            _table.Clear();
        }
        
        public void Dispatch(Typed obj)
        {
            var v = _table[obj.Index];
            v ??= Resolve(obj.Index);
            v.Invoke(obj.Value);
        }

        Action<object?> Resolve(TypeIndex index)
        {
            var t = index.Type;

            foreach (var (key, value) in _dels)
            {
                if (key.IsAssignableFrom(t))
                {
                    _table[index] = value;
                    return value;
                }
            }
            //while (t is not null)
            //{
            //    if (_dels.TryGetValue(t, out var r))
            //    {
            //        _table[index] = r;
            //        return r;
            //    }

            //    t = t.BaseType;
            //}

            //t = index.Type.Inter;

            //foreach (var t2 in t.GetInterfaces())
            //{
            //    if (_dels.TryGetValue(t2, out var i))
            //    {
            //        _table[index] = i;
            //        return i;
            //    }
            //}

            throw new Exception("îzëóÇ…é∏îsÇµÇ‹ÇµÇΩÅB");
        }
    }
}
#endif