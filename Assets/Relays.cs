// 令和弐年大暑確認済。
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Nonno.Assets;

/// <summary>
/// 代理人への接続を中継する中継器を表します。
/// <para>
/// この型のインスタンスの包括サイズは72バイトです。
/// </para>
/// </summary>
public class Relay
{
    protected Delegate _delegate;
    protected int _count;

    public Type CourierType => _delegate.GetType().GetGenericArguments()[0];
    public Delegate Delegate => _delegate;
    public int Count => _count;

    /// <summary>
    /// 中継器を初期化します。
    /// </summary>
    /// <param name="courierType">
    /// 中継する型。
    /// </param>
    public Relay(Type courierType)
    {
        _delegate = null!;

        // 注意！CourierTypeが使用できないため、ここでTarget(IReadOnlySet<object>)は呼べない。
        Target((IReadOnlySet<Delegate>)new HashSet<Delegate>(), courierType);
    }

    /// <summary>
    /// 中継先を指定します。既存の指定先は破棄されます。
    /// </summary>
    /// <param name="targets">
    /// 指定する中継先。
    /// <para>
    /// 中継先には、引数を一つ持つ代理人のみを含む集合を指定してください。
    /// </para>
    /// </param>
    /// <exception cref="ArgumentException">
    /// 代理人のシグネチャが正しくありません。引数を一つ持つ代理人のみを含む集合を指定してください。
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Obsolete("`IReadOnlySet<T>`を引数とするメソッドに置き換え可能だから。")]
    public virtual void Target(ISet<Delegate> targets) => Target((IReadOnlySet<Delegate>)targets);
    [Obsolete("`IReadOnlySet<T>`を引数とするメソッドに置き換え可能だから。")]
    protected void Target(ISet<Delegate> targets, Type courierType) => Target((IReadOnlySet<Delegate>)targets, courierType);
    /// <summary>
    /// 中継先を指定します。既存の指定先は破棄されます。
    /// </summary>
    /// <param name="targets">
    /// 指定する中継先。
    /// <para>
    /// 中継先には、引数を一つ持つ代理人のみを含む集合を指定してください。
    /// </para>
    /// </param>
    /// <exception cref="ArgumentException">
    /// 代理人のシグネチャが正しくありません。引数を一つ持つ代理人のみを含む集合を指定してください。
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void Target(IReadOnlySet<Delegate> targets) => Target(targets, CourierType);
    protected void Target(IReadOnlySet<Delegate> targets, Type courierType)
    {
        var courierE = Expression.Parameter(courierType, "courier");

        List<Expression> sentences = new();

        foreach (var target in targets) sentences.AddRange(GetSentences(courierE, target));
        _count = sentences.Count;

        // コンパイルして納める。
        _delegate = Expression.Lambda(typeof(Action<>).MakeGenericType(courierType), Expression.Block(sentences), courierE).Compile();
    }

    protected static IEnumerable<Expression> GetSentences(Expression parameterExpression, Delegate target)
    {
        const string IMPLICIT_LITERAL = "op_Implicit";
        const BindingFlags OPERATOR_FLAGS = BindingFlags.Public | BindingFlags.Static;

        var parameterT = parameterExpression.Type;

        var targetArguments = target.GetType().GetGenericArguments();
        if (targetArguments.Length != 1) throw new ArgumentException("代理人のシグネチャが正しくありません。型引数を一つ持つ代理人を指定してください。");
        var targetT = targetArguments[0];

        // 直に変換ができる場合。
        if (parameterT.IsAssignableTo(targetT))
        {
            yield return Expression.Invoke(Expression.Constant(target), parameterExpression);
            //yield return Expression.Call(Expression.Constant(recepter), targetMethodName, new Type[1] { targetType }, detonatorExpression);
        }
        // 直に変換はできない場合。
        else
        {
            // detonatorをrecepter.Reactorに渡す構文木。
            Expression? expression = null;

            foreach (var inheritedT in parameterT.GetInheritedTypes())
            {
                // キャスト演算子を探す。
                foreach (var methodI in inheritedT.GetMethods(OPERATOR_FLAGS))
                {
                    if (methodI.Name != IMPLICIT_LITERAL) continue;
                    var parameters = methodI.GetParameters();
                    if (parameters.Length != 1) continue;
                    if (parameters[0].ParameterType != inheritedT && parameters[0].ParameterType != typeof(Nullable<>).MakeGenericType(inheritedT)) continue;

                    // null許容値型を返す場合。
                    if (methodI.ReturnType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        if (!methodI.ReturnType.GenericTypeArguments.Single().IsAssignableTo(targetT)) continue;
                    }
                    else
                    {
                        if (!methodI.ReturnType.IsAssignableTo(targetT)) continue;
                    }

                    // キャスト演算子をif文でつなげる。
                    expression = Expression.IfThen(Expression.TypeIs(parameterExpression, inheritedT), expression ?? Expression.Empty());
                    break;
                }
            }

            if (parameterT.GetInheritedTypes().Any(type => type.IsAssignableTo(targetT)))
            {
                // is演算でどうにかなるかもしれない場合。
                yield return expression == null
                    ? Expression.IfThen(Expression.TypeIs(parameterExpression, targetT), Expression.Invoke(Expression.Constant(target), Expression.Convert(parameterExpression, targetT)))
                    : Expression.IfThenElse(Expression.TypeIs(parameterExpression, targetT), Expression.Invoke(Expression.Constant(target), Expression.Convert(parameterExpression, targetT)), expression);
            }
            else
            {
                // is演算ではどうにもならない場合。
                if (expression != null) yield return expression;
            }
        }
    }

    protected static Expression? GetSentence(Expression parameterExpression, Delegate target)
    {
        const string IMPLICIT_LITERAL = "op_Implicit";
        const BindingFlags OPERATOR_FLAGS = BindingFlags.Public | BindingFlags.Static;

        var parameterT = parameterExpression.Type;

        var targetArguments = target.GetType().GetGenericArguments();
        if (targetArguments.Length != 1) throw new ArgumentException("代理人のシグネチャが正しくありません。型引数を一つ持つ代理人を指定してください。");
        var targetT = targetArguments[0];

        // 直に変換ができる場合。
        if (parameterT.IsAssignableTo(targetT))
        {
            return Expression.Invoke(Expression.Constant(target), parameterExpression);
            //yield return Expression.Call(Expression.Constant(recepter), targetMethodName, new Type[1] { targetType }, detonatorExpression);
        }
        // 直に変換はできない場合。
        else
        {
            // detonatorをrecepter.Reactorに渡す構文木。
            Expression? expression = null;

            foreach (var inheritedT in parameterT.GetInheritedTypes())
            {
                // キャスト演算子を探す。
                foreach (var methodI in inheritedT.GetMethods(OPERATOR_FLAGS))
                {
                    if (methodI.Name != IMPLICIT_LITERAL) continue;
                    var parameters = methodI.GetParameters();
                    if (parameters.Length != 1) continue;
                    if (parameters[0].ParameterType != inheritedT && parameters[0].ParameterType != typeof(Nullable<>).MakeGenericType(inheritedT)) continue;

                    // null許容値型を返す場合。
                    if (methodI.ReturnType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        if (!methodI.ReturnType.GenericTypeArguments.Single().IsAssignableTo(targetT)) continue;
                    }
                    else
                    {
                        if (!methodI.ReturnType.IsAssignableTo(targetT)) continue;
                    }

                    // キャスト演算子をif文でつなげる。
                    expression = Expression.IfThen(Expression.TypeIs(parameterExpression, inheritedT), expression ?? Expression.Empty());
                    break;
                }
            }

            if (parameterT.GetInheritedTypes().Any(type => type.IsAssignableTo(targetT)))
            {
                // is演算でどうにかなるかもしれない場合。
                return expression == null
                    ? Expression.IfThen(Expression.TypeIs(parameterExpression, targetT), Expression.Invoke(Expression.Constant(target), Expression.Convert(parameterExpression, targetT)))
                    : Expression.IfThenElse(Expression.TypeIs(parameterExpression, targetT), Expression.Invoke(Expression.Constant(target), Expression.Convert(parameterExpression, targetT)), expression);
            }
            else
            {
                // is演算ではどうにもならない場合。
                if (expression != null) return expression;
            }
        }

        return null;
    }
}

