namespace Palmtree.Api.Sso.Domain.Messages.Queries
{
    using System;
    using Soap.If.Interfaces.Messages;

    public class GetMessageFailedAllRetriesLogItem : ApiQuery
    {
        public GetMessageFailedAllRetriesLogItem(Guid idOfMessageYouWantResultsFor)
        {
            IdOfMessageYouWantResultsFor = idOfMessageYouWantResultsFor;
        }

        public Guid IdOfMessageYouWantResultsFor { get; }
    }
}