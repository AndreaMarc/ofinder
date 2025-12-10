using MediatR;
using MIT.Fwk.Core.CQRS;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Bus
{
    /// <summary>
    /// In-memory bus implementation for CQRS pattern.
    /// Events are published via MediatR only (no SQL EventStore persistence).
    /// MongoDB logging is handled separately via middleware.
    /// </summary>
    public sealed class InMemoryBus : IMediatorHandler
    {
        private readonly IMediator _mediator;

        public InMemoryBus(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task SendCommand<T>(T command) where T : Command
        {
            return Publish(command);
        }

        public Task RaiseEvent<T>(T @event) where T : Event
        {
            // Events published via MediatR only (no SQL EventStore persistence)
            return Publish(@event);
        }

        private Task Publish<T>(T message) where T : Message
        {
            return _mediator.Publish(message);
        }
    }
}