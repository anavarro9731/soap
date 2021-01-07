namespace Soap.Api.Sample.Tests.Messages
{
    using System.Linq;
    using CircuitBoard;
    using FluentAssertions;
    using Soap.Api.Sample.Constants;
    using Soap.Api.Sample.Models.Aggregates;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC101UpgradingTheDatabaseMoreThanOnce : Test
    {
        public TestC101UpgradingTheDatabaseMoreThanOnce(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            SetupTestByProcessingAMessage(Commands.UpgradeTheDatabaseToV1, Identities.UserOne);
            
            TestMessage(Commands.UpgradeTheDatabaseToV2, Identities.UserOne).Wait();
        }

        [Fact]
        public void ItShouldSetTheServiceStateDbVersionTo2()
        {
            var ss = Result.DataStore.Read<ServiceState>().Result.Single();
            ss.DatabaseState.HasFlag(ReleaseVersions.V2).Should().BeTrue();
        }
    }
}
