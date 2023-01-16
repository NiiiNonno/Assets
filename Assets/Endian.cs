using static System.BitConverter;
namespace Nonno.Assets;
public unsafe class Endian : IEquatable<Endian>
{
    const int NONE_N = 0;
    const int LITE_N = 1;
    const int BIGE_N = 2;
    const int EXCP_N = -1;

    const char MARK_CHAR = '\u0201';
    const ushort MARK_USHORT = 0x0201;
    const short MARK_SHORT = 0x0201;
    const uint MARK_UINT = 0x04030201;
    const int MARK_INT = 0x04030201;
    const ulong MARK_ULONG = 0x0807060504030201;
    const long MARK_LONG = 0x0807060504030201;
    const float MARK_FLOAT = 1.539989614439558e-36f;
    const double MARK_DOUBLE = 5.447603722011605e-270;
    const int TYPE_LITTLE_E = 1;
    const int TYPE_BIG_E = 2;

    readonly byte[] _char;
    readonly byte[] _ushort;
    readonly byte[] _short;
    readonly byte[] _uint;
    readonly byte[] _int;
    readonly byte[] _ulong;
    readonly byte[] _long;
    readonly byte[] _float;
    readonly byte[] _double;
    int _type; // 0 => Unknown, 1 => Little, 2 => Big, PositiveOther => Middle, Negative => Error

    public unsafe bool IsLittleEndian
    {
        get
        {
            if (_type == 0)
            {
                for (int i = 0; i < sizeof(char);) if (_char[i] != ++i) goto outside;
                for (int i = 0; i < sizeof(ushort);) if (_ushort[i] != ++i) goto outside;
                for (int i = 0; i < sizeof(short);) if (_short[i] != ++i) goto outside;
                for (int i = 0; i < sizeof(uint);) if (_uint[i] != ++i) goto outside;
                for (int i = 0; i < sizeof(int);) if (_int[i] != ++i) goto outside;
                for (int i = 0; i < sizeof(ulong);) if (_ulong[i] != ++i) goto outside;
                for (int i = 0; i < sizeof(long);) if (_long[i] != ++i) goto outside;
                for (int i = 0; i < sizeof(float);) if (_float[i] != ++i) goto outside;
                for (int i = 0; i < sizeof(double);) if (_double[i] != ++i) goto outside;
                _type = 1;
            }
            outside:

            return _type == TYPE_LITTLE_E;
        }
    }
    public unsafe bool IsBigEndian
    {
        get
        {
            if (_type == 0)
            {
                for (int i = sizeof(char); i >= 0;) if (_char[i] != i--) goto outside;
                for (int i = sizeof(ushort); i >= 0;) if (_ushort[i] != i--) goto outside;
                for (int i = sizeof(short); i >= 0;) if (_short[i] != i--) goto outside;
                for (int i = sizeof(uint); i >= 0;) if (_uint[i] != i--) goto outside;
                for (int i = sizeof(int); i >= 0;) if (_int[i] != i--) goto outside;
                for (int i = sizeof(ulong); i >= 0;) if (_ulong[i] != i--) goto outside;
                for (int i = sizeof(long); i >= 0;) if (_long[i] != i--) goto outside;
                for (int i = sizeof(float); i >= 0;) if (_float[i] != i--) goto outside;
                for (int i = sizeof(double); i >= 0;) if (_double[i] != i--) goto outside;
                _type = 1;
            }
            outside:

            return _type == TYPE_BIG_E;
        }
    }

    public Endian(byte[] @char, byte[] @ushort, byte[] @short, byte[] @uint, byte[] @int, byte[] @ulong, byte[] @long, byte[] @float, byte[] @double)
    {
        if (@char.Length != sizeof(char) ||
            @ushort.Length != sizeof(ushort) ||
            @short.Length != sizeof(short) ||
            @uint.Length != sizeof(uint) ||
            @int.Length != sizeof(int) ||
            @ulong.Length != sizeof(ulong) ||
            @long.Length != sizeof(long) ||
            @float.Length != sizeof(float) ||
            @double.Length != sizeof(double)) throw new ArgumentException("配列の長さが値のバイト長と異なります。");
        _char = @char;
        _ushort = @ushort;
        _short = @short;
        _uint = @uint;
        _int = @int;
        _ulong = @ulong;
        _long = @long;
        _float = @float;
        _double = @double;
    }
    public Endian(byte[] source)
    {
        _char = source[0..2];
        _ushort = source[2..4];
        _short = source[4..6];
        _uint = source[6..10];
        _int = source[10..14];
        _ulong = source[14..22];
        _long = source[22..30];
        _float = source[30..34];
        _double = source[34..42];
    }

