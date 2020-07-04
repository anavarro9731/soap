namespace Soap.DomainTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using Soap.Bus;
    using Soap.Interfaces;

    public class InMemoryBus : IBus
    {
        private readonly IMessageAggregator messageAggregator;

        public InMemoryBus(IMessageAggregator messageAggregator)
        {
            this.messageAggregator = messageAggregator;
        }

        public IEnumerable<ApiCommand> Commands => this.messageAggregator.AllMessages.OfType<ApiCommand>();

        public IEnumerable<ApiEvent> Events => this.messageAggregator.AllMessages.OfType<ApiEvent>();

        public Task CommitChanges()
        {
            //* TODO
            return Task.CompletedTask;
        }

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