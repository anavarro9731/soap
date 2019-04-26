namespace Soap.Api.Sample.Domain.Messages.Queries
{
    using System;
    using Soap.Api.Sample.Domain.Models.Aggregates;
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