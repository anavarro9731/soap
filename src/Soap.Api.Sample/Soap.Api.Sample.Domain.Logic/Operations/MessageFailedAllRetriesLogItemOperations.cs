﻿namespace Soap.Api.Sample.Domain.Logic.Operations
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Domain.Models.Aggregates;
    using Soap.If.MessagePipeline.ProcessesAndOperations;

    public class MessageFailedAllRetriesLogItemOperations : Operations<MessageFailedAllRetriesLogItem>
    {
        public Task<MessageFailedAllRetriesLogItem> AddLogItem(Guid messageId)
        {
            {
                DetermineChange(out var logItem);

                return DataStore.Create(logItem);
            }

            void DetermineChange(out MessageFailedAllRetriesLogItem logItem)
            {
                logItem = new MessageFailedAllRetriesLogItem
                {
                    IdOfMessageThatFailed = messageId
                };
            }
        }
    }
}