//#define USE_BYTE_SPAN
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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
    /// 現在の接続位置を定める軸箋を取得または設定します。
    /// <para>
    /// 一度取得した軸箋は一度使用することによって失効することに注意し、<see cref="ScrollPointer"/>の用法を正しく守ってください。
    /// </para>
    /// <para>
    /// 異常動作を避けるため、デバッガによる表示は避けてください。実装には<see cref="DebuggerBrowsableAttribute"/>にて<see cref="DebuggerBrowsableState.Never"/>を示してください。
    /// </para>
    /// </summary>
    ScrollPointer Point { get; set; }
    /// <summary>
    /// 軸箋がこの巻子に対して有効であるかを確かめます。この操作は<see cref="ScrollPointer"/>の有効性に影響を与えません。
    /// </summary>
    /// <param name="pointer">
    /// 確かめる軸箋。
    /// </param>
    /// <returns>
    /// 有効である場合は<see langword="true"/>、そうで無い場合は<see langword="false"/>。
    /// </returns>
    bool IsValid(ScrollPointer pointer);
    /// <summary>
    /// 軸箋と現在位置の間が<c>T</c>型区間として解釈できる場合、その長さを求めます。
    /// <para>
    /// この操作によって軸箋の有効性は変わりません。
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// 解釈する区間の型。
    /// </typeparam>
    /// <returns>
    /// 非負の場合は区間の長さ、負の場合は解釈に失敗したことを示す。特に零の場合は軸箋が現在位置を指示することを示す。
    /// </returns>
    long FigureOutDistance<T>(ScrollPointer to) => throw new NotSupportedException();
    bool Is(ScrollPointer on);
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
    /// 軸箋を挿入します。
    /// </summary>
    /// <param name="pointer">
    /// 挿入する軸箋。
    /// </param>
    /// <returns>
    /// 軸箋を挿入したことを保証する用務。
    /// </returns>
    Task Insert(in ScrollPointer pointer);
    /// <summary>
    /// 軸箋を挿入します。
    /// </summary>
    /// <param name="pointer">
    /// 挿入する軸箋。
    /// </param>
    /// <returns>
    /// 軸箋を挿入したことを保証する用務。
    /// </returns>
    Task Insert(in ScrollPointer pointer, CancellationToken cancellationToken = default) => cancellationToken.IsCancellationRequested ? FromCanceled(cancellationToken) : Insert(pointer: pointer);
    /// <summary>
    /// 軸箋を搴取します。
    /// </summary>
    /// <param name="pointer">
    /// 搴取する軸箋。
    /// </param>
    /// <returns>
    /// 軸箋を搴取したことを保証する用務。
    /// </returns>
    Task Remove(out ScrollPointer pointer);
    /// <summary>
    /// 軸箋を搴取します。
    /// </summary>
    /// <param name="pointer">
    /// 搴取する軸箋。
    /// </param>
    /// <returns>
    /// 軸箋を搴取したことを保証する用務。
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
