namespace Soap.Api.Sso.Domain.Tests.Messages.Commands
{
    using System.Linq;
    using FluentAssertions;
    using Soap.Api.Sso.Domain.Constants;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Models;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Soap.Api.Sso.Domain.Models.ValueObjects;
    using Soap.If.Utility;
    using Xunit;

    public class WhenUpgradingTheDatabaseOnlyOnce : Test
    {
        public WhenUpgradingTheDatabaseOnlyOnce()
        {
            // Arrange
            var commmand = new UpgradeTheDatabaseCommand(ReleaseVersions.v1);

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

        [Fact]
        public void ItShouldSetTheServiceStateDbVersionTo1()
        {
            var ss = this.endPoint.QueryDatabase<ServiceState>().Result.Single();
            ss.DatabaseState.HasState(ReleaseVersions.v1).Should().BeTrue();
        }
    }
}