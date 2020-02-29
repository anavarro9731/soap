namespace Soap.MessagePipeline.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.MessagePipeline.UnitOfWork;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Models;

    public sealed class MessageLogEntry : Aggregate
    {
        public MessageLogEntry(ApiMessage message, MessageMeta meta, bool optimisticConcurrency, int numberOfRetries)
        {
            id = message.MessageId;
            MessageMeta = meta;
            MaxRetriesAllowed = numberOfRetries + 1;
            SerialisedMessage = message.ToSerialisableObject();
            MessageHash = JsonSerializer.Serialize(message).ToMd5Hash();
            UnitOfWork = new UnitOfWork(optimisticConcurrency);
        }

        public MessageLogEntry()
        {
            //- satisfy DataStore new() constraint  
        }

        public List<Attempt> Attempts { get; internal set; } = new List<Attempt>();

        public int MaxRetriesAllowed { get; internal set; }

        public string MessageHash { get; internal set; }

        public MessageMeta MessageMeta { get; internal set; }

        public bool ProcessingComplete { get; internal set; }

        public SerialisableObject SerialisedMessage { get; internal set; }

        public UnitOfWork UnitOfWork { get; internal set; }

        public class Attempt
        {
            public Attempt(FormattedExceptionInfo errors = null)
            {
                Errors = errors;
            }

            internal Attempt()
            {
                //-serialiser
            }

            public DateTime CompletedAt { get; internal set; } = DateTime.UtcNow;

            public FormattedExceptionInfo Errors { get; internal set; }
        }
    }

    public static class MessageLogEntryExts
    {
        public static void AddFailedAttempt(this MessageLogEntry messageLogItem, FormattedExceptionInfo errors)
        {
            messageLogItem.Attempts.Insert(0, new MessageLogEntry.Attempt(errors));
        }

        public static Task CompleteUnitOfWork(this MessageLogEntry messageLogEntry, IDatabaseSettings databaseSettings)
        {
            messageLogEntry.ProcessingComplete = true;

            return UpdateMessageLogEntry(messageLogEntry, databaseSettings);
        }

        public static Task UpdateUnitOfWork(
            this MessageLogEntry messageLogEntry,
            UnitOfWork u,
            IDatabaseSettings databaseSettings)
        {
            messageLogEntry.UnitOfWork = u;
            return messageLogEntry.UpdateMessageLogEntry(databaseSettings);
        }

        private static async Task UpdateMessageLogEntry(this MessageLogEntry messageLogEntry, IDatabaseSettings databaseSettings)
        {
            /* update immediately, you would need find a way to get it to be persisted
             first so use different instance of ds instead*/
            var d = new DataStore(databaseSettings.CreateRepository());
            await d.Update(messageLogEntry);
            await d.CommitChanges();
        }
    }
}