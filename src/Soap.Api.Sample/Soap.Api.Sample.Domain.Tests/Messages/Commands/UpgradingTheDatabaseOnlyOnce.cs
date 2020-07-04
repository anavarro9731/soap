namespace Sample.Tests.Messages.Commands
{
    using System.Linq;
    using FluentAssertions;
    using Sample.Models.Aggregates;
    using Sample.Models.Constants;
    using Soap.Utility.Objects.Binary;
    using Xunit;
    using Xunit.Abstractions;

    public class UpgradingTheDatabaseOnlyOnce : Test
    {
        public UpgradingTheDatabaseOnlyOnce(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            Execute(Commands.UpgradeTheDatabaseToV1, Identities.UserOne);
        }

        [Fact]
        public void ItShouldSetTheServiceStateDbVersionTo1()
        {
            var ss = this.Result.DataStore.Read<ServiceState>().Result.Single();
            ss.DatabaseState.HasState(ReleaseVersions.V1).Should().BeTrue();
        }
    }
}