using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets;
public static class ThrowHelper
{
    public static void ArgumentIsNotASCII(
        object argument,
        [CallerArgumentExpression(nameof(argument))] string cAE = "",
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new ArgumentException($"{cAE} ノ引謄 {argument} 非有効アスキー字符号。於 {cFP} ノ {cMN} ノ {cLN} 行目。");

    public static void ArgumentIsNotLatin1(
        object argument,
        [CallerArgumentExpression(nameof(argument))] string cAE = "",
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new ArgumentException($"{cAE} ノ引謄 {argument} 非有効ラテン第一字符号。於 {cFP} ノ {cMN} ノ {cLN} 行目。");

    public static void TermIsUsed(
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new InvalidOperationException($"項被演算已無効。於 {cFP} ノ {cMN} ノ {cLN} 行目。");

    public static void FailWriteBytes(
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new Exception($"至十数列への書込みに失敗しました。於 {cFP} ノ {cMN} ノ {cLN} 行目。");

    public static void InvalidArgumentFormat(
        object argument,
        [CallerArgumentExpression(nameof(argument))] string cAE = "",
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new ArgumentException($"{cAE} ノ引謄 {argument} 非有効。於 {cFP} ノ {cMN} ノ {cLN} 行目。", new FormatException());

    public static void StructureSizesDoesNotMatch(
        object argument1,
        object argument2,
        [CallerArgumentExpression(nameof(argument1))] string cAE1 = "",
        [CallerArgumentExpression(nameof(argument2))] string cAE2 = "",
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new InvalidOperationException($"{cAE1} ノ {argument1} と {cAE2} ノ {argument2} と尺寸不揃。於 {cFP} ノ {cMN} ノ {cLN} 行目。");

    public static void SpanSizeIsTooShort<T>(
        ReadOnlySpan<T> argument,
        [CallerArgumentExpression(nameof(argument))] string cAE = "",
        [CallerFilePath] string cFP = "",
        [CallerMemberName] string cMN = "",
        [CallerLineNumber] int cLN = -1) => throw new InvalidOperationException($"{cAE} ノ {argument.ToString()} 長さが不足。於 {cFP} ノ {cMN} ノ {cLN} 行目。");
}
