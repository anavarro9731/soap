namespace Soap.Pf.BusContext
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;
    using Microsoft.Azure.ServiceBus;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;

    public class QueuedPublishEvent : IQueuedBusOperation
    {
        public Func<Task> CommitClosure { get; set; }

        public bool Committed { get; set; }

        public ApiEvent EventToSend { get; set; }
    }

    public interface IQueuedBusOperation : IQueuedStateChange
    {
    }

    public class QueuedCommandToSend : IQueuedBusOperation
    {
        public ApiCommand CommandToSend { get; set; }

        public Func<Task> CommitClosure { get; set; }

        public bool Committed { get; set; }
    }

    public class MessageBus : IBusContext
    {
        private readonly IMessageAggregator messageAggregator;

        //- TODO initialise 
        private readonly QueueClient queueClient = null;

        private readonly TopicClient topicClient = null;

        public MessageBus(IMessageAggregator messageAggregator)
        {
            this.messageAggregator = messageAggregator;
        }

        private List<IQueuedBusOperation> QueuedChanges
        {
            get
            {
                var queuedStateChanges = this.messageAggregator.AllMessages.OfType<IQueuedBusOperation>().Where(c => !c.Committed).ToList();
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

        public void Publish(ApiEvent publishEvent)
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
        }

        public void Send(ApiCommand sendCommand)
        {
            var queueMessage = new Message(Encoding.Default.GetBytes(JsonSerializer.Serialize(sendCommand)))
            {
                MessageId = sendCommand.MessageId.ToString(),
                Label = sendCommand.GetType().AssemblyQualifiedName,
                CorrelationId = sendCommand.StatefulProcessId.ToString()
            };

            this.messageAggregator.Collect(
                new QueuedCommandToSend
                {
                    CommandToSend = sendCommand,
                    CommitClosure = async () => await this.queueClient?.SendAsync(queueMessage)
                });
        }
    }
}

