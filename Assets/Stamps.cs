#if USE_DOUBLE
using Dec = System.Double;
using Math = System.Math;
#else 
#endif
using System.Numerics;

namespace Nonno.Assets;

public interface IPeriodicTimeStamp
{
    long Ticks { get; }
    Double24 Seconds { get; }
}

/// <summary>
/// 定期時刻を表します。
/// </summary>
public readonly struct PeriodicTimeStamp : IPeriodicTimeStamp
{
    readonly long _ticks;

    /// <summary>
    /// 時刻数を取得します。
    /// <para>
    /// 時刻数は毎刻凡そ59.68㌨秒です。
    /// </para>
    /// </summary>
    public long Ticks => _ticks;
    public Double24 Seconds => new(_ticks);

    public PeriodicTimeStamp(long ticks) => _ticks = ticks;
    public PeriodicTimeStamp(Double24 seconds) => _ticks = seconds.Number;
    public PeriodicTimeStamp(DateTime dateTime)
    {
        // 10000000 == 16777216(0x1000000)
        _ticks = (long)(new BigInteger(dateTime.Ticks) * 0x1000000 / 10000000);
    }

    public static Delta operator -(PeriodicTimeStamp left, PeriodicTimeStamp right) => new(left._ticks - right._ticks);

    /// <summary>
    /// 時刻の差を表します。
    /// </summary>
    public readonly struct Delta
    {
        readonly long _ticks;

        public long Ticks => _ticks;
        public Double24 Seconds => new(_ticks);

        public Delta(long ticks) => _ticks = ticks;
        public Delta(Double24 seconds) => _ticks = seconds.Number;
        public Delta(TimeSpan timeSpan)
        {
            // 10000000 == 16777216(0x1000000)
            _ticks = (long)(new BigInteger(timeSpan.Ticks) * 0x1000000 / 10000000);
        }

        public static Delta operator -(Delta delta) => new(-delta._ticks);
        public static Delta operator +(Delta left, Delta right) => new(left._ticks - right._ticks);
        public static Delta operator -(Delta left, Delta right) => new(left._ticks - right._ticks);
        public static PeriodicTimeStamp operator +(PeriodicTimeStamp left, Delta right) => new(left._ticks + right._ticks);
        public static PeriodicTimeStamp operator -(PeriodicTimeStamp left, Delta right) => new(left._ticks - right._ticks);
        public static Delta operator *(int left, Delta right) => new(left * right._ticks);
        public static Delta operator *(Shift left, Delta right) => new(left * right._ticks);
    }
}

public interface IPeriodicLocationStamp
{
    Double32 MetersX { get; }
    Double32 MetersY { get; }
    Double32 MetersZ { get; }
    long TicksX { get; }
    long TicksY { get; }
    long TicksZ { get; }
}

/// <summary>
/// 定期空刻を表します。
/// </summary>
public readonly struct PeriodicLocationStamp : IPeriodicLocationStamp
{
    readonly long _ticksX, _ticksY, _ticksZ;

    /// <summary>
    /// X軸の空刻数を取得します。
    /// <para>
    /// 空刻数は毎刻凡そ2.328Åです。
    /// </para>
    /// </summary>
    public long TicksX => _ticksX;
    /// <summary>
    /// X軸の空刻数を取得します。
    /// <para>
    /// 空刻数は毎刻凡そ2.328Åです。
    /// </para>
    /// </summary>
    public long TicksY => _ticksY;
    /// <summary>
    /// X軸の空刻数を取得します。
    /// <para>
    /// 空刻数は毎刻凡そ2.328Åです。
    /// </para>
    /// </summary>
    public long TicksZ => _ticksZ;
    public Double32 MetersX => new(_ticksX);
    public Double32 MetersY => new(_ticksY);
    public Double32 MetersZ => new(_ticksX);

    public PeriodicLocationStamp(long ticksX, long ticksY, long ticksZ)
    {
        _ticksX = ticksX;
        _ticksY = ticksY;
        _ticksZ = ticksZ;
    }
    public PeriodicLocationStamp(Double32 metersX, Double32 metersY, Double32 metersZ)
    {
        _ticksX = metersX.Number;
        _ticksY = metersY.Number;
        _ticksZ = metersZ.Number;
    }

    /// <summary>
    /// 空刻の差を表します。
    /// </summary>
    public readonly struct Delta
    {
        readonly long _ticksX, _ticksY, _ticksZ;

        /// <summary>
        /// X軸の空刻数を取得します。
        /// <para>
        /// 空刻数は毎刻2.328Åです。
        /// </para>
        /// </summary>
        public long TicksX => _ticksX;
        /// <summary>
        /// X軸の空刻数を取得します。
        /// <para>
        /// 空刻数は毎刻2.328Åです。
        /// </para>
        /// </summary>
        public long TicksY => _ticksY;
        /// <summary>
        /// X軸の空刻数を取得します。
        /// <para>
        /// 空刻数は毎刻2.328Åです。
        /// </para>
        /// </summary>
        public long TicksZ => _ticksZ;
        public Double32 MetersX => new(_ticksX);
        public Double32 MetersY => new(_ticksY);
        public Double32 MetersZ => new(_ticksX);

        public Delta(long ticksX, long ticksY, long ticksZ)
        {
            _ticksX = ticksX;
            _ticksY = ticksY;
            _ticksZ = ticksZ;
        }
        public Delta(Double32 metersX, Double32 metersY, Double32 metersZ)
        {
            _ticksX = metersX.Number;
            _ticksY = metersY.Number;
            _ticksZ = metersZ.Number;
        }

        public static Delta operator -(Delta delta) => new(-delta._ticksX, -delta._ticksY, -delta._ticksZ);
        public static Delta operator +(Delta left, Delta right) => new(left._ticksX + right._ticksX, left._ticksY + right._ticksY, left._ticksZ + right._ticksZ);
        public static Delta operator -(Delta left, Delta right) => new(left._ticksX - right._ticksX, left._ticksY - right._ticksY, left._ticksZ - right._ticksZ);
        public static PeriodicLocationStamp operator +(PeriodicLocationStamp left, Delta right) => new(left._ticksX + right._ticksX, left._ticksY + right._ticksY, left._ticksZ + right._ticksZ);
        public static PeriodicLocationStamp operator -(PeriodicLocationStamp left, Delta right) => new(left._ticksX - right._ticksX, left._ticksY - right._ticksY, left._ticksZ - right._ticksZ);
        public static Delta operator *(int left, Delta right) => new(left * right._ticksX, left * right._ticksY, left * right._ticksZ);
        public static Delta operator *(Shift left, Delta right) => new(left * right._ticksX, left * right._ticksY, left * right._ticksZ);
    }
}
