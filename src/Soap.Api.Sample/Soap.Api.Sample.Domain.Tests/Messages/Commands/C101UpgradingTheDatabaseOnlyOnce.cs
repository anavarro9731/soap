namespace Sample.Tests.Messages.Commands
{
    using System.Linq;
    using FluentAssertions;
    using Sample.Constants;
    using Sample.Models.Aggregates;
    using Soap.Utility.Objects.Binary;
    using Xunit;
    using Xunit.Abstractions;

    public class C101UpgradingTheDatabaseOnlyOnce : Test
    {
        public C101UpgradingTheDatabaseOnlyOnce(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            Execute(Commands.UpgradeTheDatabaseToV1, Identities.UserOne);
        }

        [Fact]
        public void ItShouldSetTheServiceStateDbVersionTo1()
        {
            var ss = Result.DataStore.Read<ServiceState>().Result.Single();
            ss.DatabaseState.HasState(ReleaseVersions.V1).Should().BeTrue();
        }
    }
}