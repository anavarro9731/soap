namespace Soap.Pf.MessageContractsBase.Queries
{
    using System;
    using Soap.If.Interfaces.Messages;

    public abstract class AbstractGetMessageFailedAllRetriesLogItem<TResponse> : ApiQuery<TResponse>
        where TResponse : AbstractGetMessageFailedAllRetriesLogItem<TResponse>.AbstractResponseModel, new()
    {
        protected AbstractGetMessageFailedAllRetriesLogItem(Guid idOfMessageYouWantResultsFor)
        {
            IdOfMessageYouWantResultsFor = idOfMessageYouWantResultsFor;
        }

        protected AbstractGetMessageFailedAllRetriesLogItem()
        {
        }

        public Guid IdOfMessageYouWantResultsFor { get; }

        public abstract class AbstractResponseModel
        {
            public Guid IdOfMessageThatFailed { get; set; }
        }
    }
}