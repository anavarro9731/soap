namespace Palmtree.Sample.Api.Endpoint.Tests.Rebus.Commands
{
    using System;
    using Palmtree.ApiPlatform.Endpoint.Clients;
    using Palmtree.ApiPlatform.EndpointTests.Infrastructure;
    using Palmtree.Sample.Api.Domain.Messages.Commands;
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
