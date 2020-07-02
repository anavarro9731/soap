namespace Sample.Messages.Queries
{
    using Sample.Messages.Events;
    using Soap.Pf.MessageContractsBase.Queries;

    public class GetMessageFailedAllRetriesLogItemQuery : AbstractGetMessageFailedAllRetriesLogItem<GotMessageFailedAllRetriesLogItemEvent>
    {
    }
}