using System;
using System.Collections.Generic;
using System.Text;

namespace Nonno.Assets.Collections;
public interface ITable<TKey, TValue>
{
    TValue? this[TKey key] { get; set; }
    
    ISet<TKey> Keys { get; }
}
