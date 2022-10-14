//#define USE_BYTE_SPAN
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using static System.BitConverter;
using static System.Threading.Tasks.Task;
using static Nonno.Assets.Utils;
using BS = System.Span<byte>;
using MI = System.Runtime.CompilerServices.MethodImplAttribute;
using MIO = System.Runtime.CompilerServices.MethodImplOptions;

namespace Nonno.Assets.Scrolls;

/// <summary>
/// ある媒体に対して挿入搴取の可能な巻子を表します。
/// </summary>
/// <remarks>
/// このインターフェースを実装したクラスの<see cref="Point"/>に<see cref="ScrollPointer"/>共用体の使用方法を記述してください。
/// </remarks>
public interface IScroll : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// 現在の接続位置を定める指示子を取得または設定します。
    /// <para>
    /// 一度取得した指示子は一度使用することによって失効することに注意し、<see cref="ScrollPointer"/>の用法を正しく守ってください。
    /// </para>
    /// <para>
    /// 異常動作を避けるため、デバッガによる表示は避けてください。実装には<see cref="DebuggerBrowsableAttribute"/>にて<see cref="DebuggerBrowsableState.Never"/>を示してください。
    /// </para>
    /// </summary>
    ScrollPointer Point { get; set; }
    /// <summary>
    /// 指示子がこの巻子に対して有効であるかを確かめます。この操作は<see cref="ScrollPointer"/>の有効性に影響を与えません。
    /// </summary>
    /// <param name="pointer">
    /// 確かめる指示子。
    /// </param>
    /// <returns>
    /// 有効である場合は<see langword="true"/>、そうで無い場合は<see langword="false"/>。
    /// </returns>
    bool IsValid(ScrollPointer pointer);
    /// <summary>
    /// 指示子と現在位置の間が<c>T</c>型区間として解釈できる場合、その長さを求めます。
    /// <para>
    /// この操作によって指示子の有効性は変わりません。
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// 解釈する区間の型。
    /// </typeparam>
    /// <returns>
    /// 非負の場合は区間の長さ、負の場合は解釈に失敗したことを示す。特に零の場合は指示子が現在位置を指示することを示す。
    /// </returns>
    long FigureOutDistance<T>(ScrollPointer to);
    /// <summary>
    /// 巻子を複製します。
    /// <para>
    /// 取得している索引は複製元と複製先の巻子で夫一回づつ使用できますが、いくつかの実装において、索引が取得されている状態での複製はできません。
    /// </para>
    /// </summary>
    /// <returns>
    /// 複製した巻子。
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// 不正な操作が行われました。
    /// </exception>
    IScroll Copy();
    /// <summary>
    /// 巻子を複製します。
    /// <para>
    /// 取得している索引は複製元と複製先の巻子で夫一回づつ使用できますが、いくつかの実装において、索引が取得されている状態での複製はできません。
    /// </para>
    /// </summary>
    /// <returns>
    /// 複製した巻子を保証する用務。
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// 不正な操作が行われました。
    /// </exception>
    Task<IScroll> CopyAsync() => FromResult(Copy());
    /// <summary>
    /// 指示子を挿入します。
    /// </summary>
    /// <param name="pointer">
    /// 挿入する指示子。
    /// </param>
    /// <returns>
    /// 指示子を挿入したことを保証する用務。
    /// </returns>
    Task Insert(in ScrollPointer pointer);
    /// <summary>
    /// 指示子を挿入します。
    /// </summary>
    /// <param name="pointer">
    /// 挿入する指示子。
    /// </param>
    /// <returns>
    /// 指示子を挿入したことを保証する用務。
    /// </returns>
    Task Insert(in ScrollPointer pointer, CancellationToken cancellationToken = default) => cancellationToken.IsCancellationRequested ? FromCanceled(cancellationToken) : Insert(pointer: pointer);
    /// <summary>
    /// 指示子を搴取します。
    /// </summary>
    /// <param name="pointer">
    /// 搴取する指示子。
    /// </param>
    /// <returns>
    /// 指示子を搴取したことを保証する用務。
    /// </returns>
    Task Remove(out ScrollPointer pointer);
    /// <summary>
    /// 指示子を搴取します。
    /// </summary>
    /// <param name="pointer">
    /// 搴取する指示子。
    /// </param>
    /// <returns>
    /// 指示子を搴取したことを保証する用務。
    /// </returns>
    Task Remove(out ScrollPointer pointer, CancellationToken cancellationToken = default) => cancellationToken.IsCancellationRequested ? FromCanceled(cancellationToken, out pointer) : Remove(pointer: out pointer);

    /// <summary>
    /// 値を挿入します。
    /// </summary>
    /// <typeparam name="T">
    /// 値の型。
    /// </typeparam>
    /// <param name="value">
    /// 挿入する値。
    /// </param>
    /// <returns>
    /// 値を挿入したことを保証する用務。
    /// </returns>
    Task Insert<T>(in T value) where T : unmanaged
    {
        Span<T> span = stackalloc[] { value };
        InsertSync(span: span);
        return CompletedTask;
    }
    /// <summary>
    /// 値を挿入します。
    /// </summary>
    /// <typeparam name="T">
    /// 値の型。
    /// </typeparam>
    /// <param name="value">
    /// 挿入する値。
    /// </param>
    /// <returns>
    /// 値を挿入したことを保証する用務。
    /// </returns>
    Task Insert<T>(in T value, CancellationToken cancellationToken = default) where T : unmanaged => cancellationToken.IsCancellationRequested ? FromCanceled(cancellationToken) : Insert(value: value);
    /// <summary>
    /// メモリの内容を挿入します。
    /// <para>
    /// この務容が行う処理は、<see cref="InsertSync{T}(Span{T})"/>が行う処理と実質的に同じです。
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// メモリの内容の型。
    /// </typeparam>
    /// <param name="memory">
    /// 挿入する内容のメモリ。
    /// </param>
    /// <returns>
    /// メモリの内容を挿入したことを保証する用務。
    /// </returns>
    Task Insert<T>(Memory<T> memory) where T : unmanaged;
    /// <summary>
    /// メモリの内容を挿入します。
    /// <para>
    /// この務容が行う処理は、<see cref="InsertSync{T}(Span{T})"/>が行う処理と実質的に同じです。
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// メモリの内容の型。
    /// </typeparam>
    /// <param name="memory">
    /// 挿入する内容のメモリ。
    /// </param>
    /// <returns>
    /// メモリの内容を挿入したことを保証する用務。
    /// </returns>
    Task Insert<T>(Memory<T> memory, CancellationToken cancellationToken = default) where T : unmanaged => cancellationToken.IsCancellationRequested ? FromCanceled(cancellationToken) : Insert(memory: memory);
    /// <summary>
    /// 値を挿入します。
    /// </summary>
    /// <typeparam name="T">
    /// 値の型。
    /// </typeparam>
    /// <param name="value">
    /// 挿入する値。
    /// </param>
    void InsertSync<T>(in T value) where T : unmanaged
    {
        Span<T> span = stackalloc[] { value };
        InsertSync(span: span);
    }
    /// <summary>
    /// 区間の内容を挿入します。
    /// <para>
    /// この務容が行う処理は、<see cref="Insert{T}(Memory{T})"/>が行う処理と実質的に同じです。
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// 区間の内容の型。
    /// </typeparam>
    /// <param name="span">
    /// 挿入する内容の区間。
    /// </param>
    void InsertSync<T>(Span<T> span) where T : unmanaged;
    /// <summary>
    /// 値を搴取します。
    /// </summary>
    /// <typeparam name="T">
    /// 値の型。
    /// </typeparam>
    /// <param name="value">
    /// 搴取した値。
    /// </param>
    /// <returns>
    /// 値を搴取したことを保証する用務。
    /// </returns>
    Task Remove<T>(out T value) where T : unmanaged
    {
        Span<T> span = stackalloc T[1];
        RemoveSync(span: span);
        value = span[0];
        return CompletedTask;
    }
    /// <summary>
    /// 値を搴取します。
    /// </summary>
    /// <typeparam name="T">
    /// 値の型。
    /// </typeparam>
    /// <param name="value">
    /// 搴取した値。
    /// </param>
    /// <returns>
    /// 値を搴取したことを保証する用務。
    /// </returns>
    Task Remove<T>(out T value, CancellationToken cancellationToken = default) where T : unmanaged => cancellationToken.IsCancellationRequested ? FromCanceled(cancellationToken, out value) : Remove(out value);
    /// <summary>
    /// メモリの内容へ搴取します。
    /// <para>
    /// この務容が行う処理は、<see cref="RemoveSync{T}(Span{T})"/>が行う処理と実質的に同じです。
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// メモリの内容の型。
    /// </typeparam>
    /// <param name="memory">
    /// 搴取した内容のメモリ。
    /// </param>
    /// <returns>
    /// メモリの内容へ搴取したことを保証する用務。
    /// </returns>
    Task Remove<T>(Memory<T> memory) where T : unmanaged;
    /// <summary>
    /// メモリの内容へ搴取します。
    /// <para>
    /// この務容が行う処理は、<see cref="RemoveSync{T}(Span{T})"/>が行う処理と実質的に同じです。
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// メモリの内容の型。
    /// </typeparam>
    /// <param name="memory">
    /// 搴取した内容のメモリ。
    /// </param>
    /// <returns>
    /// メモリの内容へ搴取したことを保証する用務。
    /// </returns>
    Task Remove<T>(Memory<T> memory, CancellationToken cancellationToken = default) where T : unmanaged => cancellationToken.IsCancellationRequested ? FromCanceled(cancellationToken) : Remove(memory: memory);
    /// <summary>
    /// 値を搴取します。
    /// </summary>
    /// <typeparam name="T">
    /// 値の型。
    /// </typeparam>
    /// <param name="value">
    /// 搴取した値。
    /// </param>
    void RemoveSync<T>(out T value) where T : unmanaged
    {
        Span<T> span = stackalloc T[1];
        RemoveSync(span: span);
        value = span[0];
    }
    /// <summary>
    /// 区間の内容へ搴取します。
    /// <para>
    /// この務容が行う処理は、<see cref="Remove{T}(Memory{T})"/>が行う処理と実質的に同じです。
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// 区間の内容の型。
    /// </typeparam>
    /// <param name="span">
    /// 搴取した内容の区間。
    /// </param>
    void RemoveSync<T>(Span<T> span) where T : unmanaged;
}

