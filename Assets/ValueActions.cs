using System;
using System.Collections.Generic;
using System.Text;

namespace Nonno.Assets;

/*
using static System.Console;
using static System.String;
using System.Linq;

F(0);
F(1);
F(2);
F(3);
F(4);

void F(int n)
{
    string s0, s1, s2, s3;
    switch (n)
    {
    case 0:
        s0 = "";
        s1 = "";
        s2 = "";
        s3 = "";
        break;
    case 1:
        s0 = "<T>";
        s1 = "value";
        s2 = "T value";
        s3 = "T, ";
        break;
    default:
        var r = Enumerable.Range(1, n);
        s0 = "<" + Join(", ", r.Select(x => $"T{x}")) + ">";
        s1 = Join(", ", r.Select(x => $"value{x}"));
        s2 = Join(", ", r.Select(x => $"T{x} value{x}"));
        s3 = Concat(r.Select(x => $"T{x}, "));
        break;
    }


    WriteLine($$"""
              
    public readonly unsafe struct ValueAction{{s0}} : IEquatable<ValueAction{{s0}}>
    {
        readonly delegate* managed<object, {{s3}}void> _d;
        readonly object _i;

        private ValueAction(delegate* managed<object, {{s3}}void> d, object i)
        {
            _d = d;
            _i = i;
        }

        public void Invoke({{s2}}) => _d(_i{{(s1 == "" ? "" : ", ")}}{{s1}});
        public bool TryInvoke({{s2}})
        {
            if (_d == null) return false;
            Invoke({{s1}});
            return true;
        }

        public override bool Equals(object? obj) => obj is ValueAction{{s0}} action && Equals(action);
        public bool Equals(ValueAction{{s0}} other) => (void*)_d == other._d && ReferenceEquals(_i, other._i);
        public override int GetHashCode() => HashCode.Combine((nint)_d, _i);

        public static ValueAction{{s0}} Get(Action{{s0}} from)
        {
            var method = from.Method;
            if (method.IsStatic) throw new ArgumentException();
            return new((delegate* managed<object, {{s3}}void>)method.MethodHandle.GetFunctionPointer(), from.Target);
        }
        public static ValueAction{{s0}} Get<TTarget>(delegate* managed<TTarget, {{s3}}void> pointer, TTarget target) where TTarget : class
        {
            return new((delegate* managed<object, {{s3}}void>)pointer, target);
        }

        public static bool operator ==(ValueAction{{s0}} left, ValueAction{{s0}} right) => left.Equals(right);
        public static bool operator !=(ValueAction{{s0}} left, ValueAction{{s0}} right) => !(left == right);
    }
    """);
}
*/

public readonly unsafe struct ValueAction : IEquatable<ValueAction>
{
    readonly delegate* managed<object, void> _d;
    readonly object _i;

    private ValueAction(delegate* managed<object, void> d, object i)
    {
        _d = d;
        _i = i;
    }

    public void Invoke() => _d(_i);
    public bool TryInvoke()
    {
        if (_d == null) return false;

        Invoke();
        return true;
    }

    public override bool Equals(object? obj) => obj is ValueAction action && Equals(action);
    public bool Equals(ValueAction other) => (void*)_d == other._d && ReferenceEquals(_i, other._i);
    public override int GetHashCode() => HashCode.Combine((nint)_d, _i);

    public static ValueAction Get(Action from)
    {
        var method = from.Method;
        if (method.IsStatic) throw new ArgumentException();
        return new((delegate* managed<object, void>)method.MethodHandle.GetFunctionPointer(), from.Target);
    }
    public static ValueAction Get<TTarget>(delegate* managed<TTarget, void> pointer, TTarget target) where TTarget : class
    {
        return new((delegate* managed<object, void>)pointer, target);
    }

    public static bool operator ==(ValueAction left, ValueAction right) => left.Equals(right);
    public static bool operator !=(ValueAction left, ValueAction right) => !(left == right);
}

