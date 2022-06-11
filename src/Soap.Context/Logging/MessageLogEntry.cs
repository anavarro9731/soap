namespace Soap.Context.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
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
        public MessageLogEntry(SerialisableObject serialisedMessage, MessageMeta meta, int numberOfRetries, bool skeletonOnly)
        {
            //* the UOW, the LogEntry and the Message All have the Same GUID
            id = meta.MessageId;
            MessageMeta = meta;
            SkeletonOnly = skeletonOnly;
            MaxRetriesAllowed = numberOfRetries + 1;
            SerialisedMessage = serialisedMessage;
            MessageHash = serialisedMessage.ObjectData.ToMd5Hash();
        }
        
        public MessageLogEntry()
        {
            //* satisfy DataStore new() constraint and serialiser
            
        }

        //* attrib to ensure serialise internal setters?
        
        [JsonProperty]
        public List<Attempt> Attempts { get; internal set; } = new List<Attempt>();

        [JsonProperty]
        public int MaxRetriesAllowed { get; internal set; }

        [JsonProperty]
        public string MessageHash { get; internal set; }

        [JsonProperty]
        public MessageMeta MessageMeta { get; internal set; }

        [JsonProperty]
        public bool SkeletonOnly { get; internal set; }

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