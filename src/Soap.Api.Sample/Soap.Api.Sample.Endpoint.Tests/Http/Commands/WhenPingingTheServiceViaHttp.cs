namespace Soap.Api.Sample.Endpoint.Tests.Http.Commands
{
    using Soap.Api.Sample.Domain.Messages.Ping;
    using Soap.Api.Sample.Domain.Messages.Queries;
    using Soap.Pf.EndpointTestsBase;
    using Xunit;

    [Collection("Resets Db")]
    public class WhenPingingTheServiceViaHttp : AbstractWhenPingingTheServiceViaHttp
    <HttpPingCommand, HttpPingCommand.PongViewModel, GetMessageLogItemQuery,
        GetMessageLogItemQuery.GetMessageLogItemResponse>
    {
    }
}