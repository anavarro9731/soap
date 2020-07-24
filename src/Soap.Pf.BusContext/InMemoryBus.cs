namespace Soap.Bus
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    public class InMemoryBus : IBusInternal
    {
        private readonly IMessageAggregator messageAggregator;

        public InMemoryBus(IMessageAggregator messageAggregator)
        {
            this.messageAggregator = messageAggregator;
        }
            
        public Task CommitChanges()
        {
            return Task.CompletedTask;
        }

        public Task Publish(ApiEvent publishEvent)
        {
            this.messageAggregator.Collect(
                new QueuedPublishEvent
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