namespace Palmtree.Api.Sso.Endpoint.Tests.Rebus.Commands
{
    using System;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.Pf.EndpointClients;
    using Soap.Pf.EndpointTestsBase;
    using Xunit;

    public class WhenSeedingDatabaseViaRebusOnce
    {
        [Fact]
        public async void ItShouldNotFail()
        {
            var apiClient = new MsmqApiClient("serviceapi");

            var logItemMessageId = Guid.NewGuid();

            await apiClient.SendCommand(
                new SeedDatabase
                {
                    MessageId = logItemMessageId
                });

            await TestUtils.Assert.CommandSuccess(logItemMessageId);
        }
    }
}