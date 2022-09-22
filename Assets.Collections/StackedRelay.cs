namespace Nonno.Assets.Collections;

public class StackedRelay<T, TSet> : Relay<T> where TSet : ISet<Delegate>
{
    readonly Func<IReadOnlySet<Delegate>, TSet> _constructor;
    IReadOnlySet<Delegate> _targets;

    public StackedRelay(Func<IReadOnlySet<Delegate>, TSet> constructor)
    {
        _constructor = constructor;
        _targets = EmptyCollection<Delegate>.INSTANCE;
    }

    public override void Target(IReadOnlySet<Delegate> targets)
    {
        var aETargets = _targets;
        _targets = targets;
        if (aETargets is IDisposable disposable) disposable.Dispose();

        Target();
    }

    public bool Add<U>(RelayTarget<U> target) => Add(target);
    internal bool Add(Delegate @delegate)
    {
        if (_targets is not ISet<Delegate> targets)
        {
            targets = _constructor(_targets);
            Target((IReadOnlySet<Delegate>)targets);
        }

        return targets.Add(@delegate);
    }

    public bool Remove<U>(RelayTarget<U> target) => Remove(target);
    internal bool Remove(Delegate @delegate)
    {
        if (_targets is not ISet<Delegate> targets)
        {
            targets = _constructor(_targets);
            Target((IReadOnlySet<Delegate>)targets);
        }

        return targets.Remove(@delegate);
    }

    void Target() => base.Target(_targets ?? EmptyCollection<Delegate>.INSTANCE);
}

public class StackedRelay<T> : StackedRelay<T, CompactSet<Delegate>>
{
    public StackedRelay() : base(x => new CompactSet<Delegate>(x)) { }
}
