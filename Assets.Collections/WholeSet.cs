using System;
using System.Collections.Generic;
using System.Text;

namespace Nonno.Assets.Collections;
public class WholeSet<T> : ISet<T>
{
    private WholeSet() { }

    public bool Contains(T item) => true;
    public IEnumerator<T> GetEnumerator() => Enumerator ?? throw new NotSupportedException();

    public static IEnumerator<T>? Enumerator { get; set; }
    public static WholeSet<T> Instance { get; }

    static WholeSet()
    {
        Instance = new();

        switch (default(T))
        {
        case int:
        default:
            break;
        }
    }
}
