namespace Soap.Pf.ClientServerMessaging.Commands
{
    using FluentValidation;
    using Soap.If.Interfaces.Messages;

    public interface IForwardCommandFromHttpToMsmq
    {
        IApiCommand CommandToForward { get; set; }
    }

    public class ForwardCommandFromHttpToMsmq<TApiCommand> : ApiCommand, IForwardCommandFromHttpToMsmq where TApiCommand : IApiCommand
    {
        public ForwardCommandFromHttpToMsmq(TApiCommand command)
        {
            CommandToForward = command;
        }

        public IApiCommand CommandToForward { get; set; }
    }

    public class ForwardCommandFromHttpToMsmqValidator<TApiCommand> : AbstractValidator<ForwardCommandFromHttpToMsmq<TApiCommand>> where TApiCommand : ApiCommand
    {
        public ForwardCommandFromHttpToMsmqValidator()
        {
            RuleFor(cmd => cmd.MessageId).NotEmpty();

            RuleFor(cmd => cmd.CommandToForward).NotEmpty();


        }

        public static ForwardCommandFromHttpToMsmqValidator<TApiCommand> Default { get; } = new ForwardCommandFromHttpToMsmqValidator<TApiCommand>();
    }
}