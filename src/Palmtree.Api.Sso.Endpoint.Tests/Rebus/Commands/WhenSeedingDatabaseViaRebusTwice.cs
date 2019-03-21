namespace Palmtree.Api.Sso.Endpoint.Tests.Rebus.Commands
{
    using System;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Soap.Pf.EndpointTestsBase;
    using Xunit;

    public class WhenSeedingDatabaseViaRebusTwice
    {
        [Fact]
        public void ItShouldNotFail()
        {
            var apiClient = TestUtils.Endpoints.Msmq.CreateApiClient(typeof(SeedDatabase).Assembly);

            {
                SendOneDbSeedCommand(out var message1Id);
                SendOneDbSeedCommand(out var message2Id);

                TestUtils.Assert.CommandSuccess(message1Id).Wait();
                TestUtils.Assert.CommandSuccess(message2Id).Wait();
            }

            void SendOneDbSeedCommand(out Guid messageId)
            {
                messageId = Guid.NewGuid();

                apiClient.Send(
                             new SeedDatabase
                             {
                                 MessageId = messageId
                             })
                         .Wait();
            }
        }
    }
}