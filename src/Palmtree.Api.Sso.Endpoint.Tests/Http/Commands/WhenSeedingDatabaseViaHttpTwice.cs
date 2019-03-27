namespace Palmtree.Api.Sso.Endpoint.Tests.Http.Commands
{
    using System;
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Messages.Queries.Abstract;
    using Soap.Pf.ClientServerMessaging.Commands;
    using Soap.Pf.EndpointTestsBase;
    using Xunit;

    public class WhenSeedingDatabaseViaHttpTwice
    {
        [Fact]
        public async void ItShouldNotFail()
        {
            var apiClient = TestUtils.Endpoints.Http.CreateApiClient(typeof(SeedDatabase).Assembly);

            {
                var message1Id = await SendOneDbSeedCommand();
                var message2Id = await SendOneDbSeedCommand();

                await TestUtils.Assert.CommandSuccess<GetMessageLogItemQuery, GetMessageLogItemQuery.GetMessageLogItemResponse>(message1Id);
                await TestUtils.Assert.CommandSuccess<GetMessageLogItemQuery, GetMessageLogItemQuery.GetMessageLogItemResponse>(message2Id);
            }

            async Task<Guid> SendOneDbSeedCommand()
            {
                var messageId = Guid.NewGuid();

                await apiClient.SendCommand(
                             new ForwardCommandFromHttpToMsmq<SeedDatabase>(
                                 new SeedDatabase()
                                 {
                                     MessageId = Guid.NewGuid()
                                 })
                             {
                                 MessageId = messageId
                             });

                return messageId;
            }
        }
    }
}