namespace Soap.Bus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using Newtonsoft.Json;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    public class Bus : IBus
    {
        private readonly IBlobStorage blobStorage;

        private readonly IMessageAggregator messageAggregator;

        public Bus(IBusClient busClient, IBusSettings settings, IMessageAggregator messageAggregator, IBlobStorage blobStorage)
        {
            BusClient = busClient;
            this.messageAggregator = messageAggregator;
            this.blobStorage = blobStorage;
            MaximumNumberOfRetries = settings.NumberOfApiMessageRetries;
        }

        public IBusClient BusClient { get; }

        public List<ApiCommand> CommandsSent => BusClient.CommandsSent;

        public List<ApiEvent> EventsPublished => BusClient.EventsPublished;

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

        public async Task Publish<T>(T eventToPublish) where T : ApiEvent
        {
            eventToPublish = eventToPublish.Clone();
            eventToPublish.Headers.SetAndCheckHeadersOnOutgoingEvent(eventToPublish);
            await IfLargeMessageSaveToBlobStorage(eventToPublish);
            /* All operations that modify the original message to get it ready must happen in the Publish and Send commands
             and they must happen before the command is "collected" because that final state of the message is then retried by
             the unit of work directly from the queuedmessage, publish is not called again. This is very important because
             you want to avoid any change whatsoever to the message on retries once it has become part of the unit of work 
             which is why we call the underlying busclient.publish rather than the bus.publish */

            this.messageAggregator.Collect(
                new QueuedEventToPublish
                {
                    EventToPublish = eventToPublish,
                    CommitClosure = async () => await BusClient.Publish(eventToPublish)
                });
        }

        public async Task Send<T>(T commandToSend) where T : ApiCommand
        {
            commandToSend = commandToSend.Clone();
            commandToSend.Headers.SetAndCheckHeadersOnOutgoingCommand(commandToSend);
            await IfLargeMessageSaveToBlobStorage(commandToSend);
            /* All operations that modify the original message to get it ready must happen in the Publish and Send commands
             and they must happen before the command is "collected" because that final state of the message is then retried by
             the unit of work directly from the queuedmessage, publish is not called again. This is very important because
             you want to avoid any change whatsoever to the message on retries once it has become part of the unit of work 
             which is why we call the underlying busclient.publish rather than the bus.publish */

            this.messageAggregator.Collect(
                new QueuedCommandToSend
                {
                    CommandToSend = commandToSend,
                    CommitClosure = async () => await BusClient.Send(commandToSend)
                });
        }

        private async Task IfLargeMessageSaveToBlobStorage<T>(T message) where T : ApiMessage
        {
            {
                if (MessageIsTooBigForServiceBus())
                {
                    await this.blobStorage.SaveApiMessageAsBlob(message);
                    ClearAllPublicPropertyValues();
                    SetBlobIdHeader();
                }
            }

            void SetBlobIdHeader()
            {
                var blobId = message.Headers.GetMessageId(); //* for messages use the message id as the blob id
                message.Headers.SetBlobId(blobId);
            }

            bool MessageIsTooBigForServiceBus() => MessageSizeInBytes() > 256000; //* servicebus max size is 256KB

            void ClearAllPublicPropertyValues()
            {
                var publicProperties = message.GetType()
                                              .GetProperties()
                                              .Where(
                                                  p => p.CanRead && p.CanWrite
                                                                 && (p.MemberType == MemberTypes.Property
                                                                     || p.MemberType == MemberTypes.Field));

                foreach (var publicProperty in publicProperties) publicProperty.SetValue(message, null);
            }

            int MessageSizeInBytes()
            {
                var json = JsonConvert.SerializeObject(message);
                var byteCount = Encoding.UTF8.GetByteCount(json);
                var indexingAndOtherExtras = Convert.ToInt32(byteCount / 0.9M);
                return indexingAndOtherExtras;
            }
        }
    }
}