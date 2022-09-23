using Microsoft.AspNetCore.Components;
using Range = Nonno.Assets.Graphics.Range;

namespace Nonno.Assets.Razors
{
    partial class InputRange
    {
        public int Width
        {
            get => Value.Width;
            set
            {
                Value = new Range(value, Height);
                _ = ValueChanged?.InvokeAsync(Value);
            }
        }
        public int Height
        {
            get => Value.Height;
            set
            {
                Value = new Range(Width, value);
                _ = ValueChanged?.InvokeAsync(Value);
            }
        }
        [Parameter]
        public int Step { get; set; }
        [Parameter]
        public string? Name { get; set; }
        [Parameter]
        public Range Value { get; set; }
        [Parameter]
        public EventCallback<Range>? ValueChanged { get; set; }
    }
}
