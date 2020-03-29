namespace Soap.Pf.MessageContractsBase.Commands
{
    using System;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class MessageFailedAllRetries<TFailedMessage> : MessageFailedAllRetries
    {
        public MessageFailedAllRetries(Guid idOfMessageThatFailed)
        {
            IdOfMessageThatFailed = idOfMessageThatFailed;
        }

        public MessageFailedAllRetries()
        {
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