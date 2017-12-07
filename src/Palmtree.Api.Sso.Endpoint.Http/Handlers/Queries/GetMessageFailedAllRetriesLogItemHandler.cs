namespace Palmtree.Api.Sso.Endpoint.Http.Handlers.Queries
{
    using System.Linq;
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Messages.Queries;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Soap.If.MessagePipeline.Models;
    using Soap.Pf.HttpEndpointBase;

    public class GetMessageFailedAllRetriesLogItemHandler : QueryHandler<GetMessageFailedAllRetriesLogItem, MessageFailedAllRetriesLogItem>
    {
        protected override async Task<MessageFailedAllRetriesLogItem> Handle(GetMessageFailedAllRetriesLogItem message, ApiMessageMeta meta)
        {
            return (await DataStore.ReadActive<MessageFailedAllRetriesLogItem>(m => m.IdOfMessageThatFailed == message.IdOfMessageYouWantResultsFor))
                .SingleOrDefault();
        }
    }
}