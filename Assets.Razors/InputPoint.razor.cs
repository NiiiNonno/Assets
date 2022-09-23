using Microsoft.AspNetCore.Components;
using Nonno.Assets.Graphics;

namespace Nonno.Assets.Razors
{
    partial class InputPoint
    {
        public int X
        {
            get => Value.X;
            set
            {
                Value = new Point(value, Y);
                _ = ValueChanged?.InvokeAsync(Value);
            }
        }
        public int Y
        {
            get => Value.Y;
            set
            {
                Value = new Point(X, value);
                _ = ValueChanged?.InvokeAsync(Value);
            }
        }
        [Parameter]
        public int Step { get; set; }
        [Parameter]
        public string? Name { get; set; }
        [Parameter]
        public Point Value { get; set; }
        [Parameter]
        public EventCallback<Point>? ValueChanged { get; set; }
    }
}
