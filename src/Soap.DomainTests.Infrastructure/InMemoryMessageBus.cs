namespace Soap.DomainTests.Infrastructure
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ServiceApi.Interfaces.LowLevel.Messages;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using Soap.Interfaces;

    public class InMemoryMessageBus : IBusContext
    {
        private readonly List<IMessage> messages = new List<IMessage>();

        public IEnumerable<IApiCommand> Commands => this.messages.OfType<IApiCommand>();

        public IEnumerable<IApiEvent> Events => this.messages.OfType<IApiEvent>();

        public async Task Publish(IPublishEventOperation publishEvent)
        {
            await Store(publishEvent.Event).ConfigureAwait(false);
        }

        public async Task Send(ISendCommandOperation sendCommand)
        {
            await Store(sendCommand.Command).ConfigureAwait(false);
        }

        public async Task SendLocal(ISendCommandOperation sendCommand)
        {
            await Store(sendCommand.Command).ConfigureAwait(false);
        }

        private Task Store(IMessage message)
        {
            this.messages.Add(message);
            return Task.CompletedTask;
        }
    }
}
