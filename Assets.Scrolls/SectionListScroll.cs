using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nonno.Assets.Collections;
using SysGC = System.Collections.Generic;
using static Nonno.Assets.Shift;

namespace Nonno.Assets.Scrolls;
public abstract class SectionListScroll<TSection> : SectionScroll<TSection> where TSection : Section
{
    Dictionary<ulong, TSection> _list;

    protected override TSection this[ulong number] => _list[number];
    protected SysGC::IReadOnlyDictionary<ulong, TSection> List => _list;

    public SectionListScroll(ulong currentNumber) : base(currentNumber)
    {
        _list = new();
    }
    public SectionListScroll(IEnumerable<TSection> sections, ulong currentNumber) : base(currentNumber)
    {
        var list = new Dictionary<ulong, TSection>();

        var count = sections.Count();
        var dif = ulong.MaxValue / (ulong)count;
        ulong c_key = 0;
        var c_value = this[Section.ENTRY_SECTION_NUMBER];
        foreach (var section in sections)
        {
            list.Add(c_key, c_value);

            c_key += dif;
            c_value = section;
        }

        _list = list;
    }

    protected override ulong FindVacantNumber(ulong? previousSectionNumber, ulong? nextSectionNumber)
    {
        if (previousSectionNumber is ulong pSN && nextSectionNumber is ulong nSN)
        {
            var pS = this[pSN];
            var nS = this[nSN];

            var dif = nSN - pSN;
            if (dif < 2)
            {
                Rearrange();
                IEnumerable<KeyValuePair<ulong, TSection>> list = _list;
                pSN = list.FindKey(pS);
                nSN = list.FindKey(nS);
                dif = nSN - pSN;
                if (dif < 2) throw new Exception("要素数が膨大です。");
            }

            return pSN + (dif / S2);
        }
        else
        {
            ulong cand = ulong.MaxValue;
            do cand--;
            while (!_list.ContainsKey(cand));
            return cand;
        }
    }

    public virtual void Rearrange()
    {
        var list = new Dictionary<ulong, TSection>();

        var count = _list.Count;
        var dif = ulong.MaxValue / (ulong)count;
        ulong c_key = 0;
        var c_value = this[Section.ENTRY_SECTION_NUMBER];
        for (int i = 0; i < count; i++)
        {
            list.Add(c_key, c_value);

            c_key += dif;
            c_value = _list[c_value.NextNumber];
        }

        _list = list;
    }
}
