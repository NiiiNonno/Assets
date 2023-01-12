using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;
using IS = System.Runtime.InteropServices;
namespace Nonno.Assets;

public abstract class Marshal
{
    public const string RUNTIME_STRUCTURE_ORDER = "RuntimeStructure";

    // [型名/形式名]#[通番号]@[作成者/作成枝]+[他]
    public string SourceOrder {get;init;}
    public string ResultOrder {get;init;}

    public Marshal()
    {
        SourceOrder = null!;
        ResultOrder = null!;
    }

    public virtual unsafe void Conduct<T>(ref void* source, ref void* to) where T : unmanaged => Conduct(typeof(T), ref source, ref to);
    protected abstract unsafe void Conduct(Type type, ref void* source, ref void* to);
}

public abstract class StandardMarshal : Marshal
{
    public const int STANDARD_PACK = 8;// 64ビットでの標準パック数。その他のビット数でも互換性のためこれに合わせる。
    public const string STANDARD_ORDER = "Standard";

    Dictionary<Type, StructureLayout> _structureLayouts;

    public Dictionary<Type, StructureLayout> StructureLayouts => _structureLayouts;

    public StandardMarshal(Dictionary<Type, StructureLayout>? structureLayouts = null)
    {
        _structureLayouts = structureLayouts ?? new();
    }
    
    protected virtual StructureLayout GetStructureLayout(Type type)
    {
        if (!StructureLayouts.TryGetValue(type, out var structureLayout))
        {
            // var pack = type.StructLayoutAttribute is {} layout ? layout.Pack : sizeof(nint);
            // var isAuto = type.IsAutoLayout;
            // var fs = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            // List<FieldOffset> fieldOffsets_ = new(fs.Length);

            // if (isAuto)
            // {
            //     for (int i = 0; i < fs.Length; i++)
            //     {
            //         var fT = fs[i].FieldType;
            //         var curLclOfs = IS.Marshal.OffsetOf(type, fs[i].Name);

            //         fieldOffsets_.Add(new(fT, curLclOfs));
            //     }

            //     fieldOffsets_.Sort((x, y) => x.Type.Name.CompareTo(y.Type.Name));
            // }
            // else // isSequencial
            // {
            //     nint lastLclOfs = 0;
            //     nint lastStdOfs = 0;
            //     for (int i = 0; i < fs.Length; i++)
            //     {
            //         var fT = fs[i].FieldType;
            //         var curLclOfs = IS.Marshal.OffsetOf(type, fs[i].Name);
            //         var curStdOfs = lastStdOfs + ((GetStructureLayout(fT).StandardSize - 1) / pack + 1) * pack; 

            //         if (lastLclOfs >= curLclOfs) throw new Exception("取得した欄配列の順序に依って標準摺幅を決定していた所、実際の配置がそれに泝るものがありました。これは開発時における、欄配列は宣言順に並ぶという推定の誤りによるものかもしれません。");

            //         fieldOffsets_.Add(new(fT, curLclOfs, curStdOfs));

            //         lastLclOfs = curLclOfs;
            //         lastStdOfs = curStdOfs;
            //     }
            // }

            if (type.IsAutoClass)
            {
                StructureLayouts.Add(type, structureLayout = StructureLayout.GetTightLayout(type));
            }
            else if (type.IsExplicitLayout)
            {
                StructureLayouts.Add(type, structureLayout = StructureLayout.GetExplicitLayout(type));
            }
            else
            {
                StructureLayouts.Add(type, structureLayout = StructureLayout.GetSequencialLayout(type));
            }
        }

        return structureLayout;
    }

    public readonly record struct FieldOffset(Type Type, nint LocalOffset, nint StandardOffset)
    {
        readonly nint _standardOffset = StandardOffset;

        public nint StandardOffset
        {
            get => _standardOffset > 0 ? _standardOffset : throw new InvalidOperationException("欄は委配置構造体のものであり、標準摺幅を取得できません。");
        }
    }

    public class StructureLayout
    {
        const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        Type _type;
        List<FieldOffset> _fieldOffsets;
        int _standardSize;
        int _localSize;

