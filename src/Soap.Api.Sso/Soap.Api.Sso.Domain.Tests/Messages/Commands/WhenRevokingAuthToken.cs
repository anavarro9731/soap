﻿namespace Soap.Api.Sso.Domain.Tests.Messages.Commands
{
    using System.Linq;
    using FluentAssertions;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Xunit;

    public class WhenRevokingAuthToken : Test
    {
        public WhenRevokingAuthToken()
        {
            //arrange
            var user = Aggregates.User1;

            this.endPoint.AddToDatabase(user);

            var authenticateUser = new AuthenticateUser(AuthenticateUser.UserCredentials.Create(user.UserName, "secret-sauce"));
            var securityContext = this.endPoint.HandleCommand(authenticateUser);

            user = this.endPoint.QueryDatabase<User>().Result.Single();
            user.ActiveSecurityTokens.Count.Should().Be(2); //security token has been added to user

            //act
            var disableUser = new RevokeAuthToken(securityContext.AuthToken);
            this.endPoint.HandleCommand(disableUser, user);
        }

        [Fact]
        public void ItShouldRemoveSecurityTokens()
        {
            var user = this.endPoint.QueryDatabase<User>(q => q.Where(x => x.id == Aggregates.User1.id)).Result.Single();

            user.ActiveSecurityTokens.Count.Should().Be(1); //the original token before we added one during auth
        }
    }
}