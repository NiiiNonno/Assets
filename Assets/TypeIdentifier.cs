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