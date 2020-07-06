namespace Soap.Bus
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using Microsoft.Azure.ServiceBus;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class Bus : IBus
    {
        private readonly IMessageAggregator messageAggregator;

        private readonly QueueClient queueClient = null;

        private readonly TopicClient topicClient = null;

        public Bus(IMessageAggregator messageAggregator, BusSettings settings)
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
            var queueMessage = new Message(Encoding.Default.GetBytes(JsonSerializer.Serialize(publishEvent)))
            {
                MessageId = publishEvent.MessageId.ToString(),
                Label = publishEvent.GetType().AssemblyQualifiedName
            };

            this.messageAggregator.Collect(
                new QueuedPublishEvent
                {
                    EventToSend = publishEvent,
                    CommitClosure = async () => await this.topicClient?.SendAsync(queueMessage)
                });
            return Task.CompletedTask;
        }

        public Task Send(ApiCommand sendCommand)
        {
            var queueMessage = new Message(Encoding.Default.GetBytes(JsonSerializer.Serialize(sendCommand)))
            {
                MessageId = sendCommand.MessageId.ToString(),
                Label = sendCommand.GetType().AssemblyQualifiedName,
                CorrelationId = sendCommand.Headers.GetStatefulProcessId().ToString() 
            };
            
            this.messageAggregator.Collect(
                new QueuedCommandToSend
                {
                    CommandToSend = sendCommand,
                    CommitClosure = async () => await this.queueClient?.SendAsync(queueMessage)
                });
            return Task.CompletedTask;
        }
    }
}