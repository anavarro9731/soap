namespace Soap.Interfaces
{
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

        public override ApiPermission Permission { get; }

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
    }
}