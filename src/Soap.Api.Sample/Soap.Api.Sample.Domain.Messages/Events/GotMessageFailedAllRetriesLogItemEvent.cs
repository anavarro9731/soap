namespace Sample.Messages.Events
{
    using Soap.Pf.MessageContractsBase.Queries;

    public class GotMessageFailedAllRetriesLogItemEvent : AbstractGetMessageFailedAllRetriesLogItem<
        GotMessageFailedAllRetriesLogItemEvent>.AbstractResponseEvent
    {
    }
}