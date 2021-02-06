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
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;

    public class Bus : IBus
    {
        private readonly IBlobStorage blobStorage;

        private readonly IBootstrapVariables bootstrapVariables;

        private readonly string envPartitionKey;

        private readonly Func<Task<ServiceLevelAuthority>> getServiceLevelAuthority;
        
        //* if you go back to making an i/o bound op in this function remove .result, it was just a lot of work to pass so i am leaving the infrastructure in place for future
        private ServiceLevelAuthority ServiceLevelAuthority => this.getServiceLevelAuthority().Result;
        
        private readonly IMessageAggregator messageAggregator;

        private ServiceLevelAuthority serviceLevelAuthority;

        public Bus(
            IBusClient busClient,
            IBusSettings settings,
            IMessageAggregator messageAggregator,
            IBlobStorage blobStorage,
            Func<Task<ServiceLevelAuthority>> getServiceLevelAuthority,
            IBootstrapVariables bootstrapVariables)
        {
            BusClient = busClient;
            this.messageAggregator = messageAggregator;
            this.blobStorage = blobStorage;
            this.bootstrapVariables = bootstrapVariables;

            this.getServiceLevelAuthority = async () =>
                {
                //only get this once per context, it could be expensive
                this.serviceLevelAuthority ??= await getServiceLevelAuthority();
                return this.serviceLevelAuthority;
                };
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
                    throw new ApplicationException(
                        "Outgoing message is set to reply to web socket sender, but the sender has not set a sessionId");
                }

                //* transfer from incoming command to outgoing event for websocket clients
                eventToPublish.Headers.SetSessionId(contextMessage.Headers.GetSessionId());
                eventToPublish.Headers.SetCommandHash(contextMessage.Headers.GetCommandHash());
                eventToPublish.Headers.SetCommandConversationId(contextMessage.Headers.GetCommandConversationId().Value);
            }

            eventToPublish.Validate();
            eventToPublish.RequiredNotNullOrThrow();
            eventToPublish.Headers.SetTimeOfCreationAtOrigin();
            eventToPublish.Headers.SetMessageId(Guid.NewGuid());
            eventToPublish.Headers.SetTopic(eventToPublish.Headers.GetType().FullName);
            eventToPublish.Headers.SetSchema(eventToPublish.Headers.GetType().FullName);
            eventToPublish.Headers.CheckHeadersOnOutgoingEvent(eventToPublish);

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
            /* getServiceLevelAuthority could be expensive and it is cached, that's why we repeat the call below rather then getting to
             a variable is to avoid making the call unnecessarily */

            if (this.bootstrapVariables.AuthEnabled)
            {
                switch (forceServiceLevelAuthority)
                {
                    case true:
                        //* set identity chain
                        var currentChain = contextMessage.Headers.GetIdentityChain();
                        commandToSend.Headers.SetIdentityChain(
                            currentChain switch
                            {
                                null => ServiceLevelAuthority.IdentityChainSegment, //* context message could be an event, or auth disabled and not have a chain, etc
                                _ => $"{currentChain},{ServiceLevelAuthority.IdentityChainSegment}"
                            });
                        //TODO I think this is wrong missing encryption. Reconsider messages which DoNotRequireAuth but have auth headers to pass on
                        // this is really a similar issue to C109 in that the auth headers are used when the message is not authd
                        commandToSend.Headers.SetAccessToken(ServiceLevelAuthority.AccessToken); 
                        //* set identity token
                        commandToSend.Headers.SetIdentityToken(ServiceLevelAuthority.IdentityToken);
                        break;
                    
                    case false:
                        
                        /* there are a variety of cases where all of the following could be blank because the context message headers are blank, if they are blank (and they should all be blank or filled)
                        then we use the Sla instead if the flag is set to allow that, the only time this would really make sense without the flag set, would be when a message marked with 
                        [AuthorisationNotRequired] sent a command also marked with [AuthorisationNotRequired] */
                        var useSlaWhenCurrentContextMissing = this.bootstrapVariables.UseServiceLevelAuthorityInTheAbsenceOfASecurityContext;
                        commandToSend.Headers.SetIdentityChain(contextMessage.Headers.GetIdentityChain() ?? (useSlaWhenCurrentContextMissing ? ServiceLevelAuthority.IdentityChainSegment : null)); 
                        commandToSend.Headers.SetAccessToken(contextMessage.Headers.GetAccessToken() ?? (useSlaWhenCurrentContextMissing ? ServiceLevelAuthority.AccessToken : null));
                        commandToSend.Headers.SetIdentityToken(contextMessage.Headers.GetIdentityToken() ?? (useSlaWhenCurrentContextMissing ? ServiceLevelAuthority.IdentityToken : null));

                        break;
                }
            }
            
            commandToSend.Headers.SetTimeOfCreationAtOrigin();
            commandToSend.Headers.SetMessageId(Guid.NewGuid());
            commandToSend.Headers.CheckHeadersOnOutgoingCommand(commandToSend, this.bootstrapVariables.AuthEnabled, !commandToSend.GetType().HasAttribute<AuthorisationNotRequired>(), this.envPartitionKey);
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
                    new EnumerationFlags(IBlobStorage.BlobSasPermissions.ReadAndDelete),
                    "large-messages");
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
