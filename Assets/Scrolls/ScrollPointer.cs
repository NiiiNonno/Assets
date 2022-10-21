//#define USE_BYTE_SPAN
using static System.BitConverter;

namespace Nonno.Assets.Scrolls;

/// <summary>
/// 巻子の中の位置を示す軸箋を表します。
/// <para>
/// 軸箋は<see cref="IScroll.Point"/>によって正しい値が得られ、巻子同士で相互に適用することはできません。得た値は必ず得た巻子に使用してください。
/// </para>
/// <para>
/// 軸箋は<see cref="IScroll.Point"/>に設定された時点で無効となります。再び必要となる場合は同時に<see cref="IScroll.Point"/>から新しい軸箋を取得してください。
/// </para>
/// </summary>
public unsafe readonly struct ScrollPointer : IEquatable<ScrollPointer>
{
    readonly nint _num;
    readonly object? _obj;

    public int Number => (int)_num;
    /// <summary>
    /// 軸箋の番号を取得します。
    /// <para>
    /// 軸箋の番号の扱われ方は巻子の実装によってさまざまであり、この値の一致は軸箋の一致を示しません。
    /// </para>
    /// </summary>
    public long LongNumber
    {
        get
        {
            switch (sizeof(nint))
            {
            case sizeof(long):
                return _num;
            case sizeof(uint):
                uint num = (uint)_num;
                var ext = (Extension?)_obj;
                if (ext is null) return num;
                return (long)ext.Number << 32 | num; 
            default:
                throw new Exception("不明な錯誤です。`IntPtr`のバイト長が`Int64`や`UInt32`の何れのものとも異なりました。");
            }            
        }
    }
    /// <summary>
    /// 軸箋の拡張情報を取得します。
    /// <para>
    /// 軸箋の拡張情報の扱われ方は巻子の実装によってさまざまであり、この値の一致または有無は軸箋の内容を一般に表しません。
    /// </para>
    /// </summary>
    public object? Information => _obj;
    /// <summary>
    /// 軸箋の四文字の文字列を取得します。
    /// <para>
    /// 軸箋の拡張情報の扱われ方は巻子の実装によってさまざまであり、この値の一致または有無は軸箋の内容を一般に表しません。
    /// </para>
    /// </summary>
    public ASCIIString ASCIIString => new(GetBytes((uint)_num));
    /// <summary>
    /// 軸箋の四バイトのバイト列を取得します。
    /// <para>
    /// 軸箋の拡張情報の扱われ方は巻子の実装によってさまざまであり、この値の一致または有無は軸箋の内容を一般に表しません。
    /// </para>
    /// </summary>
    public byte[] Bytes => GetBytes((uint)_num);

    /// <summary>
    /// 軸箋を規定値で初期化します。
    /// </summary>
    public ScrollPointer()
    {
        _num = default;
        _obj = default;
    }
    /// <summary>
    /// 軸箋を番号のみを指定して初期化します。拡張情報は規定値で初期化されます。
    /// </summary>
    /// <param name="longNumber">
    /// 指定する番号。
    /// </param>
    public ScrollPointer(long longNumber)
    {
        _obj = null;

        switch (sizeof(nint))
        {
        case sizeof(long):
            _num = (nint)longNumber;
            return;
        case sizeof(uint):
            _num = (nint)longNumber;
            if (_num == longNumber) return;
            var exN = (int)(longNumber >> 32);
            if (!_extensions.TryGetValue(exN, out var ext)) 
            { 
                ext = new(exN);
                _extensions.Add(exN, ext);
            }
            _obj = ext;
            return;
        default:
            throw new Exception("不明な錯誤です。`IntPtr`のバイト長が`Int64`や`UInt32`の何れのものとも異なりました。");
        }
    }
    /// <summary>
    /// 軸箋を拡張情報のみを指定して初期化します。番号は規定値で初期化されます。
    /// </summary>
    /// <param name="number">
    /// 指定する整数値。
    /// </param>
    /// <param name="information">
    /// 指定する拡張情報。
    /// <para>
    /// 拡張情報の<see cref="object.GetHashCode"/>および<see cref="object.Equals(object?)"/>は<see cref="ScrollPointer"/>が有効である間常に同じ値を返す必要があります。
    /// </para>
    /// </param>
    public ScrollPointer(int number = default, object? information = null)
    {
        _num = number;
        _obj = information;
    }
    /// <summary>
    /// 軸箋を
    /// </summary>
    /// <param name="fourASCIIs"></param>
    /// <param name="information"></param>
    public ScrollPointer(ASCIIString fourASCIIs, object? information = null) : this(fourASCIIs.AsSpan(), information) { }
    public ScrollPointer(ReadOnlySpan<byte> fourBytes, object? information = null)
    {
        if (fourBytes.Length != 4) throw new ArgumentException("冊第には四バイト以上の情報を直接記録することはできません。");

        _obj = information;
        _num = ToInt32(fourBytes);
    }
    public ScrollPointer(nint number = default, object? @object = default)
    {
        _num = number;
        _obj = @object;
    }

    /// <inheritdoc/>
    public override string ToString() => $"[{LongNumber}/{ASCIIString}/{Information}]";

    public override bool Equals(object? obj) => obj is ScrollPointer pointer && _num.Equals(pointer._num) && EqualityComparer<object?>.Default.Equals(_obj, pointer._obj);
    public override int GetHashCode() => HashCode.Combine(_num, _obj);
    public bool Equals(ScrollPointer other) => _num.Equals(other._num) && EqualityComparer<object?>.Default.Equals(_obj, other._obj);

    public static bool operator ==(ScrollPointer left, ScrollPointer right) => left.Equals(right);
    public static bool operator !=(ScrollPointer left, ScrollPointer right) => !(left == right);

    static readonly Dictionary<int, Extension> _extensions = new();

    record Extension(int Number);
}
