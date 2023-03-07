using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nonno.Assets.Collections;
using static Nonno.Assets.Utils;

namespace Nonno.Assets.Scrolls;
public abstract class PartitionedScroll<TPartition> : IParallelScroll where TPartition : Partition
{
    #region 欄

    //readonly IVine<TPartition> _parts;
    //readonly IDictionary<ScrollPointer, ITuber<TPartition>> _refs;
    ICollection<PartitionedScroll<TPartition>>? _parallels;
    ITuber<TPartition> _next;
    int _magnitude;
    bool _isDisposed;

    #endregion
    #region 対外録

    public int Magnitude => _magnitude;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public ScrollPointer Point
    {
        get
        {
            var num = AverageCeiling(PreviousPartition.Number, NextPartition.Number);
            var part = CreatePartition();

            Partitions.InsertBefore(_next, part, out var tuber);

            
        }
        set
        {

        }
    }

    #endregion
    #region 対子録

    protected IVine<TPartition> Partitions => _parts;
    protected ITuber<TPartition> PreviousPartitionTuber => _next[-1];
    protected TPartition PreviousPartition => PreviousPartitionTuber.Value;
    protected ITuber<TPartition> NextPartitionTuber => _next;
    protected TPartition NextPartition => NextPartitionTuber.Value;

    #endregion
    #region 抽象録
    #endregion

    //public PartitionedScroll(IVine<TPartition> partitions, IDictionary<ScrollPointer, ITuber<TPartition>> referenceDictionary)
    //{
    //    _parts = partitions;
    //    _refs = referenceDictionary;
    //}

    #region 対外辯

    public void Rearrange()
    {

    }

    public PartitionedScroll<TPartition> Copy()
    {

    }
    IScroll IScroll.Copy() => Copy();

    #endregion
    #region 対子辯

    #endregion
    #region 抽象辯

    protected abstract void SortPartitions();

    protected abstract TPartition CreatePartition();
    protected abstract TPartition GetPartition(long number);
    protected abstract void DeletePartition(TPartition partition);

    #endregion
}

public abstract class Partition
{
    public long Number { get; set; }
    public abstract bool IsSetUpToRead { get; set; }
    public abstract bool IsSetUpToWrite { get; set; }

    
}
