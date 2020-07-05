namespace Sample.Tests.Messages.Commands
{
    using System.Linq;
    using FluentAssertions;
    using Sample.Constants;
    using Sample.Models.Aggregates;
    using Soap.Utility.Objects.Binary;
    using Xunit;
    using Xunit.Abstractions;

    public class C101UpgradingTheDatabaseMoreThanOnce : Test
    {
        public C101UpgradingTheDatabaseMoreThanOnce(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            Execute(Commands.UpgradeTheDatabaseToV1, Identities.UserOne);
            Execute(Commands.UpgradeTheDatabaseToV2, Identities.UserOne);
        }

        [Fact]
        public void ItShouldSetTheServiceStateDbVersionTo1()
        {
            var ss = Result.DataStore.Read<ServiceState>().Result.Single();
            ss.DatabaseState.HasState(ReleaseVersions.V2).Should().BeTrue();
        }
    }
}