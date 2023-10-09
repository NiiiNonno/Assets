using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets;

/// <summary>
/// 主作絡は作業中に必ず主作絡に依らなければならない工程がある場合に使用されます。
/// </summary>
/// <remarks>
/// <see cref="Invoke(Action)"/>を行うには、<see cref="Yield(ThreadStart)"/>を正しく実行している必要があります。
/// </remarks>
public static class MainThread
{
    readonly static Thread _main;
    readonly static ConcurrentQueue<Action> _queue;
    static Thread? _sub;
    static bool _hasControl;

    /// <summary>
    /// 主作絡が徒な時に、毎何毛秒に待列を候うかを取得または設定します。
    /// </summary>
    public static int Interval { get; set; } = 30;
    /// <summary>
    /// 主作絡が徒であるか、則ち<see cref="Invoke(Action)"/>の後忽ち代務が行われるかを取得します。
    /// </summary>
    public static bool IsLeisure => _queue.IsEmpty;
    /// <summary>
    /// 呼び出しが主作絡依り行われたかを取得します。
    /// </summary>
    public static bool IsMainThread => _main == Thread.CurrentThread;

    /// <summary>
    /// 作絡を譲ります。
    /// この時、続きの処理を引謄させます。
    /// <para>
    /// 必ずこの務めは主作絡にて行ってください。
    /// </para>
    /// </summary>
    /// <example>
    /// // on main thread.
    /// MainThread.Yield(() => 
    /// {
	///     // process to continue on other thread.
    /// });
    ///
    /// // here is sometimes never called.
    /// </example>
    /// <param name="process"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void Yield(ThreadStart process)
    {
        if (!IsMainThread) throw new InvalidOperationException("現在の作絡が主でないか、主とされる作絡が誤っています。この務容はプログラムの開始直後に呼び出されることが想定されています。");

        _sub = new Thread(process);
        _sub.IsBackground = false;
        _sub.Name = "譲之作絡";
        _sub.Start();

        _hasControl = true;
        _main.IsBackground = true;
        while (_sub.IsAlive)
        {
            if (_queue.TryDequeue(out var action))
            {
                action();
            }
            Thread.Sleep(Interval);
        }
    }

    /// <summary>
    /// 主作絡を望む工程を使います。
    /// </summary>
    /// <param name="action"></param>
    /// <param name="runOnForeground">工程を叵絶に使う場合<c>true</c>、否や<c>false</c></param>
    public static void Invoke(Action action, bool runOnForeground)
    {
        Invoke(runOnForeground ? () =>
        {
            var f = Thread.CurrentThread.IsBackground;
            Thread.CurrentThread.IsBackground = false;
            action();
            Thread.CurrentThread.IsBackground = f;
        }
        : action);
    }
    /// <summary>
    /// 主作絡を望む工程を使います。
    /// </summary>
    /// <example>
    /// // on main or other thread.
    /// MainThread.Invoke(() =>
    /// {
    ///	    // process to continue on main thread.
    /// });
    /// </example>
    /// <param name="action"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void Invoke(Action action)
    {
        if (IsMainThread)
        {
            action();
            return;
        }

        if (!_hasControl) throw new InvalidOperationException("主作絡は使用できません。プログラムの開始時に権限が譲られていません。");

        _queue.Enqueue(action);
    }

    static MainThread()
    {
        _main = Thread.CurrentThread;
        _main.Name = "主扱作絡";
        _queue = new();
    }
}
