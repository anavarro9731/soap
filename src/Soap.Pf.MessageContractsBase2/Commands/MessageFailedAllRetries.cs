namespace Soap.Pf.MessageContractsBase.Commands
{
    using System;
    using FluentValidation;
    using Soap.If.Interfaces;

    public class MessageFailedAllRetries<TFailedMessage> : MessageFailedAllRetries
    {
        public MessageFailedAllRetries(Guid idOfMessageThatFailed)
        {
            IdOfMessageThatFailed = idOfMessageThatFailed;
        }

        public MessageFailedAllRetries()
        {
        }

        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        private class Validator : AbstractValidator<MessageFailedAllRetries>
        {
            public Validator()
            {
                RuleFor(x => x.IdOfMessageThatFailed).NotEmpty();
            }
        }
    }
}