// 令和弐年大暑確認済。
using System.Reflection;

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

/// <summary>
/// 可搬数據区であることを示し、使用される場合のファイル署名と、区型の名を指示します。
/// <para>
/// 使用できる場面が限られるので、必ずしも付与する必要はありません。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class PortableNetworkDataChunkAttribute : Attribute
{
    public byte[] FormatSignature { get; }
    public ASCIIString Type { get; }

    public PortableNetworkDataChunkAttribute(ulong formatSignature, string type) : this(BitConverter.GetBytes(formatSignature), type) { }
    public PortableNetworkDataChunkAttribute(byte[] formatSignature, string type) : this(formatSignature, (ASCIIString)type) { }
    public PortableNetworkDataChunkAttribute(byte[] formatSignature, ASCIIString type)
    {
        if (formatSignature.Length != 8) throw new ArgumentException("型の署名は八バイトである必要があります。", nameof(formatSignature));
        if (type.Length != 4) throw new ArgumentException("区型は四バイトである必要があります。", nameof(type));

        FormatSignature = formatSignature.AsSpan().ToArray();
        Type = type;
    }
}

[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
public class TypeIdentifierAttribute : Attribute
{
    public Type Type { get; }
    public Guid Identifier { get; }

    protected TypeIdentifierAttribute(Type type, Guid identifier)
    {
        Type = type;
        Identifier = identifier;
    }
    public TypeIdentifierAttribute(Type type, string guidString)
    {
        if (type.IsGenericTypeDefinition) throw new ArgumentException("型が泛型定義でした。具体型を指定してください。", nameof(type));

        Type = type;
        Identifier = new Guid(guidString);
    }

    public KeyValuePair<Guid, Type> ToKeyValuePair() => new(Identifier, Type);
}

[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
public sealed class TypeIdentifierAttribute<T> : TypeIdentifierAttribute
{
    public TypeIdentifierAttribute(Guid identifier) : base(typeof(T), identifier)
    {
        
    }
    public TypeIdentifierAttribute(string guidString) : this(new Guid(guidString))
    {

    }
}

partial class Utils
{
    public static IEnumerable<string> GetMarks(this MemberInfo @this) => @this.GetCustomAttributes<MarkAttribute>().Select(x => x.Text);
}