        public Type Type => _type;
        public IEnumerable<FieldOffset> FieldOffsets => _fieldOffsets;
        public int StandardSize => _standardSize;
        public int LocalSize => _localSize;

        public StructureLayout(Type type, List<FieldOffset> fieldOffsets, int standardSize)
        {
            _type = type;
            _fieldOffsets = fieldOffsets;
            _localSize = IS::Marshal.SizeOf(type);
            _standardSize = standardSize;
        }

        public static StructureLayout GetExplicitLayout(Type type)
        {
            if (!type.IsExplicitLayout) throw new ArgumentException("明示配置構造体型以外が渡されました。", nameof(type));

            var fs = type.GetFields(FLAGS);
            var offsets = new List<FieldOffset>(fs.Length);

            foreach (var f in fs)
            {
                var cmnOffsetA = f.GetCustomAttribute<FieldOffsetAttribute>();
                if (cmnOffsetA is null) throw new Exception("不明な錯誤です。明示配置構造体が欄摺幅属性を持ちませんでした。");
                offsets.Add(new(f.FieldType, cmnOffsetA.Value, cmnOffsetA.Value));
            }

            var layout = type.StructLayoutAttribute ?? throw new Exception("不明な錯誤です。明示配置構造体が構造体配置属性を持ちませんでした。");
            var stdSize = layout.Size;
            Debug.Assert(stdSize == 0);

            return new(type, offsets, stdSize);
        }

        public static StructureLayout GetTightLayout(Type type)
        {
            var fs = type.GetFields(FLAGS);
            var offsets = new List<FieldOffset>(fs.Length);

            int c_stdOffset = 0;
            foreach (var f in fs.OrderBy(x => x.Name))
            {
                var fieldType = f.FieldType;
                var lclOffset = IS::Marshal.OffsetOf(type, f.Name);
                var stdOffset = c_stdOffset;
                offsets.Add(new(fieldType, lclOffset, stdOffset));

                c_stdOffset += fieldType == typeof(nint) ? sizeof(long) : fieldType == typeof(nuint) ? sizeof(ulong) : IS::Marshal.SizeOf(fieldType);
            }

            var stdSize = c_stdOffset;
            if (type.StructLayoutAttribute is {} layout)
            {
                var stdSize_ = layout.Size;
                Debug.Assert(stdSize == 0);
                Debug.Assert(stdSize_ < stdSize);
                stdSize = stdSize_;
            }

            return new(type, offsets, stdSize);
        }

        public static StructureLayout GetSequencialLayout(Type type, int pack = STANDARD_PACK)
        {
            if (!type.IsLayoutSequential) throw new ArgumentException("順序配置構造体型以外が渡されました。", nameof(type));

            var fs = type.GetFields(FLAGS);
            var offsets = new List<FieldOffset>(fs.Length);

            int c_stdOffset = 0;
            foreach (var (f, lclOffset) in fs.Select(x => (f: x, offset: IS::Marshal.OffsetOf(type, x.Name))).OrderBy(x => x.offset))
            {
                var fieldType = f.FieldType;
                var stdOffset = c_stdOffset;
                offsets.Add(new(fieldType, lclOffset, stdOffset));

                var dif_stdOffset = fieldType == typeof(nint) ? sizeof(int) : fieldType == typeof(nuint) ? sizeof(ulong) : IS::Marshal.SizeOf(fieldType);
                dif_stdOffset = ((dif_stdOffset - 1) / pack + 1) * pack;
                c_stdOffset = dif_stdOffset;
            }

            var stdSize = c_stdOffset;
            if (type.StructLayoutAttribute is {} layout)
            {
                var stdSize_ = layout.Size;
                Debug.Assert(stdSize == 0);
                Debug.Assert(stdSize_ < stdSize);
                stdSize = stdSize_;
            }

            return new(type, offsets, stdSize);
        }
    }
}

