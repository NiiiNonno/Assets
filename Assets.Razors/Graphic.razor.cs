#if USE_SYSTEM_DRAWINGS
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Nonno.Assets.Graphics;
using Bitmap = System.Drawing.Bitmap;
using Color = Nonno.Assets.Graphics.Color;
using Range = Nonno.Assets.Graphics.Range;

namespace Nonno.Assets.Razors
{
    partial class Graphic
    {
        Exception? _exception;
        IUnit _unit;
        GraphicMethod _method;
        private bool _disposedValue;

        [Parameter]
        [SuppressMessage("Interoperability", "CA1416:プラットフォームの互換性の検証", Justification = "設定次第でサポートされる場合がある。")]
        public GraphicMethod Method
        {
            get => _method;
            set
            {
                if (_method != value)
                {
                    var old = _unit;

                    _method = value;
                    _unit = value switch
                    {
                        GraphicMethod.JpegInline => new InlineSingleImageUnit(ImageFormat.Jpeg),
                        GraphicMethod.JpegFile => new LinkedSingleImageUnit(ImageFormat.Jpeg),
                        GraphicMethod.PngInline => new InlineSingleImageUnit(ImageFormat.Png),
                        GraphicMethod.PngFile => new LinkedSingleImageUnit(ImageFormat.Png),
                        GraphicMethod.GifInline => new InlineSingleImageUnit(ImageFormat.Gif),
                        GraphicMethod.GifFile => new LinkedSingleImageUnit(ImageFormat.Gif),
                        GraphicMethod.Mp4Inline => throw new NotImplementedException(),
                        GraphicMethod.Mp4File => throw new NotImplementedException(),
                        GraphicMethod.OgvInline => throw new NotImplementedException(),
                        GraphicMethod.OgvFile => throw new NotImplementedException(),
                        GraphicMethod.ArgbRaster => new CanvasUnit(JSR),
                        GraphicMethod.AhsvRaster => new CanvasUnit(JSR),
                        _ => throw new Exception($"{_method}に予期しない値が入っています。")
                    };

                    _unit.Quality = old.Quality;
                    _unit.Range = old.Range;
                    if (old is IDisposable disposable) disposable.Dispose();
                }
            }
        }
        [Parameter]
        public float Quality { get => _unit.Quality; set => _unit.Quality = value; }
        [Parameter]
        public Range Range { get => _unit.Range; set => _unit.Range = value; }
        public int Width => _unit.Range.Width;
        public int Height => _unit.Range.Height;

        public Graphic()
        {
            _unit = new EmptyUnit();
        }

        public async Task Enqueue(IOriginal<Color> original)
        {
            try
            {
                await _unit.Enqueue(original);
            }
            catch (Exception e)
            {
                _exception = e;
            }
            finally
            {
                StateHasChanged();
            }
        }