public readonly unsafe struct ValueAction<T> : IEquatable<ValueAction<T>>
{
    readonly delegate* managed<object, T, void> _d;
    readonly object _i;

    private ValueAction(delegate* managed<object, T, void> d, object i)
    {
        _d = d;
        _i = i;
    }

    public void Invoke(T value) => _d(_i, value);
    public bool TryInvoke(T value)
    {
        if (_d == null) return false;

        Invoke(value);
        return true;
    }

    public override bool Equals(object? obj) => obj is ValueAction<T> action && Equals(action);
    public bool Equals(ValueAction<T> other) => (void*)_d == other._d && ReferenceEquals(_i, other._i);
    public override int GetHashCode() => HashCode.Combine((nint)_d, _i);

    public static ValueAction<T> Get(Action<T> from)
    {
        var method = from.Method;
        if (method.IsStatic) throw new ArgumentException();
        return new((delegate* managed<object, T, void>)method.MethodHandle.GetFunctionPointer(), from.Target);
    }
    public static ValueAction<T> Get<TTarget>(delegate* managed<TTarget, T, void> pointer, TTarget target) where TTarget : class
    {
        return new((delegate* managed<object, T, void>)pointer, target);
    }

    public static bool operator ==(ValueAction<T> left, ValueAction<T> right) => left.Equals(right);
    public static bool operator !=(ValueAction<T> left, ValueAction<T> right) => !(left == right);
}

public readonly unsafe struct ValueAction<T1, T2> : IEquatable<ValueAction<T1, T2>>
{
    readonly delegate* managed<object, T1, T2, void> _d;
    readonly object _i;

    private ValueAction(delegate* managed<object, T1, T2, void> d, object i)
    {
        _d = d;
        _i = i;
    }

    public void Invoke(T1 value1, T2 value2) => _d(_i, value1, value2);
    public bool TryInvoke(T1 value1, T2 value2)
    {
        if (_d == null) return false;

        Invoke(value1, value2);
        return true;
    }

    public override bool Equals(object? obj) => obj is ValueAction<T1, T2> action && Equals(action);
    public bool Equals(ValueAction<T1, T2> other) => (void*)_d == other._d && ReferenceEquals(_i, other._i);
    public override int GetHashCode() => HashCode.Combine((nint)_d, _i);

    public static ValueAction<T1, T2> Get(Action<T1, T2> from)
    {
        var method = from.Method;
        if (method.IsStatic) throw new ArgumentException();
        return new((delegate* managed<object, T1, T2, void>)method.MethodHandle.GetFunctionPointer(), from.Target);
    }
    public static ValueAction<T1, T2> Get<TTarget>(delegate* managed<TTarget, T1, T2, void> pointer, TTarget target) where TTarget : class
    {
        return new((delegate* managed<object, T1, T2, void>)pointer, target);
    }

    public static bool operator ==(ValueAction<T1, T2> left, ValueAction<T1, T2> right) => left.Equals(right);
    public static bool operator !=(ValueAction<T1, T2> left, ValueAction<T1, T2> right) => !(left == right);
}

