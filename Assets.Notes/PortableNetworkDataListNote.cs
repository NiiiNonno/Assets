using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets;
public class PortableNetworkDataListNote : INote
{
    readonly List<ChunkInfo> _infos;

    /// <inheritdoc/>
    /// <remarks>
    /// <see cref="NotePoint"/>は
    /// </remarks>
    public NotePoint Point
    {
        get
        {
            
        }
        set
        {

        }
    }
    public object this[int chunkNumber]
    {
        get
        {

        }
        set
        {

        }
    }

    public PortableNetworkDataListNote(int capacity)
    {
        _infos = new(capacity: capacity);
    }

    public INote Copy() => throw new NotImplementedException();
    public void Dispose() => throw new NotImplementedException();
    public ValueTask DisposeAsync() => throw new NotImplementedException();

    public Task Insert(in NotePoint index)
    {
        
    }
    public Task Insert<T>(Memory<T> memory) where T : unmanaged => throw new NotImplementedException();
    public void InsertSync<T>(Span<T> span) where T : unmanaged => throw new NotImplementedException();
    public bool IsValid(NotePoint index) => throw new NotImplementedException();
    public Task Remove(out NotePoint index)
    {
        
    }
    public Task Remove<T>(Memory<T> memory) where T : unmanaged => throw new NotImplementedException();
    public void RemoveSync<T>(Span<T> span) where T : unmanaged => throw new NotImplementedException();

    protected class ChunkInfo
    {

    }
}

public class PortableNetworkDataChunk
{

}