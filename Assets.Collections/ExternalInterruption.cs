using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Nonno.Assets.Collections;
public class ExternalInterruption : Context.Index
{
    readonly string _name;

    public string Name 
    { 
        get => _name; 
        init => _name = value.ToLower(); 
    }

    public ExternalInterruption() : base(Context)
    {
        _name = $"UNKNOWN{(int)this}";
    }

    public static new Context<ExternalInterruption> Context { get; } = new();
}

public class KeyInterruption : ExternalInterruption
{
    ConsoleModifiers _consoleModifiers;

    public bool ShiftModified =>( _consoleModifiers & ConsoleModifiers.Shift) != 0;
    public bool ControlModified => (_consoleModifiers & ConsoleModifiers.Control) != 0;
    public bool AltModified => (_consoleModifiers & ConsoleModifiers.Alt) != 0;

    public void SetModifiers(ConsoleModifiers consoleModifiers)
    {
        _consoleModifiers = consoleModifiers;
    }
}

public class AbsoluteValueInterruption<TValue> : ExternalInterruption where TValue : unmanaged
{
    public new TValue Value { get; set; }
}

public class RelativeValueInterruption : ExternalInterruption
{
    int _value;

    public new int Value => _value;

    public void Add(int value)
    {
        Interlocked.Add(ref _value, value);
    }
}

partial class Utils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TInterruption GetInterruption<TInterruption>(string name) where TInterruption : ExternalInterruption, new()
    {
        foreach (var i in ExternalInterruption.Context.Indexes)
        {
            if (i.Name == name && i is TInterruption r) return r;
        }
        return new TInterruption() { Name = name };
    }
    public static KeyInterruption GetInterruption(ConsoleKey key)
    {
        return GetInterruption<KeyInterruption>(key.ToString());
    }
}
