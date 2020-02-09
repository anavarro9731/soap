namespace Soap.DomainTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using Soap.Interfaces;
    using Soap.Interfaces.Bus;
    using Soap.Interfaces.Messages;

    public class InMemoryBusContext : IBusContext
    {
        private readonly IMessageAggregator messageAggregator;

        public InMemoryBusContext(IMessageAggregator messageAggregator)
        {
            this.messageAggregator = messageAggregator;
        }

        public IEnumerable<ApiCommand> Commands => this.messageAggregator.AllMessages.OfType<ApiCommand>();

        public IEnumerable<ApiEvent> Events => this.messageAggregator.AllMessages.OfType<ApiEvent>();

        public void Publish(ApiEvent publishEvent)
        {
            this.messageAggregator.Collect(
                new QueuedPublishEvent
                {
                    EventToSend = publishEvent,
                    CommitClosure = () => Task.CompletedTask
                });
        }

        public void Send(ApiCommand sendCommand)
        {
            this.messageAggregator.Collect(
                new QueuedCommandToSend
                {
                    CommandToSend = sendCommand,
                    CommitClosure = () => Task.CompletedTask
                });
        }
    }
}