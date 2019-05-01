namespace Soap.Pf.MessageContractsBase.Queries
{
    using System;
    using Soap.If.Interfaces.Messages;

    public class AbstractGetMessageFailedAllRetriesLogItem<TResponse> : ApiQuery<TResponse>
    where TResponse : AbstractGetMessageFailedAllRetriesLogItem<TResponse>.AbstractResponseModel, new()
    {
     
        public AbstractGetMessageFailedAllRetriesLogItem(Guid idOfMessageYouWantResultsFor)
        {
            IdOfMessageYouWantResultsFor = idOfMessageYouWantResultsFor;
        }

        public AbstractGetMessageFailedAllRetriesLogItem() { }

        public Guid IdOfMessageYouWantResultsFor { get; }

        public abstract class AbstractResponseModel
        {
            public Guid IdOfMessageThatFailed { get; set; }

        }
    }
}