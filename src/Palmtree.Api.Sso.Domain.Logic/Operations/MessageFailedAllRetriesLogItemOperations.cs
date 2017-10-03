namespace Palmtree.Api.Sso.Domain.Logic.Operations
{
    using System;
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Soap.MessagePipeline.ProcessesAndOperations;

    public class MessageFailedAllRetriesLogItemOperations : Operations<MessageFailedAllRetriesLogItem>
    {
        public async Task<MessageFailedAllRetriesLogItem> AddLogItem(Guid messageId)
        {
            return await DataStore.Create(
                                      new MessageFailedAllRetriesLogItem
                                      {
                                          IdOfMessageThatFailed = messageId
                                      })
                                  .ConfigureAwait(false);
        }
    }
}
