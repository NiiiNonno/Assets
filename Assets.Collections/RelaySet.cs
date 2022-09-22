// 令和弐年大暑確認済。

namespace Nonno.Assets.Collections;

public class RelaySet<TParameter, TRelay, TSet> where TRelay : Relay<TParameter> where TSet : ISet<Delegate>
{
    readonly TRelay _relay;
    readonly TSet _targets;

    public RelaySet(Constructor<TRelay> relayConstructor, Constructor<TSet> setConstructor)
    {
        _relay = relayConstructor();
        _targets = setConstructor();
    }

    public void Add<U>(RelayTarget<U> recepter)
    {
        if (_targets.Add(recepter))
        {
            _relay.Target((IReadOnlySet<Delegate>)_targets);
        }
    }

    public void Remove<U>(RelayTarget<U> recepter)
    {
        if (_targets.Remove(recepter))
        {
            _relay.Target((IReadOnlySet<Delegate>)_targets);
        }
    }

    public void Invoke(TParameter courier) => _relay.Delegate(courier);
}

public class Nexts<T> : RelaySet<T, Relay<T>, ListSet<Delegate>>
{
    public Nexts() : base(() => new(), () => new()) { }
}

public class CachedNexts<T> : RelaySet<T, CachedRelay<T>, HashSet<Delegate>>
{
    public CachedNexts() : base(() => new(), () => new()) { }
}
