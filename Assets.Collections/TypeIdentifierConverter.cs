using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Nonno.Assets.Collections;

namespace Nonno.Assets.Collections;
public class TypeIdentifierConverter : IConverter<Type, UniqueIdentifier<Type>>
{
    public readonly static TypeIdentifierConverter INSTANCE;

    readonly TableConverter<Type, UniqueIdentifier<Type>> _converter;
    readonly string _filePath;
    bool _allowAutoCreate;

    public bool AllowAutoCreate
    {
        get => _allowAutoCreate;
        set
        {
            _allowAutoCreate = value;
        }
    }
    public UniqueIdentifier<Type> this[Type type] { get => GetForward(type); set=> SetForward(type, value); }
    public Type this[UniqueIdentifier<Type> identifier] { get => GetBackward(identifier); set => SetBackward(identifier, value); }

    public ICollection<Type> Keys => ((IConverter<Type, UniqueIdentifier<Type>>)_converter).Keys;

    public ICollection<UniqueIdentifier<Type>> Values => ((IConverter<Type, UniqueIdentifier<Type>>)_converter).Values;

    public TypeIdentifierConverter()
    {
        _converter = new();
        _filePath = Assembly.GetExecutingAssembly().Location + ".tid";
    }

    public UniqueIdentifier<Type> GetForward(Type type)
    {
        if (!TryGetForward(type, out var r))
        {
            r = UniqueIdentifier<Type>.GetNew();

            using (var writer = new StreamWriter(File.OpenRead(_filePath), Encoding.UTF8))
            {
                writer.Write($"{r}");
            }
        }

        return r;
    }
    public Type GetBackward(UniqueIdentifier<Type> key) => _converter.GetBackward(key);
    public bool TryGetForward(Type key, [MaybeNullWhen(false)] out UniqueIdentifier<Type> value) => _converter.TryGetForward(key, out value);
    public bool TryGetBackward(UniqueIdentifier<Type> key, [MaybeNullWhen(false)] out Type value) => _converter.TryGetBackward(key, out value);
    public void SetForward(Type key, UniqueIdentifier<Type> value) => _converter.SetForward(key, value);
    public void SetBackward(UniqueIdentifier<Type> key, Type value) => _converter.SetBackward(key, value);
    public bool TrySetForward(Type key, UniqueIdentifier<Type> value) => _converter.TrySetForward(key, value);
    public bool TrySetBackward(UniqueIdentifier<Type> key, Type value) => _converter.TrySetBackward(key, value);
    public UniqueIdentifier<Type> MoveForward(Type key, UniqueIdentifier<Type> value) => _converter.MoveForward(key, value);
    public Type MoveBackward(UniqueIdentifier<Type> key, Type value) => _converter.MoveBackward(key, value);
    public bool TryMoveForward(Type key, UniqueIdentifier<Type> neo, [MaybeNullWhen(false)] out UniqueIdentifier<Type> old) => _converter.TryMoveForward(key, neo, out old);
    public bool TryMoveBackward(UniqueIdentifier<Type> key, Type neo, [MaybeNullWhen(false)] out Type old) => _converter.TryMoveBackward(key, neo, out old);
    public bool ContainsForward(Type item) => _converter.ContainsForward(item);
    public bool ContainsBackward(UniqueIdentifier<Type> item) => _converter.ContainsBackward(item);
    public void Add(KeyValuePair<Type, UniqueIdentifier<Type>> item) => _converter.Add(item);
    public void Remove(KeyValuePair<Type, UniqueIdentifier<Type>> item) => _converter.Remove(item);
    public bool TryAdd(KeyValuePair<Type, UniqueIdentifier<Type>> item) => _converter.TryAdd(item);
    public bool TryRemove(KeyValuePair<Type, UniqueIdentifier<Type>> item) => _converter.TryRemove(item);
    public void Add(Type t1, UniqueIdentifier<Type> t2) => _converter.Add(t1, t2);
    public bool TryAdd(Type t1, UniqueIdentifier<Type> t2) => _converter.TryAdd(t1, t2);
    public UniqueIdentifier<Type> Remove(Type t1) => _converter.Remove(t1);
    public bool TryRemove(Type t1, [MaybeNullWhen(false)] out UniqueIdentifier<Type> t2) => _converter.TryRemove(t1, out t2);
    public IConverter<UniqueIdentifier<Type>, Type> Reverse() => _converter.Reverse();
    public IEnumerator<KeyValuePair<Type, UniqueIdentifier<Type>>> GetEnumerator() => ((IEnumerable<KeyValuePair<Type, UniqueIdentifier<Type>>>)_converter).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_converter).GetEnumerator();

    static TypeIdentifierConverter()
    {
        INSTANCE = new();

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var att in asm.GetCustomAttributes<TypeIdentifierAttribute>())
            {
                INSTANCE.Add(att.ToKeyValuePair());
            }
        }

        AppDomain.CurrentDomain.AssemblyLoad += (_, e) =>
        {
            foreach (var att in e.LoadedAssembly.GetCustomAttributes<TypeIdentifierAttribute>())
            {
                INSTANCE.Add(att.ToKeyValuePair());
            }
        };
    }
}
