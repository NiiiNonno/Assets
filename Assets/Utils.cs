﻿// 令和弐年大暑確認済。
using System.Reflection;
using System.Xml.Linq;
using System.Text;
using Sys = System.Timers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using MI = System.Runtime.CompilerServices.MethodImplAttribute;
using MIO = System.Runtime.CompilerServices.MethodImplOptions;
#if USE_DOUBLE
using Dec = System.Double;
using Math = System.Math;
#else 
using Dec = System.Single;
using Math = System.MathF;
#endif
using Single = System.Single;
using Double = System.Double;
using System.Runtime.CompilerServices;
using System.Buffers.Binary;
using Nonno.Assets.Scrolls;
using System.Collections.Immutable;
using System.Net;

namespace Nonno.Assets;

public static partial class Utils
{
    readonly static Random RANDOM = new();

    static Utils()
    {
        InitReflection();
    }

    #region Comparison

    /// <summary>
    /// 遍永符を比較します。
    /// </summary>
    /// <param name="this"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public static int Compare(this Guid @this, Guid to)
    {
        Span<byte> span1 = stackalloc byte[16];
        Span<byte> span2 = stackalloc byte[16];
        _ = @this.TryWriteBytes(span1);
        _ = to.TryWriteBytes(span2);

        if (span1[15] > span2[15]) return 16;
        if (span1[15] < span2[15]) return -16;
        if (span1[14] > span2[14]) return 15;
        if (span1[14] < span2[14]) return -15;
        if (span1[13] > span2[13]) return 14;
        if (span1[13] < span2[13]) return -14;
        if (span1[12] > span2[12]) return 13;
        if (span1[12] < span2[12]) return -13;
        if (span1[11] > span2[11]) return 12;
        if (span1[11] < span2[11]) return -12;
        if (span1[10] > span2[10]) return 11;
        if (span1[10] < span2[10]) return -11;
        if (span1[09] > span2[09]) return 10;
        if (span1[09] < span2[09]) return -10;
        if (span1[08] > span2[08]) return 09;
        if (span1[08] < span2[08]) return -09;
        if (span1[07] > span2[07]) return 08;
        if (span1[07] < span2[07]) return -08;
        if (span1[06] > span2[06]) return 07;
        if (span1[06] < span2[06]) return -07;
        if (span1[05] > span2[05]) return 06;
        if (span1[05] < span2[05]) return -06;
        if (span1[04] > span2[04]) return 05;
        if (span1[04] < span2[04]) return -05;
        if (span1[03] > span2[03]) return 04;
        if (span1[03] < span2[03]) return -04;
        if (span1[02] > span2[02]) return 03;
        if (span1[02] < span2[02]) return -03;
        if (span1[01] > span2[01]) return 02;
        if (span1[01] < span2[01]) return -02;
        if (span1[00] > span2[00]) return 01;
        if (span1[00] < span2[00]) return -01;
        return 0;

        //for (int i = span1.Length - 1; i >= 0; i--)
        //{
        //    if (span1[i] > span2[i]) return i + 1;
        //    if (span1[i] < span2[i]) return -i - 1;
        //}
        //return 0;
    }

    #endregion
    #region Array

    /// <summary>
    /// 配列の長さを変え、可能な限り内容を複写します。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static T[] Relength<T>(this T[] @this, int length)
    {
        var r = new T[length];
        for (int i = 0; i < @this.Length; i++) r[i] = @this[i];
        return r;
    }

    public static int GetIndex<T>(this T[] @this, T of) => TryGetIndex(@this, of, out int r) ? r : -1;
    public static int GetIndex<T>(this ReadOnlySpan<T> @this, T of) => TryGetIndex(@this, of, out int r) ? r : -1;
    public static bool TryGetIndex<T>(this T[] @this, T of, out int index) => TryGetIndex(@this, of, out index);
    public static bool TryGetIndex<T>(this ReadOnlySpan<T> @this, T of, out int index)
    {
        if (of == null)
        {
            for (int i = 0; i < @this.Length; i++)
            {
                if (@this[i] == null)
                {
                    index = i;
                    return true;
                }
            }
        }
        else if (of is IEquatable<T> equatable)
        {
            for (int i = 0; i < @this.Length; i++)
            {
                if (equatable.Equals(@this[i]))
                {
                    index = i;
                    return true;
                }
            }
        }
        else
        {
            for (int i = 0; i < @this.Length; i++)
            {
                if (of.Equals(@this[i]))
                {
                    index = i;
                    return true;
                }
            }
        }
        index = -1;
        return false;
    }

    public static void ReplaceFirst<T>(this T[] @this, T old, T neo) => ReplaceFirst<T>(@this, old, neo);
    public static void ReplaceFirst<T>(this Span<T> @this, T old, T neo)
    {
        for (int i = 0; i < @this.Length; i++)
        {
            if (EqualityComparer<T>.Default.Equals(@this[i],old))
            {
                @this[i] = neo;
                return;
            }
        }
    }
    public static void Replace<T>(this T[] @this, T old, T neo) => Replace<T>(@this, old, neo);
    public static void Replace<T>(this Span<T> @this, T old, T neo)
    {
        for (int i = 0; i < @this.Length; i++)
        {
            if (EqualityComparer<T>.Default.Equals(@this[i], old))
            {
                @this[i] = neo;
            }
        }
    }

    #endregion
    #region IO

