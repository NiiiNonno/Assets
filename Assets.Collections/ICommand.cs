#nullable enable
using System;
using System.Threading.Tasks;

namespace Nonno.Assets.Collections
{
    public interface ICommand
    {
        bool CanExecute { get; }
        void Execute(Typed info = default);
        Task ExecuteAsync(Typed info = default)
        {
            Execute();
            return Task.CompletedTask;
        }
    }

    public abstract class Command : ICommand
    {
        public bool CanExecute { get; protected set; } = true;

        public abstract void Execute(Typed info = default);
        public virtual Task ExecuteAsync(Typed info = default)
        {
            Execute();
            return Task.CompletedTask;
        }
    }

    public class DelegatedCommand : Command
    {
        readonly Action<Typed> _d;

        public DelegatedCommand(Action<Typed> @delegate)
        {
            _d = @delegate;
        }

        public override void Execute(Typed info = default) => _d(info);
    }
}