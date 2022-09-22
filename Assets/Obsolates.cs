using System.Diagnostics;
using MathD = System.Math;
#if USE_DOUBLE
using Dec = System.Double;
using Math = System.Math;
using static System.Double;
#else
#endif

namespace Nonno.Assets;

[Obsolete("現在は`ITime`インターフェース実装クラスの使用が推奨されています。")]
public class WhileTimer : IDisposable
{
    public bool IsEnabled { get; private set; }
    public Stopwatch Stopwatch { get; }
    public event EventHandler? Elapsed;

    public WhileTimer(Stopwatch? stopwatch = null)
    {
        IsEnabled = true;
        Stopwatch = stopwatch ?? new();

        Stopwatch.Restart();
    }

    public async virtual void RunAsync() => await Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
    public virtual void Run()
    {
        while (IsEnabled) Fire();
    }

    public void Stop() => IsEnabled = false;

    protected void Fire() => Elapsed?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!IsEnabled)
        {
            if (disposing)
            {
                // TODO: マネージド状態を破棄します (マネージド オブジェクト)
            }

            // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
            // TODO: 大きなフィールドを null に設定します
            IsEnabled = false;
        }
    }
}

[Obsolete("現在は`ITime`インターフェース実装クラスの使用が推奨されています。")]
public class IntermittentWhileTimer : WhileTimer
{
    public int Timeout { get; set; }

    public IntermittentWhileTimer(Stopwatch? stopwatch = null, int timeout = 0) : base(stopwatch)
    {
        if (timeout < 0) throw new ArgumentOutOfRangeException(nameof(timeout), "引数は非負の値でなければなりません。");

        Timeout = timeout;
    }

    public async override void RunAsync()
    {
        while (IsEnabled)
        {
            Fire();

            await Task.Delay(Timeout);
        }
    }
    public override void Run()
    {
        while (IsEnabled)
        {
            Fire();

            Thread.Sleep(Timeout);
        }
    }
}

[Obsolete("時間計測方法の精度が悪く、同じ構成ではこれ以上の精度を実現できないから。")]
public class Timer : IDisposable
{
    const double R10000000 = 1d / 10000000;
    const double R1000 = 1d / 1000;

    bool _enabled;
    double _next;
    double _interval/*糸秒*/;
    int _sleepTimeout;
    private bool _disposedValue;

    /// <summary>
    /// タイマーが有効であるかどうかを示す値を取得または設定します。
    /// <para>
    /// このプロパティの書き換えは実質<see cref="Start"/>または<see cref="Stop"/>の呼び出しと同じです。
    /// </para>
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set
        {
            switch (_enabled, value)
            {
            case (true, false):
                {
                    Stop();

                    break;
                }
            case (false, true):
                {
                    Start();

                    break;
                }
            default:
                return;
            }
        }
    }
    /// <summary>
    /// タイマーの送信間隔を取得または設定します。
    /// <para>
    /// 単位は[秒]です。
    /// </para>
    /// </summary>
    public double Interval
    {
        get => _interval/*塵秒*/ * 10000000;
        set
        {
            var accuracy = Accuracy;
            _interval/*塵秒*/ = value/*一秒*/ * R10000000;
            Accuracy = accuracy;
        }
    }
    /// <summary>
    /// 送信間隔内に何回現在時刻を確認するかによるタイマーの精度を取得または設定します。
    /// </summary>
    public double Accuracy
    {
        get => _interval/*塵秒*/ / _sleepTimeout/*毛秒*/ * R1000;
        set
        {
            if (!Double.IsNormal(value) || Double.IsNegative(value)) throw new ArgumentException("指定した値が無効です。", nameof(value));
            _sleepTimeout/*毛秒*/ = (int)(_interval/*糸秒*/ / value * R1000);
        }
    }
    /// <summary>
    /// 送信先がなく、タイマーが不要ならばtrue。そうでなければfalse。
    /// </summary>
    public bool IsUseless => Elapsed == null;
    public event ElapsedEventHandler? Elapsed;

    public Timer() : this(0) { }
    public Timer(double interval, double accuracy = 10)
    {
        _interval = interval * 10000000;

        Accuracy = accuracy;
    }

    public void Start()
    {
        if (_enabled) return;

        _enabled = true;

        Enter();
    }

    public void Stop()
    {
        if (!_enabled) return;

        _enabled = false;
    }

    async void Enter()
    {
        await Task.Run(async () =>
        {

            while (_enabled)
            {
                var tickCount/*塵秒*/ = Environment.TickCount/*塵秒*/;
                var dif/*塵秒*/ = tickCount/*塵秒*/ - _next/*塵秒*/;
                if (dif >= 0)
                {
                    int elapsedCount/*塵秒*/ = (int)MathD.Floor(dif/*塵秒*/ / _interval/*塵秒*/);
                    Elapsed?.Invoke(this, new(elapsedCount/*塵秒*/, tickCount/*塵秒*/));

                    _next/*塵秒*/ += _interval/*塵秒*/ * elapsedCount/*塵秒*/;
                }

                var task = Task.Delay(_sleepTimeout/*毛秒*/);
                await task;
            }
        });
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _enabled = false;
            }

            // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
            // TODO: 大きなフィールドを null に設定します
            _disposedValue = true;
        }
    }
    public void Dispose()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    readonly static Dictionary<double, Timer> _timers = new();

    /// <summary>
    /// イベントハンドラーをタイマーに申請します。
    /// </summary>
    /// <param name="handler">
    /// 登録するイベントハンドラー。
    /// </param>
    /// <param name="interval">
    /// タイマーの間隔。
    /// </param>
    public static void Subscribe(ElapsedEventHandler handler, double interval)
    {
        if (!_timers.TryGetValue(interval, out var timer))
        {
            timer = new Timer(interval) { Enabled = true };
            _timers.Add(interval, timer);
        }
        timer.Elapsed += handler;
    }

    /// <summary>
    /// イベントハンドラーをタイマーから絶縁します。
    /// <para>
    /// 絶縁したことによって無意味になったタイマーは自動で削除されますが、この挙動は場合によってはうまく働かないことがあります。複数回呼び出すことによって完全に浄化されます。
    /// </para>
    /// </summary>
    /// <param name="handler">
    /// 絶縁するイベントハンドラー。
    /// </param>
    public static void Break(ElapsedEventHandler handler)
    {
        double v = double.NaN;
        foreach (var (interval, timer) in _timers)
        {
            timer.Elapsed -= handler;
            if (timer.Elapsed == null) v = interval;
        }
        if (Double.IsNaN(v)) _ = _timers.Remove(v);
    }
}

[Obsolete("現在は`ITimer`を使用する方法が推奨されています。")]
public interface ITimeSensitive
{
    void OnElapsed(object sender, ElapsedEventArgs e);
}

[Obsolete("互換性がなく、使い勝手が悪いから代替案を考えることとして廃止予定。")]
public class Variable<T>
{
    T? _value;

    public bool IsNotHandled => Changed == null;
    public event Action<T>? Changed;

    public Variable(T? value)
    {
        _value = value;
    }

    public void Move(T value)
    {
        _value = value;
        Changed?.Invoke(value);
    }

    public static implicit operator Variable<T>(T? value) => new(value);
    public static explicit operator T?(Variable<T> variable) => variable._value;
}
