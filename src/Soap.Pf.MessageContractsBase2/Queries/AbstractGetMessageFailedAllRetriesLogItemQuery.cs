namespace Soap.Pf.MessageContractsBase.Queries
{
    using System;
    using Soap.Interfaces.Messages;

    public abstract class AbstractGetMessageFailedAllRetriesLogItem<TResponse> : ApiCommand<TResponse>
        where TResponse : AbstractGetMessageFailedAllRetriesLogItem<TResponse>.AbstractResponseEvent, new()
    {
        protected AbstractGetMessageFailedAllRetriesLogItem(Guid idOfMessageYouWantResultsFor)
        {
            IdOfMessageYouWantResultsFor = idOfMessageYouWantResultsFor;
        }

        protected AbstractGetMessageFailedAllRetriesLogItem()
        {
        }

        public Guid IdOfMessageYouWantResultsFor { get; internal set; }

        public abstract class AbstractResponseEvent : ApiEvent
        {
            public Guid IdOfMessageThatFailed { get; set; }
        }
    }
}