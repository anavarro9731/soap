namespace Soap.MessagesSharedWithClients.Queries
{
    using System;
    using System.Collections.Generic;
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public sealed class GetMessageLogItemQuery : ApiQuery
    {
        public Guid MessageIdOfLogItem { get; set; }

        public sealed class MessageLogItemViewModel
        {
            public string ClrTypeOfMessage { get; set; }

            public List<FailedMessageResult> FailedAttempts { get; set; } = new List<FailedMessageResult>();

            public int MaxFailedMessages { get; set; }

            public dynamic Message { get; set; }

            public SuccessMessageResult SuccessfulAttempt { get; set; }

            public class FailedMessageResult
            {
                public Exception Error { get; set; }

                public DateTime? FailedAt { get; set; }
            }

            public class SuccessMessageResult
            {
                public dynamic ReturnValue { get; set; }

                public DateTime? SucceededAt { get; set; }
            }
        }
    }

    public class GetMessageLogItemQueryValidator : AbstractValidator<GetMessageLogItemQuery>
    {
        private GetMessageLogItemQueryValidator()
        {
            RuleFor(cmd => cmd.MessageIdOfLogItem).NotEmpty();
        }

        public static GetMessageLogItemQueryValidator Default { get; } = new GetMessageLogItemQueryValidator();
    }
}