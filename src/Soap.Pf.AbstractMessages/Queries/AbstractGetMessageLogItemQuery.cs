namespace Soap.Pf.MessageContractsBase.Queries
{
    using System;
    using System.Collections.Generic;
    using FluentValidation;
    using Soap.If.Interfaces.Messages;

    public abstract class AbstractGetMessageLogItemQuery<TResponse> : ApiQuery<TResponse> where TResponse : AbstractGetMessageLogItemQuery<TResponse>.AbstractResponseModel, new()
    {
        public Guid MessageIdOfLogItem { get; set; }

        public abstract class AbstractResponseModel
        {
            public string ClrTypeOfMessage { get; set; }

            public bool Failed => FailedAttempts.Count >= MaxFailedMessages;

            public List<AbstractFailedMessageResult> FailedAttempts { get; set; } = new List<AbstractFailedMessageResult>();

            public int MaxFailedMessages { get; set; }

            public dynamic Message { get; set; }

            public bool Succeeded => SuccessfulAttempt != null;

            public AbstractSuccessMessageResult SuccessfulAttempt { get; set; }

            public class AbstractFailedMessageResult
            {
                public Exception Error { get; set; }

                public DateTime? FailedAt { get; set; }
            }

            public class AbstractSuccessMessageResult
            {
                public dynamic ReturnValue { get; set; }

                public DateTime? SucceededAt { get; set; }
            }
        }
    }

    public class GetMessageLogItemQueryValidator<TResponse> : AbstractValidator<AbstractGetMessageLogItemQuery<TResponse>>
        where TResponse : AbstractGetMessageLogItemQuery<TResponse>.AbstractResponseModel, new()
    {
        public GetMessageLogItemQueryValidator()
        {
            RuleFor(cmd => cmd.MessageIdOfLogItem).NotEmpty();
        }
    }
}