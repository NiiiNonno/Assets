using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
#if USE_DOUBLE
using Dec = System.Double;
#else
using Dec = System.Single;
#endif
using SystemTimer = System.Timers.Timer;

namespace Nonno.Assets;

public interface ITime
{
    event Action? Elapsed;
    ulong CurrentTicks { get; }
    ITimeToken GetToken();
}

public interface ITimeToken
{

}

/// <summary>
/// 物理計測時間を表します。
/// <para>
/// 多くの場合、この時間の最小単位は物理的な1秒で、分解能はおよそ10㍉秒です。
/// </para>
/// <para>
/// この時間の原点は、<see cref="Start"/>メソッド呼び出し時点です。
/// </para>
/// </summary>
public class PhysicalTime : ITime
{
    readonly Stopwatch _stopwatch;
    ulong _currentTicks;

    public event Action? Elapsed;
    public ulong CurrentTicks => _currentTicks;

    public PhysicalTime()
    {
        _stopwatch = new();
    }

    public Token GetToken() => new(_stopwatch.Elapsed);
    ITimeToken ITime.GetToken() => GetToken();

    public async void Start()
    {
        _stopwatch.Restart();

        while (true)
        {
            var ticks = ++_currentTicks * 10000000;
            if ((ulong)_stopwatch.ElapsedTicks < ticks)
            {
                await Task.Delay(new TimeSpan((long)ticks - _stopwatch.ElapsedTicks));
            }

            Elapsed?.Invoke();
        }
    }

    public readonly struct Token : ITimeToken
    {
        readonly TimeSpan _elapsed;

        public Token(TimeSpan elapsed) => _elapsed = elapsed;

        public Dec GetDifference(Token token) => (Dec)(token._elapsed - _elapsed).TotalSeconds;

        public TimeSpan GetProximateTimeSpan() => _elapsed;
    }
}

/// <summary>
/// 機械計測時間を表します。
/// <para>
/// 多くの場合、この時間の最小単位はランタイムと同じで、分解能はおよそ10㍉秒です。
/// </para>
/// <para>
/// この時間の原点は、プロセスが動作するハードウェアの起動時点です。
/// </para>
/// </summary>
public class MachineTime : ITime
{
    readonly SystemTimer _timer;
    ulong _tickCount;

    public event Action? Elapsed;
    public ulong CurrentTicks => _tickCount;

    MachineTime()
    {
        _timer = new(1);

        _timer.Elapsed += Timer_Elapsed;
    }
    public Token GetToken() => new(_tickCount);
    ITimeToken ITime.GetToken() => GetToken();

    private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        var tickCount = (ulong)Environment.TickCount64;
        int l = (int)(tickCount - _tickCount);
        _tickCount = tickCount;
        for (int i = 0; i < l; i++)
        {
            Elapsed?.Invoke();
        }
    }

    public readonly struct Token : ITimeToken
    {
        readonly ulong _tickCount;

        public Token(ulong tickCount)
        {
            _tickCount = tickCount;
        }
    }
}

/// <summary>
/// 時代計測時間を表します。
/// <para>
/// 多くの場合、この時間の最小単位はおよそ2時間で、分解能は10㍉秒です。
/// </para>
/// <para>
/// この時間の原点は、時代を象徴する出来事の発生時点です。
/// </para>
/// </summary>
public class EraTime : ITime
{
    public event Action? Elapsed;
    public ulong CurrentTicks => unchecked((ulong)DateTime.Now.Ticks);

    public Token GetToken() => new();
    ITimeToken ITime.GetToken() => GetToken();

    public readonly struct Token : ITimeToken
    {

    }
}

/// <summary>
/// 西暦計測時間を表します。
/// <para>
/// 多くの場合、この時間の最小単位はおよそ2時間で、分解能は10㍉秒です。
/// </para>
/// <para>
/// この時間の原点は、西暦1年1月1日の午前0時0分0秒0厘0毛です。
/// </para>
/// </summary>
public class DominicalTime : EraTime
{
}

/// <summary>
/// 演算計測時間を表します。
/// <para>
/// 多くの場合、この時間の最小単位は60㍉秒で、分解能は不定です。
/// </para>
/// <para>
/// この時間の原点は、<see cref="Start"/>
/// </para>
/// </summary>
public class OperatingTime : ITime
{
    ulong _currentTicks;

    public event Action? Elapsed;
    public ulong CurrentTicks => _currentTicks;

    public OperatingTime()
    {
    }

    public Token GetToken() => new();
    ITimeToken ITime.GetToken() => GetToken();

    [DoesNotReturn]
    public void Start()
    {
        while (true)
        {
            _currentTicks++;

            Elapsed?.Invoke();
        }
    }

    public readonly struct Token : ITimeToken
    {

    }
}

public class FrameTime
{

}

[Obsolete("現在は`ITimer`を使用する方法が推奨されています。")]
public class Time
{
    int _ticks;
    DateTime _origin;

