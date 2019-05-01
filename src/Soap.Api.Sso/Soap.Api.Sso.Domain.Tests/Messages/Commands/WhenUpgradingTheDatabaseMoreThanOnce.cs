namespace Soap.Api.Sso.Domain.Tests.Messages.Commands
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Soap.Api.Sso.Domain.Constants;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Xunit;

    public class WhenUpgradingTheDatabaseMoreThanOnce : Test
    {
        public WhenUpgradingTheDatabaseMoreThanOnce()
        {
            var userId = Guid.NewGuid();

            var commmand1 = new UpgradeTheDatabaseCommand(ReleaseVersions.v1);
            var commmand2 = new UpgradeTheDatabaseCommand(ReleaseVersions.v1)
            {
                ReSeed = true
            };

            this.endPoint.HandleCommand(commmand1);
            this.endPoint.HandleCommand(commmand2);
        }

        [Fact]
        public void ItShouldNotDuplicateDefaultUser()
        {
            var allUsers = this.endPoint.QueryDatabase<User>().Result.ToList();

            allUsers.Count.Should().Be(1);
        }
    }
}