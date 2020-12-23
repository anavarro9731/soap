namespace Soap.Bus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Azure.Messaging.ServiceBus;
    using CircuitBoard;
    using CircuitBoard.MessageAggregator;
    using FluentValidation;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    public class AzureBus : IBusClient
    {
        private readonly IMessageAggregator messageAggregator;

        private readonly Settings settings;
        
        private AzureBus(IMessageAggregator messageAggregator, Settings settings)
        {
            this.messageAggregator = messageAggregator;
            this.settings = settings;
        }

        public List<ApiCommand> CommandsSent { get; } = new List<ApiCommand>();

        public List<ApiEvent> BusEventsPublished { get; } = new List<ApiEvent>();
        
        public List<ApiEvent> WsEventsPublished { get; } = new List<ApiEvent>();
        
        private List<IQueuedBusOperation> QueuedChanges
        {
            get
            {
                var queuedStateChanges = this.messageAggregator.AllMessages.OfType<IQueuedBusOperation>()
                                             .Where(c => !c.Committed)
                                             .ToList();
                return queuedStateChanges;
            }
        }

        public async Task Publish(ApiEvent publishEvent, IBusClient.EventVisibilityFlags eventVisibility)
        {
            await using var serviceBusClient = new ServiceBusClient(this.settings.BusConnectionString);

            if (eventVisibility.HasFlag(IBusClient.EventVisibility.ReplyToWebSocketSender))
            {
                await SendWsReply(publishEvent);
                WsEventsPublished.Add(publishEvent);
            } 
            if (eventVisibility.HasFlag(IBusClient.EventVisibility.BroadcastToAllWebSocketClientsWithNoConversationId))
            {
                await SendWsReply(publishEvent);
                WsEventsPublished.Add(publishEvent);
            }

            if (eventVisibility.HasFlag(IBusClient.EventVisibility.BroadcastToAllBusSubscriptions))
            {
                await BusBroadcastToAllSubscribers(serviceBusClient);
                BusEventsPublished.Add(publishEvent.Clone());
            }

            async Task SendWsReply(ApiEvent apiEvent)
            {
                using var client = new HttpClient();
                //TODO config and secure sendsignalrmessage
                var sendWsMessageEnpoint = !string.IsNullOrWhiteSpace(publishEvent.Headers.GetSessionId())
                                               ? $"http://localhost:7071/api/SendSignalRMessage?connectionId={Uri.EscapeUriString(apiEvent.Headers.GetSessionId())}&type={Uri.EscapeUriString(ToShortAssemblyTypeName(apiEvent.GetType()))}"
                                               : $"http://localhost:7071/api/SendSignalRMessage?type={Uri.EscapeUriString(ToShortAssemblyTypeName(apiEvent.GetType()))}";

                HttpResponseMessage result = await client.PostAsync(
                                                 sendWsMessageEnpoint,
                                                 new StringContent(apiEvent.ToJson(SerialiserIds.ApiBusMessage)));
                
                static string ToShortAssemblyTypeName(Type t) => $"{t.FullName}, {t.Assembly.GetName().Name}";
            }
            
            async Task BusBroadcastToAllSubscribers(ServiceBusClient serviceBusClient)
            {
                var topic = publishEvent.Headers.GetTopic().ToLower();
                var sender = serviceBusClient.CreateSender(topic);

                var topicMessage =
                    new ServiceBusMessage(Encoding.UTF8.GetBytes(publishEvent.ToJson(SerialiserIds.ApiBusMessage)))
                    {
                        MessageId = publishEvent.Headers.GetMessageId().ToString(), //* required for bus envelope but out code uses the matching header  
                        Subject = publishEvent.GetType().ToShortAssemblyTypeName(), //* required by clients for quick deserialisation rather than parsing JSON $type
                    };
                
                await sender.SendMessageAsync(topicMessage);
            }
            
        }

        public async Task Send(ApiCommand sendCommand, DateTimeOffset? scheduleAt = null)
        {
            await using var client =  new ServiceBusClient(this.settings.BusConnectionString);

            var sender = client.CreateSender(sendCommand.Headers.GetQueue());
            
            var queueMessage = new ServiceBusMessage(Encoding.Default.GetBytes(sendCommand.ToJson(SerialiserIds.ApiBusMessage)))
            {
                MessageId = sendCommand.Headers.GetMessageId().ToString(),
                Subject = sendCommand.GetType().ToShortAssemblyTypeName(), //* required by clients for quick deserialisation rather than parsing JSON $type
                CorrelationId = sendCommand.Headers.GetStatefulProcessId().ToString(),
            };
            
            if (scheduleAt.HasValue)
            {
                var sequenceNumber = await sender.ScheduleMessageAsync(queueMessage, scheduleAt.Value);
            }
            else
            {
                await sender.SendMessageAsync(queueMessage);
            }

            CommandsSent.Add(sendCommand.Clone());
        }

        public class Settings : IBusSettings
        {
            public Settings(byte numberOfApiMessageRetries, string busConnectionString, string resourceGroup, string busNamespace)
            {
                NumberOfApiMessageRetries = numberOfApiMessageRetries;
                BusConnectionString = busConnectionString;
                ResourceGroup = resourceGroup;
                BusNamespace = busNamespace;
            }

            public string BusConnectionString { get; set; }

            public string BusNamespace { get; set; }

            public byte NumberOfApiMessageRetries { get; set; }

            public string ResourceGroup { get; set; }

            public IBus CreateBus(IMessageAggregator messageAggregator, IBlobStorage blobStorage) =>
                new Bus(new AzureBus(messageAggregator, this), this, messageAggregator, blobStorage);

            public class Validator : AbstractValidator<Settings>
            {
                public Validator()
                {
                    RuleFor(x => x.BusConnectionString).NotEmpty();
                    RuleFor(x => x.NumberOfApiMessageRetries).GreaterThanOrEqualTo(x => 0);
                }
            }
        }
    }
}
