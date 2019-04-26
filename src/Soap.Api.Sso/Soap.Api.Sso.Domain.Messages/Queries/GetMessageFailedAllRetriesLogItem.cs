namespace Soap.Api.Sso.Domain.Messages.Queries
{
    using System;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Soap.If.Interfaces.Messages;

    public class GetMessageFailedAllRetriesLogItem : ApiQuery<MessageFailedAllRetriesLogItem>
    {
        public GetMessageFailedAllRetriesLogItem(Guid idOfMessageYouWantResultsFor)
        {
            IdOfMessageYouWantResultsFor = idOfMessageYouWantResultsFor;
        }

        public Guid IdOfMessageYouWantResultsFor { get; }
    }
}