namespace Soap.Api.Sample.Endpoint.Http.Handlers.Ping
{
    using Soap.Api.Sample.Domain.Messages.Ping;
    using Soap.Pf.HttpEndpointBase.Handlers.Commands;

    public class PingHandlerForHttp : AbstractPingHandlerForHttp<HttpPingCommand, HttpPingCommand.PongViewModel, PongCommand>
    {
    }
}