namespace Sample.Messages.Events
{
    using Soap.Pf.MessageContractsBase.Commands;

    public class PongEvent : AbstractPingCommand<PongEvent>.AbstractPongEvent
    {
    }
}