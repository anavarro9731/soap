﻿namespace Soap.If.MessagePipeline.Models.Aggregates
{
    using System;
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel;
    using Newtonsoft.Json;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.Utility;

    public class MessageLogItem : Aggregate
    {
        public string ClrTypeOfMessage { get; set; }

        public List<FailedMessageResult> FailedAttempts { get; set; } = new List<FailedMessageResult>();

        public int MaxFailedMessages { get; set; }

        /* MessageLogItem not generic to keep all in one collection */
        public object Message { get; set; }

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

        public class FailedMessageResult
        {
            public PipelineExceptionMessages Errors { get; set; }

            public DateTime FailedAt { get; set; }

            public static FailedMessageResult Create(PipelineExceptionMessages errors)
            {
                return new FailedMessageResult
                {
                    Errors = errors, FailedAt = DateTime.UtcNow
                };
            }
        }

        public class SuccessMessageResult : Entity
        {
            public object ReturnValue { get; set; }

            public DateTime SucceededAt { get; set; }

            public static SuccessMessageResult Create(object returnValue = null)
            {
                return new SuccessMessageResult
                {
                    ReturnValue = returnValue, SucceededAt = DateTime.UtcNow
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