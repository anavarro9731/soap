namespace Soap.Api.Sample.Endpoint.Tests.Http.Commands
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Domain.Constants;
    using Soap.Api.Sample.Domain.Messages.Commands;
    using Soap.Api.Sample.Domain.Messages.Queries;
    using Soap.Pf.ClientServerMessaging.Commands;
    using Soap.Pf.EndpointTestsBase;
    using Xunit;

    [Collection("Resets Db")]
    public class WhenSeedingTheDatabaseViaHttp
    {
        private readonly Guid innerMessageId = Guid.NewGuid();

        private readonly Guid outerMessageId = Guid.NewGuid();

        [Fact]
        public async Task TheWrapperMessageShouldStillBeInTheNewDatabase()
        {
            await Setup();
            await Task.Delay(7500);
            await TestUtils.Assert.CommandSuccess<GetMessageLogItemQuery, GetMessageLogItemQuery.GetMessageLogItemResponse>(this.outerMessageId);
        }

        [Fact]
        public async Task TheUpgradeDatabaseMessageShouldSucceedAndBeInTheNewDatabase()
        {
            await Setup();
            //HACK because reseed recreates the database out of transaction you need
            //to give a little time to make sure that the message log item has been
            //recreated in the new db before querying
            await Task.Delay(5000);
            await TestUtils.Assert.CommandSuccess<GetMessageLogItemQuery, GetMessageLogItemQuery.GetMessageLogItemResponse>(this.innerMessageId);
        }

        private async Task Setup()
        {
            //arrange
            var apiClient = TestUtils.Endpoints.Http.CreateApiClient(typeof(UpgradeTheDatabaseCommand).Assembly);

            var message = new ForwardCommandFromHttpToMsmq<UpgradeTheDatabaseCommand>(
                new UpgradeTheDatabaseCommand(ReleaseVersions.v1)
                {
                    MessageId = this.innerMessageId,
                    ReSeed = true,
                    

                })
            {
                MessageId = this.outerMessageId
            };

            //act
            await apiClient.SendCommand(message);
        }
    }
}