public class StandardSerializationMarshal : StandardMarshal
{
    /* !:頻繁な出現が想定される組合せ。
     * StructLayout | 64bitLE | 32bitLE | 64bitBE | 32bitBE
     * -------------|---------|---------|---------|--------
     * Sequencial   | Copy   !| OrderSq!| OrderSq!| OrderSq
     * Sq&Primitive | Copy   !| Copy   !| Stdize !| Stdize 
     * Sq&Pointer   | Copy    | CopyP64 | Stdize  | Stdize
     * Sq&Pack8     | Copy    | Copy    | OrderSq | OrderSq
     * Explicit     | Copy    | Copy    | OrderEx | OrderEx
     * Auto         | OrderTg | OrderTg | OrderTg | OrderTg
     */

    public StandardSerializationMarshal(Dictionary<Type, StructureLayout>? structureLayouts = null) : base(structureLayouts: structureLayouts)
    {
        SourceOrder = RUNTIME_STRUCTURE_ORDER;
        ResultOrder = STANDARD_ORDER;
    }

    protected unsafe override void Conduct(Type type, ref void* source, ref void* to)
    {
        if (Endian.HostByteOrder.IsLittleEndian)
        {
            if (type.IsAutoLayout)
            {
                Order(type, ref source, ref to);
            }
            else if (sizeof(nint) == sizeof(long))
            {
                Copy(type, ref source, ref to);
            }
            else if (type.IsExplicitLayout)
            {
                Copy(type, ref source, ref to);
            }
            else if (type.StructLayoutAttribute is {} layout && layout.Pack == STANDARD_PACK)
            {
                Copy(type, ref source, ref to);
            }
            else if (type == typeof(nint))
            {
                CopyIntPtrAsInt64(ref source, ref to);
            }
            else if (type == typeof(nuint))
            {
                CopyUIntPtrAsUInt64(ref source, ref to);
            }
            else
            {
                Copy(type, ref source, ref to);
            }
        }
        else
        {
            if (type.IsPrimitive)
            {
                Standardize(type, ref source, ref to);
            }
            else
            {
                Order(type, ref source, ref to);
            }
        }
    }

    protected unsafe void Copy(Type type, ref void* source, ref void* to)
    {
        var s = (byte*)source;
        var r = (byte*)to;
        var end = s + IS::Marshal.SizeOf(type);
        while (s < end)
        {
            *r = *s;
            s++;
            r++;
        }
        source = s;
        to = r;
    }

    protected unsafe void CopyUIntPtrAsUInt64(ref void* source, ref void* to)
    {
        var s = (nuint*)source;
        var r = (ulong*)to;

        *r = *s;

        source = s + 1;
        to = r + 1;
    }

    protected unsafe void CopyIntPtrAsInt64(ref void* source, ref void* to)
    {
        var s = (nint*)source;
        var r = (long*)to;

        *r = *s;

        source = s + 1;
        to = r + 1;
    }