/// <summary>
/// 代理人への接続を中継する中継器を表します。
/// <para>
/// このクラスの包括サイズは72バイトです。
/// </para>
/// </summary>
/// <typeparam name="T">
/// 中継する型。
/// </typeparam>
public class Relay<T> : Relay
{
    /// <summary>
    /// 中継器を初期化します。
    /// </summary>
    /// <param name="detonatorType">
    /// 中継する型。
    /// </param>
    public Relay() : base(typeof(T)) { }

    /// <summary>
    /// 中継をする代理人。
    /// </summary>
    /// <param name="courier"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new void Delegate(T courier) => ((Action<T>)_delegate)(courier);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Obsolete("`IReadOnlySet<T>`を引数とするメソッドに置き換え可能だから。")]
    public override void Target(ISet<Delegate> targets) => Target(targets, typeof(T));
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Target(IReadOnlySet<Delegate> targets) => Target(targets, typeof(T));
}

/// <summary>
/// 代理人への接続を中継する中継器を表します。
/// <para>
/// この型のインスタンスに対する<see cref="Target(IReadOnlySet{object})"/>の呼び出しは、<see cref="Relay.Target(IReadOnlySet{object})"/>の呼び出しに対して数倍高速です。
/// しかし、この型のインスタンスは多くのメモリ領域を必要とします。
/// </para>
/// </summary>
/// <typeparam name="T">
/// 中継する型。
/// </typeparam>
public class CachedRelay<T> : Relay<T>
{
    protected readonly Dictionary<Delegate, Expression[]> _sentencesCache;
    protected readonly ParameterExpression _detonatorExpression;

