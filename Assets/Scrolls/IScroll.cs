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
public interface IScroll : IDisposable
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
    /// 軸箋と現在位置の間がないかどうかを返します。
    /// </summary>
    /// <param name="on">
    /// 軸箋。
    /// </param>
    /// <returns>
    /// 一致する場合は<see cref="true"/>、否や<see cref="false"/>。
    /// </returns>
    bool Is(ScrollPointer on);
    /// <summary>
    /// 軸箋を挿入します。
    /// </summary>
    /// <param name="pointer">
    /// 挿入する軸箋。
    /// </param>
    /// <returns>
    /// 軸箋を挿入したことを保証する用務。
    /// </returns>
    void Insert(in ScrollPointer pointer);
    /// <summary>
    /// 軸箋を搴取します。
    /// </summary>
    /// <param name="pointer">
    /// 搴取する軸箋。
    /// </param>
    /// <returns>
    /// 軸箋を搴取したことを保証する用務。
    /// </returns>
    void Remove(out ScrollPointer pointer);
    
    /// <summary>
    /// メモリの内容を挿入します。
    /// <para>
    /// この務容が行う処理は、<see cref="Insert{T}(Span{T})"/>が行う処理と実質的に同じです。
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
    Task InsertAsync<T>(Memory<T> memory, CancellationToken cancellationToken = default) where T : unmanaged;
    /// <summary>
    /// 値を挿入します。
    /// </summary>
    /// <typeparam name="T">
    /// 値の型。
    /// </typeparam>
    /// <param name="value">
    /// 挿入する値。
    /// </param>
    void Insert<T>(in T value) where T : unmanaged
    {
        Span<T> span = stackalloc[] { value };
        Insert(span: span);
    }
    /// <summary>
    /// 区間の内容を挿入します。
    /// <para>
    /// この務容が行う処理は、<see cref="InsertAsync{T}(Memory{T})"/>が行う処理と実質的に同じです。
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// 区間の内容の型。
    /// </typeparam>
    /// <param name="span">
    /// 挿入する内容の区間。
    /// </param>
    void Insert<T>(Span<T> span) where T : unmanaged;
    /// <summary>
    /// メモリの内容へ搴取します。
    /// <para>
    /// この務容が行う処理は、<see cref="Remove{T}(Span{T})"/>が行う処理と実質的に同じです。
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
    Task RemoveAsync<T>(Memory<T> memory, CancellationToken cancellationToken = default) where T : unmanaged;
    /// <summary>
    /// 値を搴取します。
    /// </summary>
    /// <typeparam name="T">
    /// 値の型。
    /// </typeparam>
    /// <param name="value">
    /// 搴取した値。
    /// </param>
    void Remove<T>(out T value) where T : unmanaged
    {
        Span<T> span = stackalloc T[1];
        Remove(span: span);
        value = span[0];
    }
    /// <summary>
    /// 区間の内容へ搴取します。
    /// <para>
    /// この務容が行う処理は、<see cref="RemoveAsync{T}(Memory{T})"/>が行う処理と実質的に同じです。
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// 区間の内容の型。
    /// </typeparam>
    /// <param name="span">
    /// 搴取した内容の区間。
    /// </param>
    void Remove<T>(Span<T> span) where T : unmanaged;
}

/// <summary>
/// 複数の箇所を同時に操作できる巻子を表します。
/// </summary>
public interface IParallelScroll : IScroll
{
    Task<IScroll> Parallelize();
}

public interface IScroll<TPointer>
{
    TPointer Position { get; set; }
    bool IsValid(TPointer pointer);
    bool Is(TPointer on);
    void Insert<T>(in T value) where T : unmanaged
    {
        Span<T> span = stackalloc[] { value };
        Insert(span: span);
    }
    void Insert<T>(Span<T> span) where T : unmanaged;
    void Remove<T>(out T value) where T : unmanaged
    {
        Span<T> span = stackalloc T[1];
        Remove(span: span);
        value = span[0];
    }
    void Remove<T>(Span<T> span) where T : unmanaged;
}
