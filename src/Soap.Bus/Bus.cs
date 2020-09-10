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

        public Bus(IBusInternal bus, IBusSettings settings, IMessageAggregator messageAggregator)
        {
            this.bus = bus;
            this.messageAggregator = messageAggregator;
            MaximumNumberOfRetries = settings.NumberOfApiMessageRetries;
        }

        public List<ApiCommand> CommandsSent { get; } = new List<ApiCommand>();

        public List<ApiEvent> EventsPublished { get; } = new List<ApiEvent>();

        public byte MaximumNumberOfRetries { get; }

        public async Task CommitChanges()
        {
            var queuedMessages = this.messageAggregator.AllMessages.OfType<IQueuedBusOperation>()
                                     .Where(m => m.Committed == false)
                                     .ToList();
            foreach (var queuedMessage in queuedMessages)
            {
                switch (queuedMessage)
                {
                    case QueuedCommandToSend c:
                        await this.bus.Send(c.CommandToSend);
                        CommandsSent.Add(c.CommandToSend);
                        break;
                    case QueuedEventToPublish e:
                        await this.bus.Publish(e.EventToPublish);
                        EventsPublished.Add(e.EventToPublish);
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }

                queuedMessage.Committed = true;
            }
        }

        public Task Publish<T>(T publishEvent) where T : ApiEvent
        {
            publishEvent.Headers.EnsureRequiredHeaders();
            publishEvent.Headers.SetTopic(publishEvent.GetType().FullName);
            this.messageAggregator.Collect(
                new QueuedEventToPublish
                {
                    EventToPublish = publishEvent.Clone()
                });
            return Task.CompletedTask;
        }

        public Task Send<T>(T sendCommand) where T : ApiCommand
        {
            sendCommand.Headers.EnsureRequiredHeaders();
            sendCommand.Headers.SetQueue(sendCommand.GetType().Assembly.FullName);
            this.messageAggregator.Collect(
                new QueuedCommandToSend
                {
                    CommandToSend = sendCommand.Clone()
                });
            return Task.CompletedTask;
        }
    }
}