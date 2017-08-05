namespace Palmtree.Sample.Api.Domain.Logic.Operations
{
    using System;
    using System.Threading.Tasks;
    using Palmtree.ApiPlatform.Infrastructure.ProcessesAndOperations;
    using Palmtree.Sample.Api.Domain.Models.Aggregates;

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