    /// <summary>
    /// URI文字列からストリームを作成します。
    /// <para>
    /// 'http'スキーム及び'https'、'file'、'data'スキームに対応しています。
    /// </para>
    /// </summary>
    /// <param name="uri">
    /// ストリームへの到達方法を示すURI文字列。
    /// </param>
    /// <returns>
    /// 取得したストリームを返す作業。
    /// </returns>
    public static async Task<Stream> GetStream(string uri) => await GetStream(new Uri(uri));
    /// <summary>
    /// URIからストリームを作成します。
    /// <para>
    /// 'http'スキーム及び'https'、'file'、'data'スキームに対応しています。
    /// </para>
    /// </summary>
    /// <param name="uri">
    /// ストリームへの到達方法を示すURI。
    /// </param>
    /// <returns>
    /// 取得したストリームを返す作業。
    /// </returns>
    public static async Task<Stream> GetStream(Uri uri)
    {
        switch (uri.Scheme)
        {
        case "http":
        case "https":
            {
                var client = new HttpClient();
                HttpResponseMessage responceM = await client.GetAsync(uri, HttpCompletionOption.ResponseContentRead);
                return await responceM.Content.ReadAsStreamAsync();
            }
        case "file":
            {
                if (!uri.IsFile) throw new Exception("不明なエラーです。スキームがfileであるURIがファイルを示していません。");

                return File.Open(uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
        case "data":
            {
                // data:[mime type];[charset or "base64"],[data]

                string uri_ = uri.OriginalString;
                int firstSeparatorI = uri_.IndexOf(';');
                int secondSeparatorI = uri_.IndexOf(',');

                switch (uri_[(firstSeparatorI + 1)..secondSeparatorI])
                {
                case "base64":
                    {
                        return Do(); Stream Do()
                        {
                            ReadOnlySpan<char> span = uri_.AsSpan(secondSeparatorI + 1);
                            byte[] buffeer = new byte[span.Length * 6];
                            if (!Convert.TryFromBase64Chars(span, buffeer.AsSpan(), out _)) throw new Exception("変換に失敗しました。");
                            return new MemoryStream(buffeer);
                        }
                    }
                default:
                    throw new ArgumentException("指定された文字セットはサポートされていません。", nameof(uri));
                }
            }
        default:
            throw new ArgumentException("指定されたURIスキームはサポートされていません。", nameof(uri));
        }
    }

    /// <summary>
    /// ストリームを、ローカルファイルに変換します。
    /// <para>
    /// この操作の完了後、ストリームは破棄されます。
    /// また、ファイルストリームを変換する場合、元のファイルは削除されます。
    /// </para>
    /// </summary>
    /// <param name="this">
    /// 対象のストリーム。
    /// </param>
    /// <param name="fileName">
    /// 変換先のファイルの名前。
    /// </param>
    /// <returns>
    /// 変換後のファイル情報。
    /// </returns>
    public static async Task<FileInfo> ToLocalFile(this Stream @this, string? fileName = null, bool asTemporary = false)
    {
        if (fileName == null && @this is FileStream file)
        {
            return new(file.Name);
        }

        FileInfo fileInfo;

        try
        {
            fileInfo = new FileInfo(fileName ?? Path.GetTempFileName());
            if (asTemporary) fileInfo.Attributes |= FileAttributes.Temporary;
            var fileStream = fileInfo.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            try
            {
                await @this.CopyToAsync(fileStream);
            }
            finally
            {
                fileStream.Dispose();
            }
        }
        finally
        {
            @this.Dispose();
        }

        return fileInfo;
    }

    /// <summary>
    /// 拡展辞を除いた文件品名を取得します。
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static ReadOnlySpan<char> GetFileNameWithoutExtension(this FileSystemInfo @this)
    {
        string name = @this.Name;
        int index = name.LastIndexOf('.');
        if (index >= 0) return name.AsSpan()[..index];
        else return name;
    }

    /// <summary>
    /// 帖子派、先頭に長さを千糸数で示された字符号列を読取ります。
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static string? Read(this Stream @this)
    {
        Span<byte> span = stackalloc byte[sizeof(int)];
        _ = @this.Read(span);
        var length = BitConverter.ToInt32(span);
        if (length < 0)
        {
            return null;
        }
        else
        {
            var r = new string(default, length);
            _ = @this.Read(r.UnsafeAsByteSpan());
            return r;
        }
    }
    /// <summary>
    /// 帖子方、先頭に長さを千糸数で示された字符号列を書込みます。
    /// </summary>
    /// <param name="this"></param>
    /// <param name="value"></param>
    public static void Write(this Stream @this, string? value)
    {
        if (value == null)
        {
            Span<byte> span = stackalloc byte[sizeof(int)];
            _ = BitConverter.TryWriteBytes(span, -1);
            @this.Write(span);
        }
        else
        {
            Span<byte> span = stackalloc byte[sizeof(int)];
            _ = BitConverter.TryWriteBytes(span, value.Length);
            @this.Write(span);
            @this.Write(value.UnsafeAsByteSpan());
        }
    }

    /// <summary>
    /// 科品を複写します。
    /// </summary>
    /// <param name="this"></param>
    /// <param name="to">複写先の文件科目。</param>
    /// <param name="allowOverride">複写先の文件科目既存に上書きを許すや否や。</param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static void Copy(this DirectoryInfo @this, DirectoryInfo to, bool allowOverride = false)
    {
        if (!@this.Exists) throw new InvalidOperationException("複写元の文件科目不在。");
        if (to.Exists) if (allowOverride) to.Delete(true); else throw new ArgumentException("複写先の文件科目既存。");

        to.Create();
        to.Attributes = @this.Attributes;
        to.Refresh();

        CopyFiles(@this, to);

        static void CopyFiles(DirectoryInfo from, DirectoryInfo to)
        {
            foreach (var fI in from.EnumerateFiles())
            {
                _ = fI.CopyTo(Path.Combine(to.FullName, fI.Name));
            }
            foreach (var dI in from.EnumerateDirectories())
            {
                CopyFiles(dI, Directory.CreateDirectory(Path.Combine(to.FullName, dI.Name)));
            }
        }
    }

    public static ConsoleColor ToConsoleColor(this BasicColor @this, ConsoleColor baseColor = ConsoleColor.White) => @this switch
    {
        BasicColor.None => baseColor,
        BasicColor.Black => ConsoleColor.Black,
        BasicColor.Gray => ConsoleColor.Gray,
        BasicColor.White => ConsoleColor.White,
        BasicColor.Red => ConsoleColor.Red,
        BasicColor.Green => ConsoleColor.Green,
        BasicColor.Blue => ConsoleColor.Blue,
        BasicColor.Yellow => ConsoleColor.Yellow,
        BasicColor.Cyan => ConsoleColor.Cyan,
        BasicColor.Magenta => ConsoleColor.Magenta,
        BasicColor.ThinRed => ConsoleColor.DarkRed,
        BasicColor.ThinBlue=> ConsoleColor.DarkBlue,
        BasicColor.ThinGreen => ConsoleColor.DarkGreen,
        BasicColor.ThinYellow => ConsoleColor.DarkYellow,
        BasicColor.ThinCyan => ConsoleColor.DarkCyan,
        BasicColor.ThinMagenta => ConsoleColor.DarkMagenta,
        _ => throw new UndefinedEnumerationValueException(nameof(@this), typeof(BasicColor))
    };

    #endregion
    #region Reflection

    /// <summary>
    /// プロパティ情報からプロパティキャプチャを作成します。
    /// </summary>
    /// <param name="this">
    /// 扱うプロパティ情報。
    /// </param>
    /// <param name="target">
    /// プロパティを捕捉される対象。
    /// </param>
    /// <returns>
    /// 作成されたプロパティキャプチャ。
    /// </returns>
    public static object CreateCapture(this PropertyInfo @this, object? target) => Activator.CreateInstance(typeof(PropertyCapture<>).MakeGenericType(@this.PropertyType), @this, target) ?? throw new Exception("指定されたコンストラクターが存在しない、予期しないエラーです。");

    static readonly ReaderWriterLockSlim ALL_TYPES_LOCK = new();
    static readonly List<TypeInfo> ALL_TYPES = new(AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.DefinedTypes));

    private static void InitReflection()
    {
        AppDomain.CurrentDomain.AssemblyLoad += (_, e) =>
        {
            try
            {
                ALL_TYPES_LOCK.EnterWriteLock();
                foreach (var typeInfo in e.LoadedAssembly.DefinedTypes)
                {
                    ALL_TYPES.Add(typeInfo);
                }
            }
            finally
            {
                ALL_TYPES_LOCK.ExitWriteLock();
            }
        };
    }

    /// <summary>
    /// 派生する型を列挙します。
    /// </summary>
    /// <param name="this">
    /// </param>
    /// <returns>
    /// 派生する型の列。
    /// </returns>
    public static IEnumerable<Type> GetInheritedTypes(this Type @this)
    {
        try
        {
            ALL_TYPES_LOCK.EnterReadLock();
            return ALL_TYPES.Where(type => type.IsSubclassOf(@this)).ToArray();
        }
        finally
        {
            ALL_TYPES_LOCK.ExitReadLock();
        }
    }

    /// <summary>
    /// 実装する型を列挙します。
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetAssignableTypes(this Type @this)
    {
        try
        {
            ALL_TYPES_LOCK.EnterReadLock();
            return ALL_TYPES.Where(type => type.IsAssignableTo(@this));
        }
        finally
        {
            ALL_TYPES_LOCK.ExitReadLock();
        }
    }

    /// <summary>
    /// 型の、指定した基底型における泛型引型列を取得覓ます。
    /// </summary>
    /// <param name="this">
    /// 対象の型。
    /// </param>
    /// <param name="as">
    /// 泛型引型列を取得する、対象の型の基底型。
    /// </param>
    /// <param name="results">
    /// 指定した型における泛型引型列。
    /// </param>
    /// <returns>
    /// 取得の験。
    /// </returns>
    public static bool TryGetGenericArguments(this Type? @this, Type @as, [MaybeNullWhen(false)] out Type[] results)
    {
        results = null;

        if (!@as.IsGenericType) return false;
        while (@this != null)
        {
            if (@this.IsGenericType && @this.GetGenericTypeDefinition() == @as)
            {
                results = @this.GetGenericArguments();
                return true;
            }
            @this = @this.BaseType;
        }
        return false;
    }
    /// <summary>
    /// 型の、指定した基底型における泛型引型列を取得します。
    /// </summary>
    /// <param name="this">
    /// 対象の型。
    /// </param>
    /// <param name="as">
    /// 引型を取得する、対象の型の基底型。
    /// </param>
    /// <returns>
    /// 指定した型における泛型引型列。
    /// </returns>
    public static Type[] GetGenericArguments(this Type? @this, Type @as)
    {
        if (!@as.IsGenericType) throw new ArgumentException("指定した基底型はジェネリック型ではありません。");
        while (@this != null)
        {
            if (@this.IsGenericType && @this.GetGenericTypeDefinition() == @as) return @this.GetGenericArguments();
            @this = @this.BaseType;
        }
        throw new ArgumentException("この型は指定された型のいずれのジェネリック型からも派生しません。", nameof(@this));
    }

    /// <summary>
    /// 配列、番地、参照の素の型を取得します。
    /// </summary>
    /// <param name="this"></param>
    /// <param name="throwOnNull">取得の失敗に異常を発するか。</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static Type GetElementType(this Type @this, bool throwOnNull = false)
    {
        var r = @this.GetElementType();
        return r is null ? throwOnNull ? throw new InvalidOperationException("型は要素の型を包含していません。") : @this : r;
    }

    /// <summary>
    /// 全ての基底型を列挙します。
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetBaseTypes(this Type @this)
    {
        var c = @this;
        do
        {
            yield return c;
            c = c.BaseType;
        } while (c is not null);
    }

    /// <summary>
    /// 型が待機可能である場合に、待機した戻り値を取得します。
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static Type? GetAwaitResult(this Type @this) => 
        @this.GetMethod("GetAwaiter", BindingFlags.Public | BindingFlags.Instance, Array.Empty<Type>()) is { } gAMI &&
        gAMI.ReturnType.GetMethod("OnCompleted", BindingFlags.Public | BindingFlags.Instance, ARRAY1_TYPEOF_ACTION) is { } oCMI &&
        oCMI.ReturnType == typeof(void) &&
        gAMI.ReturnType.GetMethod("GetResult", BindingFlags.Public | BindingFlags.Instance, Array.Empty<Type>()) is { } gRMI &&
        gAMI.ReturnType.GetProperty("IsCompleted", BindingFlags.Public | BindingFlags.Instance) is { } iCPI ? gRMI.ReturnType : null;
    static readonly Type[] ARRAY1_TYPEOF_ACTION = new[] { typeof(Action) };

    public static Type GetType(Guid guid)
    {
        if (!TYPE_DICTIONARY.TryGetValue(guid, out  var r))
        {
            r = ALL_TYPES.Find(x => x.GUID == guid);
            if (r is null) throw new KeyNotFoundException();
            TYPE_DICTIONARY.Add(guid, r);
        }

        return r;
    }
    static readonly Dictionary<Guid, Type> TYPE_DICTIONARY = new();

    #endregion
    #region Deconstruction

    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> from, out TKey key, out TValue value)
    {
        key = from.Key;
        value = from.Value;
    }
    [MethodImpl(MIO.AggressiveInlining)]
    public static unsafe void Deconstruct(this float @this, out bool sign, out int exponent, out uint significand)
    {
        uint v1 = *(uint*)&@this;
        uint v2 = v1 & 0x7FFF_FFFF;
        sign = v1 != v2;
        exponent = unchecked((int)(v2 >> 23)) - 127;
        significand = v2 & 0x007F_FFFF;
    }
    [MethodImpl(MIO.AggressiveInlining)]
    public static unsafe void Deconstruct(this double @this, out bool sign, out int exponent, out ulong significand)
    {
        ulong v1 = *(ulong*)&@this;
        ulong v2 = v1 & 0x7FFF_FFFF_FFFF_FFFF;
        sign = v1 != v2;
        exponent = unchecked((int)(v2 >> 52)) - 1023;
        significand = v2 & 0x000F_FFFF_FFFF_FFFF;
    }

