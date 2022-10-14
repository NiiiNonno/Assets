// 令和弐年大暑確認済。
using System.Reflection;
using Nonno.Assets.Scrolls;

namespace Nonno.Assets;

[Serializable]
public class CommandLineArgumentsException : Exception
{
    public string? Case { get; }
    public CommandLineArgumentsExceptionCause? Cause { get; }

    public CommandLineArgumentsException() { }
    public CommandLineArgumentsException(string message) : base(message) { }
    public CommandLineArgumentsException(string message, Exception inner) : base(message, inner) { }
    protected CommandLineArgumentsException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    public CommandLineArgumentsException(string? @case = null, CommandLineArgumentsExceptionCause cause = default, Exception? inner = null, string? message = null) : base(message, inner)
    {
        Case = @case;
        Cause = cause;
    }

    public override string ToString() => $"コマンドライン引数が不正です。{Message}\n\t原因:{Cause}\n\t構文:{Case}\n\t内部例外:{InnerException}";
}

public enum CommandLineArgumentsExceptionCause
{
    None,
    Missing,
    Excess,
}

/// <summary>
/// 主にデータの処理において、データが対象のものであることを裏付ける印が一致しない例外を表します。
/// <para>
/// データ整合性の確認をとる一般的な手法の一つであり、或は個別の対応が必要です。
/// </para>
/// </summary>
[Serializable]
public class AuthenticationException : Exception
{
    public object? AuthenticatedToken { get; init; }
    public object? ValidToken { get; init; }

    public AuthenticationException(object authenticatedToken, object validToken) : base($"\"{authenticatedToken}\"を印とする認証に失敗しました。あるべき、または最も有効な印は\"{validToken}\"です。")
    {
        AuthenticatedToken = authenticatedToken;
        ValidToken = validToken;
    }
    public AuthenticationException() { }
    public AuthenticationException(string message) : base(message) { }
    public AuthenticationException(string message, Exception inner) : base(message, inner) { }
    protected AuthenticationException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

/// <summary>
/// 指定された列挙値が定義されていないか、曖昧、又は無効である例外を表します。
/// <para>
/// 逆シリアル化時の範囲確認の不足、誤った型変換などの問題を孕み、或は特別な対応が必要です。
/// </para>
/// </summary>
[Serializable]
public class UndefinedEnumerationValueException : ArgumentException
{
    public Type? EnumerationType { get; }

    public UndefinedEnumerationValueException() { }
    public UndefinedEnumerationValueException(string? message = null, string? paramName = null, Type? enumeraionType = null, Exception? inner = null) : base(message, paramName, inner)
    {
        EnumerationType = enumeraionType;
    }
    public UndefinedEnumerationValueException(string paramName, Type enumeraionType) : base($"`{paramName}`に指定された値は`{enumeraionType.FullName}`に存在しないか、曖昧、又は無効です。", paramName)
    {
        EnumerationType = enumeraionType;
    }
    protected UndefinedEnumerationValueException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

[Serializable]
public class InvalidAttributeException : Exception
{
    public MemberInfo? MemberInfo { get; }
    public Attribute? Attribute { get; }

    public InvalidAttributeException(MemberInfo memberInfo, Attribute attribute) : base($"`{memberInfo.DeclaringType?.FullName}.{memberInfo.Name}`に付与された属性`{attribute.GetType().Name}`が無効です。")
    {
        MemberInfo = memberInfo;
        Attribute = attribute;
    }
    public InvalidAttributeException(string message, MemberInfo memberInfo, Attribute attribute) : base(message)
    {
        MemberInfo = memberInfo;
        Attribute = attribute;
    }
    public InvalidAttributeException(string message, MemberInfo memberInfo, Attribute attribute, Exception inner) : base(message, inner)
    {
        MemberInfo = memberInfo;
        Attribute = attribute;
    }
    protected InvalidAttributeException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ScrollDoesNotMatchException : Exception
{
    public IEnumerable<IScroll> Notes { get; init; } = Array.Empty<IScroll>();

    public ScrollDoesNotMatchException() { }
    public ScrollDoesNotMatchException(string message) : base(message) { }
    public ScrollDoesNotMatchException(string message, Exception inner) : base(message, inner) { }
    protected ScrollDoesNotMatchException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}