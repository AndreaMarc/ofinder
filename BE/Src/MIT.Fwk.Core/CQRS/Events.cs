using MediatR;
using System;

namespace MIT.Fwk.Core.CQRS
{
    /// <summary>
    /// Base class for all messages (commands and events).
    /// Implements MediatR's INotification for pub/sub pattern.
    /// </summary>
    public abstract class Message : INotification
    {
        public string MessageType { get; protected set; }
        public string AggregateId { get; protected set; }

        protected Message()
        {
            MessageType = GetType().Name;
        }
    }

    /// <summary>
    /// Base class for all domain events in Event Sourcing pattern.
    /// Events represent something that has already happened.
    /// </summary>
    public abstract class Event : Message
    {
        public DateTime Timestamp { get; private set; }
        public string EntityName { get; set; }

        protected Event()
        {
            Timestamp = DateTime.Now;
        }
    }
}
