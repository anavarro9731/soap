namespace Soap.Pf.MsmqEndpointBase
{
    public class RebusCommandHandler : IHandleMessages<IApiCommand>
    {
        private readonly MessagePipeline messagePipeline;

        public RebusCommandHandler(MessagePipeline messagePipeline)
        {
            this.messagePipeline = messagePipeline;
        }

        public async Task Handle(IApiCommand command)
        {
            await this.messagePipeline.Execute(command);
        }
    }
}