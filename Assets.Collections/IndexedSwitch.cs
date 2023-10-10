using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Nonno.Assets.Collections;
public class IndexedSwitch<TIndex> where TIndex : Context.Index
{
#if FOR_UNITY
    readonly CorrespondenceTable<TIndex, Action<object?>> _table;
#else
    readonly CorrespondenceTable<TIndex, object> _table;
#endif
    readonly ArrayList<TIndex> _indexes;

    public ReadOnlySpan<TIndex> CaseIndexes => _indexes.AsSpan();

    public IndexedSwitch(Context<TIndex> context)
    {
        _table = new(context);
        _indexes = new();
    }

    public void CaseDefault(TIndex index)
    {
        _indexes.Add(index);
    }

    public void Case<UIndex>(UIndex index, Action<UIndex>? action) where UIndex : TIndex
    {
        switch (_table[index], action)
        {
        case (null, not null): _indexes.Add(index); break;
        case (not null, null): _indexes.Remove(index); break;
        }

#if FOR_UNITY
        _table[index] = action is null ? null : x => action((UIndex)x);
#else
        _table[index] = Unsafe.As<Action<object>>(action);
#endif
    }

    public void Switch(TIndex index)
    {
#if FOR_UNITY
        _table[index]?.Invoke(index);
#else
        var action = Unsafe.As<Action<object>>(_table[index]);
        action?.Invoke(index);
#endif
    }
}
