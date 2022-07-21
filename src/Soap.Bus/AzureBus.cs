namespace Soap.Bus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Azure.Messaging.ServiceBus;
    using CircuitBoard;
    using CircuitBoard.MessageAggregator;
    using FluentValidation;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.SignalRService;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    public class AzureBus : IBusClient
    {
        private readonly IMessageAggregator messageAggregator;

        private readonly Settings settings;

        private readonly IAsyncCollector<SignalRMessage> signalRBinding;

        private static ServiceBusClient serviceBusClient;

        private static ConcurrentDictionary<string, ServiceBusSender> serviceBusSenders = new ConcurrentDictionary<string, ServiceBusSender>();
        
        
        private AzureBus(IMessageAggregator messageAggregator, Settings settings, IAsyncCollector<SignalRMessage> signalRBinding)
        {
            this.messageAggregator = messageAggregator;
            this.settings = settings;
            this.signalRBinding = signalRBinding;
            serviceBusClient ??= new ServiceBusClient(settings.BusConnectionString);
        }

        public List<ApiEvent> BusEventsPublished { get; } = new List<ApiEvent>();

        public List<ApiCommand> CommandsSent { get; } = new List<ApiCommand>();

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

        public async Task Publish(ApiEvent publishEvent, IBusClient.EventVisibilityFlags eventVisibility, Guid sessionId)
        {
            
            /* ORDER MATTERS, because we clear the session id on the Bus Broadcast, but it needs to be there for the WebSocketReply
             ideally we'd set the header here, but we need it to come through on the unitofwork item */

            if (eventVisibility.HasFlag(IBusClient.EventVisibility.ReplyToWebSocketSender))
            {
                await SendWsReply(publishEvent);
                WsEventsPublished.Add(publishEvent);
            }

            if (eventVisibility.HasFlag(IBusClient.EventVisibility.BroadcastToAllWebSocketClientsWithNoConversationId))
            {
                await SendWsBroadcast(publishEvent);
                WsEventsPublished.Add(publishEvent);
            }

            if (eventVisibility.HasFlag(IBusClient.EventVisibility.BroadcastToAllBusSubscriptions))
            {
                await BusBroadcastToAllSubscribers();
                BusEventsPublished.Add(publishEvent.Clone());
            }

            async Task SendWsReply(ApiEvent apiEvent)
            {
                await this.signalRBinding.AddAsync(
                    CreateNewSignalRMessage(apiEvent).Op(s => { s.ConnectionId = apiEvent.Headers.GetSessionId(); }));
            }

            async Task SendWsBroadcast(ApiEvent apiEvent) =>
                await this.signalRBinding.AddAsync(
                    CreateNewSignalRMessage(apiEvent).Op(s => { s.GroupName = this.settings.EnvironmentPartitionKey; }));

            async Task BusBroadcastToAllSubscribers()
            {
                var topic = publishEvent.Headers.GetTopic().ToLower();
                
                var sender = serviceBusSenders.GetOrAdd(topic, (key) => serviceBusClient.CreateSender(key));

                publishEvent.Headers.ClearSessionHeaders(); /* HACK bus messages don't use sessionId it's invalid
                this really should have been handled earlier another way, but its not the worst */
                var topicMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(publishEvent.ToJson(SerialiserIds.ApiBusMessage)))
                {
                    SessionId = sessionId.ToString(),
                    MessageId = publishEvent.Headers.GetMessageId()
                                            .ToString(), //* required for bus envelope but out code uses the matching header

                    Subject = publishEvent.GetType()
                                          .ToShortAssemblyTypeName(), //* required by clients for quick deserialisation rather than parsing JSON $type
                    ApplicationProperties =
                        { { nameof(this.settings.EnvironmentPartitionKey), this.settings.EnvironmentPartitionKey } }
                };

                await sender.SendMessageAsync(topicMessage);
            }

            static SignalRMessage CreateNewSignalRMessage(ApiEvent apiEvent)
            {
                return new SignalRMessage
                {
                    Target = "eventReceived", //client side function name
                    Arguments = new[] { "^^^" + apiEvent.ToJson(SerialiserIds.ApiBusMessage) }
                    /* don't let signalr do the serialising or it will use the wrong JSON settings, it's smart and it will recognise a JSON string, fool it with ^^^ */
                };
            }
        }

        public async Task Send(ApiCommand sendCommand, Guid sessionId, DateTimeOffset? scheduleAt = null)
        {
            var queueName = sendCommand.Headers.GetQueue();

            var sender = serviceBusSenders.GetOrAdd(queueName, (key) => serviceBusClient.CreateSender(key));
            
            var queueMessage = new ServiceBusMessage(Encoding.Default.GetBytes(sendCommand.ToJson(SerialiserIds.ApiBusMessage)))
            {
                MessageId = sendCommand.Headers.GetMessageId().ToString(),
                Subject = sendCommand.GetType()
                                     .ToShortAssemblyTypeName(), //* required by clients for quick deserialisation rather than parsing JSON $type
                CorrelationId = sendCommand.Headers.GetStatefulProcessId().ToString(),
                SessionId = sessionId.ToString()
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
            public Settings(
                byte numberOfApiMessageRetries,
                string busConnectionString,
                string resourceGroup,
                string busNamespace,
                string environmentPartitionKey)
            {
                NumberOfApiMessageRetries = numberOfApiMessageRetries;
                BusConnectionString = busConnectionString;
                ResourceGroup = resourceGroup;
                BusNamespace = busNamespace;
                EnvironmentPartitionKey = environmentPartitionKey;
            }

            public string BusConnectionString { get; set; }

            public string BusNamespace { get; set; }

            public string EnvironmentPartitionKey { get; set; }

            public byte NumberOfApiMessageRetries { get; set; }

            public string ResourceGroup { get; set; }

            public IBus CreateBus(
                IMessageAggregator messageAggregator,
                IBlobStorage blobStorage,
                IAsyncCollector<SignalRMessage> signalRBinding, Func<ServiceLevelAuthority> getServiceLevelAuthority, IBootstrapVariables bootstrapVariables) =>
                new Bus(new AzureBus(messageAggregator, this, signalRBinding), this, messageAggregator, blobStorage, getServiceLevelAuthority, bootstrapVariables);

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
