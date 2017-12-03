namespace Soap.Pf.ClientServerMessaging.Commands
{
    using FluentValidation;
    using Soap.If.Interfaces.Messages;

    public class ForwardMessageToQueueValidator<TApiCommand> : AbstractValidator<ForwardCommandToQueue<TApiCommand>> where TApiCommand : ApiCommand
    {
        public ForwardMessageToQueueValidator()
        {
            RuleFor(cmd => cmd.MessageId).NotEmpty();

            RuleFor(cmd => cmd.Command).NotEmpty();


        }

        public static ForwardMessageToQueueValidator<TApiCommand> Default { get; } = new ForwardMessageToQueueValidator<TApiCommand>();
    }
}