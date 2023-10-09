#nullable enable
using System;

namespace Nonno.Assets.Presentation
{
    public interface IPresenter
    {
        object? Model { set; }
    }

    public interface IPresenter<in T> : IPresenter
    {
        new T? Model { set; }

        object? IPresenter.Model
        {
            set
            {
                if (value is null) Model = default;
                if (value is T t) Model = t;
                throw new ArgumentException();
            }
        }
    }
}