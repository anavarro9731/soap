namespace Sample.Messages.Ping
{
    using Sample.Messages.Events;
    using Soap.Pf.MessageContractsBase.Commands;

    public class PingCommand : AbstractPingCommand<PongEvent>
    {
    }
}