    /*
        $$"""
        public unsafe void Standardize({{t}}* @{{t}}, void* to)
        {
            var p = (byte*)@{{t}};
            var t = (byte*)to;
            for (int i = 0; i < _{{t}}.Length; i++) t[_{{t}}[i]] = p[i];
        }
        """;
    */
    public unsafe void Standardize(char* @char, void* to)
    {
        var p = (byte*)@char;
        var t = (byte*)to;
        for (int i = 0; i < _char.Length; i++) t[_char[i]] = p[i];
    }
    public unsafe void Standardize(ushort* @ushort, void* to)
    {
        var p = (byte*)@ushort;
        var t = (byte*)to;
        for (int i = 0; i < _ushort.Length; i++) t[_ushort[i]] = p[i];
    }
    public unsafe void Standardize(short* @short, void* to)
    {
        var p = (byte*)@short;
        var t = (byte*)to;
        for (int i = 0; i < _short.Length; i++) t[_short[i]] = p[i];
    }
    public unsafe void Standardize(uint* @uint, void* to)
    {
        var p = (byte*)@uint;
        var t = (byte*)to;
        for (int i = 0; i < _uint.Length; i++) t[_uint[i]] = p[i];
    }
    public unsafe void Standardize(int* @int, void* to)
    {
        var p = (byte*)@int;
        var t = (byte*)to;
        for (int i = 0; i < _int.Length; i++) t[_int[i]] = p[i];
    }
    public unsafe void Standardize(ulong* @ulong, void* to)
    {
        var p = (byte*)@ulong;
        var t = (byte*)to;
        for (int i = 0; i < _ulong.Length; i++) t[_ulong[i]] = p[i];
    }
    public unsafe void Standardize(long* @long, void* to)
    {
        var p = (byte*)@long;
        var t = (byte*)to;
        for (int i = 0; i < _long.Length; i++) t[_long[i]] = p[i];
    }
    public unsafe void Standardize(float* @float, void* to)
    {
        var p = (byte*)@float;
        var t = (byte*)to;
        for (int i = 0; i < _float.Length; i++) t[_float[i]] = p[i];
    }
    public unsafe void Standardize(double* @double, void* to)
    {
        var p = (byte*)@double;
        var t = (byte*)to;
        for (int i = 0; i < _double.Length; i++) t[_double[i]] = p[i];
    }
    /*
        $$"""
        public unsafe void Localize(void* from, {{t}}* to)
        {
            var p = (byte*)from;
            var t = (byte*)to;
            for (int i = 0; i < _{{t}}.Length; i++) t[i] = p[_{{t}}[i]];
        }
        """;
    */
    public unsafe void Localize(void* from, char* to)
    {
        var p = (byte*)from;
        var t = (byte*)to;
        for (int i = 0; i < _char.Length; i++) t[i] = p[_char[i]];
    }
    public unsafe void Localize(void* from, ushort* to)
    {
        var p = (byte*)from;
        var t = (byte*)to;
        for (int i = 0; i < _ushort.Length; i++) t[i] = p[_ushort[i]];
    }
    public unsafe void Localize(void* from, short* to)
    {
        var p = (byte*)from;
        var t = (byte*)to;
        for (int i = 0; i < _short.Length; i++) t[i] = p[_short[i]];
    }
    public unsafe void Localize(void* from, uint* to)
    {
        var p = (byte*)from;
        var t = (byte*)to;
        for (int i = 0; i < _uint.Length; i++) t[i] = p[_uint[i]];
    }
    public unsafe void Localize(void* from, int* to)
    {
        var p = (byte*)from;
        var t = (byte*)to;
        for (int i = 0; i < _int.Length; i++) t[i] = p[_int[i]];
    }
    public unsafe void Localize(void* from, ulong* to)
    {
        var p = (byte*)from;
        var t = (byte*)to;
        for (int i = 0; i < _ulong.Length; i++) t[i] = p[_ulong[i]];
    }
    public unsafe void Localize(void* from, long* to)
    {
        var p = (byte*)from;
        var t = (byte*)to;
        for (int i = 0; i < _long.Length; i++) t[i] = p[_long[i]];
    }
    public unsafe void Localize(void* from, float* to)
    {
        var p = (byte*)from;
        var t = (byte*)to;
        for (int i = 0; i < _float.Length; i++) t[i] = p[_float[i]];
    }
    public unsafe void Localize(void* from, double* to)
    {
        var p = (byte*)from;
        var t = (byte*)to;
        for (int i = 0; i < _double.Length; i++) t[i] = p[_double[i]];
    }

