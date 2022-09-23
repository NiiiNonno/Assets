using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Nonno.Assets.Graphics;
using Range = Nonno.Assets.Graphics.Range;

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
