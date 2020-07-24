namespace Soap.Bus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    public class Bus : IBus
    {
        private readonly IBusInternal bus;

        private readonly IMessageAggregator messageAggregator;

        public Bus(IBusInternal bus, IMessageAggregator messageAggregator)
        {
            this.bus = bus;
            this.messageAggregator = messageAggregator;
        }

        public List<ApiCommand> CommandsSent { get; } = new List<ApiCommand>();

        public List<ApiEvent> EventsPublished { get; } = new List<ApiEvent>();

        public async Task CommitChanges()
        {

            await this.bus.CommitChanges();

            this.messageAggregator.AllMessages.OfType<IQueuedBusOperation>()
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
                            case QueuedPublishEvent e:
                                EventsPublished.Add(e.EventToPublish);
                                break;
                            default: throw new ArgumentOutOfRangeException();
                        }
                        });
        }

        public Task Publish(ApiEvent publishEvent)
        {
            publishEvent.Headers.EnsureRequiredHeaders();
            return this.bus.Publish(publishEvent);
        }

        public Task Send(ApiCommand sendCommand)
        {
            sendCommand.Headers.EnsureRequiredHeaders();
            return this.bus.Send(sendCommand);
        }
    }
}