namespace Soap.Context.Context
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CircuitBoard;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using DataStore.Interfaces.LowLevel;
    using Serilog;
    using Soap.Context.BlobStorage;
    using Soap.Context.Logging;
    using Soap.Context.MessageMapping;
    using Soap.Context.UnitOfWork;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.NotificationServer;
    using Soap.Utility.Functions.Extensions;

    public class BoostrappedContext
    {
        public readonly IBootstrapVariables AppConfig;

        public readonly IBlobStorage BlobStorage;

        public readonly IBus Bus;

        public readonly DataStore DataStore;

        public readonly ILogger Logger;

        public readonly IMessageAggregator MessageAggregator;

        public readonly MapMessagesToFunctions MessageMapper;

        public readonly NotificationServer NotificationServer;
        
        public BoostrappedContext(
            IBootstrapVariables appConfig,
            DataStore dataStore,
            IMessageAggregator messageAggregator,
            ILogger logger,
            IBus bus,
            NotificationServer notificationServer,
            IBlobStorage blobStorage,
            MapMessagesToFunctions messageMapper)
        {
            this.MessageMapper = messageMapper;
            this.AppConfig = appConfig;
            this.DataStore = dataStore;
            this.MessageAggregator = messageAggregator;
            this.Logger = logger;
            this.Bus = bus;
            this.NotificationServer = notificationServer;
            this.BlobStorage = blobStorage;
        }

        protected BoostrappedContext(BoostrappedContext c)
        {
            this.AppConfig = c.AppConfig;
            this.DataStore = c.DataStore;
            this.MessageAggregator = c.MessageAggregator;
            this.Logger = c.Logger;
            this.Bus = c.Bus;
            this.NotificationServer = c.NotificationServer;
            this.MessageMapper = c.MessageMapper;
            this.BlobStorage = c.BlobStorage;
        }
    }

    public static class BootstrapContextExtensions
    {
        public static ContextWithMessageLogEntry Upgrade(
            this BoostrappedContext current,
            ApiMessage message,
            MessageLogEntry messageLogEntry,
            UnitOfWork unitOfWork) =>
            new ContextWithMessageLogEntry(messageLogEntry, message, current, unitOfWork);

        public static async Task CreateOrFindLogEntryAndMatchingUnitOfWork(
            this BoostrappedContext ctx,
            MessageMeta meta,
            ApiMessage originalMessage,
            ApiMessage message,
            Action<MessageLogEntry> outLogEntry,
            Action<UnitOfWork> outUnitOfWork)
        {
            {
                MessageLogEntry entry = null;
                UnitOfWork uow = null;

                await FindLogEntry(ctx, message, v => entry = v);

                if (entry == null)
                {
                    await CreateNewLogEntry(meta, ctx, originalMessage, message, v => entry = v);
                }
                else
                {
                    var uowBlob = await ctx.BlobStorage.GetBlobOrNull(message.Headers.GetMessageId(), "units-of-work");
                    if (uowBlob != null) //* might not exist if we didn't make it to the point of commitchanges
                    {
                        //* same code as in  ToUnitOfWork(this Blob b) method in Soap.context
                        var json = Encoding.UTF8.GetString(uowBlob.Bytes);
                        uow = json.FromJson<UnitOfWork>(SerialiserIds.UnitOfWork, uowBlob.Type.TypeString);    
                    }
                }

                outUnitOfWork(uow);
                outLogEntry(entry);
            }

            static async Task CreateNewLogEntry(
                MessageMeta meta,
                BoostrappedContext ctx,
                ApiMessage originalMessage,
                ApiMessage message,
                Action<MessageLogEntry> outLogEntry)
            {

                try
                {
                    ctx.Logger.Debug($"Creating record for msg id {message.Headers.GetMessageId()}");

                    
                    
                    var messageLogEntry = new MessageLogEntry(
                        //* save the small version if it's in blob storage already
                        originalMessage.Headers.GetBlobId().HasValue ? originalMessage : message, 
                        meta,
                        ctx.Bus.MaximumNumberOfRetries);

                    var newItem = await ctx.DataStore.Create(messageLogEntry);
                    
                    await ctx.DataStore.CommitChanges();
                    
                    ctx.Logger.Debug($"Created record with id {newItem.id} for msg id {message.Headers.GetMessageId()}");

                    outLogEntry(newItem.Clone());
                }
                catch (Exception e)
                {
                    throw new CircuitException($"Could not write message {message.Headers.GetMessageId()} to store", e);
                }
            }

            static async Task FindLogEntry(BoostrappedContext ctx, ApiMessage message, Action<MessageLogEntry> outResult)
            {

                try
                {
                    ctx.Logger.Debug($"Looking for msg id {message.Headers.GetMessageId()}");

                    var result = await ctx.DataStore.ReadActiveById<MessageLogEntry>(message.Headers.GetMessageId());

                    ctx.Logger.Debug(
                        result == null
                            ? $"Failed to find record for msg id {message.Headers.GetMessageId()}"
                            : $"Found record with id {result.id} for msg with id {message.Headers.GetMessageId()}");

                    outResult(result);
                }
                catch (Exception e)
                {
                    throw new CircuitException($"Could not read message {message.Headers.GetMessageId()} from store at {ctx.DataStore.DocumentRepository.ConnectionSettings.ToString()}", e);
                }
            }
        }

    }
}
