namespace Soap.Pf.MsmqEndpointBase
{
    using System.Threading.Tasks;
    using Rebus.Handlers;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.MessagePipeline;

    public class RebusCommandHandler : IHandleMessages<IApiCommand>
    {
        private readonly MessagePipeline messagePipeline;

        public RebusCommandHandler(MessagePipeline messagePipeline)
        {
            this.messagePipeline = messagePipeline;
        }

        public async Task Handle(IApiCommand command)
        {
            await this.messagePipeline.Execute(command).ConfigureAwait(false);
        }
    }
}