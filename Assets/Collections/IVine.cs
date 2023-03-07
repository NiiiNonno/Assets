using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Collections;
public interface IVine<T>
{
    int Count { get; }

    void InsertBefore(ITuber<T> tuber, T value, out ITuber<T> neo);
    void InsertAfter(ITuber<T> tuber, T value, out ITuber<T> neo);
}

public interface ITuber<T>
{
    public T Value { get; set; }
    ITuber<T>? this[int index] { get; }
}