public readonly unsafe struct ValueAction<T1, T2, T3> : IEquatable<ValueAction<T1, T2, T3>>
{
    readonly delegate* managed<object, T1, T2, T3, void> _d;
    readonly object _i;

    private ValueAction(delegate* managed<object, T1, T2, T3, void> d, object i)
    {
        _d = d;
        _i = i;
    }

    public void Invoke(T1 value1, T2 value2, T3 value3) => _d(_i, value1, value2, value3);
    public bool TryInvoke(T1 value1, T2 value2, T3 value3)
    {
        if (_d == null) return false;

        Invoke(value1, value2, value3);
        return true;
    }

    public override bool Equals(object? obj) => obj is ValueAction<T1, T2, T3> action && Equals(action);
    public bool Equals(ValueAction<T1, T2, T3> other) => (void*)_d == other._d && ReferenceEquals(_i, other._i);
    public override int GetHashCode() => HashCode.Combine((nint)_d, _i);

    public static ValueAction<T1, T2, T3> Get(Action<T1, T2, T3> from)
    {
        var method = from.Method;
        if (method.IsStatic) throw new ArgumentException();
        return new((delegate* managed<object, T1, T2, T3, void>)method.MethodHandle.GetFunctionPointer(), from.Target);
    }
    public static ValueAction<T1, T2, T3> Get<TTarget>(delegate* managed<TTarget, T1, T2, T3, void> pointer, TTarget target) where TTarget : class
    {
        return new((delegate* managed<object, T1, T2, T3, void>)pointer, target);
    }

    public static bool operator ==(ValueAction<T1, T2, T3> left, ValueAction<T1, T2, T3> right) => left.Equals(right);
    public static bool operator !=(ValueAction<T1, T2, T3> left, ValueAction<T1, T2, T3> right) => !(left == right);
}

public readonly unsafe struct ValueAction<T1, T2, T3, T4> : IEquatable<ValueAction<T1, T2, T3, T4>>
{
    readonly delegate* managed<object, T1, T2, T3, T4, void> _d;
    readonly object _i;

    private ValueAction(delegate* managed<object, T1, T2, T3, T4, void> d, object i)
    {
        _d = d;
        _i = i;
    }

    public void Invoke(T1 value1, T2 value2, T3 value3, T4 value4) => _d(_i, value1, value2, value3, value4);
    public bool TryInvoke(T1 value1, T2 value2, T3 value3, T4 value4)
    {
        if (_d == null) return false;

        Invoke(value1, value2, value3, value4);
        return true;
    }

    public override bool Equals(object? obj) => obj is ValueAction<T1, T2, T3, T4> action && Equals(action);
    public bool Equals(ValueAction<T1, T2, T3, T4> other) => (void*)_d == other._d && ReferenceEquals(_i, other._i);
    public override int GetHashCode() => HashCode.Combine((nint)_d, _i);

    public static ValueAction<T1, T2, T3, T4> Get(Action<T1, T2, T3, T4> from)
    {
        var method = from.Method;
        if (method.IsStatic) throw new ArgumentException();
        return new((delegate* managed<object, T1, T2, T3, T4, void>)method.MethodHandle.GetFunctionPointer(), from.Target);
    }
    public static ValueAction<T1, T2, T3, T4> Get<TTarget>(delegate* managed<TTarget, T1, T2, T3, T4, void> pointer, TTarget target) where TTarget : class
    {
        return new((delegate* managed<object, T1, T2, T3, T4, void>)pointer, target);
    }

    public static bool operator ==(ValueAction<T1, T2, T3, T4> left, ValueAction<T1, T2, T3, T4> right) => left.Equals(right);
    public static bool operator !=(ValueAction<T1, T2, T3, T4> left, ValueAction<T1, T2, T3, T4> right) => !(left == right);
}

