using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Nonno.Assets.Collections;
public interface ISet<T> : IEnumerable<T>
{
    bool Contains(T item);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
