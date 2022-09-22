using System.Reflection;

namespace Nonno.Assets;

public interface ILog : IEquatable<ILog>, IRecognizable<ILog>
{
    string? Sender { get; }
    string Message { get; }
    ConsoleColor? Color { get; }
    bool IEquatable<ILog>.Equals(ILog? other) => other != null && other.GetLongIdentifier() == GetLongIdentifier();

    public static Action<ILog> WriteDelegate { get; set; } = log =>
    {
        Console.ResetColor();
        if (log.Color.HasValue)
        {
            Console.ForegroundColor = log.Color.Value;
            if (log.Sender is string sender)
            {
                Console.Write(sender);
            }
            Console.Write(": ");
            Console.WriteLine(log.Message);
            Console.ResetColor();
        }
        else
        {
            if (log.Sender is string sender)
            {
                Console.Write(sender);
            }
            Console.Write(": ");
            Console.WriteLine(log.Message);
        }
    };

    public static void Write(ILog log) => WriteDelegate(log);
    public static void Write(string message, ConsoleColor? color = null) => WriteDelegate(new ValueLog(null, message, color));
    public static void Write(string? sender, string message, ConsoleColor? color = null) => WriteDelegate(new ValueLog(sender, message, color));
    public static void Write(object? obj, string? sender = null) => WriteDelegate(new ValueLog(sender, obj == null ? "null" : obj.ToString() ?? String.Empty, null));
}

public class Log : ILog
{
    public LongIdentifier<ILog> Identifier { get; }
    public string? Sender { get; }
    public string Message { get; }
    public ConsoleColor? Color { get; }

    public Log(string? sender, string message, ConsoleColor? color)
    {
        Identifier = LongIdentifier<ILog>.GetNew();
        Sender = sender;
        Message = message;
        Color = color;
    }

    public ShortIdentifier<ILog> GetShortIdentifier() => (ShortIdentifier<ILog>)Identifier;
    public LongIdentifier<ILog> GetLongIdentifier() => Identifier;
    public UniqueIdentifier<ILog> GetUniqueIdentifier() => Identifier;

    /*
     * ILog.Writeなどと呼び出すのはどうにも変なのでLogの方にも同じ務容を用意してみた。要るかな？
     */

    public static void Write(ILog log) => ILog.Write(log);
    public static void Write(string message, ConsoleColor? color = null) => ILog.Write(message, color);
    public static void Write(string? sender, string message, ConsoleColor? color = null) => ILog.Write(sender, message, color);
    public static void Write(object? obj, string? sender = null) => ILog.Write(obj, sender);
}

public struct ValueLog : ILog
{
    public LongIdentifier<ILog> Identifier { get; }
    public string? Sender { get; }
    public string Message { get; }
    public ConsoleColor? Color { get; }

    public ValueLog(string? sender, string message, ConsoleColor? color)
    {
        Identifier = LongIdentifier<ILog>.GetNew();
        Sender = sender;
        Message = message;
        Color = color;
    }

    public ShortIdentifier<ILog> GetShortIdentifier() => (ShortIdentifier<ILog>)Identifier;
    public LongIdentifier<ILog> GetLongIdentifier() => Identifier;
    public UniqueIdentifier<ILog> GetUniqueIdentifier() => Identifier;
}

public class ExceptionLog : ILog
{
    public ShortIdentifier<ILog> Identifier { get; }
    public string? Sender { get; }
    public string Message { get; }
    public ConsoleColor? Color { get; }

    public ExceptionLog(Exception exception)
    {
        Identifier = new((uint)exception.GetHashCode());
        Sender = exception.TargetSite is MethodBase site ? site.DeclaringType is Type type ? $"Process[{type.FullName}.{site.Name}]" : $"Process[{site.Name}]" : "Process";
        Message = $"{exception.GetType().Name}({exception.Message})";
        Color = ConsoleColor.Red;
    }
    public ShortIdentifier<ILog> GetShortIdentifier() => Identifier;
    public LongIdentifier<ILog> GetLongIdentifier() => Identifier;
    public UniqueIdentifier<ILog> GetUniqueIdentifier() => Identifier;
}