    protected unsafe void Standardize(Type type, ref void* source, ref void* to)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean:
            case TypeCode.Byte:
            case TypeCode.SByte:
            {
                var s = (byte*)source;
                var r = (byte*)to;

                *r = *s;

                source = s + 1;
                to = r + 1;
                return;
            }
            case TypeCode.Object:
            {
                if (type == typeof(IntPtr))
                {
                    var s = (nint*)source;
                    var r = (long*)to;
                    
                    long v = *s;
                    Endian.HostByteOrder.Standardize(&v, to);

                    source = s + 1;
                    to = r + 1;
                    return;
                }
                if (type == typeof(UIntPtr))
                {
                    var s = (nuint*)source;
                    var r = (ulong*)to;
                    
                    ulong v = *s;
                    Endian.HostByteOrder.Standardize(&v, to);

                    source = s + 1;
                    to = r + 1;
                    return;
                }
                goto default;
            }
            default: throw new ArgumentException("原子型以外の型が指定されました。");
            /*
                $$"""
                case TypeCode.{{t}}:
                {
                    Endian.HostByteOrder.Standardize(({{t}}*)source, to);
                    source = ({{t}}*)source + 1;
                    to = ({{t}}*)to + 1;
                    return;
                }
                """
            */
            case TypeCode.Char:
            {
                Endian.HostByteOrder.Standardize((Char*)source, to);
                source = (Char*)source + 1;
                to = (Char*)to + 1;
                return;
            }
            case TypeCode.UInt16:
            {
                Endian.HostByteOrder.Standardize((UInt16*)source, to);
                source = (UInt16*)source + 1;
                to = (UInt16*)to + 1;
                return;
            }
            case TypeCode.Int16:
            {
                Endian.HostByteOrder.Standardize((Int16*)source, to);
                source = (Int16*)source + 1;
                to = (Int16*)to + 1;
                return;
            }
            case TypeCode.UInt32:
            {
                Endian.HostByteOrder.Standardize((UInt32*)source, to);
                source = (UInt32*)source + 1;
                to = (UInt32*)to + 1;
                return;
            }
            case TypeCode.Int32:
            {
                Endian.HostByteOrder.Standardize((Int32*)source, to);
                source = (Int32*)source + 1;
                to = (Int32*)to + 1;
                return;
            }
            case TypeCode.UInt64:
            {
                Endian.HostByteOrder.Standardize((UInt64*)source, to);
                source = (UInt64*)source + 1;
                to = (UInt64*)to + 1;
                return;
            }
            case TypeCode.Int64:
            {
                Endian.HostByteOrder.Standardize((Int64*)source, to);
                source = (Int64*)source + 1;
                to = (Int64*)to + 1;
                return;
            }
            case TypeCode.Single:
            {
                Endian.HostByteOrder.Standardize((Single*)source, to);
                source = (Single*)source + 1;
                to = (Single*)to + 1;
                return;
            }
            case TypeCode.Double:
            {
                Endian.HostByteOrder.Standardize((Double*)source, to);
                source = (Double*)source + 1;
                to = (Double*)to + 1;
                return;
            }
        }
    }

    protected unsafe void Order(Type type, ref void* source, ref void* to)
    {
        var s = (byte*)source;
        var t = (byte*)to;

        var layout = GetStructureLayout(type);
        foreach (var (fT, lclOffset, stdOffset) in layout.FieldOffsets)
        {
            void* s_ = s + lclOffset;
            void* t_ = t + stdOffset;
            Conduct(fT, ref s_, ref t_);
        }
        
        source = s + layout.LocalSize;
        to = t + layout.StandardSize;
    }
}

public unsafe class LittleStandardSerializationMarshal : StandardSerializationMarshal
{
    public LittleStandardSerializationMarshal(Dictionary<Type, StructureLayout>? structureLayouts = null) : base(structureLayouts: structureLayouts)
    {
        if (!Endian.HostByteOrder.IsLittleEndian) throw new InvalidOperationException($"リトルエンディアンでない環境で`{nameof(LittleStandardSerializationMarshal)}`の実体を作成することができません。");
    }

    protected unsafe override void Conduct(Type type, ref void* source, ref void* to)
    {
        if (type.IsAutoLayout)
        {
            Order(type, ref source, ref to);
        }
        else if (sizeof(nint) == sizeof(long))
        {
            Copy(type, ref source, ref to);
        }
        else if (type.IsExplicitLayout)
        {
            Copy(type, ref source, ref to);
        }
        else if (type.StructLayoutAttribute is {} layout && layout.Pack == STANDARD_PACK)
        {
            Copy(type, ref source, ref to);
        }
        else if (type == typeof(nint))
        {
            CopyIntPtrAsInt64(ref source, ref to);
        }
        else if (type == typeof(nuint))
        {
            CopyUIntPtrAsUInt64(ref source, ref to);
        }
        else
        {
            Copy(type, ref source, ref to);
        }
    }
}

public unsafe class StandardDeserializationMarshal : StandardMarshal
{
    /* !:頻繁な出現が想定される組合せ。
     * StructLayout | 64bitLE | 32bitLE | 64bitBE | 32bitBE
     * -------------|---------|---------|---------|--------
     * Sequencial   | Copy   !| OrderSq!| OrderSq!| OrderSq
     * Sq&Primitive | Copy   !| Copy   !| Lclize !| Lclize 
     * Sq&Pointer   | Copy    | Cast    | Lclize  | Lclize
     * Sq&Pack8     | Copy    | Copy    | OrderSq | OrderSq
     * Explicit     | Copy    | Copy    | OrderEx | OrderEx
     * Auto         | OrderTg | OrderTg | OrderTg | OrderTg
     */

