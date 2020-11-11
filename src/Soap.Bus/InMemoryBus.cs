namespace Soap.Bus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class InMemoryBus : IBusClient
    {
        private readonly Settings settings;

        private InMemoryBus(Settings settings)
        {
            this.settings = settings;
        }

        public List<ApiCommand> CommandsSent { get; } = new List<ApiCommand>();

        public List<ApiEvent> EventsPublished { get; } = new List<ApiEvent>();

        public Task Publish(ApiEvent publishEvent)
        {
            EventsPublished.Add(publishEvent);
            return Task.CompletedTask;
        }

        public Task Send(ApiCommand sendCommand, DateTimeOffset? scheduledAt = null)
        {
            CommandsSent.Add(sendCommand);
            return Task.CompletedTask;
        }

        public class Settings : IBusSettings
        {
            public byte NumberOfApiMessageRetries { get; set; }

            public IBus CreateBus(IMessageAggregator messageAggregator, IBlobStorage blobStorage) =>
                new Bus(new InMemoryBus(this), this, messageAggregator, blobStorage);
        }
    }
}