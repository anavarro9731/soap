namespace Palmtree.Sample.Api.Domain.Tests.Messages.Commands
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Palmtree.ApiPlatform.DomainTests.Infrastructure;
    using Palmtree.ApiPlatform.Utility;
    using Palmtree.Sample.Api.Domain.Messages.Commands;
    using Palmtree.Sample.Api.Domain.Models.Aggregates;
    using Palmtree.Sample.Api.Domain.Models.ValueObjects;
    using Palmtree.Sample.Api.Domain.Models.ViewModels;
    using Xunit;

    public class WhenAuthenticateUser
    {
        private readonly User anotherUser;

        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        private readonly TimeSpan expiresIn = TimeSpan.FromMinutes(15);

        private readonly DateTime issuedAt = DateTime.Now;

        private readonly string password = "secret-sauce2";

        private readonly ClientSecurityContext result;

        private readonly string username = "monmothma2";

        public WhenAuthenticateUser()
        {
            //arrange            
            this.endPoint.AddToDatabase(TestData.User1);

            this.anotherUser = CreateAnotherUser();

            this.endPoint.AddToDatabase(this.anotherUser);

            var authenticateUser = new AuthenticateUser(AuthenticateUser.UserCredentials.Create(this.username, this.password));

            this.result = (ClientSecurityContext)this.endPoint.HandleCommand(authenticateUser);
        }

        [Fact]
        public void ItShouldAddNewActiveSecurityToken()
        {
            var user = this.endPoint.QueryDatabase<User>(q => q.Where(x => x.UserName == this.username)).Result.Single();

            user.Should().NotBeNull();
            user.ActiveSecurityTokens.Should().HaveCount(1);

            var expectedSecurityToken = SecurityToken.Create(user.id, user.PasswordDetails.PasswordHash, this.issuedAt, this.expiresIn, true);
            user.ActiveSecurityTokens.Should()
                .ContainSingle(t => t.UserId == expectedSecurityToken.UserId && t.SecureHmacHash == expectedSecurityToken.SecureHmacHash);
        }

        [Fact]
        public void ItShouldAuthenticateTheUser()
        {
            Assert.True(this.result.UserProfile.Id == this.anotherUser.id);
        }

        private User CreateAnotherUser()
        {
            var passwordHash = SecureHmacHash.CreateFrom(this.password);
            var passwordDetails = PasswordDetails.Create(passwordHash.IterationsUsed, passwordHash.HexSalt, passwordHash.HexHash);
            var user = User.Create(
                "monmotha2@rebelalliance.org",
                "Mon Monthma",
                passwordDetails,
                null,
                this.username,
                Guid.Parse("33de11ce-2058-49bb-a4e9-e1b23fb0b9c4"));
            return user;
        }
    }
}
