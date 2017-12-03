namespace Soap.MessagePipeline.Models.Aggregates
{
    using System;
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility;

    public class MessageLogItem : Aggregate
    {
        public string ClrTypeOfMessage { get; set; }

        public List<FailedMessageResult> FailedAttempts { get; set; } = new List<FailedMessageResult>();

        public int MaxFailedMessages { get; set; }

        /* NOTE: Messagelogitem.Message not generic 
         * because some clients might not serialise using JSON.NET, 
         * publicly serialisable classes should not rely on $type
         * also datastore doesn't support generics*/
        public dynamic Message { get; set; }

        public string MessageHash { get; set; }

        public SuccessMessageResult SuccessfulAttempt { get; set; }

        public static MessageLogItem Create(IApiMessage message, IApplicationConfig applicationConfig)
        {
            return new MessageLogItem
            {
                id = message.MessageId,
                MaxFailedMessages = applicationConfig.NumberOfApiMessageRetries + 1,
                Message = message,
                ClrTypeOfMessage = message.GetType().FullName,
                MessageHash = Md5Hash.Create(JsonConvert.SerializeObject(message))
            };
        }

        public T MessageAs<T>()
        {
            return ((JObject)Message).ToObject<T>();
        }

        public class FailedMessageResult
        {
            public PipelineExceptionMessages Errors { get; set; }

            public DateTime FailedAt { get; set; }

            public static FailedMessageResult Create(PipelineExceptionMessages errors)
            {
                return new FailedMessageResult
                {
                    Errors = errors,
                    FailedAt = DateTime.UtcNow
                };
            }
        }

        public class SuccessMessageResult : Entity
        {
            public dynamic ReturnValue { get; set; }

            public DateTime SucceededAt { get; set; }

            public static SuccessMessageResult Create(object returnValue = null)
            {
                return new SuccessMessageResult
                {
                    ReturnValue = returnValue,
                    SucceededAt = DateTime.UtcNow
                };
            }
        }
    }

    public static class MessageLogItemOperations
    {
        public static MessageLogItem AddFailedMessageResult(MessageLogItem messageLogItem, PipelineExceptionMessages errors)
        {
            messageLogItem.FailedAttempts.Insert(
                0,
                //TODO: should be changed to use .Create and set fields like timestamp during creation
                new MessageLogItem.FailedMessageResult
                {
                    Errors = errors
                });
            return messageLogItem;
        }

        public static MessageLogItem AddSuccessfulMessageResult(MessageLogItem messageLogItem, object returnValue = null)
        {
            messageLogItem.SuccessfulAttempt = new MessageLogItem.SuccessMessageResult
            {
                ReturnValue = returnValue
            };

            return messageLogItem;
        }
    }
}