// 令和弐年大暑確認済。
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nonno.Assets;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public sealed class MarkAttribute : Attribute
{
    public string Text { get; }

    public MarkAttribute(string text)
    {
        Text = text;
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class CommandInterpreterAttribute : Attribute
{
    public string SystemName { get; }
    public string Target { get; }
    public string[]? Args { get; }

    /// <summary>
    /// コマンド解釈メソッド属性を付与します。
    /// </summary>
    /// <param name="systemName">
    /// コマンド処理体系。
    /// </param>
    /// <param name="target">
    /// コマンド名。
    /// </param>
    /// <param name="args">
    /// 考えられる引数名。引数の検査を使用しないときは<c>null</c>。
    /// <para>
    /// 引数の値に候補または選択肢を示すとき、引数名の後に<c>"(候補または選択肢,候補または選択肢,...)"</c>とします。
    /// </para>
    /// <para>
    /// 候補または選択肢の値はその値のほかに以下に示す文字列または属性の解釈系独自の文字列を使用できます。<c>"["</c>、<c>"]"</c>のエスケープ文字はそれぞれ<c>"[["</c>、<c>"]]"</c>です。
    /// <list type="bullet">
    /// <item><c>[others]</c>: 全ての値をとるその他の値を許容することを表します。</item>
    /// <item><c>[integar]</c>: 全ての整数を許容することを表します。</item>
    /// <item><c>[integar(n..m)]</c>: nからmまでの整数を許容することを表します。</item>
    /// <item><c>[decimal]</c>: 全ての実数を許容することを表します。</item>
    /// <item><c>[decimal(n..m)]</c>: nからmまでの実数を許容することを表します。</item>
    /// <item><c>[path]</c>: 存在するパスを許容することを表します。</item>
    /// <item><c>[uri]</c>: 統一資源位置指定子を許容することを表します。</item>
    /// </list>
    /// </para>
    /// </param>
    public CommandInterpreterAttribute(string systemName, string target, params string[]? args)
    {
        SystemName = systemName;
        Target = target;
        Args = args;
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class IRMethodAttribute : Attribute
{
}

partial class Utils
{
    public static IEnumerable<string> GetMarks(this MemberInfo @this) => @this.GetCustomAttributes<MarkAttribute>().Select(x => x.Text);
}
