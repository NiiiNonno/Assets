// 令和弐年大暑確認済。
using System.Reflection;

namespace Nonno.Assets;

/// <summary>
/// 任意の文に辯を標します。
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public sealed class MarkAttribute : Attribute
{
    public string Text { get; }

    public MarkAttribute(string text)
    {
        Text = text;
    }
}

/// <summary>
/// 令務容を標します。
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class CommandInterpreterAttribute : Attribute
{
    public string SystemName { get; }
    public string Target { get; }
    public string[]? Args { get; }

    /// <summary></summary>
    /// <param name="systemName">
    /// 伝令統。
    /// </param>
    /// <param name="target">
    /// 令号。
    /// </param>
    /// <param name="args">
    /// 考えられる引謄名。引謄の検査を使用しないときは<c>null</c>。
    /// <para>
    /// 引謄に候補または選択肢を示すとき、引謄名の後に<c>"(候補または選択肢,候補または選択肢,...)"</c>とします。
    /// </para>
    /// <para>
    /// 候補または選択肢はその言のほかに以下に示す字符号列または属性の伝令統独自の字符号列を使用できます。<c>"["</c>、<c>"]"</c>のエスケープ文字はそれぞれ<c>"[["</c>、<c>"]]"</c>です。
    /// <list type="bullet">
    /// <item><c>[others]</c>: 全ての値をとるその他の値を許容することを表します。</item>
    /// <item><c>[integar]</c>: 全ての整数を許容することを表します。</item>
    /// <item><c>[integar(n..m)]</c>: nからmまでの整数を許容することを表します。</item>
    /// <item><c>[decimal]</c>: 全ての実数を許容することを表します。</item>
    /// <item><c>[decimal(n..m)]</c>: nからmまでの実数を許容することを表します。</item>
    /// <item><c>[path]</c>: 存在する科品を許容することを表します。</item>
    /// <item><c>[uri]</c>: 統一資源地址を許容することを表します。</item>
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

/// <summary>
/// 挿搴務容を標します。
/// </summary>
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

/// <summary>
/// 
/// </summary>
/// <remarks>
/// 引数の作成には以下のコードを参考にしてください。
/// <code>
/// var r = System.Security.Cryptography.RandomNumberGenerator.Create();
/// string F(System.Security.Cryptography.RandomNumberGenerator r) { var a = new byte[16]; r.GetBytes(a); return $"{BitConverter.ToUInt32(a, 0):X8}-{BitConverter.ToUInt32(a, 4):X8}-{BitConverter.ToUInt32(a, 8):X8}-{BitConverter.ToUInt32(a, 12):X8}:System.Type"; }
/// </code>
/// </remarks>
[Obsolete("泛型符の活用により個別の型符定義は不要となった。")]
[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
public class TypeIdentifierAttribute : Attribute
{
    public Type Type { get; }
    public UniqueIdentifier<Type> Identifier { get; }

    protected TypeIdentifierAttribute(Type type, UniqueIdentifier<Type> identifier)
    {
        Type = type;
        Identifier = identifier;
    }
    public TypeIdentifierAttribute(Type type, string idString)
    {
        if (type.IsGenericTypeDefinition) throw new ArgumentException("型が泛型定義でした。具体型を指定してください。", nameof(type));

        Type = type;
        Identifier = new UniqueIdentifier<Type>(idString);
    }

    public KeyValuePair<Type, UniqueIdentifier<Type>> ToKeyValuePair() => new(Type, Identifier);
}

[Obsolete("泛型符の活用により個別の型符定義は不要となった。")]
[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
public sealed class TypeIdentifierAttribute<T> : TypeIdentifierAttribute
{
    public TypeIdentifierAttribute(UniqueIdentifier<Type> identifier) : base(typeof(T), identifier)
    {
        
    }
    public TypeIdentifierAttribute(string idString) : this(new UniqueIdentifier<Type>(idString))
    {

    }
}

partial class Utils
{
    public static IEnumerable<string> GetMarks(this MemberInfo @this) => @this.GetCustomAttributes<MarkAttribute>().Select(x => x.Text);
}
