namespace Palmtree.Api.Sso.Endpoint.Tests.Rebus.Commands
{
    using System;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.Pf.EndpointTestsBase;
    using Xunit;

    public class WhenSeedingDatabaseViaRebusOnce
    {
        [Fact]
        public async void ItShouldNotFail()
        {
            var apiClient = TestUtils.Endpoints.Msmq.CreateApiClient(typeof(SeedDatabase).Assembly);

            var logItemMessageId = Guid.NewGuid();

            await apiClient.Send(
                new SeedDatabase
                {
                    MessageId = logItemMessageId
                });

            await TestUtils.Assert.CommandSuccess(logItemMessageId);
        }
    }
}