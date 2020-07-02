namespace Sample.Messages.Queries
{
    using Sample.Messages.Events;
    using Soap.Pf.MessageContractsBase.Queries;

    public sealed class GetMessageLogItemQuery : AbstractGetMessageLogItemQuery<GotMessageLogItemEvent>.AbstractResponseEvent
    {
    }
}