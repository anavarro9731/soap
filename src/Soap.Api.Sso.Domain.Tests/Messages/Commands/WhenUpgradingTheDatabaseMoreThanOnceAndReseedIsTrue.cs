namespace Soap.Api.Sso.Domain.Tests.Messages.Commands
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Soap.Api.Sso.Domain.Constants;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Soap.Pf.DomainTestsBase;
    using Xunit;

    public class WhenUpgradingTheDatabaseMoreThanOnceAndReseedIsTrue
    {
        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        public WhenUpgradingTheDatabaseMoreThanOnceAndReseedIsTrue()
        {
            var userId = Guid.NewGuid();
            // Arrange
            var commmand1 = new UpgradeTheDatabase(ReleaseVersions.v1);

            // Act
            var commmand2 = new UpgradeTheDatabase(ReleaseVersions.v1) { ReSeed = true };

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