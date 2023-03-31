// 参見 https://learn.microsoft.com/en-us/archive/blogs/mattwar/linq-building-an-iqueryable-provider-part-i
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Nonno.Assets.Collections.Utils;

namespace Nonno.Assets.Collections;
public class Query<T> : IQueryable<T>, IQueryable, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable
{
    readonly QueryProvider _provider;
    readonly Expression _expression;

    public Query(QueryProvider provider)
    {
        _provider = provider;
        _expression = Expression.Constant(this);
    }

    public Query(QueryProvider provider, Expression expression)
    {
        if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type)) ThrowHelper.ArgumentTypeIsNotAssignable(expression, typeof(IQueryable<T>));

        _provider = provider;
        _expression = expression;
    }

    public Expression Expression => _expression;
    public Type ElementType => typeof(T);
    public IQueryProvider Provider => _provider;

    public IEnumerator<T> GetEnumerator() => _provider.Execute<IEnumerable<T>>(_expression).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString()
    {
        return _provider.GetQueryText(_expression);
    }
}

public abstract class QueryProvider : IQueryProvider
{
    protected QueryProvider()
    {

    }

    public Query<T> CreateQuery<T>(Expression expression)
    {
        return new Query<T>(this, expression);
    }
    IQueryable<T> IQueryProvider.CreateQuery<T>(Expression expression) => CreateQuery<T>(expression);
    public IQueryable CreateQuery(Expression expression)
    {
        Type elementType = GetElementType(expression.Type);

        try
        {
            var r = (IQueryable?)Activator.CreateInstance(typeof(Query<>).MakeGenericType(elementType), new object[] { this, expression });
            if (r is null) ThrowHelper.FailToGetReflections();
            return r;
        }
        catch (TargetInvocationException tie)
        {
            throw tie.InnerException ?? tie;
        }
    }

    public abstract object? Execute(Expression expression);
    public virtual T Execute<T>(Expression expression) => (T?)Execute(expression) ?? throw new NullReferenceException();

    public abstract string GetQueryText(Expression expression);
}

partial class Utils
{
    internal static Type GetElementType(Type seqType)
    {
        Type? ienum = FindIEnumerable(seqType);
        if (ienum is null) return seqType;
        return ienum.GetGenericArguments()[0];
    }

    private static Type? FindIEnumerable(Type seqType)
    {
        if (seqType is null || seqType == typeof(string)) return null;

        if (seqType.IsArray)return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType() ?? throw new NullReferenceException());

        if (seqType.IsGenericType)
        {
            foreach (Type arg in seqType.GetGenericArguments())
            {
                Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                if (ienum.IsAssignableFrom(seqType)) return ienum;
            }
        }

        Type[] ifaces = seqType.GetInterfaces();
        if (ifaces != null && ifaces.Length > 0)
        {
            foreach (Type iface in ifaces)
            {
                Type? ienum = FindIEnumerable(iface);
                if (ienum is not null) return ienum;
            }
        }

        if (seqType.BaseType is not null && seqType.BaseType != typeof(object))
        {
            return FindIEnumerable(seqType.BaseType);
        }

        return null;
    }
}
