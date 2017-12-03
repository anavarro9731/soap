namespace Soap.Pf.MsmqEndpointBase
{
    using System.Threading.Tasks;
    using Rebus.Handlers;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;

    public class CommandHandler : IHandleMessages<IApiCommand>
    {
        private readonly IMessagePipeline messagePipeline;

        public CommandHandler(IMessagePipeline messagePipeline)
        {
            this.messagePipeline = messagePipeline;
        }

        public async Task Handle(IApiCommand command)
        {
            await this.messagePipeline.Execute(command).ConfigureAwait(false);
        }
    }
}