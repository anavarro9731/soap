namespace Palmtree.Api.Sso.Domain.Tests.Messages.Commands
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Palmtree.Api.Sso.Domain.Models.ValueObjects;
    using Soap.DomainTests.Infrastructure;
    using Soap.Endpoint.Infrastructure;
    using Soap.Utility;
    using Xunit;

    public class WhenSeedingDatabaseMoreThanOnce
    {
        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        public WhenSeedingDatabaseMoreThanOnce()
        {
            var userId = Guid.NewGuid();
            // Arrange
            var commmand1 = new SeedDatabase();

            // Act
            var commmand2 = new SeedDatabase();

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

    public class WhenSeedingDatabaseOnlyOnce
    {
        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        public WhenSeedingDatabaseOnlyOnce()
        {
            // Arrange
            var commmand = new SeedDatabase();

            // Act
            this.endPoint.HandleCommand(commmand);
        }

        [Fact]
        public void ItShouldAutoConfirmDefaultUser()
        {
            var defaultUser = this.endPoint.QueryDatabase<User>().Result.Single();
            defaultUser.EmailConfirmed().Should().BeTrue();
        }

        [Fact]
        public void ItShouldCreateADefaultUser()
        {
            {
                var allUsers = this.endPoint.QueryDatabase<User>().Result.ToList();
                allUsers.Should().NotBeEmpty();
                allUsers.Count.Should().Be(1);

                var defaultUser = allUsers.Single();
                defaultUser.Should().NotBeNull();
                defaultUser.UserName.Should().Be(HardCodedMasterData.RootUser.UserName);
                defaultUser.Email.Should().Be(HardCodedMasterData.RootUser.EmailAddress);
                defaultUser.FullName.Should().Be(HardCodedMasterData.RootUser.FullName);
                var expectedPasswordHash = HashPassword(HardCodedMasterData.RootUser.Password, defaultUser.PasswordDetails);
                defaultUser.PasswordDetails.PasswordHash.Should().Be(expectedPasswordHash);
            }

            string HashPassword(string clearPassword, PasswordDetails passwordDetails)
            {
                return SecureHmacHash.CreateFrom(clearPassword, passwordDetails.HashIterations, passwordDetails.HexSalt).HexHash;
            }
        }
    }
}