/// <summary>
/// 複数の箇所を同時に操作できる巻子を表します。
/// </summary>
public interface IParallelScroll : IScroll
{
    Task<IScroll> Parallelize();
}

/// <summary>
/// 巻子の中の位置を示す指示子を表します。
/// <para>
/// 指示子は<see cref="IScroll.Point"/>によって正しい値が得られ、巻子同士で相互に適用することはできません。得た値は必ず得た巻子に使用してください。
/// </para>
/// <para>
/// 指示子は<see cref="IScroll.Point"/>に設定された時点で無効となります。再び必要となる場合は同時に<see cref="IScroll.Point"/>から新しい指示子を取得してください。
/// </para>
/// </summary>
public unsafe readonly struct ScrollPointer : IEquatable<ScrollPointer>
{
    readonly nint _num;
    readonly object? _obj;

    public int Number => (int)_num;
    /// <summary>
    /// 指示子の番号を取得します。
    /// <para>
    /// 指示子の番号の扱われ方は巻子の実装によってさまざまであり、この値の一致は指示子の一致を示しません。
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
    /// 指示子の拡張情報を取得します。
    /// <para>
    /// 指示子の拡張情報の扱われ方は巻子の実装によってさまざまであり、この値の一致または有無は指示子の内容を一般に表しません。
    /// </para>
    /// </summary>
    public object? Information => _obj;
    /// <summary>
    /// 指示子の四文字の文字列を取得します。
    /// <para>
    /// 指示子の拡張情報の扱われ方は巻子の実装によってさまざまであり、この値の一致または有無は指示子の内容を一般に表しません。
    /// </para>
    /// </summary>
    public ASCIIString ASCIIString => new(GetBytes((uint)_num));
    /// <summary>
    /// 指示子の四バイトのバイト列を取得します。
    /// <para>
    /// 指示子の拡張情報の扱われ方は巻子の実装によってさまざまであり、この値の一致または有無は指示子の内容を一般に表しません。
    /// </para>
    /// </summary>
    public byte[] Bytes => GetBytes((uint)_num);

    /// <summary>
    /// 指示子を規定値で初期化します。
    /// </summary>
    public ScrollPointer()
    {
        _num = default;
        _obj = default;
    }
    /// <summary>
    /// 指示子を番号のみを指定して初期化します。拡張情報は規定値で初期化されます。
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
    /// 指示子を拡張情報のみを指定して初期化します。番号は規定値で初期化されます。
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
    /// 指示子を
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

[Obsolete("IAccessorへの転換を推奨しています。")]
[SuppressMessage("Style", "IDE0058:式の値が使用されていません", Justification = "IAccessorへの転換まで保留。")]
public interface IBuiltinTypeAccessor
{
    Task Read(out bool boolean)
    {
        Read(out byte @byte);
        boolean = @byte != 0;
        return CompletedTask;
    }
    Task Read(out byte @byte);
    Task Read(out sbyte sByte)
    {
        Read(out byte v);
        sByte = unchecked((sbyte)v);
        return CompletedTask;
    }
    Task Read(out char @char)
    {
        Read(out ushort v);
        @char = unchecked((char)v);
        return CompletedTask;
    }
    Task Read(out short int16)
    {
        Read(out ushort v);
        int16 = unchecked((short)v);
        return CompletedTask;
    }
    Task Read(out int int32)
    {
        Read(out uint v);
        int32 = unchecked((int)v);
        return CompletedTask;
    }
    Task Read(out long int64)
    {
        Read(out ulong v);
        int64 = unchecked((long)v);
        return CompletedTask;
    }
    Task Read(out ushort uInt16)
    {
        BS span = stackalloc byte[sizeof(ushort)];
        for (int i = 0; i < span.Length; i++)
        {
            _ = Read(out byte @byte);
            span[i] = @byte;
        }
        uInt16 = ToUInt16(span);
        return CompletedTask;
    }
    Task Read(out uint uInt32)
    {
        BS span = stackalloc byte[sizeof(uint)];
        for (int i = 0; i < span.Length; i++)
        {
            _ = Read(out byte @byte);
            span[i] = @byte;
        }
        uInt32 = ToUInt32(span);
        return CompletedTask;
    }
    Task Read(out ulong uInt64)
    {
        BS span = stackalloc byte[sizeof(ulong)];
        for (int i = 0; i < span.Length; i++)
        {
            _ = Read(out byte @byte);
            span[i] = @byte;
        }
        uInt64 = ToUInt64(span);
        return CompletedTask;
    }
    Task Read(out float single)
    {
        BS span = stackalloc byte[sizeof(float)];
        for (int i = 0; i < span.Length; i++)
        {
            _ = Read(out byte @byte);
            span[i] = @byte;
        }
        single = ToSingle(span);
        return CompletedTask;
    }
    Task Read(out double @double)
    {
        BS span = stackalloc byte[sizeof(double)];
        for (int i = 0; i < span.Length; i++)
        {
            _ = Read(out byte @byte);
            span[i] = @byte;
        }
        @double = ToDouble(span);
        return CompletedTask;
    }
    Task Read(out decimal @decimal)
    {
        Read(out string? @string).Wait();
        @decimal = @string == null ? default : Decimal.Parse(@string);
        return CompletedTask;
    }
    Task Read(out string? @string)
    {
        Read(out int length).Wait();
        if (length < 0)
        {
            @string = null;
            return CompletedTask;
        }
        var chars = new char[length];
        for (int i = 0; i < chars.Length; i++)
        {
            _ = Read(out char @char);
            chars[i] = @char;
        }
        @string = new(chars);
        return CompletedTask;
    }
    Task Read(out byte[]? bytes)
    {
        Read(out int length).Wait();
        if (length < 0)
        {
            bytes = null;
            return CompletedTask;
        }
        bytes = new byte[length];
        for (int i = 0; i < bytes.Length; i++)
        {
            _ = Read(out byte @byte);
            bytes[i] = @byte;
        }
        return CompletedTask;
    }
    Task Write(bool boolean) => Write(boolean ? 1 : 0);
    Task Write(byte @byte);
    Task Write(sbyte sByte) => Write(unchecked((byte)sByte));
    Task Write(char @char) => Write(unchecked((ushort)@char));
    Task Write(short int16) => Write(unchecked((ushort)int16));
    Task Write(int int32) => Write(unchecked((uint)int32));
    Task Write(long int64) => Write(unchecked((ulong)int64));
    Task Write(ushort uInt16)
    {
        BS span = stackalloc byte[sizeof(ushort)];
        _ = TryWriteBytes(span, uInt16);
        for (int i = 0; i < span.Length; i++)
        {
            Write(span[i]).Wait();
        }
        return CompletedTask;
    }
    Task Write(uint uInt32)
    {
        BS span = stackalloc byte[sizeof(uint)];
        _ = TryWriteBytes(span, uInt32);
        for (int i = 0; i < span.Length; i++)
        {
            Write(span[i]).Wait();
        }
        return CompletedTask;
    }
    Task Write(ulong uInt64)
    {
        BS span = stackalloc byte[sizeof(ulong)];
        _ = TryWriteBytes(span, uInt64);
        for (int i = 0; i < span.Length; i++)
        {
            Write(span[i]).Wait();
        }
        return CompletedTask;
    }
    Task Write(float single)
    {
        BS span = stackalloc byte[sizeof(float)];
        _ = TryWriteBytes(span, single);
        for (int i = 0; i < span.Length; i++)
        {
            Write(span[i]).Wait();
        }
        return CompletedTask;
    }
    Task Write(double @double)
    {
        BS span = stackalloc byte[sizeof(double)];
        _ = TryWriteBytes(span, @double);
        for (int i = 0; i < span.Length; i++)
        {
            Write(span[i]).Wait();
        }
        return CompletedTask;
    }
    Task Write(decimal @decimal) => Write(@decimal.ToString());
    async Task Write(string? @string)
    {
        if (@string == null)
        {
            await Write(-1);
        }
        else
        {
            await Write(@string.Length);
            for (int i = 0; i < @string.Length; i++) await Write(@char: @string[i]);
        }
    }
    async Task Write(byte[]? bytes)
    {
        if (bytes == null)
        {
            await Write(-1);
        }
        else
        {
            await Write(bytes.Length);
            for (int i = 0; i < bytes.Length; i++) await Write(@byte: bytes[i]);
        }
    }
}

#pragma warning disable IDE0001 // 意図せずInsert(value:)の参照先が変わらぬようつける。
public static class FundamentalScrollUtils
{
    static readonly Assembly ASSEMBLY = Assembly.GetExecutingAssembly();
    static readonly AssemblyName ASSEMBLY_NAME = ASSEMBLY.GetName();

    static FundamentalScrollUtils()
    {
        InitSectionForIRMethods();
    }

    #region BuiltIns

    static readonly Dictionary<Type, IRMethodsDelegate> _delegates = new();

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, bool boolean) => @this.Insert<bool>(value: boolean);
    [MI(MIO.AggressiveInlining)]
    public static void InsertSync(this IScroll @this, bool boolean) => @this.InsertSync<bool>(value: boolean);
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out bool boolean) => @this.Remove<bool>(value: out boolean);
    [MI(MIO.AggressiveInlining)]
    public static void RemoveSync(this IScroll @this, out bool boolean) => @this.RemoveSync<bool>(value: out boolean);

