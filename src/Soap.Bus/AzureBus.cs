namespace Soap.Bus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        public List<ApiEvent> EventsPublished { get; } = new List<ApiEvent>();

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

        public async Task Publish(ApiEvent publishEvent, EnumerationFlags eventVisibility)
        {
            await using var serviceBusClient = new ServiceBusClient(this.settings.BusConnectionString);
            
            if (eventVisibility.HasFlag(IBusClient.EventVisibility.WebSocketSender))
            {
                await SendReplyToWsClient(serviceBusClient);    
            }

            if (eventVisibility.HasFlag(IBusClient.EventVisibility.AllBusSubscriptions))
            {
                await BusBroadcastToAllSubscribers(serviceBusClient);
            }

            if (eventVisibility.HasFlag(IBusClient.EventVisibility.AllWebSocketClientsNoConversationId))
            {
                await WsBroadCastToAllSubscribers();
            }

            EventsPublished.Add(publishEvent.Clone());

            async Task WsBroadCastToAllSubscribers()
            {
                //TODO
                await Task.CompletedTask;
            }

            async Task SendReplyToWsClient(ServiceBusClient serviceBusClient)
            {
                var sender = serviceBusClient.CreateSender("allevents");

                var broadCastMessage =
                    new ServiceBusMessage(Encoding.UTF8.GetBytes(publishEvent.ToJson(SerialiserIds.ApiBusMessage)))
                    {
                        MessageId = publishEvent.Headers.GetMessageId().ToString(), //* required for bus envelope but out code uses the matching header  
                        Subject = publishEvent.GetType().ToShortAssemblyTypeName(), //* required by .net clients for quick deserialisation rather than parsing JSON $type
                        SessionId = publishEvent.Headers.GetSessionId()
                    };
                
                await sender.SendMessageAsync(broadCastMessage);
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

        public Task Publish(ApiEvent publishEvent, IBusClient.EventVisibility sendTo) => throw new NotImplementedException();

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
