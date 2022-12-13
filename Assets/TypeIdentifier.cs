using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;

namespace Nonno.Assets;
public readonly struct TypeIdentifier
{
    readonly Guid _identifier;

    public Guid Identifier => _identifier;

    public TypeIdentifier(Guid identifier)
    {
        _identifier = identifier;
    }

    public static IEnumerator<TypeIdentifier> Of(Type? type)
    {
        if (type is null)
        {
            yield return default;
        }
        else if (type.IsGenericType)
        {
            yield return new(type.GUID);
            foreach (var gTA in type.GenericTypeArguments)
            {
                var etor = Of(gTA);
                while(etor.MoveNext()) yield return etor.Current;
            }
        }
        else
        {
            yield return new(type.GUID);
        }
    }

    public static Type? GetType(IEnumerator<TypeIdentifier> etor)
    {
        if (!etor.MoveNext()) throw new ArgumentException("ó^Ç¶ÇÁÇÍÇΩïÑí∑Ç™ë´ÇËÇ‹ÇπÇÒÅB", nameof(etor));

        if (etor.Current._identifier == default) return null;

        if (!_typeDictionaly.TryGetValue(etor.Current._identifier, out var c))
        {
            if (!_typeAsmNames.TryGetValue(etor.Current._identifier, out var aN))
            {
                throw new KeyNotFoundException(nameof(etor));
            }

            Assembly.Load(aN);
            c = _typeDictionaly[etor.Current._identifier];
        }
        
        if (c.IsGenericTypeDefinition)
        {
            var type = c.GetGenericArguments();
            for (int i = 0; i < type.Length; i++)
            {
                type[i] = GetType(etor) ?? throw new Exception();
            }
            return c.MakeGenericType(type);
        }
        else
        {
            return c;
        }
    }

    static readonly Dictionary<Guid, Type> _typeDictionaly = new();
    static readonly Dictionary<Guid, AssemblyName> _typeAsmNames = new();

    public IEnumerable<KeyValuePair<Guid, AssemblyName>> TypeAssemblyNames => _typeAsmNames;

    static TypeIdentifier()
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) Add(asm);
        AppDomain.CurrentDomain.AssemblyLoad += (sender, e) => Add(e.LoadedAssembly);

        void Add(Assembly assembly)
        {
            foreach (var type in assembly.DefinedTypes)
            {
                _typeDictionaly.Add(type.GUID, type);
            }
        }
    }

    public static void Register(Guid guid, AssemblyName assemblyName)
    {
        _typeAsmNames.Add(guid, assemblyName);
    }
    public static void Unregister(Guid guid)
    {
        _ = _typeAsmNames.Remove(guid);
    }
}

// public readonly struct TypeIdentifierElement : IEquatable<TypeIdentifierElement>
// {
//     public static readonly TypeIdentifierElement EMPTY = default;

//     readonly Guid _id;

//     public Guid Identifier => _id;
//     public bool IsValid => _id != Guid.Empty;

//     // TypeIdentifierÂÜ?„Åß„ÅÆ‰ΩøÁî®„ÅÆ„Åü„ÇÅinternal‰øÆÈ£æ„Ä?
//     internal TypeIdentifierElement(Guid identifier)
//     {
//         _id = identifier;
//     }

//     public override bool Equals(object? obj) => obj is TypeIdentifierElement element && Equals(element);
//     public bool Equals(TypeIdentifierElement other) => _id.Equals(other._id);
//     public override int GetHashCode() => HashCode.Combine(_id);

//     public static IEnumerator<TypeIdentifierElement> EnumerateElements(Type of)
//     {
//         yield return new TypeIdentifierElement(of.GUID);

//         foreach (var gAT in of.GetGenericArguments())
//         {
//             var etor = EnumerateElements(gAT);
//             while (etor.MoveNext())
//             {
//                 yield return etor.Current;
//             }
//         }
//     }

//     public static Type ConstructType(IEnumerator<TypeIdentifierElement> enumerator)
//     {
//         var flag = enumerator.MoveNext();
//         Debug.Assert(!flag);

//         var r = Utils.GetType(enumerator.Current);
//         Debug.Assert(!r.IsGenericTypeDefinition);

//         gATs = r.GetGenericArguments();
//         for (int i = 0; i < gATs.Length; i++)
//         {
//             gATs[i] = ConstructType(enumerator);
//         }
//         return r.MakeGenericType(gATs);
//     }

//     public static bool operator ==(TypeIdentifierElement left, TypeIdentifierElement right) => left.Equals(right);
//     public static bool operator !=(TypeIdentifierElement left, TypeIdentifierElement right) => !(left == right);
// }

// public class TypeIdentifier : IEnumerable<TypeIdentifierElement>
// {
//     public readonly TypeIdentifierElement _element;
//     public readonly TypeIdentifier[] _arguments;

//     public TypeIdentifier(Type type)
//     {
//         var gATs = type.GetGenericArguments();
//         var args = new TypeIdentifier[gATs.Length];
//         for (int i = 0; i < args.Length; i++) args[i] = new(gATs[i]);

//         _element = new(type);
//         _arguments = args;
//     }
//     public TypeIdentifier(TypeIdentifierElement element, params TypeIdentifier[] genericArguments)
//     {
//         _element = element;
//         _arguments = genericArguments;
//     }

//     public IEnumerator<TypeIdentifierElement> GetEnumerator()
//     {
//         yield return _element;
//         foreach (var arg in _arguments) foreach (var element in arg) yield return element;
//     }
//     IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
// }