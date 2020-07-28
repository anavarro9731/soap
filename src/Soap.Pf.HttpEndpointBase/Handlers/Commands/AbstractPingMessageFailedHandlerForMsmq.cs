﻿namespace Soap.Pf.HttpEndpointBase.Handlers.Commands
{
    public class AbstractPingMessageFailedHandlerForHttp<TPingFailed, TPing> : CommandHandler<TPingFailed>
        where TPingFailed : MessageFailedAllRetries<TPing>, new()

    {
        private readonly MessageFailedAllRetriesLogItemOperations failedMessageLogItemOperations;

        public AbstractPingMessageFailedHandlerForHttp(MessageFailedAllRetriesLogItemOperations failedMessageLogItemOperations)
        {
            this.failedMessageLogItemOperations = failedMessageLogItemOperations;
        }

        protected override async Task Handle(TPingFailed message, ApiMessageMeta meta)
        {
            await this.failedMessageLogItemOperations.AddLogItem(message.IdOfMessageThatFailed);
        }
    }
}