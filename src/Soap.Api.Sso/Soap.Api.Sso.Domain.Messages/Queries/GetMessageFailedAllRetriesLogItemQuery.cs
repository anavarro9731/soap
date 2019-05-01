namespace Soap.Api.Sso.Domain.Messages.Queries
{
    using System;
    using Soap.Pf.MessageContractsBase.Queries;

    public class GetMessageFailedAllRetriesLogItemQuery : AbstractGetMessageFailedAllRetriesLogItem<
        GetMessageFailedAllRetriesLogItemQuery.MessageFailedAllRetriesLogItemViewModel>
    {
        public GetMessageFailedAllRetriesLogItemQuery(Guid idOfMessageYouWantResultsFor)
            : base(idOfMessageYouWantResultsFor)
        {
        }

        public GetMessageFailedAllRetriesLogItemQuery()
        {
        }

        public class MessageFailedAllRetriesLogItemViewModel : AbstractResponseModel
        {
        }
    }
}