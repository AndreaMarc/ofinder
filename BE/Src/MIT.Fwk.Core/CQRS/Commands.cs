using FluentValidation.Results;
using System;

namespace MIT.Fwk.Core.CQRS
{
    /// <summary>
    /// Base class for all commands in the CQRS pattern.
    /// Commands represent intentions to change state.
    /// </summary>
    public abstract class Command : Message
    {
        public DateTime Timestamp { get; private set; }
        public ValidationResult ValidationResult { get; set; }

        protected Command()
        {
            Timestamp = DateTime.Now;
        }

        public abstract bool IsValid();
    }

    // CommandResponse removed - FASE 10: Never used in codebase
}
