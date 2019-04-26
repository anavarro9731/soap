namespace Soap.Api.Sample.Domain.Tests.Messages.Commands
{
    using System.Linq;
    using FluentAssertions;
    using Soap.Api.Sample.Domain.Constants;
    using Soap.Api.Sample.Domain.Messages.Commands;
    using Soap.Api.Sample.Domain.Models.Aggregates;
    using Soap.Pf.DomainTestsBase;
    using Xunit;

    public class WhenUpgradingTheDatabaseOnlyOnce
    {
        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        public WhenUpgradingTheDatabaseOnlyOnce()
        {
            // Arrange
            var commmand = new UpgradeTheDatabase(ReleaseVersions.v1);

            // Act
            this.endPoint.HandleCommand(commmand);
        }

        [Fact]
        public void ItShouldSetTheServiceStateDbVersionTo1()
        {
            var ss = this.endPoint.QueryDatabase<ServiceState>().Result.Single();
            ss.DatabaseState.HasState(ReleaseVersions.v1).Should().BeTrue();
        }
    }
}