        protected override void OnAfterRender(bool firstRender) => _unit.OnAfterRender();

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_unit is IDisposable disposable) disposable.Dispose();
                }

                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        record VideoAttributes(string Src);
        record ImgAttributes(string Src);
        record CanvasAttributes(string Id);

        interface IUnit
        {
            object? Attributes { get; }
            Range Range { get; set; }
            float Quality { get; set; }
            event Action? Updated;
            Task Enqueue(IOriginal<Color> original);
            void OnAfterRender() { }
        }

        struct EmptyUnit : IUnit
        {
            object? IUnit.Attributes => null;
            Range IUnit.Range { get; set; }
            float IUnit.Quality { get; set; }
            event Action? IUnit.Updated { add { } remove { } }
            Task IUnit.Enqueue(IOriginal<Color> original) => Task.CompletedTask;
        }

        [SuppressMessage("Interoperability", "CA1416:プラットフォームの互換性の検証", Justification = "設定次第でサポートされる場合がある。")]
        abstract class SingleImageUnit : IUnit, IDisposable
        {
            readonly ImageCodecInfo _codecInfo;
            readonly EncoderParameters _encoderParameters;
            Bitmap? _bitmap;
            Range _range;
            float _quality;
            private bool _disposedValue;

            public Guid Id { get; }
            public Range Range
            {
                get => _range;
                set
                {
                    if (_range != value)
                    {
                        using var _ = _bitmap;
                        _bitmap = value.Width == 0 || value.Height == 0 ? null : new Bitmap(value.Width, value.Height);
                        _range = value;
                    }
                }
            }
            public float Quality
            {
                get => _quality;
                set
                {
                    using var _ = _encoderParameters.Param[0];
                    _encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, (byte)value);
                    _quality = value;
                }
            }
            public object? Attributes { get; protected set; }
            protected ImageCodecInfo CodecInfo => _codecInfo;
            public event Action? Updated;

            public SingleImageUnit(ImageCodecInfo codecInfo)
            {
                _codecInfo = codecInfo;
                _encoderParameters = new EncoderParameters(1);
            }

            public async Task Enqueue(IOriginal<Color> original)
            {
                if (_bitmap == null) return;

                var data = _bitmap.LockBits(new(default, _bitmap.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                original.CopyTo(data.Scan0.AsSpan<Color>(data.Width * data.Height));
                _bitmap.UnlockBits(data);

                Stream stream = await GetStream();
                try
                {
                    _bitmap.Save(stream, _codecInfo, _encoderParameters);
                }
                finally
                {
                    await CloseStream(stream);
                }

                Updated?.Invoke();
            }

            protected abstract Task<Stream> GetStream();

            protected virtual Task CloseStream(Stream stream) { stream.Dispose(); return Task.CompletedTask; }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    if (disposing)
                    {
                        _bitmap?.Dispose();
                        _encoderParameters.Dispose();
                    }

                    _disposedValue = true;
                }
            }
            public void Dispose()
            {
                // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        [SuppressMessage("Interoperability", "CA1416:プラットフォームの互換性の検証", Justification = "設定次第でサポートされる場合がある。")]
        class LinkedSingleImageUnit : SingleImageUnit
        {
            readonly Queue<Guid> _queue;
            readonly string _extension;
            readonly int _bufferSize;

            public LinkedSingleImageUnit(ImageFormat format, string? extension = null, int bufferSize = 2) : base(ImageCodecInfo.GetImageEncoders().Where(x => x.FormatID == format.Guid).First())
            {
                if (bufferSize <= 1) throw new ArgumentException("1以下の数値は指定できません", nameof(bufferSize));

                _queue = new();
                _extension = extension ?? CodecInfo.FilenameExtension?.Trim(' ', '.', '*') ?? throw new ArgumentNullException(nameof(extension), "MIME型を取得できません。");
                _bufferSize = bufferSize;
            }

            protected override Task<Stream> GetStream()
            {
                var id = Utils.GetGloballyUniqueIdentifier();
                var fileInfo = new FileInfo(GetPath(id, _extension));
                var r = fileInfo.Create();
                fileInfo.Attributes |= FileAttributes.Temporary;
                _queue.Enqueue(id);
                return Task.FromResult<Stream>(r);
            }

            protected override Task CloseStream(Stream stream)
            {
                stream.Close();
                Attributes = new ImgAttributes($"_content/Nonno.Assets.Razors/temps/{_queue.Last()}.{_extension}");
                if (_queue.Count >= _bufferSize) File.Delete(GetPath(_queue.Dequeue(), _extension));
                return Task.CompletedTask;
            }

            protected override void Dispose(bool disposing) 
            {
                while (_queue.TryDequeue(out var id)) File.Delete(GetPath(id, _extension));
                base.Dispose(disposing); 
            }

            static string GetPath(Guid id, string extension) => Path.Combine(Path.GetDirectoryName(Environment.CurrentDirectory)!, "Nonno.Assets.Razors", "wwwroot", "temps", $"{id}.{extension}");
        }

        [SuppressMessage("Interoperability", "CA1416:プラットフォームの互換性の検証", Justification = "設定次第でサポートされる場合がある。")]
        class InlineSingleImageUnit : SingleImageUnit
        {
            readonly string _mimeName;

            public InlineSingleImageUnit(ImageFormat format, string? mimeName = null) : base(ImageCodecInfo.GetImageEncoders().Where(x => x.FormatID == format.Guid).First())
            {
                _mimeName = mimeName ?? CodecInfo.MimeType ?? throw new ArgumentNullException(nameof(mimeName), "MIME型を取得できません。");
            }

            protected override Task<Stream> GetStream() => Task.FromResult<Stream>(new MemoryStream());

            protected override Task CloseStream(Stream stream)
            {
                Attributes = new ImgAttributes($"data:{_mimeName};base64,{Convert.ToBase64String(((MemoryStream)stream).ToArray())}");
                stream.Close();
                return Task.CompletedTask;
            }
        }

        class CanvasUnit : IUnit
        {
            readonly Guid _id;
            readonly IJSRuntime _runtime;
            IJSObjectReference? _reference;
            Range _range;
            int _length;
            string _buffer;

            public Range Range
            {
                get => _range;
                set
                {
                    if (_range != value)
                    {
                        _length = value.Width * value.Height;
                        _buffer = new(default, _length << 1);
                        _range = value;
                    }
                }
            }
            public float Quality { get; set; }
            public object? Attributes { get; }
            event Action? IUnit.Updated { add { } remove { } }

            public CanvasUnit(IJSRuntime jSRuntime)
            {
                _id = Utils.GetGloballyUniqueIdentifier();
                _runtime = jSRuntime;
                _buffer = default!;

                Attributes = new CanvasAttributes(_id.ToString());
                Range = default;
            }

            public async Task Enqueue(IOriginal<Color> original)
            {
                original.CopyTo(_buffer.AsSpan<Color>(_length));
                if (_reference != null) await _reference.InvokeVoidAsync("drawColorSpan", _id.ToString(), _buffer);
            }

            public async void OnAfterRender()
            {
                _reference = await _runtime.InvokeAsync<IJSObjectReference>("import", $"./_content/Nonno.Assets.Razors/CanvasControl.js");
            }
        }
    }

    public enum GraphicMethod
    {
        JpegInline,
        JpegFile,
        PngInline,
        PngFile,
        GifInline,
        GifFile,
        Mp4Inline,
        Mp4File,
        OgvInline,
        OgvFile,
        ArgbRaster,
        AhsvRaster,
    }
}
#endif