    #endregion
    #region Unsafe

    /// <summary>
    /// 字符号列を整十数刪にします。
    /// <para>
    /// このメソッドは実質非安全です。
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// 区間の型。
    /// </typeparam>
    /// <param name="this">
    /// 扱うコード列。
    /// </param>
    /// <param name="length">
    /// 区間のバイト長。
    /// </param>
    /// <returns>
    /// 作成した区間。
    /// </returns>
    public unsafe static Span<byte> UnsafeAsByteSpan(this string @this) => @this.UnsafeAsSpan<byte>(@this.Length << 1);
    /// <summary>
    /// コード列を区間にします。
    /// <para>
    /// このメソッドは実質非安全です。
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// 区間の型。
    /// </typeparam>
    /// <param name="this">
    /// 扱うコード列。
    /// </param>
    /// <param name="length">
    /// 区間のバイト長。
    /// </param>
    /// <returns>
    /// 作成した区間。
    /// </returns>
    public unsafe static Span<T> UnsafeAsSpan<T>(this string @this, int length)
    {
        fixed (char* ptr = @this) return new Span<T>(ptr, length);
    }

    [Obsolete("`Utils.Is`を使用する方法が推奨されています。")]
    public unsafe static Span<T> As<TParam, T>(this Span<TParam> @this) where TParam : unmanaged where T : unmanaged
    {
        if (default(T) is not TParam) throw new ArgumentException("異なる型引数の区間を処理することはできません。");
        fixed (TParam* p = @this) return new Span<T>(p, @this.Length);
    }
    [MI(MIO.AggressiveInlining)]
    public unsafe static bool Is<TParam, T>(this Span<TParam> @this, out Span<T> result) where TParam : unmanaged where T : unmanaged
    {
        if (default(T) is not TParam)
        {
            result = default(Span<T>);
            return false;
        }
        else
        {
            fixed (TParam* p = @this) result = new Span<T>(p, @this.Length);
            return true;
        }
    }

