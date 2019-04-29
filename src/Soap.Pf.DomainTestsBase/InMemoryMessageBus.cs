namespace Soap.Pf.DomainTestsBase
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.Messages;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;

    public class InMemoryMessageBus : IBusContext
    {
        private readonly List<IMessage> messages = new List<IMessage>();

        public IEnumerable<IApiCommand> Commands => this.messages.OfType<IApiCommand>();

        public IEnumerable<IApiEvent> Events => this.messages.OfType<IApiEvent>();

        public async Task Publish(IApiEvent publishEvent)
        {
            await Store(publishEvent).ConfigureAwait(false);
        }

        public async Task Send(IApiCommand sendCommand)
        {
            await Store(sendCommand).ConfigureAwait(false);
        }

        public async Task SendLocal(IApiCommand sendCommand)
        {
            await Store(sendCommand).ConfigureAwait(false);
        }

        public bool IsOneWay { get; set; }

        private Task Store(IMessage message)
        {
            this.messages.Add(message);
            return Task.CompletedTask;
        }
    }
}