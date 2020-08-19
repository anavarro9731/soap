namespace Soap.Bus
{
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

    public class AzureBus : IBusInternal
    {
        private readonly IMessageAggregator messageAggregator;

        private readonly QueueClient queueClient = null;

        private readonly TopicClient topicClient = null;

        public AzureBus(IMessageAggregator messageAggregator, Settings settings)
        {
            this.messageAggregator = messageAggregator;
            //- TODO initialise  clients from settings
        }

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

        public async Task CommitChanges()
        {
            foreach (var queuedBusOperation in QueuedChanges)
            {
                await queuedBusOperation.CommitClosure();
                queuedBusOperation.Committed = true;
            }
        }

        public Task Publish(ApiEvent publishEvent)
        {
            var queueMessage = new Message(Encoding.Default.GetBytes(JsonConvert.SerializeObject(publishEvent)))
            {
                MessageId = publishEvent.Headers.GetMessageId().ToString(), Label = publishEvent.GetType().AssemblyQualifiedName
            };

            this.messageAggregator.Collect(
                new QueuedEventToPublish
                {
                    EventToPublish = publishEvent,
                    CommitClosure = async () => await this.topicClient?.SendAsync(queueMessage)
                });
            return Task.CompletedTask;
        }

        public Task Send(ApiCommand sendCommand)
        {
            var queueMessage = new Message(Encoding.Default.GetBytes(JsonConvert.SerializeObject(sendCommand)))
            {
                MessageId = sendCommand.Headers.GetMessageId().ToString(),
                Label = sendCommand.GetType().AssemblyQualifiedName,
                CorrelationId = sendCommand.Headers.GetStatefulProcessId().ToString()
            };

            this.messageAggregator.Collect(
                new QueuedCommandToSend
                {
                    CommandToSend = sendCommand, CommitClosure = async () => await this.queueClient?.SendAsync(queueMessage)
                });
            return Task.CompletedTask;
        }

        public class Settings : IBusSettings
        {
            public Settings(byte numberOfApiMessageRetries, string queueConnectionString)
            {
                this.NumberOfApiMessageRetries = numberOfApiMessageRetries;
                this.QueueConnectionString = queueConnectionString;
            }

            public byte NumberOfApiMessageRetries { get; set; }

            public string QueueConnectionString { get; set; }

            public class Validator : AbstractValidator<Settings>
            {
                public Validator()
                {
                    RuleFor(x => x.QueueConnectionString).NotEmpty();
                    RuleFor(x => x.NumberOfApiMessageRetries).GreaterThanOrEqualTo(x => 0);
                }
            }

            public IBus CreateBus(IMessageAggregator messageAggregator) => new Bus(new AzureBus(messageAggregator, this), this, messageAggregator);
        }
    }


}