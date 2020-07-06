namespace Soap.Bus
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class InMemoryBus : IBus
    {
        private readonly IMessageAggregator messageAggregator;

        public InMemoryBus(IMessageAggregator messageAggregator)
        {
            this.messageAggregator = messageAggregator;
        }

        public IEnumerable<ApiCommand> Commands => this.messageAggregator.AllMessages.OfType<QueuedCommandToSend>().Select(x => x.CommandToSend);

        public IEnumerable<ApiEvent> Events => this.messageAggregator.AllMessages.OfType<QueuedPublishEvent>().Select(x => x.EventToSend);

        public Task CommitChanges()
        {
            //* TODO
            return Task.CompletedTask;
        }

        public Task Publish(ApiEvent publishEvent)
        {
            this.messageAggregator.Collect(
                new QueuedPublishEvent
                {
                    EventToSend = publishEvent,
                    CommitClosure = () => Task.CompletedTask
                });
            return Task.CompletedTask;
        }

        public Task Send(ApiCommand sendCommand)
        {
            this.messageAggregator.Collect(
                new QueuedCommandToSend
                {
                    CommandToSend = sendCommand,
                    CommitClosure = () => Task.CompletedTask
                });
            return Task.CompletedTask;
        }

        
    }
}