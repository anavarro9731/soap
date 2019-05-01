namespace Soap.Api.Sso.Endpoint.Tests.Rebus.Commands
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sso.Domain.Constants;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Messages.Queries;
    using Soap.Pf.EndpointTestsBase;
    using Xunit;

    [Collection("Resets Db")]
    public class WhenSeedingDatabaseViaRebus

    {
        private readonly Guid logItemMessageId = Guid.NewGuid();

        [Fact]
        public async void TheUpgradeDatabaseMessageShouldSucceedWhenReseedIsTrue()
        {
            await Setup();
            //HACK because reseed recreates the database out of transaction you need
            //to give a little time to make sure that the message log item has been
            //recreated in the new db before querying
            await Task.Delay(5000);
            await TestUtils.Assert.CommandSuccess<GetMessageLogItemQuery, GetMessageLogItemQuery.GetMessageLogItemResponse>(this.logItemMessageId);
        }

        private async Task Setup()
        {
            var apiClient = TestUtils.Endpoints.Msmq.CreateApiClient(typeof(UpgradeTheDatabaseCommand).Assembly);

            await apiClient.Send(
                new UpgradeTheDatabaseCommand(ReleaseVersions.v1)
                {
                    MessageId = this.logItemMessageId, ReSeed = true
                });
        }
    }
}