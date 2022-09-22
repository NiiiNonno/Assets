// 令和弐年大暑確認済。
#if USE_DOUBLE
using Dec = System.Double;
using Math = System.Math;
#else
using Dec = System.Single;
using Math = System.MathF;
#endif
using static System.Math;

namespace Nonno.Assets;

public class ShortFastFourierTransformer
{
    /// <summary>
    /// 計算に使用しているサンプル数。
    /// </summary>
    readonly Shift _range;
    /// <summary>
    /// 層ごとのユニットの幅。
    /// </summary>
    readonly int[] _unitWidths;
    /// <summary>
    /// ユニットごとの演算を行う回数。
    /// </summary>
    readonly int[] _opeCs;
    readonly int[] _twiddleSkips;
    /// <summary>
    /// ビットリバース。計算で使う。
    /// </summary>
    readonly int[] _revs;
    /// <summary>
    /// 回転因子。計算で使う。
    /// </summary>
    readonly Complex[] _twiddleF;
    /// <summary>
    /// 変換するデータ。または<see cref="Run"/>メソッド実行後には結果データのビットリバース順。
    /// </summary>
    readonly Complex[] _resource;
    /// <summary>
    /// 窓函数。
    /// </summary>
    readonly Dec[] _window;

    public Shift Range => _range;

    public ShortFastFourierTransformer(Shift range, Func<Dec, Dec> window)
    {
        int range_ = range;
        int expo = range.exponent;

        if (expo < 1) throw new ArgumentException("2以下の長さは指定できません。", nameof(range));

        _range = range;
        _unitWidths = new int[expo];
        _opeCs = new int[expo];
        _twiddleSkips = new int[expo];
        _revs = Utils.BitReverse(range);
        _twiddleF = new Complex[range_];
        _resource = new Complex[range_];
        _window = new Dec[range_];

        for (int i = 0; i < expo; i++)
        {
            _unitWidths[i] = 1 << (expo - i);
            _opeCs[i] = 1 << (expo - i - 1);
            _twiddleSkips[i] = 1 << i;
        }

        for (int i = 0; i < _twiddleF.Length; i++)
        {
            Dec deg = -2 * Math.PI * i / range_;
            _twiddleF[i] = new Complex(Math.Cos(deg), Math.Sin(deg));
        }

        Dec denom = range_ - 1;
        for (int i = 0; i < _window.Length; i++) _window[i] = window(i / denom);
    }

    public void Set(IEnumerable<Complex> from)
    {
        int i = 0;
        foreach (var item in from)
        {
            if (i >= _revs.Length) break;
            _resource[i++] = item;
        }
    }

    public void Set(IEnumerator<Complex> from)
    {
        for (int i = 0; i < _revs.Length && from.MoveNext(); i++) _resource[i] = from.Current * _window[i];
    }

    public void Run()
    {
        var range = (int)_range;
        var expo = _range.exponent;
        var twiddleF = _twiddleF;

        for (int iStep = 0; iStep < expo; iStep++)
        {
            var opeC = _opeCs[iStep];
            var unitWidth = _unitWidths[iStep];
            var twiddleSkip = _twiddleSkips[iStep];

            for (int unitIndex = 0; unitIndex < range; unitIndex += unitWidth)
            {
                for (int opeNum = 0; opeNum < opeC; opeNum++)
                {
                    int iTop = unitIndex + opeNum;
                    int iBtm = iTop + opeC;

                    Complex top = _resource[iTop];

                    int iTwiddleF = opeNum * twiddleSkip;

                    _resource[iTop] = top + _resource[iBtm];
                    _resource[iBtm] = (top - _resource[iBtm]) * twiddleF[iTwiddleF];
                }
            }
        }
    }

    /// <summary>
    /// 内部配列をビットリバーズ順で区間に複写します。
    /// <para>
    /// 内部配列に複写することはできません。必ず自前で用意した配列に対する区間に複写してください。
    /// </para>
    /// </summary>
    /// <param name="to"></param>
    public void Copy(Span<Complex> to)
    {
        int length = Min(to.Length, _revs.Length);
        for (int i = 0; i < length; i++)
        {
            //to[i] = new Complex(_resource_r[i], _resource_i[i]);
            //to[i] = _resource[_revs[i]];
            to[_revs[i]] = _resource[i];
        }
    }

    /// <summary>
    /// 内部配列、すなわち<see cref="Set(IEnumerable{Complex})"/>がデータを複製格納する先の配列を取得します。
    /// <para>
    /// このメソッドによって得た配列参照は高速化のために役立つ場合がありますが、必ずしも正確にアクセスできるとは限りません。高速化が主要な問題である場合に使用を検討してください。
    /// </para>
    /// </summary>
    /// <remarks>
    /// 得た配列に直接変換前のデータを上書きすることで、<see cref="Run"/>メソッド呼び出し後に配列は変換後のデータのビットリバース順になります。
    /// </remarks>
    /// <returns>
    /// 内部配列。
    /// </returns>
    public Complex[] GetSourceArray() => _resource;
}
