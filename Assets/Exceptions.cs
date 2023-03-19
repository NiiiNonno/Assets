// 令和弐年大暑確認済。
using System.Reflection;
using Nonno.Assets.Scrolls;

namespace Nonno.Assets;

/// <summary>
/// 令謄例外を表します。
/// </summary>
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

    public override string ToString() => $"{Message}\n\t因 {Cause}\n\t構文 {Case}\n\t原異常 {InnerException}";
}

public enum CommandLineArgumentsExceptionCause
{
    None,
    Missing,
    Excess,
}

/// <summary>
/// 認証例外、即ち主に数據の処理において、数據の勘合不整の例外を表します。
/// <para>
/// 数據整合性の確認をとる一般的な手法の一つであり、或は個別の対応が必要です。
/// </para>
/// </summary>
[Serializable]
public class AuthenticationException : Exception
{
    public object? AuthenticatedToken { get; init; }
    public object? ValidToken { get; init; }

    public AuthenticationException(object authenticatedToken, object validToken) : base($"\"{authenticatedToken}\"を勘合とする認証に失敗しました。あるべき、または最も有効な勘合は\"{validToken}\"です。")
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
/// 謎異常、即ち品目が未定義か、曖昧、又は無効である異常を表します。
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
    public UndefinedEnumerationValueException(string paramName, Type enumeraionType) : base($"`{paramName}`に指定された品目は`{enumeraionType.FullName}`に未定義か、曖昧、又は無効です。", paramName)
    {
        EnumerationType = enumeraionType;
    }
    protected UndefinedEnumerationValueException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

/// <summary>
/// 無効属性異常を表します。
/// </summary>
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

[Obsolete("請、他例外に代替。")]
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