#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Nonno.Assets.Collections
{
    public readonly struct Typed
    {
        public TypeIndex Index { get; }
        public object? Value { get; }

        private Typed(TypeIndex index, object?obj)
        {
            Index = index;
            Value = obj;
        }

        public static Typed Get<T>(Typed<T> @object)
        {
            return new(@object.Index, @object.Value);
        }
        public static Typed Get<T>(T @object)
        {
            return new(TypeIndex.Of<T>(), @object);
        }

        public class Object
        {
            public TypeIndex TypeIndex { get; }

            public Object()
            {
                TypeIndex = TypeIndex.Of(GetType());
            }

            public static implicit operator Typed(Object o) => new(o.TypeIndex, o);
        }
    }

    public readonly struct Typed<T>
    {
        public TypeIndex Index { get; }
        public T Value { get; }

        private Typed(TypeIndex index, T obj)
        {
            Index = index;
            Value = obj;
        }

        public static Typed<T> Get<U>(Typed<U> @object) where U : T
        {
            return new(@object.Index, @object.Value);
        }
        public static Typed<T> Get<U>(U @object) where U : T
        {
            return new(TypeIndex.Of<U>(), @object);
        }
        public static implicit operator Typed(Typed<T> a) => Typed.Get(a);
        public static explicit operator Typed<T>(Typed a) => new(a.Index, (T)a.Value!);
    }

    public sealed class TypeIndex : Context.Index
    {
        public Type Type { get; }

        private TypeIndex(Type type) : base(Context)
        {
            Type = type;
        }

        public new static Context<TypeIndex> Context { get; } = new();

        readonly static Dictionary<Type, TypeIndex> _indexes = new();

        public static TypeIndex Of<T>()
        {
            return ValueOf<T>.index;
        }
        public static TypeIndex Of(Type type)
        {
            if (!_indexes.TryGetValue(type, out var value))
            {
                value = new(type);
                _indexes.Add(type, value);
            }
            return value;
        }

        static class ValueOf<T>
        {
            public static TypeIndex index;

            static ValueOf()
            {
                if (_indexes.TryGetValue(typeof(T), out var v))
                {
                    index = v;
                }
                else
                {
                    index = new(typeof(T));
                    _indexes.Add(typeof(T), index);
                }
            }
        }
    }
}