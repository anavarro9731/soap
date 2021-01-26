namespace Soap.Bus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CircuitBoard;
    using CircuitBoard.MessageAggregator;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.SignalRService;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    public class InMemoryBus : IBusClient
    {
        private readonly Settings settings;

        private InMemoryBus(Settings settings)
        {
            this.settings = settings;
        }

        public List<ApiCommand> CommandsSent { get; } = new List<ApiCommand>();

        public List<ApiEvent> BusEventsPublished { get; } = new List<ApiEvent>();

        public List<ApiEvent> WsEventsPublished { get; } = new List<ApiEvent>(); //TODO write a test

        public Task Publish(ApiEvent publishEvent, IBusClient.EventVisibilityFlags eventVisibility)
        {
            if (eventVisibility.HasFlag(IBusClient.EventVisibility.ReplyToWebSocketSender))
            {
                WsEventsPublished.Add(publishEvent.Clone());
            } 
            if (eventVisibility.HasFlag(IBusClient.EventVisibility.BroadcastToAllWebSocketClientsWithNoConversationId))
            {
                WsEventsPublished.Add(publishEvent.Clone());
            }

            if (eventVisibility.HasFlag(IBusClient.EventVisibility.BroadcastToAllBusSubscriptions))
            {
                BusEventsPublished.Add(publishEvent.Clone());
            }

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

            public string EnvironmentPartitionKey { get; set; }

            public IBus CreateBus(IMessageAggregator messageAggregator, IBlobStorage blobStorage, IAsyncCollector<SignalRMessage> signalRBinding, Func<Task<ServiceLevelAuthority>> getServiceLevelAuthority) =>
                new Bus(new InMemoryBus(this), this, messageAggregator, blobStorage, getServiceLevelAuthority);
        }
    }
}
