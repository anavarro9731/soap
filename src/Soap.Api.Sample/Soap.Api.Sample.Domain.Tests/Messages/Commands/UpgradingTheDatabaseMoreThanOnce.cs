namespace Sample.Tests.Messages.Commands
{
    using System.Linq;
    using FluentAssertions;
    using Sample.Models.Aggregates;
    using Sample.Models.Constants;
    using Soap.Utility.Objects.Binary;
    using Xunit;
    using Xunit.Abstractions;

    public class UpgradingTheDatabaseMoreThanOnce : Test
    {
        public UpgradingTheDatabaseMoreThanOnce(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            Execute(Commands.UpgradeTheDatabaseToV1, Identities.UserOne);
            Execute(Commands.UpgradeTheDatabaseToV2, Identities.UserOne);
        }

        [Fact]
        public void ItShouldSetTheServiceStateDbVersionTo1()
        {
            var ss = this.Result.DataStore.Read<ServiceState>().Result.Single();
            ss.DatabaseState.HasState(ReleaseVersions.V2).Should().BeTrue();
        }
    }
}