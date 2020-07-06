namespace Soap.Interfaces
{
    using System;
    using FluentValidation;
    using Soap.Interfaces.Messages;

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

        public override ApiPermission Permission { get; }
    }

    public abstract class MessageFailedAllRetries : ApiCommand
    {
        public ApiMessage FailedMessage { get; set; }

    }
}