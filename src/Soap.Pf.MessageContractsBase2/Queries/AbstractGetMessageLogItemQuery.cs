namespace Soap.Pf.MessageContractsBase.Queries
{
    using System;
    using System.Collections.Generic;
    using FluentValidation;
    using Soap.Interfaces;

    public abstract class AbstractGetMessageLogItemQuery<TResponse> : ApiCommand<TResponse>
        where TResponse : AbstractGetMessageLogItemQuery<TResponse>.AbstractResponseEvent, new()
    {
        public Guid MessageIdOfLogItem { get; set; }
        
        public abstract class AbstractResponseEvent : ApiEvent
        {
            public string ClrTypeOfMessage { get; set; }

            public bool Failed => FailedAttempts.Count >= MaxFailedMessages;

            public List<AbstractFailedMessageResult> FailedAttempts { get; set; } = new List<AbstractFailedMessageResult>();

            public int MaxFailedMessages { get; set; }

            public object Message { get; set; }

            public bool Succeeded => SuccessfulAttempt != null;

            public AbstractSuccessMessageResult SuccessfulAttempt { get; set; }

            public class AbstractFailedMessageResult
            {
                public Exception Error { get; set; }

                public DateTime? FailedAt { get; set; }
            }

            public class AbstractSuccessMessageResult
            {
                public object ReturnValue { get; set; }

                public DateTime? SucceededAt { get; set; }
            }
        }

        public class Validator : AbstractValidator<AbstractGetMessageLogItemQuery<TResponse>>
        {
            public Validator()
            {
                RuleFor(cmd => cmd.MessageIdOfLogItem).NotEmpty();
            }
        }
    }
}