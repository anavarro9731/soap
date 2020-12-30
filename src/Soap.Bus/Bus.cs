namespace Soap.Bus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using CircuitBoard;
    using CircuitBoard.MessageAggregator;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.SignalRService;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    public class Bus : IBus
    {
        private readonly IBlobStorage blobStorage;

        private readonly IMessageAggregator messageAggregator;

        private string envPartitionKey;
        
        public Bus(
            IBusClient busClient,
            IBusSettings settings,
            IMessageAggregator messageAggregator,
            IBlobStorage blobStorage)
        {
            BusClient = busClient;
            this.messageAggregator = messageAggregator;
            this.blobStorage = blobStorage;
            MaximumNumberOfRetries = settings.NumberOfApiMessageRetries;
            this.envPartitionKey = settings.EnvironmentPartitionKey;

        }

        public IBusClient BusClient { get; }

        public List<ApiCommand> CommandsSent => BusClient.CommandsSent;

        public List<ApiEvent> BusEventsPublished => BusClient.BusEventsPublished;
        
        public List<ApiEvent> WsEventsPublished => BusClient.WsEventsPublished;

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

        public async Task Publish<T, Tm>(T eventToPublish, Tm contextMessage, IBusClient.EventVisibilityFlags eventVisibility = null)
            where T : ApiEvent where Tm : ApiMessage
        {
            eventVisibility ??= GetDefaultVisibility(contextMessage);

            if (eventVisibility.HasFlag(IBusClient.EventVisibility.ReplyToWebSocketSender))
            {
                if (string.IsNullOrEmpty(contextMessage.Headers.GetSessionId()))
                {
                    throw new ApplicationException(
                        "Outgoing message is set to reply to web socket sender, but the sender has not set a sessionId");
                }
                //* transfer from incoming command to outgoing event for websocket clients
                eventToPublish.Headers.SetSessionId(contextMessage.Headers.GetSessionId());
                eventToPublish.Headers.SetCommandHash(contextMessage.Headers.GetCommandHash());
                eventToPublish.Headers.SetCommandConversationId(contextMessage.Headers.GetCommandConversationId().Value);
            }
            
            eventToPublish.Validate();
            eventToPublish = eventToPublish.Clone(); //* i think this was so no client can modify it afterwards
            eventToPublish.Headers.SetAndCheckHeadersOnOutgoingEvent(eventToPublish);
            //* make all checks first
            await IfLargeMessageSaveToBlobStorage(eventToPublish);

            /* All operations that modify the original message to get it ready must happen in the Publish and Send commands
             and they must happen before the command is "collected" because that final state of the message is then retried by
             the unit of work directly from the queuedmessage, publish is not called again. This is very important because
             you want to avoid any change whatsoever to the message on retries once it has become part of the unit of work 
             which is why we call the underlying busclient.publish rather than the bus.publish */

            this.messageAggregator.Collect(
                new QueuedEventToPublish
                {
                    EventVisibility = eventVisibility,
                    EventToPublish = eventToPublish,
                    CommitClosure = async () => await BusClient.Publish(eventToPublish, eventVisibility)
                });
            
            static IBusClient.EventVisibilityFlags GetDefaultVisibility(ApiMessage contextMessage)
            {
                var eventVisibility = new IBusClient.EventVisibilityFlags();

                if (!string.IsNullOrEmpty(contextMessage.Headers.GetSessionId()))
                {
                    eventVisibility.AddFlag(IBusClient.EventVisibility.ReplyToWebSocketSender);
                }

                eventVisibility.AddFlag(IBusClient.EventVisibility.BroadcastToAllBusSubscriptions);

                return eventVisibility;
            }
        }

        public async Task Send<T>(T commandToSend) where T : ApiCommand
        {
            commandToSend.Validate();
            commandToSend = commandToSend.Clone();
            commandToSend.Headers.SetAndCheckHeadersOnOutgoingCommand(commandToSend, this.envPartitionKey);
            //* make all checks first
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
                    SetBlobIdAndSasStorageTokenHeader();
                    await this.blobStorage.SaveApiMessageAsBlob(message);
                    ClearAllPublicPropertyValuesExceptHeaders();
                }
            }

            void SetBlobIdAndSasStorageTokenHeader()
            {
                var blobId = Guid.NewGuid();
                message.Headers.SetBlobId(blobId);

                var sasStorageToken = this.blobStorage.GetStorageSasTokenForBlob(
                    blobId,
                    new EnumerationFlags(IBlobStorage.BlobSasPermissions.ReadAndDelete));
                message.Headers.SetSasStorageToken(sasStorageToken);
            }

            bool MessageIsTooBigForServiceBus() => MessageSizeInBytes() > 256000; //* servicebus max size is 256KB

            void ClearAllPublicPropertyValuesExceptHeaders()
            {
                var publicProperties = message.GetType()
                                              .GetProperties()
                                              .Where(
                                                  p => p.Name != nameof(ApiMessage.Headers) && p.CanRead && p.CanWrite
                                                       && (p.MemberType == MemberTypes.Property
                                                           || p.MemberType == MemberTypes.Field));

                foreach (var publicProperty in publicProperties) publicProperty.SetValue(message, null);
            }

            int MessageSizeInBytes()
            {
                var json = message.ToJson(SerialiserIds.ApiBusMessage);
                var byteCount = Encoding.UTF8.GetByteCount(json);
                var indexingAndOtherExtras = Convert.ToInt32(byteCount / 0.9M);
                return indexingAndOtherExtras;
            }
        }
    }
}
