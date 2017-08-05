namespace Palmtree.Sample.Api.Domain.Messages.Queries
{
    using System;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class GetMessageFailedAllRetriesLogItem : ApiQuery
    {
        public GetMessageFailedAllRetriesLogItem(Guid idOfMessageYouWantResultsFor)
        {
            IdOfMessageYouWantResultsFor = idOfMessageYouWantResultsFor;
        }

        public Guid IdOfMessageYouWantResultsFor { get; }
    }
}
