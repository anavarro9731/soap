namespace Soap.PfBase.Logic.ProcessesAndOperations
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CircuitBoard;
    using DataStore;
    using DataStore.Interfaces;
    using Serilog;
    using Soap.Context;
    using Soap.Context.Context;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.NotificationServer;
    using Soap.PfBase.Logic.ProcessesAndOperations.ProcessMessages;
    using Soap.Utility;
    using Soap.Utility.Functions.Extensions;

    /// <summary>
    ///     represents a stateless multi-step process which occurs in a single unit of work
    ///     but involves more than one aggregate instance [regardless or type] and/or more than one service [e.g. EmailSender,
    ///     DataStore]
    ///     It records StatefulProcessStarted/Completed events whose purpose is mainly log instrumentation but could be used in
    ///     unit testing as well.
    /// </summary>
    public abstract class Process : IProcess
    {
        
        private readonly ContextWithMessageLogEntry context = ContextWithMessageLogEntry.Current;

        protected IIdaamProvider IDAAM => this.context.IdaamProvider;
        
        protected BusWrapper Bus => new BusWrapper(this.context.Bus, this.context.Message);

        protected IDataStoreReadOnly DataReader => this.context.DataStore.AsReadOnly();

        protected ILogger Logger => this.context.Logger;

        protected MessageMeta Meta => this.context.MessageLogEntry.MessageMeta;

        protected NotificationServer NotificationServer => this.context.NotificationServer;

        public async Task BeginProcess<TMessage>(TMessage message, MessageMeta meta) where TMessage : ApiCommand
        {
            var process = this as IBeginProcess<TMessage>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            RecordStarted(new ProcessStarted(GetType().Name, meta.UserProfileOrNull?.IdaamProviderId));

            await process.BeginProcess(message);

            RecordCompleted(new ProcessCompleted(GetType().Name, meta.UserProfileOrNull?.IdaamProviderId));
        }

        protected T GetCustomConfigVariable<T>(string propertyName)
        {
            var propertyInfo = this.context.AppConfig.GetType().GetProperty(propertyName);
            Guard.Against(propertyInfo == null, $"Custom config property {propertyName} not found");
            return (T)propertyInfo.GetValue(this.context.AppConfig, null);
        }

        protected async Task PublishFormDataEvent<TApiCommand>(
            UIFormDataEvent<TApiCommand> @event,
            TApiCommand commandToSubmitForm) where TApiCommand : ApiCommand
        {
            @event.Op(
                e =>
                    {
                    var commandId = Guid.NewGuid();
                    var sasToken = ContextWithMessageLogEntry.Current.BlobStorage.GetStorageSasTokenForBlob(
                        commandId,
                        new EnumerationFlags(IBlobStorage.BlobSasPermissions.CreateNew),
                        "large-messages");
                    e.SetProperties(sasToken, commandId, commandToSubmitForm);
                    });

            await Bus.Publish(@event, new IBusClient.EventVisibilityFlags(IBusClient.EventVisibility.ReplyToWebSocketSender));
        }

        private void RecordCompleted(ProcessCompleted processCompleted)
        {
            this.context.MessageAggregator.Collect(processCompleted);
        }

        private void RecordStarted(ProcessStarted statefulProcessStarted)
        {
            this.context.MessageAggregator.Collect(statefulProcessStarted);
        }

        protected class BusWrapper
        {
            private readonly IBus bus;

            private readonly ApiMessage contextMessage;

            public BusWrapper(IBus bus, ApiMessage contextMessage)
            {
                this.bus = bus;
                this.contextMessage = contextMessage;
            }

            public Task Publish(ApiEvent publishEvent, IBusClient.EventVisibilityFlags eventVisibility = null) =>
                this.bus.Publish(publishEvent, this.contextMessage, eventVisibility);

            public Task Send(
                ApiCommand sendCommand,
                bool forceServiceLevelAuthority = false,
                DateTimeOffset scheduledAt = default)
            {
                if (this.contextMessage.Headers.GetMessageId() == SpecialIds.ForceServiceLevelAuthorityOnOutgoingMessages)
                {
                    forceServiceLevelAuthority = true;
                }

                return this.bus.Send(sendCommand, this.contextMessage, forceServiceLevelAuthority, scheduledAt);
            }
        }
    }
}
