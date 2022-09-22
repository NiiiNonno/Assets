// 令和弐年大暑確認済。
using System.Collections;
using System.Dynamic;
using System.Xml.Linq;

namespace Nonno.Assets;

public class DynamicXElement : DynamicObject
{
    readonly XElement _element;

    public XElement Element => _element;

    public DynamicXElement(XDocument document) : this(document.Root ?? throw new ArgumentException("引数の'Root'プロパティが'null'を返しました。", nameof(document))) { }
    public DynamicXElement(XElement element) { _element = element; }
    public DynamicXElement(string uri) : this(XDocument.Load(uri)) { }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        result = _element.Elements(binder.Name).Select(x => new DynamicXElement(x));
        return true;
    }

    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result)
    {
        if (indexes.Length == 1 && indexes[0] is string key)
        {
            switch (Type.GetTypeCode(binder.ReturnType))
            {
            case TypeCode.Boolean:
                result = Int64.TryParse(_element.Attribute(key)?.Value, out var boolean) ? boolean : (object?)null;
                return true;
            case TypeCode.Char:
                result = Int64.TryParse(_element.Attribute(key)?.Value, out var @char) ? @char : (object?)null;
                return true;
            case TypeCode.SByte:
                result = Int64.TryParse(_element.Attribute(key)?.Value, out var sByte) ? sByte : (object?)null;
                return true;
            case TypeCode.Byte:
                result = Int64.TryParse(_element.Attribute(key)?.Value, out var @byte) ? @byte : (object?)null;
                return true;
            case TypeCode.Int16:
                result = Int64.TryParse(_element.Attribute(key)?.Value, out var int16) ? int16 : (object?)null;
                return true;
            case TypeCode.UInt16:
                result = Int64.TryParse(_element.Attribute(key)?.Value, out var uint16) ? uint16 : (object?)null;
                return true;
            case TypeCode.Int32:
                result = Int64.TryParse(_element.Attribute(key)?.Value, out var int32) ? int32 : (object?)null;
                return true;
            case TypeCode.UInt32:
                result = Int64.TryParse(_element.Attribute(key)?.Value, out var uint32) ? uint32 : (object?)null;
                return true;
            case TypeCode.Int64:
                result = Int64.TryParse(_element.Attribute(key)?.Value, out var int64) ? int64 : (object?)null;
                return true;
            case TypeCode.UInt64:
                result = UInt64.TryParse(_element.Attribute(key)?.Value, out var uint64) ? uint64 : (object?)null;
                return true;
            case TypeCode.Single:
                result = Single.TryParse(_element.Attribute(key)?.Value, out var single) ? single : (object?)null;
                return true;
            case TypeCode.Double:
                result = Double.TryParse(_element.Attribute(key)?.Value, out var @double) ? @double : (object?)null;
                return true;
            case TypeCode.Decimal:
                result = Decimal.TryParse(_element.Attribute(key)?.Value, out var @decimal) ? @decimal : (object?)null;
                return true;
            case TypeCode.DateTime:
                result = DateTime.TryParse(_element.Attribute(key)?.Value, out var dateTime) ? dateTime : (object?)null;
                return true;
            case TypeCode.String:
                result = _element.Attribute(key)?.Value;
                return true;
            default:
                result = _element.Attribute(key)?.Value;
                return true;
            }
        }
        else
        {
            return base.TryGetIndex(binder, indexes, out result);
        }
    }

    public override bool TryConvert(ConvertBinder binder, out object? result)
    {
        switch (Type.GetTypeCode(binder.Type))
        {
        case TypeCode.Boolean:
            if (Int64.TryParse(_element.Value, out var boolean))
            {
                result = boolean;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        case TypeCode.Char:
            if (Int64.TryParse(_element.Value, out var @char))
            {
                result = @char;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        case TypeCode.SByte:
            if (Int64.TryParse(_element.Value, out var sByte))
            {
                result = sByte;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        case TypeCode.Byte:
            if (Int64.TryParse(_element.Value, out var @byte))
            {
                result = @byte;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        case TypeCode.Int16:
            if (Int64.TryParse(_element.Value, out var int16))
            {
                result = int16;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        case TypeCode.UInt16:
            if (Int64.TryParse(_element.Value, out var uint16))
            {
                result = uint16;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        case TypeCode.Int32:
            if (Int64.TryParse(_element.Value, out var int32))
            {
                result = int32;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        case TypeCode.UInt32:
            if (Int64.TryParse(_element.Value, out var uint32))
            {
                result = uint32;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        case TypeCode.Int64:
            if (Int64.TryParse(_element.Value, out var int64))
            {
                result = int64;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        case TypeCode.UInt64:
            if (UInt64.TryParse(_element.Value, out var uint64))
            {
                result = uint64;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        case TypeCode.Single:
            if (Single.TryParse(_element.Value, out var single))
            {
                result = single;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        case TypeCode.Double:
            if (Double.TryParse(_element.Value, out var @double))
            {
                result = @double;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        case TypeCode.Decimal:
            if (Decimal.TryParse(_element.Value, out var @decimal))
            {
                result = @decimal;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        case TypeCode.DateTime:
            if (DateTime.TryParse(_element.Value, out var dateTime))
            {
                result = dateTime;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        case TypeCode.String:
            {
                result = _element.Value;
                return true;
            }
        }

        if (binder.Type == typeof(IEnumerable))
        {
            result = new[] { this };
            return true;
        }

        if (binder.Type == typeof(XElement))
        {
            result = _element;
            return true;
        }

        result = null;
        return false;
    }

    public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
    {
        switch (binder.Name)
        {
        case "GetEnumerator":
            {
                result = new[] { this }.GetEnumerator();

                return true;
            }
        case "GetName":
            {
                result = _element.Name.ToString();

                return true;
            }
        default:
            {
                return base.TryInvokeMember(binder, args, out result);
            }
        }
    }

    public override string ToString()
    {
        return _element.Value;
    }
}
