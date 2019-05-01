namespace Soap.Api.Sso.Endpoint.Http.Handlers.Ping
{
    using Soap.Api.Sso.Domain.Messages.Ping;
    using Soap.Pf.HttpEndpointBase.Handlers.Commands;

    public class PingHandler : AbstractPingHandler<PingCommand, PingCommand.PongViewModel, PongCommand, PongEvent>
    {
    }
}