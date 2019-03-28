namespace Palmtree.Api.Sso.Endpoint.Tests.Rebus.Commands
{
    using System;
    using System.Threading.Tasks;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Messages.Queries.Abstract;
    using Soap.Pf.EndpointTestsBase;
    using Xunit;

    public class WhenSeedingDatabaseViaRebusTwice
    {
        [Fact]
        public async void ItShouldNotFail()
        {
            var apiClient = TestUtils.Endpoints.Msmq.CreateApiClient(typeof(SeedDatabase).Assembly);

            {
                var message1Id = await SendOneDbSeedCommand();
                var message2Id = await SendOneDbSeedCommand();

                await TestUtils.Assert.CommandSuccess<GetMessageLogItemQuery, GetMessageLogItemQuery.GetMessageLogItemResponse>(message1Id);
                await TestUtils.Assert.CommandSuccess<GetMessageLogItemQuery, GetMessageLogItemQuery.GetMessageLogItemResponse>(message2Id);
            }

            async Task<Guid> SendOneDbSeedCommand()
            {
                var messageId = Guid.NewGuid();

                await apiClient.Send(
                    new SeedDatabase
                    {
                        MessageId = messageId
                    });

                return messageId;
            }
        }
    }
}