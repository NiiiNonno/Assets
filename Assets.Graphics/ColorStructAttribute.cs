using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nonno.Assets.Collections;

namespace Nonno.Assets.Graphics;
[AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
sealed class ColorStructAttribute : Attribute
{
    public ImmutableArray<LayorInfo> LayorInfos { get; }

    /// <summary>
    /// 色構造を指定します。
    /// <para>
    /// 必ず以下の四要素の繰り返しによって指定してください。
    /// <code>
    /// <see cref="LayorType"/> layorType, <see cref="double"/> frequency, <see cref="LayorValueType"/> layorValueType, <see cref="int"/> bitLength
    /// </code>
    /// </para>
    /// </summary>
    public ColorStructAttribute(params object[] layorInfoEnumeration)
    {
        var infos = new LayorInfo[layorInfoEnumeration.Length / 4];
        int c = 0;
        for (int i = 0; i < infos.Length; i++)
        {
            infos[i] = new((LayorType)layorInfoEnumeration[c++], (double)layorInfoEnumeration[c++], (LayorValueType)layorInfoEnumeration[c++], (int)layorInfoEnumeration[c++]);
        }

        LayorInfos = ImmutableArray.Create(infos);
    }
    public ColorStructAttribute(LayorInfo[] layorInfos)
    {
        LayorInfos = ImmutableArray.Create(layorInfos);
    }
}

public enum LayorType
{
    None,
    Normal,
    Red, Green, Blue,
    Cyan, Magenta, Yellow, KeyPlate,
    White, Black,
    Alpha, Add, Subtract
}

public enum LayorValueType
{
    NaN = 0,
    Bit = 1,
    Int2,
    Int3,
    Int4,
    Int5,
    Int6,
    Int7,
    Byte = 8,
    Int16 = 16,
    Int32 = 32,
    Int64 = 64,
    Int128 = 128,
    Half = -8,
    Float = -16,
    Double = -32,
    Decimal = -10,
}

public readonly record struct LayorInfo(LayorType LayorType, double Frequency, LayorValueType LayorValueType, int BitLength);
