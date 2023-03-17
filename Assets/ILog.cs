using System;
using System.Collections.Immutable;
using System.Reflection;
using static Nonno.Assets.Utils;

namespace Nonno.Assets;

public interface ILog
{
    string? Sender { get; }
    ImmutableArray<ColoredCharacter> Message { get; }
    LongIdentifier<ILog> Identifier { get; }
}

public record Log(string? Sender, ImmutableArray<ColoredCharacter> Message, LongIdentifier<ILog> Identifier) : ILog
{
    public Log(ImmutableArray<ColoredCharacter> message) : this(null, message) { }
	public Log(string? sender, ImmutableArray<ColoredCharacter> message) : this(sender, message, LongIdentifier<ILog>.GetNew()) { }
	public Log(string text, BasicColor foregroundColor = BasicColor.None, BasicColor backgroundColor = BasicColor.None) : this(text.ToColoredString(foregroundColor, backgroundColor)) { }

	public static Log Empty { get; } = new(default, ImmutableArray<ColoredCharacter>.Empty, default);
}

public readonly record struct ValueLog(string? Sender, ImmutableArray<ColoredCharacter> Message, LongIdentifier<ILog> Identifier) : ILog
{
	public ValueLog(ImmutableArray<ColoredCharacter> message) : this(null, message) { }
	public ValueLog(string? sender, ImmutableArray<ColoredCharacter> message) : this(sender, message, LongIdentifier<ILog>.GetNew()) { }
	public ValueLog(string text, BasicColor foregroundColor = BasicColor.None, BasicColor backgroundColor = BasicColor.None) : this(text.ToColoredString(foregroundColor, backgroundColor)) { }

	public static Log Empty { get; } = new(default, ImmutableArray<ColoredCharacter>.Empty, default);
}

public record ExceptionLog(Exception Exception, LongIdentifier<ILog> Identifier) : ILog
{
    public string? Sender => Exception.TargetSite is MethodBase site ? site.DeclaringType is Type type ? $"{Exception.Source}内{type.FullName}ノ{site.Name}行程" : $"{site.Name}行程({Exception.Source})" : $"行程({Exception.Source})";
	public ImmutableArray<ColoredCharacter> Message
    {
        get
        {
            var builder = GetColoredStringBuilder();
            builder.Append(Exception switch
            {
                ObjectDisposedException => "破損不能異常",
                InvalidOperationException => "状況不能異常",
                NullReferenceException => "無照異常",
                ArgumentNullException => "無照例外",
                ArgumentOutOfRangeException => "外番号例外",
                ArgumentException => "例外",
                IndexOutOfRangeException => "外番号異常",
                NotImplementedException => "未定例外",
                PlatformNotSupportedException => "境地不能例外",
                NotSupportedException => "環境不能例外",
                DirectoryNotFoundException => "謎科例外",
                FileNotFoundException => "謎品例外",
                DivideByZeroException => "空零除算例外",
                UriFormatException => "地址形式例外",
                FormatException => "形式例外",
                KeyNotFoundException => "謎例外",
                OverflowException => "不足木例外",
                PathTooLongException => "過長科品例外",
                RankException => "次数齟齬",
				TimeoutException => "時間超過例外",
				NotFiniteNumberException => "無限値例外",
				InvalidCastException => "不可換異常",
				EndOfStreamException => "文末超過例外",
				_ => "異常",
            }, BasicColor.None, BasicColor.Red);
            builder.Append('\t');
            builder.Append(Exception.Message, BasicColor.Red);
			builder.Append('\r');
			builder.Append('\n');
            builder.Append(Exception.StackTrace, BasicColor.ThinRed);
            return builder.ToImmutable();
        }
    }

    public ExceptionLog(Exception exception) : this(exception, LongIdentifier<ILog>.GetNew())
    {
    }
}
