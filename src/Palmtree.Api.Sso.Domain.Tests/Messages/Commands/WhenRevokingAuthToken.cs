namespace Palmtree.Api.Sso.Domain.Tests.Messages.Commands
{
    using System.Linq;
    using FluentAssertions;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Palmtree.Api.Sso.Domain.Models.ViewModels;
    using Soap.Pf.DomainTestsBase;
    using Xunit;

    public class WhenRevokingAuthToken
    {
        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        public WhenRevokingAuthToken()
        {
            //arrange
            var user = TestData.User1;

            this.endPoint.AddToDatabase(user);

            var authenticateUser = new AuthenticateUser(AuthenticateUser.UserCredentials.Create(user.UserName, "secret-sauce"));
            var securityContext = (ResetPasswordFromEmail.ClientSecurityContext)this.endPoint.HandleCommand(authenticateUser);

            user = this.endPoint.QueryDatabase<User>().Result.Single();
            user.ActiveSecurityTokens.Count.Should().Be(2); //security token has been added to user

            //act
            var disableUser = new RevokeAuthToken(securityContext.AuthToken);
            this.endPoint.HandleCommand(disableUser, user);
        }

        [Fact]
        public void ItShouldRemoveSecurityTokens()
        {
            var user = this.endPoint.QueryDatabase<User>(q => q.Where(x => x.Id == TestData.User1.Id)).Result.Single();

            user.ActiveSecurityTokens.Count.Should().Be(1); //the original token before we added one during auth
        }
    }
}