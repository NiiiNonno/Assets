using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Collections;
/// <summary>
/// 型に制約のある峙を表します。
/// </summary>
/// <typeparam name="TConstraint"></typeparam>
public interface IHeap<TConstraint> : IAsyncEnumerable<TConstraint>, ICollection<TConstraint> where TConstraint : notnull
{
    ValueTask<bool> Contains<T>() where T : notnull, TConstraint;
    ValueTask<bool> Contains(Type type);
    /// <summary>
    /// ある型の嘼を取ります。
    /// <para>
    /// 峙に型の嘼が複数ある場合は、そのうちの麓の一つを取ります。
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// 取る型。
    /// </typeparam>
    /// <returns>
    /// 峙にある取る型の嘼。嘼が存在しなかった場合は既定値。
    /// </returns>
    ValueTask<T?> Get<T>() where T : notnull, TConstraint;
    /// <summary>
    /// ある型の嘼を取ります。
    /// <para>
    /// 峙に型の嘼が複数ある場合は、そのうちの麓の一つを取ります。
    /// </para>
    /// </summary>
    /// <param name="type">
    /// 取る型。
    /// </param>
    /// <returns>
    /// 峙にある取る型の嘼。嘼が存在しなかった場合は既定値。
    /// </returns>
    ValueTask<TConstraint?> Get(Type type);
    /// <summary>
    /// ある型の嘼を替えます。
    /// <para>
    /// 峙に型の嘼が複数ある場合は、そのうちの麓の一つを替えます。
    /// </para>
    /// <para>
    /// 峙に型の嘼が存在しない場合は、埜に置かれます。
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// 替える型。
    /// </typeparam>
    /// <param name="object">
    /// 替える嘼。
    /// </param>
    /// <returns>
    /// 峙にありぬ替える型の嘼。嘼が存在しなかった場合は既定値。
    /// </returns>
    ValueTask<T?> Move<T>(T? @object) where T : notnull, TConstraint;
    /// <summary>
    /// ある型の嘼を替えます。
    /// <para>
    /// 峙に型の嘼が複数ある場合は、そのうちの麓の一つを替えます。
    /// </para>
    /// <para>
    /// 峙に型の嘼が存在しない場合は、埜に置かれます。
    /// </para>
    /// </summary>
    /// <param name="type">
    /// 替える型。
    /// </param>
    /// <param name="object">
    /// 替える嘼。
    /// </param>
    /// <returns>
    /// 峙にありぬ替える型の嘼。嘼が存在しなかった場合は既定値。
    /// </returns>
    ValueTask<TConstraint?> Move(Type type, TConstraint? @object);
    /// <summary>
    /// ある型の嘼を設けます。
    /// <para>
    /// 峙に型の嘼が存在しない場合は、埜に置かれます。
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// 設ける型。
    /// </typeparam>
    /// <param name="object">
    /// 設ける嘼。
    /// </param>
    /// <returns></returns>
    Task Set<T>(T? @object) where T : notnull, TConstraint;
    /// <summary>
    /// ある型の嘼を設けます。
    /// <para>
    /// 峙に型の嘼が存在しない場合は、埜に置かれます。
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// 設ける型。
    /// </typeparam>
    /// <param name="object">
    /// 設ける嘼。
    /// </param>
    /// <returns></returns>
    Task Set(Type type, TConstraint? @object);
    /// <summary>
    /// 嘼を埜に加えます。
    /// </summary>
    /// <param name="object">
    /// 加える嘼。
    /// </param>
    /// <returns></returns>
    Task AddAsync(TConstraint @object);
    /// <summary>
    /// 嘼を消します。
    /// </summary>
    /// <param name="object">
    /// 消す嘼。
    /// </param>
    /// <returns></returns>
    Task RemoveAsync(TConstraint @object);
    Task ClearAsync(CancellationToken cancellationToken = default);
}
