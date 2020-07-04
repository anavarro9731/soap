namespace Soap.Interfaces
{
    using System;
    using FluentValidation;

    public class MessageFailedAllRetries<TFailedMessage> : MessageFailedAllRetries where TFailedMessage : ApiMessage
    {
        public MessageFailedAllRetries(TFailedMessage message)
        {
            FailedMessage = message;
        }

        public MessageFailedAllRetries()
        {
        }

        private class Validator : AbstractValidator<MessageFailedAllRetries>
        {
            public Validator()
            {
                RuleFor(x => x.FailedMessage).NotEmpty();
            }
        }
    }

    public abstract class MessageFailedAllRetries : ApiCommand
    {
        public ApiMessage FailedMessage { get; set; }

        public Guid? StatefulProcessIdOfMessageThatFailed { get; set; }
    }
}