public readonly unsafe struct ValueAction<T1, T2, T3, T4, T5> : IEquatable<ValueAction<T1, T2, T3, T4, T5>>
{
    readonly delegate* managed<object, T1, T2, T3, T4, T5, void> _d;
    readonly object _i;

    private ValueAction(delegate* managed<object, T1, T2, T3, T4, T5, void> d, object i)
    {
        _d = d;
        _i = i;
    }

    public void Invoke(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5) => _d(_i, value1, value2, value3, value4, value5);
    public bool TryInvoke(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
    {
        if (_d == null) return false;

        Invoke(value1, value2, value3, value4, value5);
        return true;
    }

    public override bool Equals(object? obj) => obj is ValueAction<T1, T2, T3, T4, T5> action && Equals(action);
    public bool Equals(ValueAction<T1, T2, T3, T4, T5> other) => (void*)_d == other._d && ReferenceEquals(_i, other._i);
    public override int GetHashCode() => HashCode.Combine((nint)_d, _i);

    public static ValueAction<T1, T2, T3, T4, T5> Get(Action<T1, T2, T3, T4, T5> from)
    {
        var method = from.Method;
        if (method.IsStatic) throw new ArgumentException();
        return new((delegate* managed<object, T1, T2, T3, T4, T5, void>)method.MethodHandle.GetFunctionPointer(), from.Target);
    }
    public static ValueAction<T1, T2, T3, T4, T5> Get<TTarget>(delegate* managed<TTarget, T1, T2, T3, T4, T5, void> pointer, TTarget target) where TTarget : class
    {
        return new((delegate* managed<object, T1, T2, T3, T4, T5, void>)pointer, target);
    }

    public static bool operator ==(ValueAction<T1, T2, T3, T4, T5> left, ValueAction<T1, T2, T3, T4, T5> right) => left.Equals(right);
    public static bool operator !=(ValueAction<T1, T2, T3, T4, T5> left, ValueAction<T1, T2, T3, T4, T5> right) => !(left == right);
}

public readonly unsafe struct ValueAction<T1, T2, T3, T4, T5, T6> : IEquatable<ValueAction<T1, T2, T3, T4, T5, T6>>
{
    readonly delegate* managed<object, T1, T2, T3, T4, T5, T6, void> _d;
    readonly object _i;

    private ValueAction(delegate* managed<object, T1, T2, T3, T4, T5, T6, void> d, object i)
    {
        _d = d;
        _i = i;
    }

    public void Invoke(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6) => _d(_i, value1, value2, value3, value4, value5, value6);
    public bool TryInvoke(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
    {
        if (_d == null) return false;

        Invoke(value1, value2, value3, value4, value5, value6);
        return true;
    }

    public override bool Equals(object? obj) => obj is ValueAction<T1, T2, T3, T4, T5, T6> action && Equals(action);
    public bool Equals(ValueAction<T1, T2, T3, T4, T5, T6> other) => (void*)_d == other._d && ReferenceEquals(_i, other._i);
    public override int GetHashCode() => HashCode.Combine((nint)_d, _i);

    public static ValueAction<T1, T2, T3, T4, T5, T6> Get(Action<T1, T2, T3, T4, T5, T6> from)
    {
        var method = from.Method;
        if (method.IsStatic) throw new ArgumentException();
        return new((delegate* managed<object, T1, T2, T3, T4, T5, T6, void>)method.MethodHandle.GetFunctionPointer(), from.Target);
    }
    public static ValueAction<T1, T2, T3, T4, T5, T6> Get<TTarget>(delegate* managed<TTarget, T1, T2, T3, T4, T5, T6, void> pointer, TTarget target) where TTarget : class
    {
        return new((delegate* managed<object, T1, T2, T3, T4, T5, T6, void>)pointer, target);
    }

    public static bool operator ==(ValueAction<T1, T2, T3, T4, T5, T6> left, ValueAction<T1, T2, T3, T4, T5, T6> right) => left.Equals(right);
    public static bool operator !=(ValueAction<T1, T2, T3, T4, T5, T6> left, ValueAction<T1, T2, T3, T4, T5, T6> right) => !(left == right);
}

public readonly unsafe struct ValueAction<T1, T2, T3, T4, T5, T6, T7> : IEquatable<ValueAction<T1, T2, T3, T4, T5, T6, T7>>
{
    readonly delegate* managed<object, T1, T2, T3, T4, T5, T6, T7, void> _d;
    readonly object _i;

    private ValueAction(delegate* managed<object, T1, T2, T3, T4, T5, T6, T7, void> d, object i)
    {
        _d = d;
        _i = i;
    }

    public void Invoke(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7) => _d(_i, value1, value2, value3, value4, value5, value6, value7);
    public bool TryInvoke(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
    {
        if (_d == null) return false;

        Invoke(value1, value2, value3, value4, value5, value6, value7);
        return true;
    }

    public override bool Equals(object? obj) => obj is ValueAction<T1, T2, T3, T4, T5, T6, T7> action && Equals(action);
    public bool Equals(ValueAction<T1, T2, T3, T4, T5, T6, T7> other) => (void*)_d == other._d && ReferenceEquals(_i, other._i);
    public override int GetHashCode() => HashCode.Combine((nint)_d, _i);

    public static ValueAction<T1, T2, T3, T4, T5, T6, T7> Get(Action<T1, T2, T3, T4, T5, T6, T7> from)
    {
        var method = from.Method;
        if (method.IsStatic) throw new ArgumentException();
        return new((delegate* managed<object, T1, T2, T3, T4, T5, T6, T7, void>)method.MethodHandle.GetFunctionPointer(), from.Target);
    }
    public static ValueAction<T1, T2, T3, T4, T5, T6, T7> Get<TTarget>(delegate* managed<TTarget, T1, T2, T3, T4, T5, T6, T7, void> pointer, TTarget target) where TTarget : class
    {
        return new((delegate* managed<object, T1, T2, T3, T4, T5, T6, T7, void>)pointer, target);
    }

    public static bool operator ==(ValueAction<T1, T2, T3, T4, T5, T6, T7> left, ValueAction<T1, T2, T3, T4, T5, T6, T7> right) => left.Equals(right);
    public static bool operator !=(ValueAction<T1, T2, T3, T4, T5, T6, T7> left, ValueAction<T1, T2, T3, T4, T5, T6, T7> right) => !(left == right);
}

public readonly unsafe struct ValueAction<T1, T2, T3, T4, T5, T6, T7, T8> : IEquatable<ValueAction<T1, T2, T3, T4, T5, T6, T7, T8>>
{
    readonly delegate* managed<object, T1, T2, T3, T4, T5, T6, T7, T8, void> _d;
    readonly object _i;

    private ValueAction(delegate* managed<object, T1, T2, T3, T4, T5, T6, T7, T8, void> d, object i)
    {
        _d = d;
        _i = i;
    }

    public void Invoke(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8) => _d(_i, value1, value2, value3, value4, value5, value6, value7, value8);
    public bool TryInvoke(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8)
    {
        if (_d == null) return false;

        Invoke(value1, value2, value3, value4, value5, value6, value7, value8);
        return true;
    }

    public override bool Equals(object? obj) => obj is ValueAction<T1, T2, T3, T4, T5, T6, T7, T8> action && Equals(action);
    public bool Equals(ValueAction<T1, T2, T3, T4, T5, T6, T7, T8> other) => (void*)_d == other._d && ReferenceEquals(_i, other._i);
    public override int GetHashCode() => HashCode.Combine((nint)_d, _i);

    public static ValueAction<T1, T2, T3, T4, T5, T6, T7, T8> Get(Action<T1, T2, T3, T4, T5, T6, T7, T8> from)
    {
        var method = from.Method;
        if (method.IsStatic) throw new ArgumentException();
        return new((delegate* managed<object, T1, T2, T3, T4, T5, T6, T7, T8, void>)method.MethodHandle.GetFunctionPointer(), from.Target);
    }
    public static ValueAction<T1, T2, T3, T4, T5, T6, T7, T8> Get<TTarget>(delegate* managed<TTarget, T1, T2, T3, T4, T5, T6, T7, T8, void> pointer, TTarget target) where TTarget : class
    {
        return new((delegate* managed<object, T1, T2, T3, T4, T5, T6, T7, T8, void>)pointer, target);
    }

    public static bool operator ==(ValueAction<T1, T2, T3, T4, T5, T6, T7, T8> left, ValueAction<T1, T2, T3, T4, T5, T6, T7, T8> right) => left.Equals(right);
    public static bool operator !=(ValueAction<T1, T2, T3, T4, T5, T6, T7, T8> left, ValueAction<T1, T2, T3, T4, T5, T6, T7, T8> right) => !(left == right);
}