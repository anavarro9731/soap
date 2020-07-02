namespace Soap.Pf.MessageContractsBase.Commands
{
    using System;
    using FluentValidation;
    using Soap.Interfaces;

    public class MessageFailedAllRetries<TFailedMessage> : MessageFailedAllRetries where TFailedMessage : ApiMessage
    {
        public MessageFailedAllRetries(TFailedMessage message)
        {
            IdOfMessageThatFailed = message.MessageId;
            FailedMessage = message;
        }

        public MessageFailedAllRetries()
        {
        }

        public TFailedMessage FailedMessage { get; set; }

        private class Validator : AbstractValidator<MessageFailedAllRetries>
        {
            public Validator()
            {
                RuleFor(x => x.IdOfMessageThatFailed).NotEmpty();
            }
        }
    }

    public abstract class MessageFailedAllRetries : ApiCommand
    {
        public Guid IdOfMessageThatFailed { get; set; }

        public Guid? StatefulProcessIdOfMessageThatFailed { get; set; }
    }
}