    public StandardDeserializationMarshal(Dictionary<Type, StructureLayout>? structureLayouts = null) : base(structureLayouts: structureLayouts)
    {
        SourceOrder = RUNTIME_STRUCTURE_ORDER;
        ResultOrder = STANDARD_ORDER;
    }

    protected unsafe override void Conduct(Type type, ref void* source, ref void* to)
    {
        if (Endian.HostByteOrder.IsLittleEndian)
        {
            if (type.IsAutoLayout)
            {
                Order(type, ref source, ref to);
            }
            else if (sizeof(nint) == sizeof(long))
            {
                Copy(type, ref source, ref to);
            }
            else if (type.IsExplicitLayout)
            {
                Copy(type, ref source, ref to);
            }
            else if (type.StructLayoutAttribute is {} layout && layout.Pack == STANDARD_PACK)
            {
                Copy(type, ref source, ref to);
            }
            else if (type == typeof(nint))
            {
                CastToIntPtr(ref source, ref to);
            }
            else if (type == typeof(nuint))
            {
                CastToUIntPtr(ref source, ref to);
            }
            else
            {
                Copy(type, ref source, ref to);
            }
        }
        else
        {
            if (type.IsPrimitive)
            {
                Localize(type, ref source, ref to);
            }
            else
            {
                Order(type, ref source, ref to);
            }
        }
    }

    protected unsafe void Copy(Type type, ref void* source, ref void* to)
    {
        var s = (byte*)source;
        var r = (byte*)to;
        var end = s + IS::Marshal.SizeOf(type);
        while (s < end)
        {
            *r = *s;
            s++;
            r++;
        }
        source = s;
        to = r;
    }

    protected unsafe void CastToUIntPtr(ref void* source, ref void* to)
    {
        var s = (ulong*)source;
        var r = (nuint*)to;

        *r = checked((nuint)(*s));

        source = s + 1;
        to = r + 1;
    }

    protected unsafe void CastToIntPtr(ref void* source, ref void* to)
    {
        var s = (long*)source;
        var r = (nint*)to;

        *r = checked((nint)(*s));

        source = s + 1;
        to = r + 1;
    }

