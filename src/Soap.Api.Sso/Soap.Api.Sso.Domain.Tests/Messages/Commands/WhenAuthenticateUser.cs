namespace Soap.Api.Sso.Domain.Tests.Messages.Commands
{
    using System.Linq;
    using FluentAssertions;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Xunit;

    public class WhenAuthenticateUser : Test

    {
        private readonly AddFullyRegisteredUser createRegisteredUser1 = Commands.CreateRegisteredUser1();

        private readonly AddFullyRegisteredUser createRegisteredUser2 = Commands.CreateRegisteredUser2();

        private readonly ResetPasswordFromEmail.ClientSecurityContext result;

        public WhenAuthenticateUser()
        {
            //arrange            

            this.endPoint.HandleCommand(this.createRegisteredUser1);

            this.endPoint.HandleCommand(this.createRegisteredUser2);

            var authenticateUser =
                new AuthenticateUser(AuthenticateUser.UserCredentials.Create(this.createRegisteredUser2.Email, this.createRegisteredUser2.Password));

            this.result = this.endPoint.HandleCommand(authenticateUser);
        }

        [Fact]
        public void ItShouldAddNewActiveSecurityToken()
        {
            var user = this.endPoint.QueryDatabase<User>(q => q.Where(x => x.UserName == this.createRegisteredUser2.Email)).Result.Single();

            user.Should().NotBeNull();

            user.ActiveSecurityTokens.Should().HaveCount(1);

            user.ActiveSecurityTokens.Should().ContainSingle(t => t.UserId == this.createRegisteredUser2.Id);
        }

        [Fact]
        public void ItShouldAuthenticateTheUser()
        {
            Assert.True(this.result.UserProfile.id == this.createRegisteredUser2.Id);
        }
    }
}