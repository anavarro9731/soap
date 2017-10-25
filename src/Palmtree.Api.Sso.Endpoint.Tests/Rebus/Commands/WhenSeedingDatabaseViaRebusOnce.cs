namespace Palmtree.Api.Sso.Endpoint.Tests.Rebus.Commands
{
    using System;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.Endpoint.Clients;
    using Soap.EndpointTests.Infrastructure;
    using Xunit;

    public class WhenSeedingDatabaseViaRebusOnce
    {
        [Fact]
        public async void ItShouldNotFail()
        {
            var apiClient = new RebusApiClient("serviceapi");

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