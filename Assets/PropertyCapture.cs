using System.Reflection;
using System.Runtime.CompilerServices;

namespace Nonno.Assets;

/*
 * Blazorで使っていたけれど今はもう使わない。
 */

public class PropertyCapture<T>
{
    readonly Func<T>? _getter;
    readonly Action<T>? _setter;

    public T Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            try
            {
                return _getter!();
            }
            catch (NullReferenceException e)
            {
                throw new NotSupportedException("捕獲しているプロパティはゲッターをサポートしません。", e);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            try
            {
                _setter!(value);
            }
            catch (NullReferenceException e)
            {
                throw new NotSupportedException("捕獲しているプロパティはセッターをサポートしません。", e);
            }
        }
    }

    public PropertyCapture(object target, string propertyName)
    {
        PropertyInfo? info = target.GetType().GetProperty(propertyName, BindingFlags.Instance);
        if (info == null) throw new ArgumentException($"{target.GetType()}に{propertyName}の定義がありません。", nameof(propertyName));
        if (info.PropertyType != typeof(T)) throw new ArgumentException("引数の情報が示すパラメーターの型は、ジェネリックの型と異なります。", nameof(propertyName));

        _getter = info.GetGetMethod()?.CreateDelegate<Func<T>>(target);
        _setter = info.GetSetMethod()?.CreateDelegate<Action<T>>(target);
    }
    public PropertyCapture(PropertyInfo info, object? target)
    {
        if (info.PropertyType != typeof(T)) throw new ArgumentException("引数の情報が示すパラメーターの型は、ジェネリックの型と異なります。", nameof(info));

        if (target != null && info.DeclaringType is Type declaringT)
        {
            if (!declaringT.IsAssignableFrom(target.GetType())) throw new ArgumentException("指定されたプロパティは対象のメンバではありません。", nameof(info));
        }

        _getter = info.GetGetMethod()?.CreateDelegate<Func<T>>(target);
        _setter = info.GetSetMethod()?.CreateDelegate<Action<T>>(target);
    }
    public PropertyCapture(Func<T>? getter, Action<T>? setter)
    {
        _getter = getter;
        _setter = setter;
    }

    /// <summary>
    /// ゲッターを取得します。
    /// </summary>
    /// <returns>
    /// ゲッターが定義されている場合はゲッターのメソッドを、定義されていない場合は<see cref="null"/>を返します。
    /// </returns>
    public Func<T>? GetGetMethod() => _getter;
    /// <summary>
    /// セッターを取得します。
    /// </summary>
    /// <returns>
    /// セッターが定義されている場合はセッターのメソッドを、定義されていない場合は<see cref="null"/>を返します。
    /// </returns>
    public Action<T>? GetSetMethod() => _setter;
}