    /// <summary>
    /// 構造体を範囲に転写します。構造体の大きさに対して範囲が半端な値しか取れないときは構造体の一部を損失します。
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <typeparam name="TTo"></typeparam>
    /// <param name="this"></param>
    /// <returns></returns>
    [MI(MIO.AggressiveInlining)]
    public unsafe static Span<TTo> AsSpan<TFrom, TTo>(this TFrom @this) where TFrom : unmanaged where TTo : unmanaged
    {
        var l = sizeof(TFrom) / sizeof(TTo);
        if (sizeof(TFrom) != sizeof(TTo) * l) ThrowHelper.StructureSizesDoesNotMatch(typeof(TFrom), typeof(TTo));

        return new Span<TTo>(&@this, sizeof(TFrom) / sizeof(TTo));
    }
    [MI(MIO.AggressiveInlining)]
    public unsafe static TTo AsStruct<TFrom, TTo>(this Span<TFrom> @this) where TFrom : unmanaged where TTo : unmanaged
    {
        if (sizeof(TTo) > sizeof(TFrom) * @this.Length) ThrowHelper.SpanSizeIsTooShort<TFrom>(@this);

        TTo r;
        fixed (TFrom* ptr = @this) r = *(TTo*)ptr;
        return r;
    }
    [MI(MIO.AggressiveInlining)]
    public unsafe static TTo AsStruct<TFrom, TTo>(this ReadOnlySpan<TFrom> @this) where TFrom : unmanaged where TTo : unmanaged
    {
        if (sizeof(TTo) > sizeof(TFrom) * @this.Length) ThrowHelper.SpanSizeIsTooShort(@this);

        TTo r;
        fixed (TFrom* ptr = @this) r = *(TTo*)ptr;
        return r;
    }
    [MI(MIO.AggressiveInlining)]
    public unsafe static Span<TTo> ToSpan<TFrom, TTo>(this Span<TFrom> @this) where TFrom : unmanaged where TTo : unmanaged
    {
        if (@this.Length * sizeof(TFrom) % sizeof(TTo) != 0) ThrowHelper.StructureSizesDoesNotMatch(typeof(TFrom), typeof(TTo));

        Span<TTo> r;
        fixed (TFrom* ptr = @this) r = new(ptr, @this.Length * sizeof(TFrom) / sizeof(TTo));
        return r;
    }

    /// <summary>
    /// 符号ビットを反転します。実数部は反転しません。
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public unsafe static int ReverseSign(this int @this)
    {
        unchecked
        {
            return (int)((uint)@this ^ 0x80000000);
        }
    }

    public unsafe static int SizeOf<T>() where T : unmanaged => sizeof(T);

    /// <summary>
    /// 最低文字列長を指定して文字列型のバッファを取得します。
    /// </summary>
    /// <param name="length">
    /// バッファのバイト長。
    /// </param>
    /// <returns></returns>
    public static string CreateStringBuffer(int length) => new(default, length >> 1);

    public static Dec MakeFloat(int value, int max, bool signed)
    {
        return 0 <= value && value < max
            ? signed ? Math.Tan((value - max * (Dec)0.5) * Math.PI / max) : Math.Tan(value * Math.PI * (Dec)0.5 / max)
            : throw new ArgumentException($"パラメーターには0~{max}の値が必要です。", nameof(value));
    }

    #endregion
    #region Enumerator

    public static IEnumerator<TTo> Select<TFrom, TTo>(this IEnumerator<TFrom> @this, Func<TFrom, TTo> selector)
    {
        while (@this.MoveNext()) yield return selector(@this.Current);
    }

    public static void Foreach<T>(this IEnumerator<T> @this, Action<T> action)
    {
        while (@this.MoveNext()) action(@this.Current);
    }

    /// <summary>
    /// 列挙子の値を指定された範囲に可能な限りで複製します。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="span"></param>
    /// <returns>範囲を最後まで上書きすることができた場合はnull、列挙子の値の個数が小さくて、最後まで埋まらなかった場合は埋めた個数。</returns>
    public static int? Copy<T>(this IEnumerator<T> @this, Span<T> span)
    {
        for (int i = 0; i < span.Length; i++)
        {
            if (!@this.MoveNext()) return i;
            span[i] = @this.Current;
        }
        return null;
    }

