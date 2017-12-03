namespace Soap.Pf.ClientServerMessaging.Commands
{
    using FluentValidation;
    using Soap.If.Interfaces.Messages;

    public class ForwardCommandFromHttpToMsmq<TApiCommand> : ApiCommand where TApiCommand : IApiCommand
    {
        public ForwardCommandFromHttpToMsmq(TApiCommand command)
        {
            Command = command;
        }

        public IApiCommand Command { get; set; }
    }

    public class ForwardCommandFromHttpToMsmqValidator<TApiCommand> : AbstractValidator<ForwardCommandFromHttpToMsmq<TApiCommand>> where TApiCommand : ApiCommand
    {
        public ForwardCommandFromHttpToMsmqValidator()
        {
            RuleFor(cmd => cmd.MessageId).NotEmpty();

            RuleFor(cmd => cmd.Command).NotEmpty();


        }

        public static ForwardCommandFromHttpToMsmqValidator<TApiCommand> Default { get; } = new ForwardCommandFromHttpToMsmqValidator<TApiCommand>();
    }
}