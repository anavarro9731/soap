namespace Soap.MessagesSharedWithClients.Queries
{
    using System;
    using System.Collections.Generic;
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public sealed class GetMessageLogItemsQuery : ApiQuery
    {
        public List<Guid> MessageIdsOfLogItems { get; set; } = new List<Guid>();

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

    public class GetMessageLogItemsQueryValidator : AbstractValidator<GetMessageLogItemsQuery>
    {
        private GetMessageLogItemsQueryValidator()
        {
            RuleFor(cmd => cmd.MessageIdsOfLogItems).NotEmpty();
        }

        public static GetMessageLogItemsQueryValidator Default { get; } = new GetMessageLogItemsQueryValidator();
    }
}