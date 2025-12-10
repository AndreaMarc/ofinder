using System.Threading.Tasks;

namespace MIT.Fwk.Core.CQRS
{
    /// <summary>
    /// Mediator handler for dispatching commands and raising events.
    /// Wraps MediatR for CQRS pattern implementation.
    /// </summary>
    public interface IMediatorHandler
    {
        Task SendCommand<T>(T command) where T : Command;
        Task RaiseEvent<T>(T @event) where T : Event;
    }

    // IHandler<T> removed - FASE 10: Never used in codebase (MediatR INotificationHandler used instead)
}