    public static async IAsyncEnumerator<string> EnumerateAllLinesAsync(this TextReader @this)
    {
        var r = await @this.ReadLineAsync();
        if (r != null) yield return r;
        else yield break;
    }

    /// <summary>
    /// 負の値を無照扱いします。
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static int? CastToNonNegative(this int @this) => @this < 0 ? null : @this;
    /// <summary>
    /// 無照を負の値扱いします。
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static int CastToNonNull(this int? @this) => @this.HasValue ? @this.Value < 0 ? throw new ArgumentOutOfRangeException("正の整数を指定してください。") : @this.Value : -1;

    #endregion
    #region Document

    public static string Ditto(this string @this, int count)
    {
        var r = new StringBuilder(@this.Length * count);
        for (int i = 0; i < count; i++) _ = r.Append(@this);
        return r.ToString();
    }

    public static dynamic AsDynamic(this XElement @this) => new DynamicXElement(@this);
    public static dynamic AsDynamic(this XDocument @this) => new DynamicXElement(@this);
    /// <summary>
    /// 文字列を文章に変換します。
    /// </summary>
    /// <param name="this">
    /// 扱う文字列。
    /// </param>
    /// <returns>
    /// 作成された文章。
    /// </returns>
    [Obsolete("XElementもしくはXDoxument空の作成が推奨されています。")]
    public static DynamicXElement AsDocument(this string @this) => new(XDocument.Parse(@this));

    /// <summary>
    /// 文字列の属性をマークアップ形式に変換します。
    /// </summary>
    /// <param name="this">
    /// 扱う文字列。
    /// </param>
    /// <param name="func">
    /// 変換方式を指定する代理人。
    /// </param>
    /// <returns>
    /// 変換された文字列。
    /// </returns>
    public static string Mark(this string @this, Func<string, (string begin, string end)> func)
    {
        while (true)
        {
            int markI = @this.IndexOf(' '), beginI = @this.IndexOf('('), endI = @this.IndexOf(')');

            if (beginI < 0 || endI < 0) break;

            string attributes = @this.Substring(beginI + 1, endI - beginI - 1);
            string begin = "", end = "";
            foreach (var attribute in attributes.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var r = func(attribute);
                begin += begin + r.begin;
                end = r.end + end;
            }

            @this = @this.Remove(beginI, endI - beginI + 1).Insert(beginI, end).Insert(markI + 1, begin);
            if (markI >= 0) @this = @this.Remove(markI, 1);
        }

        return @this;
    }

    public static XElement ToXElement(this Exception @this)
    {
        var element = new XElement("Exception");
        element.Add(
            new XElement("Message", @this.Message),
            new XElement("StackTrace", @this.StackTrace),
            new XElement("Source", @this.Source),
            GetInnerExceptions(),
            GetData());
        return element;

        XElement GetInnerExceptions()
        {
            var element = new XElement("List");
            element.SetAttributeValue("Type", "Ordered");
            Exception? e = @this;
            while (e != null)
            {
                element.Add(new XElement("Item", e.Message));
                e = e.InnerException;
            }
            return element;
        }

        XElement GetData()
        {
            var element = new XElement("Dictionary");
            foreach (var key in @this.Data.Keys)
            {
                var item = new XElement("Item", @this.Data[key]?.ToString());
                item.SetAttributeValue("Key", key.ToString());
                element.Add(item);
            }
            return element;
        }
    }

    #endregion
    #region Collection

