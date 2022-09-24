using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets;

public interface ICsio
{
    /// <summary>
    /// 書の指定した位置の指定した條の墨の書かれ方を返します。
    /// </summary>
    /// <param name="index">
    /// 指定する位置。
    /// </param>
    /// <param name="stringNumber">
    /// 條の中心から数えた位置番号。
    /// </param>
    /// <returns>
    /// <see cref="PairFlags.Left"/>なら左に墨。<see cref="PairFlags.Right"/>なら右に墨。
    /// </returns>
    public PairFlags this[int index, int stringNumber] { get; }

    /// <summary>
    /// 書の軸を指定した條だけ移動させ、左を見ます。
    /// </summary>
    /// <param name="stringCount">
    /// 移動させる條の数。
    /// </param>
    public void Shift(int stringCount);

    /// <summary>
    /// 書の指定した位置を、<see cref="UInt32"/>に収まる範囲を見た時の符号を返します。
    /// </summary>
    /// <param name="index">
    /// 指定する位置。
    /// </param>
    /// <returns>
    /// 見た符号。
    /// <para>
    /// 数値の上位桁が左、下位が右であるビット列表現。
    /// </para>
    /// </returns>
    public uint GetCode(int index);
    public ReadOnlySpan<uint> GetCodeSpan(int startIndex, int length)
    {
        Span<uint> r = new uint[length];
        var c = startIndex;
        for (int i = 0; i < length; i++) r[i] = GetCode(c);
        return r;
    }
}
