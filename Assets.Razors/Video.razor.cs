using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Nonno.Assets.Graphics;
using Range = Nonno.Assets.Graphics.Range;

namespace Nonno.Assets.Razors
{
    partial class Video
    {
        readonly Bitmap _buffer;

        readonly int _cutLength;
        readonly long _current;

        [Parameter]
        public Guid Id { get; set; }
        [Parameter]
        [Required]
        public IVideoSource? Source { get; set; }
        [Parameter]
        [Required]
        public Range Range
        {
            get => _buffer.Range;
            set => _buffer.Range = value;
        }

        public Video()
        {
            _buffer = new();
            
            Id = Guid.NewGuid();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Source == null) return;

            for (int i = 0; i < _cutLength; i++)
            {
                await Source.CopyNextFrame(to: _buffer);


            }
        }

        public async void SetFile(byte[] data, string type) => await JSR.InvokeVoidAsync("setFile", data, Id.ToString(), type);
    }
}
