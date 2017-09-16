namespace Palmtree.Sample.Api.Domain.Tests.Messages.Commands
{
    using System.Linq;
    using FluentAssertions;
    using Palmtree.ApiPlatform.DomainTests.Infrastructure;
    using Palmtree.Sample.Api.Domain.Messages.Commands;
    using Palmtree.Sample.Api.Domain.Models.Aggregates;
    using Palmtree.Sample.Api.Domain.Models.ViewModels;
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
            var securityContext = (ClientSecurityContext)this.endPoint.HandleCommand(authenticateUser);

            user = this.endPoint.QueryDatabase<User>().Result.Single();
            user.ActiveSecurityTokens.Count.Should().Be(2); //security token has been added to user

            //act
            var disableUser = new RevokeAuthToken(securityContext.AuthToken);
            this.endPoint.HandleCommand(disableUser, user);
        }

        [Fact]
        public void ItShouldRemoveSecurityTokens()
        {
            var user = this.endPoint.QueryDatabase<User>(q => q.Where(x => x.id == TestData.User1.id)).Result.Single();

            user.ActiveSecurityTokens.Count.Should().Be(1); //the original token before we added one during auth
        }
    }
}
