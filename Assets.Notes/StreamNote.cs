using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nonno.Assets.Collections;
using Nonno.Assets.Notes;

namespace Nonno.Assets;
public class StreamNote : SectorNote<ISector>
{
    readonly Stream _mS;
    readonly long _bL;

    public StreamNote(Stream mainStream, long bufferLength) : base(new StreamSector(mainStream, long.MinValue))
    {
        _mS = mainStream;
        _bL = bufferLength;
    }
    protected StreamNote(StreamNote original) : base(original)
    {
        throw new NotImplementedException();
    }

    public override INote Copy()
    {
        return new StreamNote(this);
    }

    public override Task Insert(in NotePointer pointer)
    {

    }
    public override Task Remove(out NotePointer pointer)
    {

    }

    protected override ISector CreateSector(long number)
    {

    }
    protected override void DeleteSector(ISector sector)
    {

    }
    protected override NotePointer MakePointer(ISector of)
    {

    }
    protected override void DestroyPointer(NotePointer pointer)
    {

    }
}

public class NetworkStreamNote : StreamNote
{
    readonly ITwoWayDictionary<Type, TypeIdentifier> _tD;

    public NetworkStreamNote(Stream mainStream, ITwoWayDictionary<Type, TypeIdentifier> typeDictionary) : base(mainStream: mainStream)
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
                var type = _tD.Opposite[tI];
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
        public static implicit operator ASCIIString(Type type) => new((Span<byte>)BitConverter.GetBytes(type._value));

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
