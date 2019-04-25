namespace Soap.Api.Sso.Endpoint.Tests.Http.Commands
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sso.Domain.Constants;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Messages.Queries.Abstract;
    using Soap.Pf.ClientServerMessaging.Commands;
    using Soap.Pf.EndpointTestsBase;
    using Xunit;

    [Collection("Resets Db")]
    public class WhenSeedingDatabaseViaHttp
    {
        private readonly Guid innerMessageId = Guid.NewGuid();

        private readonly Guid outerMessageId = Guid.NewGuid();

        [Fact]
        public async void TheWrapperMessageShouldSucceed()
        {
            await Setup();

            await TestUtils.Assert.CommandSuccess<GetMessageLogItemQuery, GetMessageLogItemQuery.GetMessageLogItemResponse>(this.outerMessageId);
        }

        [Fact]
        public async void TheUpgradeDatabaseMessageShouldSucceedWhenReseedIsTrue()
        {
            await Setup();
            //HACK because reseed recreates the database out of transaction you need
            //to give a little time to make sure that the message log item has been
            //recreated in the new db before querying
            await Task.Delay(10000);
            await TestUtils.Assert.CommandSuccess<GetMessageLogItemQuery, GetMessageLogItemQuery.GetMessageLogItemResponse>(this.innerMessageId);
        }

        private async Task Setup()
        {
            //arrange
            var apiClient = TestUtils.Endpoints.Http.CreateApiClient(typeof(UpgradeTheDatabase).Assembly);

            var message = new ForwardCommandFromHttpToMsmq<UpgradeTheDatabase>(
                new UpgradeTheDatabase(ReleaseVersions.v1)
                {
                    MessageId = this.innerMessageId, ReSeed = true
                })
            {
                MessageId = this.outerMessageId
            };

            //act
            await apiClient.SendCommand(message);
        }
    }
}