    /// <summary>
    /// この時間を制御するタイマーオブジェクトを標識として指定します。
    /// </summary>
    public object Timer { get; set; }
    public int Ticks => _ticks;
    /// <summary>
    /// 現在の時刻を取得します。
    /// </summary>
    public DateTime Now => _origin + new System.TimeSpan(_ticks);
    public event ElapsedEventHandler? Elapsed;

    public Time(object timer, DateTime? now = null)
    {
        _origin = now ?? DateTime.Now;

        Timer = timer;
    }

    public unsafe void Forward(int count)
    {
        unchecked
        {
            int tick = _ticks + count;
            if (tick < 0)
            {
                _ticks = tick.ReverseSign();
                _origin += UNIT;
            }
            else
            {
                _ticks = tick;
            }

            Elapsed?.Invoke(this, new(count, _ticks));
        }
    }

    static System.TimeSpan UNIT { get; } = new(0x8000_0000);

    public static Time Main { get; }

    static Time()
    {
        var timer = new System.Timers.Timer(1);
        Main = new Time(timer);
        timer.Elapsed += (_, e) => Main.Forward((int)(e.SignalTime - Main.Now).Ticks);
        timer.Start();
    }
}

[Obsolete("現在は`ITimer`を使用する方法が推奨されています。")]
public class TimeFlows
{
    readonly Time _time;
    readonly List<SynchronousSet> _synchronousSets;

    public TimeFlows(Time time)
    {
        _time = time;
        _synchronousSets = new();
    }

    public bool Flow(IFlowable flowable)
    {
        var interval = (int)(flowable.INTERVAL * 10000000);

        SynchronousSet? set = null;

        foreach (var aESet in _synchronousSets)
        {
            if (aESet.Interval == interval)
            {
                set = aESet;
            }

            if (aESet.Contains(flowable)) return false;
        }

        if (set == null)
        {
            set = new(interval);
            _time.Elapsed += set.OnElapsed;
            _synchronousSets.Add(set);
        }

        set.Add(flowable);

        return true;
    }

    public bool Land(IFlowable flowable)
    {
        for (int i = 0; i < _synchronousSets.Count; i++)
        {
            if (_synchronousSets[i].Remove(flowable))
            {
                if (_synchronousSets[i].Flowings == 0)
                {
                    _synchronousSets.RemoveAt(i);
                }

                return true;
            }
        }

        return false;
    }

    class SynchronousSet
    {
        readonly int _interval;
        int _counter;
        readonly List<IFlowable> _flowings;

        public int Interval => _interval;
        public int Flowings => _flowings.Count;

        public SynchronousSet(int interval)
        {
            _interval = interval;
            _flowings = new();
        }

        public void OnElapsed(object _, ElapsedEventArgs e)
        {
            _counter += e.ElapsedTicks;

            if (_counter > _interval)
            {
                var count = _counter / _interval;
                _counter -= count * _interval;

                foreach (var flowing in _flowings) flowing.Progress(count);
            }
        }

        public void Add(IFlowable flowable) => _flowings.Add(flowable);

        public bool Remove(IFlowable flowable) => _flowings.Remove(flowable);

        public bool Contains(IFlowable flowable) => _flowings.Contains(flowable);
    }

    public static TimeFlows OfMain { get; }

    static TimeFlows()
    {
        OfMain = new(Time.Main);
    }
}

[Obsolete("現在は`ITimer`を使用する方法が推奨されています。")]
public interface IFlowable
{
    decimal INTERVAL { get; }
    void Progress(int count);
}

[Obsolete("現在は`ITimer`を使用する方法が推奨されています。")]
public delegate void ElapsedEventHandler(object sender, ElapsedEventArgs e);

[Obsolete("現在は`ITimer`を使用する方法が推奨されています。")]
public struct ElapsedEventArgs : IEquatable<ElapsedEventArgs>
{
    public int ElapsedTicks { get; init; }
    public int SignalTicks { get; init; }

    public ElapsedEventArgs(int elapsedTicks, int signalTicks)
    {
        ElapsedTicks = elapsedTicks;
        SignalTicks = signalTicks;
    }

    public override bool Equals(object? obj) => obj is ElapsedEventArgs other && Equals(other);
    public bool Equals(ElapsedEventArgs other) => other == this;
    public bool Equals(ElapsedEventArgs? other) => other == this;

    public override int GetHashCode() => SignalTicks.GetHashCode();

    public override string ToString() => $"{typeof(ElapsedEventArgs).FullName}(ElapsedTicks: {ElapsedTicks}, SignalTicks: {SignalTicks})";

    public static bool operator ==(ElapsedEventArgs left, ElapsedEventArgs right) => left.SignalTicks == right.SignalTicks;
    public static bool operator !=(ElapsedEventArgs left, ElapsedEventArgs right) => left.SignalTicks != right.SignalTicks;
    public static bool operator <(ElapsedEventArgs left, ElapsedEventArgs right) => left.SignalTicks < right.SignalTicks;
    public static bool operator >(ElapsedEventArgs left, ElapsedEventArgs right) => left.SignalTicks > right.SignalTicks;
}
