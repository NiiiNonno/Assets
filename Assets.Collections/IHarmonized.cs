using System;
using System.Collections.Generic;
using System.Text;

namespace Nonno.Assets.Collections;
/// <summary>
/// <para>
/// <see cref="ICollection{T}"/>とは要素をすべて除くことができないという点で異なります。
/// </para>
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IHarmonized<T>
{
    void Include(T item);
    void Exclude(T item);
}