    public static void SetValue<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue value)
    {
        if (!@this.TryAdd(key, value)) @this[key] = value;
    }
    public static void SetValue<TKey, TValue>(this Dictionary<TKey, TValue> @this, TKey key, TValue value) where TKey : notnull
    {
        if (!@this.TryAdd(key, value)) @this[key] = value;
    }

    public static void Copy<T>(this IEnumerable<T> @this, Span<T> to, ref int index)
    {
        foreach (var item in @this)
        {
            to[index++] = item;
        }
    }

    #endregion
    #region BitConverter

    public static void Copy(this ulong @this, Span<byte> to) => Copy(@this, to, BitConverter.IsLittleEndian);
    public static void Copy(this ulong @this, Span<byte> to, bool withLittleEndian)
    {
        if (withLittleEndian) BinaryPrimitives.WriteUInt64LittleEndian(to, @this);
        else BinaryPrimitives.WriteUInt64BigEndian(to, @this);
    }
    public static void Copy(this uint @this, Span<byte> to) => Copy(@this, to, BitConverter.IsLittleEndian);
    public static void Copy(this uint @this, Span<byte> to, bool withLittleEndian)
    {
        if (withLittleEndian) BinaryPrimitives.WriteUInt32LittleEndian(to, @this);
        else BinaryPrimitives.WriteUInt32BigEndian(to, @this);
    }
    public static void Copy(this ushort @this, Span<byte> to) => Copy(@this, to, BitConverter.IsLittleEndian);
    public static void Copy(this ushort @this, Span<byte> to, bool withLittleEndian)
    {
        if (withLittleEndian) BinaryPrimitives.WriteUInt16LittleEndian(to, @this);
        else BinaryPrimitives.WriteUInt16BigEndian(to, @this);
    }

    public static ushort AsUInt16(this ReadOnlySpan<byte> @this) => BinaryPrimitives.ReadUInt16LittleEndian(@this);
    public static uint AsUInt32(this ReadOnlySpan<byte> @this) => BinaryPrimitives.ReadUInt32LittleEndian(@this);
    public static ulong AsUint64(this ReadOnlySpan<byte> @this) => BinaryPrimitives.ReadUInt64LittleEndian(@this);

    public static bool IsDefault(this Dec @this)
    {
#if USE_DOUBLE
            return BitConverter.DoubleToInt64Bits(@this) == 0;
#else
        return BitConverter.SingleToInt32Bits(@this) == 0;
#endif
    }

    #endregion
    #region Task

    /// <summary>
    /// <see cref="Task.FromCanceled(CancellationToken)"/>を使用する際に同時に行われる値の<see cref="default"/>設定を同時に行う便利務容です。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cancellationToken"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static Task FromCanceled<T>(CancellationToken cancellationToken, out T value)
    {
        value = default!;
        return Task.FromCanceled(cancellationToken);
    }

    public static ValueTask<T> GetNeverendingTask<T>() => new(NeverendingTaskCacheSingleton<T>.INSTANCE.task);
    class NeverendingTaskCacheSingleton<T>
    {
        public static readonly NeverendingTaskCacheSingleton<T> INSTANCE = new();

        public Task<T> task;

        public NeverendingTaskCacheSingleton()
        {
            task = Task.Factory.StartNew<T>(() =>
            {
                var sw = new SpinWait();
                while (true) sw.SpinOnce();
            }, TaskCreationOptions.LongRunning);
        }
    }

    #endregion
    #region Math

    public static UInt32 ReverseSequence(UInt32 of)
    {
        UInt32 r = 0;
        for (int i = 0; i < 32; i++)
        {
            r |= of & 1u;
            of >>= 1;
            r <<= 1;
        }
        return r;
    }

    [MI(MIO.AggressiveInlining)]
    public static int AverageFloor(int of1, int of2)
    {
        var r = (of1 >> 1) + (of2 >> 1);
        if ((of1 & 1) != 0 && (of2 & 1) != 0) r++;
        return r;
    }
    [MI(MIO.AggressiveInlining)]
    public static int AverageCeiling(int of1, int of2)
    {
        var r = (of1 >> 1) + (of2 >> 1);
        if ((of1 & 1) != 0 || (of2 & 1) != 0) r++;
        return r;
    }
    [MI(MIO.AggressiveInlining)]
    public static long AverageCeiling(long of1, long of2)
    {
        var r = (of1 >> 1) + (of2 >> 1);
        if ((of1 & 1) != 0 || (of2 & 1) != 0) r++;
        return r;
    }

    public static uint GetCyclicRedundancyCheck(uint magicNumber, IEnumerable<byte> data)
    {
        // 参見:https://qiita.com/mikecat_mixc/items/e5d236e3a3803ef7d3c5

        uint r = ~0u;
        uint m = ReverseSequence(of: magicNumber);
        var table = new uint[256];

        for (uint i = 0; i < table.Length; i++)
        {
            uint v = i;
            for (int j = 0; j < 8; j++)
            {
                uint b = v & 1u;
                v >>= 1;
                if (b != 0) v ^= m; 
            }
            table[i] = v;
        }

        foreach (var item in data) r = table[(r ^ item) & 0xFF] ^ (r >> 8);
        return ~r;
    }

    public static float Det(in ((Dec, Dec), (Dec, Dec)) mat)
    {
        return mat.Item1.Item1 * mat.Item2.Item2 - mat.Item1.Item2 * mat.Item2.Item1;
    }
    public static float Det(in ((Dec, Dec, Dec), (Dec, Dec, Dec), (Dec, Dec, Dec)) mat)
    {
        var a = (
            (mat.Item2.Item2, mat.Item2.Item3),
            (mat.Item3.Item2, mat.Item3.Item3));
        var b = (
            (mat.Item1.Item2, mat.Item1.Item3),
            (mat.Item3.Item2, mat.Item3.Item3));
        var c = (
            (mat.Item1.Item2, mat.Item1.Item3),
            (mat.Item2.Item2, mat.Item2.Item3));

        return mat.Item1.Item1 * Det(a) - mat.Item2.Item1 * Det(b) + mat.Item3.Item1 * Det(c);
    }
    public static float Det(in ((Dec, Dec, Dec, Dec), (Dec, Dec, Dec, Dec), (Dec, Dec, Dec, Dec), (Dec, Dec, Dec, Dec)) mat)
    {
        var a = (
            (mat.Item2.Item2, mat.Item2.Item3, mat.Item2.Item4),
            (mat.Item3.Item2, mat.Item3.Item3, mat.Item3.Item4),
            (mat.Item4.Item2, mat.Item4.Item3, mat.Item4.Item4));
        var b = (
            (mat.Item1.Item2, mat.Item1.Item3, mat.Item1.Item4),
            (mat.Item3.Item2, mat.Item3.Item3, mat.Item3.Item4),
            (mat.Item4.Item2, mat.Item4.Item3, mat.Item4.Item4));
        var c = (
            (mat.Item1.Item2, mat.Item1.Item3, mat.Item1.Item4),
            (mat.Item2.Item2, mat.Item2.Item3, mat.Item2.Item4),
            (mat.Item4.Item2, mat.Item4.Item3, mat.Item4.Item4));
        var d = (
            (mat.Item1.Item2, mat.Item1.Item3, mat.Item1.Item4),
            (mat.Item2.Item2, mat.Item2.Item3, mat.Item2.Item4),
            (mat.Item3.Item2, mat.Item3.Item3, mat.Item3.Item4));

        return mat.Item1.Item1 * Det(a) - mat.Item2.Item1 * Det(b) + mat.Item3.Item1 * Det(c) - mat.Item4.Item1 * Det(d);
    }

    #endregion
    #region Sources

    public static string GetPointSource(int dimension, string typeName) =>
$@"[Serializable]
public readonly struct Point{dimension}
{{
    public readonly {typeName} {Join(", ", dimension, i => "c" + i)};
    public bool IsZero
    {{
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => {Join(" && ", dimension, i => $"c{i} == 0")};
    }}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point{dimension}({Join(", ", dimension, i => $"{typeName} c{i}")})
    {{
        {Join("\n", dimension, i => $"this.c{i} = c{i};")}
    }}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is Point{dimension} p && {Join(" && ", dimension, i => $"p.c{i} == c{i}")};
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => {(dimension <= 8 ? $"HashCode.Combine({Join(", ", dimension, i => $"c{i}")})" : Join(" ^ ", dimension, i => $"c{i}.GetHashCode()"))};
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $""({Join(", ", dimension, i => $"{{c{i}}}")})"";
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point{dimension} operator +(in Point{dimension} left, in Vector{dimension} right) => new({Join(", ", dimension, i => $"left.c{i} + right.c{i}")});
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point{dimension} operator -(in Point{dimension} left, in Vector{dimension} right) => new({Join(", ", dimension, i => $"left.c{i} - right.c{i}")});
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector{dimension} operator -(in Point{dimension} left, in Point{dimension} right) => new({Join(", ", dimension, i => $"left.c{i} - right.c{i}")});
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct({Join(", ", dimension, i => $"out {typeName} c{i}")})
    {{
        {Join("\n", dimension, i => $"c{i} = this.c{i};")}
    }}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Point{dimension}(in ({Join(", ", dimension, i => $"{typeName} c{i}")}) p) => new({Join(", ", dimension, i => $"p.c{i}")});

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Span<byte> to)
    {{
        {Join("\n", dimension, i => $"_ = BitConverter.TryWriteBytes(to[({i} * sizeof({typeName}))..], c{i});")}
    }}

    [Serializable]
    public readonly struct Recipro
    {{
        public readonly {typeName} {Join(", ", dimension, i => $"r{i}")};
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Recipro({Join(", ", dimension, i => $"{typeName} r{i}")})
        {{
            {Join("\n", dimension, i => $"this.r{i} = r{i};")}
        }}
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is Recipro p && {Join(" && ", dimension, i => $"p.r{i} == r{i}")};
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => {(dimension <= 8 ? $"HashCode.Combine({Join(", ", dimension, i => $"r{i}")})" : Join(" ^ ", dimension, i => $"r{i}.GetHashCode()"))};
        public override string ToString() => $""({Join(", ", dimension, i => $"{{1 / r{i}}}")})"";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Recipro(in Point{dimension} p) => new({Join(", ", dimension, i => $"1 / p.c{i}")});
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Point{dimension}(in Recipro r) => new({Join(", ", dimension, i => $"1 / r.r{i}")});
    }}
}}";

    public static string GetVectorSource(int dimension, string typeName, bool difineShiftOperator = true) =>
