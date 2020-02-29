namespace Soap.MessagePipeline.Context
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard.Permissions;
    using DataStore;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.Logging;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.Utility.Functions.Extensions;

    public class ContextAfterMessageObtained : BootrappedContext, IMessageFunctions
    {
        private readonly IMessageFunctions functions;

        public ContextAfterMessageObtained(
            ApiMessage message,
            (DateTime receivedTime, long receivedTicks) timeStamp,
            BootrappedContext context)
            : base(context)
        {
            Message = message;
            TimeStamp = timeStamp;
            this.functions = context.MessageMapper.Map(message);
        }

        protected ContextAfterMessageObtained(ContextAfterMessageObtained c)
            : base(c)
        {
            Message = c.Message;
            this.functions = c.functions;
            TimeStamp = c.TimeStamp;
        }

        public ApiMessage Message { get; }

        public (DateTime receivedTime, long receivedTicks) TimeStamp { get; }

        public IMapErrorCodesFromDomainToMessageErrorCodes GetErrorCodeMapper() => this.functions.GetErrorCodeMapper();

        public Task Handle(ApiMessage msg) => this.functions.Handle(msg);

        public Task HandleFinalFailure(ApiMessage msg) => this.functions.HandleFinalFailure(msg);

        public void Validate(ApiMessage msg) => this.functions.Validate(msg);
    }

    internal static class ContextAfterMessageObtainedExtensions
    {
        internal static async Task CreateOrFindLogEntry(
            this ContextAfterMessageObtained ctx,
            IIdentityWithPermissions identity,
            Action<MessageLogEntry> outLogEntry)
        {
            {
                MessageLogEntry entry = null;

                await FindLogEntry(ctx, v => entry = v);

                if (entry == null)
                {
                    var meta = new MessageMeta(ctx.TimeStamp, identity, ctx.Message.GetSchema());

                    await CreateNewLogEntry(meta, ctx, v => entry = v);
                }

                outLogEntry(entry);
            }

            static async Task CreateNewLogEntry(
                MessageMeta meta,
                ContextAfterMessageObtained ctx,
                Action<MessageLogEntry> outLogEntry)
            {
                var message = ctx.Message;

                try
                {
                    ctx.Logger.Debug($"Creating record for msg id {message.MessageId}");

                    var newItem = await ctx.DataStore.Create(
                                      new MessageLogEntry(
                                          message,
                                          meta,
                                          ((DataStore)ctx.DataStore).DataStoreOptions.OptimisticConcurrency, //TODO
                                          ctx.AppConfig.NumberOfApiMessageRetries));

                    await ctx.DataStore.CommitChanges();

                    ctx.Logger.Debug($"Created record with id {newItem.id} for msg id {message.MessageId}");

                    outLogEntry(newItem.Clone());
                }
                catch (Exception e)
                {
                    throw new Exception($"Could not write message {message.MessageId} to store", e);
                }
            }

            static async Task FindLogEntry(ContextAfterMessageObtained ctx, Action<MessageLogEntry> outResult)
            {
                var message = ctx.Message;

                try
                {
                    ctx.Logger.Debug($"Looking for msg id {message.MessageId}");

                    var result = await ctx.DataStore.ReadActiveById<MessageLogEntry>(message.MessageId);

                    ctx.Logger.Debug(
                        result == null
                            ? $"Failed to find record for msg id {message.MessageId}"
                            : $"Found record with id {result.id} for msg with id {message.MessageId}");

                    outResult(result);
                }
                catch (Exception e)
                {
                    throw new Exception($"Could not read message {message.MessageId} from store", e);
                }
            }
        }

        internal static ContextAfterMessageLogEntryObtained Upgrade(
            this ContextAfterMessageObtained current,
            MessageLogEntry messageLogEntry)
        {
            return new ContextAfterMessageLogEntryObtained(messageLogEntry, current);
        }
    }
}