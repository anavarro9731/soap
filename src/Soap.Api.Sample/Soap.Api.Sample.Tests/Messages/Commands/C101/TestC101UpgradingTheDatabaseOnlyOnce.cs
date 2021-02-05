//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests.Messages
{
    using System.Linq;
    using CircuitBoard;
    using FluentAssertions;
    using Soap.Api.Sample.Constants;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Objects.Binary;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC101UpgradingTheDatabaseOnlyOnce : Test
    {
        public TestC101UpgradingTheDatabaseOnlyOnce(ITestOutputHelper outputHelper)
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
