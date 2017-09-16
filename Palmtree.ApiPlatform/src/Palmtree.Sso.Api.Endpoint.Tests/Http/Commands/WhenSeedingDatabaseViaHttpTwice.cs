namespace Palmtree.Sample.Api.Endpoint.Tests.Http.Commands
{
    using System;
    using Palmtree.ApiPlatform.Endpoint.Clients;
    using Palmtree.ApiPlatform.EndpointTests.Infrastructure;
    using Palmtree.Sample.Api.Domain.Messages.Commands;
    using Xunit;

    public class WhenSeedingDatabaseViaHttpTwice
    {
        [Fact]
        public void ItShouldNotFail()
        {
            var apiClient = new HttpApiClient(TestUtils.Query.ApiHostUri);

            {
                SendOneDbSeedCommand(out Guid message1Id);
                SendOneDbSeedCommand(out Guid message2Id);

                TestUtils.Assert.CommandSuccess(message1Id).Wait();
                TestUtils.Assert.CommandSuccess(message2Id).Wait();
            }

            void SendOneDbSeedCommand(out Guid messageId)
            {
                messageId = Guid.NewGuid();

                apiClient.SendCommand(
                             new SeedDatabase
                             {
                                 MessageId = messageId
                             })
                         .Wait();
            }
        }
    }
}
