namespace Soap.If.MessagePipeline.Models.Aggregates
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using DataStore.Interfaces.LowLevel;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline2.MessagePipeline;
    using Soap.If.Utility;
    using Soap.If.Utility.Models;
    using Soap.If.Utility.PureFunctions.Extensions;

    public static class MessageLogEntryExts
    {
        public static MessageLogEntry AddFailedMessageResult(this MessageLogEntry messageLogItem, MessageExceptionInfo errors)
        {
            messageLogItem.FailedAttempts.Insert(0, new MessageLogEntry.FailedMessageResult(errors));

            return messageLogItem;
        }

        public static MessageLogEntry AddSuccessfulMessageResult(this MessageLogEntry messageLogItem)
        {
            messageLogItem.SuccessfulAttempt = new MessageLogEntry.SuccessMessageResult();

            return messageLogItem;
        }
    }

    public sealed class MessageLogEntry : Aggregate
    {
        public MessageLogEntry(ApiMessage message)
        {
            id = message.MessageId;
            MaxFailedMessages = MMessageContext.AppConfig.NumberOfApiMessageRetries + 1;
            SerialisedMessage = message.ToSerialisableObject();
            MessageHash = JsonSerializer.Serialize(message).ToMd5Hash();
        }

        public MessageLogEntry()
        {
            //- satisfy datastore new() constraint  
        }

        public List<FailedMessageResult> FailedAttempts { get; set; } = new List<FailedMessageResult>();

        public int MaxFailedMessages { get; set; }

        public string MessageHash { get; set; }

        public SerialisableObject SerialisedMessage { get; set; }

        public SuccessMessageResult SuccessfulAttempt { get; set; }

        public class FailedMessageResult
        {
            public FailedMessageResult(MessageExceptionInfo errors)
            {
                Errors = errors;
                FailedAt = DateTime.UtcNow;
            }

            internal FailedMessageResult()
            {
            }

            public MessageExceptionInfo Errors { get; set; }

            public DateTime FailedAt { get; set; }
        }

        public class SuccessMessageResult : Entity
        {
            public SuccessMessageResult(object returnValue = null)
            {
                SucceededAt = DateTime.UtcNow;
            }

            internal SuccessMessageResult()
            {
            }

            public DateTime SucceededAt { get; set; }
        }
    }
}