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
    using Serilog;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;

    public class Bus : IBus
    {
        private readonly IBlobStorage blobStorage;

        private readonly IBootstrapVariables bootstrapVariables;

        private readonly string envPartitionKey;

        private readonly ServiceLevelAuthority serviceLevelAuthority;
        
        private readonly IMessageAggregator messageAggregator;
        
        public Bus(
            IBusClient busClient,
            IBusSettings settings,
            IMessageAggregator messageAggregator,
            IBlobStorage blobStorage,
            Func<ServiceLevelAuthority> getServiceLevelAuthority,
            IBootstrapVariables bootstrapVariables)
        {
            BusClient = busClient;
            this.messageAggregator = messageAggregator;
            this.blobStorage = blobStorage;
            this.bootstrapVariables = bootstrapVariables;
            this.serviceLevelAuthority = getServiceLevelAuthority();
            MaximumNumberOfRetries = settings.NumberOfApiMessageRetries;
            this.envPartitionKey = settings.EnvironmentPartitionKey;
        }

        public IBusClient BusClient { get; }

        public List<ApiEvent> BusEventsPublished => BusClient.BusEventsPublished;

        public List<ApiCommand> CommandsSent => BusClient.CommandsSent;

        public byte MaximumNumberOfRetries { get; }

        public List<ApiEvent> WsEventsPublished => BusClient.WsEventsPublished;

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

        public async Task Publish<TEvent, TContextMessage>(
            TEvent eventToPublish,
            TContextMessage contextMessage,
            IBusClient.EventVisibilityFlags eventVisibility = null) where TEvent : ApiEvent where TContextMessage : ApiMessage
        {
            eventToPublish.Validate();
            eventToPublish.RequiredNotNullOrThrow();
            eventToPublish = eventToPublish.Clone(); //* i think this was so no client can modify it afterwards
            eventVisibility ??= GetDefaultVisibility(contextMessage);

            if (eventVisibility.HasFlag(IBusClient.EventVisibility.ReplyToWebSocketSender))
            {
                if (string.IsNullOrEmpty(contextMessage.Headers.GetSessionId()))
                {
                    /* Outgoing message is set to reply to web socket sender, but the sender has not set a sessionId
                     originally i was throwing an error here, but sometimes you want to send the same command from webclient
                     and another service and then you will get this problem. because this is basically an infrastructure concern
                     , as in apart from the Bus.Publish statement no other business logic can cause this problem, i think we
                     can probably adopt an approach of just ignoring messages like this and writing a log entry */
                    Log.Logger.Warning("Outgoing message was set to reply to web socket sender, but the sender has not set a sessionId, ignoring");
                    eventVisibility.RemoveFlag(IBusClient.EventVisibility.ReplyToWebSocketSender);
                }
                else
                {
                    //* transfer from incoming command to outgoing event for websocket clients
                    eventToPublish.Headers.SetSessionId(contextMessage.Headers.GetSessionId());
                    eventToPublish.Headers.SetCommandHash(contextMessage.Headers.GetCommandHash());
                    eventToPublish.Headers.SetCommandConversationId(contextMessage.Headers.GetCommandConversationId().Value);    
                }
            }

            eventToPublish.Validate();
            eventToPublish.RequiredNotNullOrThrow();
            
            SetPermissionsOnOutgoingMessage(eventToPublish, contextMessage, true /* event always published with service level authority */);
            
            eventToPublish.Headers.SetTimeOfCreationAtOrigin();
            eventToPublish.Headers.SetMessageId(Guid.NewGuid());
            eventToPublish.Headers.SetTopic(eventToPublish.GetType().FullName);
            eventToPublish.Headers.SetSchema(eventToPublish.GetType().FullName);
            eventToPublish.Headers.CheckHeadersOnOutgoingEvent(eventToPublish,  this.bootstrapVariables.AuthLevel.AuthenticationRequired, !eventToPublish.GetType().HasAttribute<AuthorisationNotRequired>());

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

        public async Task Send<TCommand, TContextMessage>(
            TCommand commandToSend,
            TContextMessage contextMessage,
            bool forceServiceLevelAuthority = false,
            DateTimeOffset scheduledAt = default) where TCommand : ApiCommand where TContextMessage : ApiMessage
        {
            commandToSend.Validate();
            commandToSend.RequiredNotNullOrThrow();
            commandToSend = commandToSend.Clone();
            
            SetPermissionsOnOutgoingMessage(commandToSend, contextMessage, forceServiceLevelAuthority);
            
            commandToSend.Headers.SetTimeOfCreationAtOrigin();
            commandToSend.Headers.SetMessageId(Guid.NewGuid());
            commandToSend.Headers.CheckHeadersOnOutgoingCommand(commandToSend, this.bootstrapVariables.AuthLevel.AuthenticationRequired, !commandToSend.GetType().HasAttribute<AuthorisationNotRequired>(), this.envPartitionKey);
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
                    CommitClosure = async () => await BusClient.Send(commandToSend, scheduledAt)
                });
        }

        private void SetPermissionsOnOutgoingMessage<TMessage, TContextMessage>(
            TMessage commandToSend,
            TContextMessage contextMessage,
            bool forceServiceLevelAuthority) where TMessage : ApiMessage where TContextMessage : ApiMessage
        {
            if (this.bootstrapVariables.AuthLevel.AuthenticationRequired)
            {
                switch (forceServiceLevelAuthority)
                {
                    case true:
                        //* set identity chain
                        var currentChain = contextMessage.Headers.GetIdentityChain();
                        commandToSend.Headers.SetIdentityChain(
                            currentChain switch
                            {
                                null => this.serviceLevelAuthority
                                            .IdentityChainSegment, //* context message could be an event, or auth disabled and not have a chain, etc
                                _ => $"{currentChain},{this.serviceLevelAuthority.IdentityChainSegment}"
                            });
                        commandToSend.Headers.SetAccessToken(this.serviceLevelAuthority.AccessToken);
                        //* set identity token
                        commandToSend.Headers.SetIdentityToken(this.serviceLevelAuthority.IdentityToken);
                        break;

                    case false:

                        /* there are a variety of cases where all of the following could be blank because the context message headers are blank, if they are blank (and they should all be blank or filled)
                        then we use the Sla instead if the flag is set to allow that, the only time this would really make sense without the flag set, would be when a message marked with 
                        [AuthorisationNotRequired] sent a command also marked with [AuthorisationNotRequired] */
                        var useSlaWhenCurrentContextMissing = this.bootstrapVariables.UseServiceLevelAuthorityInTheAbsenceOfASecurityContext;
                        commandToSend.Headers.SetIdentityChain(
                            contextMessage.Headers.GetIdentityChain()
                            ?? (useSlaWhenCurrentContextMissing ? this.serviceLevelAuthority.IdentityChainSegment : null));
                        commandToSend.Headers.SetAccessToken(
                            contextMessage.Headers.GetAccessToken() ?? (useSlaWhenCurrentContextMissing ? this.serviceLevelAuthority.AccessToken : null));
                        commandToSend.Headers.SetIdentityToken(
                            contextMessage.Headers.GetIdentityToken() ?? (useSlaWhenCurrentContextMissing ? this.serviceLevelAuthority.IdentityToken : null));

                        break;
                }
            }
        }

        private async Task IfLargeMessageSaveToBlobStorage<T>(T message) where T : ApiMessage
        {
            {
                if (MessageIsTooBigForServiceBus())
                {
                    SetBlobIdAndSasStorageTokenHeader();
                    await this.blobStorage.SaveApiMessageAsBlob(message);
                    message.ClearAllPublicPropertyValuesExceptHeaders();
                }
            }

            void SetBlobIdAndSasStorageTokenHeader()
            {
                var blobId = message.Headers.GetMessageId();
                message.Headers.SetBlobId(blobId);

                var sasStorageToken = this.blobStorage.GetStorageSasTokenForBlob(
                    blobId,
                    new EnumerationFlags(IBlobStorage.BlobSasPermissions.ReadAndDelete),
                    "large-messages");
                message.Headers.SetSasStorageToken(sasStorageToken);
            }

            bool MessageIsTooBigForServiceBus() => MessageSizeInBytes() > 256000; //* servicebus max size is 256KB

            

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
