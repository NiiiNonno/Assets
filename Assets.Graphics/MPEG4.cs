//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Nonno.Assets.Collections;
//using Nonno.Assets.Notes;
//using static Nonno.Assets.Graphics.PortableNetworkGraphic;

//namespace Nonno.Assets.Graphics;
//internal class MPEG4 : ContainerBox, IDisposable
//{
//    public static readonly HashTableTwoWayDictionary<NetworkStreamNote.Type, TypeIdentifier> DICTIONARY = new HashTableTwoWayDictionary<NetworkStreamNote.Type, TypeIdentifier>()
//    {
//        { new((ASCIIString)"IHDR"), new(typeof(LeafBox<ImageHeader>)) },
//        { new((ASCIIString)"IEND"), new(typeof(EmptyBox)) },
//        { new((ASCIIString)"PLTE"), new(typeof(PaletteBox)) },
//        { new((ASCIIString)"IDAT"), new(typeof(ImageDataBox)) },
//        { new((ASCIIString)"tEXt"), new(typeof(StringBox)) },
//        { new((ASCIIString)"bKGD"), new(typeof(BackgroundColorBox)) },
//    };

//    bool _isDisposed;
//    Disposables _disposables;

//    public MPEG4(INote note) : base(note)
//    {

//    }

//    public void Dispose()
//    {
//        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
//        Dispose(disposing: true);
//        GC.SuppressFinalize(this);
//    }
//    protected virtual void Dispose(bool disposing)
//    {
//        if (!_isDisposed)
//        {
//            if (disposing)
//            {
//                _disposables.Dispose();
//            }

//            _isDisposed = true;
//        }
//    }

//    public static MPEG4 Open(string path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
//    {
//        var stream = File.Open(path, mode, access, share);
//        var r = Create(from: stream);
//        r._disposables += stream;
//        return r;
//    }
//    public static MPEG4 Open(FileInfo fileInfo, FileMode mode = FileMode.Open, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
//    {
//        var stream = fileInfo.Open(mode, access, share);
//        var r = Create(from: stream);
//        r._disposables += stream;
//        return r;
//    }
//    public static MPEG4 Create(Stream from) => new(new NetworkStreamNote(from, DICTIONARY));
//}
