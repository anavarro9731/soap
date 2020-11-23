﻿namespace Soap.Bus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using FluentValidation;
    using Microsoft.Azure.ServiceBus;
    using Newtonsoft.Json;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility;
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

        public string ClientSideSessionId { get; set; }
        
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

        public async Task Publish(ApiEvent publishEvent)
        {
            await SendTopicMessage();
            await SendBroadCastMessage();
            
            EventsPublished.Add(publishEvent.Clone());

            async Task SendBroadCastMessage()
            {
                var broadCastMessage = new Message(Encoding.Default.GetBytes(publishEvent.ToJson(SerialiserIds.ApiBusMessage)))
                {
                    MessageId = publishEvent.Headers.GetMessageId().ToString(), //* required for bus envelope but out code uses the matching header  
                    Label = publishEvent.GetType().ToShortAssemblyTypeName(), //* required by clients for quick deserialisation rather than parsing JSON $type
                    SessionId = publishEvent.Headers.GetSessionId()
                };
                var broadcastClient = new TopicClient(this.settings.BusConnectionString, "allevents");
                await broadcastClient.SendAsync(broadCastMessage);
            }
            
            async Task SendTopicMessage()
            {
                var topicMessage = new Message(Encoding.Default.GetBytes(publishEvent.ToJson(SerialiserIds.ApiBusMessage)))
                {
                    MessageId = publishEvent.Headers.GetMessageId().ToString(), //* required for bus envelope but out code uses the matching header  
                    Label = publishEvent.GetType().ToShortAssemblyTypeName(), //* required by clients for quick deserialisation rather than parsing JSON $type
                };

                var topic = publishEvent.Headers.GetTopic().ToLower();
                var topicClient = new TopicClient(this.settings.BusConnectionString, topic);
                await topicClient.SendAsync(topicMessage);
            }
            
        }

        public async Task Send(ApiCommand sendCommand, DateTimeOffset? scheduleAt = null)
        {
            var queueMessage = new Message(Encoding.Default.GetBytes(sendCommand.ToJson(SerialiserIds.ApiBusMessage)))
            {
                MessageId = sendCommand.Headers.GetMessageId().ToString(),
                Label = sendCommand.GetType().ToShortAssemblyTypeName(), //* required by clients for quick deserialisation rather than parsing JSON $type
                CorrelationId = sendCommand.Headers.GetStatefulProcessId().ToString(),
            };

            var queueClient = new QueueClient(this.settings.BusConnectionString, sendCommand.Headers.GetQueue());

            if (scheduleAt.HasValue)
            {
                var sequenceNumber = await queueClient.ScheduleMessageAsync(queueMessage, scheduleAt.Value);
            }
            else
            {
                await queueClient.SendAsync(queueMessage);
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