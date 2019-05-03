namespace Soap.Api.Sso.Endpoint.Http.Handlers.Ping
{
    using Soap.Api.Sso.Domain.Messages.Ping;
    using Soap.Pf.HttpEndpointBase.Handlers.Commands;

    public class PingHandlerForHttp : AbstractPingHandlerForHttp<HttpPingCommand, HttpPingCommand.PongViewModel, PongCommand>
    {
    }
}