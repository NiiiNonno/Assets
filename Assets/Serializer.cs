//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Threading.Tasks;

//namespace Nonno.Assets;

///// <summary>
///// シリアル化メソッドを表します。
///// <para>
///// <see cref="SerializationMethods"/>の派生クラスはモジュール初期化時に読み込まれ、<see cref="GetSerializationMethods(Type)"/>によって取得することが可能となります。これを用いて<see cref="NoteExtensions.Insert(Nonno.Assets.INote, in object?)"/>と<see cref="NoteExtensions.Remove(Nonno.Assets.INote, out object?)"/>によって順逆シリアル化を行えます。
///// </para>
///// <para>
///// この型から派生する型は惣て引数の無い公開コンストラクタを定義する必要があります。
///// </para>
///// </summary>
//public abstract class SerializationMethods
//{
//    public abstract Type TargetType { get; }

//    public abstract Task Insert(INote to, in object instance);

//    public abstract Task Remove(INote from, out object instance);

//    static readonly Dictionary<Type, SerializationMethods> _methods = new();
//    static readonly Dictionary<Type, List<Type>> _serializationMethodsTypes = new();
//    static readonly AssemblyName ASSEMBLY_NAME = Assembly.GetExecutingAssembly().GetName();

//    static SerializationMethods()
//    {
//        var @lock = new object();
//        lock (@lock)
//        {
//            AppDomain.CurrentDomain.AssemblyLoad += (_, e) =>
//            {
//                lock (@lock)
//                {
//                    OnLoad(e.LoadedAssembly);
//                }
//            };
//            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
//            {
//                OnLoad(assembly);
//            }
//        }
//    }

//    public static SerializationMethods GetSerializationMethods(Type type)
//    {
//        if (_methods.TryGetValue(type, out var r)) return r;

//        var key = type;
//        if (key.IsByRef) key = key.GetElementType(true);
//        if (key.IsGenericType) key = key.GetGenericTypeDefinition();

//        if (!_serializationMethodsTypes.TryGetValue(key, out var methodsTypeCandidates)) throw new ArgumentException("指定された型に対応する順逆シリアル化メソッドを表す型が、読み込まれたすべてのアセンブリに定義されていませんでした。", nameof(key));

//        foreach (var mT_cand in methodsTypeCandidates)
//        {
//            mT_cand.
//        }
//    }

//    private static void OnLoad(Assembly assembly)
//    {
//        if (!assembly.GetReferencedAssemblies().Contains(ASSEMBLY_NAME)) return;

//        foreach (var typeInfo in assembly.DefinedTypes)
//        {
//            if (!typeInfo.IsSubclassOf(typeof(SerializationMethods))) continue;

//            if (Activator.CreateInstance(typeInfo) is not SerializationMethods methods) throw new Exception("`SerializationMethods`から派生する型が要件を満たしていません。引数の無いコンストラクタで初期化できませんでした。");
//            var key = methods.TargetType;
//            if (key.IsByRef) key = key.GetElementType(true);
//            if (key.IsGenericType) key = key.GetGenericTypeDefinition();

//            if (!_serializationMethodsTypes.TryGetValue(key, out var list))
//            {
//                list = new();
//                _serializationMethodsTypes.Add(key, list);
//            }

//            list.Add(typeInfo.AsType());
//        }
//    }
//}

//public abstract class SerializationMethods<T>
//{
//    static readonly AssemblyName ASSEMBLY_NAME = Assembly.GetExecutingAssembly().GetName();

//    public abstract Task Insert(INote to, in T instance);

//    public abstract Task Remove(INote from, out T instance);

//    static readonly Dictionary<Type, List<Type>> _methodsTypes = new();

//    public static SerializationMethods<T> GetSerializationMethods()
//    {



//        foreach (var item in _met)
//        {

//        }
//    }

//    private static void OnLoad(Assembly assembly)
//    {
//        if (!assembly.GetReferencedAssemblies().Contains(ASSEMBLY_NAME)) return;

//        foreach (var typeInfo in assembly.DefinedTypes)
//        {
//            if (typeInfo.GetBaseTypes().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(SerializationMethods<>)) is not Type methodsT) continue;

//            var targetT = methodsT.GenericTypeArguments[0];

//            var key = targetT;
//            if (key.IsByRef) key = key.GetElementType(true);
//            if (key.IsGenericType) key = key.GetGenericTypeDefinition();

//            if (!_methodsTypes.TryGetValue(key, out var list))
//            {
//                list = new();
//                _methodsTypes.Add(key, list);
//            }

//            list.Add(typeInfo.AsType());
//        }
//    }
//}
