namespace Soap.If.MessagePipeline.Models.Aggregates
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using DataStore.Interfaces.LowLevel;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.UnitOfWork;
    using Soap.If.MessagePipeline2.MessagePipeline;
    using Soap.If.Utility;
    using Soap.If.Utility.Models;
    using Soap.If.Utility.PureFunctions;
    using Soap.If.Utility.PureFunctions.Extensions;

    public static class MessageLogEntryExts
    {
        public static void AddFailedAttempt(this MessageLogEntry messageLogItem, FormattedExceptionInfo errors)
        {
            messageLogItem.Attempts.Insert(0, new MessageLogEntry.Attempt(errors));
        }

        public static void AddUnitOfWork(this MessageLogEntry messageLogEntry, UnitOfWork unitOfWork)
        {
            //- defensive programming, shouldn't happen want to catch it if I missed something
            Guard.Against(messageLogEntry.UnitOfWork != null, "Cannot add another unit of work");

            messageLogEntry.UnitOfWork = unitOfWork;
        }
    }

    public sealed class MessageLogEntry : Aggregate
    {
        public MessageLogEntry(ApiMessage message)
        {
            id = message.MessageId;
            MaxRetriesAllowed = MContext.AppConfig.NumberOfApiMessageRetries + 1;
            SerialisedMessage = message.ToSerialisableObject();
            MessageHash = JsonSerializer.Serialize(message).ToMd5Hash();
        }

        public MessageLogEntry()
        {
            //- satisfy datastore new() constraint  
        }

        public List<Attempt> Attempts { get; internal set; } = new List<Attempt>();

        public int MaxRetriesAllowed { get; internal set; }

        public string MessageHash { get; internal set; }

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
            }

            public DateTime CompletedAt { get; internal set; } = DateTime.UtcNow;

            public FormattedExceptionInfo Errors { get; internal set; }
        }
    }
}