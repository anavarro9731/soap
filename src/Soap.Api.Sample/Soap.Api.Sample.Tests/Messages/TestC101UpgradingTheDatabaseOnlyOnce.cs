namespace Soap.Api.Sample.Tests.Messages
{
    using System.Linq;
    using FluentAssertions;
    using Soap.Api.Sample.Constants;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Utility.Objects.Binary;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC101UpgradingTheDatabaseOnlyOnce : Test
    {
        public TestC101UpgradingTheDatabaseOnlyOnce(ITestOutputHelper outputHelper)
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