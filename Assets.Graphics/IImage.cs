using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace Nonno.Assets.Graphics;

public interface IReadOnlyImage<T>
{
    Point MinimumPoint { get; }
    Point MaximumPoint { get; }
    T this[Point point] { get; }

    void Get(Span<T> to, Point startPoint, Point endPoint);
    Task GetAsync(Memory<T> to, Point startPoint, Point endPoint);

    bool TryConvert<TTo>([MaybeNullWhen(false)] out IReadOnlyImage<TTo> to) { to = null; return false; }
}

public interface IImage<T> : IReadOnlyImage<T>
{
    new T this[Point point] { get; set; }

    void Set(ReadOnlySpan<T> from, Point startPoint, Point endPoint);
    Task SetAsync(ReadOnlyMemory<T> from, Point startPoint, Point endPoint);

    bool TryConvert<TTo>([MaybeNullWhen(false)]out IImage<TTo> to) { to = null; return false; }
}

public static partial class Utils
{
    static Utils()
    {
        ImageConvertMethod<byte, RGBColor24>.INSTANCE = new(x => new ConvertedRGBColor24Image(x));
    }

    public static IImage<TTo> Convert<TFrom, TTo>(this IImage<TFrom> @this)
    {
        if (@this is IImage<TTo> r) return r;
        if (@this.TryConvert(out IImage<TTo>? r_)) return r_;

        if (ImageConvertMethod<TFrom, TTo>.INSTANCE is not { } method) method = ImageConvertMethod<TFrom, TTo>.INSTANCE = new();
        return method.Delegate(@this);
    }

    internal class ImageConvertMethod<TFrom, TTo>
    {
        public static ImageConvertMethod<TFrom, TTo>? INSTANCE { get; set; }

        public ConvertDelegate<TFrom, TTo> Delegate { get; set; }

        public ImageConvertMethod()
        {
            throw new NotImplementedException();

            switch (typeof(TFrom).GetCustomAttribute<ColorStructAttribute>(), typeof(TTo).GetCustomAttribute<ColorStructAttribute>())
            {
            case ({ } fCSA, { } tCSA):
                {
                    return;
                }
            case ({ } fCSA, null):
                {
                    return;
                }
            case (null, { } tCSA):
                {
                    return;
                }
            case (null, null):
                {
                    return;
                }
            }
        }
        public ImageConvertMethod(ConvertDelegate<TFrom, TTo> @delegate)
        {
            Delegate = @delegate;
        }
    }
}

internal delegate IImage<TTo> ConvertDelegate<TFrom, TTo>(IImage<TFrom> image);

internal class ConvertedRGBColor24Image : Image<RGBColor24>
{
    readonly IImage<byte> _base;

    public override Point MinimumPoint => _base.MinimumPoint;
    public override Point MaximumPoint => _base.MaximumPoint;

    public override RGBColor24 this[Point point] { get { var v = _base[point]; return new(v, v, v); } set => _base[point] = (byte)((value.red + value.green + value.blue) / 3); }

    public ConvertedRGBColor24Image(IImage<byte> @base) => _base = @base;
}