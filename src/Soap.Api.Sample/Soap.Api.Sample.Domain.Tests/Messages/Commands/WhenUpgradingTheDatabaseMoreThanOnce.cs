namespace Soap.Api.Sample.Domain.Tests.Messages.Commands
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Soap.Api.Sample.Domain.Constants;
    using Soap.Api.Sample.Domain.Messages.Commands;
    using Soap.Api.Sample.Domain.Models.Aggregates;
    using Soap.Pf.DomainTestsBase;
    using Xunit;

    public class WhenUpgradingTheDatabaseMoreThanOnce
    {
        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        public WhenUpgradingTheDatabaseMoreThanOnce()
        {
            this.endPoint.HandleCommand(new UpgradeTheDatabase(ReleaseVersions.v1));

            this.endPoint.HandleCommand(new UpgradeTheDatabase(ReleaseVersions.v2));
        }

        [Fact]
        public void ItShouldSetTheServiceStateDbVersionTo2()
        {
            var ss = this.endPoint.QueryDatabase<ServiceState>().Result.Single();
            ss.DatabaseState.HasState(ReleaseVersions.v2).Should().BeTrue();
        }
    }
}