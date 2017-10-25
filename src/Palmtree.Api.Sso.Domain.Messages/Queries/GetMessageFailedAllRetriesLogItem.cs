namespace Palmtree.Api.Sso.Domain.Messages.Queries
{
    using System;
    using Soap.Interfaces.Messages;

    public class GetMessageFailedAllRetriesLogItem : ApiQuery
    {
        public GetMessageFailedAllRetriesLogItem(Guid idOfMessageYouWantResultsFor)
        {
            IdOfMessageYouWantResultsFor = idOfMessageYouWantResultsFor;
        }

        public Guid IdOfMessageYouWantResultsFor { get; }
    }
}