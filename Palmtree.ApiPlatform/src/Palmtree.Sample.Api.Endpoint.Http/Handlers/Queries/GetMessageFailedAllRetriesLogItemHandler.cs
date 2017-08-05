namespace Palmtree.Sample.Api.Endpoint.Http.Handlers.Queries
{
    using System.Linq;
    using System.Threading.Tasks;
    using Palmtree.ApiPlatform.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.Sample.Api.Domain.Messages.Queries;
    using Palmtree.Sample.Api.Domain.Models.Aggregates;

    public class GetMessageFailedAllRetriesLogItemHandler : MessageHandler<GetMessageFailedAllRetriesLogItem, MessageFailedAllRetriesLogItem>
    {
        protected override async Task<MessageFailedAllRetriesLogItem> Handle(GetMessageFailedAllRetriesLogItem message, ApiMessageMeta meta)
        {
            return (await DataStore.ReadActive<MessageFailedAllRetriesLogItem>(
                        query => query.Where(m => m.IdOfMessageThatFailed == message.IdOfMessageYouWantResultsFor))).SingleOrDefault();
        }
    }
}
