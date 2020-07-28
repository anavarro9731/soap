namespace Soap.Bus
{
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using Soap.Interfaces.Messages;

    public class InMemoryBus : IBusInternal
    {
        private readonly IMessageAggregator messageAggregator;

        public InMemoryBus(IMessageAggregator messageAggregator)
        {
            this.messageAggregator = messageAggregator;
        }

        public Task CommitChanges() => Task.CompletedTask;

        public Task Publish(ApiEvent publishEvent)
        {
            this.messageAggregator.Collect(
                new QueuedEventToPublish
                {
                    EventToPublish = publishEvent, CommitClosure = () => Task.CompletedTask
                });
            return Task.CompletedTask;
        }

        public Task Send(ApiCommand sendCommand)
        {
            this.messageAggregator.Collect(
                new QueuedCommandToSend
                {
                    CommandToSend = sendCommand, CommitClosure = () => Task.CompletedTask
                });
            return Task.CompletedTask;
        }
    }
}