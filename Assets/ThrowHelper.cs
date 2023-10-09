using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets;
public static class ThrowHelper
{
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ArgumentIsNotASCII(
        object argument,
        [CallerArgumentExpression(nameof(argument))] string cAE = "",
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new ArgumentException($"{cAE} ノ引謄 {argument} 非有効アスキー字符号。於 {cFP} ノ {cMN} ノ {cLN} 行目。");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ArgumentIsNotLatin1(
        object argument,
        [CallerArgumentExpression(nameof(argument))] string cAE = "",
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new ArgumentException($"{cAE} ノ引謄 {argument} 非有効ラテン第一字符号。於 {cFP} ノ {cMN} ノ {cLN} 行目。");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ArgumentTypeIsNotAssignable(
        object argument,
        Type to,
        [CallerArgumentExpression(nameof(argument))] string cAE = "",
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new ArgumentException($"{cAE} ノ引型 {argument} 非 {to} 。於 {cFP} ノ {cMN} ノ {cLN} 行目。");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void TermIsUsed(
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new InvalidOperationException($"項被演算已無効。於 {cFP} ノ {cMN} ノ {cLN} 行目。");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void FailToWriteBytes(
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new Exception($"至十数列への書込みに失敗しました。於 {cFP} ノ {cMN} ノ {cLN} 行目。");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void InvalidArgumentFormat(
        object argument,
        [CallerArgumentExpression(nameof(argument))] string cAE = "",
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new ArgumentException($"{cAE} ノ引謄 {argument} 無効。於 {cFP} ノ {cMN} ノ {cLN} 行目。", new FormatException());

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void InvalidOperation(
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new InvalidOperationException($"於 {cFP} ノ {cMN} ノ {cLN} 行目。", new FormatException());

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void StructureSizesDoesNotMatch(
        object argument1,
        object argument2,
        [CallerArgumentExpression(nameof(argument1))] string cAE1 = "",
        [CallerArgumentExpression(nameof(argument2))] string cAE2 = "",
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new InvalidOperationException($"{cAE1} ノ {argument1} と {cAE2} ノ {argument2} と尺寸不揃。於 {cFP} ノ {cMN} ノ {cLN} 行目。");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void SpanSizeIsTooShort<T>(
        ReadOnlySpan<T> argument,
        [CallerArgumentExpression(nameof(argument))] string cAE = "",
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new InvalidOperationException($"{cAE} ノ {argument.ToString()} 長さが不足。於 {cFP} ノ {cMN} ノ {cLN} 行目。");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ArgumentOutOfRange(
        object argument,
        Range range,
        [CallerArgumentExpression(nameof(argument))] string cAE = "",
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new ArgumentOutOfRangeException($"番号 {cAE} ノ {argument} 外自 {range.Start} 至 {range.End} 之範囲。於 {cFP} ノ {cMN} ノ {cLN} 行目。");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ArgumentIsNotFound(
        object argument,
        Enum @enum,
        [CallerArgumentExpression(nameof(argument))] string cAE = "",
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new ArgumentOutOfRangeException($"{cAE} ノ {argument} 非 {@enum} 。於 {cFP} ノ {cMN} ノ {cLN} 行目。");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ArgumentIsNotFound(
        object argument,
        IEnumerable<object> candidates,
        [CallerArgumentExpression(nameof(argument))] string cAE = "",
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new ArgumentOutOfRangeException($"{cAE} ノ {argument} 非 {string.Join(" 又 ", candidates)} 。於 {cFP} ノ {cMN} ノ {cLN} 行目。");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void FailToGetReflections(
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new Exception($"失敗、調身の取得。於 {cFP} ノ {cMN} ノ {cLN} 行目。");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void FailToGetDatabaseConnections(
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new Exception($"失敗、数據連絡の取得。於 {cFP} ノ {cMN} ノ {cLN} 行目。");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void HaveNotBeenConfigurated(
        object? value,
        [CallerArgumentExpression(nameof(value))] string cAE = "",
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new InvalidOperationException($"{cAE} ノ {value} 不満。於 {cFP} ノ {cMN} ノ {cLN} 行目。");

    public static void ThrowIfNot(bool @this, Func<Exception>? constructor = null)
    {
        if (!@this) throw constructor?.Invoke() ?? new Exception();
    }

    public static void ThrowIf(bool @this, Func<Exception>? constructor = null)
    {
        if (@this) throw constructor?.Invoke() ?? new Exception();
    }
}
