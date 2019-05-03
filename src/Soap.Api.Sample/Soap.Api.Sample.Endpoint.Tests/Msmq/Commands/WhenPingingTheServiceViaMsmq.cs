namespace Soap.Api.Sample.Endpoint.Tests.Msmq.Commands
{
    using Soap.Api.Sample.Domain.Messages.Ping;
    using Soap.Api.Sample.Domain.Messages.Queries;
    using Soap.Pf.EndpointTestsBase;
    using Xunit;

    [Collection("Resets Db")]
    public class WhenPingingTheServiceViaMsmq : AbstractWhenPingingTheServiceViaMsmq<MsmqPingCommand, PongCommand, GetMessageLogItemQuery,
        GetMessageLogItemQuery.GetMessageLogItemResponse>
    {
    }
}