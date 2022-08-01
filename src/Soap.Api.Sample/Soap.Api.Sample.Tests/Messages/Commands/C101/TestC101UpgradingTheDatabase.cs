namespace Soap.Api.Sample.Tests.Messages.Commands.C101
{
    using System.Linq;
    using CircuitBoard;
    using FluentAssertions;
    using Soap.Api.Sample.Constants;
    using Soap.Api.Sample.Models.Aggregates;
    using Xunit;
    using Xunit.Abstractions;
    using Commands = Soap.Api.Sample.Tests.Commands;

    public class TestC101UpgradingTheDatabase : Test
    {
        public TestC101UpgradingTheDatabase(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            TestMessage(Commands.UpgradeTheDatabaseToV1, Identities.JohnDoeAllPermissions).Wait();
        }

        [Fact]
        public void ItShouldSetTheServiceStateDbVersionTo1()
        {
            var ss = Result.DataStore.Read<ServiceState>().Result.Single();
            ss.DatabaseState.HasFlag(ReleaseVersions.V1).Should().BeTrue();
        }
    }
}
