namespace Soap.Api.Sample.Domain.Tests.Messages.Commands
{
    using System.Linq;
    using FluentAssertions;
    using Soap.Api.Sample.Domain.Constants;
    using Soap.Api.Sample.Domain.Messages.Commands;
    using Soap.Api.Sample.Domain.Models.Aggregates;
    using Xunit;

    public class WhenUpgradingTheDatabaseOnlyOnce : Test
    {
        public WhenUpgradingTheDatabaseOnlyOnce()
        {
            var command = new UpgradeTheDatabaseCommand(ReleaseVersions.v1);

            this.endPoint.HandleCommand(command);
        }

        [Fact]
        public void ItShouldSetTheServiceStateDbVersionTo1()
        {
            var ss = this.endPoint.QueryDatabase<ServiceState>().Result.Single();
            ss.DatabaseState.HasState(ReleaseVersions.v1).Should().BeTrue();
        }
    }
}