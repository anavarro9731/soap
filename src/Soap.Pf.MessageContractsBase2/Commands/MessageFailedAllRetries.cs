namespace Soap.Pf.MessageContractsBase.Commands
{
    using System;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.Interfaces;

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

        public override Task Handle()
        {
            throw new NotImplementedException();
            //- TODO find extensions
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