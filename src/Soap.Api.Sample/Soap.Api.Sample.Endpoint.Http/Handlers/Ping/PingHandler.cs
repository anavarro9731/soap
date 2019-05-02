namespace Soap.Api.Sample.Endpoint.Http.Handlers.Ping
{
    using Soap.Api.Sample.Domain.Messages.Commands;
    using Soap.Api.Sample.Domain.Messages.Ping;
    using Soap.Pf.HttpEndpointBase.Handlers.Commands;

    public class PingHandler : AbstractPingHandler<PingCommand, PingCommand.PongViewModel, PongCommand, PongEvent>
    {
    }
}