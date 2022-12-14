//* ##REMOVE-IN-COPY##
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

    public class TestC101v1UpgradingTheDatabaseMoreThanOnce : Test
    {
        public TestC101v1UpgradingTheDatabaseMoreThanOnce(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            SetupTestByProcessingAMessage(Commands.UpgradeTheDatabaseToV1, Identities.JohnDoeAllPermissions);
            
            TestMessage(Commands.UpgradeTheDatabaseToV2, Identities.JohnDoeAllPermissions).Wait();
        }

        [Fact]
        public void ItShouldSetTheServiceStateDbVersionTo2()
        {
            var ss = Result.DataStore.Read<ServiceState>().Result.Single();
            ss.DatabaseState.HasFlag(ReleaseVersions.V2).Should().BeTrue();
        }
    }
}