    public override string ToString() => IsLittleEndian ? "LittleEndian" : IsBigEndian ? "BigEndian" : "MiddleEndian";

    public override bool Equals(object? obj)
    {
        return obj is Endian endian &&
               EqualityComparer<byte[]>.Default.Equals(_char, endian._char) &&
               EqualityComparer<byte[]>.Default.Equals(_ushort, endian._ushort) &&
               EqualityComparer<byte[]>.Default.Equals(_short, endian._short) &&
               EqualityComparer<byte[]>.Default.Equals(_uint, endian._uint) &&
               EqualityComparer<byte[]>.Default.Equals(_int, endian._int) &&
               EqualityComparer<byte[]>.Default.Equals(_ulong, endian._ulong) &&
               EqualityComparer<byte[]>.Default.Equals(_long, endian._long) &&
               EqualityComparer<byte[]>.Default.Equals(_float, endian._float) &&
               EqualityComparer<byte[]>.Default.Equals(_double, endian._double);
    }
    public bool Equals(Endian? endian)
    {
        return endian is not null &&
               EqualityComparer<byte[]>.Default.Equals(_char, endian._char) &&
               EqualityComparer<byte[]>.Default.Equals(_ushort, endian._ushort) &&
               EqualityComparer<byte[]>.Default.Equals(_ushort, endian._ushort) &&
               EqualityComparer<byte[]>.Default.Equals(_short, endian._short) &&
               EqualityComparer<byte[]>.Default.Equals(_uint, endian._uint) &&
               EqualityComparer<byte[]>.Default.Equals(_int, endian._int) &&
               EqualityComparer<byte[]>.Default.Equals(_ulong, endian._ulong) &&
               EqualityComparer<byte[]>.Default.Equals(_long, endian._long) &&
               EqualityComparer<byte[]>.Default.Equals(_float, endian._float) &&
               EqualityComparer<byte[]>.Default.Equals(_double, endian._double);
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(_char);
        hash.Add(_ushort);
        hash.Add(_short);
        hash.Add(_uint);
        hash.Add(_int);
        hash.Add(_ulong);
        hash.Add(_long);
        hash.Add(_float);
        hash.Add(_double);
        return hash.ToHashCode();
    }

    public static Endian LittleEndian {get;}
    public static Endian BigEndian {get;}
    public static Endian HostByteOrder {get;}

    static Endian()
    {
        LittleEndian = new Endian(new byte[]
        {
            1, 2,
            1, 2, 
            1, 2, 
            1, 2, 3, 4, 
            1, 2, 3, 4,
            1, 2, 3, 4, 5, 6, 7, 8,
            1, 2, 3, 4, 5, 6, 7, 8,
            1, 2, 3, 4,
            1, 2, 3, 4, 5, 6, 7, 8});
        LittleEndian._type = TYPE_LITTLE_E;

        BigEndian = new Endian(new byte[]
        {
            2, 1,
            2, 1,
            2, 1,
            4, 3, 2, 1,
            4, 3, 2, 1,
            8, 7, 6, 5, 4, 3, 2, 1,
            8, 7, 6, 5, 4, 3, 2, 1,
            4, 3, 2, 1,
            8, 7, 6, 5, 4, 3, 2, 1});
        BigEndian._type = TYPE_BIG_E;

        HostByteOrder = new Endian(
            GetBytes(MARK_CHAR),
            GetBytes(MARK_USHORT),
            GetBytes(MARK_SHORT),
            GetBytes(MARK_UINT),
            GetBytes(MARK_INT),
            GetBytes(MARK_ULONG),
            GetBytes(MARK_LONG),
            GetBytes(MARK_FLOAT),
            GetBytes(MARK_DOUBLE));
    }

    public static bool operator ==(Endian left, Endian right) => left.Equals(right);
    public static bool operator !=(Endian left, Endian right) => !left.Equals(right);
}