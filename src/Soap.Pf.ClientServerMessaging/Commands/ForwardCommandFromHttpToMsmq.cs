namespace Soap.Pf.ClientServerMessaging.Commands
{
    using FluentValidation;
    using Soap.If.Interfaces.Messages;

    public interface IForwardCommandFromHttpToMsmq
    {
        IApiCommand CommandToForward { get; set; }
    }

    public class ForwardCommandFromHttpToMsmq<TApiCommand> : ApiCommand, IForwardCommandFromHttpToMsmq where TApiCommand : ApiCommand
    {
        public ForwardCommandFromHttpToMsmq(TApiCommand command)
        {
            CommandToForward = command;
        }

        public IApiCommand CommandToForward { get; set; }

        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<ForwardCommandFromHttpToMsmq<TApiCommand>>
        {
            public Validator()
            {
                RuleFor(cmd => cmd.MessageId).NotEmpty();

                RuleFor(cmd => cmd.CommandToForward).NotEmpty();
            }
        }
    }
}