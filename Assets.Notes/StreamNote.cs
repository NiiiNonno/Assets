using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets;
public class StreamNote : INote
{
    readonly Stream _mS;
    readonly LinkedList<Difference> _ds;
    readonly int _pOfs;
    
    bool _isDisposed;

    public Stream MainStream => _mS;
    public int PointerOffset
    {
        get => _pOfs;
        init => _pOfs = value;
    }

    public NotePoint Point { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public StreamNote(Stream mainStream)
    {
        _mS = mainStream;
    }

    public virtual void Flush()
    {

    }

    public virtual bool IsValid(NotePoint index) => throw new NotImplementedException();
    public INote Copy()
    {
        Flush();
    }

    public virtual Task Insert(in NotePoint index)
    {
        index.Number;
    }

    public virtual Task Remove(out NotePoint index)
    {

    }

    public virtual Task Insert<T>(Memory<T> memory) where T : unmanaged => throw new NotImplementedException();
    public virtual void InsertSync<T>(Span<T> span) where T : unmanaged => throw new NotImplementedException();

    public virtual Task Remove<T>(Memory<T> memory) where T : unmanaged => throw new NotImplementedException();
    public virtual void RemoveSync<T>(Span<T> span) where T : unmanaged => throw new NotImplementedException();

    public void Dispose()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _mS.Dispose();
            }

            // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
            // TODO: 大きなフィールドを null に設定します
            _isDisposed = true;
        }
    }
    public async ValueTask DisposeAsync()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        await DisposeAsync(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                await _mS.DisposeAsync();
            }

            // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
            // TODO: 大きなフィールドを null に設定します
            _isDisposed = true;
        }
    }

    public class Difference
    {
        long _sP;
        long _eP;

        public long StartPosition => _sP;
        public long EndPosition => _eP;

        public Difference(long startPoint, long endPoint)
        {

        }
    }
}

public class NetworkStreamNote : StreamNote
{
    readonly IDictionary<Type, TypeIdentifier> _tD;
    readonly IDictionary<TypeIdentifier, Type> _d;

    public NetworkStreamNote(Stream mainStream, IDictionary<Type, TypeIdentifier> typeDictionary) : base(mainStream: mainStream)
    {
        _tD = typeDictionary;
    }

    public override Task Insert<T>(Memory<T> memory)
    {
        if (memory.Span.Is(out Span<TypeIdentifier> result))
        {
            Tasks tasks = default;
            foreach (var tI in result)
            {
                var type = _d[tI];
                tasks += this.Insert(uInt64: type.Value);
            }
            return tasks.WhenAll();
        }

        return base.Insert(memory: memory);
    }

    public readonly struct Type : IEquatable<Type>
    {
        readonly ulong _value;

        public ulong Value => _value;

        public Type(ulong value)
        {
            _value = value;
        }
        public Type(ASCIIString @string)
        {
            _value = BitConverter.ToUInt64(@string.AsSpan());
        }

        public static explicit operator Type(ASCIIString @string) => new(@string);
        public static implicit operator ASCIIString(Type type) => new(BitConverter.GetBytes(type._value));

        public static bool operator ==(Type left, Type right) => left.Equals(right);
        public static bool operator !=(Type left, Type right) => !(left == right);

        public override bool Equals(object? obj) => obj is Type code && Equals(code);
        public bool Equals(Type other) => _value == other._value;
        public override int GetHashCode() => HashCode.Combine(_value);
    }
}

public enum NoteMode
{
    ReadOnly,
    ReadMostly,
    WriteOnly,
    WriteMostly,
    ReadWrite,
}