$@"[Serializable]
public readonly struct Vector{dimension}
{{
    public readonly {typeName} {Join(", ", dimension, i => $"c{i}")};
    public bool IsZero
    {{
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => {Join(" && ", dimension, i => $"c{i} == 0")};
    }}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector{dimension}({Join(", ", dimension, i => $"{typeName} c{i}")})
    {{
        {Join("\n", dimension, i => $"this.c{i} = c{i};")}
    }}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is Vector{dimension} p && {Join(" && ", dimension, i => $"p.c{i} == c{i}")};
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => {(dimension <= 8 ? $"HashCode.Combine({Join(", ", dimension, i => $"c{i}")})" : Join(" ^ ", dimension, i => $"c{i}.GetHashCode()"))};
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $""({Join(", ", dimension, i => $"{{c{i}}}")})"";
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector{dimension} operator -(in Vector{dimension} vector) => new({Join(", ", dimension, i => $"-vector.c{i}")});
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector{dimension} operator +(in Vector{dimension} left, in Vector{dimension} right) => new({Join(", ", dimension, i => $"left.c{i} + right.c{i}")});
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector{dimension} operator -(in Vector{dimension} left, in Vector{dimension} right) => new({Join(", ", dimension, i => $"left.c{i} - right.c{i}")});
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector{dimension} operator *({typeName} left, in Vector{dimension} right) => new({Join(", ", dimension, i => $"left * right.c{i}")});
{(!difineShiftOperator ? "// Shift operator is not defined." :
$@"   [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector{dimension} operator *(Shift left, in Vector{dimension} right) => new({Join(", ", dimension, i => $"left * right.c{i}")});")}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct({Join(", ", dimension, i => $"out {typeName} c{i}")})
    {{
        {Join("\n", dimension, i => $"c{i} = this.c{i};")}
    }}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector{dimension}(in ({Join(", ", dimension, i => $"{typeName} c{i}")}) p) => new({Join(", ", dimension, i => $"p.c{i}")});

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Span<byte> to)
    {{
        {Join("\n", dimension, i => $"_ = BitConverter.TryWriteBytes(to[({i} * sizeof({typeName}))..], c{i});")}
    }}

    [Serializable]
    public readonly struct Recipro
    {{
        public readonly {typeName} {Join(", ", dimension, i => $"r{i}")};
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Recipro({Join(", ", dimension, i => $"{typeName} r{i}")})
        {{
            {Join("\n", dimension, i => $"this.r{i} = r{i};")}
        }}
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is Recipro p && {Join(" && ", dimension, i => $"p.r{i} == r{i}")};
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => {(dimension <= 8 ? $"HashCode.Combine({Join(", ", dimension, i => $"r{i}")})" : Join(" ^ ", dimension, i => $"r{i}.GetHashCode()"))};
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $""({Join(", ", dimension, i => $"{{1 / r{i}}}")})"";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Recipro(in Vector{dimension} v) => new({Join(", ", dimension, i => $"1 / v.c{i}")});
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector{dimension}(in Recipro r) => new({Join(", ", dimension, i => $"1 / r.r{i}")});
    }}
}}";

	#endregion
	#region String

	/// <summary>
	/// 英字の小文字と西昼数字の指定した長さの無作為な組み合わせを取得します。
	/// </summary>
	/// <param name="length">
	/// 返す文字列の長さ。
	/// </param>
	/// <returns>指定した長さの無作為な英数字の組み合わせ。</returns>
	public static string GetRandomAlphamericString(int length)
    {
        Span<char> span = stackalloc char[length];
        for (int i = 0; i < span.Length; i++)
        {
            int v = RANDOM.Next('0', '9' + ('z' - 'a') + 1);
            if ('9' < v) v += 'a' - '9';
            span[i] = (char)v;
        }
        return new string(span);
    }

    public static bool TryParse(string? @this, ParameterInfo @for, out object? result) => TryParse(@this, @for.ParameterType, out result, @for.DefaultValue);
    public static bool TryParse(string? @this, Type type, out object? result, object? defaultValue = null)
    {
        switch (Type.GetTypeCode(type))
        {
        case TypeCode.Boolean:
            {
                if (@this != null)
                {
                    if (Boolean.TryParse(@this, out var result_))
                    {
                        result = result_;
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
                goto default;
            }
        case TypeCode.Char:
            {
                if (@this != null)
                {
                    if (Char.TryParse(@this, out var result_))
                    {
                        result = result_;
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
                goto default;
            }
        case TypeCode.SByte:
            {
                if (@this != null)
                {
                    if (SByte.TryParse(@this, out var result_))
                    {
                        result = result_;
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
                goto default;
            }
        case TypeCode.Byte:
            {
                if (@this != null)
                {
                    if (Byte.TryParse(@this, out var result_))
                    {
                        result = result_;
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
                goto default;
            }
        case TypeCode.Int16:
            {
                if (@this != null)
                {
                    if (Int16.TryParse(@this, out var result_))
                    {
                        result = result_;
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
                goto default;
            }
        case TypeCode.UInt16:
            {
                if (@this != null)
                {
                    if (UInt16.TryParse(@this, out var result_))
                    {
                        result = result_;
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
                goto default;
            }
        case TypeCode.Int32:
            {
                if (@this != null)
                {
                    if (Int32.TryParse(@this, out var result_))
                    {
                        result = result_;
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
                goto default;
            }
        case TypeCode.UInt32:
            {
                if (@this != null)
                {
                    if (UInt32.TryParse(@this, out var result_))
                    {
                        result = result_;
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
                goto default;
            }
        case TypeCode.Int64:
            {
                if (@this != null)
                {
                    if (Int64.TryParse(@this, out var result_))
                    {
                        result = result_;
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
                goto default;
            }
        case TypeCode.UInt64:
            {
                if (@this != null)
                {
                    if (UInt64.TryParse(@this, out var result_))
                    {
                        result = result_;
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
                goto default;
            }
        case TypeCode.Single:
            {
                if (@this != null)
                {
                    if (Single.TryParse(@this, out var result_))
                    {
                        result = result_;
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
                goto default;
            }
        case TypeCode.Double:
            {
                if (@this != null)
                {
                    if (Double.TryParse(@this, out var result_))
                    {
                        result = result_;
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
                goto default;
            }
        case TypeCode.Decimal:
            {
                if (@this != null)
                {
                    if (Decimal.TryParse(@this, out var result_))
                    {
                        result = result_;
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
                goto default;
            }
        case TypeCode.DateTime:
            {
                if (@this != null)
                {
                    if (DateTime.TryParse(@this, out var result_))
                    {
                        result = result_;
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
                goto default;
            }
        case TypeCode.String:
            {
                if (@this != null)
                {
                    result = @this;
                    return true;
                }
                goto default;
            }
        default:
            {
                if (@this is "null" or "default")
                {
                    result = null;
                    return true;
                }
                else
                {
                    result = defaultValue;
                    return true;
                }
            }
        }

        result = null;
        return false;
    }

    public static string TrimEnd(this string @this, string @string)
    {
        if (@this.EndsWith(@string)) return @this[..^@string.Length];
        else return @this;
    }
    public static string TrimStart(this string @this, string @string)
    {
        if (@this.StartsWith(@string)) return @this[@string.Length..];
        else return @this;
    }

    public static string Join(string separator, int length, Func<int, string> func)
    {
        if (length == 0) return String.Empty;
        StringBuilder r_builder = new(func(0));
        for (int i = 1; i < length; i++)
        {
            _ = r_builder.Append(separator);
            _ = r_builder.Append(func(i));
        }
        return r_builder.ToString();
    }

    public static string Simplify(this string @this)
    {
        StringBuilder builder = new();

        foreach (var @char in @this)
        {
            switch ((ushort)@char)
            {
            case '\b':
                if (builder.Length > 0) _ = builder.Remove(builder.Length - 1, 1);
                break;
            case '\t':
                _ = builder.Append('\t');
                break;
            case '\n':
                _ = builder.Append('\n');
                break;
            case '\v':
                _ = builder.Append('\v');
                break;
            case < 0x20:
                break;
            case 0x7F:
                break;
            default:
                _ = builder.Append(@char);
                break;
            }
        }

        return builder.ToString();
    }

    public static ImmutableArray<ColoredCharacter> ToColoredString(this string? @this, BasicColor foregroundColor = BasicColor.None, BasicColor backgroundColor = BasicColor.None) => @this is null ? ImmutableArray<ColoredCharacter>.Empty : ToColoredStringBuilder(@this, foregroundColor, backgroundColor).ToImmutable();
    private static ImmutableArray<ColoredCharacter>.Builder ToColoredStringBuilder(this string @this, BasicColor foregroundColor = BasicColor.None, BasicColor backgroundColor = BasicColor.None)
    {
		var builder = GetColoredStringBuilder();
		var span = @this.AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			builder.Add(new(span[i], foregroundColor, backgroundColor));
		}
        return builder;
	}
    public static ImmutableArray<ColoredCharacter>.Builder GetColoredStringBuilder() => ImmutableArray.CreateBuilder<ColoredCharacter>();
	public static void Append(this ImmutableArray<ColoredCharacter>.Builder @this, string? @string, BasicColor foregroundColor = BasicColor.None, BasicColor backgroundColor = BasicColor.None)
    {
        if (@string is null) return;

		var span = @string.AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			@this.Add(new(span[i], foregroundColor, backgroundColor));
		}
	}

	#endregion
	#region Char

	public static bool IsParenthesis(this char @this) => @this switch
    {
        '\"' => true,
        '\'' => true,
        _ => false
    };

    public static bool IsSeparator(char @this)
    {
        switch (@this)
        {
        case '\u0009':
        case '\u000A':
        case '\u000B':
        case '\u000C':
        case '\u000D':
        case '\u000E':
        case '\u000F':
        case '\u0010':
        case '\u0011':
        case '\u0012':
        case '\u0013':
        case '\u0014':
        case '\u0015':
        case '\u0016':
        case '\u0017':
        case '\u0018':
        case '\u0019':
        case '\u001A':
        case '\u001B':
        case '\u001C':
        case '\u001D':
        case '\u001E':
        case '\u001F':
        case '\u0020':
        //
        case '\u007F':
        //
        case '\u0085':
        //
        case '\u00A0':
        //
        case '\u05C1':
        case '\u05C2':
        //
        case '\u0701':
        case '\u0702':
        case '\u0703':
        case '\u0704':
        case '\u0705':
        case '\u0706':
        case '\u0707':
        case '\u0708':
        case '\u0709':
        case '\u070A':
        case '\u070B':
        case '\u070C':
        case '\u070D':
        //
        case '\u07B2':
        case '\u07B3':
        case '\u07B4':
        case '\u07B5':
        case '\u07B6':
        case '\u07B7':
        case '\u07B8':
        case '\u07B9':
        case '\u07BA':
        case '\u07BB':
        case '\u07BC':
        case '\u07BD':
        case '\u07BE':
        case '\u07BF':
        //
        case '\u0E3F':
        //
        case '\u2000':
        case '\u2001':
        case '\u2002':
        case '\u2003':
        case '\u2004':
        case '\u2005':
        case '\u2006':
        case '\u2007':
        case '\u2008':
        case '\u2009':
        case '\u200A':
        case '\u200B':
        case '\u200C':
        //
        case '\u200E':
        case '\u200F':
        //
        case '\u2028':
        case '\u2029':
        case '\u202A':
        //
        case '\u202F':
        //
        case '\u205F':
        case '\u2060':
        //
        case '\u2063':
        //
        case '\u3000':
        //
        case '\u3164':
        //
        case '\uFEFF':
            return true;
        default:
            break;
        }

        return false;
    }

    #endregion
}
