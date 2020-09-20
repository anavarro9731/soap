namespace Soap.Bus
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;
    
    
    public class Bus : IBus
    {
        private readonly IMessageAggregator messageAggregator;

        public Bus(IBusClient busClient, IBusSettings settings, IMessageAggregator messageAggregator)
        {
            this.BusClient = busClient;
            this.messageAggregator = messageAggregator;
            MaximumNumberOfRetries = settings.NumberOfApiMessageRetries;
        }

        public IBusClient BusClient { get; }

        public List<ApiCommand> CommandsSent => this.BusClient.CommandsSent;

        public List<ApiEvent> EventsPublished => this.BusClient.EventsPublished;

        public byte MaximumNumberOfRetries { get; }

        public async Task CommitChanges()
        {
            var queuedMessages = this.messageAggregator.AllMessages.OfType<IQueuedBusOperation>()
                                     .Where(m => m.Committed == false)
                                     .ToList();

            foreach (var queuedMessage in queuedMessages)
            {
                await queuedMessage.CommitClosure();
                queuedMessage.Committed = true; //* needs to be outside closure for unit testing, bare minimum I/O op inside
            }
        }

        public Task Publish<T>(T eventToPublish) where T : ApiEvent
        {
            eventToPublish = eventToPublish.Clone();
            eventToPublish.Headers.EnsureRequiredHeaders();
            eventToPublish.Headers.SetTopic(eventToPublish.GetType().FullName);
            /* All operations that modify the original message to get it ready must happen in the Publish and Send commands
             and they must happen before the command is "collected" because that final state of the message is then retried by
             the unit of work directly from the queuedmessage, publish is not called again. This is very important because
             you want to avoid any change whatsoever to the message on retries once it has become part of the unit of work 
             which is why we call the underlying busclient.publish rather than the bus.publish */
             
            this.messageAggregator.Collect(
                new QueuedEventToPublish
                {
                    EventToPublish = eventToPublish,
                    CommitClosure = async () => await this.BusClient.Publish(eventToPublish)
                });
            return Task.CompletedTask;
        }

        public Task Send<T>(T commandToSend) where T : ApiCommand
        {
            commandToSend = commandToSend.Clone();
            commandToSend.Headers.EnsureRequiredHeaders();
            commandToSend.Headers.SetQueueName(commandToSend.GetType().Assembly.FullName);
            /* All operations that modify the original message to get it ready must happen in the Publish and Send commands
             and they must happen before the command is "collected" because that final state of the message is then retried by
             the unit of work directly from the queuedmessage, publish is not called again. This is very important because
             you want to avoid any change whatsoever to the message on retries once it has become part of the unit of work 
             which is why we call the underlying busclient.publish rather than the bus.publish */

            this.messageAggregator.Collect(
                new QueuedCommandToSend
                {
                    CommandToSend = commandToSend, 
                    CommitClosure = async () => await this.BusClient.Send(commandToSend)
                });
            return Task.CompletedTask;
        }
    }
}