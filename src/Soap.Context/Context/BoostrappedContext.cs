namespace Soap.Context.Context
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using CircuitBoard;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using Serilog;
    using Soap.Context.BlobStorage;
    using Soap.Context.Logging;
    using Soap.Context.MessageMapping;
    using Soap.Context.UnitOfWork;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.NotificationServer;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Models;

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
        public static async Task<ContextWithMessageLogEntry> Upgrade(
            this BoostrappedContext context,
            ApiMessage message,
            ApiMessage messageAtPerimeter,
            MessageMeta meta)
        {
            var result = await CreateOrFindLogEntryAndMatchingUnitOfWork(
                context,
                meta,
                messageAtPerimeter,
                message);

            return new ContextWithMessageLogEntry(result.MessagLogEntry, message, messageAtPerimeter, context, result.SavedToStorageTask, result.UnitOfWork);
        }

        private static async Task<(MessageLogEntry MessagLogEntry, UnitOfWork UnitOfWork, Task SavedToStorageTask)> CreateOrFindLogEntryAndMatchingUnitOfWork(
            BoostrappedContext ctx,
            MessageMeta meta,
            ApiMessage messageAtPerimeter,
            ApiMessage message
            )
        {
            {
                MessageLogEntry entry = await FindLogEntry(ctx, message);
                Task savedToBlobStorageTask = Task.CompletedTask;
                UnitOfWork uow = null;

                if (entry == null) //* we have never seen a message with this ID before 
                {
                    var result = GetSerialisedMessage(messageAtPerimeter, message);
                    if (result.NeedsToBeBlobbed) savedToBlobStorageTask = ctx.BlobStorage.SaveApiMessageAsBlob(message);
                    /* important thing to remember here is that whatever hash you save here has to be re-creatable from the original message
                    to test on the next run, so if you save the skeleton, you need to be able to determine that on the next run */
                    entry = await CreateNewLogEntry(meta, ctx, result.SavedSkeletonOnly, result.SerialisedMessage);
                }
                else
                {
                    var uowBlob = await ctx.BlobStorage.GetBlobOrNull(message.Headers.GetMessageId(), "units-of-work");
                    if (
                        uowBlob != null) //* might not exist if this is a retry and we didn't make it to the point of CommitChanges on the UOW first time
                    {
                        //* same code as in  ToUnitOfWork(this Blob b) method in Soap.context
                        var json = Encoding.UTF8.GetString(uowBlob.Bytes);
                        uow = json.FromJson<UnitOfWork>(SerialiserIds.UnitOfWork, uowBlob.Type.TypeString);
                    }
                }

                return (entry, uow, savedToBlobStorageTask);
            }

            static (SerialisableObject SerialisedMessage, bool NeedsToBeBlobbed, bool SavedSkeletonOnly) GetSerialisedMessage(ApiMessage messageAtPerimeter, ApiMessage message)
            {
                {
                    SerialisableObject serialisedMessage;
                    bool needsToBeBlobbed = false;
                    bool savedSkeletonOnly = false;
                    if (MessageWasBlobbedByTheSender(message))
                    {
                        /* it is larger than 256kb so the payload is in blob storage
                         save the skeleton in the logs */
                        serialisedMessage = messageAtPerimeter.ToSerialisableObject(SerialiserIds.ApiBusMessage);
                        savedSkeletonOnly = true;
                    }
                    else if (Exceeds256K(message.ToBlob()))
                    {
                        //* upload to blob storage
                        needsToBeBlobbed = true;
                        //* save same shell as if client would have done it, but don't change the message that's about to go through the pipeline
                        var skeleton = message.Clone().ClearAllPublicPropertyValuesExceptHeaders();
                        serialisedMessage = skeleton.ToSerialisableObject(SerialiserIds.ApiBusMessage);
                        savedSkeletonOnly = true;
                    }
                    else
                    {
                        //* it can be saved directly on messagelog
                        serialisedMessage = message.ToSerialisableObject(SerialiserIds.ApiBusMessage);
                    }

                    return (serialisedMessage,needsToBeBlobbed, savedSkeletonOnly);
                }

                static bool MessageWasBlobbedByTheSender(ApiMessage message)
                {
                    return message.Headers.GetBlobId().HasValue;
                }

                /* if the sender used HTTP Direct it will allow large messages over 256KB. Cosmos wont take records over 2MB and it would be slow anyway */
                static bool Exceeds256K(Blob message)
                {
                    return message.Bytes.Length > 256000;
                }
            }



            static async Task<MessageLogEntry> CreateNewLogEntry(
                MessageMeta meta,
                BoostrappedContext ctx,
                bool skeletonOnly,
                SerialisableObject serialisedMessage)
            {
                try
                {
                    ctx.Logger.Debug($"Creating record for msg id {meta.MessageId}");

                    var messageLogEntry = new MessageLogEntry(serialisedMessage, meta, ctx.Bus.MaximumNumberOfRetries, skeletonOnly);

                    var newItem = await ctx.DataStore.Create(messageLogEntry);

                    await ctx.DataStore.CommitChanges();

                    ctx.Logger.Debug($"Created record with id {newItem.id} for msg id {meta.MessageId}");

                    return newItem.Clone();  //why did i clone this? was is just for safety sake since I wasn't sure what commitchanges might do to it? .create already clones it once
                }
                catch (Exception e)
                {
                    throw new CircuitException($"Could not write message {meta.MessageId} to store", e);
                }
            }

            static async Task<MessageLogEntry> FindLogEntry(BoostrappedContext ctx, ApiMessage message)
            {
                try
                {
                    ctx.Logger.Debug($"Looking for msg id {message.Headers.GetMessageId()}");

                    var result = await ctx.DataStore.ReadActiveById<MessageLogEntry>(message.Headers.GetMessageId());

                    ctx.Logger.Debug(
                        result == null
                            ? $"Failed to find record for msg id {message.Headers.GetMessageId()}"
                            : $"Found record with id {result.id} for msg with id {message.Headers.GetMessageId()}");

                    return result;
                }
                catch (Exception e)
                {
                    throw new CircuitException(
                        $"Could not read message {message.Headers.GetMessageId()} from store at {ctx.DataStore.DocumentRepository.ConnectionSettings}",
                        e);
                }
            }
        }
    }
}