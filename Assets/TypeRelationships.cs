using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Nonno.Assets;
public class TypeRelationships
{
    readonly ReaderWriterLockSlim _types_lock = new();
    readonly HashSet<Assembly> _collecteds = new();
    readonly List<TypeInfo> _types = new();
    readonly Dictionary<Guid, Type> _dict_guid = new();

    public void Collect(Assembly assembly)
    {
        if (!_collecteds.Add(assembly)) return;

        // この操作でアセンブリが新たに荷され、輪廻。要配列化。
        var definedTypes = assembly.DefinedTypes.ToArray();

        try
        {
            _types_lock.EnterWriteLock();

            foreach (var typeInfo in definedTypes)
            {
                _types.Add(typeInfo);
            }
        }
        finally
        {
            if (_types_lock.IsWriteLockHeld)
                _types_lock.ExitWriteLock();
        }
    }
    public void Discard(Assembly assembly)
    {
        if (!_collecteds.Remove(assembly)) return;

        _types.RemoveAll(x => x.Assembly == assembly);
    }

    /// <summary>
    /// 派生する型を列挙します。
    /// </summary>
    /// <param name="baseType">
    /// </param>
    /// <returns>
    /// 派生する型の列。
    /// </returns>
    public IEnumerable<Type> GetInheritedTypes(Type baseType)
    {
        try
        {
            _types_lock.EnterReadLock();
            return _types.Where(type => type.IsSubclassOf(baseType)).ToArray();
        }
        finally
        {
            _types_lock.ExitReadLock();
        }
    }

    /// <summary>
    /// 実装する型を列挙します。
    /// </summary>
    /// <param name="typeAssignableTo"></param>
    /// <returns></returns>
    public IEnumerable<Type> GetAssignableTypes(Type typeAssignableTo)
    {
        try
        {
            _types_lock.EnterReadLock();
            return _types.Where(type => type.IsAssignableTo(type));
        }
        finally
        {
            _types_lock.ExitReadLock();
        }
    }

    public Type GetType(Guid guid)
    {
        if (!_dict_guid.TryGetValue(guid, out var r))
        {
            r = _types.Find(x => x.GUID == guid);
            if (r is null) throw new KeyNotFoundException();
            _dict_guid.Add(guid, r);
        }

        return r;
    }

    static TypeRelationships? _whole;
    public static TypeRelationships Whole
    {
        get
        {
            if (_whole is null)
            {
                _whole = new();
                AppDomain.CurrentDomain.AssemblyLoad += (_, e) => _whole.Collect(e.LoadedAssembly);
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) _whole.Collect(asm);
            }
            return _whole;
        }
    }
}
