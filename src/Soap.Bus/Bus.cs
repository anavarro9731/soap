namespace Soap.Bus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class Bus : IBus
    {
        private readonly IBusInternal bus;

        private readonly IMessageAggregator messageAggregator;

        public Bus(IBusInternal bus, IBusSettings settings, IMessageAggregator messageAggregator)
        {
            this.bus = bus;
            this.messageAggregator = messageAggregator;
            this.MaximumNumberOfRetries = settings.NumberOfApiMessageRetries;
        }

        public byte MaximumNumberOfRetries { get; }

        public List<ApiCommand> CommandsSent { get; } = new List<ApiCommand>();

        public List<ApiEvent> EventsPublished { get; } = new List<ApiEvent>();

        public async Task CommitChanges()
        {
            this.messageAggregator.AllMessages.OfType<IQueuedBusOperation>()
                .Where(m => m.Committed == false)
                .ToList()
                .ForEach(
                    m =>
                        {
                        m.Committed = true;
                        switch (m)
                        {
                            case QueuedCommandToSend c:
                                CommandsSent.Add(c.CommandToSend); //TODO: clone to prevent changes, need specific type?
                                break;
                            case QueuedEventToPublish e:
                                EventsPublished.Add(e.EventToPublish);
                                break;
                            default: throw new ArgumentOutOfRangeException();
                        }
                        });
            //TODO call bus
            
            await Task.Delay(0);
        }

        public Task Publish(ApiEvent publishEvent)
        {
            publishEvent.Headers.EnsureRequiredHeaders();
            this.messageAggregator.Collect(
                new QueuedEventToPublish
                {
                    EventToPublish = publishEvent, CommitClosure = () => Task.CompletedTask
                });
            return Task.CompletedTask;
        }

        public Task Send(ApiCommand sendCommand)
        {
            sendCommand.Headers.EnsureRequiredHeaders();
            this.messageAggregator.Collect(
                new QueuedCommandToSend
                {
                    CommandToSend = sendCommand, CommitClosure = () => Task.CompletedTask
                });
            return Task.CompletedTask;
        }
    }
}