#if USE_BYTE_SPAN
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this INote @this, char @char)
    {
        BS span = stackalloc byte[sizeof(char)];
        _ = TryWriteBytes(span, @char);
        @this.InsertSync(span: span);
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this INote @this, out char @char)
    {
        BS span = stackalloc byte[sizeof(char)];
        @this.RemoveSync(span: span);
        @char = ToChar(span);
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this INote @this, byte @byte)
    {
        BS span = stackalloc byte[sizeof(byte)];
        span[0] = @byte;
        @this.InsertSync(span: span);
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this INote @this, out byte @byte)
    {
        BS span = stackalloc byte[sizeof(byte)];
        @this.RemoveSync(span: span);
        @byte = span[0];
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this INote @this, sbyte sByte)
    {
        BS span = stackalloc byte[sizeof(sbyte)];
        span[0] = unchecked((byte)sByte);
        @this.InsertSync(span: span);
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this INote @this, out sbyte sByte)
    {
        BS span = stackalloc byte[sizeof(sbyte)];
        @this.RemoveSync(span: span);
        sByte = unchecked((sbyte)span[0]);
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this INote @this, short int16)
    {
        BS span = stackalloc byte[sizeof(short)];
        _ = TryWriteBytes(span, int16);
        @this.InsertSync(span: span);
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this INote @this, out short int16)
    {
        BS span = stackalloc byte[sizeof(short)];
        @this.RemoveSync(span: span);
        int16 = ToInt16(span);
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this INote @this, ushort uInt16)
    {
        BS span = stackalloc byte[sizeof(ushort)];
        _ = TryWriteBytes(span, uInt16);
        @this.InsertSync(span: span);
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this INote @this, out ushort uInt16)
    {
        BS span = stackalloc byte[sizeof(ushort)];
        @this.RemoveSync(span: span);
        uInt16 = ToUInt16(span);
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this INote @this, int int32)
    {
        BS span = stackalloc byte[sizeof(int)];
        _ = TryWriteBytes(span, int32);
        @this.InsertSync(span: span);
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this INote @this, out int int32)
    {
        BS span = stackalloc byte[sizeof(int)];
        @this.RemoveSync(span: span);
        int32 = ToInt32(span);
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this INote @this, uint uInt32)
    {
        BS span = stackalloc byte[sizeof(uint)];
        _ = TryWriteBytes(span, uInt32);
        @this.InsertSync(span: span);
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this INote @this, out uint uInt32)
    {
        BS span = stackalloc byte[sizeof(uint)];
        @this.RemoveSync(span: span);
        uInt32 = ToUInt32(span);
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this INote @this, long int64)
    {
        BS span = stackalloc byte[sizeof(long)];
        _ = TryWriteBytes(span, int64);
        @this.InsertSync(span: span);
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this INote @this, out long int64)
    {
        BS span = stackalloc byte[sizeof(long)];
        @this.RemoveSync(span: span);
        int64 = ToInt64(span);
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this INote @this, ulong uInt64)
    {
        BS span = stackalloc byte[sizeof(ulong)];
        _ = TryWriteBytes(span, uInt64);
        @this.InsertSync(span: span);
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this INote @this, out ulong uInt64)
    {
        BS span = stackalloc byte[sizeof(ulong)];
        @this.RemoveSync(span: span);
        uInt64 = ToUInt64(span);
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this INote @this, float single)
    {
        BS span = stackalloc byte[sizeof(float)];
        _ = TryWriteBytes(span, single);
        @this.InsertSync(span: span);
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this INote @this, out float single)
    {
        BS span = stackalloc byte[sizeof(float)];
        @this.RemoveSync(span: span);
        single = ToSingle(span);
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this INote @this, double @double)
    {
        BS span = stackalloc byte[sizeof(double)];
        _ = TryWriteBytes(span, @double);
        @this.InsertSync(span: span);
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this INote @this, out double @double)
    {
        BS span = stackalloc byte[sizeof(double)];
        @this.RemoveSync(span: span);
        @double = ToDouble(span);
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this INote @this, decimal @decimal)
    {
        Span<int> span = stackalloc int[4];
        if (!Decimal.TryGetBits(@decimal, span, out _)) throw new Exception("不明なエラーです。");
        @this.InsertSync(span: span);
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this INote @this, in decimal @decimal)
    {
        Span<int> span = stackalloc int[4];
        if (!Decimal.TryGetBits(@decimal, span, out _)) throw new Exception("不明なエラーです。");
        @this.InsertSync(span: span);
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this INote @this, out decimal @decimal)
    {
        Span<int> span = stackalloc int[4];
        @this.RemoveSync(span:span);
        @decimal = new(span);
        return CompletedTask;
    }
