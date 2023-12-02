using System;
using System.Collections.Generic;
using System.Text;

namespace Nonno.Assets.Collections;
public interface IFinite
{
    int Count { get; }
    long LongCount => Count;
}
