using System;
using System.Collections.Generic;
using System.Text;

namespace Nonno.Assets.Collections;

public class ResourceIndex : Context.Index
{
    public string Address { get; }

    public ResourceIndex(Context context, string address) : base(context)
    {
        Address = address;
    }
}
