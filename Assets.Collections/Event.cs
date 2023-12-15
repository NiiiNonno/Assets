using System;
using System.Text;

namespace Nonno.Assets.Collections;
public readonly struct Event<T>(ICollection<T> collection)
{
    readonly ICollection<T> _collection = collection;

    public static Event<T> operator +(Event<T> @event, T @delegate)
    {
        @event._collection.Add(@delegate);
        return @event;
    }
    public static Event<T> operator -(Event<T> @event, T @delegate)
    {
        @event._collection.Remove(@delegate);
        return @event;
    }
}