#else
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, char @char) => @this.Insert(value: @char);
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out char @char) => @this.Remove(value: out @char);

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, byte @byte) => @this.Insert(value: @byte);
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out byte @byte) => @this.Remove(value: out @byte);

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, sbyte sByte) => @this.Insert(value: sByte);
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out sbyte sByte) => @this.Remove(value: out sByte);

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, short int16) => @this.Insert(value: int16);
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out short int16) => @this.Remove(value: out int16);

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, ushort uInt16) => @this.Insert(value: uInt16);
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out ushort uInt16) => @this.Remove(value: out uInt16);

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, int int32) => @this.Insert(value: int32);
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out int int32) => @this.Remove(value: out int32);

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, uint uInt32) => @this.Insert(value: uInt32);
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out uint uInt32) => @this.Remove(value: out uInt32);

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, long int64) => @this.Insert(value: int64);
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out long int64) => @this.Remove(value: out int64);

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, ulong uInt64) => @this.Insert(value: uInt64);
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out ulong uInt64) => @this.Remove(value: out uInt64);

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, float single) => @this.Insert(value: single);
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out float single) => @this.Remove(value: out single);

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, double @double) => @this.Insert(value: @double);
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out double @double) => @this.Remove(value: out @double);

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, decimal @decimal) => @this.Insert(value: @decimal);
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out decimal @decimal) => @this.Remove(value: out @decimal);
#endif

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, string? @string)
    {
        if (@string == null) { @this.Insert(int32: -1).Wait(); return CompletedTask; }
        @this.Insert(int32: @string.Length).Wait(); // lengthは文字長(バイト長ではない)であることに注意。
        @this.InsertSync(span: @string.AsByteSpan());
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out string? @string)
    {
        @this.Remove(out int length).Wait(); // lengthは文字長(バイト長ではない)であることに注意。
        if (length < 0) { @string = null; return CompletedTask; }
        @string = new(default, length);
        @this.RemoveSync(span: @string.AsByteSpan());
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void InsertSync(this IScroll @this, ReadOnlySpan<byte> u8string)
    {
        BS span = u8string.Length > ConstantValues.STACKALLOC_MAX_LENGTH ? new byte[u8string.Length] : stackalloc byte[u8string.Length];
        @this.Insert(int32: u8string.Length).Wait();
        @this.InsertSync(span: span);
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static void RemoveSync(this IScroll @this, out ReadOnlySpan<byte> u8string)
    {
        @this.Remove(int32: out var length);
        var r = new byte[length];
        @this.RemoveSync<byte>(span: r);
        u8string = r;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, in object? @object)
    {
        if (@object is null)
        {
            return @this.Insert(type: null);
        }
        else
        {
            var type = @object.GetType();
            if (type == typeof(object)) return @this.Insert(type);

            @this.Insert(type).Wait();

            return Insert(to: @this, @object: @object, @as: type);
        }
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out object? @object)
    {
        @this.Remove(out Type? type).Wait();
        if (type is null)
        {
            @object = null;
            return CompletedTask;
        }
        else if (type == typeof(object))
        {
            @object = new();
            return CompletedTask;
        }
        else
        {
            return Remove(to: @this, @object: out @object, @as: type);
        }
    }

    /// <summary>
    /// 型が判っている実体を巻子に挿入します。
    /// <para>
    /// <see cref="Insert(IScroll, in object?)"/>とは異なる、深層の務容です。
    /// この務容によって挿入された値は<see cref="Remove(IScroll, out object?, Type)"/>によって搴取してください。
    /// </para>
    /// </summary>
    /// <param name="to"></param>
    /// <param name="object"></param>
    /// <param name="as"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [MI(MIO.AggressiveInlining)]
    public static Task Insert(IScroll to, in object @object, Type @as)
    {
        if (@object.GetType() != @as) throw new ArgumentException("実体は指定された型のものではありませんでした。");

        if (!_delegates.TryGetValue(@as, out var @delegate))
        {
            @delegate = Activator.CreateInstance(typeof(IRMethodsDelegate<>).MakeGenericType(new Type[] { @as })) as IRMethodsDelegate ?? throw new Exception("不明な錯誤です。実体を作成できませんでした。");
            _delegates.Add(@as, @delegate);
        }
        return @delegate.Insert(to, @object);
    }
    /// <summary>
    /// 型が判っている実体を巻子から搴取します。
    /// <para>
    /// <see cref="Remove(IScroll, out object?)"/>とは異なる、深層の務容です。
    /// この務容は<see cref="Insert(IScroll, in object, Type)"/>によって挿入された値に対してのみ行ってください。
    /// </para>
    /// </summary>
    /// <param name="to"></param>
    /// <param name="object"></param>
    /// <param name="as"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [MI(MIO.AggressiveInlining)]
    public static Task Remove(IScroll to, out object @object, Type @as)
    {
        if (!_delegates.TryGetValue(@as, out var @delegate))
        {
            @delegate = Activator.CreateInstance(typeof(IRMethodsDelegate<>).MakeGenericType(new Type[] { @as })) as IRMethodsDelegate ?? throw new Exception("不明な錯誤です。実体を作成できませんでした。");
            _delegates.Add(@as, @delegate);
        }
        return @delegate.Remove(to, out @object);
    }

    abstract class IRMethodsDelegate
    {
        protected static readonly MethodInfo GENERIC_INSERT_METHOD_INFO = typeof(ScrollExtensions).GetMethods().Where(x => x.GetMarks().Contains("generic_insert_method")).Single();
        protected static readonly MethodInfo GENERIC_REMOVE_METHOD_INFO = typeof(ScrollExtensions).GetMethods().Where(x => x.GetMarks().Contains("generic_remove_method")).Single();

        public abstract Task Insert(IScroll note, in object value);
        public abstract Task Remove(IScroll note, out object value);
    }
    class IRMethodsDelegate<T> : IRMethodsDelegate where T : notnull
    {
        readonly InsertDelegate _insertDelegate;
        readonly RemoveDelegate _removeDelegate;

        public IRMethodsDelegate()
        {
            _insertDelegate = GENERIC_INSERT_METHOD_INFO.MakeGenericMethod(new Type[] { typeof(T) }).CreateDelegate<InsertDelegate>();
            _removeDelegate = GENERIC_REMOVE_METHOD_INFO.MakeGenericMethod(new Type[] { typeof(T) }).CreateDelegate<RemoveDelegate>();
        }

        public override Task Insert(IScroll note, in object value)
        {
            return _insertDelegate(note, (T)value);
        }
        public override Task Remove(IScroll note, out object value)
        {
            var r = _removeDelegate(note, out T t);
            value = t;
            return r;
        }

        delegate Task InsertDelegate(IScroll note, in T value);
        delegate Task RemoveDelegate(IScroll note, out T value);
    }

    #endregion
    #region Nullables

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, bool? booleanOrNull)
    {
        @this.Insert(booleanOrNull.HasValue).Wait();
        if (booleanOrNull.HasValue)
        {
            @this.Insert(boolean: booleanOrNull.Value).Wait();
        }
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out bool? booleanOrNull)
    {
        @this.Remove(out bool hasValue).Wait();
        if (hasValue)
        {
            @this.Remove(out bool value_).Wait();
            booleanOrNull = value_;
        }
        else
        {
            booleanOrNull = null;
        }
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, byte? byteOrNull)
    {
        @this.Insert(byteOrNull.HasValue).Wait();
        if (byteOrNull.HasValue)
        {
            @this.Insert(@byte: byteOrNull.Value).Wait();
        }
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out byte? byteOrNull)
    {
        @this.Remove(out bool hasValue).Wait();
        if (hasValue)
        {
            @this.Remove(out byte value_).Wait();
            byteOrNull = value_;
        }
        else
        {
            byteOrNull = null;
        }
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, sbyte? sByteOrNull)
    {
        @this.Insert(sByteOrNull.HasValue).Wait();
        if (sByteOrNull.HasValue)
        {
            @this.Insert(sByte: sByteOrNull.Value).Wait();
        }
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out sbyte? sByteOrNull)
    {
        @this.Remove(out bool hasValue).Wait();
        if (hasValue)
        {
            @this.Remove(out sbyte value_).Wait();
            sByteOrNull = value_;
        }
        else
        {
            sByteOrNull = null;
        }
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, char? charOrNull)
    {
        @this.Insert(charOrNull.HasValue).Wait();
        if (charOrNull.HasValue)
        {
            @this.Insert(@char: charOrNull.Value).Wait();
        }
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out char? charOrNull)
    {
        @this.Remove(out bool hasValue).Wait();
        if (hasValue)
        {
            @this.Remove(out char value_).Wait();
            charOrNull = value_;
        }
        else
        {
            charOrNull = null;
        }
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, short? int16OrNull)
    {
        @this.Insert(int16OrNull.HasValue).Wait();
        if (int16OrNull.HasValue)
        {
            @this.Insert(@int16: int16OrNull.Value).Wait();
        }
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out short? int16OrNull)
    {
        @this.Remove(out bool hasValue).Wait();
        if (hasValue)
        {
            @this.Remove(out short value_).Wait();
            int16OrNull = value_;
        }
        else
        {
            int16OrNull = null;
        }
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, int? int32OrNull)
    {
        @this.Insert(int32OrNull.HasValue).Wait();
        if (int32OrNull.HasValue)
        {
            @this.Insert(@int32: int32OrNull.Value).Wait();
        }
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out int? int32OrNull)
    {
        @this.Remove(out bool hasValue).Wait();
        if (hasValue)
        {
            @this.Remove(out int value_).Wait();
            int32OrNull = value_;
        }
        else
        {
            int32OrNull = null;
        }
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, long? int64OrNull)
    {
        @this.Insert(int64OrNull.HasValue).Wait();
        if (int64OrNull.HasValue)
        {
            @this.Insert(@int64: int64OrNull.Value).Wait();
        }
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out long? int64OrNull)
    {
        @this.Remove(out bool hasValue).Wait();
        if (hasValue)
        {
            @this.Remove(out long value_).Wait();
            int64OrNull = value_;
        }
        else
        {
            int64OrNull = null;
        }
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, ushort? uInt16OrNull)
    {
        @this.Insert(uInt16OrNull.HasValue).Wait();
        if (uInt16OrNull.HasValue)
        {
            @this.Insert(@uInt16: uInt16OrNull.Value).Wait();
        }
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out ushort? uInt16OrNull)
    {
        @this.Remove(out bool hasValue).Wait();
        if (hasValue)
        {
            @this.Remove(out ushort value_).Wait();
            uInt16OrNull = value_;
        }
        else
        {
            uInt16OrNull = null;
        }
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, uint? uInt32OrNull)
    {
        @this.Insert(uInt32OrNull.HasValue).Wait();
        if (uInt32OrNull.HasValue)
        {
            @this.Insert(@uInt32: uInt32OrNull.Value).Wait();
        }
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out uint? uInt32OrNull)
    {
        @this.Remove(out bool hasValue).Wait();
        if (hasValue)
        {
            @this.Remove(out uint value_).Wait();
            uInt32OrNull = value_;
        }
        else
        {
            uInt32OrNull = null;
        }
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, ulong? uInt64OrNull)
    {
        @this.Insert(uInt64OrNull.HasValue).Wait();
        if (uInt64OrNull.HasValue)
        {
            @this.Insert(@uInt64: uInt64OrNull.Value).Wait();
        }
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out ulong? uInt64OrNull)
    {
        @this.Remove(out bool hasValue).Wait();
        if (hasValue)
        {
            @this.Remove(out ulong value_).Wait();
            uInt64OrNull = value_;
        }
        else
        {
            uInt64OrNull = null;
        }
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, float? singleOrNull)
    {
        @this.Insert(singleOrNull.HasValue).Wait();
        if (singleOrNull.HasValue)
        {
            @this.Insert(@single: singleOrNull.Value).Wait();
        }
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out float? singleOrNull)
    {
        @this.Remove(out bool hasValue).Wait();
        if (hasValue)
        {
            @this.Remove(out float value_).Wait();
            singleOrNull = value_;
        }
        else
        {
            singleOrNull = null;
        }
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, double? doubleOrNull)
    {
        @this.Insert(doubleOrNull.HasValue).Wait();
        if (doubleOrNull.HasValue)
        {
            @this.Insert(@double: doubleOrNull.Value).Wait();
        }
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out double? doubleOrNull)
    {
        @this.Remove(out bool hasValue).Wait();
        if (hasValue)
        {
            @this.Remove(out double value_).Wait();
            doubleOrNull = value_;
        }
        else
        {
            doubleOrNull = null;
        }
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, decimal? decimalOrNull)
    {
        @this.Insert(decimalOrNull.HasValue).Wait();
        if (decimalOrNull.HasValue)
        {
            @this.Insert(@decimal: decimalOrNull.Value).Wait();
        }
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, in decimal? decimalOrNull)
    {
        @this.Insert(decimalOrNull.HasValue).Wait();
        if (decimalOrNull.HasValue)
        {
            @this.Insert(@decimal: decimalOrNull.Value).Wait();
        }
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out decimal? decimalOrNull)
    {
        @this.Remove(out bool hasValue).Wait();
        if (hasValue)
        {
            @this.Remove(out decimal value_).Wait();
            decimalOrNull = value_;
        }
        else
        {
            decimalOrNull = null;
        }
        return CompletedTask;
    }

    #endregion
    #region Array

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static async Task Insert<T>(this IScroll @this, T[]? array)
    {
        if (array == null)
        {
            await @this.Insert(int32: -1);
        }
        else
        {
            await @this.Insert(int32: array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                await @this.Insert(instance: array[i]);
            }
        }
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static async Task Insert<T>(this IScroll @this, T[]? array, CancellationToken cancellationToken = default)
    {
        if (array == null)
        {
            await @this.Insert(int32: -1);
        }
        else
        {
            await @this.Insert(int32: array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                await @this.Insert(instance: array[i]);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static async Task Remove<T>(this IScroll @this, T[]? array)
    {
        await @this.Remove(out int length);
        if (length < 0)
        {
            if (array is not null) throw new ArgumentException("渡された配列が適しません。搴取された値が`null`であったのに対し、渡された値は`null`ではありませんでした。", nameof(array));
        }
        else
        {
            if (array is null) throw new ArgumentNullException("渡された配列が適しません。搴取された値が`null`でではなかったのに対し、渡された値は`null`でした。", nameof(array));
            if (array.Length != length) throw new ArgumentException($"渡された配列が適しません。搴取された値の長さが`{length}`であったのに対し、渡された値の長さは`{array.Length}`でした。", nameof(array));

            for (int i = 0; i < array.Length; i++)
            {
                await @this.Remove(out T item);
                array[i] = item;
            }
        }
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static async Task Remove<T>(this IScroll @this, T[]? array, CancellationToken cancellationToken = default)
    {
        await @this.Remove(out int length);
        if (length < 0)
        {
            if (array is not null) throw new ArgumentException("渡された配列が適しません。搴取された値が`null`であったのに対し、渡された値は`null`ではありませんでした。", nameof(array));
        }
        else
        {
            if (array is null) throw new ArgumentNullException("渡された配列が適しません。搴取された値が`null`でではなかったのに対し、渡された値は`null`でした。", nameof(array));
            if (array.Length != length) throw new ArgumentException($"渡された配列が適しません。搴取された値の長さが`{length}`であったのに対し、渡された値の長さは`{array.Length}`でした。", nameof(array));

            for (int i = 0; i < array.Length; i++)
            {
                await @this.Remove(out T item);
                array[i] = item;
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove<T>(this IScroll @this, out T[]? array)
    {
        @this.Remove(out int length).Wait();
        if (length < 0)
        {
            array = null;
            return CompletedTask;
        }
        else
        {
            array = new T[length];
            return GetTask(array);

            async Task GetTask(T[] array)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    await @this.Remove(out T item);
                    array[i] = item;
                }
            }
        }
    }

    #endregion
    #region Type

    [IRMethod]
    public static Task Insert(this IScroll @this, Type? type)
    {
        return @this.Insert(type?.FullName);
    }
    [IRMethod]
    public static Task Remove(this IScroll @this, out Type? type)
    {
        @this.Remove(out string? name).Wait();
        type = name == null ? null : Type.GetType(name, false);
        return CompletedTask;
    }

    #endregion
    #region Others

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, DateTime dateTime)
    {
        return @this.Insert(int64: dateTime.Ticks);
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out DateTime dateTime)
    {
        var r = @this.Remove(out long ticks);
        dateTime = new(ticks);
        return r;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, TimeSpan timeSpan)
    {
        return @this.Insert(int64: timeSpan.Ticks);
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out TimeSpan timeSpan)
    {
        var r = @this.Remove(out long ticks);
        timeSpan = new(ticks);
        return r;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, Shift shift)
    {
        return @this.Insert(int32: shift.exponent);
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out Shift shift)
    {
        var r = @this.Remove(out int exponent);
        shift = new(exponent);
        return r;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, Guid guid)
    {
        BS span = stackalloc byte[16];
        _ = guid.TryWriteBytes(span);

        Tasks tasks = new();

        @this.Insert(uInt32: ToUInt32(span/*[0..]*/));
        @this.Insert(uInt16: ToUInt16(span[4..]));
        @this.Insert(uInt16: ToUInt16(span[6..]));
        @this.Insert(@byte: span[8]);
        @this.Insert(@byte: span[9]);
        @this.Insert(@byte: span[10]);
        @this.Insert(@byte: span[11]);
        @this.Insert(@byte: span[12]);
        @this.Insert(@byte: span[13]);
        @this.Insert(@byte: span[14]);
        @this.Insert(@byte: span[15]);

        return tasks.WhenAll();
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, in Guid guid)
    {
        BS span = stackalloc byte[16];
        _ = guid.TryWriteBytes(span);

        Tasks tasks = new();

        tasks += @this.Insert(uInt32: ToUInt32(span/*[0..]*/));
        tasks += @this.Insert(uInt16: ToUInt16(span[4..]));
        tasks += @this.Insert(uInt16: ToUInt16(span[6..]));
        tasks += @this.Insert(@byte: span[8]);
        tasks += @this.Insert(@byte: span[9]);
        tasks += @this.Insert(@byte: span[10]);
        tasks += @this.Insert(@byte: span[11]);
        tasks += @this.Insert(@byte: span[12]);
        tasks += @this.Insert(@byte: span[13]);
        tasks += @this.Insert(@byte: span[14]);
        tasks += @this.Insert(@byte: span[15]);

        return tasks.WhenAll();
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out Guid guid)
    {
        Tasks tasks = new();

        tasks += @this.Remove(out uint a);
        tasks += @this.Remove(out ushort b);
        tasks += @this.Remove(out ushort c);
        tasks += @this.Remove(out byte d);
        tasks += @this.Remove(out byte e);
        tasks += @this.Remove(out byte f);
        tasks += @this.Remove(out byte g);
        tasks += @this.Remove(out byte h);
        tasks += @this.Remove(out byte i);
        tasks += @this.Remove(out byte j);
        tasks += @this.Remove(out byte k);

        guid = new(a, b, c, d, e, f, g, h, i, j, k);

        return tasks.WhenAll();
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, BigInteger integer)
    {
        int length = integer.GetByteCount();
        BS span = stackalloc byte[length];
        if (!integer.TryWriteBytes(span, out int bytesWritten) || length != bytesWritten) throw new Exception("不明なエラーです。");
        @this.InsertSync(span: span);
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert(this IScroll @this, in BigInteger integer)
    {
        int length = integer.GetByteCount();
        BS span = stackalloc byte[length];
        if (!integer.TryWriteBytes(span, out int bytesWritten) || length != bytesWritten) throw new Exception("不明なエラーです。");
        @this.InsertSync(span: span);
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove(this IScroll @this, out BigInteger integer)
    {
        @this.Remove(out int length);
        BS span = stackalloc byte[length];
        @this.RemoveSync(span: span);
        integer = new BigInteger(span);
        return CompletedTask;
    }

    [MI(MIO.AggressiveInlining)]
    public static unsafe void InsertSync(this IScroll @this, RefString refString)
    {
        if (refString.IsNull) { @this.Insert(int32: -1).Wait(); return; }
        @this.Insert(int32: refString.Length).Wait(); // lengthは文字長である(バイト長ではない)ことに注意。
        fixed (char* p = refString.AsSpan()) @this.InsertSync(span: new BS(p, refString.Length << 1));
    }
    [MI(MIO.AggressiveInlining)]
    public static unsafe void InsertSync(this IScroll @this, in RefString refString)
    {
        if (refString.IsNull) { @this.Insert(int32: -1).Wait(); return; }
        @this.Insert(int32: refString.Length).Wait(); // lengthは文字長である(バイト長ではない)ことに注意。
        fixed (char* p = refString.AsSpan()) @this.InsertSync(span: new BS(p, refString.Length << 1));
    }
    [MI(MIO.AggressiveInlining)]
    public static void RemoveSync(this IScroll @this, out RefString refString)
    {
        @this.Remove(out int length).Wait(); // lengthは文字長である(バイト長ではない)ことに注意。
        if (length < 0) { refString = null; return; }
        string @string = new(default, length);
        @this.RemoveSync(span: @string.AsByteSpan());
        refString = @string;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert<T>(this IScroll @this, in ShortIdentifier<T> shortIdentifier)
    {
        BS span = stackalloc byte[ShortIdentifier<T>.SIZE];
        ShortIdentifier<T>.Write(to: span, shortIdentifier);
        @this.InsertSync(span: span);
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove<T>(this IScroll @this, out ShortIdentifier<T> shortIdentifier)
    {
        BS span = stackalloc byte[ShortIdentifier<T>.SIZE];
        @this.RemoveSync(span: span);
        shortIdentifier = ShortIdentifier<T>.Read(from: span);
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert<T>(this IScroll @this, in LongIdentifier<T> longIdentifier)
    {
        BS span = stackalloc byte[LongIdentifier<T>.SIZE];
        LongIdentifier<T>.Write(to: span, longIdentifier);
        @this.InsertSync(span: span);
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove<T>(this IScroll @this, out LongIdentifier<T> longIdentifier)
    {
        BS span = stackalloc byte[LongIdentifier<T>.SIZE];
        @this.RemoveSync(span: span);
        longIdentifier = LongIdentifier<T>.Read(from: span);
        return CompletedTask;
    }

    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Insert<T>(this IScroll @this, in UniqueIdentifier<T> uniqueIdentifier)
    {
        BS span = stackalloc byte[UniqueIdentifier<T>.SIZE];
        UniqueIdentifier<T>.Write(to: span, uniqueIdentifier);
        @this.InsertSync(span: span);
        return CompletedTask;
    }
    [IRMethod, MI(MIO.AggressiveInlining)]
    public static Task Remove<T>(this IScroll @this, out UniqueIdentifier<T> uniqueIdentifier)
    {
        BS span = stackalloc byte[UniqueIdentifier<T>.SIZE];
        @this.RemoveSync(span: span);
        uniqueIdentifier = UniqueIdentifier<T>.Read(from: span);
        return CompletedTask;
    }

    #endregion
    #region Generic

    /// <summary>
    /// 未知の型を巻子に挿入します。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="instance"></param>
    /// <returns>挿入を保証する用務。</returns>
    [MI(MIO.AggressiveInlining)]
    [Mark("generic_insert_method")]
    public static Task Insert<T>(this IScroll @this, in T instance) => IRMethods<T>.INSTANCE.Insert(@this, in instance);
    /// <summary>
    /// 未知の型を巻子に挿入します。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="instance"></param>
    /// <returns>挿入を保証する用務。</returns>
    [MI(MIO.AggressiveInlining)]
    public static Task Insert<T>(this IScroll @this, T instance) => IRMethods<T>.INSTANCE.Insert(@this, in instance);
    /// <summary>
    /// 未知の型を巻子から搴取します。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="instance"></param>
    /// <returns>挿入を保証する用務。</returns>
    [MI(MIO.AggressiveInlining)]
    [Mark("generic_remove_method")]
    public static Task Remove<T>(this IScroll @this, out T instance) => IRMethods<T>.INSTANCE.Remove(@this, out instance);
    /// <summary>
    /// 未知の型を巻子から搴取します。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="instance"></param>
    /// <returns>挿入を保証する用務。</returns>
    [MI(MIO.AggressiveInlining)]
    public static Task Remove<T>(this IScroll @this, T instance) => IRMethods<T>.INSTANCE.Remove(@this, out instance);

    #endregion
    #region ForIRMethods<T>

    /// <summary>
    /// 初期化時に読込まれた型定義ごとの全ての挿搴務容の候補の情報を保持する欄。
    /// </summary>
    internal static Dictionary<Type, (List<MethodInfo> insertMIs, List<MethodInfo> removeMIs)> IRMethodPairs { get; } = new();

    /// <summary>
    /// 未知型巻子拡張挿搴関数を初期化します。
    /// </summary>
    static void InitSectionForIRMethods()
    {
        var asms = AppDomain.CurrentDomain.GetAssemblies();
        AppDomain.CurrentDomain.AssemblyLoad += async (_, e) => await Run(() =>
        {
            if (asms.Contains(e.LoadedAssembly)) return;
            OnLoad(e.LoadedAssembly);
        });
        foreach (var asm in asms) OnLoad(asm);

        static void OnLoad(Assembly assembly)
        {
            var names = assembly.GetReferencedAssemblies();
            if (assembly != ASSEMBLY && !assembly.GetReferencedAssemblies().Any(x => AssemblyName.ReferenceMatchesDefinition(x, ASSEMBLY_NAME))) return;

            foreach (var type in assembly.DefinedTypes)
            {
                foreach (var mI in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    if (mI.GetCustomAttribute<IRMethodAttribute>() is not { } irMA) continue;

                    if (
                        mI.ReturnType.GetAwaitResult() is { } awaitRT && awaitRT == typeof(void) ||
                        mI.Name is not nameof(Insert) and not nameof(Remove) ||
                        mI.GetCustomAttribute<ExtensionAttribute>() is null ||
                        mI.DeclaringType is { IsGenericType: true } dT ||
                        mI.GetParameters() is not { Length: 2 } pIs ||
                        pIs[0].ParameterType != typeof(IScroll) ||
                        pIs[1].ParameterType.IsInterface
                        )
                    {
                        throw new InvalidAttributeException(mI, irMA);
                        //continue;
                    }

                    var key = pIs[1].ParameterType;
                    if (key.IsByRef) key = key.GetElementType() ?? throw new Exception("不明な錯誤です。参照渡し型の`ElementType`が`null`でした。");
                    if (key.IsGenericType) key = key.GetGenericTypeDefinition();
                    if (key.IsGenericParameter) if (mI.DeclaringType != typeof(ScrollExtensions)) throw new NotSupportedException("現在、ジェネリック単一の型を引数とした挿搴務容は定義できません。");

                    if (!IRMethodPairs.TryGetValue(key, out var methods))
                    {
                        methods = (new(), new());
                        IRMethodPairs.Add(key, methods);
                    }

                    switch (mI.Name)
                    {
                    case nameof(Insert):
                        methods.insertMIs.Add(mI);
                        break;
                    case nameof(Remove):
                        methods.removeMIs.Add(mI);
                        break;
                    }
                }
            }
        }
    }

    internal static Task ByRef<T>(this InsertCopiedDelegate<T> @this, in T instance)
    {
        return @this(instance);
    }
    internal static Task ByRef<T>(this RemoveCopiedDelegate<T> @this, out T instance)
    {
        throw new NotImplementedException();
        //return @this(instance);
    }

    #endregion
}
#pragma warning restore IDE0001 // 意図せずInsert(value:)の参照先が変わらぬようつける。

public delegate Task InsertDelegate<T>(in T instance);
public delegate Task InsertCanncellableDelegate<T>(in T instance, CancellationToken cancellationToken);
public delegate Task InsertCopiedDelegate<T>(T instance);
public delegate Task InsertCopiedCancellableDelegate<T>(T instance, CancellationToken cancellationToken);
public delegate Task RemoveDelegate<T>(out T instance);
public delegate Task RemoveCancellableDelegate<T>(in T instance, CancellationToken cancellationToken);
public delegate Task RemoveCopiedDelegate<T>(T instance);
public delegate Task RemoveCopiedCancellableDelegate<T>(T instance, CancellationToken cancellationToken);

// 複数の用務の同時実行非許容版。
///// <summary>
///// 挿搴務容を持つ二ない実体の型を表します。
///// </summary>
///// <typeparam name="T">
///// 挿搴務容対象の型。
///// </typeparam>
//public sealed class IRMethods<T>
//{
//    public static readonly IRMethods<T> INSTANCE = new();

//    bool _isUpdated;
//    MethodInfo? _insertMethodInfo;
//    MethodInfo? _removeMethodInfo;
//    [ThreadStatic]
//    static RelayNote? _relay; // これを共通化すると再帰的に泛型挿搴務容が呼ばれたときに上位が循環参照化してしまう。
//    [ThreadStatic]
//    static Task? _relayUsingTask;
//    [ThreadStatic]
//    static InsertDelegate<T>? _insertDelegate;
//    [ThreadStatic]
//    static RemoveDelegate<T>? _removeDelegate;

//    public bool Exists
//    {
//        [MI(MIO.AggressiveInlining)]
//        [MemberNotNullWhen(true, nameof(_insertMethodInfo), nameof(_removeMethodInfo))]
//        get
//        {
//            if (!_isUpdated) UpdateMethodInfos();
//            return _insertMethodInfo is not null && _removeMethodInfo is not null;
//        }
//    }

//    private IRMethods() { }

//    /// <summary>
//    /// 務容を用いて巻子に値を挿入します。
//    /// </summary>
//    /// <param name="note">
//    /// 挿入する先の巻子。
//    /// </param>
//    /// <param name="instance">
//    /// 挿入する値。
//    /// </param>
//    /// <returns>
//    /// 挿入を保証する用務。
//    /// </returns>
//    /// <exception cref="InvalidOperationException">
//    /// 同じ作絡で最後に実行された用務が未だ終了していない場合に投げられます。最後の書込み用務を待機してください。または、型の定義に対応する挿入務容が、最後に更新された時点で読み込まれている全ての繹典内に見つからない場合に投げられます。
//    /// </exception>
//    [MI(MIO.AggressiveInlining)]
//    public Task Insert(INote note, in T instance)
//    {
//        if (_relay == null) _relay = new();
//        if (_relayUsingTask != null && !_relayUsingTask.IsCompleted) throw new InvalidOperationException("同じ作絡で最後に実行された用務が未だ終了していません。");
//        _relay.Target(note);

//        if (_insertDelegate == null) UpdateDelegates();
//        return _relayUsingTask = _insertDelegate(in instance);
//    }
//    /// <summary>
//    /// 務容を用いて巻子から値を搴取します。
//    /// </summary>
//    /// <param name="note">
//    /// 搴取する元の巻子。
//    /// </param>
//    /// <param name="instance">
//    /// 搴取された値。
//    /// </param>
//    /// <returns>
//    /// 搴取を保証する用務。
//    /// </returns>
//    /// <exception cref="InvalidOperationException">
//    /// 同じ作絡で最後に実行された用務が未だ終了していない場合に投げられます。最後の書込み用務を待機してください。または、型の定義に対応する挿入務容が、最後に更新された時点で読み込まれている全ての繹典内に見つからない場合に投げられます。
//    /// </exception>
//    [MI(MIO.AggressiveInlining)]
//    public Task Remove(INote note, out T instance)
//    {
//        if (_relay == null) _relay = new();
//        if (_relayUsingTask != null && !_relayUsingTask.IsCompleted) throw new InvalidOperationException("同じ作絡で最後に実行された用務が未だ終了していません。");
//        _relay.Target(note);

//        if (_removeDelegate == null) UpdateDelegates();
//        return _relayUsingTask = _removeDelegate(out instance);
//    }

//    /// <summary>
//    /// 代容を更新します。
//    /// </summary>
//    /// <exception cref="InvalidOperationException">
//    /// 型の定義に対応する挿入務容が、最後に更新された時点で読み込まれている全ての繹典内に見つからない場合に投げられます。
//    /// </exception>
//    [MemberNotNull(nameof(_insertDelegate), nameof(_removeDelegate))]
//    void UpdateDelegates()
//    {
//        if (!Exists) throw new InvalidOperationException("型の定義に対応する挿入務容が、最後に更新された時点で読み込まれている全ての繹典内に見つかりませんでした。");

//        if (_insertMethodInfo.GetParameters()[1].ParameterType.IsByRef)
//            _insertDelegate = _insertMethodInfo.CreateDelegate<InsertDelegate<T>>(_relay);
//        else
//            _insertDelegate = _insertMethodInfo.CreateDelegate<InsertCopiedDelegate<T>>(_relay).ByRef;

//        if (_removeMethodInfo.GetParameters()[1].ParameterType.IsByRef)
//            _removeDelegate = _removeMethodInfo.CreateDelegate<RemoveDelegate<T>>(_relay);
//        else
//            _removeDelegate = _removeMethodInfo.CreateDelegate<RemoveCopiedDelegate<T>>(_relay).ByRef;
//    }

//    /// <summary>
//    /// 務容情報を更新します。
//    /// </summary>
//    /// <exception cref="Exception"></exception>
//    public void UpdateMethodInfos()
//    {
//        _isUpdated = true;
//        _insertMethodInfo = _removeMethodInfo = null;

//        var targetT = typeof(T);
//        var key = targetT.IsGenericType ? targetT.GetGenericTypeDefinition() : targetT;

//        if (!NoteUtils.IRMethodPairs.TryGetValue(key, out var methods)) return;

//        // _insertMI初期化
//        foreach (var insertMI_cand in methods.insertMIs)
//        {
//            if (insertMI_cand.IsGenericMethod)
//            {
//                var genArgs = insertMI_cand.GetGenericArguments();
//                var pI = insertMI_cand.GetParameters()[1];
//                var isByRef = pI.ParameterType.IsByRef;
//                var parameterType = isByRef ? pI.ParameterType.GetElementType(true) : pI.ParameterType;

//                if (Conform(parameterType.GetGenericArguments(), targetT.GetGenericArguments(), genArgs))
//                {
//                    if (_insertMethodInfo is not null)
//                    {
//                        switch (_insertMethodInfo.GetParameters()[1].ParameterType.IsByRef, isByRef)
//                        {
//                        case (true, false):
//                            break;
//                        case (false, true):
//                            _insertMethodInfo = insertMI_cand.MakeGenericMethod(genArgs);
//                            break;
//                        default:
//                            throw new Exception("指定された具体型の定義に対応する挿入務容が競合して、適切な組を取得することができません。");
//                        }
//                    }
//                    else
//                    {
//                        _insertMethodInfo = insertMI_cand.MakeGenericMethod(genArgs);
//                    }
//                }
//            }
//            else
//            {
//                _insertMethodInfo = insertMI_cand;
//                break;
//            }
//        }
//        if (_insertMethodInfo is null) return;

//        // _removeMI初期化
//        foreach (var removeMI_cand in methods.removeMIs)
//        {
//            if (removeMI_cand.IsGenericMethod)
//            {
//                var genArgs = removeMI_cand.GetGenericArguments();
//                var pI = removeMI_cand.GetParameters()[1];
//                var isByRef = pI.ParameterType.IsByRef;
//                var parameterType = isByRef ? pI.ParameterType.GetElementType(true) : pI.ParameterType;

//                if (Conform(parameterType.GetGenericArguments(), targetT.GetGenericArguments(), genArgs))
//                {
//                    if (_removeMethodInfo is not null)
//                    {
//                        switch (_removeMethodInfo.GetParameters()[1].ParameterType.IsByRef, isByRef)
//                        {
//                        case (true, false):
//                            break;
//                        case (false, true):
//                            _removeMethodInfo = removeMI_cand.MakeGenericMethod(genArgs);
//                            break;
//                        default:
//                            throw new Exception("指定された具体型の定義に対応する搴取務容が競合して、適切な組を取得することができません。");
//                        }
//                    }
//                    else
//                    {
//                        _removeMethodInfo = removeMI_cand.MakeGenericMethod(genArgs);
//                    }
//                }
//            }
//            else
//            {
//                _removeMethodInfo = removeMI_cand;
//            }
//        }
//        if (_removeMethodInfo is null) return;

//        if (_insertMethodInfo.DeclaringType != _removeMethodInfo.DeclaringType) throw new Exception("指定された具体型の定義に対応する挿搴務容が競合して、適切な組を取得することができません。");
//        return;

//        static bool Conform(Type[] args, Type[] to, Type[] genericParameters)
//        {
//            for (int i = 0; i < args.Length; i++)
//            {
//                if (args[i].IsGenericParameter)
//                {
//                    foreach (var constraint in args[i].GetGenericParameterConstraints())
//                    {
//                        if (!to[i].IsAssignableTo(constraint)) return false;
//                    }
//                    genericParameters[args[i].GenericParameterPosition] = to[i];
//                }
//                else if (args[i].IsGenericType)
//                {
//                    var isSucceeded = Conform(args[i].GetGenericArguments(), to[i].GetGenericArguments(), genericParameters);
//                    if (!isSucceeded) return false;
//                }
//            }
//            return true;
//        }
//    }
//}

/// <summary>
/// 挿搴務容を持つ二ない実体の型を表します。
/// </summary>
/// <typeparam name="T">
/// 挿搴務容対象の型。
/// </typeparam>
public sealed class IRMethods<T>
{
    public static readonly IRMethods<T> INSTANCE = new();

    bool _isUpdated;
    MethodInfo? _insertMethodInfo;
    MethodInfo? _removeMethodInfo;
    [ThreadStatic]
    static List<Delegates>? _delegates;

    public bool Exists
    {
        [MI(MIO.AggressiveInlining)]
        [MemberNotNullWhen(true, nameof(_insertMethodInfo), nameof(_removeMethodInfo))]
        get
        {
            if (!_isUpdated) UpdateMethodInfos();
            return _insertMethodInfo is not null && _removeMethodInfo is not null;
        }
    }

    private IRMethods() { }

    /// <summary>
    /// 務容を用いて巻子に値を挿入します。
    /// </summary>
    /// <param name="note">
    /// 挿入する先の巻子。
    /// </param>
    /// <param name="instance">
    /// 挿入する値。
    /// </param>
    /// <returns>
    /// 挿入を保証する用務。
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// 同じ作絡で最後に実行された用務が未だ終了していない場合に投げられます。最後の書込み用務を待機してください。または、型の定義に対応する挿入務容が、最後に更新された時点で読み込まれている全ての繹典内に見つからない場合に投げられます。
    /// </exception>
    public Task Insert(IScroll note, in T instance)
    {
        _delegates ??= new(capacity: 1);

        Delegates delegates;
        // 同一作絡でも用務ごとに競合が発生することはある。競合が発生した場合は_delegatesに別のインスタンスを作ってそちらに回す。
        for (int i = 0; i < _delegates.Count; i++)
        {
            delegates = _delegates[i];
            // 中継の対象が同じ場合はDelegates実体を共有しようか迷うけれども、完了を表す用務は両方の用務の完了を待つ必要があり(Task.WhenAll)、どうせそこでnew負担がかかるのなら新しく作ってしまった方が良いと思われる。
            if (/*delegates.relay.Target == note || */delegates.task.IsCompleted) goto fin;
        }
        delegates = GetDelegates();
        _delegates.Add(delegates);

        fin:
        delegates.relay.Target = note;
        return delegates.task = delegates.insertDelegate(in instance);
    }
    /// <summary>
    /// 務容を用いて巻子から値を搴取します。
    /// </summary>
    /// <param name="note">
    /// 搴取する元の巻子。
    /// </param>
    /// <param name="instance">
    /// 搴取された値。
    /// </param>
    /// <returns>
    /// 搴取を保証する用務。
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// 同じ作絡で最後に実行された用務が未だ終了していない場合に投げられます。最後の書込み用務を待機してください。または、型の定義に対応する挿入務容が、最後に更新された時点で読み込まれている全ての繹典内に見つからない場合に投げられます。
    /// </exception>
    public Task Remove(IScroll note, out T instance)
    {
        _delegates ??= new(capacity: 1);

        Delegates delegates;
        // 同一作絡でも用務ごとに競合が発生することはある。競合が発生した場合は_delegatesに別のインスタンスを作ってそちらに回す。
        for (int i = 0; i < _delegates.Count; i++)
        {
            delegates = _delegates[i];
            // 中継の対象が同じ場合はDelegates実体を共有しようか迷うけれども、完了を表す用務は両方の用務の完了を待つ必要があり(Task.WhenAll)、どうせそこでnew負担がかかるのなら新しく作ってしまった方が良いと思われる。
            if (/*delegates.relay.Target == note || */delegates.task.IsCompleted) goto fin;
        }
        delegates = GetDelegates();
        _delegates.Add(delegates);

        fin:
        delegates.relay.Target = note;
        return delegates.task = delegates.removeDelegate(out instance);
    }

    Delegates GetDelegates()
    {
        if (!Exists) throw new InvalidOperationException("型の定義に対応する挿入務容が、最後に更新された時点で読み込まれている全ての繹典内に見つかりませんでした。");

        var relay = new RelayNote();
        InsertDelegate<T> insertDelegate = _insertMethodInfo.GetParameters()[1].ParameterType.IsByRef
            ? _insertMethodInfo.CreateDelegate<InsertDelegate<T>>(relay)
            : _insertMethodInfo.CreateDelegate<InsertCopiedDelegate<T>>(relay).ByRef;
        RemoveDelegate<T> removeDelegate = _removeMethodInfo.GetParameters()[1].ParameterType.IsByRef
            ? _removeMethodInfo.CreateDelegate<RemoveDelegate<T>>(relay)
            : _removeMethodInfo.CreateDelegate<RemoveCopiedDelegate<T>>(relay).ByRef;

        return new Delegates(relay, insertDelegate, removeDelegate);
    }

    /// <summary>
    /// 務容情報を更新します。
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void UpdateMethodInfos()
    {
        _isUpdated = true;
        _insertMethodInfo = _removeMethodInfo = null;

        var targetT = typeof(T);
        var key = targetT.IsGenericType ? targetT.GetGenericTypeDefinition() : targetT;

        if (!FundamentalScrollUtils.IRMethodPairs.TryGetValue(key, out var methods)) return;

        // _insertMI初期化
        foreach (var insertMI_cand in methods.insertMIs)
        {
            if (insertMI_cand.IsGenericMethod)
            {
                var genArgs = insertMI_cand.GetGenericArguments();
                var pI = insertMI_cand.GetParameters()[1];
                var isByRef = pI.ParameterType.IsByRef;
                var parameterType = isByRef ? pI.ParameterType.GetElementType(true) : pI.ParameterType;

                if (Conform(parameterType.GetGenericArguments(), targetT.GetGenericArguments(), genArgs))
                {
                    if (_insertMethodInfo is not null)
                    {
                        switch (_insertMethodInfo.GetParameters()[1].ParameterType.IsByRef, isByRef)
                        {
                        case (true, false):
                            break;
                        case (false, true):
                            _insertMethodInfo = insertMI_cand.MakeGenericMethod(genArgs);
                            break;
                        default:
                            throw new Exception("指定された具体型の定義に対応する挿入務容が競合して、適切な組を取得することができません。");
                        }
                    }
                    else
                    {
                        _insertMethodInfo = insertMI_cand.MakeGenericMethod(genArgs);
                    }
                }
            }
            else
            {
                _insertMethodInfo = insertMI_cand;
                break;
            }
        }
        if (_insertMethodInfo is null) return;

        // _removeMI初期化
        foreach (var removeMI_cand in methods.removeMIs)
        {
            if (removeMI_cand.IsGenericMethod)
            {
                var genArgs = removeMI_cand.GetGenericArguments();
                var pI = removeMI_cand.GetParameters()[1];
                var isByRef = pI.ParameterType.IsByRef;
                var parameterType = isByRef ? pI.ParameterType.GetElementType(true) : pI.ParameterType;

                if (Conform(parameterType.GetGenericArguments(), targetT.GetGenericArguments(), genArgs))
                {
                    if (_removeMethodInfo is not null)
                    {
                        switch (_removeMethodInfo.GetParameters()[1].ParameterType.IsByRef, isByRef)
                        {
                        case (true, false):
                            break;
                        case (false, true):
                            _removeMethodInfo = removeMI_cand.MakeGenericMethod(genArgs);
                            break;
                        default:
                            throw new Exception("指定された具体型の定義に対応する搴取務容が競合して、適切な組を取得することができません。");
                        }
                    }
                    else
                    {
                        _removeMethodInfo = removeMI_cand.MakeGenericMethod(genArgs);
                    }
                }
            }
            else
            {
                _removeMethodInfo = removeMI_cand;
            }
        }
        if (_removeMethodInfo is null) return;

        if (_insertMethodInfo.DeclaringType != _removeMethodInfo.DeclaringType) throw new Exception("指定された具体型の定義に対応する挿搴務容が競合して、適切な組を取得することができません。");
        return;

        static bool Conform(Type[] args, Type[] to, Type[] genericParameters)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].IsGenericParameter)
                {
                    foreach (var constraint in args[i].GetGenericParameterConstraints())
                    {
                        if (!to[i].IsAssignableTo(constraint)) return false;
                    }
                    genericParameters[args[i].GenericParameterPosition] = to[i];
                }
                else if (args[i].IsGenericType)
                {
                    var isSucceeded = Conform(args[i].GetGenericArguments(), to[i].GetGenericArguments(), genericParameters);
                    if (!isSucceeded) return false;
                }
            }
            return true;
        }
    }

    class Delegates
    {
        public readonly RelayNote relay;
        public readonly InsertDelegate<T> insertDelegate;
        public readonly RemoveDelegate<T> removeDelegate;
        public Task task;

        public Delegates(RelayNote relay, InsertDelegate<T> insertDelegate, RemoveDelegate<T> removeDelegate) => (this.relay, this.insertDelegate, this.removeDelegate, task) = (relay, insertDelegate, removeDelegate, CompletedTask);
    }
}

public class RelayNote : IScroll
{
    IScroll? _target;

    public IScroll? Target
    {
        get => _target;
        set
        {
            if (value == this) throw new ArgumentException("中継先を自身で循環参照させることはできません。");
            _target = value;
        }
    }

    public void Detarget()
    {
        if (_target == null) throw new InvalidOperationException("既に中継巻子は解放されています。行程の混乱が起こっている可能性があります。");
        _target = null;
    }

#nullable disable
    public ScrollPointer Point { get => _target.Point; set => _target.Point = value; }
    public IScroll Copy() => _target.Copy();
    public Task<IScroll> CopyAsync() => _target.CopyAsync();
    public void Dispose() => _target.Dispose();
    public ValueTask DisposeAsync() => _target.DisposeAsync();
    public Task Insert(in ScrollPointer pointer) => _target.Insert(pointer);
    public Task Insert<T>(Memory<T> memory) where T : unmanaged => _target.Insert(memory);
    public void InsertSync<T>(Span<T> span) where T : unmanaged => _target.InsertSync(span);
    public bool IsValid(ScrollPointer pointer) => _target.IsValid(pointer);
    public Task Remove(out ScrollPointer index) => _target.Remove(out index);
    public Task Remove<T>(Memory<T> memory) where T : unmanaged => _target.Remove(memory);
    public void RemoveSync<T>(Span<T> span) where T : unmanaged => _target.RemoveSync(span);
    public long FigureOutDistance<T>(ScrollPointer to) => _target.FigureOutDistance<T>(to);
#nullable restore
}

public class AirNote : IScroll
{
    private readonly static object KEY = new();
    public readonly static AirNote INSTANCE = new();
    public readonly static ScrollPointer AIR_POINT = new(information: KEY);

    ScrollPointer IScroll.Point { get => AIR_POINT; set { if (value.Information != KEY) throw new ArgumentException("指示子の出所が異なります。", nameof(value)); } }

    IScroll IScroll.Copy() => this;
    void IDisposable.Dispose() { }
    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;
    Task IScroll.Insert(in ScrollPointer pointer) => CompletedTask;
    Task IScroll.Insert<T>(Memory<T> memory) => CompletedTask;
    void IScroll.InsertSync<T>(Span<T> span) { }
    bool IScroll.IsValid(ScrollPointer pointer) => pointer == AIR_POINT;
    Task IScroll.Remove(out ScrollPointer pointer) { pointer = AIR_POINT; return CompletedTask; }
    Task IScroll.Remove<T>(Memory<T> memory) => CompletedTask;
    void IScroll.RemoveSync<T>(Span<T> span) { }
    public long FigureOutDistance<T>(ScrollPointer to) => 0;
}

public delegate T CopyDelegate<T>(T original);
