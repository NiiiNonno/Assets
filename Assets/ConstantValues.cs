// 令和弐年大暑確認済。
#if USE_DOUBLE
using Dec = System.Double;
using Math = System.Math;
#else
using Dec = System.Single;
using Math = System.MathF;
#endif

namespace Nonno.Assets;

public static class ConstantValues
{
    /// <summary>
    /// 可視光下限周波数(1153[THz])
    /// </summary>
    public const Dec A_FREQ = 1153_047_915_384_620f;//alpha
    /// <summary>
    /// 青色周波数(713[THz])
    /// </summary>
    public const Dec B_FREQ = 713_791_566_666_667f;//blue
    /// <summary>
    /// 青緑境周波数(647[THz])
    /// </summary>
    public const Dec C_FREQ = 647_499_909_287_257f;//cyan
    /// <summary>
    /// 緑色周波数(561[THz])
    /// </summary>
    public const Dec G_FREQ = 561_409_097_378_277f;//green
    /// <summary>
    /// 緑赤境周波数(547[THz])
    /// </summary>
    public const Dec O_FREQ = 547_066_529_197_080f;//orange
    /// <summary>
    /// 赤色周波数(531[THz])
    /// </summary>
    public const Dec R_FREQ = 531_546_911_347_518f;//red
    /// <summary>
    /// 可視光上限周波数(361[THz])
    /// </summary>
    public const Dec X_FREQ = 361_195_732_530_120f;//max

    public const Dec BYTE_MAX = Byte.MaxValue;
    public const Dec UINT16_MAX = UInt16.MaxValue;
    public const Dec UINT32_MAX = UInt32.MaxValue;

    public const Dec SBYTE_MAX = SByte.MaxValue;
    public const Dec INT16_MAX = Int16.MaxValue;
    public const Dec INT32_MAX = Int32.MaxValue;

    public const Dec SBYTE_MIN = SByte.MinValue;
    public const Dec INT16_MIN = Int16.MinValue;
    public const Dec INT32_MIN = Int32.MinValue;

    public const Dec BYTE_MAX_RECIPRO = 1 / BYTE_MAX;
    public const Dec UINT16_MAX_RECIPRO = 1 / UINT16_MAX;
    public const Dec UINT32_MAX_RECIPRO = 1 / UINT32_MAX;

    public const Dec PI_RECIPRO = 1 / Math.PI;

    public const int STACKALLOC_MAX_LENGTH = 0x1000;

    public const int NUMBER_STRING_MAX_LENGTH_DECIMAL_INT32 = 11; // 2147483647は十文字、符号を含めて十一文字。
    public const int NUMBER_STRING_MAX_LENGTH_DECIMAL_UNSIGNED_INT32 = 10; // 4294967295は十文字。
}
