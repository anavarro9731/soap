namespace Soap.Context.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Options;
    using Newtonsoft.Json;
    using Soap.Context.Exceptions;
    using Soap.Context.UnitOfWork;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Models;

    public sealed class MessageLogEntry : Aggregate
    {
        public MessageLogEntry(ApiMessage message, MessageMeta meta, int numberOfRetries)
        {
            //* the UOW, the LogEntry and the Message All have the Same GUID
            id = message.Headers.GetMessageId();
            MessageMeta = meta;
            MaxRetriesAllowed = numberOfRetries + 1;
            var serialisedMessage = message.ToSerialisableObject();
            var byteCount = Encoding.UTF8.GetByteCount(serialisedMessage.ObjectData);
            var indexingAndOtherMessageLogEntryData = Convert.ToInt32(byteCount / 0.8M);
            if (indexingAndOtherMessageLogEntryData > 2000000 /*2MB */)
            {
                //* HTTP Direct will allow large messages over 256KB, not in blob storage
                //* Cosmos wont take records over 2MB, better I suppose to process it rather than reject it, throw a CircuitException here if you change your mind 
                //* another option is too save large items to blob storage, though that will add to processing time
                SerialisedMessage = null;
            }
            MessageHash = message.ToJson(SerialiserIds.ApiBusMessage).ToMd5Hash();
            
            
                
            
        }
        


        public MessageLogEntry()
        {
            //* satisfy DataStore new() constraint and serialiser
        }

        [JsonProperty]
        public List<Attempt> Attempts { get; internal set; } = new List<Attempt>();

        [JsonProperty]
        public int MaxRetriesAllowed { get; internal set; }

        [JsonProperty]
        public string MessageHash { get; internal set; }

        [JsonProperty]
        public MessageMeta MessageMeta { get; internal set; }

        [JsonProperty]
        public bool ProcessingComplete { get; internal set; }

        [JsonProperty]
        public SerialisableObject SerialisedMessage { get; internal set; }
        
        public class Attempt
        {
            public Attempt(FormattedExceptionInfo errors = null)
            {
                Errors = errors;
            }

            public Attempt()
            {
                //-serialiser
            }

            [JsonProperty]
            public DateTime CompletedAt { get; internal set; } = DateTime.UtcNow;

            [JsonProperty]
            public FormattedExceptionInfo Errors { get; internal set; }
        }
    }

    public static class MessageLogEntryExts
    {
        public static void AddFailedAttempt(this MessageLogEntry messageLogItem, FormattedExceptionInfo errors)
        {
            messageLogItem.Attempts.Insert(0, new MessageLogEntry.Attempt(errors));
        }

        public static async Task CompleteUnitOfWork(this MessageLogEntry messageLogEntry, IDocumentRepository documentRepository)
        {
            messageLogEntry.ProcessingComplete = true;

            /* update immediately, you would need find a way to get it to be persisted
            first so use different instance of ds instead*/
            var d = new DataStore(documentRepository,
                dataStoreOptions: DataStoreOptions.Create().DisableOptimisticConcurrency());
            await d.Update(messageLogEntry);
            await d.CommitChanges();
        }
    }
}