    protected unsafe void Localize(Type type, ref void* source, ref void* to)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean:
            case TypeCode.Byte:
            case TypeCode.SByte:
            {
                var s = (byte*)source;
                var r = (byte*)to;

                *r = *s;

                source = s + 1;
                to = r + 1;
                return;
            }
            case TypeCode.Object:
            {
                if (type == typeof(IntPtr))
                {
                    var s = (long*)source;
                    var r = (nint*)to;
                    
                    long v = 0;
                    Endian.HostByteOrder.Localize(s, &v);
                    *r = checked((nint)v);

                    source = s + 1;
                    to = r + 1;
                    return;
                }
                if (type == typeof(UIntPtr))
                {
                    var s = (ulong*)source;
                    var r = (nuint*)to;
                    
                    ulong v = 0;
                    Endian.HostByteOrder.Standardize(s, &v);
                    *r = checked((nuint)v);

                    source = s + 1;
                    to = r + 1;
                    return;
                }
                goto default;
            }
            default: throw new ArgumentException("原子型以外の型が指定されました。");
            /*
                $$"""
                case TypeCode.{{t}}:
                {
                    Endian.HostByteOrder.Localize(source, ({{t}}*)to);
                    source = ({{t}}*)source + 1;
                    to = ({{t}}*)to + 1;
                    return;
                }
                """
            */
            case TypeCode.Char:
            {
                Endian.HostByteOrder.Localize(source, (Char*)to);
                source = (Char*)source + 1;
                to = (Char*)to + 1;
                return;
            }
            case TypeCode.UInt16:
            {
                Endian.HostByteOrder.Localize(source, (UInt16*)to);
                source = (UInt16*)source + 1;
                to = (UInt16*)to + 1;
                return;
            }
            case TypeCode.Int16:
            {
                Endian.HostByteOrder.Localize(source, (Int16*)to);
                source = (Int16*)source + 1;
                to = (Int16*)to + 1;
                return;
            }
            case TypeCode.UInt32:
            {
                Endian.HostByteOrder.Localize(source, (UInt32*)to);
                source = (UInt32*)source + 1;
                to = (UInt32*)to + 1;
                return;
            }
            case TypeCode.Int32:
            {
                Endian.HostByteOrder.Localize(source, (Int32*)to);
                source = (Int32*)source + 1;
                to = (Int32*)to + 1;
                return;
            }
            case TypeCode.UInt64:
            {
                Endian.HostByteOrder.Localize(source, (UInt64*)to);
                source = (UInt64*)source + 1;
                to = (UInt64*)to + 1;
                return;
            }
            case TypeCode.Int64:
            {
                Endian.HostByteOrder.Localize(source, (Int64*)to);
                source = (Int64*)source + 1;
                to = (Int64*)to + 1;
                return;
            }
            case TypeCode.Single:
            {
                Endian.HostByteOrder.Localize(source, (Single*)to);
                source = (Single*)source + 1;
                to = (Single*)to + 1;
                return;
            }
            case TypeCode.Double:
            {
                Endian.HostByteOrder.Localize(source, (Double*)to);
                source = (Double*)source + 1;
                to = (Double*)to + 1;
                return;
            }
        }
    }

    protected unsafe void Order(Type type, ref void* source, ref void* to)
    {
        var s = (byte*)source;
        var t = (byte*)to;

        var layout = GetStructureLayout(type);
        foreach (var (fT, lclOffset, stdOffset) in layout.FieldOffsets)
        {
            void* s_ = s + stdOffset;
            void* t_ = t + lclOffset;
            Conduct(fT, ref s_, ref t_);
        }
        
        source = s + layout.StandardSize;
        to = t + layout.LocalSize;
    }
}

public unsafe class LittleStandardDeserializationMarshal : StandardDeserializationMarshal
{
    public LittleStandardDeserializationMarshal(Dictionary<Type, StructureLayout>? structureLayouts = null) : base(structureLayouts: structureLayouts)
    {
        if (!Endian.HostByteOrder.IsLittleEndian) throw new InvalidOperationException($"リトルエンディアンでない環境で`{nameof(StandardDeserializationMarshal)}`の実体を作成することができません。");
    }
}

public abstract unsafe class TightSerializationMarshal : StandardSerializationMarshal
{
    public const string TIGHT_ORDER = "Tight";

    public TightSerializationMarshal(Dictionary<Type, StructureLayout>? structureLayouts = null) : base(structureLayouts)
    {
        SourceOrder = RUNTIME_STRUCTURE_ORDER;
        ResultOrder = TIGHT_ORDER;
    }

    protected override unsafe void Conduct(Type type, ref void* source, ref void* to)
    {
        if (Endian.HostByteOrder.IsLittleEndian)
        {
            if (type.IsPrimitive)
            {
                if (sizeof(nint) == sizeof(long))
                {
                    Copy(type, ref source, ref to);
                }             
                else if (type == typeof(nint))
                {
                    CopyIntPtrAsInt64(ref source, ref to);
                }
                else if (type == typeof(nuint))
                {
                    CopyUIntPtrAsUInt64(ref source, ref to);
                }
                else
                {
                    Copy(type, ref source, ref to);
                }
            }
            else
            {
                Order(type, ref source, ref to);
            }
        }
        else
        {
            if (type.IsPrimitive)
            {
                Standardize(type, ref source, ref to);
            }
            else
            {
                Order(type, ref source, ref to);
            }
        }
    }

    protected override StructureLayout GetStructureLayout(Type type)
    {
        if (!StructureLayouts.TryGetValue(type, out var r))
        {
            StructureLayouts.Add(type, r = StructureLayout.GetTightLayout(type));
        }

        return r;
    }
}
