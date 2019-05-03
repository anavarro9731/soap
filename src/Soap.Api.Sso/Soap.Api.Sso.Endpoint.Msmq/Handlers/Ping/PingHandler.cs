namespace Soap.Api.Sso.Endpoint.Msmq.Handlers.Ping
{
    using Soap.Api.Sso.Domain.Messages.Ping;
    using Soap.Pf.MsmqEndpointBase.Handlers.Commands;

    public class PingHandler : AbstractPingHandlerForMsmq<MsmqPingCommand, PongCommand, PongEvent>
    {
    }
}