    /// <summary>
    /// 中継器を初期化します。
    /// </summary>
    /// <param name="detonatorType">
    /// 中継する型。
    /// </param>
    public CachedRelay()
    {
        _sentencesCache = new();
        _detonatorExpression = Expression.Parameter(typeof(T), "detonator");
    }

    /// <summary>
    /// 中継先を指定します。既存の指定先は破棄されます。
    /// <para>
    /// このメソッドの呼び出しは、<see cref="Relay.Target(IReadOnlySet{object})"/>の呼び出しに対して数倍高速です。
    /// </para>
    /// </summary>
    /// <param name="targets">
    /// 指定する中継先。
    /// </param>
    [Obsolete("`IReadOnlySet<T>`を引数とするメソッドに置き換え可能だから。")]
    public override void Target(ISet<Delegate> targets) => Target((IReadOnlySet<Delegate>)targets);
    /// <summary>
    /// 中継先を指定します。既存の指定先は破棄されます。
    /// <para>
    /// このメソッドの呼び出しは、<see cref="Relay.Target(IReadOnlySet{object})"/>の呼び出しに対して数倍高速です。
    /// </para>
    /// </summary>
    /// <param name="targets">
    /// 指定する中継先。
    /// </param>
    public override void Target(IReadOnlySet<Delegate> targets)
    {
        // キャッシュから今後使用しないものを除く。
        foreach (var target in _sentencesCache.Keys.Except(targets))
        {
            _count -= _sentencesCache[target].Length;
            _ = _sentencesCache.Remove(target);
        }

        // 全ての、追加すべき中継先について、
        foreach (var target in targets.Except(_sentencesCache.Keys))
        {
            var sentences = GetSentences(_detonatorExpression, target);
            if (sentences.Any())
            {
                Expression[] sentences_ = sentences.ToArray();

                _count += sentences_.Length;
                _sentencesCache.Add(target, sentences_);
            }
        }

        // コンパイルして納める。
        _delegate = Expression.Lambda<Action<T>>(Expression.Block(from sentence in _sentencesCache.Values from expression in sentence select expression), _detonatorExpression).Compile();
    }

    public override string ToString() => Expression.Block(from sentence in _sentencesCache.Values from expression in sentence select expression).ToString();
}

public delegate void RelayTarget<in T>(T parameter);
