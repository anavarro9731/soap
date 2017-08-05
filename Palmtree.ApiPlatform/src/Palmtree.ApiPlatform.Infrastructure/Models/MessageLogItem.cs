﻿namespace Palmtree.ApiPlatform.Infrastructure.Models
{
    using System;
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using Palmtree.ApiPlatform.Interfaces;
    using Palmtree.ApiPlatform.Utility;

    public class MessageLogItem : Aggregate
    {
        public string ClrTypeOfMessage { get; set; }

        public List<FailedMessageResult> FailedAttempts { get; set; } = new List<FailedMessageResult>();

        public int MaxFailedMessages { get; set; }

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
