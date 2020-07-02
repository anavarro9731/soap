namespace Sample.Messages.Events
{
    using Soap.Pf.MessageContractsBase.Commands;
    using Soap.Pf.MessageContractsBase.Queries;

    public class GotMessageLogItemEvent : AbstractGetMessageLogItemQuery<GotMessageLogItemEvent>.AbstractResponseEvent
    {
    }
}