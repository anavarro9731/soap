namespace Soap.Api.Sample.Endpoint.Http.Handlers.Queries
{
    using Soap.Api.Sample.Domain.Messages.Queries;
    using Soap.Pf.HttpEndpointBase.Handlers.Queries;

    public class GetMessageFailedAllRetriesLogItemHandler : 
        AbstractGetMessageFailedAllRetriesLogItemHandler<
            GetMessageFailedAllRetriesLogItemQuery, GetMessageFailedAllRetriesLogItemQuery.MessageFailedAllRetriesLogItemViewModel>
    {
    }
}