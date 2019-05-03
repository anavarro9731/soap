namespace Soap.Api.Sso.Endpoint.Tests.Msmq.Commands
{
    using Soap.Api.Sso.Domain.Messages.Ping;
    using Soap.Api.Sso.Domain.Messages.Queries;
    using Soap.Pf.EndpointTestsBase;
    using Xunit;

    [Collection("Resets Db")]
    public class WhenPingingTheServiceViaMsmq : AbstractWhenPingingTheServiceViaMsmq<MsmqPingCommand, PongCommand, GetMessageLogItemQuery,
        GetMessageLogItemQuery.GetMessageLogItemResponse>
    {
    }
}