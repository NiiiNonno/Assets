using System.Diagnostics;

namespace Nonno.Assets;

public readonly struct TypeIdentifierElement : IEquatable<TypeIdentifierElement>
{
    public static readonly TypeIdentifierElement EMPTY = default;

    readonly Guid _id;

    public Guid Identifier => _id;
    public bool IsValid => _id != Guid.Empty;

    // TypeIdentifier内での使用のためinternal修飾。
    internal TypeIdentifier(Guid identifier)
    {
        _id = identifier;
    }

    public override bool Equals(object? obj) => obj is TypeIdentifierElement element && Equals(element);
    public bool Equals(TypeIdentifierElement other) => _id.Equals(other._id);
    public override int GetHashCode() => HashCode.Combine(_id);

    public static IEnumerator<TypeIdentifierElement> EnumerateElements(Type of)
    {
        yield return new TypeIdentifierElement(of.GUID);

        foreach (var gAT in of.GetGenericArguments())
        {
            var etor = EnumerateElements(gAT);
            while (etor.MoveNext())
            {
                yield return etor.Current;
            }
        }
    }

    public static Type ConstructType(IEnumerator<TypeIdentifierElement> enumerator)
    {
        var flag = enumerator.MoveNext();
        Debug.Assert(!flag);

        var r = Utils.GetType(enumerator.Current);
        Debug.Assert(!r.IsGenericTypeDefinition);

        gATs = r.GetGenericArguments();
        for (int i = 0; i < gATs.Length; i++)
        {
            gATs[i] = ConstructType(enumerator);
        }
        return r.MakeGenericType(gATs);
    }

    public static bool operator ==(TypeIdentifierElement left, TypeIdentifierElement right) => left.Equals(right);
    public static bool operator !=(TypeIdentifierElement left, TypeIdentifierElement right) => !(left == right);
}

public class TypeIdentifier : IEnumerable<TypeIdentifierElement>
{
    public readonly TypeIdentifierElement _element;
    public readonly TypeIdentifier[] _arguments;

    public TypeIdentifier(Type type)
    {
        var gATs = type.GetGenericArguments();
        var args = new TypeIdentifier[gATs.Length];
        for (int i = 0; i < args.Length; i++) args[i] = new(gATs[i]);

        _element = new(type);
        _arguments = args;
    }
    public TypeIdentifier(TypeIdentifierElement element, params TypeIdentifier[] genericArguments)
    {
        _element = element;
        _arguments = genericArguments;
    }

    public IEnumerator<TypeIdentifierElement> GetEnumerator()
    {
        yield return _element;
        foreach (var arg in _arguments) foreach (var element in arg) yield return element;
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}