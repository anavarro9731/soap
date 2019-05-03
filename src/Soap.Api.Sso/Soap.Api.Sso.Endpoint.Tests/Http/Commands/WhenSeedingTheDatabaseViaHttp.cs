namespace Soap.Api.Sso.Endpoint.Tests.Http.Commands
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sso.Domain.Constants;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Messages.Queries;
    using Soap.Pf.ClientServerMessaging.Commands;
    using Soap.Pf.EndpointTestsBase;
    using Xunit;

    [Collection("Resets Db")]
    public class WhenSeedingTheDatabaseViaHttp
    {
        private readonly Guid innerMessageId = Guid.NewGuid();

        private readonly Guid outerMessageId = Guid.NewGuid();

        [Fact]
        public async void TheUpgradeDatabaseMessageShouldSucceedAndBeInTheNewDatabase()
        {
            await Setup();
            //HACK because reseed recreates the database out of transaction you need
            //to give a little time to make sure that the message log item has been
            //recreated in the new db before querying
            await Task.Delay(7500);
            await TestUtils.Assert.CommandSuccess<GetMessageLogItemQuery, GetMessageLogItemQuery.GetMessageLogItemResponse>(this.innerMessageId);
        }

        [Fact]
        public async void TheWrapperMessageShouldStillBeInTheNewDatabase()
        {
            await Setup();
            await Task.Delay(5000);
            await TestUtils.Assert.CommandSuccess<GetMessageLogItemQuery, GetMessageLogItemQuery.GetMessageLogItemResponse>(this.outerMessageId);
        }

        private async Task Setup()
        {
            //arrange
            var apiClient = TestUtils.Endpoints.Http.CreateApiClient(typeof(UpgradeTheDatabaseCommand).Assembly);

            var message = new ForwardCommandFromHttpToMsmq<UpgradeTheDatabaseCommand>(
                new UpgradeTheDatabaseCommand(ReleaseVersions.v1)
                {
                    MessageId = this.innerMessageId, ReSeed = true, EnvelopeId = this.outerMessageId
                })
            {
                MessageId = this.outerMessageId
            };

            //act
            await apiClient.SendCommand(message);
        }
    }
}