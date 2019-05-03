namespace Soap.Api.Sample.Endpoint.Msmq.Handlers.Ping
{
    using Soap.Api.Sample.Domain.Messages.Ping;
    using Soap.Pf.MsmqEndpointBase.Handlers.Commands;

    public class PingHandler : AbstractPingHandlerForMsmq<MsmqPingCommand, PongCommand, PongEvent>
    {
    }
}