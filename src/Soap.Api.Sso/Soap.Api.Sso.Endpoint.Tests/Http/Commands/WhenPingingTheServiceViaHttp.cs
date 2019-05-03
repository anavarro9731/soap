namespace Soap.Api.Sso.Endpoint.Tests.Http.Commands
{
    using Soap.Api.Sso.Domain.Messages.Ping;
    using Soap.Api.Sso.Domain.Messages.Queries;
    using Soap.Pf.EndpointTestsBase;
    using Xunit;

    [Collection("Resets Db")]
    public class WhenPingingTheServiceViaHttp : AbstractWhenPingingTheServiceViaHttp<HttpPingCommand, HttpPingCommand.PongViewModel, GetMessageLogItemQuery,
        GetMessageLogItemQuery.GetMessageLogItemResponse>
    {
    }
}