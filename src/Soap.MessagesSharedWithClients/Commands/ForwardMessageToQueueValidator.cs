namespace Soap.MessagesSharedWithClients.Commands
{
    using FluentValidation;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class ForwardMessageToQueueValidator<TApiCommand> : AbstractValidator<ForwardMessageToQueue<TApiCommand>> where TApiCommand : ApiCommand
    {
        public ForwardMessageToQueueValidator()
        {
            RuleFor(cmd => cmd.MessageId).NotEmpty();

            RuleFor(cmd => cmd.Message).NotEmpty();
        }

        public static ForwardMessageToQueueValidator<TApiCommand> Default { get; } = new ForwardMessageToQueueValidator<TApiCommand>();
    }
}
